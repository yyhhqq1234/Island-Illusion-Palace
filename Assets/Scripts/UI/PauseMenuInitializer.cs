using UnityEngine;

/// <summary>
/// 暂停菜单初始化器 - 确保暂停菜单在场景中存在并正常工作
/// </summary>
public class PauseMenuInitializer : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("暂停菜单预制件（可选，如果为空会查找场景中的PauseMenu）")]
    public GameObject pauseMenuPrefab;

    [Tooltip("是否在Awake时自动初始化")]
    public bool autoInitialize = true;

    private static PauseMenuInitializer _instance;

    void Awake()
    {
        if (autoInitialize)
        {
            InitializePauseMenu();
        }
    }

    /// <summary>
    /// 初始化暂停菜单
    /// </summary>
    public void InitializePauseMenu()
    {
        // 检查是否已存在PauseMenu实例
        PauseMenu existingPauseMenu = FindObjectOfType<PauseMenu>();
        
        if (existingPauseMenu != null)
        {
            Debug.Log("[PauseMenuInitializer] 找到现有的PauseMenu");
            
            // 如果这是重复的初始化器，销毁自己
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        // 尝试从预制件创建
        if (pauseMenuPrefab != null)
        {
            GameObject pauseMenuObj = Instantiate(pauseMenuPrefab);
            pauseMenuObj.name = "PauseMenu";
            DontDestroyOnLoad(pauseMenuObj);
            Debug.Log("[PauseMenuInitializer] 从预制件创建了PauseMenu");
        }
        else
        {
            Debug.LogWarning("[PauseMenuInitializer] 没有找到PauseMenu，也没有配置预制件！");
            Debug.LogWarning("[PauseMenuInitializer] 请在Unity编辑器中使用 GameObject > IIP > Create Compact Pause Menu 创建暂停菜单");
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 手动触发暂停菜单（用于测试）
    /// </summary>
    [ContextMenu("Toggle Pause")]
    public void TogglePause()
    {
        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.TogglePause();
        }
        else
        {
            Debug.LogWarning("[PauseMenuInitializer] 没有找到PauseMenu组件");
        }
    }
}
