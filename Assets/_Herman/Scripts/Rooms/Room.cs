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

        public RoomType Type { get; private set; }
        public RoomLayout Layout { get; private set; }
        public Transform ContentRoot => contentRoot;

        private TileSet floorSet, rockSet, pitSet, wallSet;
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
                tilesCreatedFor = type;
            }

            ClearContent();
            Draw();

            foreach (var feature in type.features)
                if (feature != null)
                    feature.Populate(this, layout);
        }

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
                            blockMap.SetTile(pos, pitSet.Pick(roll));
                            break;
                    }
                }
            }

            DrawWalls();
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
                        floorMap.SetTile(pos, floorSet.Pick(0.5f));      // проход
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