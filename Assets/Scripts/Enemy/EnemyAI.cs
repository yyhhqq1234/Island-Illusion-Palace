using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敌人AI控制器 - 组件化架构的协调器
/// 保留嵌套枚举以向后兼容，内部逻辑委托给各个子组件
/// </summary>
public class EnemyAI : MonoBehaviour, IEnemyProvider, ILootProvider, IDieHandler
{
    // ==================== 嵌套枚举（向后兼容） ====================
    public enum EnemyType
    {
        CorruptedVillager, CrystalLizard, SwampStalker, IceWolf,
        MechanicalDebris, SkeletonWarrior, Wraith, Gargoyle,
        SoulEater, LavaElemental, MechanicalConstruct,
        TimeGuardian, MemoryGuardian, CorruptionGuardian, ScarletSoulShana
    }

    public enum EnemyCategory
    {
        CrystalCreature, SoulRemnant, TechConstruct, MemoryManifestation
    }

    public enum EnemyQuality
    {
        Common, Elite, Boss
    }

    public enum ElementType
    {
        Frost, Water, Fire, Lightning, Soul, Holy, Physical
    }

    public enum BehaviorMode
    {
        Pursuit, Berserk, Ambush, Summoner, Ranged, Tactical
    }

    // ==================== IEnemyProvider 接口实现 ====================
    float IEnemyProvider.damage
    {
        get => combatComponent != null ? combatComponent.baseDamage : 0f;
        set { if (combatComponent != null) combatComponent.baseDamage = value; }
    }
    bool IEnemyProvider.isBerserk
    {
        get => stateComponent != null && stateComponent.IsBerserk;
        set { if (value && stateComponent != null) stateComponent.EnterBerserkState(); }
    }
    void IEnemyProvider.EnterBerserkState() => stateComponent?.EnterBerserkState();
    void IEnemyProvider.TakeDamage(float amount)
    {
        var hs = GetComponent<HealthSystem>();
        if (hs != null) hs.TakeDamage(amount);
    }

    // ==================== ILootProvider 接口实现 ====================
    DropTableData ILootProvider.dropTable
    {
        get => lootComponent != null ? lootComponent.dropTable : null;
        set { if (lootComponent != null) lootComponent.dropTable = value; }
    }
    int ILootProvider.soulDropAmount
    {
        get => lootComponent != null ? lootComponent.soulDropAmount : 0;
        set { if (lootComponent != null) lootComponent.soulDropAmount = value; }
    }
    int ILootProvider.essenceDropAmount
    {
        get => lootComponent != null ? lootComponent.essenceDropAmount : 0;
        set { if (lootComponent != null) lootComponent.essenceDropAmount = value; }
    }
    void ILootProvider.SpawnSoulCore() => lootComponent?.SpawnSoulCore(enemyType, enemyQuality);

    // ==================== 敌人配置 ====================
    [Header("敌人基础设置")]
    public EnemyType enemyType;
    public EnemyCategory enemyCategory;
    public EnemyQuality enemyQuality;
    public BehaviorMode behaviorMode;
    public ElementType primaryElement;
    public ElementType elementType;

    [Header("弱点与抗性")]
    public List<ElementType> weaknesses = new List<ElementType>();
    public List<ElementType> resistances = new List<ElementType>();

    public bool IsWeakness(ElementType element) => weaknesses.Contains(element);
    public bool IsResistance(ElementType element) => resistances.Contains(element);

    [Header("外观设置")]
    public float scaleMultiplier = 2.5f;

    // ==================== 向后兼容属性（委托给组件） ====================
    /// <summary>攻击伤害 - 委托给战斗组件</summary>
    public float damage
    {
        get => combatComponent != null ? combatComponent.baseDamage : 10f;
        set { if (combatComponent != null) combatComponent.baseDamage = value; }
    }

    /// <summary>移动速度 - 委托给移动组件</summary>
    public float moveSpeed
    {
        get => movementComponent != null ? movementComponent.baseMoveSpeed : 3f;
        set { if (movementComponent != null) movementComponent.baseMoveSpeed = value; }
    }

    /// <summary>攻击范围 - 委托给战斗组件</summary>
    public float attackRange
    {
        get => combatComponent != null ? combatComponent.attackRange : 2f;
        set { if (combatComponent != null) combatComponent.attackRange = value; }
    }

    /// <summary>攻击速率 - 委托给战斗组件</summary>
    public float attackRate
    {
        get => combatComponent != null ? combatComponent.baseAttackRate : 1f;
        set { if (combatComponent != null) combatComponent.baseAttackRate = value; }
    }

