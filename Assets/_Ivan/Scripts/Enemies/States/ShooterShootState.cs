using UnityEngine;

public class ShooterShootState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    public ShooterShootState(
        ShooterEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ENTER SHOOT");
    }

    public void Update()
    {
        float distance =
            enemy.DistanceToPlayer();

        if (distance > enemy.MaxDistance)
        {
            enemy.ChangeState(
                enemy.ApproachState);

            return;
        }

        if (distance < enemy.MinDistance)
        {
            enemy.ChangeState(
                enemy.RetreatState);

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
        Vector2 direction =
            enemy.DirectionToPlayer();

        EnemyProjectile projectile =
            Object.Instantiate(
                enemy.ProjectilePrefab,
                enemy.FirePoint.position,
                Quaternion.identity);

        projectile.Initialize(direction);
    }

    public void Exit()
    {
    }
}