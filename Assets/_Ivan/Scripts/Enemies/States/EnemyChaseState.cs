using UnityEngine;

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
        Vector2 direction =
            (enemy.Player.position -
             enemy.transform.position).normalized;

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