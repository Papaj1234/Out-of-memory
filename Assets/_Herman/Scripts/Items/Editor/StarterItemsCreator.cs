using System;
using UnityEditor;
using UnityEngine;

namespace Assets._Herman.Scripts.Items.Editor
{
    // Обязательно лежит в папке "Editor".
    // Меню: Tools > Herman > Create Starter Items
    // Все бонусы процентные — баланс не зависит от базовых статов героя.
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

            Create("IronBranch", "Iron Branch",
                "+10% to all stats.",
                AllStats(0.10f));

            Create("WraithBand", "Wraith Band",
                "+20% attack speed, +10% move speed.",
                Pct(StatType.AttackSpeed, 0.20f),
                Pct(StatType.MoveSpeed, 0.10f));

            Create("MaskOfMadness", "Mask of Madness",
                "+60% attack speed, -20% move speed.",
                Pct(StatType.AttackSpeed, 0.60f),
                Pct(StatType.MoveSpeed, -0.20f));

            Create("DemonEdge", "Demon Edge",
                "+50% damage, -15% attack speed.",
                Pct(StatType.Damage, 0.50f),
                Pct(StatType.AttackSpeed, -0.15f));

            Create("DragonLance", "Dragon Lance",
                "+30% attack range, -10% move speed.",
                Pct(StatType.AttackRange, 0.30f),
                Pct(StatType.MoveSpeed, -0.10f));

            Create("PhaseBoots", "Phase Boots",
                "+30% move speed, +10% damage.",
                Pct(StatType.MoveSpeed, 0.30f),
                Pct(StatType.Damage, 0.10f));

            Create("AetherLens", "Aether Lens",
                "Melee: +30% attack angle. Ranged: +30% projectile speed and radius. -10% damage.",
                Pct(StatType.AttackAngle, 0.30f),
                Pct(StatType.ProjectileSpeed, 0.30f),
                Pct(StatType.ProjectileRadius, 0.30f),
                Pct(StatType.Damage, -0.10f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Starter items created/updated in {FolderPath}");
        }

        private static StatModifier Pct(StatType stat, float value) => new StatModifier(stat, percent: value);

        private static StatModifier[] AllStats(float percent)
        {
            var stats = (StatType[])Enum.GetValues(typeof(StatType));
            var result = new StatModifier[stats.Length];
            for (int i = 0; i < stats.Length; i++)
                result[i] = Pct(stats[i], percent);
            return result;
        }

        // Если ассет уже есть — обновляется на месте (GUID сохраняется, ссылки не теряются)
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