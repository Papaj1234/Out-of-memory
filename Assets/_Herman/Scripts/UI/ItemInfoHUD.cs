using System.Text;
using TMPro;
using UnityEngine;
using Assets._Herman.Scripts.Items;

namespace Assets._Herman.Scripts.UI
{
    // Пока в комнате лежит предмет — показывает его название и бонусы
    public class ItemInfoHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private string nameColor = "#FFFFFF";
        [SerializeField] private string upColor = "#66FF88";
        [SerializeField] private string downColor = "#FF6677";

        private ItemPickup current;
        private ItemData shownItem;
        private float nextSearch;
        private readonly StringBuilder sb = new StringBuilder();

        private void Reset() => text = GetComponent<TMP_Text>();

        private void Update()
        {
            if (text == null) return;

            // ищем предмет раз в 0.2 сек (выключенные комнаты не находятся)
            if (Time.time >= nextSearch)
            {
                current = FindAnyObjectByType<ItemPickup>();
                nextSearch = Time.time + 0.2f;
            }

            ItemData item = current != null ? current.Item : null;
            if (item == shownItem) return;

            shownItem = item;
            text.text = item != null ? Describe(item) : "";
        }

        private string Describe(ItemData item)
        {
            sb.Clear();
            sb.Append("<color=").Append(nameColor).Append('>')
              .Append(item.DisplayName).Append("</color>\n");

            foreach (var m in item.Modifiers)
            {
                if (!Mathf.Approximately(m.flat, 0f))
                    AppendLine(StatName(m.stat), m.flat, m.flat.ToString("+0.##;-0.##"));

                if (!Mathf.Approximately(m.percent, 0f))
                    AppendLine(StatName(m.stat), m.percent, (m.percent * 100f).ToString("+0;-0") + "%");
            }

            return sb.ToString();
        }

        private void AppendLine(string stat, float value, string amount)
        {
            sb.Append("<color=").Append(value > 0 ? upColor : downColor).Append('>')
              .Append(amount).Append(' ').Append(stat).Append("</color>\n");
        }

        private static string StatName(StatType stat)
        {
            switch (stat)
            {
                case StatType.MoveSpeed: return "Speed";
                case StatType.Damage: return "Damage";
                case StatType.AttackSpeed: return "Fire rate";
                case StatType.AttackRange: return "Range";
                case StatType.AttackAngle: return "Swing angle";
                case StatType.ProjectileSpeed: return "Shot speed";
                case StatType.ProjectileRadius: return "Shot size";
                case StatType.MaxHealth: return "Health";
                default: return stat.ToString();
            }
        }
    }
}