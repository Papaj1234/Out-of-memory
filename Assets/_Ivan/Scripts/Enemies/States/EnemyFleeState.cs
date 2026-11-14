using UnityEngine;

public class EnemyFleeState : IEnemyState
{
    private EnemyBrain enemy;

    public EnemyFleeState(EnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ENTER FLEE");
    }

    public void Update()
    {
        if (enemy.Player == null)
            return;

        Vector2 direction =
            enemy.DirectionAwayFromPlayer();

        enemy.transform.position +=
            (Vector3)(
                direction *
                enemy.MoveSpeed *
                Time.deltaTime);
    }

    public void Exit()
    {
        Debug.Log("EXIT FLEE");
    }
}