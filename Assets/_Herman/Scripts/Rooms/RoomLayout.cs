using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    public class RoomLayout
    {
        public readonly int width;
        public readonly int height;
        public readonly Doors doors;
        public readonly Cell[,] cells;
        public readonly bool[,] protectedCells;   // сюда нельзя ставить препятствия
        public readonly float[,] variantRoll;

        public RoomLayout(int width, int height, Doors doors)
        {
            this.width = width;
            this.height = height;
            this.doors = doors;
            cells = new Cell[width, height];
            protectedCells = new bool[width, height];
            variantRoll = new float[width, height];
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        public Cell this[int x, int y]
        {
            get => cells[x, y];
            set => cells[x, y] = value;
        }

        // Клетка двери + направление внутрь комнаты
        public List<(Vector2Int cell, Vector2Int inward)> GetDoors()
        {
            var list = new List<(Vector2Int, Vector2Int)>();
            if ((doors & Doors.Up) != 0) list.Add((new Vector2Int(width / 2, height - 1), Vector2Int.down));
            if ((doors & Doors.Down) != 0) list.Add((new Vector2Int(width / 2, 0), Vector2Int.up));
            if ((doors & Doors.Left) != 0) list.Add((new Vector2Int(0, height / 2), Vector2Int.right));
            if ((doors & Doors.Right) != 0) list.Add((new Vector2Int(width - 1, height / 2), Vector2Int.left));
            return list;
        }
    }
}