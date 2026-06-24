using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss阶段枚举 — 4个难度阶段
/// </summary>
public enum BossPhase { Phase1, Phase2, Phase3, Enraged }

/// <summary>
/// Boss AI 基类 — 所有Boss的公共基类
/// 
/// 架构说明（v2重构）：
/// - BossAI 负责属性管理、阶段转换、生命值、接口实现
/// - BossCombatController（组件）负责行为状态机、攻击选择与执行
/// - 子类（如TimeGuardianBoss）负责专属技能和特殊机制
/// 
/// 接口兼容：
/// - IEnemyProvider: damage/isBerserk/EnterBerserkState/TakeDamage
/// - IBossPhaseProvider: currentPhase/UpdatePhase/OnPhaseTransition/AddAttackPattern/SwitchWeakness/ShowPhaseChangeEffect
/// </summary>
public class BossAI : MonoBehaviour, IEnemyProvider, IBossPhaseProvider
{
    // ═══════════════════════════════════════════
    // 接口实现 (IEnemyProvider)
    // ═══════════════════════════════════════════

    float IEnemyProvider.damage { get => damage; set => damage = value; }
    bool IEnemyProvider.isBerserk { get => isEnraged; set => isEnraged = value; }
    void IEnemyProvider.EnterBerserkState() { isEnraged = true; }
    void IEnemyProvider.TakeDamage(float amount) { TakeDamage(amount); }

    // ═══════════════════════════════════════════
    // 接口实现 (IBossPhaseProvider)
    // ═══════════════════════════════════════════

    BossPhase IBossPhaseProvider.currentPhase { get => currentPhase; set => currentPhase = value; }

    // ═══════════════════════════════════════════
    // BOSS属性
    // ═══════════════════════════════════════════

    [Header("BOSS属性")]
    [Tooltip("最大生命值")]
    public float maxHealth = 500f;

    [Tooltip("当前生命值")]
    public float currentHealth;

    [Tooltip("基础攻击力")]
    public float damage = 20f;

    [Tooltip("近战攻击范围")]
    public float attackRange = 3f;

    private List<ElementType> weaknesses = new List<ElementType>();
    private List<ElementType> resistances = new List<ElementType>();

    [Tooltip("追索范围（进入此范围开始追踪玩家）")]
    public float chaseRange = 15f;

    [Tooltip("基础移动速度")]
    public float moveSpeed = 3f;

    // ═══════════════════════════════════════════
    // BOSS阶段配置
    // ═══════════════════════════════════════════

    [Header("BOSS阶段")]
    [Tooltip("当前阶段")]
    public BossPhase currentPhase = BossPhase.Phase1;

    [Tooltip("阶段血量阈值（从高到低）：100%, 70%, 40%, 10%")]
    public float[] phaseHealthThresholds = { 1f, 0.70f, 0.40f, 0.10f };

    /// <summary>上一帧的阶段（用于检测阶段变化）</summary>
    private BossPhase lastFramePhase = BossPhase.Phase1;

    // ═══════════════════════════════════════════
    // 战斗控制器引用
    // ═══════════════════════════════════════════

    [Header("战斗控制器")]
    [Tooltip("行为状态机与攻击控制器（自动查找或手动指定）")]
    public BossCombatController combatController;

    // ═══════════════════════════════════════════
    // 兼容性：旧技能系统（保留供无CombatController时使用）
    // ═══════════════════════════════════════════

    [Header("技能设置（旧版兼容）")]
    [Tooltip("旧版技能列表（建议使用BossAttackPattern替代）")]
    public List<BossSkill> skills = new List<BossSkill>();

    [Tooltip("通用技能冷却")]
    public float skillCooldown = 2f;
    private float skillTimer = 0f;

    // ═══════════════════════════════════════════
    // 状态标志
    // ═══════════════════════════════════════════

    [Header("状态")]
    [Tooltip("是否处于狂暴阶段")]
    public bool isEnraged = false;

    [Tooltip("是否处于无敌状态（受击无效）")]
    public bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;

    /// <summary>Boss是否已被激活（用于延迟初始化）</summary>
    protected bool isActivated = false;

    // ═══════════════════════════════════════════
    // 引用
    // ═══════════════════════════════════════════

