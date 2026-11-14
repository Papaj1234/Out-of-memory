using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;

    protected int currentHealth;
    protected Transform player;
    protected Rigidbody2D rb;

    public float MoveSpeed => moveSpeed;
    public Transform Player => player;

    public float HealthPercent =>
        maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    // Движение через физику — упирается в стены, камни, ямы
    public void Move(Vector2 direction, float speed)
    {
        if (rb != null)
            rb.linearVelocity = direction * speed;
        else
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void Stop()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        return Vector2.Distance(transform.position, player.position);
    }

    public Vector2 DirectionToPlayer()
    {
        if (player == null)
            return Vector2.zero;

        return (player.position - transform.position).normalized;
    }

    public Vector2 DirectionAwayFromPlayer()
    {
        if (player == null)
            return Vector2.zero;

        return (transform.position - player.position).normalized;
    }
}