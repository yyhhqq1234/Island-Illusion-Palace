using UnityEngine;

/// <summary>
/// 召唤物状态机组件 — 管理行为模式切换
/// 状态优先级：Defend > Attack > Patrol > Follow
/// 内置状态切换冷却防止频繁切换
/// </summary>
public class SummonStateComponent : MonoBehaviour
{
    /// <summary>
    /// 行为模式枚举（与 SummonedCreatureAI 保持一致）
    /// </summary>
    public enum BehaviorMode { Follow, Attack, Defend, Patrol }

    [Header("状态切换设置")]
    [Tooltip("玩家低血量阈值（低于此比例进入防御状态）")]
    [Range(0f, 1f)]
    public float playerLowHealthThreshold = 0.3f;
    [Tooltip("状态切换冷却时间（秒）")]
    public float stateChangeCooldown = 1f;
    [Tooltip("巡逻触发距离（与玩家距离小于此值进入巡逻）")]
    public float patrolDistance = 5f;

    // 当前状态
    public BehaviorMode CurrentBehavior { get; private set; } = BehaviorMode.Follow;
    public BehaviorMode PreviousBehavior { get; private set; } = BehaviorMode.Follow;

    // 冷却计时器
    private float cooldownTimer = 0f;

    // 事件
    public System.Action<BehaviorMode, BehaviorMode> OnBehaviorChanged; // (oldMode, newMode)

    /// <summary>
    /// 初始化状态机
    /// </summary>
    public void Initialize(BehaviorMode initialMode = BehaviorMode.Follow)
    {
        CurrentBehavior = initialMode;
        PreviousBehavior = initialMode;
        cooldownTimer = 0f;
    }

    /// <summary>
    /// 更新状态：根据玩家血量、敌人存在、距离等条件切换行为模式
    /// </summary>
    /// <param name="playerHealthPercent">玩家血量百分比 (0-1)</param>
    /// <param name="hasEnemyNearby">附近是否有敌人</param>
    /// <param name="distanceToPlayer">与玩家的距离</param>
    /// <returns>当前行为模式</returns>
    public BehaviorMode UpdateState(float playerHealthPercent, bool hasEnemyNearby, float distanceToPlayer)
    {
        // 更新冷却计时器
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        BehaviorMode desiredMode = DetermineDesiredMode(playerHealthPercent, hasEnemyNearby, distanceToPlayer);

        if (desiredMode != CurrentBehavior && cooldownTimer <= 0)
        {
            SwitchToBehavior(desiredMode);
        }

        return CurrentBehavior;
    }

    /// <summary>
    /// 根据条件确定期望的行为模式
    /// 优先级：Defend > Attack > Patrol > Follow
    /// </summary>
    private BehaviorMode DetermineDesiredMode(float playerHealthPercent, bool hasEnemyNearby, float distanceToPlayer)
    {
        // 1. 防御状态：玩家血量低于阈值
        if (playerHealthPercent < playerLowHealthThreshold)
        {
            return BehaviorMode.Defend;
        }

        // 2. 攻击状态：附近有敌人
        if (hasEnemyNearby)
        {
            return BehaviorMode.Attack;
        }

        // 3. 巡逻状态：在玩家附近
        if (distanceToPlayer <= patrolDistance)
        {
            return BehaviorMode.Patrol;
        }

        // 4. 跟随状态：远离玩家
        return BehaviorMode.Follow;
    }

    /// <summary>
    /// 执行行为切换
    /// </summary>
    private void SwitchToBehavior(BehaviorMode newMode)
    {
        if (cooldownTimer > 0) return;

        PreviousBehavior = CurrentBehavior;
        CurrentBehavior = newMode;
        cooldownTimer = stateChangeCooldown;

        Debug.Log($"[SummonState] 行为切换: {PreviousBehavior} → {CurrentBehavior}");

        OnBehaviorChanged?.Invoke(PreviousBehavior, CurrentBehavior);
    }

    /// <summary>
    /// 强制设置行为模式（外部调用，绕过冷却）
    /// </summary>
    public void SetBehavior(BehaviorMode mode)
    {
        if (mode == CurrentBehavior) return;

        PreviousBehavior = CurrentBehavior;
        CurrentBehavior = mode;
        cooldownTimer = 0f; // 重置冷却

        Debug.Log($"[SummonState] 强制行为切换: {PreviousBehavior} → {CurrentBehavior}");

        OnBehaviorChanged?.Invoke(PreviousBehavior, CurrentBehavior);
    }

    /// <summary>
    /// 获取当前行为模式
    /// </summary>
    public BehaviorMode GetCurrentBehavior()
    {
        return CurrentBehavior;
    }

    /// <summary>
    /// 重置状态切换冷却
    /// </summary>
    public void ResetCooldown()
    {
        cooldownTimer = 0f;
    }
}