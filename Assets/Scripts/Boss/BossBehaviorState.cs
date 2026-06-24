using UnityEngine;

/// <summary>
/// Boss行为状态枚举 — 控制Boss AI的多层次状态机
/// </summary>
public enum BossBehaviorState
{
    Patrol,      // 巡逻（Boss房间内缓慢移动）
    Alert,       // 警戒（发现玩家，准备战斗）
    Chase,       // 追击（向玩家移动）
    Attack,      // 攻击（执行攻击模式）
    Defend,      // 防御（短暂减伤/格挡）
    CastSkill,   // 施放特殊技能
    Stunned,     // 硬直（攻击后或受击）
    Enraged      // 狂暴（低血量，最大侵略性）
}

/// <summary>
/// 距离分类 — 用于AI决策的距离层级
/// </summary>
public enum DistanceCategory
{
    Close,   // 近距离 (< attackRange)
    Medium,  // 中距离 (attackRange ~ chaseRange * 0.5)
    Far      // 远距离 (> chaseRange * 0.5)
}

/// <summary>
/// Boss决策上下文 — 每帧决策时构建的完整上下文信息
/// 用于动态决策系统评估当前应采取的行为
/// </summary>
public struct BossDecisionContext
{
    /// <summary>到玩家的距离</summary>
    public float distanceToPlayer;

    /// <summary>距离分类</summary>
    public DistanceCategory distanceCategory;

    /// <summary>自身生命值百分比(0~1)</summary>
    public float healthPercent;

    /// <summary>当前阶段</summary>
    public BossPhase currentPhase;

    /// <summary>当前行为状态</summary>
    public BossBehaviorState currentState;

    /// <summary>各技能冷却剩余时间（索引对应技能列表）</summary>
    public float[] skillCooldownsRemaining;

    /// <summary>是否有任意技能就绪</summary>
    public bool hasAnySkillReady;

    /// <summary>是否处于无敌状态</summary>
    public bool isInvulnerable;

    /// <summary>是否处于硬直状态</summary>
    public bool isStunned;

    /// <summary>从决策点到玩家的方向向量（归一化）</summary>
    public Vector2 directionToPlayer;

    /// <summary>玩家当前位置</summary>
    public Vector3 playerPosition;

    /// <summary>Boss当前位置</summary>
    public Vector3 bossPosition;

    /// <summary>随机权重因子(0~1)，用于避免完全可预测的行为</summary>
    public float randomFactor;

    /// <summary>
    /// 从距离计算距离分类
    /// </summary>
    public static DistanceCategory ClassifyDistance(float distance, float attackRange, float chaseRange)
    {
        float mediumThreshold = chaseRange * 0.5f;
        if (distance <= attackRange) return DistanceCategory.Close;
        if (distance <= mediumThreshold) return DistanceCategory.Medium;
        return DistanceCategory.Far;
    }

    /// <summary>
    /// 构建决策上下文的便捷方法
    /// </summary>
    public static BossDecisionContext Create(
        Transform bossTransform,
        Transform playerTransform,
        float currentHealth,
        float maxHealth,
        BossPhase phase,
        BossBehaviorState state,
        float[] cooldowns,
        bool invulnerable,
        bool stunned)
    {
        var ctx = new BossDecisionContext();
        ctx.bossPosition = bossTransform.position;
        ctx.playerPosition = playerTransform != null ? playerTransform.position : Vector3.zero;
        ctx.directionToPlayer = (ctx.playerPosition - ctx.bossPosition).normalized;
        ctx.distanceToPlayer = Vector3.Distance(ctx.bossPosition, ctx.playerPosition);
        ctx.healthPercent = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        ctx.currentPhase = phase;
        ctx.currentState = state;
        ctx.skillCooldownsRemaining = cooldowns;
        ctx.isInvulnerable = invulnerable;
        ctx.isStunned = stunned;
        ctx.randomFactor = Random.value;

        // 根据距离分类（3m内近战 / 12m内追击 / 其余巡逻）
        ctx.distanceCategory = ctx.distanceToPlayer < 2f ? DistanceCategory.Close
            : ctx.distanceToPlayer < 6f ? DistanceCategory.Medium
            : DistanceCategory.Far;

        // 检查是否有技能就绪
        ctx.hasAnySkillReady = false;
        if (cooldowns != null)
        {
            for (int i = 0; i < cooldowns.Length; i++)
            {
                if (cooldowns[i] <= 0f) { ctx.hasAnySkillReady = true; break; }
            }
        }

        return ctx;
    }
}
