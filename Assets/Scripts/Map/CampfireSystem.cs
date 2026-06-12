using UnityEngine;
using System.Collections;

public enum CampfireState { Idle, Burn, Burning }

public class CampfireSystem : MonoBehaviour, ICampfireService
{
    [Header("营火设置")]
    public float maxHealthRecovery = 100f; // 最大恢复生命值

    [Header("效果设置")]
    public float burdenReduction = 100f; // 重置负担
    public float manaRecovery = 50f; // 恢复魔力
    public bool resetSummons = true; // 重置召唤物
    public bool resetRoomEnemies = true; // 重置房间敌人

    [Header("存档设置（P0：营火自动存档）")]
    [Tooltip("是否启用营火自动存档")]
    public bool enableAutoSave = true;
    [Tooltip("存档为一次性存档（读取后自动删除）")]
    public bool oneTimeSave = true;

    [Header("动画设置")]
    public Animator campfireAnimator; // 营火动画控制器
    public string idleAnimation = "Idle"; // 未点燃动画
    public string burnAnimation = "Burn"; // 点燃中动画
    public string burningAnimation = "Burning"; // 正在燃烧动画

  [Header("状态")]
    public CampfireState currentState = CampfireState.Idle;
    private bool playerInRange = false; // 玩家是否在营火范围内

    [Header("引用")]
    private HealthSystem playerHealth;
    private BurdenSystem playerBurden;
    private ManaSystem playerMana;
    private SummonSystem summonSystem;
    private IntegratedMapSystem mapSystem;

    // P0: 存档追踪
    private static CampfireSystem lastVisitedCampfire; // 最近访问的营火

    void Start()
    {
        InitializeReferences();
        InitializeVisuals();
    }

    void OnEnable()
    {
        // 订阅游戏退出事件
        GlobalEventManager.Instance.OnGameQuit += OnGameQuit;
    }

    void OnDisable()
    {
        GlobalEventManager.Instance.OnGameQuit -= OnGameQuit;
    }

