using UnityEngine;

public class DashAction : DashBase
{
    [SerializeField, Min(0f)] private float distance = 3f;
    [SerializeField, Min(0.01f)] private float duration = 0.2f;
    [SerializeField, Min(0f)] private float invulnerability = 0.3f;

    private PlayerMovement movement;
    private bool ownsDash;

    protected override bool IsBusy =>
        ownsDash && movement != null && movement.IsDashing;

    protected override bool CanExecute() =>
        Context.Movement != null && !Context.Movement.IsDashing;

    protected override void Execute(Vector2 direction)
    {
        movement = Context.Movement;
        ownsDash = movement.BeginDash(this, direction, distance, duration);

        if (!ownsDash)
            return;

        Context.Health.GrantInvulnerability(invulnerability);
    }

    public override void Cancel()
    {
        if (ownsDash && movement != null)
            movement.CancelDash(this);

        ownsDash = false;
    }
}