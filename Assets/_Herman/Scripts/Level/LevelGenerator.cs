using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets._Herman.Scripts.Rooms;

namespace Assets._Herman.Scripts.Level
{
    public static class LevelGenerator
    {
        private const int MaxAttempts = 100;

        private static readonly Vector2Int[] Dirs =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        public static Dictionary<Vector2Int, RoomNode> Generate(LevelSettings settings, int seed)
        {
            var rng = new System.Random(seed);

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var map = TryGenerate(settings, rng);
                if (map != null) return map;
            }

            Debug.LogError($"Не удалось сгенерировать уровень (seed {seed})");
            return null;
        }

        private static Dictionary<Vector2Int, RoomNode> TryGenerate(LevelSettings settings, System.Random rng)
        {
            // 1. растим дерево комнат от старта
            var cells = new List<Vector2Int> { Vector2Int.zero };
            var occupied = new HashSet<Vector2Int> { Vector2Int.zero };

            int guard = 0;
            while (cells.Count < settings.roomCount && guard++ < 1000)
            {
                var from = cells[rng.Next(cells.Count)];
                var next = from + Dirs[rng.Next(Dirs.Length)];

                if (occupied.Contains(next)) continue;
                if (CountNeighbours(occupied, next) > 1) continue;

                occupied.Add(next);
                cells.Add(next);
            }

            if (cells.Count < settings.roomCount) return null;

            // 2. расстояния от старта
            var distance = Distances(occupied);

            // 3. тупики, самые дальние первыми
            var deadEnds = cells
                .Where(c => c != Vector2Int.zero && CountNeighbours(occupied, c) == 1)
                .OrderByDescending(c => distance[c])
                .ToList();

            if (deadEnds.Count < 2) return null;

            var bossPos = deadEnds[0];
            var treasurePos = deadEnds[1];
            if (distance[bossPos] < 2) return null;

            // 4. узлы
            var map = new Dictionary<Vector2Int, RoomNode>();
            foreach (var pos in cells)
            {
                RoomRole role =
                    pos == Vector2Int.zero ? RoomRole.Start :
                    pos == bossPos ? RoomRole.Boss :
                    pos == treasurePos ? RoomRole.Treasure :
                                             RoomRole.Normal;

                map[pos] = new RoomNode
                {
                    gridPos = pos,
                    role = role,
                    type = TypeFor(settings, role),
                    doors = DoorsFor(occupied, pos),
                    seed = rng.Next(1, int.MaxValue),
                    distance = distance[pos]
                };
            }

            return map;
        }

        private static RoomType TypeFor(LevelSettings s, RoomRole role)
        {
            switch (role)
            {
                case RoomRole.Start: return s.startType;
                case RoomRole.Treasure: return s.treasureType;
                case RoomRole.Boss: return s.bossType;
                default: return s.normalType;
            }
        }

        private static Doors DoorsFor(HashSet<Vector2Int> occupied, Vector2Int pos)
        {
            Doors d = Doors.None;
            if (occupied.Contains(pos + Vector2Int.up)) d |= Doors.Up;
            if (occupied.Contains(pos + Vector2Int.down)) d |= Doors.Down;
            if (occupied.Contains(pos + Vector2Int.left)) d |= Doors.Left;
            if (occupied.Contains(pos + Vector2Int.right)) d |= Doors.Right;
            return d;
        }

        private static int CountNeighbours(HashSet<Vector2Int> occupied, Vector2Int pos)
        {
            int n = 0;
            foreach (var d in Dirs)
                if (occupied.Contains(pos + d)) n++;
            return n;
        }

        private static Dictionary<Vector2Int, int> Distances(HashSet<Vector2Int> occupied)
        {
            var dist = new Dictionary<Vector2Int, int> { [Vector2Int.zero] = 0 };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(Vector2Int.zero);

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var d in Dirs)
                {
                    var n = c + d;
                    if (!occupied.Contains(n) || dist.ContainsKey(n)) continue;
                    dist[n] = dist[c] + 1;
                    queue.Enqueue(n);
                }
            }
            return dist;
        }
    }
}