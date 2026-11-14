using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class AttackPickup : MonoBehaviour
{
    [SerializeField] private AttackBase attackPrefab;

    public AttackBase AttackPrefab => attackPrefab;

    private CircleCollider2D pickupCollider;
    private PlayerAttack ignoredPlayer;
    private bool isPickedUp;

    private void Awake()
    {
        pickupCollider = GetComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;
    }

    private void Reset()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    // Выпавший предмет временно игнорирует того,
    // кто его только что выбросил.
    public void IgnoreUntilPlayerLeaves(PlayerAttack player)
    {
        ignoredPlayer = player;
    }

    private void Update()
    {
        if (ignoredPlayer == null)
            return;

        // Проверяем, отошёл ли герой от области подбора.
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

        PlayerAttack player =
            other.GetComponentInParent<PlayerAttack>();

        if (player == null || player == ignoredPlayer)
            return;

        // Блокируем повторные вызовы от нескольких коллайдеров.
        isPickedUp = true;

        if (!player.TryEquipItem(this))
        {
            isPickedUp = false;
            return;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}