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
    public float attackInterval = 0.833f;
    float IWeaponProvider.attackInterval { get => attackInterval; set => attackInterval = value; }
    [Header("自定义攻击间隔")]
    [Tooltip("如果设置为true，将使用自定义的攻击间隔值，而不是武器默认值")]
    public bool useCustomAttackInterval = false;
    [Tooltip("自定义攻击间隔值，当useCustomAttackInterval为true时生效")]
    public float customAttackInterval = 0.833f;
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

    private Camera mainCamera;
    private CharacterStats characterStats;
    private BurdenSystem burdenSystem;
    private float physicalDamageBonus = 0f;
    private float magicalDamageBonus = 0f;
    private float burdenDamageMultiplier = 1f;

    private readonly float[] enhancementDamageBonus = { 0f, 0.06f, 0.12f, 0.18f, 0.24f, 0.30f };

    void Awake()
    {
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

        // 检查背包是否打开
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            return;
        }

        // 检查炼金界面是否打开
        AlchemyUI alchemyUI = FindObjectOfType<AlchemyUI>();
        if (alchemyUI != null && alchemyUI.IsAlchemyPanelOpen())
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main ?? FindObjectOfType<Camera>();
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
        {
            return;
        }

        // 检查背包是否打开
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            return;
        }

        // 检查炼金界面是否打开
        AlchemyUI alchemyUI = FindObjectOfType<AlchemyUI>();
        if (alchemyUI != null && alchemyUI.IsAlchemyPanelOpen())
        {
            return;
        }

        // 获取当前攻击间隔值（应用晶能变体调整）
        float currentAttackInterval = useCustomAttackInterval ? customAttackInterval : attackInterval;
        currentAttackInterval *= GetCrystalAspectIntervalMultiplier();
        
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime >= currentAttackInterval)
        {
            PrimaryAttack();
            lastAttackTime = Time.time;
        }

        if (Input.GetMouseButtonDown(1) && Time.time - lastAttackTime >= currentAttackInterval)
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
            
            // 应用晶能变体特殊效果
            if (hasCrystalCore)
            {
                switch (currentCrystalAspect)
                {
                    // 散射晶能：发射3枚散射弹
                    case CrystalAspectType.SpreadCrystal:
                        ExecuteScatterAttack(damage);
                        PlayAttackSound();
                        TriggerAttackAnimation();
                        return;
                    
                    // 牵引法杖/勾爪镰刀：牵引攻击
                    case CrystalAspectType.PullStaff:
                    case CrystalAspectType.HookScythe:
                        ExecutePullAttack(damage);
                        PlayAttackSound();
                        TriggerAttackAnimation();
                        return;
                    
                    // 其他变体使用标准ExecuteAttack，但应用范围调整
                    default:
                        ExecuteAttackWithAdjustedRange(damage);
                        break;
                }
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
        
        // 根据当前武器类型设置默认变体
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
                currentCrystalAspect = CrystalAspectType.StandardSword;
                break;
            case WeaponType.Staff:
                currentCrystalAspect = CrystalAspectType.StandardStaff;
                break;
            case WeaponType.Scythe:
                currentCrystalAspect = CrystalAspectType.StandardScythe;
                break;
            case WeaponType.CrystalArm:
                currentCrystalAspect = CrystalAspectType.StandardCrystal;
                break;
        }

        Debug.Log($"[WeaponSystem] 晶能核心已嵌入，当前变体: {currentCrystalAspect}");
    }

    /// <summary>
    /// 切换晶能变体（需在营火处操作）
    /// </summary>
    /// <param name="newAspect">目标变体类型</param>
    /// <returns>是否切换成功</returns>
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

        // 验证变体与当前武器类型匹配
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
    /// 验证变体是否适用于当前武器类型
    /// </summary>
    private bool IsAspectValidForCurrentWeapon(CrystalAspectType aspect)
    {
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
                return aspect == CrystalAspectType.StandardSword
                    || aspect == CrystalAspectType.WindfurySword
                    || aspect == CrystalAspectType.HeavySword
                    || aspect == CrystalAspectType.ElementalBlade;
            case WeaponType.Staff:
                return aspect == CrystalAspectType.StandardStaff
                    || aspect == CrystalAspectType.RapidFireStaff
                    || aspect == CrystalAspectType.ChargedStaff
                    || aspect == CrystalAspectType.PullStaff;
            case WeaponType.Scythe:
                return aspect == CrystalAspectType.StandardScythe
                    || aspect == CrystalAspectType.SweepingScythe
                    || aspect == CrystalAspectType.HookScythe
                    || aspect == CrystalAspectType.ReaperScythe;
            case WeaponType.CrystalArm:
                return aspect == CrystalAspectType.StandardCrystal
                    || aspect == CrystalAspectType.SpreadCrystal
                    || aspect == CrystalAspectType.FocusCrystal
                    || aspect == CrystalAspectType.ResonanceCrystal;
            default:
                return false;
        }
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
    /// 获取晶能变体范围倍率
    /// </summary>
    public float GetCrystalAspectRangeMultiplier()
    {
        if (!hasCrystalCore) return 1f;

        return currentCrystalAspect switch
        {
            // 剑类
            CrystalAspectType.WindfurySword => 1.30f,     // +30%
            // 法杖
            CrystalAspectType.ChargedStaff => 1.20f,       // +20%
            CrystalAspectType.PullStaff => 1.40f,           // +40%
            // 镰刀
            CrystalAspectType.SweepingScythe => 1.40f,      // +40%
            CrystalAspectType.HookScythe => 1.80f,          // +80%
            // 晶能武装
            CrystalAspectType.SpreadCrystal => 1.50f,       // +50%
            CrystalAspectType.FocusCrystal => 0.80f,        // -20%
            _ => 1f
        };
    }

    /// <summary>
    /// 获取晶能变体攻击间隔倍率
    /// </summary>
    public float GetCrystalAspectIntervalMultiplier()
    {
        if (!hasCrystalCore) return 1f;

        return currentCrystalAspect switch
        {
            // 剑类
            CrystalAspectType.HeavySword => 1.50f,           // +50%
            // 法杖
            CrystalAspectType.RapidFireStaff => 0.65f,       // -35%
            CrystalAspectType.ChargedStaff => 1.60f,          // +60%
            // 镰刀
            CrystalAspectType.HookScythe => 1.30f,            // +30%
            // 晶能武装
            CrystalAspectType.FocusCrystal => 1.25f,          // +25%
            _ => 1f
        };
    }

    /// <summary>
    /// 获取晶能变体伤害倍率
    /// </summary>
    public float GetCrystalAspectDamageMultiplier()
    {
        if (!hasCrystalCore) return 1f;

        return currentCrystalAspect switch
        {
            // 剑类
            CrystalAspectType.WindfurySword => 0.85f,        // 85%
            CrystalAspectType.HeavySword => 1.40f,           // 140%
            // 法杖
            CrystalAspectType.RapidFireStaff => 0.70f,       // 70%
            CrystalAspectType.ChargedStaff => 1.60f,          // 160%
            CrystalAspectType.PullStaff => 0.90f,             // 90%
            // 镰刀
            CrystalAspectType.SweepingScythe => 0.85f,        // 85%
            CrystalAspectType.HookScythe => 1.10f,            // 110%
            // 晶能武装
            CrystalAspectType.SpreadCrystal => 0.60f,         // 60%
            CrystalAspectType.FocusCrystal => 1.50f,          // 150%
            _ => 1f
        };
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
    /// 使用指定范围检测敌人（变体版本）
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
                if (enemy != null)
                {
                    float finalDamage = CalculateDamageToEnemy(enemy);
                    
                    // 应用收割镰刀加成
                    finalDamage = ApplyReaperBonus(finalDamage, enemy);
                    
                    // 应用共鸣晶能加成
                    finalDamage *= ApplyResonanceBonus();
                    
                    hitColliders[i].GetComponent<HealthSystem>()?.TakeDamage(finalDamage);
                }
                else
                {
                    float finalDamage = damage;
                    finalDamage *= ApplyResonanceBonus();
                    hitColliders[i].GetComponent<HealthSystem>()?.TakeDamage(finalDamage);
                }
            }
        }
    }

    /// <summary>
    /// 散射攻击（晶能武装 - 散射晶能）：发射3枚散射弹，每枚独立伤害判定
    /// </summary>
    private void ExecuteScatterAttack(float damage)
    {
        int projectileCount = 3;
        float spreadAngle = 30f; // 总散射角度

        Vector3 mousePos = mainCamera != null ? mainCamera.ScreenToWorldPoint(Input.mousePosition) : transform.position + transform.right * 5f;
        mousePos.z = 0f;
        Vector3 baseDirection = (mousePos - transform.position).normalized;

        float startAngle = -spreadAngle / 2f;
        float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg + angle);

            if (swordAttackTriggerPrefab != null && attackTriggerPos != null)
            {
                GameObject trigger = Instantiate(swordAttackTriggerPrefab, attackTriggerPos.position, rotation);
                AttackTrigger attackTrigger = trigger.GetComponent<AttackTrigger>();
                if (attackTrigger != null)
                {
                    attackTrigger.SetDamage((int)damage);
                    attackTrigger.SetAttacker(transform.root.gameObject, true, false);
                }
            }
        }

        Debug.Log($"[WeaponSystem] 散射攻击：发射{projectileCount}枚散射弹，每枚伤害{damage}");
    }

    /// <summary>
    /// 牵引攻击（法杖/镰刀）：将命中敌人向玩家方向牵引
    /// </summary>
    private void ExecutePullAttack(float damage)
    {
        float pullRange = attackRange * GetCrystalAspectRangeMultiplier();
        float pullForce = 5f;

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useTriggers = false;

        Collider2D[] hitColliders = new Collider2D[20];
        int count = Physics2D.OverlapCircle(transform.position, pullRange, contactFilter, hitColliders);

        for (int i = 0; i < count; i++)
        {
            if (hitColliders[i].CompareTag("Enemy"))
            {
                EnemyAI enemy = hitColliders[i].GetComponent<EnemyAI>();
                float finalDamage = damage;
                
                if (enemy != null)
                {
                    finalDamage = CalculateDamageToEnemy(enemy);
                    finalDamage = ApplyReaperBonus(finalDamage, enemy);
                }
                
                finalDamage *= ApplyResonanceBonus();
                hitColliders[i].GetComponent<HealthSystem>()?.TakeDamage(finalDamage);

                // 将敌人向玩家方向牵引
                Rigidbody2D enemyRb = hitColliders[i].GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 pullDirection = ((Vector2)transform.position - enemyRb.position).normalized;
                    enemyRb.AddForce(pullDirection * pullForce, ForceMode2D.Impulse);
                }
            }
        }

        Debug.Log($"[WeaponSystem] 牵引攻击：伤害{damage}，牵引力{pullForce}");
    }

    /// <summary>
    /// 共鸣加成计算（晶能武装 - 共鸣晶能）：攻击命中时场上每有1个友方召唤物伤害+10%，最多+30%
    /// </summary>
    private float ApplyResonanceBonus()
    {
        if (!hasCrystalCore || currentCrystalAspect != CrystalAspectType.ResonanceCrystal)
            return 1f;

        SummonSystem summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem == null) return 1f;

        int activeSummonCount = 0;
        var activeSummons = summonSystem.GetActiveSummons();
        if (activeSummons != null)
        {
            foreach (var s in activeSummons)
            {
                if (s != null)
                    activeSummonCount++;
            }
        }

        float bonus = 1f + Mathf.Min(activeSummonCount * 0.10f, 0.30f);
        Debug.Log($"[WeaponSystem] 共鸣加成：场上{activeSummonCount}个召唤物，伤害倍率{bonus:P0}");
        return bonus;
    }

    /// <summary>
    /// 收割判定（镰刀 - 收割镰刀）：对生命低于30%的敌人伤害+40%
    /// </summary>
    private float ApplyReaperBonus(float damage, EnemyAI enemy)
    {
        if (!hasCrystalCore || currentCrystalAspect != CrystalAspectType.ReaperScythe)
            return damage;

        if (enemy == null) return damage;

        HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
        if (enemyHealth == null) return damage;

        float healthPercent = enemyHealth.currentHealth / enemyHealth.maxHealth;
        if (healthPercent < 0.30f)
        {
            float bonusDamage = damage * 1.40f;
            Debug.Log($"[WeaponSystem] 收割触发：敌人生命{healthPercent:P0}，伤害+40% ({damage} -> {bonusDamage})");
            return bonusDamage;
        }

        return damage;
    }

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
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
                baseDamage = 25f; attackInterval = 0.833f; attackRange = 2f;
                damageType = DamageType.Physical; weaponElement = ElementType.None; break;
            case WeaponType.Staff:
                baseDamage = 20f; attackInterval = 1.25f; attackRange = 5f;
                damageType = DamageType.Magic; weaponElement = ElementType.Soul; break;
            case WeaponType.Scythe:
                baseDamage = 30f; attackInterval = 1.43f; attackRange = 2.5f;
                damageType = DamageType.Mixed; weaponElement = ElementType.Soul; break;
            case WeaponType.CrystalArm:
                baseDamage = 18f; attackInterval = 0.67f; attackRange = 3f;
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
        var ss = FindObjectOfType<SummonSystem>();
        if (ss != null)
            foreach (var s in ss.GetActiveSummons())
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
