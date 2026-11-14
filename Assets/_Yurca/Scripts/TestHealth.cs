using UnityEngine;

public class TestHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log(
            $"{name}: получено {damage} урона. Осталось HP: {health}",
            this
        );

        if (health <= 0f)
            Destroy(gameObject);
    }
}