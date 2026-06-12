using UnityEngine;

/// <summary>
/// 召唤物战斗组件 — 负责目标寻找、攻击执行、伤害计算
/// 优化：使用 OverlapCircleNonAlloc 减少 GC 分配
/// </summary>
public class SummonCombatComponent : MonoBehaviour
{
    [Header("战斗设置")]
    [Tooltip("基础伤害")]
    public float baseDamage = 10f;
    [Tooltip("攻击范围")]
    public float attackRange = 1.5f;
    [Tooltip("检测范围（寻找敌人的范围）")]
    public float detectionRange = 8f;
    [Tooltip("攻击冷却时间（秒）")]
    public float attackCooldown = 1f;

    [Header("伤害加成")]
    [Tooltip("伤害加成比例")]
    public float damageBonus = 0f;
    [Tooltip("暴击率加成")]
    public float critBonus = 0f;
    [Tooltip("Boss伤害加成")]
    public float bossDamageBonus = 0f;
    [Tooltip("冷却时间缩减")]
    public float cooldownReduction = 0f;

    // 内部状态
    private Transform currentTarget;
    private float lastAttackTime = 0f;

    // NonAlloc 缓冲区
    private Collider2D[] overlapResults = new Collider2D[32];

    // 灵魂之核数据引用
    private SoulCoreData soulCoreData;

    public Transform CurrentTarget => currentTarget;

    /// <summary>
    /// 设置灵魂之核数据（用于伤害计算）
    /// </summary>
    public void SetSoulCoreData(SoulCoreData core)
    {
        soulCoreData = core;
    }

    /// <summary>
    /// 设置基础伤害
    /// </summary>
    public void SetBaseDamage(float damage)
    {
        baseDamage = damage;
    }

    /// <summary>
    /// 寻找最近的敌人（使用 NonAlloc 减少 GC）
    /// </summary>
    /// <returns>最近的敌人 Transform，未找到返回 null</returns>
    public Transform FindNearestEnemy()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, detectionRange, overlapResults);

        Transform nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector2 currentPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col == null) continue;

            if (col.CompareTag("Enemy"))
            {
                float distSqr = ((Vector2)col.transform.position - currentPos).sqrMagnitude;
                if (distSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distSqr;
                    nearestEnemy = col.transform;
                }
            }
        }

        currentTarget = nearestEnemy;
        return nearestEnemy;
    }

    /// <summary>
    /// 在指定范围内寻找敌人
    /// </summary>
    public Transform FindNearestEnemy(float range)
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, range, overlapResults);

        Transform nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector2 currentPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col == null) continue;

            if (col.CompareTag("Enemy"))
            {
                float distSqr = ((Vector2)col.transform.position - currentPos).sqrMagnitude;
                if (distSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distSqr;
                    nearestEnemy = col.transform;
                }
            }
        }

        currentTarget = nearestEnemy;
        return nearestEnemy;
    }

    /// <summary>
    /// 判断敌人是否在攻击范围内
    /// </summary>
    public bool IsEnemyInRange(Transform enemy)
    {
        if (enemy == null) return false;
        float distSqr = ((Vector2)(enemy.position - transform.position)).sqrMagnitude;
        return distSqr <= attackRange * attackRange;
    }

    /// <summary>
    /// 判断敌人是否在攻击范围内（使用缓存距离平方值）
    /// </summary>
    public bool IsEnemyInRange(float cachedDistanceSqr)
    {
        return cachedDistanceSqr <= attackRange * attackRange;
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    /// <param name="target">攻击目标</param>
    /// <returns>是否成功攻击</returns>
    public bool PerformAttack(Transform target)
    {
        if (target == null) return false;

        float effectiveCooldown = attackCooldown - cooldownReduction;
        if (Time.time - lastAttackTime < effectiveCooldown)
            return false;

        float finalDamage = CalculateDamage(target);
        ApplyDamageToTarget(target, finalDamage);

        lastAttackTime = Time.time;

        Debug.Log($"[SummonCombat] 攻击 {target.name}，造成 {finalDamage:F1} 点伤害");
        return true;
    }

    /// <summary>
    /// 计算最终伤害
    /// </summary>
    private float CalculateDamage(Transform target)
    {
        float finalDamage = baseDamage * (1f + damageBonus);

        // Boss 伤害加成
        EnemyAI enemyAI = target.GetComponent<EnemyAI>();
        if (enemyAI != null && enemyAI.enemyQuality == EnemyAI.EnemyQuality.Boss)
        {
            finalDamage *= (1f + bossDamageBonus);
        }

        // 暴击判定
        if (Random.value < critBonus)
        {
            finalDamage *= 1.5f;
            Debug.Log("[SummonCombat] 召唤物暴击！");
        }

        return finalDamage;
    }

    /// <summary>
    /// 对目标施加伤害
    /// </summary>
    private void ApplyDamageToTarget(Transform target, float damage)
    {
        HealthSystem enemyHealth = target.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 获取当前目标
    /// </summary>
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    /// <summary>
    /// 清除当前目标
    /// </summary>
    public void ClearTarget()
    {
        currentTarget = null;
    }

    /// <summary>
    /// 获取到当前目标的距离平方
    /// </summary>
    public float GetDistanceToTargetSqr()
    {
        if (currentTarget == null) return float.MaxValue;
        return ((Vector2)(currentTarget.position - transform.position)).sqrMagnitude;
    }

    void OnDrawGizmosSelected()
    {
        // 检测范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 当前目标连线
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}