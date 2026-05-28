using UnityEngine;

public interface IHealthProvider
{
    float currentHealth { get; set; }
    float maxHealth { get; set; }
    void TakeDamage(float amount);
    void Heal(float amount);
}
