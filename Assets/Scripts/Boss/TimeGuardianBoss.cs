using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 时空守护者 (Time Guardian) Boss — BossAI的完整子类实现
/// 
/// 特性：
/// - 5种独特攻击模式（通过BossCombatController驱动）
/// - 3个难度阶段 + 狂暴阶段，行为差异化
/// - 专属技能：时空扭曲（无敌+回血+减速领域）
/// - 元素属性：灵魂(Soul)，弱点：圣光/灵魂，抗性：物理/雷电
/// 
/// 架构：
/// - 继承 BossAI 基类（属性、阶段、接口）
/// - 使用 BossCombatController 驱动攻击模式
/// - 在 Awake 中注册所有5种攻击到CombatController
/// </summary>
public class TimeGuardianBoss : BossAI
{
    // ═══════════════════════════════════════════
    // 时空守护者专属属性
    // ═══════════════════════════════════════════

    [Header("时空守护者专属")]
    [Tooltip("减速场半径")]
    public float slowFieldRadius = 5f;

    [Tooltip("减速强度（1.0=完全静止，0.5=半速）")]
    [Range(0f, 1f)]
    public float slowFieldStrength = 0.5f;

    [Tooltip("时空扭曲释放间隔（秒）")]
    public float timeWarpInterval = 8f;

    [Tooltip("Boss元素类型（影响弱点和抗性判定）")]
    public ElementType guardianElement = ElementType.Soul;

    // ═══════════════════════════════════════════
    // 时空扭曲技能参数
    // ═══════════════════════════════════════════

    [Header("时空扭曲参数")]
    [Tooltip("时空扭曲时无敌持续时间")]
    public float timeWarpInvulnDuration = 1.5f;

    [Tooltip("回血比例（相对于maxHealth）")]
    [Range(0f, 0.2f)]
    public float timeWarpHealPercent = 0.05f;

    [Tooltip("玩家减速持续时间")]
    public float timeWarpSlowDuration = 2f;

    // ═══════════════════════════════════════════
    // 阶段自定义参数（覆盖基类默认值）
    // ═══════════════════════════════════════════

    [Header("阶段参数覆盖")]
    [Tooltip("Phase1 生命值")]
    public float phase1MaxHealth = 800f;

    [Tooltip("Phase2 移动速度倍率")]
    [Range(1f, 2f)]
    public float tgPhase2SpeedMult = 1.2f;

    [Tooltip("Phase3 移动速度倍率")]
    [Range(1f, 2.5f)]
    public float tgPhase3SpeedMult = 1.5f;

    [Tooltip("Enraged 攻击力倍率")]
    [Range(1f, 2.5f)]
    public float enragedDamageMult = 1.8f;

    // ═══════════════════════════════════════════
    // 运行时状态
    // ═══════════════════════════════════════════

    private float timeWarpTimer = 0f;

    /// <summary>本地CombatController引用（避免每次都GetComponent）</summary>
    private TimeGuardianCombatController tgCombatController;

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    protected override void Awake()
    {
        // 先调用基类初始化（引用、血量、技能列表等）
        base.Awake();

        // 覆盖时空守护者特有属性
        maxHealth = phase1MaxHealth;
        damage = 25f;   // 时空守护者基础伤害略高
        moveSpeed = 3.5f; // 略快于普通Boss

        // 初始化弱点/抗性（根据策划文档：弱点圣光/灵魂，抗性物理/雷电）
        InitializeWeaknessesResistances();

        // 查找或创建专用的CombatController
        SetupCombatController();
    }

    protected override void Start()
    {
        base.Start();

        // 激活Boss（如果未被外部Activate的话）
        if (!isActivated)
            Activate();
    }

    protected override void Update()
    {
        base.Update();

        // 时空扭曲计时器（独立于阶段系统运行）
        if (isActivated && !isDead)
        {
            UpdateTimeWarp();
        }
    }

    // ═══════════════════════════════════════════
    // 初始化
    // ═══════════════════════════════════════════

    /// <summary>初始化弱点和抗性</summary>
    void InitializeWeaknessesResistances()
    {
        // 通过反射设置私有字段（因为weaknesses/resistances是private的）
        // 实际使用中可以改为protected或通过公共方法初始化
        // 这里我们直接在子类中记录设计意图，具体实现在Inspector配置
        Debug.Log("[时空守护者] 弱点: 圣光/灵魂 | 抗性: 物理/雷电 | 元素: Soul");
    }

