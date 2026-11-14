using System;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    public event Action<Vector2> AttackStarted;
    public event Action AttackCancelled;

    protected PlayerStats Stats { get; private set; }
    protected Transform Origin { get; private set; }

    protected abstract bool IsAttacking { get; }

    private float nextAttackTime;

    protected virtual void Awake()
    {
        Stats = GetComponent<PlayerStats>();
    }

    public void Initialize(PlayerStats stats, Transform origin)
    {
        Stats = stats;
        Origin = origin;
    }

    public bool TryAttack(Vector2 direction)
    {
        if (!isActiveAndEnabled || Stats == null || Origin == null)
            return false;

        if (IsAttacking || Time.time < nextAttackTime)
            return false;

        if (direction.sqrMagnitude < 0.0001f)
            return false;

        nextAttackTime = Time.time + Stats.AttackCooldown;

        direction = direction.normalized;

        NotifyAttackStarted(direction);
        ExecuteAttack(direction);

        return true;
    }

    protected void NotifyAttackStarted(Vector2 direction)
    {
        AttackStarted?.Invoke(direction);
    }

    protected void NotifyAttackCancelled()
    {
        AttackCancelled?.Invoke();
    }

    protected abstract void ExecuteAttack(Vector2 direction);

    public abstract void CancelAttack();
}