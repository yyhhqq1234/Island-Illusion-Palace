using UnityEngine;
using System.Collections;

/// <summary>
/// Boss攻击模式基类 — 所有Boss攻击的抽象基类
/// 定义攻击的完整生命周期：前摇 -> 判定 -> 后摇
/// </summary>
public abstract class BossAttackPattern
{
    // ═══════════════════════════════════════════
    // 基础属性（子类通过构造函数设置）
    // ═══════════════════════════════════════════

    /// <summary>攻击名称（用于日志和调试）</summary>
    public string attackName { get; protected set; }

    /// <summary>伤害倍率（基于Boss基础damage计算）</summary>
    public float damageMultiplier { get; protected set; }

    /// <summary>前摇时间（秒）— 攻击准备阶段，播放蓄力提示</summary>
    public float windupDuration { get; protected set; }

    /// <summary>判定持续时间（秒）— 攻击Collider激活的时间窗口</summary>
    public float activeDuration { get; protected set; }

    /// <summary>后摇时间（秒）— 攻击后硬直时间</summary>
    public float recoveryDuration { get; protected set; }

    /// <summary>冷却时间（秒）— 本次攻击结束后到下次可用的时间</summary>
    public float cooldown { get; protected set; }

    /// <summary>最低解锁阶段 — 该攻击在哪个阶段之后才可用</summary>
    public BossPhase minimumPhase { get; protected set; }

    /// <summary>攻击元素类型</summary>
    public ElementType elementType { get; protected set; }

    // ═══════════════════════════════════════════
    // 运行时状态
    // ═════════════════════════════════════════

    /// <summary>当前冷却剩余时间</summary>
    protected float currentCooldown = 0f;

    /// <summary>是否正在执行攻击</summary>
    protected bool isExecuting = false;

    /// <summary>所属Boss引用</summary>
    protected BossAI ownerBoss;

    // ═══════════════════════════════════════════
    // 公共接口
    // ═══════════════════════════════════════════

    /// <summary>检查该攻击是否可以使用</summary>
    public bool IsReady => currentCooldown <= 0f && !isExecuting;

    /// <summary>检查该攻击是否在当前阶段可用</summary>
    public bool IsAvailableInPhase(BossPhase phase) => (int)phase >= (int)minimumPhase;

    /// <summary>设置所属Boss引用</summary>
    public void SetOwner(BossAI boss) => ownerBoss = boss;

    /// <summary>每帧更新冷却</summary>
    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown > 0f)
            currentCooldown -= deltaTime;
    }

    /// <summary>重置冷却（用于狂暴阶段等特殊状态）</summary>
    public void ResetCooldown() => currentCooldown = 0f;

    /// <summary>
    /// 执行攻击的完整流程（协程驱动）
    /// 由 BossCombatController 调用启动
    /// </summary>
    public IEnumerator ExecuteAttack(MonoBehaviour runner)
    {
        if (isExecuting || ownerBoss == null) yield break;
        isExecuting = true;

        // === 阶段1: 前摇（蓄力提示）===
        OnWindupStart();
        yield return new WaitForSeconds(windupDuration);
        OnWindupEnd();

        // === 阶段2: 判定（激活攻击区域）===
        OnActiveStart();
        yield return new WaitForSeconds(activeDuration);
        OnActiveEnd();

        // === 阶段3: 后摇（硬直）===
        OnRecoveryStart();
        yield return new WaitForSeconds(recoveryDuration);
        OnRecoveryEnd();

        // 攻击结束，进入冷却
        isExecuting = false;
        currentCooldown = cooldown;
        OnAttackComplete();
    }

    // ═══════════════════════════════════════════
    // 抽象/虚方法（子类按需重写）
    // ═══════════════════════════════════════════

    /// <summary>前摇开始 — 播放蓄力动画/特效提示</summary>
    protected virtual void OnWindupStart()
    {
        if (ownerBoss.animator != null)
            ownerBoss.animator.SetTrigger("Attack");
        Debug.Log($"[BossAttack:{attackName}] 开始蓄力（前摇 {windupDuration:F1}s）");
    }

    /// <summary>前摇结束</summary>
    protected virtual void OnWindupEnd() { }

    /// <summary>判定开始 — 激活攻击Collider，检测命中</summary>
    protected virtual void OnActiveStart() { }

    /// <summary>判定结束 — 关闭攻击Collider</summary>
    protected virtual void OnActiveEnd() { }

    /// <summary>后摇开始 — 进入硬直</summary>
    protected virtual void OnRecoveryStart() { }

    /// <summary>后摇结束</summary>
    protected virtual void OnRecoveryEnd() { }

    /// <summary>攻击完全结束</summary>
    protected virtual void OnAttackComplete()
    {
        Debug.Log($"[BossAttack:{attackName}] 攻击完成，进入冷却 {cooldown:F1}s");
    }
}

