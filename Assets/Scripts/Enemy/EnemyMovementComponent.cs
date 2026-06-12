using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 敌人移动控制组件 - 负责巡逻、移动、安全区限制等移动相关逻辑
/// </summary>
public class EnemyMovementComponent : MonoBehaviour
{
    [Header("移动设置")]
    public float baseMoveSpeed = 3f;
    [Tooltip("敌人是否会被安全区阻挡")]
    public bool blockBySafeZone = true;
    [Tooltip("安全区警戒距离（在进入安全区前多少距离开始避开）")]
    public float safeZoneAvoidDistance = 2f;

    [Header("巡逻设置")]
    public bool enablePatrol = true;
    public float patrolRange = 5f;
    public float patrolWaitTime = 2f;
    public float patrolSpeed = 2f;

    [Header("动态追逐范围")]
    public float chaseRange = 10f;
    [Tooltip("动态追逐范围扩展值，当锁定目标时扩大范围")]
    public float chaseRangeExpansion = 5f;

    /// <summary>
    /// 按敌人类型配置的基础移动速度（未配置的类型使用 baseMoveSpeed）
    /// </summary>
    private static readonly Dictionary<EnemyAI.EnemyType, float> TypeBaseSpeed = new Dictionary<EnemyAI.EnemyType, float>
    {
        { EnemyAI.EnemyType.CorruptedVillager, 3f },
        { EnemyAI.EnemyType.CrystalLizard, 4f },
        { EnemyAI.EnemyType.IceWolf, 3.5f },
        { EnemyAI.EnemyType.SwampStalker, 2.5f },
        { EnemyAI.EnemyType.MechanicalDebris, 2f },
        { EnemyAI.EnemyType.SkeletonWarrior, 3f },
        { EnemyAI.EnemyType.Wraith, 3.2f },
        { EnemyAI.EnemyType.Gargoyle, 2.2f },
        { EnemyAI.EnemyType.SoulEater, 1.8f },
        { EnemyAI.EnemyType.LavaElemental, 1.5f },
        { EnemyAI.EnemyType.MechanicalConstruct, 1.2f },
        { EnemyAI.EnemyType.TimeGuardian, 1f },
        { EnemyAI.EnemyType.MemoryGuardian, 0.9f },
        { EnemyAI.EnemyType.CorruptionGuardian, 0.8f },
        { EnemyAI.EnemyType.ScarletSoulShana, 0.7f },
    };

    // 内部引用
    private Rigidbody2D rb;
    private EnemyStateComponent stateComponent;
    private EnemyAI enemyAI;

    // 巡逻相关
    private Vector3 spawnPosition;
    private Vector3 patrolTarget;
    private float patrolWaitTimer = 0f;
    private bool isPatrolling = false;

    // 安全区相关
    private bool isInSafeZone = false;

    // 动态追逐范围
    public bool IsChaseRangeExpanded { get; private set; }
    public float ExpandedChaseRange { get; private set; }
    public float OriginalChaseRange { get; private set; }

    public event Action<bool> OnPatrolStateChanged;
    public event Action<Vector2> OnMovementDirectionChanged;

    public void Initialize(Rigidbody2D rigidbody, EnemyStateComponent state)
    {
        rb = rigidbody;
        stateComponent = state;
        enemyAI = GetComponent<EnemyAI>();
        spawnPosition = transform.position;
        OriginalChaseRange = chaseRange;
        ExpandedChaseRange = chaseRange + chaseRangeExpansion;

        if (enablePatrol)
        {
            SetNewPatrolTarget();
        }
    }

    /// <summary>
    /// 获取当前移动速度（综合类型基础速度+状态倍率）
    /// </summary>
    public float GetCurrentMoveSpeed()
    {
        float speed = GetTypeBaseSpeed();
        if (stateComponent != null)
        {
            speed *= stateComponent.GetMoveSpeedMultiplier();
        }
        return speed;
    }

    /// <summary>
    /// 获取当前敌人类型的基础移动速度（优先使用 TypeBaseSpeed 字典，否则回退 baseMoveSpeed）
    /// </summary>
    public float GetTypeBaseSpeed()
    {
        if (enemyAI != null && TypeBaseSpeed.TryGetValue(enemyAI.enemyType, out float typeSpeed))
        {
            return typeSpeed;
        }
        return baseMoveSpeed;
    }

    /// <summary>
    /// 设置移动方向和速度（自动应用安全区限制）
    /// </summary>
    public void SetMovementDirection(Vector2 direction)
    {
        Vector2 restrictedDirection = ApplySafeZoneRestriction(direction);
        if (rb != null)
        {
            rb.velocity = restrictedDirection * GetCurrentMoveSpeed();
        }
        OnMovementDirectionChanged?.Invoke(restrictedDirection);
    }

    /// <summary>
    /// 设置移动方向和指定速度（不自动应用安全区限制）
    /// </summary>
    public void SetMovementVelocity(Vector2 velocity)
    {
        if (rb != null)
        {
            rb.velocity = velocity;
        }
        OnMovementDirectionChanged?.Invoke(velocity.normalized);
    }

