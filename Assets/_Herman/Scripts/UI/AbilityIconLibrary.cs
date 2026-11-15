using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Herman.Scripts.UI
{
    // Иконки способностей: имя префаба действия -> картинка
    [CreateAssetMenu(fileName = "AbilityIcons", menuName = "Herman/Ability Icon Library")]
    public class AbilityIconLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string actionName;
            public Sprite icon;
        }

        public List<Entry> entries = new List<Entry>();

        public Sprite Get(string actionName)
        {
            foreach (var e in entries)
                if (e.actionName == actionName) return e.icon;
            return null;
        }
    }
}