using UnityEngine;

public class ShooterRetreatState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    public ShooterRetreatState(
        ShooterEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ENTER RETREAT");
    }

    public void Update()
    {
        if (enemy.Player == null)
            return;

        float distance =
            enemy.DistanceToPlayer();

        if (distance >= enemy.MinDistance)
        {
            enemy.ChangeState(
                enemy.ShootState);

            return;
        }

        Vector2 direction =
            enemy.DirectionAwayFromPlayer();

        enemy.transform.position +=
            (Vector3)(direction *
                      enemy.MoveSpeed *
                      Time.deltaTime);
    }

    public void Exit()
    {
    }
}