    [Header("引用")]
    public Rigidbody2D rb;
    public Transform player;
    public HealthSystem playerHealth;
    public Animator animator;
    public Collider2D bossCollider;

    // ═══════════════════════════════════════════
    // 视觉效果
    // ═══════════════════════════════════════════

    [Header("视觉效果")]
    [Tooltip("死亡特效预制件")]
    public GameObject deathEffect;

    [Tooltip("受击特效预制件")]
    public GameObject hitEffect;

    [Tooltip("阶段转换特效预制件")]
    public GameObject phaseChangeEffect;

    // ═══════════════════════════════════════════
    // 统计数据（用于调试和成就）
    // ═══════════════════════════════════════════

    [Header("统计数据（只读）")]
    private float battleStartTime;
    private float totalDamageTaken = 0f;
    private int hitCount = 0;
    public bool isDead = false;

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    protected virtual void Awake()
    {
        InitializeReferences();
        ResetHealth();
        InitializeSkills();
        FindCombatController();
    }

    protected virtual void Start()
    {
        lastFramePhase = currentPhase;
        battleStartTime = Time.time;

        // 延迟初始化CombatController（确保所有Awake完成）
        if (combatController != null)
            combatController.LateInitialize(this);
    }

    protected virtual void Update()
    {
        if (!isActivated || isDead) return;

        // 阶段检测与转换
        UpdatePhase();

        // 无敌计时器更新
        UpdateInvulnerability();

        // 旧版技能冷却更新（兼容模式）
        if (combatController == null)
            LegacyUpdateBehavior();
    }

    // ═══════════════════════════════════════════
    // 初始化方法
    // ═══════════════════════════════════════════

