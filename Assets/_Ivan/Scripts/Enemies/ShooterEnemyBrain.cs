using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterEnemyBrain : CombatEnemyBase
{
    [Header("Distance")]
    [SerializeField] private float minDistance = 3f;

    [SerializeField] private float maxDistance = 6f;

    [Header("Shooting")]
    [SerializeField] private float fireCooldown = 1f;

    [SerializeField] private Transform firePoint;

    [SerializeField] private EnemyProjectile projectilePrefab;

    private ShooterApproachState approachState;
    private ShooterShootState shootState;
    private ShooterRetreatState retreatState;

    private float currentCooldown;

    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public float FireCooldown => fireCooldown;

    public Transform FirePoint => firePoint;

    public EnemyProjectile ProjectilePrefab =>
        projectilePrefab;

    public ShooterApproachState ApproachState =>
        approachState;

    public ShooterShootState ShootState =>
        shootState;

    public ShooterRetreatState RetreatState =>
        retreatState;

    public bool CanShoot => currentCooldown <= 0f;

    protected override void Start()
    {
        base.Start();

        approachState =
            new ShooterApproachState(this);

        shootState =
            new ShooterShootState(this);

        retreatState =
            new ShooterRetreatState(this);

        ChangeState(shootState);
    }

    protected override void Update()
    {
        base.Update();

        currentCooldown -= Time.deltaTime;

#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            TakeDamage(10);

            Debug.Log(
                $"Shooter HP: {HealthPercent:P0}");
        }
#endif
    }

    public void ResetShootCooldown()
    {
        currentCooldown = fireCooldown;
    }
}