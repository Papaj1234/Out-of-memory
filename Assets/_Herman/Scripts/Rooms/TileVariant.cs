using System.Collections;
using UnityEngine;

namespace Assets._Herman.Scripts.Rooms
{
    [System.Serializable]
    public class TileVariant
    {
        public Sprite sprite;
        public Color color = Color.white;
        [Min(0f)] public float weight = 1f;
    }
}