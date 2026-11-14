using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(CircleCollider2D))]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "AttackPickup")]
public class AttackPickup : MonoBehaviour
{
    [FormerlySerializedAs("attackPrefab")]
    [SerializeField] private EquippedActionBase actionPrefab;
    [SerializeField, Min(0.1f)] private float dropPickupDelay = 1f;
    public EquippedActionBase ActionPrefab => actionPrefab;
    private CircleCollider2D pickupCollider;
    private PlayerEquipment ignoredPlayer;
    private bool isPickedUp;
    private float pickupAllowedTime;

    private void Awake()
    {
        pickupCollider = GetComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;
    }
    private void Reset() { GetComponent<CircleCollider2D>().isTrigger = true; }
    public void Store(PlayerEquipment owner)
    {
        isPickedUp = true;
        transform.SetParent(owner.transform, true);
        gameObject.SetActive(false);
    }
    public void IgnoreUntilPlayerLeaves(PlayerEquipment owner) { ignoredPlayer = owner; }
    public void Drop(Vector3 position, PlayerEquipment owner)
    {
        transform.SetParent(null, true);
        transform.position = position;
        isPickedUp = false;
        ignoredPlayer = owner;
        pickupAllowedTime = Time.time + Mathf.Max(0.1f, dropPickupDelay);
        gameObject.SetActive(true);
    }
    private void FixedUpdate()
    {
        if (ignoredPlayer == null || Time.time < pickupAllowedTime) return;
        foreach (Collider2D collider in ignoredPlayer.GetComponentsInChildren<Collider2D>())
        {
            if (collider.enabled && collider.gameObject.activeInHierarchy &&
                pickupCollider.bounds.Intersects(collider.bounds)) return;
        }
        ignoredPlayer = null;
    }
    private void OnTriggerEnter2D(Collider2D other) { TryPickUp(other); }
    private void OnTriggerStay2D(Collider2D other) { TryPickUp(other); }
    private void TryPickUp(Collider2D other)
    {
        if (isPickedUp || actionPrefab == null || Time.time < pickupAllowedTime) return;
        PlayerEquipment player = other.GetComponentInParent<PlayerEquipment>();
        if (player == null || player == ignoredPlayer) return;
        isPickedUp = true;
        if (!player.TryEquipItem(this)) isPickedUp = false;
    }
}
