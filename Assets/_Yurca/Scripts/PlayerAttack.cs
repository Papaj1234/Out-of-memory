using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting.APIUpdating;
[RequireComponent(typeof(PlayerEquipment))]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerAttack")]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    public AttackBase CurrentAttack => equipment == null ? null :
        equipment.GetAction(EquipmentSlot.Attack) as AttackBase;
    public event Action<AttackBase> AttackChanged;
    private PlayerEquipment equipment;
    private PlayerMovement movement;

    private void Awake()
    {
        equipment = GetComponent<PlayerEquipment>();
        movement = GetComponent<PlayerMovement>();
        if (aimCamera == null) aimCamera = Camera.main;
    }
    private void OnEnable() { equipment.SlotChanged += OnSlotChanged; }
    private void OnDisable() { equipment.SlotChanged -= OnSlotChanged; }
    private void OnSlotChanged(EquipmentSlot slot, EquippedActionBase action)
    {
        if (slot == EquipmentSlot.Attack) AttackChanged?.Invoke(action as AttackBase);
    }
    public void SetAimCamera(Camera camera) { aimCamera = camera; }

    private void Update()
    {
        Keyboard keys = Keyboard.current;
        if (keys != null)
        {
            if (keys[Controls.SelectAbilitySlot1].wasPressedThisFrame) equipment.SelectAbilitySlot(0);
            if (keys[Controls.SelectAbilitySlot2].wasPressedThisFrame) equipment.SelectAbilitySlot(1);
        }
        Vector2 aim = AimDirection();
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse[Controls.Attack] is ButtonControl button &&
            button.wasPressedThisFrame) equipment.TryUse(EquipmentSlot.Attack, aim);
        if (keys == null) return;
        if (keys[Controls.Dash].wasPressedThisFrame)
        {
            Vector2 direction = movement != null && movement.InputDirection.sqrMagnitude > 0.001f
                ? movement.InputDirection : aim;
            equipment.TryUse(EquipmentSlot.Dash, direction);
        }
        if (keys[Controls.Ability1].wasPressedThisFrame) equipment.TryUse(EquipmentSlot.Ability1, aim);
        if (keys[Controls.Ability2].wasPressedThisFrame) equipment.TryUse(EquipmentSlot.Ability2, aim);
        if (keys[Controls.Ultimate].wasPressedThisFrame) equipment.TryUse(EquipmentSlot.Ultimate, aim);
    }
    private Vector2 AimDirection()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && aimCamera != null)
        {
            Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
            Plane plane = new Plane(Vector3.forward, equipment.Origin.position);
            if (plane.Raycast(ray, out float distance))
            {
                Vector2 direction = (Vector2)ray.GetPoint(distance) -
                    (Vector2)equipment.Origin.position;
                if (direction.sqrMagnitude > 0.0001f) return direction.normalized;
            }
        }
        return movement != null ? movement.LastDirection : Vector2.down;
    }
}
