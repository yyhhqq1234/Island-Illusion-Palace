using UnityEngine;

public class HealthSystem : MonoBehaviour, IHealthProvider
{
    [Header("生命值设置")]
    public float maxHealth = 100f;
    public float currentHealth;
    public System.Action onDeath;

    float IHealthProvider.currentHealth { get => currentHealth; set => currentHealth = value; }
    float IHealthProvider.maxHealth { get => maxHealth; set => maxHealth = value; }

    

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

    [Header("调试/作弊")]
    [Tooltip("无敌模式（仅测试用 — F1 切换）")]
    public bool invincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalMaterial = spriteRenderer.material;
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            invincible = !invincible;
            Debug.Log($"[HealthSystem] 无敌模式: {(invincible ? "开启" : "关闭")}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (invincible || damage <= 0) return;

        float finalDamage = damage * defenseMultiplier;
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        UpdateUI();
        ApplyHitEffect();

        if (currentHealth <= 0 && !IsDead()) Die();
    }

    /// <summary>右键菜单切换无敌</summary>
    [ContextMenu("切换无敌模式")]
    void ToggleInvincible()
    {
        invincible = !invincible;
        Debug.Log($"[HealthSystem] 无敌模式: {(invincible ? "ON" : "OFF")}");
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

        // 通过事件驱动音效播放（替代反射调用不存在的 AudioManager）
        if (CompareTag("Player"))
        {
            GlobalEventManager.Instance.RequestAudio("PlayPlayerDeath");
        }

        // 通过 IDieHandler 接口解耦（替代直接 GetComponent<EnemyAI/SummonedCreatureAI>）
        var dieHandler = GetComponent<IDieHandler>();
        if (dieHandler != null)
        {
            dieHandler.OnDie();
        }
        else
        {
            Debug.LogWarning($"[HealthSystem] {gameObject.name} 没有实现 IDieHandler，无法处理死亡逻辑");
        }

        // 广播实体死亡事件（供其他系统订阅）
        GlobalEventManager.Instance.TriggerEntityDied(gameObject);

        if (CompareTag("Player"))
        {
            // 触发玩家死亡事件（替代 BattleEventManager）
            GlobalEventManager.Instance.TriggerPlayerDeath();

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
