using System;
using UnityEngine;

public abstract class EquippedActionBase : MonoBehaviour
{
    public abstract EquipmentKind Kind { get; }
    public bool IsInitialized => Context != null;
    public float RemainingCooldown => cooldown == null ? 0f : cooldown.Remaining;
    public event Action<Vector2> Started;
    public event Action Cancelled;
    protected ActionContext Context { get; private set; }
    protected PlayerStats Stats => Context.Stats;
    protected Transform Origin => Context.Origin;
    protected virtual bool IsBusy => false;
    protected abstract float Cooldown { get; }
    private ActionCooldown cooldown;

    public void Initialize(ActionContext context, ActionCooldown slotCooldown)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (slotCooldown == null) throw new ArgumentNullException(nameof(slotCooldown));
        if (Context != null && Context.Health != null)
            Context.Health.Died -= Cancel;
        Context = context;
        cooldown = slotCooldown;
        if (Context.Health != null) Context.Health.Died += Cancel;
    }
    public bool TryUse(Vector2 direction)
    {
        if (!IsInitialized || !isActiveAndEnabled ||
            !Stats.gameObject.activeInHierarchy ||
            (Context.Health != null && Context.Health.IsDead) ||
            IsBusy || !cooldown.IsReady ||
            direction.sqrMagnitude < 0.0001f || !CanExecute())
            return false;

        direction.Normalize();
        cooldown.Begin(Cooldown);
        Execute(direction);
        Started?.Invoke(direction);
        OnStarted(direction);
        return true;
    }
    protected virtual bool CanExecute() => true;
    protected virtual void OnStarted(Vector2 direction) { }
    protected void NotifyCancelled() { Cancelled?.Invoke(); }
    public abstract void Cancel();
    protected abstract void Execute(Vector2 direction);
    protected virtual void OnDisable() { Cancel(); }
    protected virtual void OnDestroy()
    {
        Cancel();
        if (Context != null && Context.Health != null) Context.Health.Died -= Cancel;
    }
}
