using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private MeleeEnemyBrain enemy;

    public EnemyChaseState(MeleeEnemyBrain enemy)
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

        if (enemy.HealthPercent <= 0.2f)
        {
            enemy.EnterFleeMode();
            return;
        }

        if (enemy.IsPlayerInAttackRange())
        {
            enemy.ChangeState(enemy.WindupState);
            return;
        }

        enemy.Move(enemy.DirectionToPlayer(), enemy.MoveSpeed);
    }

    public void Exit() { }
}