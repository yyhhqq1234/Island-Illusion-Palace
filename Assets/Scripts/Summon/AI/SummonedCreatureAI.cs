using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 召唤物AI控制器 — 组件化架构的协调器
/// 持有 SummonMovementComponent / SummonCombatComponent / SummonStateComponent
/// 负责协调子组件完成跟随/巡逻/攻击/护卫等行为
/// 保持所有现有公共API不变，向后兼容
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SummonMovementComponent))]
[RequireComponent(typeof(SummonCombatComponent))]
[RequireComponent(typeof(SummonStateComponent))]
public class SummonedCreatureAI : MonoBehaviour, IDieHandler
{
    // 常量
    private const float MIN_VELOCITY_THRESHOLD = 0.1f;

    // ==================== 行为模式（委托给 SummonStateComponent） ====================
    public enum BehaviorMode { Follow, Attack, Defend, Patrol }

    // ==================== 基础设置 ====================
    [Header("基础设置")]
    public CreatureType creatureType;
    [Tooltip("当前行为模式（委托给状态组件）")]
    public BehaviorMode currentBehavior = BehaviorMode.Follow;

    [Header("外观设置")]
    [Tooltip("召唤物缩放倍率")]
    public float scaleMultiplier = 2.0f;
    [Tooltip("死亡后延迟销毁的时间（秒）")]
    public float deathDestroyDelay = 0.1f;

    [Header("灵魂之核数据")]
    public SoulCoreData soulCoreData;

    [Header("记忆碎片加成")]
    public float healthBonus = 0f;
    public float damageBonus = 0f;
    public float defenseBonus = 0f;
    public float critBonus = 0f;
    public float cooldownReduction = 0f;
    public float skillCooldownReduction = 0f;
    public float controlBonus = 0f;
    public float healingBonus = 0f;
    public float healthRegen = 0f;
    public float bossDamageBonus = 0f;
    public bool deathExplosion = false;

    [Header("召唤时限")]
    public float lifetime = 0f;         // 0=无限
    public bool isPermanent = false;    // 永久的不会被销毁

    [Header("感知组件")]
    public Transform playerTransform;
    public Transform currentTarget;
    [Tooltip("敌人是否在攻击范围内")]
    public bool enemyInAttackRange = false;

    // ==================== 组件引用 ====================
    [Header("AI组件")]
    [SerializeField] private SummonMovementComponent movementComponent;
    [SerializeField] private SummonCombatComponent combatComponent;
    [SerializeField] private SummonStateComponent stateComponent;

    // 内部组件
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;

    // 状态变量
    private bool isRecalled = false;
    private bool isMoving = false;

    // 缓存变量
    private CreatureType lastAppliedCreatureType = CreatureType.CorruptedVillager;

    // 距离缓存
    private float cachedDistanceToTarget = float.MaxValue;
    private float distanceUpdateInterval = 0.1f;
    private float lastDistanceUpdateTime = 0f;

