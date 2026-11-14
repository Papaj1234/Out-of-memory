using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    [CreateAssetMenu(fileName = "Pattern", menuName = "Herman/Obstacle Pattern")]
    public class ObstaclePattern : ScriptableObject
    {
        [Tooltip("X = клетка препятствия, любой другой символ = пусто. Верхняя строка = верх.")]
        [TextArea(3, 10)] public string shape = "XX\nXX";
        public bool allowRotate = true;
        public bool allowMirror = true;
        [Min(0f)] public float weight = 1f;

        public List<Vector2Int> GetCells(System.Random rng)
        {
            var cells = new List<Vector2Int>();
            var lines = shape.Replace("\r", "").Split('\n');
            int h = lines.Length;

            for (int row = 0; row < h; row++)
                for (int col = 0; col < lines[row].Length; col++)
                    if (lines[row][col] == 'X' || lines[row][col] == 'x')
                        cells.Add(new Vector2Int(col, h - 1 - row));

            if (allowMirror && rng.Next(2) == 0)
                for (int i = 0; i < cells.Count; i++)
                    cells[i] = new Vector2Int(-cells[i].x, cells[i].y);

            if (allowRotate)
            {
                int turns = rng.Next(4);
                for (int t = 0; t < turns; t++)
                    for (int i = 0; i < cells.Count; i++)
                        cells[i] = new Vector2Int(-cells[i].y, cells[i].x);
            }

            return cells;
        }
    }
}