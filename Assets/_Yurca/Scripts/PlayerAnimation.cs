using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAttack))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Rigidbody2D body;
    private PlayerAttack playerAttack;
    private AttackBase subscribedAttack;

    private Vector2 lastDirection = Vector2.down;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int MoveXHash =
        Animator.StringToHash("MoveX");

    private static readonly int MoveYHash =
        Animator.StringToHash("MoveY");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int AttackXHash =
        Animator.StringToHash("AttackX");

    private static readonly int AttackYHash =
        Animator.StringToHash("AttackY");

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        playerAttack.AttackChanged += SubscribeToAttack;
        SubscribeToAttack(playerAttack.CurrentAttack);
    }

    private void OnDisable()
    {
        playerAttack.AttackChanged -= SubscribeToAttack;

        SubscribeToAttack(null);
        ResetAttackTrigger();
    }

    private void SubscribeToAttack(AttackBase attack)
    {
        if (subscribedAttack != null)
        {
            subscribedAttack.AttackStarted -= PlayAttack;
            subscribedAttack.AttackCancelled -= ResetAttackTrigger;
        }

        subscribedAttack = attack;

        if (subscribedAttack != null)
        {
            subscribedAttack.AttackStarted += PlayAttack;
            subscribedAttack.AttackCancelled += ResetAttackTrigger;
        }
    }

    private void LateUpdate()
    {
        if (animator == null)
            return;

        Vector2 velocity = body.linearVelocity;
        bool isMoving = velocity.sqrMagnitude > 0.01f;

        if (isMoving)
            lastDirection = velocity.normalized;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetFloat(MoveXHash, lastDirection.x);
        animator.SetFloat(MoveYHash, lastDirection.y);
    }

    private void PlayAttack(Vector2 direction)
    {
        if (animator == null)
            return;

        lastDirection = direction;

        animator.SetFloat(AttackXHash, direction.x);
        animator.SetFloat(AttackYHash, direction.y);
        animator.SetTrigger(AttackHash);
    }

    private void ResetAttackTrigger()
    {
        if (animator != null)
            animator.ResetTrigger(AttackHash);
    }
}