    // 召唤物基础属性字典
    private readonly Dictionary<CreatureType, SummonStats> summonStatsDict = new Dictionary<CreatureType, SummonStats>()
    {
        { CreatureType.CorruptedVillager, new SummonStats { baseDamage = 8, attackSpeed = 1.2f, moveSpeed = 3f, attackRange = 1.5f } },
        { CreatureType.CrystalLizard, new SummonStats { baseDamage = 6, attackSpeed = 1.5f, moveSpeed = 4f, attackRange = 1.2f } },
        { CreatureType.SwampStalker, new SummonStats { baseDamage = 12, attackSpeed = 0.8f, moveSpeed = 2.5f, attackRange = 2f } },
        { CreatureType.IceWolf, new SummonStats { baseDamage = 10, attackSpeed = 1.3f, moveSpeed = 3.5f, attackRange = 1.5f } },
        { CreatureType.MechanicalDebris, new SummonStats { baseDamage = 15, attackSpeed = 0.6f, moveSpeed = 2f, attackRange = 2.5f } },
        { CreatureType.SkeletonWarrior, new SummonStats { baseDamage = 7, attackSpeed = 1.4f, moveSpeed = 3f, attackRange = 1.3f } },
        { CreatureType.Wraith, new SummonStats { baseDamage = 9, attackSpeed = 1.1f, moveSpeed = 3.2f, attackRange = 1.8f } },
        { CreatureType.Gargoyle, new SummonStats { baseDamage = 18, attackSpeed = 0.7f, moveSpeed = 2.2f, attackRange = 2.2f } },
        { CreatureType.SoulEater, new SummonStats { baseDamage = 25, attackSpeed = 0.5f, moveSpeed = 1.8f, attackRange = 3f } },
        { CreatureType.LavaElemental, new SummonStats { baseDamage = 30, attackSpeed = 0.4f, moveSpeed = 1.5f, attackRange = 3.5f } },
        { CreatureType.MechanicalConstruct, new SummonStats { baseDamage = 35, attackSpeed = 0.35f, moveSpeed = 1.2f, attackRange = 4f } },
        { CreatureType.TimeGuardian, new SummonStats { baseDamage = 40, attackSpeed = 0.3f, moveSpeed = 1f, attackRange = 4.5f } },
        { CreatureType.MemoryGuardian, new SummonStats { baseDamage = 45, attackSpeed = 0.28f, moveSpeed = 0.9f, attackRange = 5f } },
        { CreatureType.CorruptionGuardian, new SummonStats { baseDamage = 55, attackSpeed = 0.25f, moveSpeed = 0.8f, attackRange = 5.5f } },
        { CreatureType.ScarletSoulShana, new SummonStats { baseDamage = 80, attackSpeed = 0.2f, moveSpeed = 0.7f, attackRange = 6f } }
    };

    // ==================== Unity 生命周期 ====================

