using UnityEngine;

public class BurdenSystem : MonoBehaviour, IBurdenProvider
{
    [Header("负担基本设置")]
    public float maxBurden = 100f;
    public float currentBurden = 0f;
    public float highBurdenThreshold = 70f;
    public float criticalBurdenThreshold = 90f;

    [Header("负担增长设置")]
    public float summonBurdenCost = 5f;           // 召唤消耗
    public float skillBurdenCost = 3f;            // 技能消耗
    public float soulCoreBurdenPerLevel = 0.5f;    // 每个灵魂之核等级的基础负担

    [Header("负担恢复设置")]
    public float naturalRecoveryRate = 0.1f;       // 自然恢复速率
    public float campfireRecoveryRate = 5f;        // 营火恢复速率
    public float itemRecoveryMultiplier = 1f;      // 物品恢复倍率

    [Header("负担效果设置")]
    public float highBurdenDamagePenalty = 0.8f;    // 高负担伤害惩罚
    public float highBurdenDefensePenalty = 0.8f;   // 高负担防御惩罚
    public float highBurdenSpeedPenalty = 0.9f;     // 高负担速度惩罚
    public float criticalBurdenDamagePenalty = 0.6f; // 临界负担伤害惩罚
    public float criticalBurdenDefensePenalty = 0.6f; // 临界负担防御惩罚
    public float criticalBurdenSpeedPenalty = 0.7f;  // 临界负担速度惩罚

    [Header("系统引用")]
    private CharacterStats characterStats;
    private HealthSystem healthSystem;
    private WeaponSystem weaponSystem;
    private PlayerController playerController;

    [Header("状态")]
    private bool isHighBurden = false;
    private bool isCriticalBurden = false;
    private bool isInCampfire = false;

    float IBurdenProvider.currentBurden { get => currentBurden; set => currentBurden = value; }
    float IBurdenProvider.maxBurden => maxBurden;

    void Start()
    {
        InitializeReferences();
        UpdateBurdenStatus();
    }

    void InitializeReferences()
    {
        characterStats = GetComponent<CharacterStats>();
        healthSystem = GetComponent<HealthSystem>();
        weaponSystem = GetComponent<WeaponSystem>();
        playerController = GetComponent<PlayerController>();
        
        // 验证关键组件
        if (characterStats == null) Debug.LogWarning("BurdenSystem: CharacterStats component not found!");
        if (healthSystem == null) Debug.LogWarning("BurdenSystem: HealthSystem component not found!");
        if (weaponSystem == null) Debug.LogWarning("BurdenSystem: WeaponSystem component not found!");
        if (playerController == null) Debug.LogWarning("BurdenSystem: PlayerController component not found!");
    }

    void Update()
    {
        if (maxBurden <= 0) return;
        
        UpdateNaturalRecovery();
        if (isInCampfire) UpdateCampfireRecovery();
        UpdateBurdenStatus();
        ApplyBurdenEffects();
    }

    public void AddBurden(float amount)
    {
        if (amount <= 0) return;
        
        // 应用负担抗性
        float effectiveAmount = amount * (1 - GetEffectiveBurdenResistance());
        currentBurden = Mathf.Min(maxBurden, currentBurden + effectiveAmount);
        UpdateBurdenUI();
    }

    public void RemoveBurden(float amount)
    {
        if (amount <= 0) return;
        
        float effectiveAmount = amount * itemRecoveryMultiplier;
        currentBurden = Mathf.Max(0, currentBurden - effectiveAmount);
        UpdateBurdenUI();
    }

    public void SetBurden(float amount)
    {
        currentBurden = Mathf.Clamp(amount, 0, maxBurden);
        UpdateBurdenUI();
    }

    public void ResetBurden()
    {
        currentBurden = 0f;
        UpdateBurdenUI();
    }
    
    public void ChangeBurden(float amount)
    {
        if (amount > 0)
        {
            AddBurden(amount);
        }
        else if (amount < 0)
        {
            RemoveBurden(Mathf.Abs(amount));
        }
    }
    
    public float GetCurrentBurden()
    {
        return currentBurden;
    }
    
    public float GetMaxBurden()
    {
        return maxBurden;
    }

    void UpdateNaturalRecovery()
    {
        if (currentBurden > 0 && !isInCampfire)
        {
            RemoveBurden(naturalRecoveryRate * Time.deltaTime);
        }
    }

    void UpdateCampfireRecovery()
    {
        if (currentBurden > 0)
        {
            RemoveBurden(campfireRecoveryRate * Time.deltaTime);
        }
    }