// ════════════════════════════════════════════════════════════════════════
// 5种具体攻击实现 — 时空守护者专用
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// 攻击#1: 晶能斩击 — 近战扇形攻击
/// 范围: 前方120度角/3米半径 | 伤害: 1.0x | 无特殊效果
/// </summary>
public class CrystalSlashAttack : BossAttackPattern
{
    [Header("晶能斩击参数")]
    public float slashAngle = 120f;      // 扇形角度
    public float slashRadius = 3f;       // 攻击半径

    private Collider2D slashCollider;     // 攻击判定碰撞体

    public CrystalSlashAttack()
    {
        attackName = "晶能斩击";
        damageMultiplier = 1.0f;
        windupDuration = 0.4f;
        activeDuration = 0.3f;
        recoveryDuration = 0.5f;
        cooldown = 2.0f;
        minimumPhase = BossPhase.Phase1;
        elementType = ElementType.Soul;
    }

    protected override void OnWindupStart()
    {
        base.OnWindupStart();
        // 蓄力时面向玩家
        if (ownerBoss.player != null && ownerBoss.rb != null)
        {
            // 通过Animator方向参数控制朝向
            FacePlayer();
        }
    }

    protected override void OnActiveStart()
    {
        // 扇形范围检测
        Vector2 bossPos = ownerBoss.transform.position;
        Vector2 playerPos = ownerBoss.player != null ? (Vector2)ownerBoss.player.position : bossPos;
        Vector2 toPlayer = (playerPos - bossPos).normalized;
        float angleToPlayer = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapCircleAll(bossPos, slashRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Vector2 toTarget = ((Vector2)hit.transform.position - bossPos).normalized;
            float angleToTarget = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angleToPlayer, angleToTarget));

            if (angleDiff <= slashAngle * 0.5f)
            {
                DealDamage(hit.gameObject, ownerBoss.damage * damageMultiplier);
            }
        }

        Debug.Log($"[晶能斩击] 扇形判定完成 角度{slashAngle}° 半径{slashRadius}m");
    }

    private void FacePlayer()
    {
        if (ownerBoss.player == null) return;
        Vector2 dir = ((Vector2)(ownerBoss.player.position - ownerBoss.transform.position)).normalized;
        // 保持Rigidbody2D velocity为0（攻击时停止移动）
        ownerBoss.rb.velocity = Vector2.zero;
    }

    private void DealDamage(GameObject target, float damage)
    {
        var health = target.GetComponent<HealthSystem>();
        if (health != null && !health.IsDead())
        {
            health.TakeDamage(damage);
            GlobalEventManager.Instance.TriggerDamageTaken(ownerBoss.gameObject, damage);
            Debug.Log($"[晶能斩击] 命中目标 造成 {damage:F1} 点伤害");
        }
    }
}

/// <summary>
/// 攻击#2: 时空冲击波 — 远程直线弹幕
/// 范围: 正前方8m长/2m宽 | 伤害: 0.8x | 减速30%持续2s
/// </summary>
public class TimeShockwaveAttack : BossAttackPattern
{
    public float waveLength = 8f;         // 冲击波长度
    public float waveWidth = 2f;          // 冲击波宽度
    public float slowStrength = 0.7f;     // 减速强度（剩余速度比例）
    public float slowDuration = 2f;       // 减速持续时间

