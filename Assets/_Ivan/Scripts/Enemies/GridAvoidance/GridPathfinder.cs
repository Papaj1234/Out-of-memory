using System.Collections.Generic;
using UnityEngine;
using Assets._Herman.Scripts.Rooms;

public static class GridPathfinder
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public static Vector2Int GetNextStep(
        RoomLayout layout,
        Vector2Int start,
        Vector2Int target)
    {
        if (start == target)
            return start;

        Queue<Vector2Int> queue =
            new Queue<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom =
            new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector2Int current =
                queue.Dequeue();

            if (current == target)
                break;

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next =
                    current + direction;

                if (!layout.InBounds(
                        next.x,
                        next.y))
                    continue;

                if (layout[next.x, next.y] != Cell.Floor)
                    continue;

                if (cameFrom.ContainsKey(next))
                    continue;

                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(target))
            return start;

        Vector2Int step = target;

        while (cameFrom[step] != start)
        {
            step = cameFrom[step];
        }

        return step;
    }
}