    /// <summary>初始化组件引用</summary>
    protected virtual void InitializeReferences()
    {
        rb = GetComponent<Rigidbody2D>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerHealth == null)
            playerHealth = FindObjectOfType<HealthSystem>();
        animator = GetComponent<Animator>();
        bossCollider = GetComponent<Collider2D>();
    }

    /// <summary>重置生命值到满</summary>
    protected virtual void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    /// <summary>初始化默认技能列表</summary>
    protected virtual void InitializeSkills()
    {
        if (skills.Count == 0)
        {
            skills.Add(new BossSkill("普通攻击", 1f, 0f, false));
            skills.Add(new BossSkill("范围攻击", 3f, 5f, true));
            skills.Add(new BossSkill("冲刺攻击", 2f, 3f, false));
        }
    }

    /// <summary>查找或创建CombatController</summary>
    void FindCombatController()
    {
        if (combatController == null)
            combatController = GetComponent<BossCombatController>();

        // 如果没有找到且需要使用新系统，给出警告但不强制要求
        // （允许纯BossAI模式运行以保持向后兼容）
    }

    /// <summary>激活Boss（由BossRoomManager调用）</summary>
    public virtual void Activate()
    {
        isActivated = true;
        battleStartTime = Time.time;
        totalDamageTaken = 0f;
        hitCount = 0;
        isDead = false;

        // 广播Boss遭遇事件
        GlobalEventManager.Instance.TriggerBossEncounter(gameObject);
        GlobalEventManager.Instance.TriggerBattleStart(gameObject);

        Debug.Log($"[BossAI] {gameObject.name} 被激活！当前阶段: {currentPhase}");
    }

    // ═══════════════════════════════════════════
    // 阶段管理系统 (IBossPhaseProvider)
    // ═══════════════════════════════════════════

    /// <summary>每帧检查并处理阶段转换</summary>
    public virtual void UpdatePhase()
    {
        if (maxHealth <= 0f) return;
        float healthPercent = currentHealth / maxHealth;

        // 从高到低检测阶段转换
        if (healthPercent <= phaseHealthThresholds[3] && !isEnraged)
        {
            EnterEnragedPhase();
        }
        else if (healthPercent <= phaseHealthThresholds[2] && currentPhase == BossPhase.Phase2)
        {
            EnterPhase3();
        }
        else if (healthPercent <= phaseHealthThresholds[1] && currentPhase == BossPhase.Phase1)
        {
            EnterPhase2();
        }

        // 检测阶段变化并通知CombatController
        if (currentPhase != lastFramePhase)
        {
            OnPhaseTransition(currentPhase);
            lastFramePhase = currentPhase;

            if (combatController != null)
                combatController.OnPhaseChanged(currentPhase);
        }
    }

    // ═══════════════════════════════════════════
    // 阶段转换方法
    // ═══════════════════════════════════════════

    protected virtual void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        Debug.Log($"[BossAI] 进入第二阶段！移动速度提升");
        ShowPhaseChangeEffect();
        moveSpeed *= 1.2f;
    }

    protected virtual void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;
        Debug.Log($"[BossAI] 进入第三阶段！全技能解锁");
        ShowPhaseChangeEffect();
        moveSpeed *= 1.3f;
    }

    protected virtual void EnterEnragedPhase()
    {
        currentPhase = BossPhase.Enraged;
        isEnraged = true;
        Debug.Log($"[BossAI] 进入狂暴阶段(Phase4)！");
        ShowPhaseChangeEffect();

        // 根据记忆碎片数量决定最终形态
        var mf = FindObjectOfType<MemoryFragmentSystem>();
        int collectedFragments = mf != null ? mf.GetFragmentCount() : 0;
        if (collectedFragments >= 7)
            EnableAllyMode();
        else
            EnableTrueForm();

        SwitchWeakness();
        StartMapShrink();
    }

    protected virtual void EnableAllyMode()
    {
        Debug.Log("[BossAI] 莎娜意识觉醒，协助玩家战斗！");
        damage *= 0.7f;
    }

    protected virtual void EnableTrueForm()
    {
        Debug.Log("[BossAI] 死灵圣王释放真正力量！");
        weaknesses.Clear();
        damage *= 1.5f;
    }

    // ═══════════════════════════════════════════
    // 无敌系统
    // ═══════════════════════════════════════════

    void UpdateInvulnerability()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }
    }

    /// <summary>设置无敌状态（持续指定时间）</summary>
    public virtual void SetInvulnerable(float duration)
    {
        isInvulnerable = true;
        invulnerabilityTimer = duration;
        Debug.Log($"[BossAI] 进入无敌状态，持续 {duration:F1}s");
    }

    // ═══════════════════════════════════════════
    // 受伤系统
    // ═══════════════════════════════════════════

    /// <summary>受到伤害 — 核心受伤入口</summary>
    public virtual void TakeDamage(float damageAmount)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damageAmount;
        totalDamageTaken += damageAmount;
        hitCount++;

        Debug.Log($"[BossAI] 受到 {damageAmount:F1} 点伤害，剩余HP: {currentHealth:F0}/{maxHealth:F0} ({currentHealth / maxHealth:P0})");

        // 显示受击效果
        SpawnHitEffect();

        // 播放受击动画
        PlayHitAnimation();

        // 广播伤害事件
        GlobalEventManager.Instance.TriggerDamageTaken(gameObject, damageAmount);

        // 通过CombatController触发硬直反应
        if (combatController != null && CanAct())
        {
            combatController.EnterStun(0.25f);
        }

        // 检查死亡
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>检查Boss是否能行动（非硬直/非攻击中）</summary>
    public virtual bool CanAct()
    {
        if (combatController != null)
            return combatController.CanAct;
        return true; // 无CombatController时默认可行动
    }

    void SpawnHitEffect()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
    }

    void PlayHitAnimation()
    {
        if (animator != null)
            animator.SetTrigger("hit"); // 或 "IsHitHit" 取决于Animator Controller
    }

    // ═══════════════════════════════════════════
    // 死亡系统
    // ═══════════════════════════════════════════

    /// <summary>死亡处理</summary>
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        LogBattleStatistics();

        // 禁用碰撞体
        if (bossCollider != null)
            bossCollider.enabled = false;

        // 停止移动
        if (rb != null)
            rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // 播放死亡动画
        if (animator != null)
            animator.SetTrigger("die");

        // 生成死亡特效
        SpawnDeathEffect();

        // 播放死亡音效
        GlobalEventManager.Instance.RequestAudio("PlayBossDefeat");

        // 触发Boss击败事件（BossRoomManager监听此事件生成奖励）
        GlobalEventManager.Instance.TriggerBossDefeated(gameObject);
        GlobalEventManager.Instance.TriggerBattleEnd(gameObject);
        GlobalEventManager.Instance.TriggerEntityDied(gameObject);

        // 延迟销毁
        Destroy(gameObject, 2f);
    }

    void SpawnDeathEffect()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
    }

    /// <summary>记录战斗统计（调试用）</summary>
    void LogBattleStatistics()
    {
        float duration = Time.time - battleStartTime;
        Debug.Log($"========== Boss战统计 ==========");
        Debug.Log($"  Boss: {gameObject.name}");
        Debug.Log($"  战斗时长: {duration:F1}s");
        Debug.Log($"  总承受伤害: {totalDamageTaken:F0}");
        Debug.Log($"  受击次数: {hitCount}");
        Debug.Log($"  最终阶段: {currentPhase}");
        Debug.Log($"  DPS(承伤): {totalDamageTaken / Mathf.Max(duration, 1f):F1}/s");
        Debug.Log("===============================");
    }

    // ═══════════════════════════════════════════
    // 旧版行为系统（兼容模式 — 无CombatController时使用）
    // ═══════════════════════════════════════════

    void LegacyUpdateBehavior()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Phase1Behavior(); break;
            case BossPhase.Phase2:
                Phase2Behavior(); break;
            case BossPhase.Phase3:
                Phase3Behavior(); break;
            case BossPhase.Enraged:
                EnragedBehavior(); break;
        }

        // 技能冷却计时
        if (skillTimer > 0f)
            skillTimer -= Time.deltaTime;
    }

    void Phase1Behavior() { ChasePlayer(); TryMeleeAttack(); }
    void Phase2Behavior() { ChasePlayer(); TryRandomSkill(); }
    void Phase3Behavior() { ChasePlayer(true); TryPreferSpecialSkill(); }
    void EnragedBehavior() { ChasePlayer(true); TryRandomSkill(); }

    void ChasePlayer(bool fast = false)
    {
        if (player == null || rb == null) return;
        float speed = fast ? moveSpeed * 1.5f : moveSpeed;
        Vector2 direction = ((Vector2)(player.position - transform.position)).normalized;
        rb.velocity = direction * speed;
    }

    void TryMeleeAttack()
    {
        if (skillTimer > 0f || !IsPlayerInRange(attackRange)) return;
        UseSkill(0);
    }

    void TryRandomSkill()
    {
        if (skillTimer > 0f) return;
        int idx = Random.Range(0, skills.Count);
        UseSkill(idx);
    }

    void TryPreferSpecialSkill()
    {
        if (skillTimer > 0f) return;
        int idx = Random.Range(1, Mathf.Max(1, skills.Count));
        UseSkill(idx);
    }

    void UseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count) return;
        BossSkill skill = skills[skillIndex];
        Debug.Log($"[BossAI](Legacy) 使用技能: {skill.skillName}");

        if (animator != null)
            animator.SetTrigger("skill" + skillIndex);

        ApplySkillEffect(skill);
        skillTimer = skill.cooldown > 0 ? skill.cooldown : skillCooldown;
    }

    void ApplySkillEffect(BossSkill skill)
    {
        switch (skill.skillName)
        {
            case "普通攻击": LegacyMeleeAttack(); break;
            case "范围攻击": LegacyAreaAttack(); break;
            case "冲刺攻击": LegacyDashAttack(); break;
            default: LegacyMeleeAttack(); break;
        }
    }

    void LegacyMeleeAttack()
    {
        if (playerHealth != null && IsPlayerInRange(attackRange))
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"[BossAI](Legacy) 普通攻击 造成 {damage} 伤害");
        }
    }

    void LegacyAreaAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (var c in hits)
        {
            if (!c.CompareTag("Player")) continue;
            var health = c.GetComponent<HealthSystem>();
            if (health != null) health.TakeDamage(damage * 1.5f);
        }
    }

    void LegacyDashAttack()
    {
        if (player != null && rb != null)
        {
            Vector2 dir = ((Vector2)(player.position - transform.position)).normalized;
            rb.velocity = dir * moveSpeed * 3f;
            StartCoroutine(LegacyDashEndCoroutine());
        }
    }

    IEnumerator LegacyDashEndCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (rb != null) rb.velocity = Vector2.zero;
    }

    bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    // ═══════════════════════════════════════════
    // IBossPhaseProvider 接口实现
    // ═══════════════════════════════════════════

    /// <summary>阶段转换回调</summary>
    public virtual void OnPhaseTransition(BossPhase newPhase)
    {
        Debug.Log($"[BossAI] 阶段转换: {currentPhase} -> {newPhase}");
        ShowPhaseChangeEffect();
    }

    /// <summary>添加攻击模式</summary>
    public virtual void AddAttackPattern(string patternID)
    {
        Debug.Log($"[BossAI] 新增攻击模式: {patternID}");
        // 由子类或CombatController具体实现
    }

    /// <summary>切换弱点属性</summary>
    public virtual void SwitchWeakness()
    {
        if (weaknesses.Count > 0)
        {
            weaknesses[0] = (ElementType)Random.Range(0, 7);
            Debug.Log($"[BossAI] 弱点切换为: {weaknesses[0]}");
        }
    }

    /// <summary>显示阶段转换特效</summary>
    public virtual void ShowPhaseChangeEffect()
    {
        if (phaseChangeEffect != null)
            Instantiate(phaseChangeEffect, transform.position, Quaternion.identity);
    }

    // ═══════════════════════════════════════════
    // 地图缩小（S-SN最终战专用）
    // ═══════════════════════════════════════════

    protected virtual void StartMapShrink()
    {
        var map = FindObjectOfType<IntegratedMapSystem>();
        if (map != null) map.SetCycle(map.currentCycle);
        Debug.Log("[BossAI] 地图开始缩小！");
    }

    // ═══════════════════════════════════════════
    // 公共工具方法
    // ═══════════════════════════════════════════

    /// <summary>获取当前血量百分比</summary>
    public float GetHealthPercent() => maxHealth > 0f ? currentHealth / maxHealth : 0f;

    /// <summary>获取方向向量（指向玩家）</summary>
    public Vector2 GetDirectionToPlayer()
    {
        if (player == null) return Vector2.down;
        return ((Vector2)(player.position - transform.position)).normalized;
    }

    /// <summary>获取到玩家的距离</summary>
    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    // ═══════════════════════════════════════════
    // 调试方法
    // ═══════════════════════════════════════════

    /// <summary>输出详细Boss状态</summary>
    public virtual void DebugPrintBossStatus()
    {
        Debug.Log("========== Boss Status ==========");
        Debug.Log($"  Name: {gameObject.name}");
        Debug.Log($"  HP: {currentHealth:F0}/{maxHealth:F0} ({GetHealthPercent():P0})");
        Debug.Log($"  Phase: {currentPhase}");
        DebugLog($"  Enraged: {isEnraged}");
        DebugLog($"  Invuln: {isInvulnerable} ({invulnerabilityTimer:F1}s)");
        DebugLog($"  Speed: {moveSpeed}");
        DebugLog($"  Damage: {damage}");
        DebugLog($"  CombatController: {(combatController != null ? combatController.currentState.ToString() : "None")}");
        Debug.Log("==================================");
    }

    void DebugLog(string msg)
    {
        // 仅在Debug模式下输出详细信息
#if UNITY_EDITOR
        Debug.Log(msg);
#endif
    }

    // ═══════════════════════════════════════════
    // Gizmos 绘制
    // ═══════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        // 攻击范围（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 追击范围（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 旧版技能范围（蓝色）
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        foreach (var skill in skills)
        {
            if (skill.areaEffect)
                Gizmos.DrawWireSphere(transform.position, skill.range);
        }
    }
}

// ════════════════════════════════════════════════════════════════════════
// 旧版技能数据结构（保留向后兼容）
// ════════════════════════════════════════════════════════════════════════

[System.Serializable]
public class BossSkill
{
    public string skillName;
    public float damage;
    public float range;
    public bool areaEffect;
    public float cooldown = 2f;

    public BossSkill(string name, float dmg, float rng, bool area)
    {
        skillName = name;
        damage = dmg;
        range = rng;
        areaEffect = area;
    }
}
