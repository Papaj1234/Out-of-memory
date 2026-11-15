using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


//это пиздец я перестал понимать как эта хуйня работает
namespace Assets._Herman.Scripts.Rooms
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private Tilemap floorMap;
        [SerializeField] private Tilemap blockMap;
        [SerializeField] private Tilemap pitMap;     // ямы — отдельно, сквозь них летят пули
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Door doorPrefab;

        public RoomType Type { get; private set; }
        public RoomLayout Layout { get; private set; }
        public Transform ContentRoot => contentRoot;
        public IReadOnlyList<Door> DoorList => doors;
        public bool IsCleared { get; private set; }
        public int AliveEnemies => aliveEnemies.Count;

        public Vector2Int GridPos { get; set; }
        public Assets._Herman.Scripts.Level.RoomRole Role { get; set; }
        public Vector3 CenterWorld => CellToWorld(Layout.width / 2, Layout.height / 2);

        // Для камеры, UI и т.п.
        public static event System.Action<Room> PlayerEntered;
        public static event System.Action<Room> RoomCleared;

        private readonly List<Door> doors = new List<Door>();
        private readonly List<RoomEnemy> aliveEnemies = new List<RoomEnemy>();
        private bool encounterStarted;

        private TileSet floorSet, rockSet, pitSet, pitEdgeSet, wallSet;
        private RoomType tilesCreatedFor;

        private class TileSet
        {
            public Tile[] tiles;
            public float[] weights;
            public float totalWeight;

            public Tile Pick(float roll)
            {
                if (tiles.Length == 0) return null;

                float target = roll * totalWeight;
                for (int i = 0; i < tiles.Length; i++)
                {
                    target -= weights[i];
                    if (target < 0f) return tiles[i];
                }
                return tiles[tiles.Length - 1];
            }
        }

        private void Awake()
        {
            if (contentRoot == null)
            {
                contentRoot = new GameObject("Content").transform;
                contentRoot.SetParent(transform, false);
            }
        }

        public void Build(RoomType type, RoomLayout layout)
        {
            Type = type;
            Layout = layout;
            flow = null;

            if (tilesCreatedFor != type)
            {
                floorSet = MakeSet(type.floorVariants);
                rockSet = MakeSet(type.rockVariants);
                pitSet = MakeSet(type.pitVariants);
                pitEdgeSet = MakeSet(type.pitEdgeVariants);
                wallSet = MakeSet(type.wallVariants);
                tilesCreatedFor = type;
            }

            ClearContent();
            Draw();
            SpawnDoors();
            CreateTrigger();

            IsCleared = false;
            encounterStarted = false;
            SetDoorsOpen(true); // снаружи в любую комнату можно зайти

            foreach (var feature in type.features)
                if (feature != null)
                    feature.Populate(this, layout);
        }

        // ===== Логика комнаты =====

        public void OnPlayerEnter()
        {
            PlayerEntered?.Invoke(this);

            if (IsCleared || encounterStarted) return;
            encounterStarted = true;

            SpawnEnemies();

            if (aliveEnemies.Count == 0)
            {
                OnRoomCleared(); // мобов нет — комната сразу чистая
                return;
            }

            CloseDoors();
        }

        public void OnRoomCleared()
        {
            IsCleared = true;
            OpenDoors();
            RoomCleared?.Invoke(this);
        }

        public void OnEnemyRemoved(RoomEnemy enemy)
        {
            if (!aliveEnemies.Remove(enemy)) return;

            if (aliveEnemies.Count == 0 && encounterStarted && !IsCleared)
                OnRoomCleared();
        }

        [ContextMenu("Open Doors")]
        public void OpenDoors() => SetDoorsOpen(true);

        [ContextMenu("Close Doors")]
        public void CloseDoors() => SetDoorsOpen(false);

        [ContextMenu("Kill All Enemies (debug)")]
        private void KillAllEnemies()
        {
            foreach (var e in new List<RoomEnemy>(aliveEnemies))
                if (e != null) Destroy(e.gameObject);
        }

        [ContextMenu("Enter Room (debug)")]
        private void DebugEnter() => OnPlayerEnter();

        private void SetDoorsOpen(bool open)
        {
            foreach (var d in doors)
                if (d != null) d.SetOpen(open);
        }

        // ===== Мобы =====

        private void SpawnEnemies()
        {
            if (Type.enemyPrefabs == null || Type.enemyPrefabs.Count == 0) return;

            int min = Mathf.Max(0, Type.enemyCount.x);
            int max = Mathf.Max(min, Type.enemyCount.y);
            int count = Random.Range(min, max + 1);

            var cells = GetFreeCells();

            for (int i = 0; i < count && cells.Count > 0; i++)
            {
                int index = Random.Range(0, cells.Count);
                var cell = cells[index];
                cells.RemoveAt(index);

                var prefab = Type.enemyPrefabs[Random.Range(0, Type.enemyPrefabs.Count)];
                if (prefab == null) continue;

                var enemy = Instantiate(prefab, CellToWorld(cell.x, cell.y), Quaternion.identity, contentRoot);
                var marker = enemy.AddComponent<RoomEnemy>();
                marker.Init(this);
                aliveEnemies.Add(marker);
                enemy.SetActive(true);
            }
        }

        // Свободный пол для спавна: всё, кроме ям/камней и зоны у дверей
        public List<Vector2Int> GetFreeCells()
        {
            var doorZone = new HashSet<Vector2Int>();
            foreach (var (cell, inward) in Layout.GetDoors())
                for (int i = 0; i < Type.doorClearance; i++)
                    doorZone.Add(cell + inward * i);

            var result = new List<Vector2Int>();
            for (int x = 0; x < Layout.width; x++)
                for (int y = 0; y < Layout.height; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (Layout[x, y] == Cell.Floor && !doorZone.Contains(c))
                        result.Add(c);
                }
            return result;
        }

        // Где появляется герой, вошедший через дверь на стороне side
        public Vector3 EntryPoint(Doors side)
        {
            foreach (var (cell, inward) in Layout.GetDoors())
            {
                if (SideFromInward(inward) != side) continue;

                var inside = cell + inward;
                if (Layout.InBounds(inside.x, inside.y) && Layout[inside.x, inside.y] == Cell.Floor)
                    return CellToWorld(inside.x, inside.y);

                return CellToWorld(cell.x, cell.y);
            }
            return CenterWorld;
        }

        // ===== Триггер входа =====

        private void CreateTrigger()
        {
            var go = new GameObject("RoomTrigger");
            go.transform.SetParent(contentRoot, false);

            // зона на 1 клетку меньше комнаты с каждой стороны
            Vector3 a = CellToWorld(1, 1);
            Vector3 b = CellToWorld(Layout.width - 2, Layout.height - 2);
            go.transform.position = (a + b) / 2f;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(Mathf.Max(1, Layout.width - 2), Mathf.Max(1, Layout.height - 2));

            go.AddComponent<RoomTrigger>().Init(this);
        }

        // ===== Двери =====

        private void SpawnDoors()
        {
            var prefab = Type.doorPrefabOverride != null ? Type.doorPrefabOverride : doorPrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"[{name}] не назначен Door Prefab");
                return;
            }

            foreach (var (cell, inward) in Layout.GetDoors())
            {
                var gap = cell - inward;
                var side = SideFromInward(inward);
                var rotation = Quaternion.Euler(0f, 0f, AngleFor(side));

                var door = Instantiate(prefab, CellToWorld(gap.x, gap.y), rotation, contentRoot);
                door.name = $"Door {side}";
                door.Init(this, side, true);
                doors.Add(door);
            }
        }

        private static Doors SideFromInward(Vector2Int inward)
        {
            if (inward == Vector2Int.down) return Doors.Up;
            if (inward == Vector2Int.up) return Doors.Down;
            if (inward == Vector2Int.right) return Doors.Left;
            return Doors.Right;
        }

        private static float AngleFor(Doors side)
        {
            switch (side)
            {
                case Doors.Down: return 180f;
                case Doors.Left: return 90f;
                case Doors.Right: return -90f;
                default: return 0f;
            }
        }

        // ===== Поиск пути для мобов =====

        private int[,] flow;                       // расстояние (в шагах) от каждой клетки до цели
        private Vector2Int flowTarget = new Vector2Int(int.MinValue, int.MinValue);

        private static readonly Vector2Int[] Neighbours8 =
        {
            new Vector2Int( 1, 0), new Vector2Int(-1, 0), new Vector2Int(0,  1), new Vector2Int( 0, -1),
            new Vector2Int( 1, 1), new Vector2Int( 1,-1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        public Vector2Int WorldToCell(Vector3 world)
        {
            var t = floorMap.WorldToCell(world);
            return new Vector2Int(t.x + Layout.width / 2, t.y + Layout.height / 2);
        }

        public bool IsWalkable(Vector2Int c) =>
            Layout.InBounds(c.x, c.y) && Layout[c.x, c.y] == Cell.Floor;

        // шаг возможен, если клетка свободна, а по диагонали — ещё и не срезаем угол
        private bool CanStep(Vector2Int from, Vector2Int d)
        {
            var next = from + d;
            if (!IsWalkable(next)) return false;
            if (d.x == 0 || d.y == 0) return true;
            return IsWalkable(from + new Vector2Int(d.x, 0)) && IsWalkable(from + new Vector2Int(0, d.y));
        }

        private void UpdateFlow(Vector2Int target)
        {
            if (flow != null && target == flowTarget) return;

            flowTarget = target;
            if (flow == null || flow.GetLength(0) != Layout.width || flow.GetLength(1) != Layout.height)
                flow = new int[Layout.width, Layout.height];

            for (int x = 0; x < Layout.width; x++)
                for (int y = 0; y < Layout.height; y++)
                    flow[x, y] = -1;

            var queue = new Queue<Vector2Int>();
            flow[target.x, target.y] = 0;
            queue.Enqueue(target);

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var d in Neighbours8)
                {
                    var n = c + d;
                    if (!Layout.InBounds(n.x, n.y) || flow[n.x, n.y] >= 0) continue;
                    if (!CanStep(c, d)) continue;

                    flow[n.x, n.y] = flow[c.x, c.y] + 1;
                    queue.Enqueue(n);
                }
            }
        }

        // Куда идти, чтобы дойти до цели (away = true — чтобы уйти от неё), обходя ямы и камни
        public Vector2 PathDirection(Vector3 from, Vector3 target, bool away = false)
        {
            Vector2 straight = ((Vector2)(away ? from - target : target - from)).normalized;
            if (Layout == null) return straight;

            var fc = WorldToCell(from);
            var tc = WorldToCell(target);
            tc.x = Mathf.Clamp(tc.x, 0, Layout.width - 1);
            tc.y = Mathf.Clamp(tc.y, 0, Layout.height - 1);

            if (!Layout.InBounds(fc.x, fc.y)) return straight;

            UpdateFlow(tc);

            int current = flow[fc.x, fc.y];
            if (current < 0) return straight;               // нет пути — как раньше
            if (!away && fc == tc) return straight;          // уже в клетке героя — прямо к нему

            var best = fc;
            int bestValue = current;

            foreach (var d in Neighbours8)
            {
                if (!CanStep(fc, d)) continue;
                var n = fc + d;
                int v = flow[n.x, n.y];
                if (v < 0) continue;

                if (away ? v > bestValue : v < bestValue)
                {
                    bestValue = v;
                    best = n;
                }
            }

            if (best == fc) return away ? Vector2.zero : straight; // загнан в угол — стоит

            return ((Vector2)(CellToWorld(best.x, best.y) - from)).normalized;
        }

        // ===== Отрисовка =====

        public Vector3 CellToWorld(int x, int y) => floorMap.GetCellCenterWorld(ToTilePos(x, y));

        private Vector3Int ToTilePos(int x, int y) =>
            new Vector3Int(x - Layout.width / 2, y - Layout.height / 2, 0);

        private TileSet MakeSet(List<TileVariant> variants)
        {
            var set = new TileSet
            {
                tiles = new Tile[variants.Count],
                weights = new float[variants.Count]
            };

            for (int i = 0; i < variants.Count; i++)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = variants[i].sprite;
                tile.color = variants[i].color;
                tile.colliderType = Tile.ColliderType.Grid;

                set.tiles[i] = tile;
                set.weights[i] = variants[i].weight;
                set.totalWeight += variants[i].weight;
            }

            return set;
        }

        private void ClearContent()
        {
            aliveEnemies.Clear(); // сначала очищаем, чтобы уничтожение мобов не "зачистило" комнату
            doors.Clear();
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);
        }

        private void Draw()
        {
            floorMap.ClearAllTiles();
            blockMap.ClearAllTiles();
            if (pitMap != null) pitMap.ClearAllTiles();

            for (int x = 0; x < Layout.width; x++)
            {
                for (int y = 0; y < Layout.height; y++)
                {
                    var pos = ToTilePos(x, y);
                    float roll = Layout.variantRoll[x, y];

                    switch (Layout[x, y])
                    {
                        case Cell.Floor:
                            floorMap.SetTile(pos, floorSet.Pick(roll));
                            break;
                        case Cell.Rock:
                            floorMap.SetTile(pos, floorSet.Pick(roll));
                            blockMap.SetTile(pos, rockSet.Pick(roll));
                            break;
                        case Cell.Pit:
                            (pitMap != null ? pitMap : blockMap).SetTile(pos, PickPit(x, y, roll));
                            break;
                    }
                }
            }

            DrawWalls();
        }

        // Верхний край ямы — градиент, всё что ниже — темнота
        private Tile PickPit(int x, int y, float roll)
        {
            bool pitAbove = Layout.InBounds(x, y + 1) && Layout[x, y + 1] == Cell.Pit;

            if (!pitAbove && pitEdgeSet.tiles.Length > 0)
                return pitEdgeSet.Pick(roll);

            return pitSet.Pick(roll);
        }

        private void DrawWalls()
        {
            // проёмы — клетки стены прямо за клеткой двери
            var gaps = new HashSet<Vector2Int>();
            foreach (var (cell, inward) in Layout.GetDoors())
                gaps.Add(cell - inward);

            for (int x = -1; x <= Layout.width; x++)
            {
                for (int y = -1; y <= Layout.height; y++)
                {
                    bool isEdge = x == -1 || y == -1 || x == Layout.width || y == Layout.height;
                    if (!isEdge) continue;

                    var pos = ToTilePos(x, y);

                    if (gaps.Contains(new Vector2Int(x, y)))
                        floorMap.SetTile(pos, floorSet.Pick(0.5f));        // проход
                    else
                        blockMap.SetTile(pos, wallSet.Pick(Hash01(x, y))); // стена
                }
            }
        }

        private static float Hash01(int x, int y)
        {
            unchecked
            {
                int h = x * 73856093 ^ y * 19349663;
                return (h & 0x7fffffff) % 1000 / 1000f;
            }
        }

        private void OnDrawGizmos()
        {
            if (Layout == null) return;
            Gizmos.color = Color.green;
            foreach (var (cell, _) in Layout.GetDoors())
                Gizmos.DrawWireCube(CellToWorld(cell.x, cell.y), Vector3.one * 0.9f);
        }
    }
}