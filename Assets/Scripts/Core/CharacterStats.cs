using UnityEngine;

public class CharacterStats : MonoBehaviour, IStatProvider
{
    [Header("基础属性")]
    public int strength = 10;      // 力量：物理伤害、负重
    public int vitality = 10;       // 体质：生命值、负担抗性
    public int intelligence = 10;   // 智慧：灵魂控制、炼金、魔法
    public int resonance = 10;      // 共鸣：碎片发现率、特殊事件
    
    // 兼容其他文件的引用
    public int wisdom { get { return intelligence; } set { intelligence = value; } }

    [Header("成长属性")]
    public int freeAttributePoints = 0;  // 自由属性点
    public int level = 1;                // 角色等级
    public int experience = 0;           // 当前经验
    public int experienceToNextLevel = 100;  // 升级所需经验
    
    // 兼容其他文件的引用
    public int currentExperience { get { return experience; } set { experience = value; } }
    public int requiredExperience { get { return experienceToNextLevel; } set { experienceToNextLevel = value; } }

    [Header("衍生属性")]
    public float physicalDamageBonus = 0f;    // 物理伤害加成
    public float magicalDamageBonus = 0f;     // 魔法伤害加成
    public float healthBonus = 0f;            // 生命值加成
    public float manaBonus = 0f;              // 魔法值加成
    public float burdenResistance = 0f;       // 负担抗性
    public float alchemySuccessRateBonus = 0f; // 炼金成功率加成
    public float fragmentDiscoveryRate = 0f;  // 碎片发现率
    public float memoryFragmentEffectBonus = 0f; // 记忆碎片效果加成

    [Header("系统引用")]
    private HealthSystem healthSystem;
    private WeaponSystem weaponSystem;
    private AlchemySystem alchemySystem;

    void Start()
    {
        InitializeReferences();
        RecalculateDerivedStats();
        ApplyStatEffects();
        BroadcastLevel();
    }

    /// <summary>广播等级/经验变化（仅玩家广播给 HUD）</summary>
    void BroadcastLevel()
    {
        if (!CompareTag("Player")) return;
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.TriggerPlayerLevelChanged(level, experience, experienceToNextLevel);
    }

    void InitializeReferences()
    {
        // 安全地获取组件引用
        healthSystem = GetComponent<HealthSystem>();
        weaponSystem = GetComponent<WeaponSystem>();
        alchemySystem = GetComponent<AlchemySystem>();
        
        // 可选：添加调试信息，帮助定位问题
        if (healthSystem == null) Debug.LogWarning("HealthSystem not found on " + gameObject.name);
        if (weaponSystem == null) Debug.LogWarning("WeaponSystem not found on " + gameObject.name);
        if (alchemySystem == null) Debug.LogWarning("AlchemySystem not found on " + gameObject.name);
    }

    public void AddExperience(int amount)
    {
        // 确保经验值不为负数
        if (amount <= 0) return;
        
        // 防止经验值溢出
        const int maxExperience = 999999999;
        if (experience >= maxExperience) return;
        
        experience = Mathf.Min(experience + amount, maxExperience);
        CheckLevelUp();
        BroadcastLevel(); // 经验变化后广播给 HUD
    }

    void CheckLevelUp()
    {
        // 防止无限循环，最多升级100次
        int maxLevelUps = 100;
        int levelUpCount = 0;
        
        while (experience >= experienceToNextLevel && levelUpCount < maxLevelUps)
        {
            LevelUp();
            levelUpCount++;
        }
        
        // 如果达到最大升级次数，可能是经验值计算有问题
        if (levelUpCount >= maxLevelUps)
        {
            Debug.LogWarning("Reached maximum level up limit. Experience calculation may be incorrect.");
        }
    }

    void LevelUp()
    {
        // 防止等级溢出
        const int maxLevel = 999;
        if (level >= maxLevel) return;
        
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel = CalculateExperienceNeeded();
        freeAttributePoints += 2; // 每级获得2点自由属性点

        Debug.Log($"角色升级到 {level} 级！获得 2 点自由属性点。");
        RecalculateDerivedStats();
        ApplyStatEffects();
        BroadcastLevel(); // 升级后广播给 HUD
    }

