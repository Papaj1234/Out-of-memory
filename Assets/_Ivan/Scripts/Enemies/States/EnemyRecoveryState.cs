using UnityEngine;

public class EnemyRecoveryState : IEnemyState
{
    private MeleeEnemyBrain enemy;
    private float timer;

    public EnemyRecoveryState(MeleeEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        timer = enemy.AttackCooldown;
        enemy.Stop();
    }

    public void Update()
    {
        enemy.Stop();

        if (enemy.HealthPercent <= 0.2f)
        {
            enemy.EnterFleeMode();
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            enemy.ChangeState(enemy.ChaseState);
    }

    public void Exit() { }
}