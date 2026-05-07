using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    // 常量定义
    private const float MIN_VELOCITY_THRESHOLD = 0.1f;

    // ==================== 敌人类型枚举（15种） ====================
    public enum EnemyType
    {
        // 普通敌人（8种）
        CorruptedVillager,      // 腐化村民
        CrystalLizard,          // 结晶蜥蜴
        SwampStalker,           // 沼泽潜伏者
        IceWolf,                // 冰原狼
        MechanicalDebris,       // 机械残骸
        SkeletonWarrior,        // 骷髅战士
        Wraith,                 // 怨魂
        Gargoyle,               // 石像鬼

        // 精英/BOSS敌人（7种）
        SoulEater,              // 灵魂吞噬者（精英）
        LavaElemental,          // 熔岩元素（精英）
        MechanicalConstruct,    // 机械构造体（精英）
        TimeGuardian,           // 时空守护者（BOSS）
        MemoryGuardian,         // 记忆守护者（BOSS）
        CorruptionGuardian,     // 腐化守护者（精英）
        ScarletSoulShana        // Scarlet Soul Shana（最终BOSS）
    }

    // ==================== 敌人分类枚举 ====================
    public enum EnemyCategory
    {
        CrystalCreature,        // 晶化生物
        SoulRemnant,            // 灵魂残留
        TechConstruct,          // 科技造物
        MemoryManifestation     // 记忆具现
    }

    // ==================== 品质枚举 ====================
    public enum EnemyQuality
    {
        Common,     // 普通（白色）
        Elite,      // 精英（蓝色）
        Boss        // BOSS（金色）
    }

    // ==================== 属性枚举（7种） ====================
    public enum ElementType
    {
        Frost,      // 冰霜
        Water,      // 流水
        Fire,       // 火焰
        Lightning,  // 雷电
        Soul,       // 灵魂
        Holy,       // 圣光
        Physical    // 物理
    }

    // ==================== 行为模式枚举 ====================
    public enum BehaviorMode
    {
        Pursuit,    // 追击型
        Berserk,    // 狂暴型
        Ambush,     // 伏击型
        Summoner,   // 召唤型
        Ranged,     // 远程型
        Tactical    // 战术型
    }

    [Header("敌人基础设置")]
    public EnemyType enemyType;
    public EnemyCategory enemyCategory;
    public EnemyQuality enemyQuality;
    public BehaviorMode behaviorMode;
    public ElementType primaryElement;
    public ElementType elementType;


    [Header("基础属性")]
    public float moveSpeed = 3f;
    public float chaseRange = 10f;
    [Tooltip("动态追逐范围扩展值，当锁定目标时扩大范围")]
    public float chaseRangeExpansion = 5f;
    public float attackRange = 2f;
    public float damage = 10f;
    public float attackRate = 1f;
    private float lastAttackTime = 0f;

    // 动态追逐范围相关
    private bool isChaseRangeExpanded = false;
    private float expandedChaseRange = 0f;
    private float originalChaseRange = 0f;

    [Header("狂暴机制设置")]
    [Tooltip("狂暴触发的血量比例阈值 (0-1)")]
    [Range(0f, 1f)]
    public float berserkHealthThreshold = 0.3f;
    [Tooltip("狂暴时攻击速度倍率")]
    public float berserkAttackSpeedMultiplier = 1.5f;
    [Tooltip("狂暴时移动速度倍率")]
    public float berserkMoveSpeedMultiplier = 1.2f;
    [Tooltip("狂暴时防御降低比例")]
    public float berserkDefenseReduction = 0.3f;

    // 狂暴状态
    private bool isBerserk = false;

    [Header("低血量逃跑设置")]
    [Tooltip("低血量逃跑的血量比例阈值 (0-1)")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.3f;
    [Tooltip("逃跑时检测敌人的范围")]
    public float fleeDetectionRange = 15f;
    [Tooltip("逃跑时的移动速度倍率")]
    public float fleeSpeedMultiplier = 1.5f;
    [Tooltip("逃跑时仍可攻击的间隔倍率")]
    public float fleeAttackIntervalMultiplier = 0.5f;

    // 低血量逃跑相关
    private bool isFleeing = false;
    private Vector2 fleeDirection = Vector2.zero;
    private float fleeDirectionUpdateTimer = 0f;
    private float fleeDirectionUpdateInterval = 1f;

    [Header("脱战HP回复设置")]
    [Tooltip("脱战后开始回复HP的延迟时间（秒）")]
    public float outOfCombatHealDelay = 3f;
    [Tooltip("HP回复比例（每秒回复最大HP的百分比）")]
    [Range(0f, 1f)]
    public float healRatePerSecond = 0.05f;

    // 脱战HP回复相关
    private float outOfCombatTimer = 0f;
    private bool isOutOfCombat = true;

    [Header("巡逻设置")]
    public bool enablePatrol = true;
    public float patrolRange = 5f;
    public float patrolWaitTime = 2f;
    public float patrolSpeed = 2f;
    private Vector3 spawnPosition;
    private Vector3 patrolTarget;
    private bool isPatrolling = false;
    private float patrolWaitTimer = 0f;

    [Header("感知组件")]
    public Transform player;
    private HealthSystem playerHealth;
    private float playerSearchCooldown = 0.5f;
    private float lastPlayerSearchTime = 0f;

    // 召唤物相关
    private Transform currentTarget;
    private bool isTargetingSummon = false;
    private float targetCheckTimer = 0f;
    private float targetCheckInterval = 1f;
    private float summonDefeatTimer = 0f;
    private bool justDefeatedSummon = false;

    [Header("AI组件")]
    private Rigidbody2D rb;
    private Animator anim;
    private HealthSystem enemyHealth;

    // 缓存的距离计算
    private float cachedDistanceToPlayer = float.MaxValue;
    private float distanceUpdateInterval = 0.1f;
    private float lastDistanceUpdateTime = 0f;

    [Header("攻击触发器")]
    public GameObject enemyAttackTriggerPrefab;
    public Transform attackTriggerPos;

    [Header("状态")]
    public bool isChasing = false;
    private bool isAttacking = false;
    private bool playerInAttackRange = false;
    private bool canAttack = true;
    private float attackCooldown = 0.5f;
    private bool isMoving = false;
    private Vector2 moveDirection = Vector2.zero;
    private bool isDead = false;
    
    // 战斗音乐状态
    private bool isBattleMusicPlaying = false;
    private const float BATTLE_MUSIC_DISTANCE = 7f;
    private const float BATTLE_MUSIC_DISTANCE_SQR = BATTLE_MUSIC_DISTANCE * BATTLE_MUSIC_DISTANCE;

    [Header("弱点与抗性")]
    public List<ElementType> weaknesses = new List<ElementType>();
    public List<ElementType> resistances = new List<ElementType>();

    [Header("外观设置")]
    public float scaleMultiplier = 2.5f;

    [Header("调试设置")]
    [Tooltip("启用动画朝向调试日志")]
    public bool debugAnimationFacing = false;
    [Tooltip("显示当前缩放状态")]
    public bool showScaleDebug = false;

    void Start()
    {
        spawnPosition = transform.position;
        transform.localScale = Vector3.one * scaleMultiplier;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<HealthSystem>();

        if (player != null)
            playerHealth = player.GetComponent<HealthSystem>();

        if (enablePatrol)
        {
            SetNewPatrolTarget();
        }

        currentTarget = player;
        justDefeatedSummon = false;

        originalChaseRange = chaseRange;
        expandedChaseRange = chaseRange + chaseRangeExpansion;

        isFleeing = false;
        isBerserk = false;
        isOutOfCombat = true;
        outOfCombatTimer = 0f;
    }

    // ==================== 狂暴机制 ====================
    void CheckBerserkState()
    {
        if (enemyHealth == null) return;

        bool shouldBeBerserk = enemyHealth.GetHealthPercent() <= berserkHealthThreshold;

        if (shouldBeBerserk && !isBerserk)
        {
            EnterBerserkState();
        }
        else if (!shouldBeBerserk && isBerserk)
        {
            ExitBerserkState();
        }
    }

    void EnterBerserkState()
    {
        isBerserk = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"{gameObject.name} 进入狂暴状态！");
#endif

        SetAnimatorTriggerSafe("Berserk");

        if (enemyHealth != null)
        {
            enemyHealth.SetDefenseMultiplier(1f - berserkDefenseReduction);
        }
    }

    void ExitBerserkState()
    {
        isBerserk = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"{gameObject.name} 退出狂暴状态");
