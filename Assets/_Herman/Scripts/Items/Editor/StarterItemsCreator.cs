using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets._Herman.Scripts.Items.Editor
{
    // Обязательно лежит в папке "Editor".
    // Меню: Tools > Herman > Create Starter Items
    // ВНИМАНИЕ: повторный запуск перезапишет ручные правки бонусов и описаний.
    public static class StarterItemsCreator
    {
        private const string ParentPath = "Assets/_Herman/Settings";
        private const string FolderName = "Items";
        private const string FolderPath = ParentPath + "/" + FolderName;

        [MenuItem("Tools/Herman/Create Starter Items")]
        public static void CreateAll()
        {
            if (!AssetDatabase.IsValidFolder(FolderPath))
                AssetDatabase.CreateFolder(ParentPath, FolderName);

            Create("SSD", "SSD",
                "Loads faster than you. +1 max health, +10% move speed.",
                Flat(StatType.MaxHealth, 1f),
                Pct(StatType.MoveSpeed, 0.10f));

            Create("GPU", "GPU",
                "Heavy but powerful. +40% damage, -10% move speed.",
                Pct(StatType.Damage, 0.40f),
                Pct(StatType.MoveSpeed, -0.10f));

            Create("CPUOverclock", "CPU Overclock",
                "Runs hot. +45% attack speed, -1 max health.",
                Pct(StatType.AttackSpeed, 0.45f),
                Flat(StatType.MaxHealth, -1f));

            Create("RAM32GB", "32GB RAM",
                "No more lag. +15% attack speed, +15% move speed.",
                Pct(StatType.AttackSpeed, 0.15f),
                Pct(StatType.MoveSpeed, 0.15f));

            Create("WaterCooling", "Water Cooling",
                "Stay chill. +1 max health, +10% attack speed.",
                Flat(StatType.MaxHealth, 1f),
                Pct(StatType.AttackSpeed, 0.10f));

            Create("FiberOptic", "Fiber Optic",
                "Speed of light. +40% projectile speed, +25% attack range.",
                Pct(StatType.ProjectileSpeed, 0.40f),
                Pct(StatType.AttackRange, 0.25f));

            Create("UltrawideMonitor", "Ultrawide Monitor",
                "See everything. +40% attack angle, +25% projectile radius, -10% damage.",
                Pct(StatType.AttackAngle, 0.40f),
                Pct(StatType.ProjectileRadius, 0.25f),
                Pct(StatType.Damage, -0.10f));

            Create("MechanicalKeyboard", "Mechanical Keyboard",
                "Clicky. +20% attack speed, +10% damage.",
                Pct(StatType.AttackSpeed, 0.20f),
                Pct(StatType.Damage, 0.10f));

            Create("GamingMouse", "Gaming Mouse",
                "16000 DPI. +25% move speed, +10% attack range.",
                Pct(StatType.MoveSpeed, 0.25f),
                Pct(StatType.AttackRange, 0.10f));

            Create("PowerSupply", "Power Supply 1000W",
                "Enough power for everything. +10% to all stats.",
                AllStats(0.10f));

            Create("Antivirus", "Antivirus",
                "Safe but slow. +2 max health, -15% damage.",
                Flat(StatType.MaxHealth, 2f),
                Pct(StatType.Damage, -0.15f));

            Create("RGBLighting", "RGB Lighting",
                "Everyone knows RGB adds FPS. +15% damage.",
                Pct(StatType.Damage, 0.15f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Starter items created/updated in {FolderPath}");
        }

        private static StatModifier Pct(StatType stat, float value) => new StatModifier(stat, percent: value);
        private static StatModifier Flat(StatType stat, float value) => new StatModifier(stat, flat: value);

        // процент ко всем статам, кроме здоровья (здоровье — целые сердца)
        private static StatModifier[] AllStats(float percent)
        {
            var result = new List<StatModifier>();
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
                if (stat != StatType.MaxHealth)
                    result.Add(Pct(stat, percent));
            return result.ToArray();
        }

        // Если ассет уже есть — обновляется на месте (GUID сохраняется)
        private static void Create(string fileName, string displayName, string description, params StatModifier[] modifiers)
        {
            string path = $"{FolderPath}/{fileName}.asset";

            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.Setup(displayName, description, modifiers);
            EditorUtility.SetDirty(item);
        }
    }
}