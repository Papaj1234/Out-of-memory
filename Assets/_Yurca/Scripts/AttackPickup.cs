using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class AttackPickup : MonoBehaviour
{
    [SerializeField] private AttackBase attackPrefab;

    [Tooltip("Сколько секунд нельзя подбирать выпавший предмет")]
    [SerializeField, Min(0f)] private float dropPickupDelay = 0.5f;

    public AttackBase AttackPrefab => attackPrefab;

    private CircleCollider2D pickupCollider;
    private PlayerAttack ignoredPlayer;

    private bool isPickedUp;
    private float pickupAllowedTime;

    private void Awake()
    {
        pickupCollider = GetComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;
    }

    private void Reset()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public void Drop(Vector3 position, PlayerAttack previousOwner)
    {
        transform.SetParent(null, true);
        transform.position = position;

        isPickedUp = false;
        ignoredPlayer = previousOwner;

        // Минимальная пауза нужна и при нулевом значении
        // в Inspector, чтобы физика успела обновиться.
        pickupAllowedTime =
            Time.time + Mathf.Max(0.1f, dropPickupDelay);

        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (ignoredPlayer == null)
            return;

        // Во время задержки не снимаем блокировку владельца.
        if (Time.time < pickupAllowedTime)
            return;

        Collider2D[] playerColliders =
            ignoredPlayer.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (!playerCollider.enabled ||
                !playerCollider.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (pickupCollider.bounds.Intersects(playerCollider.bounds))
                return;
        }

        // Игрок вышел из области предмета.
        ignoredPlayer = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickUp(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryPickUp(other);
    }

    private void TryPickUp(Collider2D other)
    {
        if (isPickedUp || attackPrefab == null)
            return;

        if (Time.time < pickupAllowedTime)
            return;

        PlayerAttack player =
            other.GetComponentInParent<PlayerAttack>();

        if (player == null || player == ignoredPlayer)
            return;

        isPickedUp = true;

        if (!player.TryEquipItem(this))
            isPickedUp = false;
    }
}