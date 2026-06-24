using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss战斗控制器 — 协调AI决策、攻击执行与状态管理的核心组件
/// 
/// 职责：
/// 1. 维护行为状态机（BossBehaviorState）
/// 2. 每帧基于决策上下文（BossDecisionContext）进行动态决策
/// 3. 管理攻击模式列表，执行攻击协程
/// 4. 处理阶段转换时的状态变更
/// 
/// 使用方式：挂载到Boss GameObject上，与 BossAI 基类配合工作
/// </summary>
public class BossCombatController : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // 引用
    // ═══════════════════════════════════════════

    [Header("Boss引用")]
    [Tooltip("引用的BossAI基类组件")]
    public BossAI bossAI;

    // ═══════════════════════════════════════════
    // 行为状态机
    // ═══════════════════════════════════════════

    [Header("行为状态")]
    [Tooltip("当前行为状态")]
    public BossBehaviorState currentState = BossBehaviorState.Patrol;

    private BossBehaviorState previousState = BossBehaviorState.Patrol;

    /// <summary>状态持续时间</summary>
    private float stateTimer = 0f;

    /// <summary>硬直剩余时间</summary>
    private float stunnedTimer = 0f;

    // ═══════════════════════════════════════════
    // 攻击管理
    // ═══════════════════════════════════════════

    [Header("攻击模式列表")]
    [Tooltip("所有可用的攻击模式（在Inspector或代码中配置）")]
    public List<BossAttackPattern> attackPatterns = new List<BossAttackPattern>();

    // ═══════════════════════════════════════════
    // 攻击触发器（参考 EnemyCombatComponent 模式）
    // ═══════════════════════════════════════════

    [Header("攻击触发器")]
    [Tooltip("Boss攻击触发器预制件（AttackTrigger + BoxCollider2D）")]
    public GameObject bossAttackTriggerPrefab;

    [Tooltip("攻击触发器生成位置（通常为Boss正前方）")]
    public Transform attackTriggerPos;

    /// <summary>当前正在执行的攻击</summary>
    private BossAttackPattern currentExecutingAttack = null;

    /// <summary>各攻击模式的冷却计时器</summary>
    private float[] attackCooldowns;

    // ═══════════════════════════════════════════
    // 决策参数
    // ═══════════════════════════════════════════

    [Header("决策参数")]
    [Tooltip("巡逻时移动速度比例（相对baseMoveSpeed）")]
    [Range(0.1f, 1f)]
    public float patrolSpeedRatio = 0.3f;

    [Tooltip("追击时移动速度比例")]
    [Range(0.5f, 1.5f)]
    public float chaseSpeedRatio = 1f;

    [Tooltip("攻击距离阈值（小于此距离进入Attack状态）")]
    public float attackDistanceThreshold = 2f;

    [Tooltip("追击距离阈值（小于此距离进入Chase状态）")]
    public float chaseDistanceThreshold = 6f;

    [Tooltip("警戒距离阈值（小于此距离进入Alert状态）")]
    public float alertDistanceThreshold = 8f;

    [Tooltip("硬直时间（受击后/攻击后摇结束后的最小硬直）")]
    public float stunDurationAfterHit = 0.3f;

    [Tooltip("防御状态持续时间")]
    public float defendDuration = 0.8f;

    [Tooltip("防御减伤比例（1.0=完全格挡）")]
    [Range(0f, 1f)]
    public float defenseDamageReduction = 0.5f;

    [Tooltip("阶段转换后短暂无敌时间")]
    public float phaseTransitionInvulnDuration = 1.5f;

    // ═══════════════════════════════════════════
    // 阶段行为参数
    // ═══════════════════════════════════════════

    [Header("阶段行为参数")]
    [Tooltip("Phase1 技能使用概率(0~1) — 越低越倾向普攻")]
    [Range(0f, 1f)]
    public float phase1SkillChance = 0.05f;

    [Tooltip("Phase2 移动速度倍率")]
    [Range(1f, 2f)]
    public float phase2SpeedMultiplier = 1.2f;

    [Tooltip("Phase2 技能使用概率")]
    [Range(0f, 1f)]
    public float phase2SkillChance = 0.5f;

    [Tooltip("Phase3 移动速度倍率")]
    [Range(1f, 2.5f)]
    public float phase3SpeedMultiplier = 1.5f;

    [Tooltip("Phase3 技能使用概率")]
    [Range(0f, 1f)]
    public float phase3SkillChance = 0.7f;

    [Tooltip("Phase3 防御触发概率")]
    [Range(0f, 0.5f)]
    public float phase3DefendChance = 0.15f;

    [Tooltip("Enraged 冷却缩减比例（实际冷却 *= 此值）")]
    [Range(0f, 1f)]
    public float enragedCooldownMultiplier = 0.3f;

    [Tooltip("Enraged 技能使用概率（几乎总是放技能）")]
    [Range(0f, 1f)]
    public float enragedSkillChance = 0.95f;

    // ═══════════════════════════════════════════
    // 调试设置
    // ═══════════════════════════════════════════

    [Header("调试设置")]
    [Tooltip("启用详细调试日志输出")]
    public bool enableDebugLog = false;

    [Tooltip("Gizmos显示攻击范围")]
    public bool showDebugGizmos = true;

    // ═══════════════════════════════════════════
    // 运行时状态
    // ═══════════════════════════════════════════

    private float statusLogTimer = 0f;
    private float postAttackCooldown = 0f; // 攻击后冷却，期间不可再次攻击

    /// <summary>是否正在执行攻击（涵盖 Attack/CastSkill/Enraged 所有攻击状态）</summary>
    public bool IsAttacking => currentExecutingAttack != null;

    /// <summary>是否处于硬直</summary>
    public bool IsStunned => currentState == BossBehaviorState.Stunned;

    /// <summary>是否可以行动（非攻击中/非硬直/非施法中）</summary>
    public bool CanAct => !IsAttacking && !IsStunned && currentState != BossBehaviorState.CastSkill;

    /// <summary>当前有效移动速度（含阶段加成）</summary>
    public float CurrentEffectiveSpeed
    {
        get
        {
            if (bossAI == null) return 1f;
            float baseSpeed = bossAI.moveSpeed * chaseSpeedRatio;
            switch (bossAI.currentPhase)
            {
                case BossPhase.Phase2: return baseSpeed * phase2SpeedMultiplier;
                case BossPhase.Phase3: return baseSpeed * phase3SpeedMultiplier;
                case BossPhase.Enraged: return baseSpeed * phase3SpeedMultiplier * 1.2f;
                default: return baseSpeed;
            }
        }
    }

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    void Awake()
    {
        InitializeAttackPatterns();
    }

    void Start()
    {
        if (bossAI == null)
            bossAI = GetComponent<BossAI>();

        Debug.LogWarning($"[BossCombatController] 初始化完成 bossAI={bossAI?.name} 状态={currentState} 攻击数量={attackPatterns.Count}");
    }

    void Update()
    {
        if (bossAI == null || !bossAI.isActivated || bossAI.isDead) return;

        if (postAttackCooldown > 0f) postAttackCooldown -= Time.deltaTime;

        UpdateCooldowns();
        UpdateStunTimer();

        if (!CanAct) return;

        // 构建决策上下文
        var context = BuildDecisionContext();

        // 执行动态决策
        BossBehaviorState newState = EvaluateDecision(context);

        // 状态切换处理
        if (newState != currentState)
        {
            TransitionToState(newState, context);
        }

        // 执行当前状态行为
        ExecuteCurrentState(context);

        // attackTriggerPos 追踪玩家位置（每帧更新，确保触发器命中）
        UpdateAttackTriggerPos();

        // 每3秒输出状态日志
        statusLogTimer += Time.deltaTime;
        if (statusLogTimer >= 3f)
        {
            statusLogTimer = 0f;
            float dist = bossAI.player != null ? Vector3.Distance(transform.position, bossAI.player.position) : -1f;
            Debug.LogWarning($"[BossCombatController] 状态={currentState} 距离={dist:F1}m bossPos={transform.position} playerPos={bossAI.player?.position} phase={bossAI.currentPhase}");
        }
    }

    /// <summary>attackTriggerPos 追踪玩家 — 攻击期间冻结，玩家可躲避</summary>
    void UpdateAttackTriggerPos()
    {
        if (attackTriggerPos == null || bossAI == null || bossAI.player == null) return;
        if (IsAttacking) return; // 攻击中不追踪，位置锁定
        float dist = Vector3.Distance(transform.position, bossAI.player.position);
        if (dist <= chaseDistanceThreshold)
            attackTriggerPos.position = bossAI.player.position;
    }

    // ═══════════════════════════════════════════
    // 初始化
    // ═══════════════════════════════════════════

    /// <summary>
    /// 初始化攻击模式列表 — 为每个攻击设置Owner引用
    /// </summary>
    void InitializeAttackPatterns()
    {
        attackCooldowns = new float[attackPatterns.Count];
        for (int i = 0; i < attackPatterns.Count; i++)
        {
            // Owner在Start中设置（因为BossAI可能还未Awake）
            attackCooldowns[i] = 0f;
        }
    }

    /// <summary>
    /// 在BossAI Start之后调用，确保Owner已就绪
    /// </summary>
    public void LateInitialize(BossAI owner)
    {
        bossAI = owner;
        for (int i = 0; i < attackPatterns.Count; i++)
        {
            if (attackPatterns[i] != null)
                attackPatterns[i].SetOwner(owner);
        }
        if (attackCooldowns == null || attackCooldowns.Length != attackPatterns.Count)
            attackCooldowns = new float[attackPatterns.Count];
    }

    // ═══════════════════════════════════════════
    // 每帧更新
    // ═══════════════════════════════════════════

    void UpdateCooldowns()
    {
        // 更新攻击模式冷却
        for (int i = 0; i < attackPatterns.Count; i++)
        {
            if (attackPatterns[i] != null)
            {
                attackPatterns[i].UpdateCooldown(Time.deltaTime);
                // 同步到本地数组用于决策上下文
                attackCooldowns[i] = GetEffectiveCooldown(i);
            }
        }
    }

    void UpdateStunTimer()
    {
        if (currentState == BossBehaviorState.Stunned)
        {
            stunnedTimer -= Time.deltaTime;
            if (stunnedTimer <= 0f)
            {
                ChangeState(BossBehaviorState.Chase); // 硬直结束后默认追击
            }
        }
    }

    // ═══════════════════════════════════════════
    // 决策系统核心
    // ═══════════════════════════════════════════

    /// <summary>
    /// 构建当前帧的决策上下文
    /// </summary>
    BossDecisionContext BuildDecisionContext()
    {
        return BossDecisionContext.Create(
            bossAI.transform,
            bossAI.player,
            bossAI.currentHealth,
            bossAI.maxHealth,
            bossAI.currentPhase,
            currentState,
            attackCooldowns,
            bossAI.isInvulnerable,
            IsStunned
        );
    }

    /// <summary>
    /// 动态决策评估 — 基于多因素加权选择下一个行为状态
    /// 这是整个Boss AI的核心大脑
    /// </summary>
    BossBehaviorState EvaluateDecision(BossDecisionContext ctx)
    {
        if (ctx.isStunned) return BossBehaviorState.Stunned;
        if (ctx.isInvulnerable) return BossBehaviorState.Chase;
        if (IsAttacking) return currentState;

        // === 攻击后冷却：强制追击2秒，确保Boss会移动 ===
        if (postAttackCooldown > 0f)
            return BossBehaviorState.Chase;

        // === 根据阶段和距离动态决策 ===
        switch (ctx.currentPhase)
        {
            case BossPhase.Phase1:
                return EvaluatePhase1(ctx);
            case BossPhase.Phase2:
                return EvaluatePhase2(ctx);
            case BossPhase.Phase3:
                return EvaluatePhase3(ctx);
            case BossPhase.Enraged:
                return EvaluateEnraged(ctx);
            default:
                return BossBehaviorState.Patrol;
        }
    }

    /// <summary>Phase1决策: 以 Patrol + Chase + 基础攻击为主</summary>
    BossBehaviorState EvaluatePhase1(BossDecisionContext ctx)
    {
        // 远距离 -> 巡逻
        if (ctx.distanceCategory == DistanceCategory.Far)
            return BossBehaviorState.Patrol;

        // 中等距离 -> 追击
        if (ctx.distanceCategory == DistanceCategory.Medium)
            return BossBehaviorState.Chase;

        // 近距离 -> 攻击
        if (ctx.distanceCategory == DistanceCategory.Close)
        {
            // 低概率使用技能（Phase1主要用普攻）
            if (ctx.hasAnySkillReady && ctx.randomFactor < phase1SkillChance)
                return BossBehaviorState.Attack; // 通过ExecuteCurrentState选择具体技能
            return BossBehaviorState.Attack;
        }

        return BossBehaviorState.Chase;
    }

    /// <summary>Phase2决策: 解锁范围攻击+冲刺，增加CastSkill频率</summary>
    BossBehaviorState EvaluatePhase2(BossDecisionContext ctx)
    {
        if (ctx.distanceCategory == DistanceCategory.Far)
        {
            // 远距离：有远程技能就用，没有则追击
            if (HasRangedSkillReady() && ctx.randomFactor < phase2SkillChance)
                return BossBehaviorState.CastSkill;
            return BossBehaviorState.Chase;
        }

        if (ctx.distanceCategory == DistanceCategory.Medium)
        {
            // 中距离：高概率释放技能
            if (ctx.hasAnySkillReady && ctx.randomFactor < phase2SkillChance)
                return BossBehaviorState.CastSkill;
            return BossBehaviorState.Chase;
        }

        // 近距离：攻击或追击
        if (ctx.hasAnySkillReady && ctx.randomFactor < phase2SkillChance)
            return BossBehaviorState.Attack;
        return BossBehaviorState.Attack;
    }

    /// <summary>Phase3决策: 全技能解锁，频繁连招，偶尔Defend</summary>
    BossBehaviorState EvaluatePhase3(BossDecisionContext ctx)
    {
        // 偶尔防御（特别是刚受到伤害后）
        if (ctx.randomFactor < phase3DefendChance && ctx.healthPercent < 0.25f)
            return BossBehaviorState.Defend;

        // 高优先级使用技能
        if (ctx.hasAnySkillReady && ctx.randomFactor < phase3SkillChance)
        {
            if (ctx.distanceCategory == DistanceCategory.Far || ctx.distanceCategory == DistanceCategory.Medium)
                return BossBehaviorState.CastSkill;
            return BossBehaviorState.Attack;
        }

        // 默认追击
        return BossBehaviorState.Chase;
    }

    /// <summary>Enraged决策: 最大侵略性，全技能乱发</summary>
    protected virtual BossBehaviorState EvaluateEnraged(BossDecisionContext ctx)
    {
        // 狂暴状态下几乎总是攻击/施法
        if (ctx.hasAnySkillReady)
        {
            if (ctx.randomFactor < enragedSkillChance)
                return ctx.distanceCategory == DistanceCategory.Close
                    ? BossBehaviorState.Attack : BossBehaviorState.CastSkill;
        }

        // 即使没技能也激进追击
        return BossBehaviorState.Chase;
    }

    // ═══════════════════════════════════════════
    // 状态切换与执行
    // ═══════════════════════════════════════════

    void TransitionToState(BossBehaviorState newState, BossDecisionContext ctx)
    {
        previousState = currentState;
        ChangeState(newState);

        if (enableDebugLog)
        {
            Debug.Log($"[BossCombatController] 状态切换: {previousState} -> {newState} " +
                      $"(距离={ctx.distanceToPlayer:F1}m, 血量%={ctx.healthPercent:P0}, 阶段={ctx.currentPhase})");
        }
    }

    void ChangeState(BossBehaviorState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    /// <summary>
    /// 执行当前状态的具体行为
    /// </summary>
    void ExecuteCurrentState(BossDecisionContext ctx)
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case BossBehaviorState.Patrol:
                ExecutePatrol(ctx);
                break;
            case BossBehaviorState.Alert:
                ExecuteAlert(ctx);
                break;
            case BossBehaviorState.Chase:
                ExecuteChase(ctx);
                break;
            case BossBehaviorState.Attack:
                ExecuteAttack(ctx);
                break;
            case BossBehaviorState.Defend:
                ExecuteDefend(ctx);
                break;
            case BossBehaviorState.CastSkill:
                ExecuteCastSkill(ctx);
                break;
            case BossBehaviorState.Enraged:
                ExecuteEnragedBehavior(ctx);
                break;
        }
    }

    // ────────────────────────────────────────
    // 各状态行为实现
    // ────────────────────────────────────────

    void ExecutePatrol(BossDecisionContext ctx)
    {
        // 缓慢随机移动（在Boss房间内）
        if (bossAI.rb != null)
        {
            Vector2 randomDir = new Vector2(
                Mathf.Sin(Time.time * 0.3f),
                Mathf.Cos(Time.time * 0.5f)
            ).normalized;
            bossAI.rb.velocity = randomDir * bossAI.moveSpeed * patrolSpeedRatio;
        }
    }

    void ExecuteAlert(BossDecisionContext ctx)
    {
        // 减速面向玩家方向
        if (bossAI.rb != null)
        {
            bossAI.rb.velocity = ctx.directionToPlayer * bossAI.moveSpeed * 0.5f;
        }
        // 短暂Alert后自动转Chase
        if (stateTimer > 0.5f)
            ChangeState(BossBehaviorState.Chase);
    }

    private float chaseLogTimer = 0f;

    void ExecuteChase(BossDecisionContext ctx)
    {
        if (bossAI.rb != null && bossAI.player != null)
        {
            float speed = CurrentEffectiveSpeed;

            // 保持最小距离，避免撞到/推动玩家
            if (ctx.distanceToPlayer < 1.5f)
            {
                bossAI.rb.velocity = Vector2.zero;
                return;
            }

            bossAI.rb.velocity = ctx.directionToPlayer * speed;

            chaseLogTimer += Time.deltaTime;
            if (chaseLogTimer >= 1f)
            {
                chaseLogTimer = 0f;
                Debug.LogWarning($"[BossCombatController] 🏃 追击中 speed={speed:F1} dist={ctx.distanceToPlayer:F1}m");
            }
        }
    }

    void ExecuteAttack(BossDecisionContext ctx)
    {
        if (IsAttacking) return;
        if (ctx.distanceToPlayer > attackDistanceThreshold) { ChangeState(BossBehaviorState.Chase); return; }

        BossAttackPattern selected = SelectAttackPattern(true);
        if (selected != null)
            ExecuteAttackPattern(selected);
        else
            ChangeState(BossBehaviorState.Chase);
    }

    void ExecuteDefend(BossDecisionContext ctx)
    {
        // 防御状态：停止移动，减少受到的伤害
        if (bossAI.rb != null)
            bossAI.rb.velocity = Vector2.zero;

        if (stateTimer > defendDuration)
            ChangeState(BossBehaviorState.Chase);
    }

    void ExecuteCastSkill(BossDecisionContext ctx)
    {
        if (IsAttacking) return;
        if (ctx.distanceToPlayer > attackDistanceThreshold) { ChangeState(BossBehaviorState.Chase); return; }

        BossAttackPattern selected = SelectAttackPattern(false);
        if (selected != null)
            ExecuteAttackPattern(selected);
        else
            ChangeState(BossBehaviorState.Chase);
    }

    void ExecuteEnragedBehavior(BossDecisionContext ctx)
    {
        ExecuteChase(ctx);
        if (IsAttacking) return;
        if (ctx.hasAnySkillReady && stateTimer > 0.3f)
        {
            BossAttackPattern selected = SelectAttackPattern(false);
            if (selected != null)
                ExecuteAttackPattern(selected);
        }
    }

    // ═══════════════════════════════════════════
    // 攻击模式选择与执行
    // ═══════════════════════════════════════════

    /// <summary>
    /// 从可用攻击中选择一个（带权重随机）
    /// @param preferMelee 是否优先选择近战攻击
    /// </summary>
    BossAttackPattern SelectAttackPattern(bool preferMelee = false)
    {
        List<BossAttackPattern> available = new List<BossAttackPattern>();

        for (int i = 0; i < attackPatterns.Count; i++)
        {
            var pattern = attackPatterns[i];
            if (pattern == null) continue;
            if (!pattern.IsReady) continue;
            if (!pattern.IsAvailableInPhase(bossAI.currentPhase)) continue;

            available.Add(pattern);
        }

        if (available.Count == 0) return null;

        // 简单随机选择（后续可扩展为权重轮盘）
        int index = Random.Range(0, available.Count);
        return available[index];
    }

    /// <summary>
    /// 执行选定的攻击模式（启动协程）
    /// </summary>
    public void ExecuteAttackPattern(BossAttackPattern pattern)
    {
        if (pattern == null || !pattern.IsReady) return;

        currentExecutingAttack = pattern;
        StartCoroutine(pattern.ExecuteAttack(this));

        Debug.LogWarning($"[BossCombatController] ⚔ {pattern.attackName} | 伤害:{bossAI.damage * pattern.damageMultiplier:F0} | 追踪:{pattern.useTracking}");
    }

    /// <summary>
    /// 攻击完成回调（由攻击模式内部调用）
    /// </summary>
    public void OnAttackExecutionFinished()
    {
        currentExecutingAttack = null;
        postAttackCooldown = 2f; // 攻击后2秒冷却，期间强制移动
        ChangeState(BossBehaviorState.Chase);
    }

    /// <summary>
    /// 生成攻击触发器 — 完全参考 EnemyCombatComponent.DealDamageToTarget 模式
    /// 在 attackTriggerPos 位置实例化 BossAttackTrigger 预制件，
    /// 以 attackTriggerPos 为父节点，通过 AttackTrigger 的 PerformAreaAttack 自动检测并伤害玩家。
    /// </summary>
    /// <param name="damage">本次攻击伤害值</param>
    /// <param name="size">触发器大小（可选，覆盖预制件上 BoxCollider2D 的默认尺寸）</param>
    public void SpawnAttackTrigger(float damage, Vector2? size = null)
    {
        if (bossAttackTriggerPrefab == null)
        {
            // 降级：prefab 未配置时直接对玩家造成伤害
            if (bossAI != null && bossAI.player != null)
            {
                var playerHealth = bossAI.player.GetComponent<HealthSystem>();
                if (playerHealth != null && !playerHealth.IsDead())
                    playerHealth.TakeDamage(damage);
            }
            return;
        }

        // 只在 attackTriggerPos 生成（追踪时跟随玩家，攻击时冻结，玩家可躲开）
        Vector3 spawnPos = attackTriggerPos != null ? attackTriggerPos.position : transform.position;
        GameObject trigger = Instantiate(bossAttackTriggerPrefab, spawnPos, Quaternion.identity);

        // 设置触发器尺寸（默认使用 attackRange × 2 保证覆盖玩家）
        Vector2 triggerSize = size ?? new Vector2(attackDistanceThreshold * 2f, attackDistanceThreshold * 2f);
        BoxCollider2D col = trigger.GetComponent<BoxCollider2D>();
        if (col != null) col.size = triggerSize;

        AttackTrigger at = trigger.GetComponent<AttackTrigger>();
        if (at != null)
        {
            at.SetDamage((int)damage);
            at.SetAttacker(gameObject, false, false);
        }
    }

    /// <summary>
    /// 追踪式攻击触发器 — 前摇阶段跟随玩家位置移动，判定阶段锁定位置并造成伤害
    /// 由 BossAttackPattern.ExecuteAttack 协程在 windup 阶段调用，
    /// 返回的 GameObject 在 active 阶段停止追踪。
    /// </summary>
    public System.Collections.IEnumerator TrackingAttackCoroutine(float damage, float trackDuration, float lockDuration, Vector2? size = null)
    {
        if (bossAttackTriggerPrefab == null || bossAI == null) yield break;

        Vector3 spawnPos = bossAI.player != null ? bossAI.player.position : transform.position;
        GameObject trigger = Instantiate(bossAttackTriggerPrefab, spawnPos, Quaternion.identity);
        Vector2 triggerSize = size ?? new Vector2(attackDistanceThreshold * 2f, attackDistanceThreshold * 2f);
        BoxCollider2D col = trigger.GetComponent<BoxCollider2D>();
        if (col != null) col.size = triggerSize;

        // 可视指示器：红色半透明方块表示追踪区域
        GameObject indicator = GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
        indicator.transform.SetParent(trigger.transform);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = Vector3.one * 0.6f;
        indicator.name = "TrackingIndicator";
        var renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        }
        Destroy(indicator.GetComponent<Collider>());

        AttackTrigger at = trigger.GetComponent<AttackTrigger>();
        if (at != null)
        {
            at.SetDamage((int)damage);
            at.SetAttacker(gameObject, false, false);
            at.delayActivation = true;
        }

        // 阶段1：前摇追踪 — 触发器跟随玩家移动
        Debug.LogWarning($"[BossCombatController] 🎯 追踪开始 player={(bossAI.player!=null)} playerPos={bossAI.player?.position} spawnPos={spawnPos}");
        float elapsed = 0f;
        while (elapsed < trackDuration && trigger != null)
        {
            if (bossAI.player != null)
                trigger.transform.position = bossAI.player.position;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 阶段2：锁定判定 — 触发器停止移动，激活伤害
        if (trigger != null && at != null)
        {
            if (renderer != null) renderer.material.color = Color.red;
            at.PerformAreaAttackPublic();
            Debug.LogWarning($"[BossCombatController] 💥 锁定！triggerPos={trigger.transform.position} playerPos={bossAI.player?.position}");
            Destroy(trigger, lockDuration);
        }
        else if (trigger != null)
        {
            Destroy(trigger);
        }
    }

    /// <summary>
    /// 获取考虑狂暴缩减后的实际冷却时间
    /// </summary>
    float GetEffectiveCooldown(int patternIndex)
    {
        if (patternIndex < 0 || patternIndex >= attackPatterns.Count) return 999f;
        var pattern = attackPatterns[patternIndex];
        if (pattern == null) return 999f;

        // 狂暴模式下大幅缩减冷却
        if (bossAI != null && bossAI.isEnraged)
            return pattern.cooldown * enragedCooldownMultiplier;
        return pattern.cooldown;
    }

    bool HasRangedSkillReady()
    {
        foreach (var pattern in attackPatterns)
        {
            if (pattern == null) continue;
            if (pattern.IsReady && pattern.IsAvailableInPhase(bossAI.currentPhase))
            {
                // 判定为远程攻击的简单启发式：前摇较长或范围较大
                if (pattern.windupDuration >= 0.5f || pattern is TimeShockwaveAttack || pattern is CrystalCageAttack)
                    return true;
            }
        }
        return false;
    }

    // ═══════════════════════════════════════════
    // 外部控制接口
    // ═══════════════════════════════════════════

    /// <summary>进入硬直状态</summary>
    public void EnterStun(float duration)
    {
        ChangeState(BossBehaviorState.Stunned);
        stunnedTimer = duration;
        if (bossAI.rb != null)
            bossAI.rb.velocity = Vector2.zero;

        if (enableDebugLog)
            Debug.Log($"[BossCombatController] 进入硬直 持续{duration:F1}s");
    }

    /// <summary>强制进入指定状态（供外部调用如受击反应）</summary>
    public void ForceState(BossBehaviorState state)
    {
        ChangeState(state);
    }

    /// <summary>阶段转换时调用 — 重置所有冷却</summary>
    public void OnPhaseChanged(BossPhase newPhase)
    {
        // 阶段转换时重置部分冷却（让Boss能立即展示新能力）
        for (int i = 0; i < attackPatterns.Count; i++)
        {
            var pattern = attackPatterns[i];
            if (pattern != null && pattern.IsAvailableInPhase(newPhase))
            {
                // 新解锁的攻击立即可用
                if ((int)newPhase > (int)pattern.minimumPhase ||
                    (int)newPhase == (int)pattern.minimumPhase)
                {
                    pattern.ResetCooldown();
                }
            }
        }

        if (enableDebugLog)
            Debug.Log($"[BossCombatController] 阶段转换为 {newPhase}，新技能已就绪");
    }

    /// <summary>获取当前可用的攻击数量</summary>
    public int GetAvailableAttackCount()
    {
        int count = 0;
        foreach (var pattern in attackPatterns)
        {
            if (pattern != null && pattern.IsAvailableInPhase(bossAI.currentPhase))
                count++;
        }
        return count;
    }

    // ═══════════════════════════════════════════
    // 调试方法
    // ═══════════════════════════════════════════

    /// <summary>
    /// 输出详细Boss状态信息（供调试使用）
    /// </summary>
    public void DebugPrintStatus()
    {
        Debug.Log("========== Boss Combat Status ==========");
        Debug.Log($"  State: {currentState} (prev: {previousState})");
        Debug.Log($"  Phase: {bossAI?.currentPhase}");
        Debug.Log($"  Health: {bossAI?.currentHealth:F0}/{bossAI?.maxHealth:F0} ({(bossAI?.currentHealth / bossAI?.maxHealth ?? 0):P0})");
        Debug.Log($"  Speed: {CurrentEffectiveSpeed:F1} (base: {bossAI?.moveSpeed})");
        Debug.Log($"  Enraged: {bossAI?.isEnraged}");
        Debug.Log($"  Invulnerable: {bossAI?.isInvulnerable}");
        Debug.Log($"  Stunned: {IsStunned} (timer: {stunnedTimer:F2}s)");
        Debug.Log($"  Attacking: {IsAttacking} ({currentExecutingAttack?.attackName ?? "None"})");
        Debug.Log("  Attack Patterns:");
        for (int i = 0; i < attackPatterns.Count; i++)
        {
            var p = attackPatterns[i];
            if (p == null) continue;
            Debug.Log($"    [{i}] {p.attackName}: Ready={p.IsReady}, Phase>={p.minimumPhase}, " +
                       $"CD={GetEffectiveCooldown(i):F1}s");
        }
        Debug.Log("==========================================");
    }

    // ═══════════════════════════════════════════
    // Gizmos 调试绘制
    // ═══════════════════════════════════════════

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || bossAI == null) return;

        // 攻击范围（红色实心）
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackDistanceThreshold);

        // 攻击触发器生成位置
        if (attackTriggerPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackTriggerPos.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, attackTriggerPos.position);
        }

        // 追击范围（黄色）
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, chaseDistanceThreshold);

    }
}
