using UnityEngine;

public class RangedAttack : AttackBase
{
    [SerializeField] private Projectile projectilePrefab;

    [Tooltip("Слои целей, получающих урон")]
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("Слои стен и препятствий")]
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField, Min(0f)] private float rangeMultiplier = 4f;

    // Выстрел происходит сразу.
    // Перезарядку проверяет AttackBase.
    protected override bool IsAttacking => false;

    protected override void ExecuteAttack(Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "RangedAttack: назначь Projectile Prefab.",
                this
            );

            return;
        }

        float rotation =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Projectile projectile = Instantiate(
            projectilePrefab,
            Origin.position,
            Quaternion.Euler(0f, 0f, rotation)
        );

        projectile.Initialize(
            Stats.transform,
            direction,
            Stats.Damage,
            Stats.AttackRange * rangeMultiplier,
            Stats.ProjectileSpeed,
            Stats.ProjectileRadius,
            targetLayers,
            obstacleLayers
        );
    }

    public override void CancelAttack()
    {
        // Уже выпущенные снаряды продолжают лететь
        // после замены типа атаки.
    }
}