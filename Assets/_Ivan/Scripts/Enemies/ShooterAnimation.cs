
using UnityEngine;

public class ShooterAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D body;

    [Header("Settings")]
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField] private float movementThreshold = 0.1f;

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private bool attackFacingLocked;
    private float attackEndTime;

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

        if (attackFacingLocked)
        {
            if (Time.time < attackEndTime)
                return;

            attackFacingLocked = false;
        }

        // В обычном состоянии смотрим по направлению движения.
        if (Mathf.Abs(velocity.x) > movementThreshold)
            SetFacing(velocity.x);
    }

    public void PlayShoot(Vector2 direction)
    {
        if (animator == null)
            return;

        if (direction.sqrMagnitude > 0.001f)
            SetFacing(direction.x);

        // Длительность клипа определяем из состояния Animator.
        // В Inspector для RangedAttack оставь Speed = 1.
        float duration = GetAttackDuration();

        attackFacingLocked = true;
        attackEndTime = Time.time + duration;

        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);
    }

    private void SetFacing(float directionX)
    {
        if (spriteRenderer == null ||
            Mathf.Abs(directionX) < 0.001f)
            return;

        bool facesLeft = directionX < 0f;

        spriteRenderer.flipX =
            spriteFacesRight ? facesLeft : !facesLeft;
    }

    private float GetAttackDuration()
    {
        if (animator.runtimeAnimatorController == null)
            return 0.5f;

        foreach (AnimationClip clip in
                 animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "RangedAttack")
                return Mathf.Max(0.01f, clip.length);
        }

        return 0.5f;
    }
}