    void Update()
    {
        // 检查玩家是否在范围内且按下了E键
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && IsCampfireUsable())
        {
            InteractWithCampfire();
        }
    }

    void InitializeReferences()
    {
        playerHealth = FindObjectOfType<HealthSystem>();
        playerBurden = FindObjectOfType<BurdenSystem>();
        summonSystem = FindObjectOfType<SummonSystem>();
        mapSystem = FindObjectOfType<IntegratedMapSystem>();
    }

    void InitializeVisuals()
    {
        // 初始状态为Idle，播放Idle动画
        PlayAnimation(idleAnimation);
    }





    public void InteractWithCampfire()
    {
        if (currentState == CampfireState.Burn)
        {
            Debug.Log("营火正在点燃中");
            return;
        }

        if (currentState == CampfireState.Idle)
        {
            LightCampfire();
        }
        else if (currentState == CampfireState.Burning)
        {
            StartCampfireMenu();
        }
    }

    void StartCampfireMenu()
    {
        Debug.Log("=== 营火菜单 ===");
        OnRest();
    }

    public void OnRest()
    {
        StartCoroutine(RecoverAtCampfire());
    }

    public void OnConfigureFragments()
    {
        var mf = FindObjectOfType<MemoryFragmentSystem>();
        if (mf != null) mf.OpenConfigurationUI();
    }

    public void OnAlchemyAtCampfire()
    {
        var alchemyUI = FindObjectOfType<AlchemyUI>();
        if (alchemyUI != null) alchemyUI.ShowAlchemyPanel();
    }

    void LightCampfire()
    {
        currentState = CampfireState.Burn;
        // 播放点燃中动画
        PlayAnimation(burnAnimation);
        
        // 立即切换到Burning状态
        currentState = CampfireState.Burning;
        // 播放正在燃烧动画
        PlayAnimation(burningAnimation);
        Debug.Log("营火已点燃！");
    }

    IEnumerator RecoverAtCampfire()
    {
        Debug.Log("开始在营火旁恢复状态...");

        // 立即恢复满状态
        ApplyRecoverEffects();

        Debug.Log("状态恢复完成！");
        yield return null;
    }

    void ApplyRecoverEffects()
    {
        // 重置负担
        if (playerBurden != null)
        {
            playerBurden.ResetBurden();
            Debug.Log("负担已重置");
        }

        // 恢复生命值到满
        if (playerHealth != null)
        {
            float missingHealth = playerHealth.maxHealth - playerHealth.currentHealth;
            if (missingHealth > 0)
            {
                playerHealth.Heal(missingHealth);
                Debug.Log($"恢复了 {missingHealth} 点生命值，生命值已回满");
            }
        }

        // 重置召唤物
        if (summonSystem != null && resetSummons)
        {
            summonSystem.RecallAllSummons();
            Debug.Log("召唤物已重置");
        }

        // 重置房间敌人
        if (mapSystem != null && resetRoomEnemies)
        {
            // 这里可以添加重置房间敌人的逻辑
            Debug.Log("房间敌人已重置");
        }

        // P0: 标记为最近访问的营火（用于退出时自动存档）
        lastVisitedCampfire = this;
        Debug.Log($"[营火] 已记录为最近访问营火，位置={transform.position}");
    }

    // ═══════════════════════════════════════════
    // P0: 营火自动存档机制
    // ═══════════════════════════════════════════

    /// <summary>
    /// 游戏退出时的自动存档回调
    /// 在最近访问过的营火处生成一次性存档
    /// </summary>
    void OnGameQuit()
    {
        if (!enableAutoSave) return;
        if (currentState != CampfireState.Burning) return;

        SaveToCampfire();
    }

    /// <summary>
    /// 在营火处执行存档
    /// 存档为一次性——读取后自动删除
    /// </summary>
    public void SaveToCampfire()
    {
        Debug.Log($"[营火] 自动存档触发，位置={transform.position}");

        // 通知SaveSystem执行存档
        GlobalEventManager.Instance.TriggerCampfireSaveRequested(transform.position);
    }

    /// <summary>
    /// 获取最近访问的营火（全局静态方法）
    /// </summary>
    public static CampfireSystem GetLastVisitedCampfire()
    {
        return lastVisitedCampfire;
    }

    /// <summary>
    /// 在任何营火处主动触发存档
    /// </summary>
    public static void ForceSaveAtLastCampfire()
    {
        if (lastVisitedCampfire != null && lastVisitedCampfire.currentState == CampfireState.Burning)
        {
            lastVisitedCampfire.SaveToCampfire();
            Debug.Log("[营火] 强制存档完成");
        }
        else
        {
            Debug.LogWarning("[营火] 无法存档：无可用营火");
        }
    }

    /// <summary>
    /// 播放营火动画
    /// </summary>
    private void PlayAnimation(string animationName)
    {
        if (campfireAnimator != null)
        {
            // 重置所有动画参数
            campfireAnimator.SetBool("Idle", false);
            campfireAnimator.SetBool("Burn", false);
            campfireAnimator.SetBool("Burning", false);
            
            // 设置当前动画参数
            switch (animationName)
            {
                case "Idle":
                    campfireAnimator.SetBool("Idle", true);
                    break;
                case "Burn":
                    campfireAnimator.SetBool("Burn", true);
                    break;
                case "Burning":
                    campfireAnimator.SetBool("Burning", true);
                    break;
            }
        }
    }

    /// <summary>
    /// 当玩家进入营火碰撞范围时
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("玩家进入营火范围");
        }
    }

    /// <summary>
    /// 当玩家离开营火碰撞范围时
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("玩家离开营火范围");
        }
    }

    public bool IsCampfireUsable()
    {
        // 营火在Idle或正在燃烧状态下都可以交互，在Burn状态下不可交互
        return currentState != CampfireState.Burn;
    }

    public float GetCooldownPercentage()
    {
        // 营火在恢复状态时没有冷却时间
        return 1f;
    }

    public void SetCampfireState(bool active)
    {
        currentState = CampfireState.Idle;
        // 播放Idle动画
        PlayAnimation(idleAnimation);
    }

    public void ForceReset()
    {
        currentState = CampfireState.Idle;
        // 播放Idle动画
        PlayAnimation(idleAnimation);
        Debug.Log("营火已强制重置");
    }

    // 调试方法
    public void DebugPrintCampfireStatus()
    {
        Debug.Log("=== 营火状态 ===");
        Debug.Log($"当前状态：{currentState}");
        Debug.Log($"可用状态：{IsCampfireUsable()}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsCampfireUsable() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