    int CalculateExperienceNeeded()
    {
        if (level <= 10) return 100 + level * 20;
        if (level <= 20) return 300 + level * 30;
        if (level <= 30) return 500 + level * 40;
        if (level <= 40) return 800 + level * 50;
        return 1200 + level * 60;
    }

    public void AddAttributePoints(string attributeName, int points)
    {
        // 确保点数为正数，且有足够的自由属性点
        if (points <= 0 || freeAttributePoints < points) return;
        
        // 防止属性值过高
        const int maxAttributeValue = 999;

        switch (attributeName.ToLower())
        {
            case "strength":
                strength = Mathf.Min(strength + points, maxAttributeValue);
                break;
            case "vitality":
                vitality = Mathf.Min(vitality + points, maxAttributeValue);
                break;
            case "intelligence":
                intelligence = Mathf.Min(intelligence + points, maxAttributeValue);
                break;
            case "resonance":
                resonance = Mathf.Min(resonance + points, maxAttributeValue);
                break;
        }

        freeAttributePoints -= points;
        RecalculateDerivedStats();
        ApplyStatEffects();
    }

    public void RecalculateDerivedStats()
    {
        // 确保基础属性不为负数
        strength = Mathf.Max(0, strength);
        vitality = Mathf.Max(0, vitality);
        intelligence = Mathf.Max(0, intelligence);
        resonance = Mathf.Max(0, resonance);
        
        // 计算衍生属性
        physicalDamageBonus = strength * 0.01f;         // 每点力量+1%物理伤害
        magicalDamageBonus = intelligence * 0.01f;      // 每点智慧+1%魔法伤害
        healthBonus = vitality * 10f;                   // 每点体质+10生命值
        manaBonus = intelligence * 5f;                  // 每点智慧+5魔法值
        burdenResistance = vitality * 0.005f;           // 每点体质+0.5%负担抗性
        alchemySuccessRateBonus = intelligence * 0.01f; // 每点智慧+1%炼金成功率
        fragmentDiscoveryRate = resonance * 0.01f;      // 每点共鸣+1%碎片发现率
        memoryFragmentEffectBonus = resonance * 0.02f;  // 每点共鸣+2%记忆碎片效果

        // 确保衍生属性不为负数
        physicalDamageBonus = Mathf.Max(0f, physicalDamageBonus);
        magicalDamageBonus = Mathf.Max(0f, magicalDamageBonus);
        healthBonus = Mathf.Max(0f, healthBonus);
        manaBonus = Mathf.Max(0f, manaBonus);
        burdenResistance = Mathf.Max(0f, burdenResistance);
        alchemySuccessRateBonus = Mathf.Max(0f, alchemySuccessRateBonus);
        fragmentDiscoveryRate = Mathf.Max(0f, fragmentDiscoveryRate);
        memoryFragmentEffectBonus = Mathf.Max(0f, memoryFragmentEffectBonus);

        // 应用属性阈值递减
        ApplyStatDiminishingReturns();
    }

    void ApplyStatDiminishingReturns()
    {
        // 力量：50点后收益减半
        if (strength > 50)
        {
            float excess = strength - 50;
            physicalDamageBonus -= excess * 0.005f;
            physicalDamageBonus = Mathf.Max(0f, physicalDamageBonus);
        }

        // 体质：40点后收益减半
        if (vitality > 40)
        {
            float excess = vitality - 40;
            healthBonus -= excess * 5f;
            burdenResistance -= excess * 0.0025f;
            healthBonus = Mathf.Max(0f, healthBonus);
            burdenResistance = Mathf.Max(0f, burdenResistance);
        }

        // 智慧：45点后收益减半
        if (intelligence > 45)
        {
            float excess = intelligence - 45;
            magicalDamageBonus -= excess * 0.005f;
            alchemySuccessRateBonus -= excess * 0.005f;
            magicalDamageBonus = Mathf.Max(0f, magicalDamageBonus);
            alchemySuccessRateBonus = Mathf.Max(0f, alchemySuccessRateBonus);
        }

        // 共鸣：30点后收益显著降低
        if (resonance > 30)
        {
            float excess = resonance - 30;
            fragmentDiscoveryRate -= excess * 0.0075f;
            memoryFragmentEffectBonus -= excess * 0.015f;
            fragmentDiscoveryRate = Mathf.Max(0f, fragmentDiscoveryRate);
            memoryFragmentEffectBonus = Mathf.Max(0f, memoryFragmentEffectBonus);
        }
    }

