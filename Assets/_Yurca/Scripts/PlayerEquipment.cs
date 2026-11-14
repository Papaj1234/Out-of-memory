using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats), typeof(PlayerMovement), typeof(PlayerHealth))]
public class PlayerEquipment : MonoBehaviour
{
    private const int SlotCount = (int)EquipmentSlot.Ultimate + 1;

    [SerializeField] private Transform actionOrigin;
    [Header("Optional starting item prefabs")]
    [SerializeField] private AttackPickup startingAttack;
    [SerializeField] private AttackPickup startingDash;
    [SerializeField] private AttackPickup startingAbility1;
    [SerializeField] private AttackPickup startingAbility2;
    [SerializeField] private AttackPickup startingUltimate;

    public int SelectedAbilitySlot { get; private set; }
    public event Action<EquipmentSlot, EquippedActionBase> SlotChanged;
    private readonly EquippedActionBase[] actions = new EquippedActionBase[SlotCount];
    private readonly AttackPickup[] items = new AttackPickup[SlotCount];
    private readonly ActionCooldown[] cooldowns = {
        new ActionCooldown(), new ActionCooldown(), new ActionCooldown(),
        new ActionCooldown(), new ActionCooldown()
    };
    private ActionContext context;
    public Transform Origin { get { EnsureContext(); return context.Origin; } }

    private void Awake() { EnsureContext(); }
    private void EnsureContext()
    {
        if (context != null) return;
        context = new ActionContext(GetComponent<PlayerStats>(),
            GetComponent<PlayerHealth>(), GetComponent<PlayerMovement>(),
            actionOrigin != null ? actionOrigin : transform);
    }
    private void Start()
    {
        GiveStarting(startingAttack, EquipmentSlot.Attack);
        GiveStarting(startingDash, EquipmentSlot.Dash);
        GiveStarting(startingAbility1, EquipmentSlot.Ability1);
        GiveStarting(startingAbility2, EquipmentSlot.Ability2);
        GiveStarting(startingUltimate, EquipmentSlot.Ultimate);
    }
    private void GiveStarting(AttackPickup prefab, EquipmentSlot slot)
    {
        if (prefab == null || GetAction(slot) != null) return;
        if (prefab.ActionPrefab == null)
        {
            Debug.LogWarning("Starting item has no action prefab: " + slot, this);
            return;
        }
        if (!Fits(slot, prefab.ActionPrefab.Kind))
        {
            Debug.LogWarning("Starting item has the wrong category: " + slot, this);
            return;
        }
        AttackPickup item = Instantiate(prefab, transform.position, Quaternion.identity);
        item.IgnoreUntilPlayerLeaves(this);
        if (!Equip(item, slot)) Destroy(item.gameObject);
    }
    public EquippedActionBase GetAction(EquipmentSlot slot) =>
        actions[GetSlotIndex(slot)];

    public float GetRemainingCooldown(EquipmentSlot slot) =>
        cooldowns[GetSlotIndex(slot)].Remaining;

    public void SelectAbilitySlot(int index) { SelectedAbilitySlot = Mathf.Clamp(index, 0, 1); }

    public bool TryUse(EquipmentSlot slot, Vector2 direction)
    {
        if (!isActiveAndEnabled || !IsValidSlot(slot)) return false;
        EquippedActionBase action = GetAction(slot);
        return action != null && action.TryUse(direction);
    }
    public bool TryEquipItem(AttackPickup pickup)
    {
        if (pickup == null || pickup.ActionPrefab == null) return false;
        EquipmentSlot slot;
        switch (pickup.ActionPrefab.Kind)
        {
            case EquipmentKind.Attack: slot = EquipmentSlot.Attack; break;
            case EquipmentKind.Dash: slot = EquipmentSlot.Dash; break;
            case EquipmentKind.Ultimate: slot = EquipmentSlot.Ultimate; break;
            case EquipmentKind.Ability:
                slot = actions[(int)EquipmentSlot.Ability1] == null ? EquipmentSlot.Ability1 :
                    actions[(int)EquipmentSlot.Ability2] == null ? EquipmentSlot.Ability2 :
                    SelectedAbilitySlot == 0 ? EquipmentSlot.Ability1 : EquipmentSlot.Ability2;
                break;
            default: return false;
        }
        return Equip(pickup, slot);
    }
    public static bool Fits(EquipmentSlot slot, EquipmentKind kind)
    {
        return (slot == EquipmentSlot.Attack && kind == EquipmentKind.Attack) ||
            (slot == EquipmentSlot.Dash && kind == EquipmentKind.Dash) ||
            ((slot == EquipmentSlot.Ability1 || slot == EquipmentSlot.Ability2) &&
                kind == EquipmentKind.Ability) ||
            (slot == EquipmentSlot.Ultimate && kind == EquipmentKind.Ultimate);
    }
    private bool Equip(AttackPickup pickup, EquipmentSlot slot)
    {
        EnsureContext();
        if (!isActiveAndEnabled || context.Health.IsDead ||
            pickup.ActionPrefab == null || !Fits(slot, pickup.ActionPrefab.Kind))
            return false;
        int index = (int)slot;
        if (items[index] == pickup) return false;
        // Reject an item already stored by any player.
        if (pickup.transform.GetComponentInParent<PlayerEquipment>() != null)
            return false;
        Vector3 dropPosition = pickup.transform.position;
        EquippedActionBase next = Instantiate(pickup.ActionPrefab, transform, false);
        next.transform.localPosition = Vector3.zero;
        next.transform.localRotation = Quaternion.identity;
        next.Initialize(context, cooldowns[index]);
        EquippedActionBase previous = actions[index];
        AttackPickup oldItem = items[index];
        if (previous != null)
        {
            previous.Cancel();
            previous.gameObject.SetActive(false);
        }
        actions[index] = next;
        items[index] = pickup;
        pickup.Store(this);
        next.gameObject.SetActive(true);
        next.enabled = true;
        SlotChanged?.Invoke(slot, next);
        if (previous != null) Destroy(previous.gameObject);
        if (oldItem != null) oldItem.Drop(dropPosition, this);
        return true;
    }

    private static int GetSlotIndex(EquipmentSlot slot)
    {
        int index = (int)slot;
        if (!IsValidSlot(slot))
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown equipment slot.");
        return index;
    }

    private static bool IsValidSlot(EquipmentSlot slot) =>
        (uint)slot < SlotCount;

    private void OnDisable()
    {
        foreach (EquippedActionBase action in actions)
            if (action != null) action.Cancel();
    }
}
