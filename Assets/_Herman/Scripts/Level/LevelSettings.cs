using UnityEngine;
using Assets._Herman.Scripts.Rooms;

namespace Assets._Herman.Scripts.Level
{
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Herman/Level Settings")]
    public class LevelSettings : ScriptableObject
    {
        [Tooltip("Всего комнат, включая старт, сокровищницу и босса")]
        [Min(3)] public int roomCount = 5;

        [Header("Room types (все одного размера!)")]
        public RoomType startType;
        public RoomType normalType;
        public RoomType treasureType;
        public RoomType bossType;
    }
}