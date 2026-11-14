using UnityEngine;

public class EnemyProjectile : ProjectileBase
{
    [Tooltip("Урон герою в сердцах")]
    [SerializeField] private float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInParent<EnemyBase>() != null) return;

        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"[Bullet] пуля попала в героя на {damage}", this);
        }

        Destroy(gameObject);
    }
}