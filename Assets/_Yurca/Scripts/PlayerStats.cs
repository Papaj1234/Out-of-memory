using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Assets._Herman.Scripts.Items;

[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerStats")]
public class PlayerStats : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float attackRange = 2f;
    [SerializeField, Range(1f, 360f)] private float attackAngle = 30f;
    [SerializeField, Min(0.01f)] private float attackDuration = 0.2f;
    [SerializeField, Min(0f)] private float attackCooldown = 0.4f;
    [SerializeField, Min(0f)] private float damage = 10f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 10f;
    [SerializeField, Min(0.01f)] private float projectileRadius = 0.15f;
    [SerializeField, Min(0.5f)] private float maxHealth = 3f;

    [Header("Passive items")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    private readonly Dictionary<object, Vector2> modifiers = new Dictionary<object, Vector2>();

    // ===== Итоговые статы = база + предметы (имена прежние) =====
    public float MoveSpeed => ApplyItems(StatType.MoveSpeed, moveSpeed);
    public float Damage => ApplyItems(StatType.Damage, damage);
    public float MaxHealth => ApplyItems(StatType.MaxHealth, maxHealth);
    public float ProjectileSpeed => ApplyItems(StatType.ProjectileSpeed, projectileSpeed);

    public float AttackSpeedMultiplier => Mathf.Clamp(Product(true) * ItemMultiplier(StatType.AttackSpeed), 0.01f, 100f);
    public float AttackSizeMultiplier => Product(false);
    public float AttackCooldown => attackCooldown / AttackSpeedMultiplier;
    public float AttackDuration => Mathf.Max(0.01f, attackDuration / AttackSpeedMultiplier);
    public float AttackRange => ApplyItems(StatType.AttackRange, attackRange) * AttackSizeMultiplier;
    public float AttackAngle => Mathf.Clamp(ApplyItems(StatType.AttackAngle, attackAngle) * AttackSizeMultiplier, 1f, 360f);
    public float ProjectileRadius => ApplyItems(StatType.ProjectileRadius, projectileRadius) * AttackSizeMultiplier;

    // ===== Предметы =====
    public IReadOnlyList<ItemData> Items => items;

    public void AddItem(ItemData item)
    {
        if (item != null) items.Add(item);
    }

    public bool RemoveItem(ItemData item) => items.Remove(item);

    // (база + плоские бонусы) * (1 + сумма процентов). Считается каждый раз,
    // поэтому правка ассета предмета в Play Mode применяется сразу.
    private float ApplyItems(StatType stat, float baseValue)
    {
        float flat = 0f;
        float percent = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];
            if (item == null) continue;

            IReadOnlyList<StatModifier> mods = item.Modifiers;
            for (int j = 0; j < mods.Count; j++)
            {
                if (mods[j].stat != stat) continue;
                flat += mods[j].flat;
                percent += mods[j].percent;
            }
        }

        float value = (baseValue + flat) * Mathf.Max(0f, 1f + percent);
        return stat == StatType.MaxHealth ? Mathf.Max(1f, value) : Mathf.Max(0f, value);
    }

    // для статов-множителей (скорость атаки): база = 1
    private float ItemMultiplier(StatType stat) => Mathf.Clamp(ApplyItems(stat, 1f), 0.1f, 5f);

    // ===== Модификаторы атак (как было) =====
    private float Product(bool speed)
    {
        float value = 1f;
        foreach (Vector2 modifier in modifiers.Values)
            value *= speed ? modifier.x : modifier.y;
        return Mathf.Clamp(value, 0.01f, 100f);
    }

    public void SetAttackModifier(object source, float speed, float size)
    {
        if (source == null) return;
        modifiers[source] = new Vector2(Mathf.Max(0.01f, speed), Mathf.Max(0.01f, size));
    }

    public void RemoveAttackModifier(object source)
    {
        if (source != null) modifiers.Remove(source);
    }
}