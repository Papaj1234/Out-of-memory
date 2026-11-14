using UnityEngine;

    // Optional adapter: no modification to Ivan's scripts.
[RequireComponent(typeof(EnemyBase))]
public class EnemyDamageReceiver : MonoBehaviour, IDamageable
{
    private EnemyBase enemy;
    private float pendingDamage;
    private void Awake() { enemy = GetComponent<EnemyBase>(); }
    public void TakeDamage(float damage)
    {
        if (damage <= 0f || enemy == null) return;
        pendingDamage += damage;
        int wholeDamage = Mathf.FloorToInt(pendingDamage);
        if (wholeDamage <= 0) return;
        pendingDamage -= wholeDamage;
        enemy.TakeDamage(wholeDamage);
    }
}