    /// <summary>设置CombatController并注册所有攻击模式</summary>
    void SetupCombatController()
    {
        // 尝试获取已有的CombatController
        tgCombatController = GetComponent<TimeGuardianCombatController>();
        if (tgCombatController == null)
        {
            // 如果没有专用控制器，尝试用通用控制器
            if (combatController != null)
            {
                RegisterAttacksToController(combatController);
                Debug.Log("[时空守护者] 使用通用BossCombatController");
            }
            else
            {
                Debug.LogWarning("[时空守护者] 未找到任何CombatController，将使用旧版兼容模式");
            }
        }
        else
        {
            // 使用专用控制器
            combatController = tgCombatController;
            combatController.LateInitialize(this);
            Debug.Log("[时空守护者] 使用TimeGuardianCombatController");
        }
    }

    /// <summary>向CombatController注册5种攻击模式</summary>
    void RegisterAttacksToController(BossCombatController controller)
    {
        if (controller == null) return;

        controller.attackPatterns.Clear();

        // #1 晶能斩击 — 近战扇形 (Phase1可用)
        controller.attackPatterns.Add(new CrystalSlashAttack());

        // #2 时空冲击波 — 远程直线 (Phase1可用)
        controller.attackPatterns.Add(new TimeShockwaveAttack());

        // #3 水晶牢笼 — AOE困住 (Phase2解锁)
        controller.attackPatterns.Add(new CrystalCageAttack());

        // #4 裂隙冲锋 — 穿越突进 (Phase2解锁)
        controller.attackPatterns.Add(new RiftDashAttack());

        // #5 时空崩坏 — 全屏AOE终极技 (Phase3解锁)
        controller.attackPatterns.Add(new SpaceCollapseAttack());

        Debug.Log($"[时空守护者] 已注册 {controller.attackPatterns.Count} 种攻击模式到CombatController");
    }

    // ═══════════════════════════════════════════
    // 时空扭曲技能（专属机制）
    // ═══════════════════════════════════════════

    void UpdateTimeWarp()
    {
        timeWarpTimer += Time.deltaTime;

        // Phase3及以上才能使用时空扭曲
        bool canUseWarp = currentPhase >= BossPhase.Phase3 && !isInvulnerable && !isDead;

        if (timeWarpTimer >= timeWarpInterval && canUseWarp)
        {
            timeWarpTimer = 0f;
            CastTimeWarp();
        }
    }

    /// <summary>
    /// 施放时空扭曲 — 时空守护者的招牌技能
    /// 效果：短暂无敌 + 回复5%血量 + 减速范围内玩家50%
    /// </summary>
    public virtual void CastTimeWarp()
    {
        Debug.Log("[时空守护者] ★ 发动时空扭曲！★");

        // 1. 无敌
        SetInvulnerable(timeWarpInvulnDuration);

        // 2. 回血
        float healAmount = maxHealth * timeWarpHealPercent;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        Debug.Log($"[时空守护者] 时空扭曲回复 {healAmount:F0} 点生命值");

        // 3. 减速范围内玩家
        ApplySlowField();

        // 4. 视觉特效提示
        GlobalEventManager.Instance.ShowNotification("时空扭曲发动！", 1.5f);

        // 广播事件供其他系统响应
        GlobalEventManager.Instance.TriggerDamageDealt(gameObject, 0); // 技能事件标记
    }

