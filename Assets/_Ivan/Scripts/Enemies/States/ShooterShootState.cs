using UnityEngine;

public class ShooterShootState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    private float approachTimer;
    private float retreatTimer;

    public ShooterShootState(
        ShooterEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        approachTimer = 0f;
        retreatTimer = 0f;

        Debug.Log("ENTER SHOOT");
    }

    public void Update()
    {
        float distance =
            enemy.DistanceToPlayer();

        if (distance > enemy.MaxDistance)
        {
            approachTimer += Time.deltaTime;

            if (approachTimer >= enemy.StateChangeDelay)
            {
                enemy.ChangeState(
                    enemy.ApproachState);
            }

            return;
        }

        approachTimer = 0f;

        if (distance < enemy.MinDistance)
        {
            retreatTimer += Time.deltaTime;

            if (retreatTimer >= enemy.StateChangeDelay)
            {
                enemy.ChangeState(
                    enemy.RetreatState);
            }

            return;
        }

        retreatTimer = 0f;

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