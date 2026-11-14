using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "AttackBase")]
public abstract class AttackBase : EquippedActionBase
{
    public sealed override EquipmentKind Kind => EquipmentKind.Attack;
    public event Action<Vector2> AttackStarted;
    public event Action AttackCancelled;
    protected sealed override float Cooldown => Stats.AttackCooldown;
    protected sealed override bool IsBusy => IsAttacking;
    protected abstract bool IsAttacking { get; }
    public bool TryAttack(Vector2 direction) => TryUse(direction);
    protected sealed override void Execute(Vector2 direction) => ExecuteAttack(direction);
    protected override void OnStarted(Vector2 direction) { AttackStarted?.Invoke(direction); }
    protected void NotifyAttackCancelled()
    {
        AttackCancelled?.Invoke();
        NotifyCancelled();
    }
    public sealed override void Cancel() { CancelAttack(); }
    public abstract void CancelAttack();
    protected abstract void ExecuteAttack(Vector2 direction);
}
