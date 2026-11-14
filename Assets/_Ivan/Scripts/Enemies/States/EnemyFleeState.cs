using UnityEngine;

public class EnemyFleeState : IEnemyState
{
    private MeleeEnemyBrain enemy;

    public EnemyFleeState(MeleeEnemyBrain enemy)
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