    /// <summary>追逐范围 - 委托给移动组件</summary>
    public float chaseRange
    {
        get => movementComponent != null ? movementComponent.chaseRange : 10f;
        set { if (movementComponent != null) movementComponent.chaseRange = value; }
    }

    /// <summary>是否狂暴 - 委托给状态组件</summary>
    public bool isBerserk
    {
        get => stateComponent != null && stateComponent.IsBerserk;
    }

    /// <summary>是否启用巡逻 - 委托给移动组件</summary>
    public bool enablePatrol
    {
        get => movementComponent != null && movementComponent.enablePatrol;
        set { if (movementComponent != null) movementComponent.enablePatrol = value; }
    }

    // ==================== 组件引用 ====================
    [Header("AI组件")]
    [SerializeField] private EnemyStateComponent stateComponent;
    [SerializeField] private EnemyMovementComponent movementComponent;
    [SerializeField] private EnemyCombatComponent combatComponent;
    [SerializeField] private EnemyTargetingComponent targetingComponent;
    [SerializeField] private EnemyBehaviorComponent behaviorComponent;
    [SerializeField] private EnemyAnimationComponent animationComponent;
    [SerializeField] private EnemyLootComponent lootComponent;

    // 内部引用
    private Rigidbody2D rb;
    private Animator anim;
    private HealthSystem enemyHealth;

    // 行为上下文
    private EnemyBehaviorContext behaviorContext;

    // 状态
    private bool isChasing = false;
    private bool isDead = false;

    // 距离缓存
    private float cachedDistanceToTargetSqr = float.MaxValue;
    private float distanceUpdateInterval = 0.1f;
    private float lastDistanceUpdateTime = 0f;

    // 战斗音乐
    private bool isBattleMusicPlaying = false;
    private const float BATTLE_MUSIC_DISTANCE = 7f;
    private const float BATTLE_MUSIC_DISTANCE_SQR = BATTLE_MUSIC_DISTANCE * BATTLE_MUSIC_DISTANCE;

    // 逃跑方向
    private Vector2 fleeDirection = Vector2.zero;
    private float fleeDirectionUpdateTimer = 0f;
    private float fleeDirectionUpdateInterval = 1f;
    private float lastFleeAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<HealthSystem>();

        transform.localScale = Vector3.one * scaleMultiplier;

        // 获取所有子组件
        stateComponent = GetComponent<EnemyStateComponent>();
        movementComponent = GetComponent<EnemyMovementComponent>();
        combatComponent = GetComponent<EnemyCombatComponent>();
        targetingComponent = GetComponent<EnemyTargetingComponent>();
        behaviorComponent = GetComponent<EnemyBehaviorComponent>();
        animationComponent = GetComponent<EnemyAnimationComponent>();
        lootComponent = GetComponent<EnemyLootComponent>();

        // 初始化组件
        stateComponent?.Initialize(enemyHealth);
        animationComponent?.Initialize(anim, rb, scaleMultiplier);
        movementComponent?.Initialize(rb, stateComponent);
        combatComponent?.Initialize(stateComponent, animationComponent);

        // 查找玩家
        FindPlayer();
        targetingComponent?.Initialize(targetingComponent?.Player);