    public TimeShockwaveAttack()
    {
        attackName = "时空冲击波";
        damageMultiplier = 0.8f;
        windupDuration = 0.6f;
        activeDuration = 0.5f;
        recoveryDuration = 0.8f;
        cooldown = 4.0f;
        minimumPhase = BossPhase.Phase1;
        elementType = ElementType.Soul;
    }

    protected override void OnActiveStart()
    {
        Vector2 bossPos = ownerBoss.transform.position;
        Vector2 direction = Vector2.up; // 默认向上发射

        if (ownerBoss.player != null)
        {
            direction = ((Vector2)(ownerBoss.player.position - ownerBoss.transform.position)).normalized;
        }

        // 矩形区域检测（用BoxCast模拟直线冲击波）
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            bossPos,
            new Vector2(waveWidth, waveLength),
            Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg,
            direction,
            waveLength * 0.5f
        );

        foreach (var hit in hits)
        {
            if (!hit.collider.CompareTag("Player")) continue;

            DealDamageWithSlow(hit.collider.gameObject);
        }

        Debug.Log($"[时空冲击波] 直线冲击波释放 方向={direction} 长度={waveLength}m");
    }

    private void DealDamageWithSlow(GameObject target)
    {
        var health = target.GetComponent<HealthSystem>();
        if (health != null && !health.IsDead())
        {
            health.TakeDamage(ownerBoss.damage * damageMultiplier);
            GlobalEventManager.Instance.TriggerDamageTaken(ownerBoss.gameObject, ownerBoss.damage * damageMultiplier);

            // 施加减速效果
            var pc = target.GetComponent<PlayerController>();
            if (pc != null && ownerBoss is MonoBehaviour mb)
            {
                mb.StartCoroutine(ApplySlowEffect(pc));
            }
        }
    }

    IEnumerator ApplySlowEffect(PlayerController pc)
    {
        if (pc == null) yield break;
        float originalSpeed = pc.moveSpeed;
        float originalRunSpeed = pc.runSpeed;
        pc.moveSpeed *= slowStrength;
        pc.runSpeed *= slowStrength;
        Debug.Log($"[时空冲击波] 玩家被减速 {(1f - slowStrength) * 100}% 持续{slowDuration}s");
        yield return new WaitForSeconds(slowDuration);
        // 安全恢复（防止玩家在减速期间死亡导致空引用）
        if (pc != null)
        {
            pc.moveSpeed = originalSpeed;
            pc.runSpeed = originalRunSpeed;
        }
    }
}

/// <summary>
/// 攻击#3: 水晶牢笼 — 范围AOE困住
/// 范围: 目标点4m半径 | 伤害: 0.6x | 困住玩家1.5s
/// </summary>
public class CrystalCageAttack : BossAttackPattern
{
    public float cageRadius = 4f;          // 牢笼半径
    public float cageDuration = 1.5f;      // 困住持续时间

    public CrystalCageAttack()
    {
        attackName = "水晶牢笼";
        damageMultiplier = 0.6f;
        windupDuration = 0.8f;
        activeDuration = 0.3f;
        recoveryDuration = 1.0f;
        cooldown = 6.0f;
        minimumPhase = BossPhase.Phase2;
        elementType = ElementType.Soul;
    }

    protected override void OnActiveStart()
    {
        // 以玩家当前位置为中心生成牢笼
        Vector3 cageCenter = ownerBoss.player != null
            ? ownerBoss.player.position
            : ownerBoss.transform.position + ownerBoss.transform.up * 3f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(cageCenter, cageRadius);
        bool hitPlayer = false;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            hitPlayer = true;
            var health = hit.GetComponent<HealthSystem>();
            if (health != null && !health.IsDead())
            {
                health.TakeDamage(ownerBoss.damage * damageMultiplier);
                GlobalEventManager.Instance.TriggerDamageTaken(ownerBoss.gameObject, ownerBoss.damage * damageMultiplier);
            }

            // 困住效果（通过PlayerController接口施加禁锢）
            var pc = hit.GetComponent<PlayerController>();
            if (pc != null && ownerBoss is MonoBehaviour mb)
            {
                mb.StartCoroutine(ApplyImprisonEffect(pc, cageCenter));
            }
        }

