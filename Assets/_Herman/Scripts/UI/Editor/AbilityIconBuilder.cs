using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets._Herman.Scripts.UI.Editor
{
    // Меню: Tools > Herman > Build Ability Icons
    // Находит все префабы способностей (EquippedActionBase) и ставит им картинки
    // из Sprites/Abilities с ТАКИМ ЖЕ именем, как у префаба (DashAction.png -> DashAction).
    public static class AbilityIconBuilder
    {
        private const string SpritesFolder = "Assets/_Herman/Sprites/Abilities";
        private const string LibraryPath = "Assets/_Herman/Settings/AbilityIcons.asset";

        [MenuItem("Tools/Herman/Build Ability Icons")]
        public static void Build()
        {
            var library = AssetDatabase.LoadAssetAtPath<AbilityIconLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AbilityIconLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            // картинки по имени файла
            var sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            if (AssetDatabase.IsValidFolder(SpritesFolder))
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesFolder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                        if (asset is Sprite sprite)
                        {
                            sprites[Path.GetFileNameWithoutExtension(path)] = sprite;
                            break;
                        }
                }
            }

            // все префабы способностей в проекте
            library.entries.Clear();
            var missing = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab == null || prefab.GetComponent<EquippedActionBase>() == null) continue;

                sprites.TryGetValue(prefab.name, out Sprite icon);
                library.entries.Add(new AbilityIconLibrary.Entry { actionName = prefab.name, icon = icon });
                if (icon == null) missing.Add(prefab.name);
            }

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            Debug.Log($"Способностей: {library.entries.Count}. Без иконки (назови картинки так): {string.Join(", ", missing)}");
            Selection.activeObject = library;
        }
    }
}