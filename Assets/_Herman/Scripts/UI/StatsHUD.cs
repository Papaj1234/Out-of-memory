using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Assets._Herman.Scripts.UI
{
    // Статы героя текстом слева, как в Isaac. Изменения подсвечиваются на пару секунд.
    public class StatsHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [Tooltip("Пусто = найдёт героя сам")]
        [SerializeField] private PlayerStats stats;

        [SerializeField] private float highlightTime = 2f;
        [SerializeField] private string upColor = "#66FF88";
        [SerializeField] private string downColor = "#FF6677";

        private class StatLine
        {
            public string name;
            public System.Func<float> get;
            public string format;
            public float last;
            public float delta;
            public float changedAt = -999f;
        }

        private readonly List<StatLine> lines = new List<StatLine>();
        private readonly StringBuilder sb = new StringBuilder();

        private void Start()
        {
            if (stats == null) stats = FindAnyObjectByType<PlayerStats>();
            if (stats == null) return;

            Add("Speed", () => stats.MoveSpeed, "0.00");
            Add("Damage", () => stats.Damage, "0.0");
            Add("Fire rate", () => FireRate(), "0.00");
            Add("Range", () => stats.AttackRange, "0.0");
            Add("Shot speed", () => stats.ProjectileSpeed, "0.0");
        }

        private void Add(string name, System.Func<float> get, string format)
        {
            lines.Add(new StatLine { name = name, get = get, format = format, last = get() });
        }

        // атак в секунду = 1 / (длительность удара + перезарядка)
        private float FireRate()
        {
            float cycle = stats.AttackDuration + stats.AttackCooldown;
            return cycle > 0.001f ? 1f / cycle : 0f;
        }

        private void Update()
        {
            if (text == null || stats == null) return;

            sb.Clear();

            foreach (var line in lines)
            {
                float value = line.get();

                if (!Mathf.Approximately(value, line.last))
                {
                    line.delta = value - line.last;
                    line.last = value;
                    line.changedAt = Time.time;
                }

                sb.Append(line.name).Append("  ").Append(value.ToString(line.format));

                if (Time.time - line.changedAt < highlightTime)
                {
                    string color = line.delta > 0 ? upColor : downColor;
                    string sign = line.delta > 0 ? "+" : "";
                    sb.Append("  <color=").Append(color).Append('>')
                      .Append(sign).Append(line.delta.ToString(line.format))
                      .Append("</color>");
                }

                sb.Append('\n');
            }

            text.text = sb.ToString();
        }
    }
}