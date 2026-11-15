using UnityEngine;

public class ShooterApproachState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    public ShooterApproachState(ShooterEnemyBrain enemy)
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

        float distance = enemy.DistanceToPlayer();

        if (distance < enemy.MinDistance)
        {
            enemy.ChangeState(enemy.RetreatState);
            return;
        }

        if (distance <= enemy.MaxDistance)
        {
            enemy.ChangeState(enemy.ShootState);
            return;
        }

        enemy.Move(enemy.PathToPlayer(), enemy.MoveSpeed);
    }

    public void Exit() { }
}