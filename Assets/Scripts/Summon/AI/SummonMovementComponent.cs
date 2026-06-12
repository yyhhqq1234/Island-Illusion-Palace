using UnityEngine;

/// <summary>
/// 召唤物移动控制组件 — 负责缓冲跟随、巡逻、护卫定位等所有移动逻辑
/// 优化：使用 SmoothDamp 替代 Lerp，缓冲区间避免抖动，护卫定位算法
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SummonMovementComponent : MonoBehaviour
{
    private const float MIN_VELOCITY_THRESHOLD = 0.1f;

    [Header("缓冲跟随")]
    [Tooltip("缓冲跟随最小距离：小于此距离停止移动")]
    public float followBufferMin = 1.5f;
    [Tooltip("缓冲跟随最大距离：大于此距离开始移动")]
    public float followBufferMax = 3.5f;
    [Tooltip("SmoothDamp 平滑时间")]
    public float smoothTime = 0.3f;

    [Header("巡逻设置")]
    [Tooltip("巡逻半径（围绕玩家）")]
    public float patrolRadius = 5f;
    [Tooltip("巡逻速度倍率（相对于移动速度）")]
    public float patrolSpeedMultiplier = 0.6f;
    [Tooltip("巡逻点停留时间最小值（秒）")]
    public float patrolWaitMin = 1f;
    [Tooltip("巡逻点停留时间最大值（秒）")]
    public float patrolWaitMax = 3f;

    [Header("护卫设置")]
    [Tooltip("护卫时与玩家的距离")]
    public float guardDistance = 2.5f;
    [Tooltip("护卫巡逻半径（无敌人时在玩家周围随机巡逻）")]
    public float guardPatrolRadius = 3f;

    [Header("基础移动速度")]
    public float moveSpeed = 3f;

    // 内部引用
    private Rigidbody2D rb;
    private Transform playerTransform;

    // 巡逻状态
    private Vector2 patrolTarget;
    private float patrolWaitTimer = 0f;
    private bool isWaitingAtPatrolPoint = false;

    // 护卫状态
    private Vector2 guardOffset;
    private float guardTimer = 0f;
    private float guardUpdateInterval = 3f;

    // SmoothDamp 速度缓存
    private Vector2 currentVelocity = Vector2.zero;

    // 目标（用于护卫时计算面向）
    public Vector2 LastMoveDirection { get; private set; }
    public bool IsMoving { get; private set; }

    /// <summary>
    /// 初始化组件
    /// </summary>
    public void Initialize(Rigidbody2D rigidbody, Transform player)
    {
        rb = rigidbody;
        playerTransform = player;
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    /// <summary>
    /// 设置移动速度
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 向目标点移动（带 SmoothDamp 平滑加减速）
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="speedMultiplier">速度倍率（默认1.0）</param>
    public void MoveTowards(Vector2 target, float speedMultiplier = 1f)
    {
        if (rb == null) return;

        Vector2 currentPos = transform.position;
        Vector2 direction = (target - currentPos).normalized;
        Vector2 targetVelocity = direction * moveSpeed * speedMultiplier;

        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref currentVelocity, smoothTime);

        IsMoving = rb.velocity.sqrMagnitude > MIN_VELOCITY_THRESHOLD;
        if (IsMoving)
        {
            LastMoveDirection = rb.velocity.normalized;
        }
    }

    /// <summary>
    /// 直接设置速度向量
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        if (rb == null) return;
        rb.velocity = velocity;
        IsMoving = velocity.sqrMagnitude > MIN_VELOCITY_THRESHOLD;
        if (IsMoving)
        {
            LastMoveDirection = velocity.normalized;
        }
    }

    /// <summary>
    /// 停止所有移动
    /// </summary>
    public void StopMovement()
    {
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        IsMoving = false;
    }

    /// <summary>
    /// 缓冲跟随玩家：使用缓冲区避免抖动
    /// 距离 > followBufferMax → 移动；距离 < followBufferMin → 停止；中间 → 保持当前状态
    /// </summary>
    public void FollowPlayerWithBuffer()
    {
        if (playerTransform == null || rb == null) return;

        Vector2 playerPos = playerTransform.position;
        Vector2 myPos = transform.position;
        float distanceToPlayer = Vector2.Distance(myPos, playerPos);

        if (distanceToPlayer > followBufferMax)
        {
            // 超出缓冲区上限，向玩家移动
            MoveTowards(playerPos, 1f);
        }
        else if (distanceToPlayer < followBufferMin)
        {
            // 在缓冲区下限内，停止移动
            StopMovement();
        }
        // 在缓冲区内，保持当前速度状态（滞回区，避免抖动）
    }

    /// <summary>
    /// 在玩家周围巡逻：随机选点，到达后停留随机时间
    /// </summary>
    public void PatrolAroundPlayer(float deltaTime)
    {
        if (playerTransform == null || rb == null) return;

        if (isWaitingAtPatrolPoint)
        {
            patrolWaitTimer -= deltaTime;
            StopMovement();
            if (patrolWaitTimer <= 0f)
            {
                isWaitingAtPatrolPoint = false;
                PickNewPatrolTarget();
            }
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);
        if (distanceToTarget > 0.5f)
        {
            MoveTowards(patrolTarget, patrolSpeedMultiplier);
        }
        else
        {
            // 到达巡逻点，开始等待
            isWaitingAtPatrolPoint = true;
            patrolWaitTimer = Random.Range(patrolWaitMin, patrolWaitMax);
            StopMovement();
        }
    }

    /// <summary>
    /// 重新选择巡逻目标点
    /// </summary>
    public void PickNewPatrolTarget()
    {
        if (playerTransform == null) return;

        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = (Vector2)playerTransform.position + randomOffset;
    }

    /// <summary>
    /// 护卫定位：站在玩家与最近敌人之间
    /// 当没有敌人时，退化为在玩家周围随机护卫点巡逻
    /// </summary>
    /// <param name="nearestEnemy">最近的敌人（可为 null）</param>
    public void GuardPositioning(Transform nearestEnemy)
    {
        if (playerTransform == null || rb == null) return;

        if (nearestEnemy != null)
        {
            // 有敌人：定位在玩家与敌人连线上，距离玩家 guardDistance 处
            Vector2 playerPos = playerTransform.position;
            Vector2 enemyPos = nearestEnemy.position;
            Vector2 directionToEnemy = (enemyPos - playerPos).normalized;
            Vector2 guardPosition = playerPos + directionToEnemy * guardDistance;

            // 面向敌人方向
            LastMoveDirection = directionToEnemy;

            float distanceToGuard = Vector2.Distance(transform.position, guardPosition);
            if (distanceToGuard > 0.3f)
            {
                MoveTowards(guardPosition, 1f);
            }
            else
            {
                StopMovement();
            }
        }
        else
        {
            // 无敌人：在玩家周围随机护卫点巡逻
            guardTimer -= Time.deltaTime;
            if (guardTimer <= 0f)
            {
                guardTimer = guardUpdateInterval;
                guardOffset = Random.insideUnitCircle * guardPatrolRadius;
            }

            Vector2 guardPosition = (Vector2)playerTransform.position + guardOffset;
            float distanceToGuard = Vector2.Distance(transform.position, guardPosition);
            if (distanceToGuard > 0.3f)
            {
                MoveTowards(guardPosition, patrolSpeedMultiplier);
            }
            else
            {
                guardTimer = 0f; // 立即刷新新目标
                StopMovement();
            }
        }
    }

    /// <summary>
    /// 获取当前速度
    /// </summary>
    public Vector2 GetVelocity()
    {
        return rb != null ? rb.velocity : Vector2.zero;
    }

    /// <summary>
    /// 获取到玩家的距离
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (playerTransform == null) return float.MaxValue;
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    /// <summary>
    /// 获取到玩家的距离平方
    /// </summary>
    public float GetDistanceToPlayerSqr()
    {
        if (playerTransform == null) return float.MaxValue;
        return ((Vector2)(transform.position - playerTransform.position)).sqrMagnitude;
    }

    /// <summary>
    /// 重置巡逻状态（行为切换时调用）
    /// </summary>
    public void ResetPatrolState()
    {
        isWaitingAtPatrolPoint = false;
        patrolWaitTimer = 0f;
        PickNewPatrolTarget();
    }

    void OnDrawGizmosSelected()
    {
        // 绘制缓冲跟随区间
        if (playerTransform != null)
        {
            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(playerTransform.position, followBufferMin);
            Gizmos.color = new Color(0, 0.5f, 1f, 0.15f);
            Gizmos.DrawWireSphere(playerTransform.position, followBufferMax);
        }

        // 绘制巡逻目标
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolTarget, 0.3f);

        // 绘制护卫距离
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, guardDistance);
        }
    }
}