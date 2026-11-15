using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAttack))]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerAnimation")]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Через сколько секунд после остановки/удара повернуться лицом к камере")]
    [SerializeField] private float faceCameraDelay = 0f;

    private Rigidbody2D body;
    private PlayerAttack playerAttack;
    private AttackBase subscribedAttack;

    private Vector2 lastDirection = Vector2.down;
    private float idleTime;

    [SerializeField] private float attackAnimationDuration = 0.5f;

    private bool isAttackFacingLocked;
    private Vector2 attackDirection;
    private float attackFacingEndTime;

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

        // Проверяем, закончилась ли анимация атаки
        if (isAttackFacingLocked &&
            Time.time >= attackFacingEndTime)
        {
            isAttackFacingLocked = false;
        }

        // Движение обновляет направление только вне атаки
        if (!isAttackFacingLocked)
        {
            if (isMoving)
            {
                lastDirection = velocity.normalized;
                idleTime = 0f;
            }
            else
            {
                idleTime += Time.deltaTime;

                if (faceCameraDelay >= 0f &&
                    idleTime >= faceCameraDelay)
                {
                    lastDirection = Vector2.down;
                }
            }
        }

        // Во время атаки используем направление удара,
        // в остальное время — направление движения
        Vector2 facingDirection =
            isAttackFacingLocked
                ? attackDirection
                : lastDirection;

        bool isHorizontal =
            Mathf.Abs(facingDirection.x) >
            Mathf.Abs(facingDirection.y);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX =
                isHorizontal && facingDirection.x < 0f;
        }

        animator.SetBool(IsMovingHash, isMoving);

        animator.SetFloat(
            MoveXHash,
            facingDirection.x
        );

        animator.SetFloat(
            MoveYHash,
            facingDirection.y
        );
    }



    private void PlayAttack(Vector2 direction)
    {
        if (animator == null)
            return;

        if (direction.sqrMagnitude < 0.001f)
            direction = lastDirection;

        attackDirection = direction.normalized;

        isAttackFacingLocked = true;
        attackFacingEndTime =
            Time.time + attackAnimationDuration;

        idleTime = 0f;

        animator.SetFloat(AttackXHash, attackDirection.x);
        animator.SetFloat(AttackYHash, attackDirection.y);
        animator.SetTrigger(AttackHash);

        // Сразу поворачиваем персонажа к атаке
        if (spriteRenderer != null)
        {
            bool isHorizontal =
                Mathf.Abs(attackDirection.x) >
                Mathf.Abs(attackDirection.y);

            spriteRenderer.flipX =
                isHorizontal && attackDirection.x < 0f;
        }
    }


    private void ResetAttackTrigger()
    {
        isAttackFacingLocked = false;

        if (animator != null)
            animator.ResetTrigger(AttackHash);
    }
}