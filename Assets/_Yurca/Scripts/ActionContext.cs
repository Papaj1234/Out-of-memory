using System;
using UnityEngine;

public sealed class ActionContext
{
    public readonly PlayerStats Stats;
    public readonly PlayerHealth Health;
    public readonly PlayerMovement Movement;
    public readonly Transform Origin;
    public ActionContext(PlayerStats stats, PlayerHealth health,
        PlayerMovement movement, Transform origin)
    {
        if (stats == null) throw new ArgumentNullException(nameof(stats));
        if (health == null) throw new ArgumentNullException(nameof(health));
        if (movement == null) throw new ArgumentNullException(nameof(movement));
        if (origin == null) throw new ArgumentNullException(nameof(origin));

        Stats = stats; Health = health; Movement = movement; Origin = origin;
    }
}
// Lives in the slot, not in the replaceable prefab instance.
public sealed class ActionCooldown
{
    public float ReadyAt { get; private set; }
    public bool IsReady => Time.time >= ReadyAt;
    public float Remaining => Mathf.Max(0f, ReadyAt - Time.time);
    public void Begin(float seconds) { ReadyAt = Time.time + Mathf.Max(0f, seconds); }
}
