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
    private AttackPickup currentItem;

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

        Vector3 dropPosition = pickup.transform.position;

        AttackBase newAttack = Instantiate(
            pickup.AttackPrefab,
            transform,
            false
        );

        newAttack.Initialize(stats, attackOrigin);

        AttackBase previousAttack = currentAttack;
        AttackPickup previousItem = currentItem;

        if (previousAttack != null)
        {
            previousAttack.CancelAttack();
            previousAttack.gameObject.SetActive(false);
        }

        currentAttack = newAttack;
        currentItem = pickup;

        // Храним подобранный предмет внутри героя выключенным.
        currentItem.transform.SetParent(transform, true);
        currentItem.gameObject.SetActive(false);

        currentAttack.gameObject.SetActive(true);
        currentAttack.enabled = true;

        AttackChanged?.Invoke(currentAttack);

        if (previousAttack != null)
            Destroy(previousAttack.gameObject);

        // Возвращаем сам старый предмет, а не создаём
        // копию по ссылке на уничтоженный объект.
        if (previousItem != null)
            previousItem.Drop(dropPosition, this);

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