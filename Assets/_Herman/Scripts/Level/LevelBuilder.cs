using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets._Herman.Scripts.Rooms;

namespace Assets._Herman.Scripts.Level
{
    public class LevelBuilder : MonoBehaviour
    {
        [SerializeField] private Room roomPrefab;
        [SerializeField] private LevelSettings settings;
        [Tooltip("0 = случайный")]
        [SerializeField] private int seed = 0;

        private readonly List<Room> rooms = new List<Room>();
        private readonly Dictionary<Vector2Int, Room> roomsByGrid = new Dictionary<Vector2Int, Room>();

        public Room StartRoom { get; private set; }
        public IReadOnlyList<Room> Rooms => rooms;

        // Вызывается, когда уровень полностью построен
        public static event System.Action<LevelBuilder> Built;

        private void Start() => Build();

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Build();
        }

        public Room GetNeighbour(Room room, Doors side)
        {
            roomsByGrid.TryGetValue(room.GridPos + Offset(side), out var neighbour);
            return neighbour;
        }

        public static Vector2Int Offset(Doors side)
        {
            switch (side)
            {
                case Doors.Up: return Vector2Int.up;
                case Doors.Down: return Vector2Int.down;
                case Doors.Left: return Vector2Int.left;
                default: return Vector2Int.right;
            }
        }

        public void Build()
        {
            foreach (var r in rooms)
                if (r != null) Destroy(r.gameObject);
            rooms.Clear();
            roomsByGrid.Clear();
            StartRoom = null;

            int s = seed != 0 ? seed : Random.Range(1, int.MaxValue);
            Debug.Log($"Level seed: {s}");

            var map = LevelGenerator.Generate(settings, s);
            if (map == null) return;

            int stepX = settings.normalType.width + 2;
            int stepY = settings.normalType.height + 2;

            foreach (var node in map.Values)
            {
                var worldPos = new Vector3(node.gridPos.x * stepX, node.gridPos.y * stepY, 0f);
                var room = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
                room.name = $"Room {node.gridPos} {node.role}";
                room.GridPos = node.gridPos;

                var layout = RoomGenerator.Generate(node.type, node.doors, node.seed);
                room.Build(node.type, layout);

                rooms.Add(room);
                roomsByGrid[node.gridPos] = room;

                if (node.role == RoomRole.Start) StartRoom = room;
            }

            Built?.Invoke(this);
        }
    }
}