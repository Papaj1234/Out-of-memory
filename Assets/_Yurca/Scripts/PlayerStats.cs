using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float attackRange = 2f;
    [SerializeField, Range(1f, 360f)] private float attackAngle = 120f;
    [SerializeField, Min(0.01f)] private float attackDuration = 0.2f;
    [SerializeField, Min(0f)] private float attackCooldown = 0.4f;
        [SerializeField, Min(0f)] private float damage = 10f;
    
    public float AttackRange => attackRange;
    public float AttackAngle => attackAngle;
    public float AttackDuration => attackDuration;
    public float AttackCooldown => attackCooldown;
    public float MoveSpeed => moveSpeed;
    public float Damage => damage;
}