using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerStats stats;
    private Vector2 direction;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        direction = Vector2.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard[Controls.MoveUp].isPressed)
            direction.y += 1f;

        if (keyboard[Controls.MoveDown].isPressed)
            direction.y -= 1f;

        if (keyboard[Controls.MoveLeft].isPressed)
            direction.x -= 1f;

        if (keyboard[Controls.MoveRight].isPressed)
            direction.x += 1f;

        direction = Vector2.ClampMagnitude(direction, 1f);
    }

    private void FixedUpdate()
    {
        body.linearVelocity = direction * stats.MoveSpeed;
    }

    private void OnDisable()
    {
        direction = Vector2.zero;

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}