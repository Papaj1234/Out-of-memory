
using UnityEngine;

public class EnemyVisualAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D body;

    [Header("Settings")]
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private bool spriteFacesRight = true;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private bool attackFacingLocked;
    private bool attackFacesLeft;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (body == null)
            body = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (animator == null || body == null)
            return;

        Vector2 velocity = body.linearVelocity;

        bool isMoving =
            velocity.sqrMagnitude >
            movementThreshold * movementThreshold;

        animator.SetBool(IsMovingHash, isMoving);

        if (!attackFacingLocked &&
            spriteRenderer != null &&
            Mathf.Abs(velocity.x) > movementThreshold)
        {
            SetFacing(velocity.x);
        }
    }

    public void PlayAttack(Vector2 direction)
    {
        if (animator == null)
            return;

        attackFacingLocked = true;
        attackFacesLeft = direction.x < 0f;

        if (spriteRenderer != null)
            spriteRenderer.flipX =
                spriteFacesRight
                    ? attackFacesLeft
                    : !attackFacesLeft;

        animator.SetTrigger(AttackHash);
    }

    public void EndAttackAnimation()
    {
        attackFacingLocked = false;
    }

    private void SetFacing(float directionX)
    {
        spriteRenderer.flipX =
            spriteFacesRight
                ? directionX < 0f
                : directionX > 0f;
    }

    private void OnDisable()
    {
        attackFacingLocked = false;
    }
}
