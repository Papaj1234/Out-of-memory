using UnityEngine;

public class ShooterRetreatState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    public ShooterRetreatState(ShooterEnemyBrain enemy)
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

        if (enemy.DistanceToPlayer() >= enemy.MinDistance)
        {
            enemy.ChangeState(enemy.ShootState);
            return;
        }

        enemy.Move(enemy.PathAwayFromPlayer(), enemy.MoveSpeed);
    }

    public void Exit() { }
}