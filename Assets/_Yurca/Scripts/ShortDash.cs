using UnityEngine;

public class ShortDash : DashBase
{
    [SerializeField, Min(0f)] private float distance = 3f;
    [SerializeField, Min(0.01f)] private float duration = 0.2f;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.3f;
    private bool started;
    protected override bool IsBusy => IsInitialized && Context.Movement.IsDashing;
    protected override bool CanExecute() => Context.Movement != null && !Context.Movement.IsDashing;
    protected override void Execute(Vector2 direction)
    {
        started = Context.Movement.BeginDash(this, direction, distance, duration);
        if (started) Context.Health.GrantInvulnerability(invulnerabilityDuration);
    }
    public override void Cancel()
    {
        if (started && IsInitialized && Context.Movement != null)
            Context.Movement.CancelDash(this);
        started = false;
    }
}
