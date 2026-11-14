using UnityEngine;

public class ShooterShootState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    public ShooterShootState(ShooterEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.Stop();
    }

    public void Update()
    {
        enemy.Stop();

        if (enemy.Player == null)
            return;

        float distance = enemy.DistanceToPlayer();

        if (distance > enemy.MaxDistance)
        {
            enemy.ChangeState(enemy.ApproachState);
            return;
        }

        if (distance < enemy.MinDistance)
        {
            enemy.ChangeState(enemy.RetreatState);
            return;
        }

        if (enemy.CanShoot)
        {
            Shoot();
            enemy.ResetShootCooldown();
        }
    }

    private void Shoot()
    {
        if (enemy.ProjectilePrefab == null || enemy.FirePoint == null)
            return;

        EnemyProjectile projectile = Object.Instantiate(
            enemy.ProjectilePrefab,
            enemy.FirePoint.position,
            Quaternion.identity);

        projectile.Initialize(enemy.DirectionToPlayer());
    }

    public void Exit() { }
}