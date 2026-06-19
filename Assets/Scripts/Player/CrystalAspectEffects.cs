using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 晶能变体具体效果策略实现
/// 包含4种特殊效果：散射、牵引、共鸣、收割
/// 从 WeaponSystem 中提取，遵循策略模式
/// </summary>

#region 散射攻击（晶能武装 - 散射晶能）
/// 发射3枚散射弹，每枚独立伤害判定
public class ScatterEffect : ICrystalAspectEffect
{
    private const int ProjectileCount = 3;
    private const float SpreadAngle = 30f;

    public bool CanExecute(CrystalAspectType aspectType)
    {
        return aspectType == CrystalAspectType.SpreadCrystal;
    }

    public bool Execute(WeaponSystem ws, float damage)
    {
        Vector3 mousePos = ws.MainCamera != null
            ? ws.MainCamera.ScreenToWorldPoint(Input.mousePosition)
            : ws.transform.position + ws.transform.right * 5f;
        mousePos.z = 0f;
        Vector3 baseDirection = (mousePos - ws.transform.position).normalized;

        float startAngle = -SpreadAngle / 2f;
        float angleStep = ProjectileCount > 1 ? SpreadAngle / (ProjectileCount - 1) : 0f;

        for (int i = 0; i < ProjectileCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = Quaternion.Euler(0, 0,
                Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg + angle);

            ws.InstantiateAttackTrigger(rotation, (int)damage);
        }

        Debug.Log($"[ScatterEffect] 发射{ProjectileCount}枚散射弹，每枚伤害{damage}");
        return true; // 已完全接管攻击
    }
}
#endregion

#region 牵引攻击（法杖/镰刀）
/// 将命中敌人向玩家方向牵引
public class PullEffect : ICrystalAspectEffect
{
    private const float PullForce = 5f;

    public bool CanExecute(CrystalAspectType aspectType)
    {
        return aspectType == CrystalAspectType.PullStaff
            || aspectType == CrystalAspectType.HookScythe;
    }

    public bool Execute(WeaponSystem ws, float damage)
    {
        float pullRange = ws.AttackRange * ws.GetCrystalAspectRangeMultiplier();
        var hits = ws.FindEnemiesInRange(pullRange);

        foreach (var hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            float finalDamage = damage;

            if (enemy != null)
                finalDamage = ws.CalculateDamageToEnemy(enemy);

            // 应用收割加成（如果当前变体是收割镰刀，在牵引时也生效）
            finalDamage *= ws.GetResonanceBonus();
            finalDamage = ws.GetReaperBonus(finalDamage, enemy);

            hit.GetComponent<HealthSystem>()?.TakeDamage(finalDamage);

            // 牵引力
            Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 pullDir = ((Vector2)ws.transform.position - enemyRb.position).normalized;
                enemyRb.AddForce(pullDir * PullForce, ForceMode2D.Impulse);
            }
        }

        Debug.Log($"[PullEffect] 牵引攻击：伤害{damage}，牵引力{PullForce}，命中{hits.Count}个目标");
        return true; // 已完全接管攻击
    }
}
#endregion

#region 共鸣加成（晶能武装 - 共鸣晶能）
/// 攻击命中时场上每有1个友方召唤物伤害+10%，最多+30%
public class ResonanceEffect : ICrystalAspectEffect
{
    private const float BonusPerSummon = 0.10f;
    private const float MaxBonus = 0.30f;

    public bool CanExecute(CrystalAspectType aspectType)
    {
        return aspectType == CrystalAspectType.ResonanceCrystal;
    }

    public bool Execute(WeaponSystem ws, float damage)
    {
        // 共鸣不是独立攻击效果，而是伤害修正器
        // 此方法不接管攻击流程，返回 false 让标准攻击继续执行
        return false;
    }

    /// <summary>计算共鸣伤害倍率</summary>
    public float CalculateMultiplier(WeaponSystem ws)
    {
        if (!ws.HasCrystalCore || ws.CurrentCrystalAspect != CrystalAspectType.ResonanceCrystal)
            return 1f;

        SummonSystem summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem == null) return 1f;

        int activeCount = ws.GetActiveSummonCount(summonSystem);
        float multiplier = 1f + Mathf.Min(activeCount * BonusPerSummon, MaxBonus);

        if (activeCount > 0)
            Debug.Log($"[ResonanceEffect] 场上{activeCount}个召唤物，伤害倍率{multiplier:P0}");

        return multiplier;
    }
}
#endregion

#region 收割判定（镰刀 - 收割镰刀）
/// 对生命低于30%的敌人伤害+40%
public class ReaperEffect : ICrystalAspectEffect
{
    private const float HealthThreshold = 0.30f;
    private const float BonusMultiplier = 1.40f;

    public bool CanExecute(CrystalAspectType aspectType)
    {
        return aspectType == CrystalAspectType.ReaperScythe;
    }

    public bool Execute(WeaponSystem ws, float damage)
    {
        // 收割不是独立攻击效果，而是伤害修正器
        return false;
    }

    /// <summary>计算收割后的伤害值</summary>
    public float ApplyBonus(WeaponSystem ws, float damage, EnemyAI enemy)
    {
        if (!ws.HasCrystalCore || ws.CurrentCrystalAspect != CrystalAspectType.ReaperScythe)
            return damage;
        if (enemy == null) return damage;

        HealthSystem health = enemy.GetComponent<HealthSystem>();
        if (health == null) return damage;

        float healthPercent = health.currentHealth / health.maxHealth;
        if (healthPercent < HealthThreshold)
        {
            float bonus = damage * BonusMultiplier;
            Debug.Log($"[ReaperEffect] 收割触发：敌人生命{healthPercent:P0}，伤害+40% ({damage:F0} -> {bonus:F0})");
            return bonus;
        }

        return damage;
    }
}
#endregion
