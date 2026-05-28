using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BossPhase { Phase1, Phase2, Phase3, Enraged }

public class BossAI : MonoBehaviour, IEnemyProvider, IBossPhaseProvider
{
    float IEnemyProvider.damage { get => damage; set => damage = value; }
    bool IEnemyProvider.isBerserk { get => isEnraged; set => isEnraged = value; }
    void IEnemyProvider.EnterBerserkState() { isEnraged = true; }
    void IEnemyProvider.TakeDamage(float amount) { TakeDamage(amount); }
    BossPhase IBossPhaseProvider.currentPhase { get => currentPhase; set => currentPhase = value; }

    [Header("BOSS属性")]
    public float maxHealth = 500f;
    public float currentHealth;
    public float damage = 20f;
    public float attackRange = 3f;
    private List<ElementType> weaknesses = new List<ElementType>();
    private List<ElementType> resistances = new List<ElementType>();
    public float chaseRange = 15f;
    public float moveSpeed = 3f;
    public float phaseChangeThreshold = 0.7f;

    [Header("BOSS阶段")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float[] phaseHealthThresholds = { 1f, 0.70f, 0.40f, 0.10f };

    [Header("技能设置")]
    public List<BossSkill> skills = new List<BossSkill>();
    public float skillCooldown = 2f;
    private float skillTimer = 0f;

    [Header("状态")]
    private bool isEnraged = false;
    protected bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;

    [Header("引用")]
    public Rigidbody2D rb;
    public Transform player;
    public HealthSystem playerHealth;
    public Animator animator;
    public Collider2D bossCollider;

    [Header("视觉效果")]
    public GameObject deathEffect;
    public GameObject hitEffect;
    public GameObject phaseChangeEffect;

    void Start()
    {
        InitializeReferences();
        ResetHealth();
        InitializeSkills();
    }

    void Update()
    {
        UpdatePhase();
        UpdateSkills();
        UpdateBehavior();
    }

    void InitializeReferences()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = FindObjectOfType<HealthSystem>();
        animator = GetComponent<Animator>();
        bossCollider = GetComponent<Collider2D>();
    }

    void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    void InitializeSkills()
    {
        // 添加默认技能
        if (skills.Count == 0)
        {
            skills.Add(new BossSkill("普通攻击", 1f, 0f, false));
            skills.Add(new BossSkill("范围攻击", 3f, 5f, true));
            skills.Add(new BossSkill("冲刺攻击", 2f, 3f, false));
        }
    }