    void Awake()
    {
        // 初始化核心组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        anim = GetComponent<Animator>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null) healthSystem = gameObject.AddComponent<HealthSystem>();

        // 获取子组件
        movementComponent = GetComponent<SummonMovementComponent>();
        if (movementComponent == null) movementComponent = gameObject.AddComponent<SummonMovementComponent>();

        combatComponent = GetComponent<SummonCombatComponent>();
        if (combatComponent == null) combatComponent = gameObject.AddComponent<SummonCombatComponent>();

        stateComponent = GetComponent<SummonStateComponent>();
        if (stateComponent == null) stateComponent = gameObject.AddComponent<SummonStateComponent>();

        // 设置缩放
        transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1.0f);
    }

    void Start()
    {
        // 初始化移动组件
        movementComponent?.Initialize(rb, playerTransform);

        // 初始化状态组件
        stateComponent?.Initialize(SummonStateComponent.BehaviorMode.Follow);
        stateComponent.OnBehaviorChanged += OnStateBehaviorChanged;

        if (playerTransform == null)
        {
            FindPlayer();
        }

        if (healthSystem != null && soulCoreData != null)
        {
            float baseHealth = GetBaseHealthForType(creatureType);
            healthSystem.maxHealth = baseHealth * (1 + soulCoreData.level * 0.05f) * (1 + healthBonus);
        }

        ApplyTypeStats();
        currentTarget = playerTransform;

        // 播放召唤动画
        SetAnimatorTriggerSafe("Appear");

        // 播放召唤出现音效
        try { GameplayAudioManager.Instance?.PlaySummonAppear(); } catch { }
    }

    void Update()
    {
        if (isRecalled) return;

        // 处理召唤时限
        if (!isPermanent && lifetime > 0)
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
            {
                MarkAsRecalled();
                Destroy(gameObject, 0.5f);
                return;
            }
        }

        // 确保玩家引用
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        // 更新移动组件中的玩家引用
        if (movementComponent != null)
        {
            movementComponent.Initialize(rb, playerTransform);
        }

        // 更新距离缓存
        UpdateTargetDistance();

        // 死亡检查
        CheckDeath();

        // 更新行为
        UpdateBehavior();

        // 生命回复
        HandleHealthRegen();

        // 更新移动动画
        UpdateMovementAnimation();
    }

    void FixedUpdate()
    {
        if (isRecalled) return;
        // 移动逻辑已在 Update 中通过子组件处理
    }

    // ==================== 行为协调 ====================

    void UpdateBehavior()
    {
        if (playerTransform == null || stateComponent == null || movementComponent == null || combatComponent == null)
            return;

        // 获取玩家血量百分比
        float playerHealthPercent = GetPlayerHealthPercent();

        // 寻找敌人
        combatComponent.FindNearestEnemy();
        Transform nearestEnemy = combatComponent.GetCurrentTarget();
        bool hasEnemyNearby = nearestEnemy != null;

        // 获取与玩家的距离
        float distanceToPlayer = movementComponent.GetDistanceToPlayer();

        // 更新状态机
        SummonStateComponent.BehaviorMode newMode = stateComponent.UpdateState(playerHealthPercent, hasEnemyNearby, distanceToPlayer);
        currentBehavior = (BehaviorMode)(int)newMode;

        // 同步当前目标
        currentTarget = nearestEnemy;

        // 执行当前行为
        ExecuteCurrentBehavior();
    }

    void ExecuteCurrentBehavior()
    {
        switch (currentBehavior)
        {
            case BehaviorMode.Follow:
                ExecuteFollow();
                break;
            case BehaviorMode.Attack:
                ExecuteAttack();
                break;
            case BehaviorMode.Defend:
                ExecuteDefend();
                break;
            case BehaviorMode.Patrol:
                ExecutePatrol();
                break;
        }
    }

    void ExecuteFollow()
    {
        // 使用缓冲跟随
        movementComponent?.FollowPlayerWithBuffer();

        // 检查是否有敌人进入检测范围
        if (combatComponent != null)
        {
            combatComponent.FindNearestEnemy();
            Transform enemy = combatComponent.GetCurrentTarget();
            if (enemy != null)
            {
                stateComponent?.SetBehavior(SummonStateComponent.BehaviorMode.Attack);
            }
        }
    }

    void ExecuteAttack()
    {
        Transform target = combatComponent?.GetCurrentTarget();
        if (target == null)
        {
            combatComponent?.FindNearestEnemy();
            target = combatComponent?.GetCurrentTarget();
            if (target == null)
            {
                stateComponent?.SetBehavior(SummonStateComponent.BehaviorMode.Follow);
                return;
            }
        }

        // 检查目标是否仍然有效
        if (!target.CompareTag("Enemy"))
        {
            combatComponent?.ClearTarget();
            combatComponent?.FindNearestEnemy();
            target = combatComponent?.GetCurrentTarget();
            if (target == null)
            {
                stateComponent?.SetBehavior(SummonStateComponent.BehaviorMode.Follow);
                return;
            }
        }

        float distanceToTarget = combatComponent.GetDistanceToTargetSqr();
        float attackRangeSqr = combatComponent.attackRange * combatComponent.attackRange;

        if (distanceToTarget <= attackRangeSqr)
        {
            // 在攻击范围内，停止移动并攻击
            movementComponent?.StopMovement();
            combatComponent.PerformAttack(target);
        }
        else
        {
            // 不在攻击范围内，向目标移动
            movementComponent?.MoveTowards(target.position, 1f);
        }
    }

    void ExecuteDefend()
    {
        Transform nearestEnemy = combatComponent?.GetCurrentTarget();
        if (nearestEnemy == null)
        {
            combatComponent?.FindNearestEnemy();
            nearestEnemy = combatComponent?.GetCurrentTarget();
        }

        if (nearestEnemy != null && combatComponent.IsEnemyInRange(nearestEnemy))
        {
            // 敌人在攻击范围内，攻击
            movementComponent?.StopMovement();
            combatComponent.PerformAttack(nearestEnemy);
        }

        // 护卫定位
        movementComponent?.GuardPositioning(nearestEnemy);
    }

    void ExecutePatrol()
    {
        // 巡逻
        movementComponent?.PatrolAroundPlayer(Time.deltaTime);

        // 检查是否有敌人进入检测范围
        if (combatComponent != null)
        {
            combatComponent.FindNearestEnemy();
            Transform enemy = combatComponent.GetCurrentTarget();
            if (enemy != null)
            {
                stateComponent?.SetBehavior(SummonStateComponent.BehaviorMode.Attack);
            }
        }
    }

    // ==================== 状态机回调 ====================

    void OnStateBehaviorChanged(SummonStateComponent.BehaviorMode oldMode, SummonStateComponent.BehaviorMode newMode)
    {
        currentBehavior = (BehaviorMode)(int)newMode;

        // 切换到巡逻时重置巡逻状态
        if (newMode == SummonStateComponent.BehaviorMode.Patrol)
        {
            movementComponent?.ResetPatrolState();
        }
    }

    // ==================== 辅助方法 ====================

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            currentTarget = playerTransform;
        }
    }

    void UpdateTargetDistance()
    {
        if (Time.time - lastDistanceUpdateTime >= distanceUpdateInterval)
        {
            lastDistanceUpdateTime = Time.time;
            if (currentTarget != null)
            {
                Vector2 directionToTarget = (Vector2)(currentTarget.position - transform.position);
                cachedDistanceToTarget = directionToTarget.sqrMagnitude;
            }
            else
            {
                cachedDistanceToTarget = float.MaxValue;
            }
        }
    }

    void CheckDeath()
    {
        if (healthSystem == null) return;
        if (healthSystem.currentHealth <= 0 && !healthSystem.IsDead())
        {
            Die();
        }
    }

    void ApplyTypeStats()
    {
        if (creatureType != lastAppliedCreatureType)
        {
            if (summonStatsDict.TryGetValue(creatureType, out SummonStats stats))
            {
                movementComponent?.SetMoveSpeed(stats.moveSpeed);
                combatComponent.attackRange = stats.attackRange;
                combatComponent.attackCooldown = 1f / stats.attackSpeed;
                combatComponent.SetBaseDamage(stats.baseDamage);

                lastAppliedCreatureType = creatureType;
            }
        }
    }

    float GetPlayerHealthPercent()
    {
        if (playerTransform == null) return 1f;
        HealthSystem playerHealth = playerTransform.GetComponent<HealthSystem>();
        if (playerHealth != null && playerHealth.maxHealth > 0)
        {
            return playerHealth.currentHealth / playerHealth.maxHealth;
        }
        return 1f;
    }

    float GetBaseHealthForType(CreatureType type)
    {
        return CreatureStatsDatabase.GetBaseHealthForType(type);
    }

    void HandleHealthRegen()
    {
        if (healthRegen > 0 && healthSystem != null)
        {
            healthSystem.Heal(healthRegen * Time.deltaTime);
        }
    }

    // ==================== 动画控制 ====================

    void UpdateMovementAnimation()
    {
        if (anim == null) return;

        Vector2 velocity = movementComponent != null ? movementComponent.GetVelocity() : Vector2.zero;
        bool currentlyMoving = velocity.sqrMagnitude > MIN_VELOCITY_THRESHOLD;

        if (currentlyMoving != isMoving)
        {
            isMoving = currentlyMoving;
            SetAnimatorBoolSafe("IsMoving", isMoving);
        }

        if (isMoving)
        {
            Vector2 moveDirection = velocity.normalized;

            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);

            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
            {
                SetAnimatorBoolSafe(moveDirection.x > 0 ? "MoveRight" : "MoveLeft", true);
            }
            else
            {
                SetAnimatorBoolSafe(moveDirection.y > 0 ? "MoveUp" : "MoveDown", true);
            }
        }
        else
        {
            SetAnimatorBoolSafe("IsMoving", false);
            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);
            SetAnimatorBoolSafe("Idle", true);
        }
    }

    void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (anim == null) return;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(paramName, value);
                return;
            }
        }
    }

    void SetAnimatorTriggerSafe(string paramName)
    {
        if (anim == null) return;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(paramName);
                return;
            }
        }
    }

    // ==================== 公共API（向后兼容） ====================

    /// <summary>
    /// 设置灵魂之核数据
    /// </summary>
    public void SetSoulCoreData(SoulCoreData core)
    {
        soulCoreData = core;
        creatureType = CreatureTypeConverter.FromEnemyType(core.enemyType);
        combatComponent?.SetSoulCoreData(core);

        ApplyTypeStats();
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
        movementComponent?.Initialize(rb, player);
    }

    /// <summary>
    /// 设置行为模式
    /// </summary>
    public void SetBehavior(BehaviorMode mode)
    {
        stateComponent?.SetBehavior((SummonStateComponent.BehaviorMode)(int)mode);
        Debug.Log($"[SummonedCreatureAI] 召唤物行为模式切换为: {mode}");
    }

    /// <summary>
    /// 标记为召回状态
    /// </summary>
    public void MarkAsRecalled()
    {
        isRecalled = true;
        movementComponent?.StopMovement();

        if (deathExplosion)
        {
            TriggerDeathExplosion();
        }

        SetAnimatorTriggerSafe("Disappear");

        try { GameplayAudioManager.Instance?.PlaySummonDisappear(); } catch { }
    }

    /// <summary>
    /// 添加增益
    /// </summary>
    public void AddBuff(float percentBuff)
    {
        damageBonus += percentBuff;
        healthBonus += percentBuff * 0.5f;
    }

    /// <summary>
    /// 设置缩放
    /// </summary>
    public void SetScale(float newScale)
    {
        scaleMultiplier = newScale;
        transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1.0f);
        Debug.Log($"[SummonedCreatureAI] 召唤物大小已调整为 {scaleMultiplier} 倍");
    }

    /// <summary>
    /// 死亡处理（IDieHandler 事件驱动接口）
    /// </summary>
    public void OnDie() => Die();

    /// 死亡处理
    /// </summary>
    public void Die()
    {
        if (isRecalled) return;

        if (deathExplosion)
        {
            TriggerDeathExplosion();
        }

        SetAnimatorTriggerSafe("Death");

        Debug.Log($"[SummonedCreatureAI] 召唤物 {gameObject.name} 死亡并消失");

        Destroy(gameObject, deathDestroyDelay);
    }

    // ==================== 死亡爆炸 ====================

    void TriggerDeathExplosion()
    {
        float explosionRadius = 3f;
        float explosionDamage = GetBaseDamage() * 2f;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D enemy in nearbyEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(explosionDamage);
                    Debug.Log($"[SummonedCreatureAI] 献祭爆炸对 {enemy.name} 造成 {explosionDamage} 点伤害");
                }
            }
        }

        Debug.Log("[SummonedCreatureAI] 召唤物触发献祭爆炸！");
    }

    float GetBaseDamage()
    {
        if (combatComponent != null) return combatComponent.baseDamage;
        return 10f;
    }

    // ==================== 触发器 ====================

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            enemyInAttackRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            enemyInAttackRange = false;
        }
    }

    void OnDestroy()
    {
        if (stateComponent != null)
        {
            stateComponent.OnBehaviorChanged -= OnStateBehaviorChanged;
        }

        if (deathExplosion && !isRecalled)
        {
            TriggerDeathExplosion();
        }
    }

    // ==================== Gizmos ====================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, (combatComponent != null ? combatComponent.detectionRange : 8f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, (combatComponent != null ? combatComponent.attackRange : 1.5f));

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    // ==================== 内部数据结构 ====================

    private class SummonStats
    {
        public float baseDamage;
        public float attackSpeed;
        public float moveSpeed;
        public float attackRange;
    }
}