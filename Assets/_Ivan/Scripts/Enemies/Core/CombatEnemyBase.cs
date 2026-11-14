using UnityEngine;

public abstract class CombatEnemyBase : EnemyBase
{
    [Header("Combat")]
    [SerializeField] protected int damage = 10;

    protected EnemyStateMachine stateMachine;

    public int Damage => damage;

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
}