    public void UpdatePhase()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= phaseHealthThresholds[3] && !isEnraged)
        {
            EnterEnragedPhase();
        }
        else if (healthPercent <= phaseHealthThresholds[2] && currentPhase == BossPhase.Phase2)
        {
            EnterPhase3();
        }
        else if (healthPercent <= phaseHealthThresholds[1] && currentPhase == BossPhase.Phase1)
        {
            EnterPhase2();
        }
    }

    void UpdateSkills()
    {
        if (skillTimer > 0f)
        {
            skillTimer -= Time.deltaTime;
        }

        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }
    }

    void UpdateBehavior()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Phase1Behavior();
                break;
            case BossPhase.Phase2:
                Phase2Behavior();
                break;
            case BossPhase.Phase3:
                Phase3Behavior();
                break;
            case BossPhase.Enraged:
                EnragedBehavior();
                break;
        }
    }

    void Phase1Behavior()
    {
        // 基础行为：追击和普通攻击
        ChasePlayer();
        if (IsPlayerInRange(attackRange) && skillTimer <= 0f)
        {
            UseSkill(0); // 普通攻击
        }
    }

    void Phase2Behavior()
    {
        // 第二阶段：增加技能使用频率
        ChasePlayer();
        if (skillTimer <= 0f)
        {
            int skillIndex = Random.Range(0, skills.Count);
            UseSkill(skillIndex);
        }
    }

    void Phase3Behavior()
    {
        // 第三阶段：更频繁使用技能，增加移动速度
        ChasePlayer(true);
        if (skillTimer <= 0f)
        {
            int skillIndex = Random.Range(1, skills.Count); // 优先使用特殊技能
            UseSkill(skillIndex);
        }
    }

    void EnragedBehavior()
    {
        // 狂暴阶段：最大移动速度，频繁使用所有技能
        ChasePlayer(true);
        if (skillTimer <= 0f)
        {
            int skillIndex = Random.Range(0, skills.Count);
            UseSkill(skillIndex);
        }
    }

    void ChasePlayer(bool fast = false)
    {
        if (player == null) return;

        float speed = fast ? moveSpeed * 1.5f : moveSpeed;
        Vector2 direction = ((Vector2)(player.position - transform.position)).normalized;
        rb.velocity = direction * speed;
    }

    void UseSkill(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skills.Count)
        {
            BossSkill skill = skills[skillIndex];
            Debug.Log($"使用技能：{skill.skillName}");

            // 播放技能动画
            if (animator != null)
            {
                animator.SetTrigger("skill" + skillIndex);
            }

            // 应用技能效果
            ApplySkillEffect(skill);

            // 重置技能冷却
            skillTimer = skill.cooldown;
        }
    }

    void ApplySkillEffect(BossSkill skill)
    {
        switch (skill.skillName)
        {
            case "普通攻击":
                MeleeAttack();
                break;
            case "范围攻击":
                AreaAttack();
                break;
            case "冲刺攻击":
                DashAttack();
                break;
            default:
                MeleeAttack();
                break;
        }
    }

    void MeleeAttack()
    {
        if (playerHealth != null && IsPlayerInRange(attackRange))
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"BOSS普通攻击，造成 {damage} 点伤害");
        }
    }

    void AreaAttack()
    {
        // 范围攻击逻辑
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                HealthSystem health = collider.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(damage * 1.5f);
                    Debug.Log($"BOSS范围攻击，造成 {damage * 1.5f} 点伤害");
                }
            }
        }
    }

    void DashAttack()
    {
        // 冲刺攻击逻辑
        if (player != null)
        {
            Vector2 direction = ((Vector2)(player.position - transform.position)).normalized;
            rb.velocity = direction * moveSpeed * 3f;
            StartCoroutine(DashAttackCoroutine());
        }
    }

    IEnumerator DashAttackCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
    }

    bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= range;
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"BOSS受到 {damage} 点伤害，剩余生命值：{currentHealth}");

        // 显示受伤效果
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // 播放受伤动画
        if (animator != null)
        {
            animator.SetTrigger("hit");
        }

        // 检查是否死亡
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        Debug.Log("进入第二阶段");
        ShowPhaseChangeEffect();
        // 增加移动速度
        moveSpeed *= 1.2f;
    }

    void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;
        Debug.Log("进入第三阶段");
        ShowPhaseChangeEffect();
        // 再次增加移动速度
        moveSpeed *= 1.3f;
    }

    void EnterEnragedPhase()
    {
        currentPhase = BossPhase.Enraged;
        isEnraged = true;
        Debug.Log("进入狂暴阶段(Phase4)");
        ShowPhaseChangeEffect();

        var mf = FindObjectOfType<MemoryFragmentSystem>();
        int collectedFragments = mf != null ? mf.GetFragmentCount() : 0;
        if (collectedFragments >= 7)
            EnableAllyMode();
        else
            EnableTrueForm();

        SwitchWeakness();
        StartMapShrink();
    }

    void EnableAllyMode()
    {
        Debug.Log("莎娜意识觉醒，协助玩家战斗！");
        damage *= 0.7f;
    }

    void EnableTrueForm()
    {
        Debug.Log("死灵圣王释放真正力量！");
        weaknesses.Clear();
        damage *= 1.5f;
    }

    public void SwitchWeakness()
    {
        if (weaknesses.Count > 0)
        {
            weaknesses[0] = (ElementType)Random.Range(0, 7);
            Debug.Log($"弱点切换为：{weaknesses[0]}");
        }
    }

    void StartMapShrink()
    {
        var map = FindObjectOfType<IntegratedMapSystem>();
        if (map != null) map.SetCycle(map.currentCycle);
        Debug.Log("地图开始缩小！");
    }

    public void OnPhaseTransition(BossPhase newPhase)
    {
        Debug.Log($"Boss 阶段转换：{currentPhase} → {newPhase}");
        currentPhase = newPhase;
        ShowPhaseChangeEffect();
    }

    public void AddAttackPattern(string patternID)
    {
        Debug.Log($"Boss 新增攻击模式：{patternID}");
    }

    public void ShowPhaseChangeEffect()
    {
        if (phaseChangeEffect != null)
        {
            Instantiate(phaseChangeEffect, transform.position, Quaternion.identity);
        }
    }

    public void SetInvulnerable(float duration)
    {
        isInvulnerable = true;
        invulnerabilityTimer = duration;
        Debug.Log($"BOSS进入无敌状态，持续 {duration} 秒");
    }

    protected virtual void Die()
    {
        Debug.Log("BOSS死亡");

        // 显示死亡效果
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 禁用碰撞器
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        // 禁用移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("die");
        }

        // 触发BOSS击败事件
        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.TriggerBossDefeated(gameObject);
        }

        // 销毁BOSS
        Destroy(gameObject, 2f);
    }

    // 调试方法
    public void DebugPrintBossStatus()
    {
        Debug.Log("=== BOSS状态 ===");
        Debug.Log($"当前生命值：{currentHealth}/{maxHealth}");
        Debug.Log($"当前阶段：{currentPhase}");
        Debug.Log($"是否狂暴：{isEnraged}");
        Debug.Log($"移动速度：{moveSpeed}");
        Debug.Log($"技能冷却：{skillTimer:F2}");
    }

    void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制追击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 绘制技能范围
        Gizmos.color = Color.blue;
        foreach (var skill in skills)
        {
            if (skill.areaEffect)
            {
                Gizmos.DrawWireSphere(transform.position, skill.range);
            }
        }
    }
}

[System.Serializable]
public class BossSkill
{
    public string skillName;
    public float damage;
    public float range;
    public bool areaEffect;
    public float cooldown = 2f;

    public BossSkill(string name, float dmg, float rng, bool area)
    {
        skillName = name;
        damage = dmg;
        range = rng;
        areaEffect = area;
    }
}


