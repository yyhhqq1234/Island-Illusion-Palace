using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用攻击触发器 - 支持玩家和敌人攻击
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    [Header("基础设置")]
    public int damage = 10;
    public float lifetime = 0.2f;
    public bool isPlayerAttack = false;
    public bool isSummonAttack = false;
    public bool canHitMultipleTargets = false;

    [Header("范围检测设置")]
    [Tooltip("是否启用主动范围检测，确保范围内的目标一定会受到伤害")]
    public bool enableAreaDetection = true;
    [Tooltip("范围检测的额外扩展距离")]
    public float detectionExpansion = 0.1f;
    [Tooltip("是否启用详细的命中调试日志")]
    public bool debugHitDetection = false;

    [Header("攻击者")]
    public GameObject attacker;

    private readonly HashSet<GameObject> hitTargets = new();

    void Start()
    {
        PerformAreaAttack();
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(int damageValue) => damage = damageValue;
    public void SetAttacker(GameObject attackerObj, bool isPlayer = false, bool isSummon = false)
    {
        attacker = attackerObj;
        isPlayerAttack = isPlayer;
        isSummonAttack = isSummon;
    }

    void PerformAreaAttack()
    {
        if (!enableAreaDetection || attacker == null) return;

        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (debugHitDetection)
        {
            string attackerType = isPlayerAttack ? "玩家" : (isSummonAttack ? "召唤物" : "敌人");
            Debug.Log($"🔍 {attackerType}执行范围攻击检测，位置: {transform.position}");
        }
#endif

        string[] targetTags = isPlayerAttack || isSummonAttack ? new[] { "Enemy" } : new[] { "Player", "SummonedCreature" };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (debugHitDetection) Debug.Log($"🎯 目标标签: {string.Join(", ", targetTags)}");
#endif

        foreach (string tag in targetTags)
        {
            GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject target in potentialTargets)
            {
                if (target == null || target == attacker) continue;
                HealthSystem targetHealth = target.GetComponent<HealthSystem>();
                if (targetHealth == null) continue;

                if (IsTargetInRange(target, triggerCollider))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (debugHitDetection)
                        Debug.Log($"✅ 检测到目标在范围内: {target.name} (距离: {Vector3.Distance(transform.position, target.transform.position):F2})");
#endif
                    ApplyDamageToTarget(target, targetHealth);
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else if (debugHitDetection)
                    Debug.Log($"❌ 目标不在范围内: {target.name} (距离: {Vector3.Distance(transform.position, target.transform.position):F2})");
#endif
            }
        }
    }

    bool IsTargetInRange(GameObject target, Collider2D triggerCollider)
    {
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (targetCollider != null)
        {
            Bounds expandedBounds = triggerCollider.bounds;
            expandedBounds.Expand(detectionExpansion);
            if (expandedBounds.Intersects(targetCollider.bounds))
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                float maxRange = triggerCollider.bounds.extents.magnitude + detectionExpansion;
                return distance <= maxRange;
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            float triggerRadius = triggerCollider.bounds.extents.magnitude + detectionExpansion;
            return distance <= triggerRadius;
        }
        return false;
    }

    void ApplyDamageToTarget(GameObject target, HealthSystem targetHealth)
    {
        if (!canHitMultipleTargets && hitTargets.Contains(target)) return;
        hitTargets.Add(target);

        targetHealth.TakeDamage(damage);
        targetHealth.TriggerHitEffect();

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.ShowDamage(target.transform.position + Vector3.up * 0.5f, damage);

        if (isPlayerAttack || isSummonAttack)
        {
            try
            {
                var audioManager = FindObjectOfType<GameplayAudioManager>();
                audioManager?.GetType().GetMethod("PlayEnemyHit")?.Invoke(audioManager, null);
            }
            catch { }
        }

        BattleEventManager.Instance?.TriggerDamageDealt(attacker, damage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string attackerType = isPlayerAttack ? "玩家" : (isSummonAttack ? "召唤物" : "敌人");
        string targetType = target.CompareTag("Enemy") ? "敌人" : target.CompareTag("Player") ? "玩家" : "召唤物";
        Debug.Log($"{attackerType}攻击命中{targetType}: {target.name}，造成{damage}点伤害");
#endif
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        GameObject target = collision.gameObject;
        if (hitTargets.Contains(target)) return;

        if (isSummonAttack || isPlayerAttack ? target.CompareTag("Enemy") :
            target.CompareTag("Player") || target.CompareTag("SummonedCreature"))
        {
            HealthSystem targetHealth = target.GetComponent<HealthSystem>();
            if (targetHealth != null) ApplyDamageToTarget(target, targetHealth);
        }
    }

    /// <summary>
    /// 测试范围检测功能（仅在编辑器中可用）
    /// </summary>
    [ContextMenu("测试范围检测")]
    void TestAreaDetection()
    {
        #if UNITY_EDITOR
        Debug.Log("🧪 开始测试范围检测功能...");
        enableAreaDetection = true;
        debugHitDetection = true;

        // 执行范围检测
        PerformAreaAttack();

        Debug.Log("🧪 范围检测测试完成");
        #endif
    }

    /// <summary>
    /// 重置调试设置
    /// </summary>
    [ContextMenu("重置调试设置")]
    void ResetDebugSettings()
    {
        #if UNITY_EDITOR
        debugHitDetection = false;
        Debug.Log("🔄 已重置调试设置");
        #endif
    }
}
