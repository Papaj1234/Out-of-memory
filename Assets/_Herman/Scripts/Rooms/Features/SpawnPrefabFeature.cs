using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    [CreateAssetMenu(fileName = "SpawnPrefab", menuName = "Herman/Features/Spawn Prefab")]
    public class SpawnPrefabFeature : RoomFeature
    {
        public GameObject prefab;
        [Tooltip("Смещение от центра комнаты в клетках")]
        public Vector2Int offsetFromCenter = Vector2Int.zero;

        private Vector2Int TargetCell(RoomLayout layout) =>
            new Vector2Int(layout.width / 2, layout.height / 2) + offsetFromCenter;

        // гарантируем, что под объектом пол
        public override void ModifyLayout(RoomLayout layout, System.Random rng)
        {
            var c = TargetCell(layout);
            if (layout.InBounds(c.x, c.y))
                layout[c.x, c.y] = Cell.Floor;
        }

        public override void Populate(Room room, RoomLayout layout)
        {
            if (prefab == null) return;

            var c = TargetCell(layout);
            if (!layout.InBounds(c.x, c.y)) return;

            var obj = Object.Instantiate(prefab, room.CellToWorld(c.x, c.y), Quaternion.identity, room.ContentRoot);
            obj.name = prefab.name;
        }
    }
}