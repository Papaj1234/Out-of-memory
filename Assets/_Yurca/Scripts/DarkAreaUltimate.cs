using UnityEngine;

public class DarkAreaUltimate : UltimateBase
{
    [SerializeField, Min(0.01f)] private float radiusMultiplier = 2f;
    [SerializeField, Min(0f)] private float damageMultiplier = 0.5f;
    [SerializeField, Min(0.01f)] private float duration = 5f;
    [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private Color tint = new Color(0f, 0f, 0f, 0.45f);
    protected override void Execute(Vector2 direction)
    {
        GameObject instance = new GameObject("DamageArea");
        instance.transform.position = Origin.position;
        instance.AddComponent<DamageArea>().Initialize(Stats.transform,
            Stats.AttackRange * radiusMultiplier, Stats.Damage * damageMultiplier,
            duration, tickInterval, targetLayers, tint);
    }
    public override void Cancel()
    {
        // Existing zones, like launched projectiles, finish independently.
    }
}
