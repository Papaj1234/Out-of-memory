using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : AttackBase
{
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("Задержка перед нанесением урона")]
    [SerializeField, Min(0f)] private float hitDelay = 0.08f;

    [SerializeField, Min(0f)] private float multiplier = 0f;

    [SerializeField] private GameObject slashEffectPrefab;

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
        float range = Stats.AttackRange;
        float angle = Stats.AttackAngle;
        float damage = Stats.Damage * multiplier;
        float duration = Mathf.Max(0.01f, Stats.AttackDuration);
        float delay = Mathf.Clamp(hitDelay, 0f, duration);

        yield return new WaitForSeconds(delay);

        SpawnEffect(direction, range, angle);
        DealDamage(direction, range, angle, damage);

        yield return new WaitForSeconds(
            Mathf.Max(0f, duration - delay)
        );

        ClearEffect();
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
    }

    private void DealDamage(
        Vector2 direction,
        float range,
        float angle,
        float damage
    )
    {
        Vector2 origin = Origin.position;

        Collider2D[] targets = Physics2D.OverlapCircleAll(
            origin,
            range,
            targetLayers
        );

        HashSet<IDamageable> hitTargets = new();

        foreach (Collider2D target in targets)
        {
            // Не наносим урон своему персонажу.
            if (target.transform.IsChildOf(Stats.transform))
                continue;

            Vector2 offset =
                (Vector2)target.bounds.center - origin;

            if (offset.sqrMagnitude > 0.0001f &&
                Vector2.Angle(direction, offset) > angle * 0.5f)
            {
                continue;
            }

            IDamageable damageable =
                target.GetComponentInParent<IDamageable>();

            // Одна цель получает урон один раз за удар.
            if (damageable != null && hitTargets.Add(damageable))
                damageable.TakeDamage(damage);
        }
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

    private void OnDisable()
    {
        CancelAttack();
    }
}