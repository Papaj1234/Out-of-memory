using UnityEngine;

public class TestContactDamage : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float damage = 0.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider2D other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
            health.TakeDamage(damage);
    }
}