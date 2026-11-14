using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerMovement")]
public class PlayerMovement : MonoBehaviour
{
    public Vector2 InputDirection { get; private set; }
    public Vector2 LastDirection { get; private set; } = Vector2.down;
    public bool IsDashing => Time.time < dashUntil;
    private Rigidbody2D body;
    private PlayerStats stats;
    private PlayerHealth health;
    private Vector2 dashVelocity;
    private float dashUntil;
    private object dashOwner;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        health = GetComponent<PlayerHealth>();
    }
    private void Update()
    {
        InputDirection = Vector2.zero;
        if (health == null) health = GetComponent<PlayerHealth>();
        if (health != null && health.IsDead) return;
        Keyboard keys = Keyboard.current;
        if (keys == null) return;
        Vector2 direction = Vector2.zero;
        if (keys[Controls.MoveUp].isPressed) direction.y++;
        if (keys[Controls.MoveDown].isPressed) direction.y--;
        if (keys[Controls.MoveLeft].isPressed) direction.x--;
        if (keys[Controls.MoveRight].isPressed) direction.x++;
        InputDirection = Vector2.ClampMagnitude(direction, 1f);
        if (InputDirection.sqrMagnitude > 0.001f) LastDirection = InputDirection.normalized;
    }
    private void FixedUpdate()
    {
        if (dashOwner != null && !IsDashing)
        {
            dashOwner = null;
            dashVelocity = Vector2.zero;
        }

        body.linearVelocity = health != null && health.IsDead ? Vector2.zero :
            IsDashing ? dashVelocity : InputDirection * stats.MoveSpeed;
    }
    public bool BeginDash(object owner, Vector2 direction, float distance, float duration)
    {
        if (IsDashing || (health != null && health.IsDead) ||
            direction.sqrMagnitude < 0.0001f) return false;
        duration = Mathf.Max(0.01f, duration);
        dashOwner = owner;
        dashVelocity = direction.normalized * Mathf.Max(0f, distance) / duration;
        dashUntil = Time.time + duration;
        return true;
    }
    public void CancelDash(object owner)
    {
        if (!ReferenceEquals(dashOwner, owner)) return;
        bool wasDashing = IsDashing;
        dashUntil = 0f;
        dashOwner = null;
        dashVelocity = Vector2.zero;
        if (wasDashing && body != null) body.linearVelocity = Vector2.zero;
    }
    private void OnDisable()
    {
        InputDirection = Vector2.zero;
        dashUntil = 0f;
        dashOwner = null;
        dashVelocity = Vector2.zero;
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}