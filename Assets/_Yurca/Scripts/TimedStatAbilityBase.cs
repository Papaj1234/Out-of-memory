using UnityEngine;

public abstract class TimedStatAbilityBase : AbilityBase
{
    [SerializeField, Min(0.01f)] private float duration = 5f;
    [SerializeField, Min(1f)] private float multiplier = 1.5f;
    protected abstract bool BoostSpeed { get; }
    protected override bool IsBusy => active;
    private bool active;
    private float endsAt;
    protected override void Execute(Vector2 direction)
    {
        active = true;
        endsAt = Time.time + duration;
        Stats.SetAttackModifier(this,
            BoostSpeed ? multiplier : 1f, BoostSpeed ? 1f : multiplier);
    }
    protected virtual void Update()
    {
        if (active && (Time.time >= endsAt || Context.Health.IsDead)) Cancel();
    }
    public override void Cancel()
    {
        if (!active) return;
        active = false;
        if (IsInitialized && Stats != null) Stats.RemoveAttackModifier(this);
        NotifyCancelled();
    }
}
