using UnityEngine;

public class EnemyWindupState : IEnemyState
{
    private MeleeEnemyBrain enemy;
    private float timer;

    public EnemyWindupState(MeleeEnemyBrain enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        timer = enemy.WindupDuration;
        enemy.Stop();
    }

    public void Update()
    {
        enemy.Stop();
        timer -= Time.deltaTime;

        if (timer <= 0f)
            enemy.ChangeState(enemy.AttackState);
    }

    public void Exit() { }
}