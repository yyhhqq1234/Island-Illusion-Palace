using UnityEngine;
using System.Collections;

public enum CauldronState { Idle, Alchemy }

public class CauldronSystem : MonoBehaviour
{


    [Header("效果设置")]
    public bool showCraftingEffect = true; // 是否显示炼药效果

    [Header("动画设置")]
    public Animator cauldronAnimator; // 炼药锅动画控制器
    public string idleAnimation = "Idle"; // 空闲动画
    public string alchemyAnimation = "Alchemy"; // 炼药动画

    [Header("状态")]
    public CauldronState currentState = CauldronState.Idle;
    private bool playerInRange = false; // 玩家是否在炼药锅范围内

    [Header("引用")]
    private AlchemySystem alchemySystem;
    private AlchemyUI alchemyUI;

    void Start()
    {
        InitializeReferences();
        InitializeVisuals();
    }

    void InitializeReferences()
    {
        alchemySystem = FindObjectOfType<AlchemySystem>();
        if (alchemySystem == null)
        {
            Debug.LogWarning("CauldronSystem: AlchemySystem not found!");
        }
        
        alchemyUI = FindObjectOfType<AlchemyUI>();
        if (alchemyUI == null)
        {
            Debug.LogWarning("CauldronSystem: AlchemyUI not found!");
        }
    }

    void InitializeVisuals()
    {
        // 初始状态为Idle，播放Idle动画
        PlayAnimation(idleAnimation);
    }

    public void InteractWithCauldron()
    {
        // 炼药锅交互逻辑由AlchemyUI处理
        Debug.Log("炼药锅被交互");
    }

    /// <summary>
    /// 播放炼药锅动画
    /// </summary>
    private void PlayAnimation(string animationName)
    {
        if (cauldronAnimator != null)
        {
            // 重置所有动画参数
            cauldronAnimator.SetBool("Idle", false);
            cauldronAnimator.SetBool("Alchemy", false);
            
            // 设置当前动画参数
            switch (animationName)
            {
                case "Idle":
                    cauldronAnimator.SetBool("Idle", true);
                    break;
                case "Alchemy":
                    cauldronAnimator.SetBool("Alchemy", true);
                    break;
            }
        }
    }

    /// <summary>
    /// 当玩家进入炼药锅碰撞范围时
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("玩家进入炼药锅范围");
            // 玩家进入范围时，播放炼金动画
            currentState = CauldronState.Alchemy;
            PlayAnimation(alchemyAnimation);
            
            // 通知AlchemyUI玩家进入范围
            if (alchemyUI != null)
            {
                alchemyUI.SetPlayerInCauldronRange(true);
            }
        }
    }

    /// <summary>
    /// 当玩家离开炼药锅碰撞范围时
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("玩家离开炼药锅范围");
            // 玩家离开范围时，播放默认动画
            currentState = CauldronState.Idle;
            PlayAnimation(idleAnimation);
            
            // 通知AlchemyUI玩家离开范围
            if (alchemyUI != null)
            {
                alchemyUI.SetPlayerInCauldronRange(false);
                // 如果炼金面板打开，关闭它
                if (alchemyUI.IsAlchemyPanelOpen())
                {
                    alchemyUI.HideAlchemyPanel();
                }
            }
        }
    }

    public bool IsCauldronUsable()
    {
        // 炼药锅在任何状态下都可以交互
        return true;
    }

    public void SetCauldronState(bool active)
    {
        currentState = CauldronState.Idle;
        // 播放Idle动画
        PlayAnimation(idleAnimation);
    }

    public void ForceReset()
    {
        currentState = CauldronState.Idle;
        // 播放Idle动画
        PlayAnimation(idleAnimation);
        Debug.Log("炼药锅已强制重置");
    }

    // 调试方法
    public void DebugPrintCauldronStatus()
    {
        Debug.Log("=== 炼药锅状态 ===");
        Debug.Log($"当前状态：{currentState}");
        Debug.Log($"可用状态：{IsCauldronUsable()}");
        Debug.Log($"玩家在范围内：{playerInRange}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsCauldronUsable() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}