    /// <summary>
    /// 停止所有移动
    /// </summary>
    public void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        OnMovementDirectionChanged?.Invoke(Vector2.zero);
    }

    /// <summary>
    /// 更新路径状态
    /// </summary>
    public void UpdatePatrol(float deltaTime)
    {
        if (!enablePatrol) return;

        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= deltaTime;
            isPatrolling = false;
            StopMovement();
            OnPatrolStateChanged?.Invoke(false);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, patrolTarget);
        if (distanceToTarget < 0.5f)
        {
            patrolWaitTimer = patrolWaitTime;
            SetNewPatrolTarget();
            isPatrolling = false;
            StopMovement();
            OnPatrolStateChanged?.Invoke(false);
        }
        else
        {
            Vector2 direction = ((Vector2)(patrolTarget - transform.position)).normalized;
            if (rb != null)
            {
                rb.velocity = direction * patrolSpeed;
            }
            isPatrolling = true;
            OnPatrolStateChanged?.Invoke(true);
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * patrolRange;
        patrolTarget = spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        isPatrolling = true;
    }

    /// <summary>
    /// 更新动态追逐范围
    /// </summary>
    public void UpdateChaseRange(float distanceToTargetSqr)
    {
        if (distanceToTargetSqr <= (OriginalChaseRange * OriginalChaseRange) && !IsChaseRangeExpanded)
        {
            IsChaseRangeExpanded = true;
        }
        else if (IsChaseRangeExpanded && distanceToTargetSqr > (ExpandedChaseRange * ExpandedChaseRange))
        {
            IsChaseRangeExpanded = false;
        }
    }

    /// <summary>
    /// 获取当前有效追逐范围（平方）
    /// </summary>
    public float GetEffectiveChaseRangeSqr()
    {
        float range = IsChaseRangeExpanded ? ExpandedChaseRange : OriginalChaseRange;
        return range * range;
    }

    /// <summary>
    /// 应用安全区限制
    /// </summary>
    public Vector2 ApplySafeZoneRestriction(Vector2 direction)
    {
        if (!blockBySafeZone) return direction;

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentPosition + direction * GetCurrentMoveSpeed() * Time.deltaTime;

        bool wouldEnterSafeZone = SafeZoneDetector.IsInAnySafeZone(targetPosition);

        if (wouldEnterSafeZone)
        {
            Vector2 avoidDirection;
            if (SafeZoneDetector.TryGetSafeZoneBoundary(currentPosition, out avoidDirection))
            {
                isInSafeZone = true;
                return avoidDirection;
            }
            else
            {
                return GetAvoidSafeZoneDirection(currentPosition, direction);
            }
        }
        else
        {
            isInSafeZone = false;
        }

        return direction;
    }

    /// <summary>
    /// 计算逃跑方向
    /// </summary>
    public Vector2 CalculateFleeDirection()
    {
        float fleeDetectionRange = stateComponent != null ? stateComponent.fleeDetectionRange : 15f;
        Vector2 currentPosition = transform.position;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] summons = GameObject.FindGameObjectsWithTag("SummonedCreature");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        List<GameObject> threats = new List<GameObject>();
        threats.AddRange(players);
        threats.AddRange(summons);
        threats.AddRange(enemies);
        threats.Remove(gameObject);

        if (threats.Count == 0)
        {
            return UnityEngine.Random.insideUnitCircle.normalized;
        }

        float[] directionAngles = new float[8];
        for (int i = 0; i < 8; i++)
        {
            directionAngles[i] = i * 45f;
        }

        int[] threatCounts = new int[8];
        float fleeDetectionRangeSquared = fleeDetectionRange * fleeDetectionRange;

        foreach (GameObject threat in threats)
        {
            if (threat == null) continue;

            Vector2 directionToThreat = (Vector2)threat.transform.position - currentPosition;
            float distanceSquared = directionToThreat.sqrMagnitude;

            if (distanceSquared <= fleeDetectionRangeSquared)
            {
                float angle = Mathf.Atan2(directionToThreat.y, directionToThreat.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360;

                int closestDirection = 0;
                float minAngleDiff = 360f;

                for (int i = 0; i < 8; i++)
                {
                    float angleDiff = Mathf.Abs(angle - directionAngles[i]);
                    if (angleDiff > 180) angleDiff = 360 - angleDiff;

                    if (angleDiff < minAngleDiff)
                    {
                        minAngleDiff = angleDiff;
                        closestDirection = i;
                    }
                }

                threatCounts[closestDirection]++;
            }
        }

        int maxThreatDirection = 0;
        int maxThreatCount = 0;

        for (int i = 0; i < 8; i++)
        {
            if (threatCounts[i] > maxThreatCount)
            {
                maxThreatCount = threatCounts[i];
                maxThreatDirection = i;
            }
        }

        float fleeAngle = directionAngles[maxThreatDirection] + 180f;
        if (fleeAngle >= 360f) fleeAngle -= 360f;

        return new Vector2(Mathf.Cos(fleeAngle * Mathf.Deg2Rad), Mathf.Sin(fleeAngle * Mathf.Deg2Rad));
    }

    private Vector2 GetAvoidSafeZoneDirection(Vector2 currentPosition, Vector2 desiredDirection)
    {
        SafeZoneDetector nearestSafeZone = SafeZoneDetector.GetNearestSafeZone(currentPosition);
        if (nearestSafeZone == null) return desiredDirection;

        Vector2 safeZoneCenter = nearestSafeZone.transform.position;
        Vector2 toSafeZone = (currentPosition - safeZoneCenter).normalized;

        float avoidWeight = 0.8f;
        float originalWeight = 0.2f;

        return (toSafeZone * avoidWeight + desiredDirection * originalWeight).normalized;
    }

    /// <summary>
    /// 获取当前Rigidbody2D速度
    /// </summary>
    public Vector2 GetVelocity()
    {
        return rb != null ? rb.velocity : Vector2.zero;
    }

    public bool IsPatrolling => isPatrolling;
    public bool IsInSafeZone => isInSafeZone;
}