#endif

        if (enemyHealth != null)
        {
            enemyHealth.SetDefenseMultiplier(1f);
        }
    }

    float GetCurrentMoveSpeed()
    {
        float speed = moveSpeed;
        if (isBerserk)
            speed *= berserkMoveSpeedMultiplier;
        if (isFleeing)
            speed *= fleeSpeedMultiplier;
        return speed;
    }

    float GetCurrentAttackRate()
    {
        float rate = attackRate;
        if (isBerserk)
            rate *= berserkAttackSpeedMultiplier;
        return rate;
    }

    // ==================== 目标查找 ====================
    void FindNearestTarget()
    {
        Transform nearestTarget = player;
        isTargetingSummon = false;

        // 缓存当前位置，避免重复访问transform.position
        Vector2 currentPosition = transform.position;
        
        GameObject[] summons = GameObject.FindGameObjectsWithTag("SummonedCreature");
        GameObject nearestSummon = null;
        float nearestSummonDistanceSquared = float.MaxValue;

        foreach (GameObject summon in summons)
        {
            if (summon != null)
            {
                // 使用sqrMagnitude代替Distance，避免平方根计算
                float distanceToSummonSquared = ((Vector2)summon.transform.position - currentPosition).sqrMagnitude;
                if (distanceToSummonSquared < nearestSummonDistanceSquared)
                {
                    nearestSummonDistanceSquared = distanceToSummonSquared;
                    nearestSummon = summon;
                }
            }
        }

        if (nearestSummon != null && player != null)
        {
            // 使用sqrMagnitude代替Distance，避免平方根计算
            float distanceToPlayerSquared = ((Vector2)player.position - currentPosition).sqrMagnitude;
            float attackRangeThresholdSquared = (attackRange * 1.5f) * (attackRange * 1.5f);

            if (distanceToPlayerSquared <= attackRangeThresholdSquared)
            {
                nearestTarget = player;
                isTargetingSummon = false;
            }
            else
            {
                nearestTarget = nearestSummon.transform;
                isTargetingSummon = true;
            }
        }
        else if (nearestSummon != null)
        {
            nearestTarget = nearestSummon.transform;
            isTargetingSummon = true;
        }
        else
        {
            nearestTarget = player;
            isTargetingSummon = false;
        }

        currentTarget = nearestTarget;
    }

    void OnSummonDefeated()
    {
        justDefeatedSummon = true;
        summonDefeatTimer = 0f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"{gameObject.name} 击败了召唤物！进入庆祝状态");
