using UnityEngine;

public class ShooterApproachState : IEnemyState
{
    private ShooterEnemyBrain enemy;

    public ShooterApproachState(
        ShooterEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ENTER APPROACH");
    }

    public void Update()
    {
        if (enemy.Player == null)
            return;

        float distance =
            enemy.DistanceToPlayer();

        if (distance < enemy.MinDistance)
        {
            enemy.ChangeState(
                enemy.RetreatState);

            return;
        }

        if (distance <= enemy.MaxDistance)
        {
            enemy.ChangeState(
                enemy.ShootState);

            return;
        }

        Vector2 direction =
            enemy.DirectionToPlayer();

        enemy.transform.position +=
            (Vector3)(direction *
                      enemy.MoveSpeed *
                      Time.deltaTime);
    }

    public void Exit()
    {
    }
}