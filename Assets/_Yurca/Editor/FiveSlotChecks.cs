#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
namespace Yurca.Editor
{
    public static class FiveSlotChecks
    {
        [MenuItem("Tools/Yurca/Run Five Slot Checks")]
        public static void Run()
        {
            GameObject test = new GameObject("TemporaryStatsCheck");
            try
            {
                PlayerStats stats = test.AddComponent<PlayerStats>();
                float range = stats.AttackRange;
                float radius = stats.ProjectileRadius;
                float duration = stats.AttackDuration;
                float cooldown = stats.AttackCooldown;
                Assert.AreApproximatelyEqual(3f, stats.MaxHealth);
                object speed = new object(), size = new object(), speed2 = new object();
                stats.SetAttackModifier(speed, 2f, 1f);
                Assert.AreApproximatelyEqual(cooldown / 2f, stats.AttackCooldown);
                Assert.AreApproximatelyEqual(duration / 2f, stats.AttackDuration);
                stats.SetAttackModifier(size, 1f, 1.5f);
                Assert.AreApproximatelyEqual(range * 1.5f, stats.AttackRange);
                Assert.AreApproximatelyEqual(radius * 1.5f, stats.ProjectileRadius);
                stats.SetAttackModifier(speed, 2f, 1f);
                Assert.AreApproximatelyEqual(2f, stats.AttackSpeedMultiplier);
                stats.SetAttackModifier(speed2, 1.5f, 1f);
                Assert.AreApproximatelyEqual(3f, stats.AttackSpeedMultiplier);
                stats.RemoveAttackModifier(speed);
                Assert.AreApproximatelyEqual(1.5f, stats.AttackSpeedMultiplier);
                stats.RemoveAttackModifier(speed2);
                stats.RemoveAttackModifier(size);
                stats.RemoveAttackModifier(size);
                Assert.AreApproximatelyEqual(range, stats.AttackRange);
                Assert.AreApproximatelyEqual(cooldown, stats.AttackCooldown);
                Assert.AreApproximatelyEqual(3f, stats.MaxHealth);

                int validPairs = 0;
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                    foreach (EquipmentKind kind in Enum.GetValues(typeof(EquipmentKind)))
                    {
                        bool expected = slot == EquipmentSlot.Attack ? kind == EquipmentKind.Attack :
                            slot == EquipmentSlot.Dash ? kind == EquipmentKind.Dash :
                            slot == EquipmentSlot.Ultimate ? kind == EquipmentKind.Ultimate :
                            kind == EquipmentKind.Ability;
                        Assert.AreEqual(expected, PlayerEquipment.Fits(slot, kind));
                        if (expected) validPairs++;
                    }
                Assert.AreEqual(5, validPairs);
                var timer = new ActionCooldown();
                timer.Begin(10f);
                Assert.IsFalse(timer.IsReady);
                Assert.IsTrue(timer.Remaining > 0f);
                timer.Begin(0f);
                Assert.IsTrue(timer.IsReady);
                Debug.Log("Five Slot Checks: PASSED.");
            }
            finally { UnityEngine.Object.DestroyImmediate(test); }
        }
    }
}
#endif
