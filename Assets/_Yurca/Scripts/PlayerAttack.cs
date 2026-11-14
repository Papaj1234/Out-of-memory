using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerStats))]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform attackOrigin;

    public AttackBase CurrentAttack => currentAttack;

    public event Action<AttackBase> AttackChanged;

    private PlayerStats stats;
    private AttackBase currentAttack;

    // Префаб предмета, соответствующего текущей атаке.
    private AttackPickup currentItemPrefab;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (attackOrigin == null)
            attackOrigin = transform;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || aimCamera == null || currentAttack == null)
            return;

        if (mouse[Controls.Attack] is not ButtonControl button ||
            !button.wasPressedThisFrame)
        {
            return;
        }

        Ray ray = aimCamera.ScreenPointToRay(
            mouse.position.ReadValue()
        );

        Plane plane = new Plane(
            Vector3.forward,
            attackOrigin.position
        );

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector2 cursorPosition = ray.GetPoint(distance);

        Vector2 direction =
            cursorPosition - (Vector2)attackOrigin.position;

        currentAttack.TryAttack(direction);
    }

    public bool TryEquipItem(AttackPickup pickup)
    {
        if (pickup == null || pickup.AttackPrefab == null)
            return false;

        // Получаем исходный префаб этого предмета.
        AttackItemReference itemReference =
            pickup.GetComponent<AttackItemReference>();

        if (itemReference == null || itemReference.WorldPrefab == null)
        {
            Debug.LogWarning(
                "У предмета не назначен World Prefab.",
                pickup
            );

            return false;
        }

        // Создаём только логику новой атаки.
        AttackBase newAttack = Instantiate(
            pickup.AttackPrefab,
            transform,
            false
        );

        newAttack.Initialize(stats, attackOrigin);

        AttackBase previousAttack = currentAttack;
        AttackPickup previousItemPrefab = currentItemPrefab;

        if (previousAttack != null)
        {
            previousAttack.CancelAttack();
            previousAttack.gameObject.SetActive(false);
        }

        currentAttack = newAttack;
        currentItemPrefab = itemReference.WorldPrefab;

        currentAttack.gameObject.SetActive(true);
        currentAttack.enabled = true;

        // PlayerAnimation переподписывается на новую атаку.
        AttackChanged?.Invoke(currentAttack);

        if (previousAttack != null)
            Destroy(previousAttack.gameObject);

        if (previousItemPrefab != null)
        {
            // Оставляем старый предмет там,
            // где лежал только что подобранный.
            AttackPickup droppedItem = Instantiate(
                previousItemPrefab,
                pickup.transform.position,
                Quaternion.identity
            );

            droppedItem.IgnoreUntilPlayerLeaves(this);
        }

        return true;
    }

    public void SetAimCamera(Camera roomCamera)
    {
        aimCamera = roomCamera;
    }

    private void OnDisable()
    {
        if (currentAttack != null)
            currentAttack.CancelAttack();
    }
}