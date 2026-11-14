using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Herman.Scripts.UI
{
    // Сердца как в Isaac: кружки, заполнение 1 / 0.5 / 0
    public class HeartsHUD : MonoBehaviour
    {
        [Tooltip("Пусто = найдёт героя сам")]
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Sprite heartSprite;

        [SerializeField] private Color fullColor = new Color(0.9f, 0.1f, 0.2f, 1f);
        [SerializeField] private Color emptyColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);

        [SerializeField] private float size = 32f;
        [SerializeField] private float spacing = 6f;
        [SerializeField] private int perRow = 6;

        private readonly List<GameObject> hearts = new List<GameObject>();
        private readonly List<Image> fills = new List<Image>();
        private int builtCount = -1;

        private void Start()
        {
            if (health == null) health = FindAnyObjectByType<PlayerHealth>();
        }

        private void Update()
        {
            if (health == null) return;

            int count = Mathf.CeilToInt(health.MaxHealth - 0.001f);
            if (count != builtCount)
                Rebuild(count);

            for (int i = 0; i < fills.Count; i++)
                fills[i].fillAmount = Mathf.Clamp01(health.CurrentHealth - i); // 1, 0.5 или 0
        }

        private void Rebuild(int count)
        {
            foreach (var h in hearts) Destroy(h);
            hearts.Clear();
            fills.Clear();

            for (int i = 0; i < count; i++)
            {
                // фон — пустое сердце
                var bg = CreateImage($"Heart {i + 1}", transform, emptyColor);
                var rt = bg.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                rt.sizeDelta = Vector2.one * size;
                rt.anchoredPosition = new Vector2((i % perRow) * (size + spacing),
                                                  -(i / perRow) * (size + spacing));

                // заливка — красная часть
                var fill = CreateImage("Fill", rt, fullColor);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = (int)Image.OriginHorizontal.Left;

                var frt = fill.rectTransform;
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = Vector2.one;
                frt.offsetMin = frt.offsetMax = Vector2.zero;

                hearts.Add(bg.gameObject);
                fills.Add(fill);
            }

            builtCount = count;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = heartSprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}