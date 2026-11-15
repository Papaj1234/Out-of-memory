using UnityEngine;
using Assets._Herman.Scripts.Rooms;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f;

    protected float currentHealth;
    protected Transform player;
    protected Rigidbody2D rb;
    protected RoomEnemy roomEnemy;

    public float MoveSpeed => moveSpeed;
    public Transform Player => player;
    public bool IsDead => currentHealth <= 0f;

    public float HealthPercent =>
        maxHealth > 0f ? currentHealth / maxHealth : 0f;

    protected Room CurrentRoom =>
        roomEnemy != null
            ? roomEnemy.Room
            : null;

    protected RoomLayout CurrentLayout =>
        CurrentRoom != null
            ? CurrentRoom.Layout
            : null;

    protected bool HasRoomLayout =>
        CurrentLayout != null;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        roomEnemy = GetComponent<RoomEnemy>();

        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    public void Move(Vector2 direction, float speed)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            transform.position +=
                (Vector3)(direction *
                          speed *
                          Time.deltaTime);
        }
    }

    public void Stop()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public virtual void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        currentHealth =
            Mathf.Max(
                0f,
                currentHealth - damage);

        Debug.Log(
            $"[Enemy] {name} получил {damage}, HP {currentHealth}/{maxHealth}",
            this);

        if (IsDead)
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

        return Vector2.Distance(
            transform.position,
            player.position);
    }

    public Vector2 DirectionToPlayer()
    {
        if (player == null)
            return Vector2.zero;

        return (player.position -
                transform.position).normalized;
    }

    public Vector2 DirectionAwayFromPlayer()
    {
        if (player == null)
            return Vector2.zero;

        return (transform.position -
                player.position).normalized;
    }

    protected Vector2Int GetClosestCell(Vector3 worldPosition)
    {
        if (!HasRoomLayout)
            return Vector2Int.zero;

        Vector2Int closestCell =
            Vector2Int.zero;

        float bestDistance =
            float.MaxValue;

        for (int x = 0; x < CurrentLayout.width; x++)
        {
            for (int y = 0; y < CurrentLayout.height; y++)
            {
                Vector3 cellWorld =
                    CurrentRoom.CellToWorld(x, y);

                float distance =
                    Vector2.Distance(
                        worldPosition,
                        cellWorld);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closestCell =
                        new Vector2Int(x, y);
                }
            }
        }

        return closestCell;
    }

    protected Vector2Int GetCurrentCell()
    {
        return GetClosestCell(
            transform.position);
    }

    protected Vector2Int GetPlayerCell()
    {
        if (player == null)
            return Vector2Int.zero;

        return GetClosestCell(
            player.position);
    }

    public Vector2 GetPathDirectionToPlayer()
    {
        if (!HasRoomLayout)
            return DirectionToPlayer();

        Vector2Int startCell =
            GetCurrentCell();

        Vector2Int targetCell =
            GetPlayerCell();

        Vector2Int nextCell =
            GridPathfinder.GetNextStep(
                CurrentLayout,
                startCell,
                targetCell);

        if (nextCell == startCell)
            return Vector2.zero;

        Vector3 targetWorld =
            CurrentRoom.CellToWorld(
                nextCell.x,
                nextCell.y);

        return ((Vector2)targetWorld -
                (Vector2)transform.position)
                .normalized;
    }

    public Vector2 GetPathDirectionAwayFromPlayer()
    {
        if (!HasRoomLayout)
            return DirectionAwayFromPlayer();

        Vector2Int startCell =
            GetCurrentCell();

        Vector2Int playerCell =
            GetPlayerCell();

        Vector2Int bestCell =
            startCell;

        float bestDistance =
            Vector2Int.Distance(
                startCell,
                playerCell);

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

        foreach (var direction in directions)
        {
            Vector2Int candidate =
                startCell + direction;

            if (!CurrentLayout.InBounds(
                    candidate.x,
                    candidate.y))
                continue;

            if (CurrentLayout[candidate.x, candidate.y]
                != Cell.Floor)
                continue;

            float distance =
                Vector2Int.Distance(
                    candidate,
                    playerCell);

            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestCell = candidate;
            }
        }

        if (bestCell == startCell)
            return Vector2.zero;

        Vector3 targetWorld =
            CurrentRoom.CellToWorld(
                bestCell.x,
                bestCell.y);

        return ((Vector2)targetWorld -
                (Vector2)transform.position)
                .normalized;
    }
}