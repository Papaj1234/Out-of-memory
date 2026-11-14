using System.Collections;
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

        private TileSet floorSet, rockSet, pitSet;
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

            if (variants.Count == 0)
                Debug.LogWarning($"[{name}] пустой список вариантов текстур");

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