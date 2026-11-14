using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

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

    private readonly Dictionary<object, Vector2> modifiers = new Dictionary<object, Vector2>();
    public float MoveSpeed => moveSpeed;
    public float Damage => damage;
    public float MaxHealth => maxHealth;
    public float ProjectileSpeed => projectileSpeed;
    public float AttackSpeedMultiplier => Product(true);
    public float AttackSizeMultiplier => Product(false);
    public float AttackCooldown => attackCooldown / AttackSpeedMultiplier;
    public float AttackDuration => Mathf.Max(0.01f, attackDuration / AttackSpeedMultiplier);
    public float AttackRange => attackRange * AttackSizeMultiplier;
    public float AttackAngle => Mathf.Clamp(attackAngle * AttackSizeMultiplier, 1f, 360f);
    public float ProjectileRadius => projectileRadius * AttackSizeMultiplier;

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
