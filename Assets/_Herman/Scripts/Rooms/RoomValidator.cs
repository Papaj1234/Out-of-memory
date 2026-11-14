using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    public static class RoomValidator
    {
        private static readonly Vector2Int[] Dirs =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // true, если весь пол — одна связная область (а значит, все двери достижимы)
        public static bool IsConnected(RoomLayout layout)
        {
            int totalFloor = 0;
            Vector2Int start = new Vector2Int(-1, -1);

            for (int x = 0; x < layout.width; x++)
                for (int y = 0; y < layout.height; y++)
                    if (layout[x, y] == Cell.Floor)
                    {
                        totalFloor++;
                        if (start.x < 0) start = new Vector2Int(x, y);
                    }

            if (totalFloor == 0) return false;

            var visited = new bool[layout.width, layout.height];
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited[start.x, start.y] = true;
            int reached = 0;

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                reached++;

                foreach (var d in Dirs)
                {
                    var n = c + d;
                    if (!layout.InBounds(n.x, n.y)) continue;
                    if (visited[n.x, n.y]) continue;
                    if (layout[n.x, n.y] != Cell.Floor) continue;

                    visited[n.x, n.y] = true;
                    queue.Enqueue(n);
                }
            }

            return reached == totalFloor;
        }
    }
}