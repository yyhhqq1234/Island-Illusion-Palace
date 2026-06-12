using UnityEngine;
using System;

/// <summary>
/// 敌人状态管理组件 - 负责狂暴、逃跑、脱战等状态的统一管理
/// </summary>
public class EnemyStateComponent : MonoBehaviour
{
    [Header("狂暴设置")]
    [Tooltip("狂暴触发的血量比例阈值 (0-1)")]
    [Range(0f, 1f)]
    public float berserkHealthThreshold = 0.3f;
    [Tooltip("狂暴时攻击速度倍率")]
    public float berserkAttackSpeedMultiplier = 1.5f;
    [Tooltip("狂暴时移动速度倍率")]
    public float berserkMoveSpeedMultiplier = 1.2f;
    [Tooltip("狂暴时防御降低比例")]
    public float berserkDefenseReduction = 0.3f;

    [Header("逃跑设置")]
    [Tooltip("低血量逃跑的血量比例阈值 (0-1)")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.3f;
    [Tooltip("逃跑时检测敌人的范围")]
    public float fleeDetectionRange = 15f;
    [Tooltip("逃跑时的移动速度倍率")]
    public float fleeSpeedMultiplier = 1.5f;
    [Tooltip("逃跑时仍可攻击的间隔倍率")]
    public float fleeAttackIntervalMultiplier = 0.5f;

    [Header("脱战回复设置")]
    [Tooltip("脱战后开始回复HP的延迟时间（秒）")]
    public float outOfCombatHealDelay = 3f;
    [Tooltip("HP回复比例（每秒回复最大HP的百分比）")]
    [Range(0f, 1f)]
    public float healRatePerSecond = 0.05f;

    // 状态属性
    public bool IsBerserk { get; private set; }
    public bool IsFleeing { get; private set; }
    public bool IsOutOfCombat { get; private set; }

    // 事件
    public event Action<bool> OnBerserkStateChanged;
    public event Action<bool> OnFleeingStateChanged;

    // 内部引用
    private HealthSystem healthSystem;
    private float outOfCombatTimer = 0f;

    public void Initialize(HealthSystem health)
    {
        healthSystem = health;
        IsBerserk = false;
        IsFleeing = false;
        IsOutOfCombat = true;
        outOfCombatTimer = 0f;
    }

    /// <summary>
    /// 每帧更新所有状态
    /// </summary>
    public void UpdateState(float deltaTime, bool inCombat)
    {
        UpdateBerserkState();
        UpdateFleeingState();
        UpdateOutOfCombatState(deltaTime, inCombat);
    }

    private void UpdateBerserkState()
    {
        if (healthSystem == null) return;
        bool shouldBeBerserk = healthSystem.GetHealthPercent() <= berserkHealthThreshold;

        if (shouldBeBerserk != IsBerserk)
        {
            IsBerserk = shouldBeBerserk;
            OnBerserkStateChanged?.Invoke(IsBerserk);

            if (healthSystem != null)
            {
                healthSystem.SetDefenseMultiplier(IsBerserk ? (1f - berserkDefenseReduction) : 1f);
            }
        }
    }

    private void UpdateFleeingState()
    {
        if (healthSystem == null) return;
        bool shouldBeFleeing = healthSystem.GetHealthPercent() <= lowHealthThreshold;

        if (shouldBeFleeing != IsFleeing)
        {
            IsFleeing = shouldBeFleeing;
            OnFleeingStateChanged?.Invoke(IsFleeing);
        }
    }

    private void UpdateOutOfCombatState(float deltaTime, bool inCombat)
    {
        if (healthSystem == null || IsFleeing) return;

        if (inCombat)
        {
            if (!IsOutOfCombat)
            {
                IsOutOfCombat = true;
                outOfCombatTimer = 0f;
            }
        }
        else
        {
            if (IsOutOfCombat)
            {
                IsOutOfCombat = false;
                outOfCombatTimer = 0f;
            }

            outOfCombatTimer += deltaTime;
            if (outOfCombatTimer >= outOfCombatHealDelay)
            {
                float healAmount = healthSystem.maxHealth * healRatePerSecond * deltaTime;
                healthSystem.Heal(healAmount);
            }
        }
    }

    /// <summary>
    /// 获取当前移动速度倍率（综合狂暴和逃跑状态）
    /// </summary>
    public float GetMoveSpeedMultiplier()
    {
        float multiplier = 1f;
        if (IsBerserk) multiplier *= berserkMoveSpeedMultiplier;
        if (IsFleeing) multiplier *= fleeSpeedMultiplier;
        return multiplier;
    }

    /// <summary>
    /// 获取当前攻击速度（综合狂暴状态）
    /// </summary>
    public float GetAttackSpeedMultiplier(float baseRate)
    {
        float rate = baseRate;
        if (IsBerserk) rate *= berserkAttackSpeedMultiplier;
        return rate;
    }

    /// <summary>
    /// 获取逃跑时的攻击间隔倍率
    /// </summary>
    public float GetFleeAttackIntervalMultiplier()
    {
        return fleeAttackIntervalMultiplier;
    }

    /// <summary>
    /// 进入狂暴状态（外部调用，如IEnemyProvider接口）
    /// </summary>
    public void EnterBerserkState()
    {
        if (!IsBerserk)
        {
            IsBerserk = true;
            OnBerserkStateChanged?.Invoke(true);
            if (healthSystem != null)
            {
                healthSystem.SetDefenseMultiplier(1f - berserkDefenseReduction);
            }
        }
    }
}