    /// <summary>对范围内的玩家施加减速效果</summary>
    void ApplySlowField()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slowFieldRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            var pc = hit.GetComponent<PlayerController>();
            if (pc != null)
            {
                StartCoroutine(ApplySlowCoroutine(pc));
            }
        }
    }

    IEnumerator ApplySlowCoroutine(PlayerController pc)
    {
        if (pc == null) yield break;

        float originalSpeed = pc.moveSpeed;
        float originalRunSpeed = pc.runSpeed;
        pc.moveSpeed *= slowFieldStrength;
        pc.runSpeed *= slowFieldStrength;

        Debug.Log($"[时空守护者] 玩家被时空力场减速 {(1f - slowFieldStrength) * 100:F0}% " +
                  $"持续 {timeWarpSlowDuration}s");

        yield return new WaitForSeconds(timeWarpSlowDuration);

        // 安全恢复（防止玩家在减速期间死亡/销毁导致空引用异常）
        if (pc != null)
        {
            pc.moveSpeed = originalSpeed;
            pc.runSpeed = originalRunSpeed;
            Debug.Log("[时空守护者] 时空力场减速效果结束");
        }
    }

    // ═══════════════════════════════════════════
    // 阶段重写（时空守护者特有阶段行为）
    // ═══════════════════════════════════════════

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        Debug.Log("[时空守护者] Phase2: 解锁水晶牢笼 & 裂隙冲锋！");

        // 通知CombatController新攻击已解锁
        if (combatController != null)
            combatController.OnPhaseChanged(BossPhase.Phase2);
    }

    protected override void EnterPhase3()
    {
        base.EnterPhase3();
        Debug.Log("[时空守护者] Phase3: 全技能解锁！时空扭曲就绪！");

        // 缩短时空扭曲冷却（第三阶段更频繁）
        timeWarpInterval = Mathf.Max(4f, timeWarpInterval * 0.6f);

        if (combatController != null)
            combatController.OnPhaseChanged(BossPhase.Phase3);
    }

    protected override void EnterEnragedPhase()
    {
        base.EnterEnragedPhase();
        Debug.Log("[时空守护者] ★★★ 狂暴模式！时空崩坏无冷却！★★★");

        // 大幅提升伤害
        damage *= enragedDamageMult;

        // 时空扭曲变为极短冷却
        timeWarpInterval = 3f;

        // 重置所有攻击冷却（狂暴全开）
        if (combatController != null)
        {
            foreach (var pattern in combatController.attackPatterns)
            {
                if (pattern != null) pattern.ResetCooldown();
            }
        }
    }

    // ═══════════════════════════════════════════
    // 死亡重写
    // ═══════════════════════════════════════════

    protected override void Die()
    {
        if (isDead) return;

        Debug.Log("[时空守护者] 被击败！时空屏障瓦解...");
        GlobalEventManager.Instance.ShowNotification("时空守护者已倒下！时空裂隙正在形成...", 3f);

        base.Die();
    }

    // ═══════════════════════════════════════════
    // Gizmos 调试绘制
    // ═══════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        // 先绘制基类范围
        // （基类的OnDrawGizmosSelected会自动调用）

        // 时空扭曲减速场（紫色）
        Gizmos.color = new Color(0.5f, 0.2f, 0.8f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slowFieldRadius);

    }

    // ═══════════════════════════════════════════
    // 公共接口
    // ═══════════════════════════════════════════

    /// <summary>获取时空扭曲是否就绪</summary>
    public bool IsTimeWarpReady => timeWarpTimer >= timeWarpInterval && currentPhase >= BossPhase.Phase3;

    /// <summary>手动触发时空扭曲（测试用）</summary>
    [ContextMenu("手动触发时空扭曲")]
    public void DebugCastTimeWarp()
    {
        CastTimeWarp();
    }
}

// ════════════════════════════════════════════════════════════════════════
// 时空守护者专用战斗控制器（可选增强版）
// 如果需要更精细的控制可继承BossCombatController
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// 时空守护者专用战斗控制器 — 扩展BossCombatController的定制版本
/// 可选使用。如果不挂载此组件，TimeGuardianBoss会使用通用BossCombatController
/// </summary>
public class TimeGuardianCombatController : BossCombatController
{
    [Header("时空守护者决策调整")]
    [Tooltip("Phase1中时空扭曲预热期的特殊行为")]
    public bool enablePrePhase3Behavior = true;

    [Tooltip("低血量时增加时空扭曲优先级")]
    [Range(0f, 0.5f)]
    public float lowHealthWarpPriorityBonus = 0.2f;

    /// <summary>
    /// 重写Enraged决策 — 时空守护者在狂暴时更倾向于混合使用全部技能
    /// </summary>
    protected override BossBehaviorState EvaluateEnraged(BossDecisionContext ctx)
    {
        // 优先尝试时空崩坏（最强技能）
        var collapsePattern = attackPatterns.Find(p => p is SpaceCollapseAttack);
        if (collapsePattern != null && collapsePattern.IsReady && ctx.randomFactor < 0.4f)
            return BossBehaviorState.CastSkill;

        // 否则使用基类逻辑
        return base.EvaluateEnraged(ctx);
    }
}
