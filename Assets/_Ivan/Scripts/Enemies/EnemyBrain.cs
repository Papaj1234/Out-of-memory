using UnityEngine;

public class EnemyBrain : EnemyBase
{
    private EnemyStateMachine stateMachine;

    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    private EnemyRecoveryState recoveryState;

    private bool hasHitPlayer;

    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;
    public EnemyRecoveryState RecoveryState => recoveryState;

    protected override void Start()
    {
        base.Start();

        stateMachine = new EnemyStateMachine();

        chaseState = new EnemyChaseState(this);
        attackState = new EnemyAttackState(this);
        recoveryState = new EnemyRecoveryState(this);

        stateMachine.ChangeState(chaseState);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void ChangeState(IEnemyState state)
    {
        stateMachine.ChangeState(state);
    }

    public void ResetAttack()
    {
        hasHitPlayer = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHitPlayer)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            hasHitPlayer = true;

            Debug.Log("PLAYER HIT");

            ChangeState(recoveryState);
        }
    }
}