        // 初始化行为上下文
        behaviorContext = new EnemyBehaviorContext
        {
            Movement = movementComponent,
            Combat = combatComponent,
            Animation = animationComponent,
            Targeting = targetingComponent,
            State = stateComponent,
            Transform = transform
        };
        behaviorComponent?.Initialize(behaviorContext);
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && targetingComponent != null)
        {
            targetingComponent.Initialize(playerObj.transform);
        }
    }

    void Update()
    {
        if (isDead) return;

        // 确保玩家引用存在
        if (targetingComponent == null || !targetingComponent.TryFindPlayer())
            return;

        // 更新状态（狂暴、逃跑、脱战回复）
        stateComponent?.UpdateState(Time.deltaTime, isChasing);

        // 处理召唤物击败庆祝
        if (targetingComponent.JustDefeatedSummon)
        {
            movementComponent?.StopMovement();
            UpdateAnimState(Vector2.zero);
            return;
        }

        // 更新目标查找
        float attackRange = combatComponent != null ? combatComponent.attackRange : 2f;
        targetingComponent?.UpdateTargeting(attackRange);

        // 更新距离缓存
        UpdateDistanceCache();

        // 更新追逐范围
        movementComponent?.UpdateChaseRange(cachedDistanceToTargetSqr);

        // 更新战斗音乐
        UpdateBattleMusic();

        // 执行行为
        ExecuteBehaviorByState();

        // 更新动画
        UpdateAnimState(movementComponent != null ? movementComponent.GetVelocity() : Vector2.zero);
    }

    private void UpdateDistanceCache()
    {
        if (Time.time - lastDistanceUpdateTime >= distanceUpdateInterval)
        {
            lastDistanceUpdateTime = Time.time;
            cachedDistanceToTargetSqr = targetingComponent != null
                ? targetingComponent.GetDistanceToTargetSqr()
                : float.MaxValue;
        }
    }

    private void UpdateBattleMusic()
    {
        if (targetingComponent?.Player == null) return;

        bool isPlayerInSafeZone = SafeZoneDetector.IsInAnySafeZone(targetingComponent.Player.position);
        bool shouldPlayBattleMusic = !isPlayerInSafeZone && cachedDistanceToTargetSqr <= BATTLE_MUSIC_DISTANCE_SQR;

        if (shouldPlayBattleMusic && !isBattleMusicPlaying)
        {
            GlobalEventManager.Instance.TriggerBattleStart(gameObject);
            isBattleMusicPlaying = true;
        }
        else if (!shouldPlayBattleMusic && isBattleMusicPlaying)
        {
            GlobalEventManager.Instance.TriggerBattleEnd(gameObject);
            isBattleMusicPlaying = false;
        }
    }

    private void ExecuteBehaviorByState()
    {
        bool isFleeing = stateComponent != null && stateComponent.IsFleeing;

        if (isFleeing)
        {
            isChasing = false;
            ExecuteFleeBehavior();
            return;
        }

        float effectiveChaseRangeSqr = movementComponent != null
            ? movementComponent.GetEffectiveChaseRangeSqr()
            : 100f;

        if (cachedDistanceToTargetSqr <= effectiveChaseRangeSqr)
        {
            isChasing = true;
            behaviorComponent?.ExecuteBehavior(behaviorMode, cachedDistanceToTargetSqr, targetingComponent?.CurrentTarget);
        }
        else
        {
            isChasing = false;
            movementComponent?.UpdatePatrol(Time.deltaTime);
        }
    }

    private void ExecuteFleeBehavior()
    {
        fleeDirectionUpdateTimer += Time.deltaTime;
        if (fleeDirectionUpdateTimer >= fleeDirectionUpdateInterval)
        {
            fleeDirectionUpdateTimer = 0f;
            fleeDirection = movementComponent != null
                ? movementComponent.CalculateFleeDirection()
                : UnityEngine.Random.insideUnitCircle.normalized;
        }

        Vector2 fleeVelocity = fleeDirection * (movementComponent != null ? movementComponent.GetCurrentMoveSpeed() : 3f);
        movementComponent?.SetMovementVelocity(fleeVelocity);

        // 逃跑时反击
        if (combatComponent != null && targetingComponent != null)
        {
            Transform targetInRange = targetingComponent.GetTargetInRange(combatComponent.attackRange);
            if (targetInRange != null)
            {
                if (Time.time - lastFleeAttackTime >= combatComponent.GetFleeAttackInterval())
                {
                    lastFleeAttackTime = Time.time;
                    combatComponent.PerformAttack(targetInRange);
                }
            }
        }
    }

    private void UpdateAnimState(Vector2 velocity)
    {
        if (animationComponent == null) return;

        bool isFleeing = stateComponent != null && stateComponent.IsFleeing;
        bool isPatrolling = movementComponent != null && movementComponent.IsPatrolling;

        animationComponent.UpdateAnimation(velocity, isChasing, isPatrolling, isFleeing, fleeDirection, targetingComponent?.CurrentTarget);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isChasing = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isChasing = false;
        }
    }

    /// <summary>
    /// 调整敌人大小
    /// </summary>
    public void SetScale(float newScale)
    {
        scaleMultiplier = newScale;
        animationComponent?.SetScale(newScale);
    }

    /// <summary>
    /// 死亡处理（IDieHandler 事件驱动接口）
    /// </summary>
    public void OnDie() => Die();

    /// 死亡处理
    /// </summary>
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log(gameObject.name + " 被击败了！");

        enabled = false;
        movementComponent?.StopMovement();

        animationComponent?.TriggerDeath();

        GlobalEventManager.Instance.TriggerEnemyDefeated(gameObject);

        // 掉落由组件处理
        lootComponent?.HandleLoot(enemyType, enemyQuality);

        Destroy(gameObject, 0.5f);
    }
}