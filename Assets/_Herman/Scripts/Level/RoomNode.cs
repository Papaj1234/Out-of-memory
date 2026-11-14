using UnityEngine;
using Assets._Herman.Scripts.Rooms;

namespace Assets._Herman.Scripts.Level
{
    public enum RoomRole { Start, Normal, Treasure, Boss }

    public class RoomNode
    {
        public Vector2Int gridPos;
        public RoomRole role;
        public RoomType type;
        public Doors doors;
        public int seed;
        public int distance;
    }
}