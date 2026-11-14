using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//это пизда я уже перестал понимать что здесь происходит, но вроде как это комната, которая строится по типу и лэйауту и прочая канитель

namespace Assets._Herman.Scripts.Rooms
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private Tilemap floorMap;
        [SerializeField] private Tilemap blockMap;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Door doorPrefab;

        public RoomType Type { get; private set; }
        public RoomLayout Layout { get; private set; }
        public Transform ContentRoot => contentRoot;
        public IReadOnlyList<Door> DoorList => doors;
        public bool IsCleared { get; private set; }
        public int AliveEnemies => aliveEnemies.Count;

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
            }
        }

        // Свободный пол, не у дверей и не в защищённых зонах
        public List<Vector2Int> GetFreeCells()
        {
            var result = new List<Vector2Int>();
            for (int x = 0; x < Layout.width; x++)
                for (int y = 0; y < Layout.height; y++)
                    if (Layout[x, y] == Cell.Floor && !Layout.protectedCells[x, y])
                        result.Add(new Vector2Int(x, y));
            return result;
        }

        // ===== Триггер входа =====

        private void CreateTrigger()
        {
            var go = new GameObject("RoomTrigger");
            go.transform.SetParent(contentRoot, false);

            // зона на 1 клетку меньше комнаты с каждой стороны,
            // чтобы дверь закрывалась уже за спиной героя
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
                            blockMap.SetTile(pos, PickPit(x, y, roll));
                            break;
                    }
                }
            }

            DrawWalls();
        }

        private Tile PickPit(int x, int y, float roll)
        {
            bool pitAbove = Layout.InBounds(x, y + 1) && Layout[x, y + 1] == Cell.Pit;

            if (!pitAbove && pitEdgeSet.tiles.Length > 0)
                return pitEdgeSet.Pick(roll);

            return pitSet.Pick(roll);
        }

        private void DrawWalls()
        {
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
                        floorMap.SetTile(pos, floorSet.Pick(0.5f));
                    else
                        blockMap.SetTile(pos, wallSet.Pick(Hash01(x, y)));
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