using UnityEngine;
using System;

/// <summary>
/// 敌人目标查找组件 - 负责查找和选择最近的攻击目标
/// </summary>
public class EnemyTargetingComponent : MonoBehaviour
{
    [Header("目标查找设置")]
    public float targetCheckInterval = 1f;
    public float playerSearchCooldown = 0.5f;

    // 目标引用
    public Transform Player { get; private set; }
    public Transform CurrentTarget { get; private set; }
    public HealthSystem PlayerHealth { get; private set; }

    // 目标状态
    public bool IsTargetingSummon { get; private set; }
    public bool JustDefeatedSummon { get; private set; }

    // 内部状态
    private float targetCheckTimer = 0f;
    private float lastPlayerSearchTime = 0f;
    private float summonDefeatTimer = 0f;
    private float summonDefeatCelebrateDuration = 1.5f;

    // 事件
    public event Action<Transform> OnTargetChanged;
    public event Action OnSummonDefeated;

    public void Initialize(Transform player)
    {
        Player = player;
        CurrentTarget = player;
        IsTargetingSummon = false;
        JustDefeatedSummon = false;
    }

    /// <summary>
    /// 更新目标查找
    /// </summary>
    public void UpdateTargeting(float attackRange)
    {
        // 处理庆祝状态
        if (JustDefeatedSummon)
        {
            summonDefeatTimer += Time.deltaTime;
            if (summonDefeatTimer >= summonDefeatCelebrateDuration)
            {
                JustDefeatedSummon = false;
                summonDefeatTimer = 0f;
            }
            return;
        }

        // 检查召唤物是否被销毁
        if (CurrentTarget != null && IsTargetingSummon)
        {
            if (CurrentTarget.gameObject == null || !CurrentTarget.gameObject.activeInHierarchy)
            {
                HandleSummonDefeated();
                return;
            }
        }

        // 定时查找目标
        targetCheckTimer += Time.deltaTime;
        if (targetCheckTimer >= targetCheckInterval)
        {
            targetCheckTimer = 0f;
            FindNearestTarget(attackRange);
        }
    }

    /// <summary>
    /// 查找并更新玩家引用
    /// </summary>
    public bool TryFindPlayer()
    {
        if (Player != null) return true;

        if (Time.time - lastPlayerSearchTime < playerSearchCooldown)
            return false;

        lastPlayerSearchTime = Time.time;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Player = playerObj.transform;
            PlayerHealth = playerObj.GetComponent<HealthSystem>();
            if (CurrentTarget == null)
            {
                CurrentTarget = Player;
            }
            return true;
        }
        return false;
    }

    private void FindNearestTarget(float attackRange)
    {
        Transform nearestTarget = Player;
        IsTargetingSummon = false;

        Vector2 currentPosition = transform.position;

        GameObject[] summons = GameObject.FindGameObjectsWithTag("SummonedCreature");
        GameObject nearestSummon = null;
        float nearestSummonDistanceSquared = float.MaxValue;

        foreach (GameObject summon in summons)
        {
            if (summon != null)
            {
                float distanceToSummonSquared = ((Vector2)summon.transform.position - currentPosition).sqrMagnitude;
                if (distanceToSummonSquared < nearestSummonDistanceSquared)
                {
                    nearestSummonDistanceSquared = distanceToSummonSquared;
                    nearestSummon = summon;
                }
            }
        }

        if (nearestSummon != null && Player != null)
        {
            float distanceToPlayerSquared = ((Vector2)Player.position - currentPosition).sqrMagnitude;
            float attackRangeThresholdSquared = (attackRange * 1.5f) * (attackRange * 1.5f);

            if (distanceToPlayerSquared <= attackRangeThresholdSquared)
            {
                nearestTarget = Player;
                IsTargetingSummon = false;
            }
            else
            {
                nearestTarget = nearestSummon.transform;
                IsTargetingSummon = true;
            }
        }
        else if (nearestSummon != null)
        {
            nearestTarget = nearestSummon.transform;
            IsTargetingSummon = true;
        }
        else
        {
            nearestTarget = Player;
            IsTargetingSummon = false;
        }

        if (CurrentTarget != nearestTarget)
        {
            CurrentTarget = nearestTarget;
            OnTargetChanged?.Invoke(CurrentTarget);
        }
        else
        {
            CurrentTarget = nearestTarget;
        }
    }

    private void HandleSummonDefeated()
    {
        JustDefeatedSummon = true;
        summonDefeatTimer = 0f;
        IsTargetingSummon = false;
        CurrentTarget = Player;
        OnSummonDefeated?.Invoke();
    }

    /// <summary>
    /// 获取攻击范围内的目标（用于逃跑时反击）
    /// </summary>
    public Transform GetTargetInRange(float attackRange)
    {
        Vector2 currentPosition = transform.position;
        float attackRangeSquared = attackRange * attackRange;

        // 检查玩家
        if (Player != null)
        {
            float distanceToPlayerSquared = ((Vector2)Player.position - currentPosition).sqrMagnitude;
            if (distanceToPlayerSquared <= attackRangeSquared)
            {
                return Player;
            }
        }

        // 检查召唤物
        GameObject[] summons = GameObject.FindGameObjectsWithTag("SummonedCreature");
        foreach (GameObject summon in summons)
        {
            if (summon != null)
            {
                float distanceToSummonSquared = ((Vector2)summon.transform.position - currentPosition).sqrMagnitude;
                if (distanceToSummonSquared <= attackRangeSquared)
                {
                    return summon.transform;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 计算到当前目标的距离平方
    /// </summary>
    public float GetDistanceToTargetSqr()
    {
        if (CurrentTarget == null) return float.MaxValue;
        return ((Vector2)(CurrentTarget.position - transform.position)).sqrMagnitude;
    }
}