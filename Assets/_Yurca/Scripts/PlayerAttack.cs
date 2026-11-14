using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerStats))]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private AttackBase startingAttack;

    public AttackBase CurrentAttack => currentAttack;

    public event Action<AttackBase> AttackChanged;

    private PlayerStats stats;
    private AttackBase currentAttack;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (attackOrigin == null)
            attackOrigin = transform;
    }

    private void Start()
    {
        if (currentAttack == null && startingAttack != null)
            SetAttack(startingAttack);
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

    public void SetAttack(AttackBase newAttack)
    {
        if (newAttack == null || newAttack == currentAttack)
            return;

        if (currentAttack != null)
        {
            currentAttack.CancelAttack();
            currentAttack.enabled = false;
        }

        currentAttack = newAttack;
        currentAttack.Initialize(stats, attackOrigin);
        currentAttack.enabled = true;

        AttackChanged?.Invoke(currentAttack);
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