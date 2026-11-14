using UnityEngine;

public class EnemyFleeState : IEnemyState
{
    private MeleeEnemyBrain enemy;

    public EnemyFleeState(MeleeEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter() { }

    public void Update()
    {
        if (enemy.Player == null)
        {
            enemy.Stop();
            return;
        }

        enemy.Move(enemy.DirectionAwayFromPlayer(), enemy.MoveSpeed);
    }

    public void Exit() { }
}