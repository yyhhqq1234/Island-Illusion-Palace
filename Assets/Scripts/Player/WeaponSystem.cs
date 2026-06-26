using UnityEngine;
using System.Collections.Generic;

public enum WeaponType { Sword, Staff, Scythe, CrystalArm }
public enum DamageType { Physical, Magic, Mixed }
public enum ElementType { None, Frost, Water, Fire, Lightning, Soul, Holy }

// 晶能变体类型
public enum CrystalAspectType
{
    // 剑类
    StandardSword,
    WindfurySword,
    HeavySword,
    ElementalBlade,
    
    // 法杖
    StandardStaff,
    RapidFireStaff,
    ChargedStaff,
    PullStaff,
    
    // 镰刀
    StandardScythe,
    SweepingScythe,
    HookScythe,
    ReaperScythe,
    
    // 晶能武装
    StandardCrystal,
    SpreadCrystal,
    FocusCrystal,
    ResonanceCrystal
}

public class WeaponSystem : MonoBehaviour, IWeaponProvider
{
    [Header("武器类型")]
    public WeaponType currentWeaponType;

    WeaponType IWeaponProvider.currentWeaponType { get => currentWeaponType; set => currentWeaponType = value; }
    [Header("武器数据（ScriptableObject 驱动）")]
    [Tooltip("拖入 WeaponData asset 后自动覆盖下方数值")]
    public WeaponData weaponData;

    [Header("武器属性")]
    public float baseDamage = 20f;
    float IWeaponProvider.baseDamage { get => baseDamage; set => baseDamage = value; }
    [HideInInspector]
    public float attackInterval = 0.45f;
    float IWeaponProvider.attackInterval { get => attackInterval; set => attackInterval = value; }
    [Header("自定义攻击间隔")]
    [Tooltip("如果设置为true，将使用自定义的攻击间隔值，而不是武器默认值")]
    public bool useCustomAttackInterval = false;
    [Tooltip("自定义攻击间隔值，当useCustomAttackInterval为true时生效")]
    public float customAttackInterval = 0.45f;
    public float attackRange = 2f;
    public DamageType damageType = DamageType.Physical;
    public ElementType weaponElement = ElementType.None;

    [Header("武器强化")]
    public int enhancementLevel = 0;
    public int maxEnhancementLevel = 5;
    int IWeaponProvider.enhancementLevel { get => enhancementLevel; set => enhancementLevel = value; }
    int IWeaponProvider.maxEnhancementLevel => maxEnhancementLevel;
    public List<WeaponEffect> activeEffects = new List<WeaponEffect>();
    public int maxEffectSlots = 0;

    [Header("武器组件")]
    public GameObject weaponCollider;
    public Animator weaponAnimator;
    public Animator slashAnimator;
    public SpriteRenderer weaponSpriteRenderer;

    [Header("武器精灵")]
    public Sprite swordSprite;
    public Sprite staffSprite;
    public Sprite scytheSprite;
    public Sprite crystalArmSprite;

    [Header("攻击触发器")]
    public GameObject swordAttackTriggerPrefab;
    public Transform attackTriggerPos;

    [Header("连击系统")]
    public int comboCount = 0;
    public float comboResetTime = 1.5f;
    private float lastAttackTime = 0f;
    private float comboTimer = 0f;

    [Header("输入缓冲")]
    [Tooltip("缓冲窗口（秒）：冷却期间点击会被缓存，冷却结束后自动触发攻击")]
    public float inputBufferWindow = 0.3f;
    private bool attackBuffered = false;
    private float attackBufferExpireTime = 0f;

    [Header("晶能变体")]
    [Tooltip("当前激活的晶能变体类型")]
    public CrystalAspectType currentCrystalAspect = CrystalAspectType.StandardSword;
    [Tooltip("是否已嵌入晶能核心")]
    public bool hasCrystalCore = false;
    [Tooltip("是否在营火处（允许切换变体）")]
    public bool isAtCampfire = false;
    [Tooltip("晶能变体基准参数（由InitializeCrystalAspects自动填充）")]
    private float crystalBaseRange;
    private float crystalBaseInterval;
    private float crystalBaseDamage;

    [Header("晶能变体配置（ScriptableObject驱动）")]
    [Tooltip("拖入CrystalAspectConfig资源以启用配置驱动的变体系统")]
    public CrystalAspectConfig aspectConfig;

    // 配置缓存：运行时快速查找，避免每次搜索List
    private Dictionary<CrystalAspectType, CrystalAspectData> _aspectConfigCache;
    // 效果策略列表：散射、牵引、共鸣、收割
    private List<ICrystalAspectEffect> _effectStrategies;

