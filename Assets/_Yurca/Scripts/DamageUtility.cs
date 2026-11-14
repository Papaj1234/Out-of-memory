using System.Collections.Generic;
using UnityEngine;

public static class DamageUtility
{
    public static void DealArea(Vector2 origin, float radius,
        Vector2 direction, float angle, float damage, LayerMask layers, Transform owner)
    {
        if (radius < 0f || damage <= 0f)
            return;

        var receivers = new HashSet<IDamageable>();
        foreach (Collider2D target in Physics2D.OverlapCircleAll(origin, radius, layers))
        {
            if (owner != null &&
                (target.transform == owner || target.transform.IsChildOf(owner)))
                continue;

            Vector2 offset = (Vector2)target.bounds.center - origin;
            if (angle < 360f && offset.sqrMagnitude > 0.0001f &&
                Vector2.Angle(direction, offset) > angle * 0.5f) continue;
            IDamageable receiver = target.GetComponentInParent<IDamageable>();
            if (receiver != null && receivers.Add(receiver)) receiver.TakeDamage(damage);
        }
    }
}
