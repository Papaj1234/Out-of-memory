using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        private readonly List<Door> doors = new List<Door>();
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
                wallSet = MakeSet(type.wallVariants);
                pitEdgeSet = MakeSet(type.pitEdgeVariants);
                tilesCreatedFor = type;
            }

            ClearContent();
            Draw();
            SpawnDoors();

            IsCleared = type.doorsStartOpen;
            SetDoorsOpen(type.doorsStartOpen);

            foreach (var feature in type.features)
                if (feature != null)
                    feature.Populate(this, layout);
        }

        // ===== Логика комнаты — пока заглушки =====

        // TODO: вызвать, когда герой зашёл в комнату (закрыть двери, заспавнить мобов)
        public void OnPlayerEnter()
        {
        }

        // TODO: вызвать, когда убит последний моб
        public void OnRoomCleared()
        {
            IsCleared = true;
            OpenDoors();
        }

        [ContextMenu("Open Doors")]
        public void OpenDoors() => SetDoorsOpen(true);

        [ContextMenu("Close Doors")]
        public void CloseDoors() => SetDoorsOpen(false);

        private void SetDoorsOpen(bool open)
        {
            foreach (var d in doors)
                if (d != null) d.SetOpen(open);
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
                var gap = cell - inward;                 // клетка проёма в стене
                var side = SideFromInward(inward);
                var rotation = Quaternion.Euler(0f, 0f, AngleFor(side));

                var door = Instantiate(prefab, CellToWorld(gap.x, gap.y), rotation, contentRoot);
                door.name = $"Door {side}";
                door.Init(this, side, false);
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

        // спрайт нарисован для верхней стены
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