#endif
    }

    void Update()
    {
        if (player == null)
        {
            if (Time.time - lastPlayerSearchTime >= playerSearchCooldown)
            {
                lastPlayerSearchTime = Time.time;
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    playerHealth = player.GetComponent<HealthSystem>();
                }
                else return;
            }
            else return;
        }

        CheckBerserkState();

        if (justDefeatedSummon)
        {
            summonDefeatTimer += Time.deltaTime;
            if (summonDefeatTimer >= 1.5f)
            {
                justDefeatedSummon = false;
                summonDefeatTimer = 0f;
            }
            else
            {
                if (rb != null)
                    rb.velocity = Vector2.zero;
                UpdateMovementAnimation();
                return;
            }
        }

        if (currentTarget != null && isTargetingSummon)
        {
            if (currentTarget.gameObject == null || !currentTarget.gameObject.activeInHierarchy)
            {
                OnSummonDefeated();
            }
        }

        targetCheckTimer += Time.deltaTime;
        if (targetCheckTimer >= targetCheckInterval)
        {
            targetCheckTimer = 0f;
            FindNearestTarget();
        }

        bool wasFleeing = isFleeing;
        if (enemyHealth != null)
        {
            float healthPercent = enemyHealth.GetHealthPercent();
            isFleeing = healthPercent <= lowHealthThreshold;

            if (wasFleeing != isFleeing)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (isFleeing)
                {
                    Debug.Log($"{gameObject.name} 血量过低 ({healthPercent:P1})，进入逃跑状态！");
                }
                else
                {
                    Debug.Log($"{gameObject.name} 血量恢复 ({healthPercent:P1})，退出逃跑状态");
                }
#endif
            }
        }

        if (enemyHealth != null && !isFleeing)
        {
            bool currentlyInCombat = isChasing;

            if (currentlyInCombat)
            {
                if (!isOutOfCombat)
                {
                    isOutOfCombat = true;
                    outOfCombatTimer = 0f;
                }
            }
            else
            {
                if (isOutOfCombat)
                {
                    isOutOfCombat = false;
                    outOfCombatTimer = 0f;
                }

                outOfCombatTimer += Time.deltaTime;

                if (outOfCombatTimer >= outOfCombatHealDelay)
                {
                    float healAmount = enemyHealth.maxHealth * healRatePerSecond * Time.deltaTime;
                    enemyHealth.Heal(healAmount);
                }
            }
        }

        if (Time.time - lastDistanceUpdateTime >= distanceUpdateInterval)
        {
            lastDistanceUpdateTime = Time.time;
            if (currentTarget != null)
            {
                Vector2 directionToTarget = (Vector2)(currentTarget.position - transform.position);
                cachedDistanceToPlayer = directionToTarget.sqrMagnitude;
            }
            else
            {
                cachedDistanceToPlayer = float.MaxValue;
            }
        }

        float currentChaseRange = isChaseRangeExpanded ? expandedChaseRange : originalChaseRange;
        float currentChaseRangeSqr = currentChaseRange * currentChaseRange;

        if (cachedDistanceToPlayer <= (originalChaseRange * originalChaseRange) && !isChaseRangeExpanded)
        {
            isChaseRangeExpanded = true;
        }
        else if (isChaseRangeExpanded && cachedDistanceToPlayer > (expandedChaseRange * expandedChaseRange))
        {
            isChaseRangeExpanded = false;
        }

        bool wasChasing = isChasing;
        
        if (isFleeing)
        {
            isChasing = false;
            FleeBehavior();
        }
        else if (cachedDistanceToPlayer <= currentChaseRangeSqr)
        {
            isChasing = true;
            HandleBehaviorByMode(Mathf.Sqrt(cachedDistanceToPlayer));
        }
        else
        {
            isChasing = false;
            if (enablePatrol)
            {
                PatrolBehavior();
            }
            else rb.velocity = Vector2.zero;
        }
        
        // 基于距离的战斗音乐逻辑
        bool shouldPlayBattleMusic = cachedDistanceToPlayer <= BATTLE_MUSIC_DISTANCE_SQR;
        
        if (shouldPlayBattleMusic && !isBattleMusicPlaying)
        {
            // 距离小于等于7，开始播放战斗音乐
            GlobalEventManager.Instance.TriggerBattleStart(gameObject);
            isBattleMusicPlaying = true;
        }
        else if (!shouldPlayBattleMusic && isBattleMusicPlaying)
        {
            // 距离大于7，停止播放战斗音乐
            GlobalEventManager.Instance.TriggerBattleEnd(gameObject);
            isBattleMusicPlaying = false;
        }

        UpdateMovementAnimation();
    }

    void HandleBehaviorByMode(float distanceToTarget)
    {
        switch (behaviorMode)
        {
            case BehaviorMode.Pursuit:
                HandlePursuitBehavior(distanceToTarget);
                break;
            case BehaviorMode.Ranged:
                HandleRangedBehavior(distanceToTarget);
                break;
            case BehaviorMode.Berserk:
                HandleBerserkBehavior(distanceToTarget);
                break;
            case BehaviorMode.Ambush:
                HandleAmbushBehavior(distanceToTarget);
                break;
            case BehaviorMode.Summoner:
                HandleSummonerBehavior(distanceToTarget);
                break;
            case BehaviorMode.Tactical:
                HandleTacticalBehavior(distanceToTarget);
                break;
            default:
                HandlePursuitBehavior(distanceToTarget);
                break;
        }
    }

    void HandlePursuitBehavior(float distanceToTarget)
    {
        float attackRangeSqr = attackRange * attackRange;

        if (cachedDistanceToPlayer > attackRangeSqr)
        {
            Vector2 direction = ((Vector2)(currentTarget.position - transform.position)).normalized;
            if (rb != null)
            {
                rb.velocity = direction * GetCurrentMoveSpeed();
            }
            playerInAttackRange = false;
        }
        else
        {
            rb.velocity = Vector2.zero;
            playerInAttackRange = true;
            AttackCurrentTarget();
        }
    }

    void HandleRangedBehavior(float distanceToTarget)
    {
        float preferredRangeSqr = (attackRange * 1.5f) * (attackRange * 1.5f);
        float tooCloseRangeSqr = (attackRange * 0.8f) * (attackRange * 0.8f);

        if (cachedDistanceToPlayer > preferredRangeSqr)
        {
            Vector2 direction = ((Vector2)(currentTarget.position - transform.position)).normalized;
            if (rb != null)
            {
                rb.velocity = direction * GetCurrentMoveSpeed() * 0.7f;
            }
        }
        else if (cachedDistanceToPlayer < tooCloseRangeSqr)
        {
            Vector2 direction = ((Vector2)(transform.position - currentTarget.position)).normalized;
            if (rb != null)
            {
                rb.velocity = direction * GetCurrentMoveSpeed() * 0.5f;
            }
        }
        else
        {
            AttackCurrentTarget();

            if (player.position.x > transform.position.x)
                transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
            else
                transform.localScale = new Vector3(-scaleMultiplier, scaleMultiplier, scaleMultiplier);
        }
    }

    void HandleBerserkBehavior(float distanceToTarget)
    {
        HandlePursuitBehavior(distanceToTarget);
    }

    void HandleAmbushBehavior(float distanceToTarget)
    {
        HandlePursuitBehavior(distanceToTarget);
    }

    void HandleSummonerBehavior(float distanceToTarget)
    {
        HandleRangedBehavior(distanceToTarget);
    }

    void HandleTacticalBehavior(float distanceToTarget)
    {
        HandlePursuitBehavior(distanceToTarget);
    }

    void PatrolBehavior()
    {
        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            isPatrolling = false;
            rb.velocity = Vector2.zero;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, patrolTarget);
        if (distanceToTarget < 0.5f)
        {
            patrolWaitTimer = patrolWaitTime;
            SetNewPatrolTarget();
            isPatrolling = false;
            rb.velocity = Vector2.zero;
        }
        else
        {
            Vector2 direction = ((Vector2)(patrolTarget - transform.position)).normalized;
            if (rb != null)
            {
                rb.velocity = direction * patrolSpeed;
            }
            isPatrolling = true;
        }
    }

    void SetNewPatrolTarget()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * patrolRange;
        patrolTarget = spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        isPatrolling = true;
    }

    void CalculateFleeDirection()
    {
        // 缓存当前位置
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
            fleeDirection = UnityEngine.Random.insideUnitCircle.normalized;
            return;
        }

        // 预计算方向角度
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

            // 使用sqrMagnitude代替magnitude，避免平方根计算
            Vector2 directionToThreat = (Vector2)threat.transform.position - currentPosition;
            float distanceSquared = directionToThreat.sqrMagnitude;

            if (distanceSquared <= fleeDetectionRangeSquared)
            {
                float angle = Mathf.Atan2(directionToThreat.y, directionToThreat.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360;

                // 找到最接近的方向
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

        // 找到威胁最大的方向
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

        // 计算逃跑方向（与威胁最大的方向相反）
        float fleeAngle = directionAngles[maxThreatDirection];
        // 反方向逃跑
        fleeAngle += 180f;
        if (fleeAngle >= 360f) fleeAngle -= 360f;
        
        fleeDirection = new Vector2(Mathf.Cos(fleeAngle * Mathf.Deg2Rad), Mathf.Sin(fleeAngle * Mathf.Deg2Rad));

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"{gameObject.name} 逃跑方向: {fleeDirection}, 威胁数量: {maxThreatCount}, 角度: {fleeAngle:F1}°");
#endif
    }

    void FleeBehavior()
    {
        fleeDirectionUpdateTimer += Time.deltaTime;
        if (fleeDirectionUpdateTimer >= fleeDirectionUpdateInterval)
        {
            fleeDirectionUpdateTimer = 0f;
            CalculateFleeDirection();
        }

        // 应用逃跑速度
        Vector2 fleeVelocity = fleeDirection * GetCurrentMoveSpeed();
        if (rb != null)
        {
            rb.velocity = fleeVelocity;
        }

        // 缓存当前位置
        Vector2 currentPosition = transform.position;
        Transform targetInRange = null;
        float attackRangeSquared = attackRange * attackRange;

        // 检查玩家是否在攻击范围内
        if (player != null)
        {
            float distanceToPlayerSquared = ((Vector2)player.position - currentPosition).sqrMagnitude;
            if (distanceToPlayerSquared <= attackRangeSquared)
            {
                targetInRange = player;
            }
        }

        // 检查召唤物是否在攻击范围内
        if (targetInRange == null)
        {
            GameObject[] summons = GameObject.FindGameObjectsWithTag("SummonedCreature");
            foreach (GameObject summon in summons)
            {
                if (summon != null)
                {
                    float distanceToSummonSquared = ((Vector2)summon.transform.position - currentPosition).sqrMagnitude;
                    if (distanceToSummonSquared <= attackRangeSquared)
                    {
                        targetInRange = summon.transform;
                        break; // 找到第一个在范围内的召唤物就停止
                    }
                }
            }
        }

        // 如果有目标在攻击范围内，进行攻击
        if (targetInRange != null)
        {
            bool wasTargetingSummon = isTargetingSummon;
            isTargetingSummon = targetInRange.CompareTag("SummonedCreature");
            currentTarget = targetInRange;

            // 检查攻击冷却
            float attackCooldown = (1f / GetCurrentAttackRate()) * fleeAttackIntervalMultiplier;
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackCurrentTarget();
            }
        }
    }

    void AttackCurrentTarget()
    {
        // 检查攻击条件
        float attackInterval = 1f / GetCurrentAttackRate();
        if (Time.time - lastAttackTime < attackInterval || !canAttack || currentTarget == null)
        {
            return;
        }

        // 检查目标是否在攻击范围内
        bool inRange = false;
        if (isTargetingSummon)
        {
            float distanceToTargetSquared = ((Vector2)currentTarget.position - (Vector2)transform.position).sqrMagnitude;
            float attackRangeSquared = attackRange * attackRange;
            inRange = distanceToTargetSquared <= attackRangeSquared;
        }
        else
        {
            inRange = playerInAttackRange;
        }

        if (!inRange) return;

        // 执行攻击
        lastAttackTime = Time.time;

        // 播放攻击动画
        SetAnimatorTriggerSafe("Attack");
        
        // 播放攻击音效
        try
        {
            GameplayAudioManager.Instance?.PlayEnemyHit();
        }
        catch { }

        // 应用伤害
        if (enemyAttackTriggerPrefab != null && attackTriggerPos != null)
        {
            // 使用攻击触发器
            GameObject trigger = Instantiate(enemyAttackTriggerPrefab, attackTriggerPos.position, attackTriggerPos.rotation, attackTriggerPos);
            var attackTrigger = trigger.GetComponent(System.Reflection.Assembly.GetExecutingAssembly().GetType("AttackTrigger"));
            if (attackTrigger != null)
            {
                var attackTriggerType = attackTrigger.GetType();
                var setDamageMethod = attackTriggerType.GetMethod("SetDamage");
                var setAttackerMethod = attackTriggerType.GetMethod("SetAttacker");

                if (setDamageMethod != null)
                {
                    int finalDamage = (int)(damage + UnityEngine.Random.Range(0, 6));
                    setDamageMethod.Invoke(attackTrigger, new object[] { finalDamage });
                }
                if (setAttackerMethod != null)
                {
                    setAttackerMethod.Invoke(attackTrigger, new object[] { gameObject, false, false });
                }
            }
        }
        else
        {
            // 直接应用伤害
            var targetHealth = currentTarget.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }

        // 重置攻击状态
        canAttack = false;
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        canAttack = true;
    }

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

        Vector2 directionToTarget = Vector2.zero;
        if (currentTarget != null)
        {
            directionToTarget = ((Vector2)(currentTarget.position - transform.position)).normalized;
        }
        else if (isFleeing)
        {
            directionToTarget = fleeDirection;
        }

        if (currentlyMoving)
        {
            Vector2 moveDirection = velocity.normalized;

            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

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
        else if (isChasing && currentTarget != null)
        {
            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);

            if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y))
            {
                if (directionToTarget.x > 0)
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
                if (directionToTarget.y > 0)
                {
                    SetAnimatorBoolSafe("MoveUp", true);
                }
                else
                {
                    SetAnimatorBoolSafe("MoveDown", true);
                }
            }
        }
        else if (isPatrolling && enablePatrol)
        {
            SetAnimatorBoolSafe("IsMoving", currentlyMoving);

            if (currentlyMoving)
            {
                SetAnimatorBoolSafe("MoveUp", false);
                SetAnimatorBoolSafe("MoveDown", false);
                SetAnimatorBoolSafe("MoveLeft", false);
                SetAnimatorBoolSafe("MoveRight", false);

                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                {
                    if (velocity.x > 0)
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
                    if (velocity.y > 0)
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
                SetAnimatorBoolSafe("IsMoving", false);
                SetAnimatorBoolSafe("MoveUp", false);
                SetAnimatorBoolSafe("MoveDown", false);
                SetAnimatorBoolSafe("MoveLeft", false);
                SetAnimatorBoolSafe("MoveRight", false);
            }
        }
        else
        {
            SetAnimatorBoolSafe("IsMoving", false);
            SetAnimatorBoolSafe("MoveUp", false);
            SetAnimatorBoolSafe("MoveDown", false);
            SetAnimatorBoolSafe("MoveLeft", false);
            SetAnimatorBoolSafe("MoveRight", false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInAttackRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInAttackRange = false;
        }
    }
    
    // 调整敌人大小的方法
    public void SetScale(float newScale)
    {
        scaleMultiplier = newScale;
        transform.localScale = Vector3.one * scaleMultiplier;
        Debug.Log($"敌人大小已调整为 {scaleMultiplier} 倍");
    }

    [Header("灵魂之核设置")]
    public GameObject soulCorePrefab;
    public Sprite[] soulCoreSprites;

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log(gameObject.name + " 被击败了！");

        enabled = false;
        if (rb != null)
            rb.velocity = Vector2.zero;

        SetAnimatorBoolSafe("IsMoving", false);
        SetAnimatorBoolSafe("MoveUp", false);
        SetAnimatorBoolSafe("MoveDown", false);
        SetAnimatorBoolSafe("MoveLeft", false);
        SetAnimatorBoolSafe("MoveRight", false);

        SetAnimatorTriggerSafe("Die");

        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.TriggerEnemyDefeated(gameObject);
        }

        SpawnSoulCore();

        Destroy(gameObject, 0.5f);
    }

    void SpawnSoulCore()
    {
        Debug.Log($"生成灵魂之核：{enemyType} 在位置 {transform.position}");

        if (soulCorePrefab != null)
        {
            GameObject prefabInstance = Instantiate(soulCorePrefab, transform.position, Quaternion.identity);
            var prefabSoulCore = prefabInstance.GetComponent(System.Reflection.Assembly.GetExecutingAssembly().GetType("SoulCore"));
            if (prefabSoulCore != null)
            {
                var soulCoreType = prefabSoulCore.GetType();
                var setEnemyTypeMethod = soulCoreType.GetMethod("SetEnemyType");
                var setQualityMethod = soulCoreType.GetMethod("SetQuality");
                if (setEnemyTypeMethod != null)
                {
                    setEnemyTypeMethod.Invoke(prefabSoulCore, new object[] { enemyType });
                }
                if (setQualityMethod != null)
                {
                    setQualityMethod.Invoke(prefabSoulCore, new object[] { enemyQuality });
                }
            }
        }
        else
        {
            GameObject soulCoreObj = new GameObject("SoulCore_" + enemyType);
            soulCoreObj.transform.position = transform.position;

            var soulCore = soulCoreObj.AddComponent(System.Reflection.Assembly.GetExecutingAssembly().GetType("SoulCore"));
            var soulCoreType1 = soulCore.GetType();
            var setEnemyTypeMethod = soulCoreType1.GetMethod("SetEnemyType");
            var setQualityMethod = soulCoreType1.GetMethod("SetQuality");
            if (setEnemyTypeMethod != null)
            {
                setEnemyTypeMethod.Invoke(soulCore, new object[] { enemyType });
            }
            if (setQualityMethod != null)
            {
                setQualityMethod.Invoke(soulCore, new object[] { enemyQuality });
            }
        }
    }
}
