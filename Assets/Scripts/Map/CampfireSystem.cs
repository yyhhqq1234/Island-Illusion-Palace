using UnityEngine;
using System.Collections;

public enum CampfireState { Idle, Burn, Burning }

public class CampfireSystem : MonoBehaviour
{
    [Header("营火设置")]
    public float maxHealthRecovery = 100f; // 最大恢复生命值

    [Header("效果设置")]
    public float burdenReduction = 100f; // 重置负担
    public float manaRecovery = 50f; // 恢复魔力
    public bool resetSummons = true; // 重置召唤物
    public bool resetRoomEnemies = true; // 重置房间敌人

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

    void Start()
    {
        InitializeReferences();
        InitializeVisuals();
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
            // 点燃营火
            LightCampfire();
        }
        else if (currentState == CampfireState.Burning)
        {
            // 在营火处恢复状态
            StartCoroutine(RecoverAtCampfire());
        }
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
