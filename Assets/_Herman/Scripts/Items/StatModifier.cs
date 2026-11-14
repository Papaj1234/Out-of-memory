using System;
using UnityEngine;

namespace Assets._Herman.Scripts.Items
{
    public enum StatType
    {
        MoveSpeed,
        AttackRange,
        AttackAngle,
        AttackSpeed,
        Damage,
        ProjectileSpeed,
        ProjectileRadius
    }

    [Serializable]
    public struct StatModifier
    {
        public StatType stat;

        [Tooltip("Плоский бонус к базе (например +1.5 к скорости).")]
        public float flat;

        [Tooltip("Процентный бонус: 0.1 = +10%, -0.2 = -20%.")]
        public float percent;

        public StatModifier(StatType stat, float flat = 0f, float percent = 0f)
        {
            this.stat = stat;
            this.flat = flat;
            this.percent = percent;
        }
    }
}