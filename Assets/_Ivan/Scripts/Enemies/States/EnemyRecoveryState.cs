using UnityEngine;

public class EnemyRecoveryState : IEnemyState
{
    private EnemyBrain enemy;

    private float timer;

    public EnemyRecoveryState(EnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        timer = enemy.AttackCooldown;

        Debug.Log("ENTER RECOVERY");
    }

    public void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            enemy.ChangeState(enemy.ChaseState);
        }
    }

    public void Exit()
    {
        Debug.Log("EXIT RECOVERY");
    }
}