    private Camera mainCamera;
    private CharacterStats characterStats;
    private BurdenSystem burdenSystem;
    private InventoryUI cachedInventoryUI;
    private AlchemyUI cachedAlchemyUI;
    private SummonSystem cachedSummonSystem;
    private float physicalDamageBonus = 0f;
    private float magicalDamageBonus = 0f;
    private float burdenDamageMultiplier = 1f;

    private readonly float[] enhancementDamageBonus = { 0f, 0.06f, 0.12f, 0.18f, 0.24f, 0.30f };

    void Awake()
    {
        InitializeAspectConfigCache();
        InitializeEffectStrategies();
    }

    void Start()
    {
        if (weaponCollider != null) weaponCollider.SetActive(false);
        weaponSpriteRenderer ??= GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);

        if (weaponAnimator == null)
        {
            weaponAnimator = FindChildTransform("Sword")?.GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
        }

        if (slashAnimator == null)
        {
            slashAnimator = FindChildTransform("Slash")?.GetComponent<Animator>();
        }

        if (weaponSpriteRenderer != null) weaponSpriteRenderer.enabled = true;
        UpdateWeaponStats();
        mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        characterStats = GetComponentInParent<CharacterStats>();
        burdenSystem = GetComponentInParent<BurdenSystem>();
        cachedInventoryUI = FindObjectOfType<InventoryUI>();
        cachedAlchemyUI = FindObjectOfType<AlchemyUI>();
        cachedSummonSystem = FindObjectOfType<SummonSystem>();
    }

    void Update()
    {
        RotateWeaponToMouse();
        HandleAttack();
        UpdateComboTimer();
    }

    void RotateWeaponToMouse()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsPaused())
        {
            return;
        }

        if (cachedInventoryUI != null && cachedInventoryUI.IsInventoryOpen())
            return;

        if (cachedAlchemyUI != null && cachedAlchemyUI.IsAlchemyPanelOpen())
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = mousePos - transform.position;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    void HandleAttack()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsPaused())
            return;

        if (cachedInventoryUI != null && cachedInventoryUI.IsInventoryOpen())
            return;

        if (cachedAlchemyUI != null && cachedAlchemyUI.IsAlchemyPanelOpen())
            return;

        // 获取当前攻击间隔值（应用晶能变体调整）
        float currentAttackInterval = useCustomAttackInterval ? customAttackInterval : attackInterval;
        currentAttackInterval *= GetCrystalAspectIntervalMultiplier();

        float timeSinceLastAttack = Time.time - lastAttackTime;
        bool canAttack = timeSinceLastAttack >= currentAttackInterval;

        // 左键：缓冲输入 — 点击后无论是否在冷却都记录意图
        if (Input.GetMouseButtonDown(0))
        {
            if (canAttack)
            {
                PrimaryAttack();
                lastAttackTime = Time.time;
                attackBuffered = false;
            }
            else
            {
                // 冷却中，缓冲这次点击
                attackBuffered = true;
                attackBufferExpireTime = Time.time + inputBufferWindow;
            }
        }

        // 消耗缓冲：冷却结束后自动触发
        if (attackBuffered && canAttack)
        {
            if (Time.time <= attackBufferExpireTime)
            {
                PrimaryAttack();
                lastAttackTime = Time.time;
            }
            attackBuffered = false;
        }

        // 右键：保持原有即时检测逻辑
        if (Input.GetMouseButtonDown(1) && canAttack)
        {
            SecondaryAttack();
            lastAttackTime = Time.time;
        }
    }

    void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                comboCount = 0;
            }
        }
    }

    void PrimaryAttack()
    {
        comboCount++;
        comboTimer = comboResetTime;

        if (currentWeaponType == WeaponType.CrystalArm && comboCount >= 10)
        {
            ChargedBurst();
            comboCount = 0;
        }
        else
        {
            float damage = GetCurrentDamage();

            // 应用晶能变体特殊效果（策略模式）
            if (hasCrystalCore)
            {
                ICrystalAspectEffect effect = FindEffectForCurrentAspect();
                if (effect != null && effect.Execute(this, damage))
                {
                    PlayAttackSound();
                    TriggerAttackAnimation();
                    return;
                }

                // 无替换效果 → 使用标准攻击（应用范围调整）
                ExecuteAttackWithAdjustedRange(damage);
            }
            else
            {
                ExecuteAttack(damage);
            }
        }

        PlayAttackSound();
        TriggerAttackAnimation();
    }

    void SecondaryAttack()
    {
        if (weaponData != null && weaponData.secondaryEffects.Count > 0)
        {
            foreach (var effect in weaponData.secondaryEffects)
                if (effect != null) effect.Execute(this, null);
        }
        else
        {
            switch (currentWeaponType)
            {
                case WeaponType.Sword:       PrecisionStrike();  break;
                case WeaponType.Staff:       SpellAmplify();     break;
                case WeaponType.Scythe:      SoulHarvest();      break;
                case WeaponType.CrystalArm:  EnergyBurst();      break;
            }
        }
        PlayAttackSound();
        TriggerAttackAnimation();
    }

    public void ExecuteAttack(float damage)
    {
        if (swordAttackTriggerPrefab != null && attackTriggerPos != null)
        {
            GameObject trigger = Instantiate(swordAttackTriggerPrefab, attackTriggerPos.position, attackTriggerPos.rotation);
            AttackTrigger attackTrigger = trigger.GetComponent<AttackTrigger>();
            if (attackTrigger != null)
            {
                attackTrigger.SetDamage((int)damage);
                attackTrigger.SetAttacker(transform.root.gameObject, true, false);
            }
        }
        else
        {
            CheckForEnemiesInAttackRange(damage);
        }
    }

    void PrecisionStrike()
    {
        float precisionDamage = GetCurrentDamage() * 1.3f;
        ExecuteAttack(precisionDamage);
        Debug.Log("精准打击：对弱点伤害+10%");
    }

    void SpellAmplify()
    {
        float magicDamage = GetCurrentDamage() * 1.2f;
        ExecuteAttack(magicDamage);
        Debug.Log("法术增幅：魔法伤害+20%");
    }

    void SoulHarvest()
    {
        ExecuteAttack(GetCurrentDamage());
        Debug.Log("灵魂收割：击杀回魔");
    }

    void EnergyBurst()
    {
        float burstDamage = GetCurrentDamage() * 1.5f;
        ExecuteAttack(burstDamage);
        Debug.Log("充能爆发：连续命中后伤害提升");
    }

    void ChargedBurst()
    {
        float chargedDamage = GetCurrentDamage() * 2f;
        ExecuteAttack(chargedDamage);
        Debug.Log("充能爆发！10连击触发特殊攻击");
        // 充能爆发消耗负担
        if (burdenSystem != null)
        {
            burdenSystem.AddBurden(3f);
        }
    }

    public float GetCurrentDamage()
    {
        float damage = baseDamage;

        if (enhancementLevel > 0 && enhancementLevel <= 5)
        {
            damage *= (1f + enhancementDamageBonus[enhancementLevel]);
        }

        foreach (var effect in activeEffects)
        {
            if (effect.effectType == WeaponEffectType.Damage)
            {
                damage *= (1f + effect.value);
            }
        }

        // 应用属性加成
        if (damageType == DamageType.Physical || damageType == DamageType.Mixed)
        {
            damage *= (1f + physicalDamageBonus);
        }
        if (damageType == DamageType.Magic || damageType == DamageType.Mixed)
        {
            damage *= (1f + magicalDamageBonus);
        }

        damage += Random.Range(0, 6);

        // 应用晶能变体伤害倍率
        damage *= GetCrystalAspectDamageMultiplier();

        return damage * burdenDamageMultiplier;
    }

    public void UpdateDamageBonuses(float physicalBonus, float magicalBonus)
    {
        physicalDamageBonus = physicalBonus;
        magicalDamageBonus = magicalBonus;
    }

    public void UpdateBurdenDamageMultiplier(float multiplier)
    {
        burdenDamageMultiplier = multiplier;
    }

    public float CalculateDamageToEnemy(EnemyAI enemy)
    {
        float baseDmg = GetCurrentDamage();
        float multiplier = 1f;

        // 应用弱点 (弱点倍率=1.8x → 加成+0.8)
        if (enemy.weaknesses.Contains(ConvertElementType(weaponElement)))
        {
            multiplier += 0.8f;
            Debug.Log($"击中弱点！伤害+80%");
        }
        // 应用抗性 (抗性倍率=0.6x → 加成-0.4)
        else if (enemy.resistances.Contains(ConvertElementType(weaponElement)))
        {
            multiplier -= 0.4f;  // 底线 0.6x
            Debug.Log($"击中抗性！伤害-40%");
        }

        // 应用元素克制链 (克制链倍率=1.2x → 加成+0.2，加法叠加)
        if (IsElementalChain(weaponElement, ConvertEnemyElementType(enemy.elementType)))
        {
            multiplier += 0.2f;
            Debug.Log($"元素克制链！额外伤害+20%");
        }

        // 加法叠加上限 2.0x，下限 0.1x
        multiplier = Mathf.Clamp(multiplier, 0.1f, 2.0f);

        return baseDmg * multiplier;
    }

    bool IsElementalChain(ElementType weaponElem, ElementType enemyElem)
    {
        // 基础元素循环：冰霜 → 流水 → 火焰 → 冰霜
        if ((weaponElem == ElementType.Frost && enemyElem == ElementType.Water) ||
            (weaponElem == ElementType.Water && enemyElem == ElementType.Fire) ||
            (weaponElem == ElementType.Fire && enemyElem == ElementType.Frost))
        {
            return true;
        }

        // 特殊克制：雷电 → 流水
        if (weaponElem == ElementType.Lightning && enemyElem == ElementType.Water)
        {
            return true;
        }

        // 特殊克制：圣光 → 灵魂
        if (weaponElem == ElementType.Holy && enemyElem == ElementType.Soul)
        {
            return true;
        }

        return false;
    }

    EnemyAI.ElementType ConvertElementType(ElementType elem)
    {
        return elem switch
        {
            ElementType.Frost => EnemyAI.ElementType.Frost,
            ElementType.Water => EnemyAI.ElementType.Water,
            ElementType.Fire => EnemyAI.ElementType.Fire,
            ElementType.Lightning => EnemyAI.ElementType.Lightning,
            ElementType.Soul => EnemyAI.ElementType.Soul,
            ElementType.Holy => EnemyAI.ElementType.Holy,
            _ => EnemyAI.ElementType.Physical
        };
    }

    ElementType ConvertEnemyElementType(EnemyAI.ElementType elem)
    {
        return elem switch
        {
            EnemyAI.ElementType.Frost => ElementType.Frost,
            EnemyAI.ElementType.Water => ElementType.Water,
            EnemyAI.ElementType.Fire => ElementType.Fire,
            EnemyAI.ElementType.Lightning => ElementType.Lightning,
            EnemyAI.ElementType.Soul => ElementType.Soul,
            EnemyAI.ElementType.Holy => ElementType.Holy,
            EnemyAI.ElementType.Physical => ElementType.None,
            _ => ElementType.None
        };
    }

    void CheckForEnemiesInAttackRange(float damage)
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useTriggers = false;

        Collider2D[] hitColliders = new Collider2D[10];
        int count = Physics2D.OverlapCircle(transform.position, attackRange, contactFilter, hitColliders);

        for (int i = 0; i < count; i++)
        {
            if (hitColliders[i].CompareTag("Enemy"))
            {
                EnemyAI enemy = hitColliders[i].GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    float finalDamage = CalculateDamageToEnemy(enemy);
                    hitColliders[i].GetComponent<HealthSystem>()?.TakeDamage(finalDamage);
                }
                else
                {
                    hitColliders[i].GetComponent<HealthSystem>()?.TakeDamage(damage);
                }
            }
        }
    }

    // ==================== 晶能变体系统 ====================

    /// <summary>
    /// 初始化晶能变体基准参数，记录当前武器的基础属性
    /// </summary>
    private void InitializeCrystalAspects()
    {
        crystalBaseRange = attackRange;
        crystalBaseInterval = attackInterval;
        crystalBaseDamage = baseDamage;
        Debug.Log($"[WeaponSystem] 晶能变体基准参数已初始化 - 范围:{crystalBaseRange} 间隔:{crystalBaseInterval} 伤害:{crystalBaseDamage}");
    }

    /// <summary>
    /// 嵌入晶能核心，激活默认变体
    /// </summary>
    public void EmbedCrystalCore()
    {
        if (hasCrystalCore)
        {
            Debug.Log("[WeaponSystem] 已嵌入晶能核心，无需重复嵌入");
            return;
        }

        hasCrystalCore = true;
        InitializeCrystalAspects();

        // 通过配置获取当前武器的默认变体
        if (aspectConfig != null)
        {
            currentCrystalAspect = aspectConfig.GetDefaultAspectForWeapon(currentWeaponType);
        }
        else
        {
            // 降级：无配置时使用硬编码映射
            currentCrystalAspect = GetFallbackDefaultAspect(currentWeaponType);
        }

        Debug.Log($"[WeaponSystem] 晶能核心已嵌入，当前变体: {currentCrystalAspect}");
    }

    /// <summary>
    /// 切换晶能变体（需在营火处操作）
    /// </summary>
    public bool SwitchCrystalAspect(CrystalAspectType newAspect)
    {
        if (!hasCrystalCore)
        {
            Debug.Log("[WeaponSystem] 未嵌入晶能核心，无法切换变体");
            return false;
        }

        if (!isAtCampfire)
        {
            Debug.Log("[WeaponSystem] 切换晶能变体需要在营火处操作");
            return false;
        }

        // 通过配置验证变体兼容性
        if (!IsAspectValidForCurrentWeapon(newAspect))
        {
            Debug.Log($"[WeaponSystem] 变体 {newAspect} 不适用于当前武器 {currentWeaponType}");
            return false;
        }

        currentCrystalAspect = newAspect;
        Debug.Log($"[WeaponSystem] 晶能变体已切换至: {currentCrystalAspect}");
        return true;
    }

    /// <summary>
    /// 验证变体是否适用于当前武器类型（优先使用配置，降级为硬编码）
    /// </summary>
    private bool IsAspectValidForCurrentWeapon(CrystalAspectType aspect)
    {
        if (aspectConfig != null)
            return aspectConfig.IsAspectCompatible(aspect, currentWeaponType);

        return IsAspectCompatibleFallback(aspect, currentWeaponType);
    }

    /// <summary>
    /// 应用晶能变体参数修改器（由外部系统在需要时调用）
    /// </summary>
    public void ApplyCrystalAspectModifiers()
    {
        if (!hasCrystalCore) return;

        float rangeMultiplier = GetCrystalAspectRangeMultiplier();
        float intervalMultiplier = GetCrystalAspectIntervalMultiplier();

        Debug.Log($"[WeaponSystem] 应用变体参数 - 范围倍率:{rangeMultiplier} 间隔倍率:{intervalMultiplier} 伤害倍率:{GetCrystalAspectDamageMultiplier()}");
    }

    /// <summary>
    /// 获取晶能变体范围倍率（配置驱动，降级为硬编码）
    /// </summary>
    public float GetCrystalAspectRangeMultiplier()
    {
        if (!hasCrystalCore) return 1f;

        if (_aspectConfigCache != null && _aspectConfigCache.TryGetValue(currentCrystalAspect, out var data))
            return data.rangeMultiplier;

        // 降级：配置缺失时使用硬编码值
        return GetFallbackRangeMultiplier(currentCrystalAspect);
    }

    /// <summary>
    /// 获取晶能变体攻击间隔倍率（配置驱动，降级为硬编码）
    /// </summary>
    public float GetCrystalAspectIntervalMultiplier()
    {
        if (!hasCrystalCore) return 1f;

        if (_aspectConfigCache != null && _aspectConfigCache.TryGetValue(currentCrystalAspect, out var data))
            return data.intervalMultiplier;

        return GetFallbackIntervalMultiplier(currentCrystalAspect);
    }

    /// <summary>
    /// 获取晶能变体伤害倍率（配置驱动，降级为硬编码）
    /// </summary>
    public float GetCrystalAspectDamageMultiplier()
    {
        if (!hasCrystalCore) return 1f;

        if (_aspectConfigCache != null && _aspectConfigCache.TryGetValue(currentCrystalAspect, out var data))
            return data.damageMultiplier;

        return GetFallbackDamageMultiplier(currentCrystalAspect);
    }

    /// <summary>
    /// 使用变体调整后的范围执行攻击
    /// </summary>
    private void ExecuteAttackWithAdjustedRange(float damage)
    {
        float adjustedRange = attackRange * GetCrystalAspectRangeMultiplier();

        if (swordAttackTriggerPrefab != null && attackTriggerPos != null)
        {
            GameObject trigger = Instantiate(swordAttackTriggerPrefab, attackTriggerPos.position, attackTriggerPos.rotation);
            AttackTrigger attackTrigger = trigger.GetComponent<AttackTrigger>();
            if (attackTrigger != null)
            {
                attackTrigger.SetDamage((int)damage);
                attackTrigger.SetAttacker(transform.root.gameObject, true, false);
            }
        }
        else
        {
            CheckForEnemiesInAttackRangeWithRange(damage, adjustedRange);
        }
    }

    /// <summary>
    /// 使用指定范围检测敌人（变体版本，应用共鸣/收割策略修饰器）
    /// </summary>
    private void CheckForEnemiesInAttackRangeWithRange(float damage, float range)
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useTriggers = false;

        Collider2D[] hitColliders = new Collider2D[20];
        int count = Physics2D.OverlapCircle(transform.position, range, contactFilter, hitColliders);

        for (int i = 0; i < count; i++)
        {
            if (hitColliders[i].CompareTag("Enemy"))
            {
                EnemyAI enemy = hitColliders[i].GetComponent<EnemyAI>();
                float finalDamage = damage;

                if (enemy != null)
                    finalDamage = CalculateDamageToEnemy(enemy);

                // 通过策略修饰器应用共鸣和收割加成
                finalDamage = ApplyAspectModifiers(finalDamage, enemy);

                hitColliders[i].GetComponent<HealthSystem>()?.TakeDamage(finalDamage);
            }
        }
    }

    // ==================== 晶能变体基础设施方法 ====================

    #region 配置与策略初始化

    void InitializeAspectConfigCache()
    {
        _aspectConfigCache = new Dictionary<CrystalAspectType, CrystalAspectData>();
        if (aspectConfig != null)
        {
            foreach (var data in aspectConfig.aspects)
            {
                if (data != null && !_aspectConfigCache.ContainsKey(data.aspectType))
                    _aspectConfigCache[data.aspectType] = data;
            }
            Debug.Log($"[WeaponSystem] 变体配置已加载：{_aspectConfigCache.Count}条");
        }
    }

    void InitializeEffectStrategies()
    {
        _effectStrategies = new List<ICrystalAspectEffect>
        {
            new ScatterEffect(),
            new PullEffect(),
            new ResonanceEffect(),
            new ReaperEffect()
        };
    }

    /// <summary>查找当前变体对应的效果策略</summary>
    ICrystalAspectEffect FindEffectForCurrentAspect()
    {
        if (_effectStrategies == null) return null;
        return _effectStrategies.Find(e => e.CanExecute(currentCrystalAspect));
    }

    #endregion

    #region 策略修饰器：供效果策略类和攻击检测调用

    /// <summary>应用所有激活的变体伤害修饰器（共鸣、收割等）</summary>
    public float ApplyAspectModifiers(float damage, EnemyAI enemy = null)
    {
        if (_effectStrategies == null) return damage;

        float result = damage;
        foreach (var effect in _effectStrategies)
        {
            if (effect is ResonanceEffect resonance)
                result *= resonance.CalculateMultiplier(this);
            else if (effect is ReaperEffect reaper)
                result = reaper.ApplyBonus(this, result, enemy);
        }
        return result;
    }

    /// <summary>获取共鸣倍率（公开给策略类和外部调用）</summary>
    public float GetResonanceBonus()
    {
        if (_effectStrategies != null)
        {
            var resonance = _effectStrategies.Find(e => e is ResonanceEffect) as ResonanceEffect;
            if (resonance != null) return resonance.CalculateMultiplier(this);
        }
        return 1f;
    }

    /// <summary>获取收割修正后伤害（公开给策略类和外部调用）</summary>
    public float GetReaperBonus(float damage, EnemyAI enemy)
    {
        if (_effectStrategies != null)
        {
            var reaper = _effectStrategies.Find(e => e is ReaperEffect) as ReaperEffect;
            if (reaper != null) return reaper.ApplyBonus(this, damage, enemy);
        }
        return damage;
    }

    #endregion

    #region 公开辅助方法：供策略类访问WeaponSystem内部能力

    /// <summary>实例化攻击触发器（公开给ScatterEffect等策略使用）</summary>
    public void InstantiateAttackTrigger(Quaternion rotation, int damage = 0)
    {
        if (swordAttackTriggerPrefab != null && attackTriggerPos != null)
        {
            GameObject trigger = Instantiate(swordAttackTriggerPrefab, attackTriggerPos.position, rotation);
            AttackTrigger at = trigger.GetComponent<AttackTrigger>();
            if (at != null)
            {
                at.SetAttacker(transform.root.gameObject, true, false);
                if (damage > 0) at.SetDamage(damage);
            }
        }
    }

    /// <summary>在指定范围内查找所有敌人（公开给PullEffect等策略使用）</summary>
    public List<Collider2D> FindEnemiesInRange(float range)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filter.useTriggers = false;

        Collider2D[] results = new Collider2D[20];
        int count = Physics2D.OverlapCircle(transform.position, range, filter, results);

        List<Collider2D> enemies = new List<Collider2D>();
        for (int i = 0; i < count; i++)
        {
            if (results[i].CompareTag("Enemy"))
                enemies.Add(results[i]);
        }
        return enemies;
    }

    /// <summary>获取场上活跃召唤物数量（公开给ResonanceEffect使用）</summary>
    public int GetActiveSummonCount(SummonSystem summonSystem)
    {
        if (summonSystem == null) return 0;
        var activeSummons = summonSystem.GetActiveSummons();
        if (activeSummons == null) return 0;

        int count = 0;
        foreach (var s in activeSummons) { if (s != null) count++; }
        return count;
    }

    /// <summary>暴露主相机引用（公开给ScatterEffect使用）</summary>
    public Camera MainCamera => mainCamera;

    #endregion

    #region 降级硬编码值（配置缺失时的回退）

    static CrystalAspectType GetFallbackDefaultAspect(WeaponType weapon)
    {
        return weapon switch
        {
            WeaponType.Sword => CrystalAspectType.StandardSword,
            WeaponType.Staff => CrystalAspectType.StandardStaff,
            WeaponType.Scythe => CrystalAspectType.StandardScythe,
            WeaponType.CrystalArm => CrystalAspectType.StandardCrystal,
            _ => CrystalAspectType.StandardSword
        };
    }

    static bool IsAspectCompatibleFallback(CrystalAspectType aspect, WeaponType weapon)
    {
        return weapon switch
        {
            WeaponType.Sword => aspect is CrystalAspectType.StandardSword or CrystalAspectType.WindfurySword or CrystalAspectType.HeavySword or CrystalAspectType.ElementalBlade,
            WeaponType.Staff => aspect is CrystalAspectType.StandardStaff or CrystalAspectType.RapidFireStaff or CrystalAspectType.ChargedStaff or CrystalAspectType.PullStaff,
            WeaponType.Scythe => aspect is CrystalAspectType.StandardScythe or CrystalAspectType.SweepingScythe or CrystalAspectType.HookScythe or CrystalAspectType.ReaperScythe,
            WeaponType.CrystalArm => aspect is CrystalAspectType.StandardCrystal or CrystalAspectType.SpreadCrystal or CrystalAspectType.FocusCrystal or CrystalAspectType.ResonanceCrystal,
            _ => false
        };
    }

    static float GetFallbackDamageMultiplier(CrystalAspectType type)
    {
        return type switch
        {
            // 剑类
            CrystalAspectType.WindfurySword => 0.85f,
            CrystalAspectType.HeavySword => 1.40f,
            // 法杖
            CrystalAspectType.RapidFireStaff => 0.70f,
            CrystalAspectType.ChargedStaff => 1.60f,
            CrystalAspectType.PullStaff => 0.90f,
            // 镰刀
            CrystalAspectType.SweepingScythe => 0.85f,
            CrystalAspectType.HookScythe => 1.10f,
            // 晶能武装
            CrystalAspectType.SpreadCrystal => 0.60f,
            CrystalAspectType.FocusCrystal => 1.50f,
            _ => 1f
        };
    }

    static float GetFallbackRangeMultiplier(CrystalAspectType type)
    {
        return type switch
        {
            CrystalAspectType.WindfurySword => 1.30f,
            CrystalAspectType.ChargedStaff => 1.20f,
            CrystalAspectType.PullStaff => 1.40f,
            CrystalAspectType.SweepingScythe => 1.40f,
            CrystalAspectType.HookScythe => 1.80f,
            CrystalAspectType.SpreadCrystal => 1.50f,
            CrystalAspectType.FocusCrystal => 0.80f,
            _ => 1f
        };
    }

    static float GetFallbackIntervalMultiplier(CrystalAspectType type)
    {
        return type switch
        {
            CrystalAspectType.HeavySword => 1.50f,
            CrystalAspectType.RapidFireStaff => 0.65f,
            CrystalAspectType.ChargedStaff => 1.60f,
            CrystalAspectType.HookScythe => 1.30f,
            CrystalAspectType.FocusCrystal => 1.25f,
            _ => 1f
        };
    }

    #endregion

    // ==================== 晶能变体系统结束 ====================

    void PlayAttackSound()
    {
        try
        {
            var audioManagerType = System.Reflection.Assembly.GetExecutingAssembly().GetType("AudioManager");
            if (audioManagerType != null)
            {
                var instanceProperty = audioManagerType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var audioManager = instanceProperty.GetValue(null);
                    if (audioManager != null)
                    {
                        var playPlayerAttackMethod = audioManagerType.GetMethod("PlayPlayerAttack");
                        if (playPlayerAttackMethod != null)
                        {
                            playPlayerAttackMethod.Invoke(audioManager, null);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error playing attack sound: {e.Message}");
        }
    }

    void TriggerAttackAnimation()
    {
        SetAnimatorTriggerSafe(weaponAnimator, "IsAttack");
        SetAnimatorTriggerSafe(slashAnimator, "IsAttack");
    }

    void DeactivateWeaponCollider()
    {
        weaponCollider?.SetActive(false);
    }

    Transform FindChildTransform(string name)
    {
        Transform found = transform.Find(name);

        if (found == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name == name)
                {
                    found = child;
                    break;
                }
            }
        }

        if (found == null)
        {
            found = FindChildTransformRecursive(transform, name);
        }

        return found;
    }

    Transform FindChildTransformRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildTransformRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    public void SwitchWeapon(WeaponType newWeaponType)
    {
        currentWeaponType = newWeaponType;
        UpdateWeaponStats();
        UpdateWeaponSprite();
    }

    void UpdateWeaponStats()
    {
        if (weaponData != null)
        {
            baseDamage = weaponData.GetRandomDamage();
            attackInterval = weaponData.attackInterval;
            attackRange = weaponData.attackRange;
            weaponElement = (ElementType)(int)weaponData.elementType;
            switch (weaponData.weaponType)
            {
                case WeaponType.Sword:       damageType = DamageType.Physical; break;
                case WeaponType.Staff:       damageType = DamageType.Magic;    break;
                default:                                damageType = DamageType.Mixed;    break;
            }
            return;
        }
        // 无 WeaponData 时保留 Inspector 设置（不覆盖 attackInterval 等字段）
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
                damageType = DamageType.Physical; weaponElement = ElementType.None; break;
            case WeaponType.Staff:
                damageType = DamageType.Magic; weaponElement = ElementType.Soul; break;
            case WeaponType.Scythe:
                damageType = DamageType.Mixed; weaponElement = ElementType.Soul; break;
            case WeaponType.CrystalArm:
                damageType = DamageType.Mixed; weaponElement = ElementType.Frost; break;
        }
    }

    void UpdateWeaponSprite()
    {
        if (weaponSpriteRenderer == null) return;

        Sprite targetSprite = currentWeaponType switch
        {
            WeaponType.Sword => swordSprite,
            WeaponType.Staff => staffSprite,
            WeaponType.Scythe => scytheSprite,
            WeaponType.CrystalArm => crystalArmSprite,
            _ => null
        };

        if (targetSprite != null)
            weaponSpriteRenderer.sprite = targetSprite;
        weaponSpriteRenderer.enabled = true;
    }

    public bool EnhanceWeapon(IInventoryService inventory = null)
    {
        if (enhancementLevel >= maxEnhancementLevel)
        {
            Debug.Log("已达最大强化等级！");
            return false;
        }

        int nextLevel = enhancementLevel + 1;
        int[] soulCost = { 0, 60, 120, 240, 400, 600 };

        if (inventory != null)
        {
            if (!inventory.HasEnoughSouls(soulCost[nextLevel]))
            {
                Debug.Log($"灵魂不足！需要 {soulCost[nextLevel]} 灵魂");
                return false;
            }
            inventory.ConsumeSouls(soulCost[nextLevel]);
        }

        enhancementLevel++;

        if (enhancementLevel == 3)
        {
            maxEffectSlots = 1;
            Debug.Log("解锁特效槽1");
        }
        else if (enhancementLevel == 5)
        {
            maxEffectSlots = 2;
            Debug.Log("解锁特效槽2");
        }

        Debug.Log($"武器强化至+{enhancementLevel}，伤害+{enhancementDamageBonus[enhancementLevel] * 100}%");
        return true;
    }

    public bool AddEffect(WeaponEffect effect)
    {
        if (activeEffects.Count >= maxEffectSlots)
        {
            Debug.Log("特效槽已满！");
            return false;
        }

        activeEffects.Add(effect);
        Debug.Log($"添加武器特效：{effect.effectName}");
        return true;
    }

    void SetAnimatorTriggerSafe(Animator animator, string paramName)
    {
        if (animator == null) return;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(paramName);
                return;
            }
        }

        string[] alternatives = { "Attack", "IsAttacking", "Swing", "Slash" };
        foreach (string alt in alternatives)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == alt && param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(alt);
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void HealPercent(float percent)
    {
        var hs = GetComponent<HealthSystem>();
        if (hs != null) hs.Heal(hs.maxHealth * percent);
    }

    public void RestoreMana(float amount)
    {
        var ms = GetComponent<ManaSystem>();
        if (ms != null) ms.RestoreMana(amount);
    }

    public void BuffSummons(float percent)
    {
        if (cachedSummonSystem != null)
            foreach (var s in cachedSummonSystem.GetActiveSummons())
                if (s != null) s.GetComponent<SummonedCreatureAI>()?.AddBuff(percent);
    }

    public void DebuffEnemy(GameObject target, float percent)
    {
        if (target == null) return;
        var ai = target.GetComponent<EnemyAI>();
        if (ai != null) { ai.damage *= (1f - percent); ai.moveSpeed *= (1f - percent); }
    }
}

public enum WeaponEffectType { Damage, Critical, Lifesteal, Precision, ArmorPenetration }

[System.Serializable]
public class WeaponEffect
{
    public string effectName;
    public WeaponEffectType effectType;
    public float value;

    public WeaponEffect(string name, WeaponEffectType type, float val)
    {
        effectName = name;
        effectType = type;
        value = val;
    }
}
