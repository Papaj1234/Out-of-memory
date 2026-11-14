using UnityEngine;

public class EnemyProjectile : ProjectileBase
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER HIT");

            Destroy(gameObject);
        }
    }
}