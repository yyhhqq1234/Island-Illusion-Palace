using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 敌人战斗组件 - 负责攻击、伤害计算、攻击触发器管理
/// </summary>
public class EnemyCombatComponent : MonoBehaviour
{
    [Header("攻击设置")]
    public float baseDamage = 10f;
    public float baseAttackRate = 1f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;

    [Header("攻击触发器")]
    public GameObject enemyAttackTriggerPrefab;
    public Transform attackTriggerPos;

    // 事件
    public event Action OnAttackPerformed;
    public event Action OnAttackCompleted;

    // 内部状态
    private float lastAttackTime = 0f;
    private bool canAttack = true;
    private EnemyStateComponent stateComponent;
    private EnemyAnimationComponent animationComponent;

    public void Initialize(EnemyStateComponent state, EnemyAnimationComponent animation)
    {
        stateComponent = state;
        animationComponent = animation;
    }

    /// <summary>
    /// 获取当前攻击速率（综合状态倍率）
    /// </summary>
    public float GetCurrentAttackRate()
    {
        if (stateComponent != null)
            return stateComponent.GetAttackSpeedMultiplier(baseAttackRate);
        return baseAttackRate;
    }

    /// <summary>
    /// 计算当前伤害（含随机加成）
    /// </summary>
    public float GetCurrentDamage()
    {
        float randomBonus = UnityEngine.Random.Range(0, 6);
        return baseDamage + randomBonus;
    }

    /// <summary>
    /// 检查是否可以攻击
    /// </summary>
    public bool CanAttack()
    {
        float attackInterval = 1f / GetCurrentAttackRate();
        return Time.time - lastAttackTime >= attackInterval && canAttack;
    }

    /// <summary>
    /// 获取攻击间隔时间
    /// </summary>
    public float GetAttackInterval()
    {
        return 1f / GetCurrentAttackRate();
    }

    /// <summary>
    /// 获取逃跑状态下的攻击间隔
    /// </summary>
    public float GetFleeAttackInterval()
    {
        float baseInterval = GetAttackInterval();
        if (stateComponent != null)
            baseInterval *= stateComponent.GetFleeAttackIntervalMultiplier();
        return baseInterval;
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    public void PerformAttack(Transform target)
    {
        if (!CanAttack() || target == null) return;

        lastAttackTime = Time.time;
        canAttack = false;

        // 触发攻击动画
        animationComponent?.TriggerAttack();

        // 播放攻击音效
        try { GameplayAudioManager.Instance?.PlayEnemyHit(); } catch { }

        // 执行伤害逻辑
        DealDamageToTarget(target);

        OnAttackPerformed?.Invoke();

        // 延迟重置攻击状态
        StartCoroutine(ResetAttackCoroutine());
    }

    private void DealDamageToTarget(Transform target)
    {
        if (enemyAttackTriggerPrefab != null && attackTriggerPos != null)
        {
            // 使用攻击触发器
            GameObject trigger = Instantiate(enemyAttackTriggerPrefab, attackTriggerPos.position, attackTriggerPos.rotation, attackTriggerPos);
            var attackTriggerType = System.Reflection.Assembly.GetExecutingAssembly().GetType("AttackTrigger");
            if (attackTriggerType != null)
            {
                var attackTrigger = trigger.GetComponent(attackTriggerType);
                if (attackTrigger != null)
                {
                    var setDamageMethod = attackTriggerType.GetMethod("SetDamage");
                    var setAttackerMethod = attackTriggerType.GetMethod("SetAttacker");

                    if (setDamageMethod != null)
                    {
                        int finalDamage = (int)GetCurrentDamage();
                        setDamageMethod.Invoke(attackTrigger, new object[] { finalDamage });
                    }
                    if (setAttackerMethod != null)
                    {
                        setAttackerMethod.Invoke(attackTrigger, new object[] { gameObject, false, false });
                    }
                }
            }
        }
        else
        {
            // 直接应用伤害
            var targetHealth = target.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(GetCurrentDamage());
            }
        }
    }

    /// <summary>
    /// 检查目标是否在攻击范围内
    /// </summary>
    public bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;
        float attackRangeSqr = attackRange * attackRange;
        float distanceSqr = ((Vector2)(target.position - transform.position)).sqrMagnitude;
        return distanceSqr <= attackRangeSqr;
    }

    /// <summary>
    /// 检查目标是否在攻击范围内（使用缓存距离值）
    /// </summary>
    public bool IsTargetInRange(float cachedDistanceSqr)
    {
        float attackRangeSqr = attackRange * attackRange;
        return cachedDistanceSqr <= attackRangeSqr;
    }

    /// <summary>
    /// 获取攻击范围平方
    /// </summary>
    public float GetAttackRangeSqr()
    {
        return attackRange * attackRange;
    }

    private IEnumerator ResetAttackCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        OnAttackCompleted?.Invoke();
    }
}