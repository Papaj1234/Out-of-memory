using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private MeleeEnemyBrain enemy;

    private Vector2 attackDirection;
    private Vector2 startPosition;

    public EnemyAttackState(MeleeEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.ResetAttack();

        attackDirection =
            enemy.DirectionToPlayer();

        startPosition =
            enemy.transform.position;

        Debug.Log("ENTER ATTACK");
    }

    public void Update()
    {
        if (enemy.Player == null)
            return;

        enemy.transform.position +=
            (Vector3)(
                attackDirection *
                enemy.DashSpeed *
                Time.deltaTime);

        float distanceToPlayer =
            Vector2.Distance(
                enemy.transform.position,
                enemy.Player.position);

        if (distanceToPlayer < 0.3f)
        {
            enemy.ChangeState(
                enemy.RecoveryState);

            return;
        }

        float travelled =
            Vector2.Distance(
                startPosition,
                enemy.transform.position);

        if (travelled >= enemy.DashDistance)
        {
            enemy.ChangeState(
                enemy.RecoveryState);
        }
    }

    public void Exit()
    {
        Debug.Log("EXIT ATTACK");
    }
}
