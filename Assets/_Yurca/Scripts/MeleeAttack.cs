using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "MeleeAttack")]
public class MeleeAttack : AttackBase
{
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("Задержка перед нанесением урона")]
    [SerializeField, Min(0f)] private float hitDelay = 0.08f;

    [SerializeField, Min(1f)] private float multiplier = 1f;

    [SerializeField] private GameObject slashEffectPrefab;

    [SerializeField, Min(0f)] private float rangeMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float slashLifetime = 0.2f;

    // Исходный угол изображения эффекта.
    private const float EffectBaseAngle = 30f;

    private Coroutine attackRoutine;
    private GameObject activeEffect;

    protected override bool IsAttacking => attackRoutine != null;

    protected override void ExecuteAttack(Vector2 direction)
    {
        attackRoutine = StartCoroutine(Attack(direction));
    }

    private IEnumerator Attack(Vector2 direction)
    {
        // Все параметры удара берём из характеристик игрока.
        float range = Stats.AttackRange * rangeMultiplier;
        float angle = Stats.AttackAngle;
        float damage = Stats.Damage * multiplier;
        float duration = Mathf.Max(0.01f, Stats.AttackDuration);
        float delay = Mathf.Clamp(hitDelay / Stats.AttackSpeedMultiplier, 0f, duration);

        yield return new WaitForSeconds(delay);

        SpawnEffect(direction, range, angle);
        DealDamage(direction, range, angle, damage);

        yield return new WaitForSeconds(
            Mathf.Max(0f, duration - delay)
        );

        attackRoutine = null;
    }

    private void SpawnEffect(
        Vector2 direction,
        float range,
        float angle
    )
    {
        if (slashEffectPrefab == null)
            return;

        float rotation =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        activeEffect = Instantiate(
            slashEffectPrefab,
            Origin.position,
            Quaternion.Euler(0f, 0f, rotation),
            Origin
        );

        // Растягивание картинки рассчитано на узкие углы.
        // Ограничиваем только отображение, а не область урона.
        float visualAngle = Mathf.Clamp(angle, 1f, 90f);

        float widthMultiplier =
            Mathf.Tan(visualAngle * 0.5f * Mathf.Deg2Rad) /
            Mathf.Tan(EffectBaseAngle * 0.5f * Mathf.Deg2Rad);

        activeEffect.transform.localScale = new Vector3(
            range,
            range * widthMultiplier,
            1f
        );
        Destroy(activeEffect, slashLifetime);
    }

    private void DealDamage(
        Vector2 direction,
        float range,
        float angle,
        float damage
    )
    {
        DamageUtility.DealArea(
            Origin.position, range, direction, angle,
            damage, targetLayers, Stats.transform);
    }

    public override void CancelAttack()
    {
        bool wasAttacking = attackRoutine != null;

        if (wasAttacking)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        ClearEffect();

        if (wasAttacking)
            NotifyAttackCancelled();
    }

    private void ClearEffect()
    {
        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }
    }

    protected override void OnDisable()
    {
        CancelAttack();
    }
}