        Debug.Log($"[水晶牢笼] 牢笼生成于 ({cageCenter.x:F1}, {cageCenter.y:F1}) 半径{cageRadius}m" +
                  (hitPlayer ? " 命中玩家！" : ""));
    }

    IEnumerator ApplyImprisonEffect(PlayerController pc, Vector3 cageCenter)
    {
        if (pc == null) yield break;
        // 禁锢期间将玩家拉向牢笼中心并禁止移动
        Rigidbody2D playerRb = pc.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.velocity = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < cageDuration && pc != null)
        {
            // 将玩家位置约束在牢笼中心附近
            if (playerRb != null) playerRb.velocity = Vector2.zero;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[水晶牢笼] 囚禁结束 持续{cageDuration}s");
    }
}

/// <summary>
/// 攻击#4: 裂隙冲锋 — 穿越直线突进
/// 范围: 6m直线 | 伤害: 1.2x | 路径残留伤害区
/// </summary>
public class RiftDashAttack : BossAttackPattern
{
    public float dashDistance = 6f;        // 冲锋距离
    public float dashSpeed = 18f;          // 冲锋速度
    public float trailDamageInterval = 0.3f;// 路径伤害判定间隔
    public float trailDamagePercent = 0.3f;// 路径残留伤害比例(相对基础damage)

    private Vector3 dashStartPosition;
    private Vector3 dashEndPosition;
    private bool trailActive = false;

    public RiftDashAttack()
    {
        attackName = "裂隙冲锋";
        damageMultiplier = 1.2f;
        windupDuration = 0.3f;
        activeDuration = 0.4f;
        recoveryDuration = 0.6f;
        cooldown = 5.0f;
        minimumPhase = BossPhase.Phase2;
        elementType = ElementType.Soul;
    }

    protected override void OnWindupStart()
    {
        base.OnWindupStart();
        // 记录冲锋起点和终点
        dashStartPosition = ownerBoss.transform.position;
        if (ownerBoss.player != null)
        {
            Vector2 dir = ((Vector2)(ownerBoss.player.position - ownerBoss.transform.position)).normalized;
            dashEndPosition = dashStartPosition + (Vector3)(dir * dashDistance);
        }
        else
        {
            dashEndPosition = dashStartPosition + Vector3.up * dashDistance;
        }
    }

    protected override void OnActiveStart()
    {
        trailActive = true;
        // 给Boss一个瞬间的冲力
        if (ownerBoss.rb != null)
        {
            Vector2 dir = ((Vector2)(dashEndPosition - dashStartPosition)).normalized;
            ownerBoss.rb.velocity = dir * dashSpeed;
        }

        // 直接对路径上的敌人造成伤害
        CheckDashCollision();

        if (ownerBoss is MonoBehaviour mb)
        {
            mb.StartCoroutine(TrailDamageCoroutine());
        }
    }

    protected override void OnActiveEnd()
    {
        trailActive = false;
        // 停止移动
        if (ownerBoss.rb != null)
            ownerBoss.rb.velocity = Vector2.zero;
    }

    void CheckDashCollision()
    {
        Vector2 dir = ((Vector2)(dashEndPosition - dashStartPosition)).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            dashStartPosition,
            dir,
            dashDistance
        );

        foreach (var hit in hits)
        {
            if (!hit.collider.CompareTag("Player")) continue;

            var health = hit.collider.GetComponent<HealthSystem>();
            if (health != null && !health.IsDead())
            {
                health.TakeDamage(ownerBoss.damage * damageMultiplier);
                GlobalEventManager.Instance.TriggerDamageTaken(ownerBoss.gameObject, ownerBoss.damage * damageMultiplier);
                Debug.Log($"[裂隙冲锋] 冲锋命中 造成 {ownerBoss.damage * damageMultiplier:F1} 点伤害");
            }
        }
    }

    IEnumerator TrailDamageCoroutine()
    {
        // 路径残留伤害：每隔一段时间检测一次
        float elapsed = 0f;
        while (elapsed < activeDuration && trailActive)
        {
            // 在冲锋路径上做圆形检测
            Vector2 currentPos = ownerBoss.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(currentPos, 1.5f);
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Player")) continue;
                var health = hit.GetComponent<HealthSystem>();
                if (health != null && !health.IsDead())
                {
                    health.TakeDamage(ownerBoss.damage * trailDamagePercent);
                    GlobalEventManager.Instance.TriggerDamageTaken(ownerBoss.gameObject, ownerBoss.damage * trailDamagePercent);
                }
            }
            yield return new WaitForSeconds(trailDamageInterval);
            elapsed += trailDamageInterval;
        }
    }
}

