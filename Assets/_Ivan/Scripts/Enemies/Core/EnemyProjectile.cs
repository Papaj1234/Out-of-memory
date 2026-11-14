using UnityEngine;

public class EnemyProjectile : ProjectileBase
{
    [Tooltip("Урон герою в сердцах")]
    [SerializeField] private float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // невидимые зоны и других мобов пролетаем
        if (other.isTrigger) return;
        if (other.GetComponentInParent<EnemyBase>() != null) return;

        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        // герой, стена, камень, закрытая дверь — пуля исчезает
        Destroy(gameObject);
    }
}