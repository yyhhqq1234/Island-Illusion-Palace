using UnityEngine;

public interface IHealthProvider
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }
    void TakeDamage(float amount, GameObject source = null);
    void Heal(float amount);
    void SetMaxHealth(float maxHealth);
    void OnDeath();
}
