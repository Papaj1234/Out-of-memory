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

        Debug.Log("ENTER WINDUP");
    }

    public void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            enemy.ChangeState(
                enemy.AttackState);
        }
    }

    public void Exit()
    {
        Debug.Log("EXIT WINDUP");
    }
}