using UnityEngine;

public class MeleeEnemyBrain : CombatEnemyBase
{
    [Header("Melee Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float windupDuration = 0.4f;
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDistance = 2f;
    [SerializeField] private float attackCooldown = 1f;

    private EnemyChaseState chaseState;
    private EnemyWindupState windupState;
    private EnemyAttackState attackState;
    private EnemyRecoveryState recoveryState;
    private EnemyFleeState fleeState;

    private bool hasHitPlayer;
    private bool isFleeing;

    public float AttackRange => attackRange;
    public float WindupDuration => windupDuration;
    public float DashSpeed => dashSpeed;
    public float DashDistance => dashDistance;
    public float AttackCooldown => attackCooldown;

    public bool IsFleeing => isFleeing;

    public EnemyChaseState ChaseState => chaseState;
    public EnemyWindupState WindupState => windupState;
    public EnemyAttackState AttackState => attackState;
    public EnemyRecoveryState RecoveryState => recoveryState;
    public EnemyFleeState FleeState => fleeState;

    protected override void Start()
    {
        base.Start();

        chaseState = new EnemyChaseState(this);
        windupState = new EnemyWindupState(this);
        attackState = new EnemyAttackState(this);
        recoveryState = new EnemyRecoveryState(this);
        fleeState = new EnemyFleeState(this);

        stateMachine.ChangeState(chaseState);
    }

    public bool IsPlayerInAttackRange()
    {
        return DistanceToPlayer() <= attackRange;
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

    protected override void Update()
    {
        base.Update();

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);

            Debug.Log(
                $"HP: {HealthPercent:P0}");
        }
        #endif
    }
}