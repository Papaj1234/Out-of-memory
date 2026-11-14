using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    [CreateAssetMenu(fileName = "RoomType", menuName = "Herman/Room Type")]
    public class RoomType : ScriptableObject
    {
        [Header("Size")]
        public int width = 13;
        public int height = 7;

        [Header("Tile variants")]
        public List<TileVariant> floorVariants = new List<TileVariant>();
        public List<TileVariant> rockVariants = new List<TileVariant>();
        public List<TileVariant> pitVariants = new List<TileVariant>();
        public List<TileVariant> wallVariants = new List<TileVariant>();

        [Header("Pits")]
        public List<ObstaclePattern> pitPatterns = new List<ObstaclePattern>();
        [Tooltip("x = минимум, y = максимум фигур")]
        public Vector2Int pitPatternCount = new Vector2Int(1, 3);

        [Header("Rocks")]
        public List<ObstaclePattern> rockPatterns = new List<ObstaclePattern>();
        public Vector2Int rockPatternCount = new Vector2Int(2, 5);

        [Header("Rules")]
        [Tooltip("Сколько клеток перед дверью всегда свободны")]
        [Min(1)] public int doorClearance = 2;

        [Tooltip("Радиус свободной зоны в центре: 0 = только центральная клетка, 1 = квадрат 3x3, -1 = выкл")]
        public Vector2Int centerClearance = new Vector2Int(1, 1);

        [Header("Features")]
        public List<RoomFeature> features = new List<RoomFeature>();
    }
}