    void ApplyStatEffects()
    {
        // 应用生命值加成
        if (healthSystem != null)
        {
            // 这里需要根据HealthSystem的实现来调整
            // 示例：healthSystem.maxHealth = 100 + healthBonus;
        }

        // 应用伤害加成
        if (weaponSystem != null)
        {
            weaponSystem.UpdateDamageBonuses(physicalDamageBonus, magicalDamageBonus);
        }

        // 应用炼金成功率加成
        if (alchemySystem != null)
        {
            alchemySystem.UpdateSuccessRateBonus(alchemySuccessRateBonus);
        }
    }

    public int GetMaxSoulCoreSlots()
    {
        // 基础10槽位，每5点智慧+1
        return 10 + (intelligence / 5);
    }

    public float GetEffectiveBurdenResistance()
    {
        return burdenResistance;
    }

    public float GetEffectiveAlchemySuccessRateBonus()
    {
        return alchemySuccessRateBonus;
    }

    public float GetEffectiveFragmentDiscoveryRate()
    {
        return fragmentDiscoveryRate;
    }

    public float GetEffectiveMemoryFragmentEffectBonus()
    {
        return memoryFragmentEffectBonus;
    }

    public void ResetStats()
    {
        // 重置属性（用于多周目继承）
        strength = 10;
        vitality = 10;
        intelligence = 10;
        resonance = 10;
        level = 1;
        experience = 0;
        freeAttributePoints = 0;
        RecalculateDerivedStats();
        ApplyStatEffects();
    }

    public void ApplyPermanentStats(int[] permanentStats) // [strength, vitality, intelligence, resonance]
    {
        // 确保数组不为空，且长度至少为4
        if (permanentStats == null || permanentStats.Length < 4) return;
        
        // 确保永久属性值不为负数
        int permanentStrength = Mathf.Max(0, permanentStats[0]);
        int permanentVitality = Mathf.Max(0, permanentStats[1]);
        int permanentIntelligence = Mathf.Max(0, permanentStats[2]);
        int permanentResonance = Mathf.Max(0, permanentStats[3]);
        
        // 防止属性值过高
        const int maxAttributeValue = 999;
        strength = Mathf.Min(strength + permanentStrength, maxAttributeValue);
        vitality = Mathf.Min(vitality + permanentVitality, maxAttributeValue);
        intelligence = Mathf.Min(intelligence + permanentIntelligence, maxAttributeValue);
        resonance = Mathf.Min(resonance + permanentResonance, maxAttributeValue);
        
        RecalculateDerivedStats();
        ApplyStatEffects();
    }

    // 调试方法
    public void DebugPrintStats()
    {
        Debug.Log($"等级: {level}, 经验: {experience}/{experienceToNextLevel}");
        Debug.Log($"力量: {strength}, 体质: {vitality}, 智慧: {intelligence}, 共鸣: {resonance}");
        Debug.Log($"自由属性点: {freeAttributePoints}");
        Debug.Log($"物理伤害加成: {physicalDamageBonus * 100}%, 魔法伤害加成: {magicalDamageBonus * 100}%");
        Debug.Log($"生命值加成: {healthBonus}, 魔法值加成: {manaBonus}");
        Debug.Log($"负担抗性: {burdenResistance * 100}%, 炼金成功率加成: {alchemySuccessRateBonus * 100}%");
        Debug.Log($"碎片发现率: {fragmentDiscoveryRate * 100}%, 记忆碎片效果加成: {memoryFragmentEffectBonus * 100}%");
    }

    int IStatProvider.strength { get => strength; set => strength = value; }
    int IStatProvider.vitality { get => vitality; set => vitality = value; }
    int IStatProvider.intelligence { get => intelligence; set => intelligence = value; }
    int IStatProvider.resonance { get => resonance; set => resonance = value; }
    int IStatProvider.level => level;
    float IStatProvider.physicalDamageBonus => physicalDamageBonus;
    float IStatProvider.magicalDamageBonus => magicalDamageBonus;
    float IStatProvider.alchemySuccessRateBonus => alchemySuccessRateBonus;
    float IStatProvider.fragmentDiscoveryRate => fragmentDiscoveryRate;
}