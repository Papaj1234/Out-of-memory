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
        {
            enemy.Stop();
            return;
        }

        enemy.Move(
            enemy.GetPathDirectionAwayFromPlayer(),
            enemy.MoveSpeed);
    }

    public void Exit()
    {
        Debug.Log("EXIT FLEE");
    }
}