    void UpdateBurdenStatus()
    {
        bool wasHighBurden = isHighBurden;
        bool wasCriticalBurden = isCriticalBurden;

        isHighBurden = currentBurden >= highBurdenThreshold;
        isCriticalBurden = currentBurden >= criticalBurdenThreshold;

        // 状态变化时触发效果
        if (isHighBurden != wasHighBurden)
        {
            OnBurdenStatusChange();
        }

        if (isCriticalBurden != wasCriticalBurden)
        {
            OnCriticalBurdenStatusChange();
        }

        // 广播负担等级变化事件
        BurdenLevel newLevel = GetCurrentBurdenLevel();
        GlobalEventManager.Instance.TriggerBurdenLevelChanged(newLevel);
    }

    public BurdenLevel GetCurrentBurdenLevel()
    {
        if (currentBurden >= criticalBurdenThreshold) return BurdenLevel.Critical;
        if (currentBurden >= highBurdenThreshold) return BurdenLevel.High;
        return BurdenLevel.Normal;
    }

    void ApplyBurdenEffects()
    {
        // 应用负担对属性的影响
        if (weaponSystem != null)
        {
            float damageMultiplier = 1f;
            if (isCriticalBurden)
            {
                damageMultiplier = criticalBurdenDamagePenalty;
            }
            else if (isHighBurden)
            {
                damageMultiplier = highBurdenDamagePenalty;
            }
            weaponSystem.UpdateBurdenDamageMultiplier(damageMultiplier);
        }

        if (healthSystem != null)
        {
            float defenseMultiplier = 1f;
            if (isCriticalBurden)
            {
                defenseMultiplier = criticalBurdenDefensePenalty;
            }
            else if (isHighBurden)
            {
                defenseMultiplier = highBurdenDefensePenalty;
            }
            healthSystem.SetDefenseMultiplier(defenseMultiplier);
        }

        if (playerController != null)
        {
            // 这里需要在PlayerController中添加速度调整方法
            // playerController.UpdateSpeedMultiplier(speedMultiplier);
        }
    }

    void OnBurdenStatusChange()
    {
        if (isHighBurden)
        {
            Debug.Log("高负担状态：属性降低，行动速度减慢！");
            // 可以添加视觉效果或音效
        }
        else
        {
            Debug.Log("负担恢复：属性恢复正常！");
        }
    }

    void OnCriticalBurdenStatusChange()
    {
        if (isCriticalBurden)
        {
            Debug.Log("临界负担状态：属性大幅降低，生命恢复停止！");
            // 可以添加更明显的视觉效果或音效
        }
    }

    float GetEffectiveBurdenResistance()
    {
        if (characterStats != null)
        {
            return characterStats.GetEffectiveBurdenResistance();
        }
        return 0f;
    }

    public float GetBurdenPercent()
    {
        return currentBurden / maxBurden;
    }

    public bool IsHighBurden()
    {
        return isHighBurden;
    }

    public bool IsCriticalBurden()
    {
        return isCriticalBurden;
    }

    public void SetCampfireState(bool inCampfire)
    {
        isInCampfire = inCampfire;
    }

    public float GetSummonBurdenCost()
    {
        return summonBurdenCost;
    }

    public float GetSkillBurdenCost()
    {
        return skillBurdenCost;
    }

    public float CalculateSoulCoreBurden(int level, float qualityMultiplier)
    {
        return soulCoreBurdenPerLevel * level * qualityMultiplier;
    }

    void UpdateBurdenUI()
    {
        // 更新UI显示
        if (healthSystem != null)
        {
            // 这里需要在HealthSystem中添加更新负担UI的方法
            // healthSystem.UpdateBurdenUI(currentBurden, maxBurden);
        }
    }

    // 调试方法
    public void DebugPrintBurdenStatus()
    {
        Debug.Log($"当前负担: {currentBurden}/{maxBurden} ({GetBurdenPercent() * 100}%)");
        Debug.Log($"高负担状态: {isHighBurden}, 临界负担状态: {isCriticalBurden}");
        Debug.Log($"负担抗性: {GetEffectiveBurdenResistance() * 100}%");
    }

    // 获取负担抗性（供其他系统使用）
    public float GetBurdenResistance()
    {
        return GetEffectiveBurdenResistance();
    }

    // 获取伤害倍率（供其他系统使用）
    public float GetDamageMultiplier()
    {
        if (isCriticalBurden)
        {
            return criticalBurdenDamagePenalty;
        }
        else if (isHighBurden)
        {
            return highBurdenDamagePenalty;
        }
        return 1f;
    }

    // 获取防御倍率（供其他系统使用）
    public float GetDefenseMultiplier()
    {
        if (isCriticalBurden)
        {
            return criticalBurdenDefensePenalty;
        }
        else if (isHighBurden)
        {
            return highBurdenDefensePenalty;
        }
        return 1f;
    }
}
