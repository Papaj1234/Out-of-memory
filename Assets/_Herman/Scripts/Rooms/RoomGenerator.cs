using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    public static class RoomGenerator
    {
        private const int AttemptsPerPattern = 30;

        public static RoomLayout Generate(RoomType type, Doors doors, int seed)
        {
            var rng = new System.Random(seed);
            var layout = new RoomLayout(type.width, type.height, doors);

            ProtectDoors(layout, type.doorClearance);

            // ямы первыми — им нужно больше места
            PlacePatterns(layout, type.pitPatterns, Cell.Pit, RandomCount(rng, type.pitPatternCount), rng);
            PlacePatterns(layout, type.rockPatterns, Cell.Rock, RandomCount(rng, type.rockPatternCount), rng);

            foreach (var feature in type.features)
                if (feature != null)
                    feature.ModifyLayout(layout, rng);

            if (!RoomValidator.IsConnected(layout))
                Debug.LogWarning($"[{type.name}] seed {seed}: комната несвязная — проверь фичи");

            RollVariants(layout, rng);
            return layout;
        }

        private static void ProtectDoors(RoomLayout layout, int clearance)
        {
            foreach (var (cell, inward) in layout.GetDoors())
            {
                for (int i = 0; i < clearance; i++)
                {
                    var p = cell + inward * i;
                    if (layout.InBounds(p.x, p.y))
                        layout.protectedCells[p.x, p.y] = true;
                }
            }
        }

        private static void PlacePatterns(RoomLayout layout, List<ObstaclePattern> patterns,
                                          Cell cellType, int count, System.Random rng)
        {
            if (patterns == null || patterns.Count == 0) return;

            for (int i = 0; i < count; i++)
            {
                for (int attempt = 0; attempt < AttemptsPerPattern; attempt++)
                {
                    var pattern = PickWeighted(patterns, rng);
                    if (pattern == null) return;

                    var shape = pattern.GetCells(rng);
                    var origin = new Vector2Int(rng.Next(layout.width), rng.Next(layout.height));

                    if (TryPlace(layout, shape, origin, cellType))
                        break;
                }
            }
        }

        private static bool TryPlace(RoomLayout layout, List<Vector2Int> shape, Vector2Int origin, Cell cellType)
        {
            var positions = new List<Vector2Int>(shape.Count);

            foreach (var offset in shape)
            {
                var p = origin + offset;
                if (!layout.InBounds(p.x, p.y)) return false;
                if (layout.protectedCells[p.x, p.y]) return false;
                if (layout[p.x, p.y] != Cell.Floor) return false;
                positions.Add(p);
            }

            foreach (var p in positions)
                layout[p.x, p.y] = cellType;

            if (RoomValidator.IsConnected(layout))
                return true;

            // перекрыли проход — откатываем
            foreach (var p in positions)
                layout[p.x, p.y] = Cell.Floor;
            return false;
        }

        private static ObstaclePattern PickWeighted(List<ObstaclePattern> patterns, System.Random rng)
        {
            float total = 0f;
            foreach (var p in patterns)
                if (p != null) total += p.weight;
            if (total <= 0f) return null;

            float target = (float)rng.NextDouble() * total;
            foreach (var p in patterns)
            {
                if (p == null) continue;
                target -= p.weight;
                if (target < 0f) return p;
            }
            return null;
        }

        private static int RandomCount(System.Random rng, Vector2Int range)
        {
            int min = Mathf.Max(0, range.x);
            int max = Mathf.Max(min, range.y);
            return rng.Next(min, max + 1);
        }

        private static void RollVariants(RoomLayout layout, System.Random rng)
        {
            for (int x = 0; x < layout.width; x++)
                for (int y = 0; y < layout.height; y++)
                    layout.variantRoll[x, y] = (float)rng.NextDouble();
        }
    }
}