using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    public enum WeaponType { Sword, Staff, Scythe, CrystalArm }
    public enum DamageType { Physical, Magic, Mixed }
    public enum ElementType { None, Frost, Water, Fire, Lightning, Soul, Holy }

    [Header("武器类型")]
    public WeaponType currentWeaponType;

    [Header("武器属性")]
    public float baseDamage = 20f;
    [HideInInspector]
    public float attackInterval = 0.833f;
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

        // 获取当前攻击间隔值
        float currentAttackInterval = useCustomAttackInterval ? customAttackInterval : attackInterval;
        
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
            ExecuteAttack(GetCurrentDamage());
        }

        PlayAttackSound();
        TriggerAttackAnimation();
    }

    void SecondaryAttack()
    {
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
                PrecisionStrike();
                break;
            case WeaponType.Staff:
                SpellAmplify();
                break;
            case WeaponType.Scythe:
                SoulHarvest();
                break;
            case WeaponType.CrystalArm:
                EnergyBurst();
                break;
        }

        PlayAttackSound();
        TriggerAttackAnimation();
    }

    void ExecuteAttack(float damage)
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

    float GetCurrentDamage()
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

        // 应用弱点抗性
        if (enemy.weaknesses.Contains(ConvertElementType(weaponElement)))
        {
            multiplier *= 1.5f;
            Debug.Log($"击中弱点！伤害x1.5");
        }
        else if (enemy.resistances.Contains(ConvertElementType(weaponElement)))
        {
            multiplier *= 0.8f;
            Debug.Log($"击中抗性！伤害x0.8");
        }

        // 应用元素克制链
        if (IsElementalChain(weaponElement, ConvertEnemyElementType(enemy.elementType)))
        {
            multiplier *= 1.2f;
            Debug.Log($"元素克制链！额外伤害x1.2");
        }

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
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
                baseDamage = 25f;
                attackInterval = 0.833f;
                attackRange = 2f;
                damageType = DamageType.Physical;
                weaponElement = ElementType.None;
                break;
            case WeaponType.Staff:
                baseDamage = 20f;
                attackInterval = 1.25f;
                attackRange = 5f;
                damageType = DamageType.Magic;
                weaponElement = ElementType.Soul;
                break;
            case WeaponType.Scythe:
                baseDamage = 30f;
                attackInterval = 1.43f;
                attackRange = 2.5f;
                damageType = DamageType.Mixed;
                weaponElement = ElementType.Soul;
                break;
            case WeaponType.CrystalArm:
                baseDamage = 18f;
                attackInterval = 0.67f;
                attackRange = 3f;
                damageType = DamageType.Mixed;
                weaponElement = ElementType.Frost;
                break;
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

    public bool EnhanceWeapon()
    {
        if (enhancementLevel >= maxEnhancementLevel)
        {
            Debug.Log("已达最大强化等级！");
            return false;
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
