using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.Items
{
    [CreateAssetMenu(menuName = "Herman/Item", fileName = "NewItem")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private StatModifier[] modifiers = Array.Empty<StatModifier>();

        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<StatModifier> Modifiers => modifiers;

#if UNITY_EDITOR
        // Используется редактор-скриптом, который создаёт стартовые предметы
        public void Setup(string displayName, string description, params StatModifier[] modifiers)
        {
            this.displayName = displayName;
            this.description = description;
            this.modifiers = modifiers;
        }
#endif
    }
}