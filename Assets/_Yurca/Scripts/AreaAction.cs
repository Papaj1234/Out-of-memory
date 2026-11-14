using UnityEngine;

public class AreaAction : AbilityBase
{
    [SerializeField, Min(0.01f)] private float duration = 5f;
    [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;

    [SerializeField, Min(0.01f)] private float radiusMultiplier = 2f;
    [SerializeField, Min(0f)] private float damageMultiplier = 0.5f;

    [SerializeField] private LayerMask targetLayers;

    [SerializeField] private Color tint =
        new Color(0f, 0f, 0f, 0.45f);

    protected override void Execute(Vector2 direction)
    {
        GameObject areaObject = new GameObject("DamageArea");
        areaObject.transform.position = Origin.position;
        DamageArea area = areaObject.AddComponent<DamageArea>();
        area.Initialize(
            Stats.transform,
            Stats.AttackRange * radiusMultiplier,
            Stats.Damage * damageMultiplier,
            duration,
            tickInterval,
            targetLayers,
            tint
        );
    }

    public override void Cancel()
    {
        // The created area runs independently until its duration expires.
    }
}