/// <summary>
/// 攻击#5: 时空崩坏 — 全屏AOE终极技能（Phase3+解锁）
/// 范围: 全屏幕 | 伤害: 1.5x | 强制推离中心5m
/// </summary>
public class SpaceCollapseAttack : BossAttackPattern
{
    public float collapseRadius = 15f;       // 实际影响半径（覆盖整个屏幕）
    public float pushForce = 5f;             // 推离力度
    public float innerSafeRadius = 2f;       // 内圈安全区（贴近Boss反而不受推力）
    public float warningDuration = 0.3f;     // 额外预警时间（加在前摇之前）

    public SpaceCollapseAttack()
    {
        attackName = "时空崩坏";
        damageMultiplier = 1.5f;
        windupDuration = 1.2f;               // 长前摇给玩家反应时间
        activeDuration = 0.5f;
        recoveryDuration = 1.5f;             // 长后摇（高风险高回报的设计）
        cooldown = 10.0f;                    // 长冷却
        minimumPhase = BossPhase.Phase3;     // 仅第三阶段和狂暴可用
        elementType = ElementType.Soul;
    }

    protected override void OnWindupStart()
    {
        base.OnWindupStart();
        // 全屏攻击需要更明显的预警
        Debug.Log("[时空崩坏] ★★★ 终极技能蓄力中！请尽快远离Boss！★★★");

        // 广播事件让UI显示警告
        GlobalEventManager.Instance.ShowNotification("时空守护者正在积蓄毁灭性能量！", warningDuration + windupDuration);

        // Boss短暂无敌
        ownerBoss.SetInvulnerable(windupDuration + 0.1f);
    }

    protected override void OnActiveStart()
    {
        Vector2 bossPos = ownerBoss.transform.position;

        // 全屏AOE检测
        Collider2D[] hits = Physics2D.OverlapCircleAll(bossPos, collapseRadius);
        int hitCount = 0;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            hitCount++;
            var health = hit.GetComponent<HealthSystem>();
            if (health != null && !health.IsDead())
            {
                health.TakeDamage(ownerBoss.damage * damageMultiplier);
                GlobalEventManager.Instance.TriggerDamageTaken(ownerBoss.gameObject, ownerBoss.damage * damageMultiplier);
            }

            // 推离效果
            PushPlayerFromCenter(hit.gameObject, bossPos);
        }

        Debug.Log($"[时空崩坏] ★★★ 时空崩坏爆发！影响半径{collapseRadius}m 命中{hitCount}个目标 ★★★");
    }

    void PushPlayerFromCenter(GameObject player, Vector2 center)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector2 fromCenter = (Vector2)player.transform.position - center;
        float distance = fromCenter.magnitude;

        // 内圈安全区不推离（鼓励近战博弈）
        if (distance < innerSafeRadius) return;

        Vector2 pushDir = fromCenter.normalized;
        // 推离力度随距离衰减
        float forceMagnitude = pushForce * (1f - Mathf.Clamp01(distance / collapseRadius));
        playerRb.velocity = pushDir * forceMagnitude;

        Debug.Log($"[时空崩坏] 玩家被推离 力度={forceMagnitude:F1}");
    }

    protected override void OnRecoveryStart()
    {
        base.OnRecoveryStart();
        // 后摇期间Boss也处于脆弱状态（不能行动）
        Debug.Log("[时空崩坏] Boss进入长时间硬直...");
    }
}
