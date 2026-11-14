using UnityEngine;

public abstract class CombatEnemyBase : EnemyBase
{
    [Header("Combat")]
    [SerializeField] protected int damage = 10;

    [Tooltip("Урон герою в сердцах (1 = целое сердце, 0.5 = половинка)")]
    [SerializeField] protected float damageToPlayer = 0.5f;

    protected EnemyStateMachine stateMachine;

    public int Damage => damage;
    public float DamageToPlayer => damageToPlayer;

    protected override void Start()
    {
        base.Start();
        stateMachine = new EnemyStateMachine();
    }

    public void ChangeState(IEnemyState state)
    {
        stateMachine.ChangeState(state);
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    // true, если объект — герой и урон нанесён (неуязвимость героя сама гасит повторы)
    protected bool TryDamagePlayer(GameObject target)
    {
        var health = target.GetComponentInParent<PlayerHealth>();
        if (health == null) return false;

        health.TakeDamage(damageToPlayer);
        return true;
    }
}