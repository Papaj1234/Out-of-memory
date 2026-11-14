using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private EnemyBrain enemy;

    public EnemyChaseState(EnemyBrain enemy)
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

        if (enemy.IsPlayerInAttackRange())
        {
            enemy.ChangeState(enemy.AttackState);
            return;
        }

        Vector2 direction =
            enemy.DirectionToPlayer();

        enemy.transform.position +=
            (Vector3)(direction *
                      enemy.MoveSpeed *
                      Time.deltaTime);
    }

    public void Exit()
    {
        Debug.Log("EXIT CHASE");
    }
}