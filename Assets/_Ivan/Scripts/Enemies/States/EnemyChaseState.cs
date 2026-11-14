using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private MeleeEnemyBrain enemy;

    public EnemyChaseState(MeleeEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ENTER CHASE");
    }

    public void Update()
    {
        if (enemy.Player == null)
            return;

        if (enemy.HealthPercent <= 0.2f)
        {
            enemy.EnterFleeMode();
            return;
        }

        if (enemy.IsPlayerInAttackRange())
        {
            enemy.ChangeState(
                enemy.WindupState);

            return;
        }

        Vector2 direction =
            enemy.DirectionToPlayer();

        enemy.transform.position +=
            (Vector3)(
                direction *
                enemy.MoveSpeed *
                Time.deltaTime);
    }

    public void Exit()
    {
        Debug.Log("EXIT CHASE");
    }
}