using UnityEngine;

public class ManaSystem : MonoBehaviour
{
    [Header("魔法值设置")]
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 1f;
    public float manaRegenDelay = 2f;

    [Header("引用")]
    public CharacterStats characterStats;

    [Header("状态")]
    private float manaRegenTimer = 0f;
    private bool isRegenerating = true;

    void Start()
    {
        InitializeReferences();
        ResetMana();
    }

    void Update()
    {
        UpdateManaRegen();
        UpdateMaxMana();
    }

    void InitializeReferences()
    {
        characterStats = GetComponentInParent<CharacterStats>();
        
        // 验证关键组件
        if (characterStats == null)
        {
            Debug.LogWarning("ManaSystem: CharacterStats component not found!");
        }
    }

    void UpdateManaRegen()
    {
        if (maxMana <= 0) return;
        
        if (isRegenerating && currentMana < maxMana)
        {
            manaRegenTimer += Time.deltaTime;
            if (manaRegenTimer >= manaRegenDelay)
            {
                RegenerateMana();
            }
        }
    }

    void RegenerateMana()
    {
        if (maxMana <= 0) return;
        
        float regenAmount = manaRegenRate * Time.deltaTime;
        // 根据智慧属性增加回蓝速度
        if (characterStats != null)
        {
            regenAmount *= (1f + characterStats.intelligence * 0.01f);
        }
        currentMana = Mathf.Min(currentMana + regenAmount, maxMana);
    }

    void UpdateMaxMana()
    {
        if (characterStats != null)
        {
            float baseMana = 100f;
            float intelligenceBonus = characterStats.intelligence * 5f;
            maxMana = baseMana + intelligenceBonus;
        }
    }

    public bool UseMana(float amount)
    {
        if (amount <= 0 || maxMana <= 0) return false;
        
        if (currentMana >= amount)
        {
            currentMana -= amount;
            ResetRegenTimer();
            return true;
        }
        return false;
    }

    public void AddMana(float amount)
    {
        if (amount <= 0 || maxMana <= 0) return;
        
        currentMana = Mathf.Min(currentMana + amount, maxMana);
    }

    public void ResetMana()
    {
        currentMana = maxMana;
    }

    public void SetMana(float amount)
    {
        currentMana = Mathf.Clamp(amount, 0f, maxMana);
    }

    public void SetMaxMana(float amount)
    {
        maxMana = Mathf.Max(1f, amount);
        currentMana = Mathf.Min(currentMana, maxMana);
    }

    public void ResetRegenTimer()
    {
        manaRegenTimer = 0f;
    }

    public void SetRegenerating(bool regenerating)
    {
        isRegenerating = regenerating;
    }

    public float GetCurrentMana()
    {
        return currentMana;
    }

    public float GetMaxMana()
    {
        return maxMana;
    }

    public float GetManaPercentage()
    {
        return currentMana / maxMana;
    }

    public bool HasMana(float amount)
    {
        return currentMana >= amount;
    }

    public void DrainAllMana()
    {
        currentMana = 0f;
        ResetRegenTimer();
    }

    // 调试方法
    public void DebugPrintManaStatus()
    {
        Debug.Log("=== 魔法值状态 ===");
        Debug.Log($"当前魔法值：{currentMana:F1}/{maxMana:F1}");
        Debug.Log($"回蓝速度：{manaRegenRate:F2}/秒");
        Debug.Log($"回蓝延迟：{manaRegenDelay:F1}秒");
        Debug.Log($"是否正在回蓝：{isRegenerating}");
    }
}
