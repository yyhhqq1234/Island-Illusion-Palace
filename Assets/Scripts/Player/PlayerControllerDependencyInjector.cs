using UnityEngine;

/// <summary>
/// 依赖注入器 - 负责查找和注入PlayerController的依赖项
/// 消除PlayerController中的FindObjectOfType运行时查找
/// </summary>
public class PlayerControllerDependencyInjector : MonoBehaviour
{
    [Header("注入目标")]
    [Tooltip("目标PlayerController（为空时自动查找）")]
    public PlayerController targetController;

    [Header("依赖项（可选，手动指定时优先使用）")]
    public PlayerSpawnManager manualSpawnManager;
    public InventoryUI manualInventoryUI;
    public AlchemyUI manualAlchemyUI;
    public PauseMenu manualPauseMenu;

    void Start()
    {
        if (targetController == null)
        {
            targetController = FindObjectOfType<PlayerController>();
        }

        if (targetController != null)
        {
            InjectDependencies();
        }
        else
        {
            Debug.LogWarning("[PlayerControllerDependencyInjector] 未找到PlayerController目标");
        }
    }

    /// <summary>
    /// 执行依赖注入：优先使用手动指定的依赖，否则自动查找
    /// </summary>
    public void InjectDependencies()
    {
        PlayerSpawnManager spawnMgr = manualSpawnManager != null ? manualSpawnManager : FindObjectOfType<PlayerSpawnManager>();
        InventoryUI invUI = manualInventoryUI != null ? manualInventoryUI : FindObjectOfType<InventoryUI>();
        AlchemyUI alcUI = manualAlchemyUI != null ? manualAlchemyUI : FindObjectOfType<AlchemyUI>();
        PauseMenu pm = manualPauseMenu != null ? manualPauseMenu : FindObjectOfType<PauseMenu>();

        targetController.SetDependencies(spawnMgr, invUI, alcUI, pm);

        Debug.Log("[PlayerControllerDependencyInjector] 依赖注入完成");
    }

    /// <summary>
    /// 静态方法 - 用于代码中直接调用注入（降级场景）
    /// </summary>
    public static void InjectToController(PlayerController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("[PlayerControllerDependencyInjector] 控制器为空，无法注入");
            return;
        }

        PlayerSpawnManager spawnMgr = FindObjectOfType<PlayerSpawnManager>();
        InventoryUI invUI = FindObjectOfType<InventoryUI>();
        AlchemyUI alcUI = FindObjectOfType<AlchemyUI>();
        PauseMenu pm = FindObjectOfType<PauseMenu>();

        controller.SetDependencies(spawnMgr, invUI, alcUI, pm);
    }
}
