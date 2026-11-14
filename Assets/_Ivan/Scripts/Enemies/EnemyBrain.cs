using UnityEngine;

public class EnemyBrain : EnemyBase
{
    private EnemyStateMachine stateMachine;

    private EnemyChaseState chaseState;
    private EnemyWindupState windupState;
    private EnemyAttackState attackState;
    private EnemyRecoveryState recoveryState;
    private EnemyFleeState fleeState;

    private bool hasHitPlayer;
    private bool isFleeing;

    public EnemyChaseState ChaseState => chaseState;
    public EnemyWindupState WindupState => windupState;
    public EnemyAttackState AttackState => attackState;
    public EnemyRecoveryState RecoveryState => recoveryState;
    public EnemyFleeState FleeState => fleeState;

    public bool IsFleeing => isFleeing;

    protected override void Start()
    {
        base.Start();

        stateMachine = new EnemyStateMachine();

        chaseState = new EnemyChaseState(this);
        windupState = new EnemyWindupState(this);
        attackState = new EnemyAttackState(this);
        recoveryState = new EnemyRecoveryState(this);
        fleeState = new EnemyFleeState(this);

        stateMachine.ChangeState(chaseState);
    }

    private void Update()
    {
        stateMachine.Update();

        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }
    }

    public void ChangeState(IEnemyState state)
    {
        stateMachine.ChangeState(state);
    }

    public void ResetAttack()
    {
        hasHitPlayer = false;
    }

    public void EnterFleeMode()
    {
        if (isFleeing)
            return;

        isFleeing = true;

        ChangeState(fleeState);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHitPlayer)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            hasHitPlayer = true;

            ChangeState(recoveryState);
        }
    }
}