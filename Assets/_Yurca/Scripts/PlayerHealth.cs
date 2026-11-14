using System;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(PlayerStats))]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerHealth")]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    private const float InvulnerabilityDuration = 1f;

    public float CurrentHealth { get; private set; }

    public float MaxHealth => stats.MaxHealth;
    public bool IsDead => CurrentHealth <= 0f;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    private PlayerStats stats;
    private float invulnerableUntil;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        CurrentHealth = MaxHealth;
    }

    public void GrantInvulnerability(float duration)
    {
        if (IsDead || duration <= 0f) return;
        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || IsInvulnerable || damage <= 0f)
            return;

        // Сначала включаем защиту, затем уведомляем подписчиков.
        invulnerableUntil = Time.time + InvulnerabilityDuration;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        Debug.Log(
            $"Игрок: {CurrentHealth}/{MaxHealth} HP",
            this
        );

        if (IsDead)
        {
            Debug.Log("Игрок погиб.", this);
            Died?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        float newHealth = Mathf.Min(
            CurrentHealth + amount,
            MaxHealth
        );

        if (Mathf.Approximately(newHealth, CurrentHealth))
            return;

        CurrentHealth = newHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}

