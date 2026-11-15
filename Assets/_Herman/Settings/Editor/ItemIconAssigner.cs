using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets._Herman.Scripts.Items.Editor
{
    // Меню: Tools > Herman > Assign Item Icons
    // Берёт картинки из Sprites/Items и ставит их в предметы с ТАКИМ ЖЕ именем файла.
    // Пример: Sprites/Items/GPU.png -> Settings/Items/GPU.asset
    public static class ItemIconAssigner
    {
        private const string SpritesFolder = "Assets/_Herman/Sprites/Items";
        private const string ItemsFolder = "Assets/_Herman/Settings/Items";

        [MenuItem("Tools/Herman/Assign Item Icons")]
        public static void Assign()
        {
            if (!AssetDatabase.IsValidFolder(SpritesFolder))
            {
                Debug.LogWarning($"Нет папки {SpritesFolder}");
                return;
            }

            // имя файла -> спрайт (если картинка нарезана, берём первый спрайт)
            var sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string key = Path.GetFileNameWithoutExtension(path);

                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Sprite sprite)
                    {
                        sprites[key] = sprite;
                        break;
                    }
                }
            }

            int assigned = 0;
            var missing = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:ItemData", new[] { ItemsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                string key = Path.GetFileNameWithoutExtension(path);

                if (!sprites.TryGetValue(key, out Sprite sprite))
                {
                    missing.Add(key);
                    continue;
                }

                var so = new SerializedObject(item);
                so.FindProperty("icon").objectReferenceValue = sprite;
                so.ApplyModifiedProperties();
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Иконок назначено: {assigned}. Без иконки: {string.Join(", ", missing)}");
        }
    }
}