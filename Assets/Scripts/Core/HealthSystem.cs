using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("生命值设置")]
    public float maxHealth = 100f;
    public float currentHealth;
    public System.Action onDeath;

    

    [Header("UI引用")]
    public UnityEngine.UI.Slider healthSlider;

    [Header("受击效果")]
    public Material hitMaterial;
    [Tooltip("受击效果持续时间（秒）")]
    public float hitEffectDuration = 0.2f;
    private Material originalMaterial;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    [Header("防御系统")]
    private float defenseMultiplier = 1f;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalMaterial = spriteRenderer.material;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        // 确保伤害不为负数
        if (damage <= 0) return;
        
        float finalDamage = damage * defenseMultiplier;
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        UpdateUI();
        ApplyHitEffect();

        if (currentHealth <= 0 && !IsDead()) Die();
    }

    void ApplyHitEffect()
    {
        if (spriteRenderer != null && hitMaterial != null)
        {
            spriteRenderer.material = hitMaterial;
            Invoke(nameof(EndHitEffect), hitEffectDuration);
        }
    }

    void EndHitEffect()
    {
        if (spriteRenderer != null && originalMaterial != null)
            spriteRenderer.material = originalMaterial;
    }

    public void Heal(float amount)
    {
        // 确保治疗量不为负数
        if (amount <= 0) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }



    public void SetDefenseMultiplier(float multiplier)
    {
        defenseMultiplier = Mathf.Max(0.1f, multiplier);
    }

    void UpdateUI()
    {
        // 更新生命值UI
        if (healthSlider != null)
        {
            float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            healthSlider.value = healthPercentage;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log(gameObject.name + " 死亡！");
        onDeath?.Invoke();

        if (CompareTag("Player"))
        {
            try
            {
                var audioManagerType = System.Reflection.Assembly.GetExecutingAssembly().GetType("AudioManager");
                var instance = audioManagerType?.GetProperty("Instance")?.GetValue(null);
                audioManagerType?.GetMethod("PlayPlayerDeath")?.Invoke(instance, null);
            }
            catch { }
        }

        // 调用EnemyAI的Die方法
        var enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.Die();
        }
        
        // 调用SummonedCreatureAI的Die方法
        var summonAI = GetComponent<SummonedCreatureAI>();
        if (summonAI != null)
        {
            summonAI.Die();
        }
        
        if (CompareTag("Player"))
        {
            // 触发玩家死亡事件
            if (BattleEventManager.Instance != null)
            {
                BattleEventManager.Instance.TriggerPlayerDeath();
            }
            
            // 死亡时重置负担
            var burdenSystem = GetComponent<BurdenSystem>();
            if (burdenSystem != null)
            {
                burdenSystem.ResetBurden();
            }
        }
    }

    public float GetHealthPercent() => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead() => isDead;
    public void TriggerHitEffect() => ApplyHitEffect();
    public float GetCurrentHealth() => currentHealth;
}
