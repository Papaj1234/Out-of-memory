using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Herman.Scripts.UI
{
    // Панель способностей как в MOBA: 5 слотов, иконка, перезарядка снизу вверх, секунды, клавиша
    public class AbilityBarHUD : MonoBehaviour
    {
        [Tooltip("Пусто = найдёт героя сам")]
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private AbilityIconLibrary icons;
        [SerializeField] private TMP_FontAsset font;

        [Header("Layout")]
        [SerializeField] private float slotSize = 80f;
        [SerializeField] private float spacing = 12f;
        [SerializeField] private float iconPadding = 4f;

        [Header("Keys (Attack, Dash, Ability1, Ability2, Ultimate)")]
        [SerializeField] private string[] keyLabels = { "LMB", "SPACE", "Q", "E", "R" };

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.85f);
        [SerializeField] private Color frameColor = new Color(0.12f, 0.12f, 0.18f, 0.9f);
        [SerializeField] private Color selectedColor = new Color(0.4f, 1f, 0.55f, 1f);
        [SerializeField] private Color cooldownColor = new Color(0f, 0f, 0f, 0.7f);

        private const int SlotCount = 5;

        private class Slot
        {
            public Image frame, icon, overlay;
            public TMP_Text timer, key;
            public EquippedActionBase shownAction;
            public bool initialized;
            public float cooldownMax, lastRemaining;
        }

        private Slot[] slots;

        private void Start()
        {
            if (equipment == null) equipment = FindAnyObjectByType<PlayerEquipment>();
            Build();
        }

        private void Build()
        {
            slots = new Slot[SlotCount];
            float total = SlotCount * slotSize + (SlotCount - 1) * spacing;

            for (int i = 0; i < SlotCount; i++)
            {
                var s = new Slot();

                // рамка/фон
                s.frame = CreateImage($"Slot {i}", transform, emptyColor);
                var frt = s.frame.rectTransform;
                frt.sizeDelta = Vector2.one * slotSize;
                frt.anchoredPosition = new Vector2(-total / 2f + slotSize / 2f + i * (slotSize + spacing), 0f);

                // иконка
                s.icon = CreateImage("Icon", frt, Color.white);
                Stretch(s.icon.rectTransform, iconPadding);
                s.icon.preserveAspect = true;
                s.icon.enabled = false;

                // затемнение перезарядки: тёмная часть сверху, уходит вверх — иконка открывается снизу
                s.overlay = CreateImage("Cooldown", frt, cooldownColor);
                Stretch(s.overlay.rectTransform, iconPadding);
                s.overlay.type = Image.Type.Filled;
                s.overlay.fillMethod = Image.FillMethod.Vertical;
                s.overlay.fillOrigin = (int)Image.OriginVertical.Top;
                s.overlay.fillAmount = 0f;

                // секунды
                s.timer = CreateText("Timer", frt, 32, TextAlignmentOptions.Center);
                Stretch(s.timer.rectTransform, 0f);

                // клавиша — в левом верхнем углу
                s.key = CreateText("Key", frt, 16, TextAlignmentOptions.TopLeft);
                Stretch(s.key.rectTransform, 6f);
                s.key.text = i < keyLabels.Length ? keyLabels[i] : "";

                slots[i] = s;
            }
        }

        private void Update()
        {
            if (equipment == null || slots == null) return;

            for (int i = 0; i < SlotCount; i++)
            {
                var slotId = (EquipmentSlot)i;
                var s = slots[i];
                var action = equipment.GetAction(slotId);

                // способность сменилась — обновляем иконку
                if (!s.initialized || action != s.shownAction)
                {
                    s.initialized = true;
                    s.shownAction = action;

                    Sprite sprite = action != null && icons != null ? icons.Get(CleanName(action)) : null;
                    s.icon.sprite = sprite;
                    s.icon.enabled = sprite != null;
                    s.cooldownMax = 0f;
                    s.lastRemaining = 0f;
                }

                // рамка: пусто / обычная / выбранная способность
                bool isSelected =
                    (slotId == EquipmentSlot.Ability1 && equipment.SelectedAbilitySlot == 0) ||
                    (slotId == EquipmentSlot.Ability2 && equipment.SelectedAbilitySlot == 1);

                s.frame.color = action == null ? emptyColor : isSelected ? selectedColor : frameColor;

                // перезарядка
                float remaining = action != null ? equipment.GetRemainingCooldown(slotId) : 0f;

                if (remaining > s.lastRemaining + 0.01f) s.cooldownMax = remaining; // началась новая
                if (remaining <= 0f) s.cooldownMax = 0f;
                s.lastRemaining = remaining;

                s.overlay.fillAmount = s.cooldownMax > 0f ? Mathf.Clamp01(remaining / s.cooldownMax) : 0f;

                s.timer.text = remaining <= 0f ? ""
                    : remaining >= 1f ? Mathf.CeilToInt(remaining).ToString()
                    : remaining.ToString("0.0");
            }
        }

        private static string CleanName(EquippedActionBase action) =>
            action.gameObject.name.Replace("(Clone)", "").Trim();

        private static void Stretch(RectTransform rt, float padding)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.one * padding;
            rt.offsetMax = -Vector2.one * padding;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private TMP_Text CreateText(string name, Transform parent, float size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.raycastTarget = false;
            return t;
        }
    }
}