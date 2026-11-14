using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private MeleeEnemyBrain enemy;

    private Vector2 attackDirection;
    private Vector2 startPosition;
    private float maxDashTime;

    public EnemyAttackState(MeleeEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.ResetAttack();
        attackDirection = enemy.DirectionToPlayer();
        startPosition = enemy.transform.position;

        // если рывок упёрся в стену — не застревать навсегда
        maxDashTime = enemy.DashSpeed > 0f ? enemy.DashDistance / enemy.DashSpeed * 1.5f : 0.5f;
    }

    public void Update()
    {
        if (enemy.Player == null)
        {
            enemy.Stop();
            return;
        }

        enemy.Move(attackDirection, enemy.DashSpeed);
        maxDashTime -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.Player.position);
        float travelled = Vector2.Distance(startPosition, enemy.transform.position);

        if (distanceToPlayer < 0.3f || travelled >= enemy.DashDistance || maxDashTime <= 0f)
            enemy.ChangeState(enemy.RecoveryState);
    }

    public void Exit()
    {
        enemy.Stop();
    }
}