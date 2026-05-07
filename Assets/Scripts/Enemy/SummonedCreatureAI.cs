using UnityEngine;
using System.Collections.Generic;

public class SummonedCreatureAI : MonoBehaviour
{
    // 常量定义
    private const float MIN_VELOCITY_THRESHOLD = 0.1f;

    // 行为模式枚举
    public enum BehaviorMode { Follow, Attack, Defend, Patrol }

    [Header("基础设置")]
    public EnemyAI.EnemyType creatureType;
    public BehaviorMode currentBehavior = BehaviorMode.Follow;

    [Header("基础属性")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float detectionRange = 8f;
    public float followDistance = 2f;
    public float patrolDistance = 5f;
    public float playerLowHealthThreshold = 0.3f;
    public float attackCooldown = 1f;

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

    [Header("感知组件")]
    public Transform playerTransform;
    public Transform currentTarget;
    [Tooltip("敌人是否在攻击范围内")]
    public bool enemyInAttackRange = false;

    [Header("AI组件")]
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;

    // 状态变量
    private float lastAttackTime = 0f;
    private bool isRecalled = false;
    private Vector2 patrolTarget;
    private float patrolTimer = 0f;
    private bool isMoving = false;
    private float stateChangeCooldown = 0f;
    private const float STATE_CHANGE_DELAY = 1.0f;
    // 缓存变量
    private EnemyAI.EnemyType lastAppliedCreatureType = EnemyAI.EnemyType.CorruptedVillager;

    // 缓存的距离计算
    private float cachedDistanceToTarget = float.MaxValue;
    private float distanceUpdateInterval = 0.1f;
    private float lastDistanceUpdateTime = 0f;

    private readonly Dictionary<EnemyAI.EnemyType, SummonStats> summonStatsDict = new Dictionary<EnemyAI.EnemyType, SummonStats>()
    {
        { EnemyAI.EnemyType.CorruptedVillager, new SummonStats { baseDamage = 8, attackSpeed = 1.2f, moveSpeed = 3f, attackRange = 1.5f } },
        { EnemyAI.EnemyType.CrystalLizard, new SummonStats { baseDamage = 6, attackSpeed = 1.5f, moveSpeed = 4f, attackRange = 1.2f } },
        { EnemyAI.EnemyType.SwampStalker, new SummonStats { baseDamage = 12, attackSpeed = 0.8f, moveSpeed = 2.5f, attackRange = 2f } },
        { EnemyAI.EnemyType.IceWolf, new SummonStats { baseDamage = 10, attackSpeed = 1.3f, moveSpeed = 3.5f, attackRange = 1.5f } },
        { EnemyAI.EnemyType.MechanicalDebris, new SummonStats { baseDamage = 15, attackSpeed = 0.6f, moveSpeed = 2f, attackRange = 2.5f } },
        { EnemyAI.EnemyType.SkeletonWarrior, new SummonStats { baseDamage = 7, attackSpeed = 1.4f, moveSpeed = 3f, attackRange = 1.3f } },
        { EnemyAI.EnemyType.Wraith, new SummonStats { baseDamage = 9, attackSpeed = 1.1f, moveSpeed = 3.2f, attackRange = 1.8f } },
        { EnemyAI.EnemyType.Gargoyle, new SummonStats { baseDamage = 18, attackSpeed = 0.7f, moveSpeed = 2.2f, attackRange = 2.2f } },
        { EnemyAI.EnemyType.SoulEater, new SummonStats { baseDamage = 25, attackSpeed = 0.5f, moveSpeed = 1.8f, attackRange = 3f } },
        { EnemyAI.EnemyType.LavaElemental, new SummonStats { baseDamage = 30, attackSpeed = 0.4f, moveSpeed = 1.5f, attackRange = 3.5f } },
        { EnemyAI.EnemyType.MechanicalConstruct, new SummonStats { baseDamage = 35, attackSpeed = 0.35f, moveSpeed = 1.2f, attackRange = 4f } },
        { EnemyAI.EnemyType.TimeGuardian, new SummonStats { baseDamage = 40, attackSpeed = 0.3f, moveSpeed = 1f, attackRange = 4.5f } },
        { EnemyAI.EnemyType.MemoryGuardian, new SummonStats { baseDamage = 45, attackSpeed = 0.28f, moveSpeed = 0.9f, attackRange = 5f } },
        { EnemyAI.EnemyType.CorruptionGuardian, new SummonStats { baseDamage = 55, attackSpeed = 0.25f, moveSpeed = 0.8f, attackRange = 5.5f } },
        { EnemyAI.EnemyType.ScarletSoulShana, new SummonStats { baseDamage = 80, attackSpeed = 0.2f, moveSpeed = 0.7f, attackRange = 6f } }
    };

    void Awake()
    {
        // 初始化组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        anim = GetComponent<Animator>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null) healthSystem = gameObject.AddComponent<HealthSystem>();
        
        // 设置召唤物大小
        transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1.0f);
    }

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
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
        try
        {
            GameplayAudioManager.Instance?.PlaySummonAppear();
        }
        catch { }
    }

    void Update()
    {
        if (isRecalled) return;

        // 更新状态切换冷却时间
        if (stateChangeCooldown > 0)
        {
            stateChangeCooldown -= Time.deltaTime;
        }

        // 确保找到玩家（只在必要时查找）
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        // 更新目标距离
        UpdateTargetDistance();

        // 先处理死亡检查，避免不必要的计算
        CheckDeath();
        
        // 处理其他逻辑
        UpdateBehavior();
        HandleHealthRegen();
        UpdateMovementAnimation();
    }
    
    void FindPlayer()
    {
        // 只在需要时调用GameObject.FindGameObjectWithTag()
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
        // 检查健康系统是否存在
        if (healthSystem == null) return;
        
        // 检查是否需要触发死亡
        if (healthSystem.currentHealth <= 0 && !healthSystem.IsDead())
        {
            Die();
        }
    }
    
    public void Die()
    {
        if (isRecalled) return;
        
        // 触发死亡爆炸（如果启用）
        if (deathExplosion)
        {
            TriggerDeathExplosion();
        }
        
        // 播放死亡动画
        SetAnimatorTriggerSafe("Death");
        
        Debug.Log($"召唤物 {gameObject.name} 死亡并消失");
        
        // 延迟销毁，以便播放死亡动画
        Destroy(gameObject, deathDestroyDelay);
    }

    void FixedUpdate()
    {
        if (isRecalled) return;

        ExecuteMovement();
    }

    void ApplyTypeStats()
    {
        // 只有当creatureType发生变化时才重新计算属性
        if (creatureType != lastAppliedCreatureType)
        {
            if (summonStatsDict.TryGetValue(creatureType, out SummonStats stats))
            {
                moveSpeed = stats.moveSpeed;
                attackRange = stats.attackRange;
                attackCooldown = 1f / stats.attackSpeed;
                
                // 更新缓存的类型
                lastAppliedCreatureType = creatureType;
            }
        }
    }

    float GetBaseHealthForType(EnemyAI.EnemyType type)
    {
        return type switch
        {
            EnemyAI.EnemyType.CorruptedVillager => 60f,
            EnemyAI.EnemyType.CrystalLizard => 45f,
            EnemyAI.EnemyType.SwampStalker => 80f,
            EnemyAI.EnemyType.IceWolf => 55f,
            EnemyAI.EnemyType.MechanicalDebris => 70f,
            EnemyAI.EnemyType.SkeletonWarrior => 40f,
            EnemyAI.EnemyType.Wraith => 45f,
            EnemyAI.EnemyType.Gargoyle => 110f,
            EnemyAI.EnemyType.SoulEater => 180f,
            EnemyAI.EnemyType.LavaElemental => 240f,
            EnemyAI.EnemyType.MechanicalConstruct => 300f,
            EnemyAI.EnemyType.TimeGuardian => 350f,
            EnemyAI.EnemyType.MemoryGuardian => 400f,
            EnemyAI.EnemyType.CorruptionGuardian => 600f,
            EnemyAI.EnemyType.ScarletSoulShana => 1000f,
            _ => 50f
        };
    }

    void UpdateBehavior()
    {
        // 智能状态切换逻辑
        UpdateSmartBehavior();
        
        // 执行当前状态的行为
        switch (currentBehavior)
        {
            case BehaviorMode.Follow:
                FollowPlayer();
                break;
            case BehaviorMode.Attack:
                AttackNearestEnemy();
                break;
            case BehaviorMode.Defend:
                DefendPlayer();
                break;
            case BehaviorMode.Patrol:
                PatrolArea();
                break;
        }
    }
    
    void UpdateSmartBehavior()
    {
        if (playerTransform == null) return;
        
        // 检查玩家血量
        float playerHealthPercent = GetPlayerHealthPercent();
        
        // 检查敌人存在
        FindNearestEnemy();
        bool hasEnemyNearby = currentTarget != null && currentTarget.tag == "Enemy";
        
        // 检查与玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // 状态优先级：防御 > 攻击 > 巡逻 > 跟随
        
        // 1. 防御状态：玩家血量低
        if (playerHealthPercent < playerLowHealthThreshold)
        {
            if (currentBehavior != BehaviorMode.Defend)
            {
                ChangeBehavior(BehaviorMode.Defend, "玩家血量低，召唤物进入防御状态");
            }
            return;
        }
        
        // 2. 攻击状态：附近有敌人
        if (hasEnemyNearby)
        {
            if (currentBehavior != BehaviorMode.Attack)
            {
                ChangeBehavior(BehaviorMode.Attack, "发现敌人，召唤物进入攻击状态");
            }
            return;
        }
        
        // 3. 巡逻状态：在玩家附近一定范围内
        if (distanceToPlayer <= patrolDistance)
        {
            if (currentBehavior != BehaviorMode.Patrol)
            {
                ChangeBehavior(BehaviorMode.Patrol, "在玩家附近，召唤物进入巡逻状态");
            }
            return;
        }
        
        // 4. 跟随状态：远离玩家
        if (distanceToPlayer > patrolDistance)
        {
            if (currentBehavior != BehaviorMode.Follow)
            {
                ChangeBehavior(BehaviorMode.Follow, "远离玩家，召唤物进入跟随状态");
            }
            return;
        }
    }
    
    void ChangeBehavior(BehaviorMode newBehavior, string message)
    {
        // 检查状态切换冷却时间
        if (stateChangeCooldown > 0)
        {
            return;
        }
        
        // 切换状态
        currentBehavior = newBehavior;
        Debug.Log(message);
        
        // 设置状态切换冷却时间
        stateChangeCooldown = STATE_CHANGE_DELAY;
        
        // 如果切换到巡逻状态，立即更新巡逻目标
        if (newBehavior == BehaviorMode.Patrol)
        {
            patrolTimer = 0f;
        }
    }
    
    float GetPlayerHealthPercent()
    {
        HealthSystem playerHealth = playerTransform.GetComponent<HealthSystem>();
        if (playerHealth != null && playerHealth.maxHealth > 0)
        {
            return playerHealth.currentHealth / playerHealth.maxHealth;
        }
        return 1.0f; // 默认玩家血量满
    }

    void FollowPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Mathf.Sqrt(cachedDistanceToTarget);

        if (distanceToPlayer > followDistance)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            // 平滑加速到移动速度
            rb.velocity = Vector2.Lerp(rb.velocity, direction * moveSpeed, Time.deltaTime * 5f);
        }
        else
        {
            // 平滑减速到停止
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 3f);
        }

        FindNearestEnemy();
        
        // 如果找到敌人，切换到攻击模式
        if (currentTarget != null && currentTarget.tag == "Enemy")
        {
            ChangeBehavior(BehaviorMode.Attack, "发现敌人，召唤物进入攻击状态");
        }
    }

    void AttackNearestEnemy()
    {
        if (currentTarget == null)
        {
            FindNearestEnemy();
            if (currentTarget == null)
            {
                currentBehavior = BehaviorMode.Follow;
                return;
            }
        }
        
        // 检查当前目标是否仍然存在且是敌人
        if (currentTarget == null || currentTarget.tag != "Enemy")
        {
            FindNearestEnemy();
            if (currentTarget == null)
            {
                currentBehavior = BehaviorMode.Follow;
                return;
            }
        }
        
        // 检查敌人是否在检测范围内
        float distanceToTarget = Mathf.Sqrt(cachedDistanceToTarget);
        if (distanceToTarget > detectionRange)
        {
            FindNearestEnemy();
            if (currentTarget == null)
            {
                currentBehavior = BehaviorMode.Follow;
                return;
            }
        }

        // 更新目标距离
        distanceToTarget = Mathf.Sqrt(cachedDistanceToTarget);

        if (distanceToTarget <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown - cooldownReduction)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
            rb.velocity = Vector2.zero;
        }
        else
        {
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
    }

    void DefendPlayer()
    {
        if (playerTransform == null) return;

        FindNearestEnemy();

        if (currentTarget != null)
        {
            float distanceToTarget = Mathf.Sqrt(cachedDistanceToTarget);

            if (distanceToTarget <= detectionRange)
            {
                // 切换到攻击模式
                currentBehavior = BehaviorMode.Attack;
                
                Vector2 direction = (currentTarget.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;

                if (distanceToTarget <= attackRange && Time.time - lastAttackTime >= attackCooldown - cooldownReduction)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                ReturnToPlayer();
            }
        }
        else
        {
            ReturnToPlayer();
        }
    }

    void PatrolArea()
    {
        if (playerTransform == null) return;

        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0)
        {
            float patrolRadius = patrolDistance * 0.8f;
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            patrolTarget = (Vector2)playerTransform.position + randomOffset;
            patrolTimer = Random.Range(2f, 5f);
        }

        float distanceToPatrolTarget = Vector2.Distance(transform.position, patrolTarget);

        if (distanceToPatrolTarget > 0.5f)
        {
            Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed * 0.7f;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        FindNearestEnemy();
        
        // 如果找到敌人，切换到攻击模式
        if (currentTarget != null && currentTarget.tag == "Enemy")
        {
            currentBehavior = BehaviorMode.Attack;
        }
    }

    void ReturnToPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Mathf.Sqrt(cachedDistanceToTarget);

        if (distanceToPlayer > followDistance)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void FindNearestEnemy()
    {
        // 使用OverlapCircleAll获取附近的碰撞体
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        Transform nearestEnemy = null;
        float nearestDistanceSquared = float.MaxValue;

        // 缓存当前位置，避免重复访问transform.position
        Vector2 currentPosition = transform.position;

        foreach (Collider2D collider in nearbyColliders)
        {
            // 只处理敌人标签的碰撞体
            if (collider.CompareTag("Enemy"))
            {
                // 使用sqrMagnitude代替Distance，避免平方根计算
                float distanceSquared = ((Vector2)collider.transform.position - currentPosition).sqrMagnitude;
                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    nearestEnemy = collider.transform;
                }
            }
        }

        currentTarget = nearestEnemy;
    }

    void PerformAttack()
    {
        if (currentTarget == null) return;

        float baseDamage = GetBaseDamage();
        float finalDamage = baseDamage * (1 + damageBonus);

        EnemyAI enemyAI = currentTarget.GetComponent<EnemyAI>();
        if (enemyAI != null && enemyAI.enemyQuality == EnemyAI.EnemyQuality.Boss)
        {
            finalDamage *= (1 + bossDamageBonus);
        }

        if (Random.value < critBonus)
        {
            finalDamage *= 1.5f;
            Debug.Log("召唤物暴击！");
        }

        HealthSystem enemyHealth = currentTarget.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(finalDamage);
            Debug.Log($"召唤物攻击 {currentTarget.name}，造成 {finalDamage} 点伤害");
        }

        // 播放攻击动画
        SetAnimatorTriggerSafe("Attack");
        
        // 播放攻击音效
        try
        {
            GameplayAudioManager.Instance?.PlaySummonAttack();
        }
        catch { }
    }

    float GetBaseDamage()
    {
        if (summonStatsDict.TryGetValue(creatureType, out SummonStats stats))
        {
            float levelBonus = soulCoreData != null ? soulCoreData.level * 0.05f : 0f;
            float qualityBonus = soulCoreData != null ? soulCoreData.attributeMultiplier : 1f;
            return stats.baseDamage * (1 + levelBonus) * qualityBonus;
        }
        return 10f;
    }

    void HandleHealthRegen()
    {
        if (healthRegen > 0 && healthSystem != null)
        {
            healthSystem.Heal(healthRegen * Time.deltaTime);
        }
    }

    void ExecuteMovement()
    {
        // 移除翻转逻辑，召唤物不再翻转
    }

    void UpdateMovementAnimation()
    {
        if (anim == null) return;

        Vector2 velocity = rb != null ? rb.velocity : Vector2.zero;
        bool currentlyMoving = velocity.sqrMagnitude > MIN_VELOCITY_THRESHOLD;

        if (currentlyMoving != isMoving)
        {
            isMoving = currentlyMoving;
            SetAnimatorBoolSafe("IsMoving", isMoving);
        }

        if (isMoving)
        {
            Vector2 moveDirection = velocity.normalized;

            // 重置所有移动方向参数
            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);

            // 根据移动方向设置相应的动画参数
            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
            {
                if (moveDirection.x > 0)
                {
                    SetAnimatorBoolSafe("MoveRight", true);
                }
                else
                {
                    SetAnimatorBoolSafe("MoveLeft", true);
                }
            }
            else
            {
                if (moveDirection.y > 0)
                {
                    SetAnimatorBoolSafe("MoveUp", true);
                }
                else
                {
                    SetAnimatorBoolSafe("MoveDown", true);
                }
            }
        }
        else
        {
            // 待机动画
            SetAnimatorBoolSafe("IsMoving", false);
            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);
            
            // 尝试设置待机动画参数
            SetAnimatorBoolSafe("Idle", true);
        }
    }

    // 安全设置动画布尔参数
    void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool(paramName, value);
                    return;
                }
            }
        }
    }

    // 安全设置动画触发器参数
    void SetAnimatorTriggerSafe(string paramName)
    {
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.SetTrigger(paramName);
                    return;
                }
            }
        }
    }

    public void SetSoulCoreData(SoulCoreData core)
    {
        soulCoreData = core;
        creatureType = core.enemyType;
        ApplyTypeStats();
    }

    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    public void SetBehavior(BehaviorMode mode)
    {
        currentBehavior = mode;
        Debug.Log($"召唤物行为模式切换为: {mode}");
    }

    public void MarkAsRecalled()
    {
        isRecalled = true;
        rb.velocity = Vector2.zero;

        if (deathExplosion)
        {
            TriggerDeathExplosion();
        }

        SetAnimatorTriggerSafe("Disappear");
        
        // 播放召唤消失音效
        try
        {
            GameplayAudioManager.Instance?.PlaySummonDisappear();
        }
        catch { }
    }

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
                    Debug.Log($"献祭爆炸对 {enemy.name} 造成 {explosionDamage} 点伤害");
                }
            }
        }

        Debug.Log("召唤物触发献祭爆炸！");
    }

    // 调整召唤物大小的方法
    public void SetScale(float newScale)
    {
        scaleMultiplier = newScale;
        transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1.0f);
        Debug.Log($"召唤物大小已调整为 {scaleMultiplier} 倍");
    }

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
        if (deathExplosion && !isRecalled)
        {
            TriggerDeathExplosion();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    private class SummonStats
    {
        public float baseDamage;
        public float attackSpeed;
        public float moveSpeed;
        public float attackRange;
    }
}
