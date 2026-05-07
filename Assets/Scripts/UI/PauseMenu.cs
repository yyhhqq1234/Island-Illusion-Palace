using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单管理器 - 负责处理游戏暂停和恢复
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("暂停菜单配置")]
    [Tooltip("是否在游戏启动时自动隐藏暂停菜单")]
    public bool hideOnStart = true;
    
    [Tooltip("是否使用ESC键切换暂停状态")]
    public bool useESCKey = true;
    
    [Tooltip("暂停时是否调暗游戏界面")]
    public bool dimScreen = true;
    
    [Tooltip("屏幕调暗透明度")]
    [Range(0f, 1f)]
    public float dimOpacity = 0.7f;

    [Header("UI组件")]
    [Tooltip("暂停菜单面板")]
    public GameObject pausePanel;
    
    [Tooltip("屏幕调暗遮罩")]
    public Image dimMask;

    private bool isPaused = false;
    private CanvasGroup pausePanelCanvasGroup;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化UI组件
        InitializeUI();
        
        // 启动时隐藏暂停菜单
        if (hideOnStart)
        {
            HidePauseMenu();
        }
    }

    void Update()
    {
        // 检测ESC键按下
        if (useESCKey && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    void InitializeUI()
    {
        // 初始化暂停面板
        if (pausePanel != null)
        {
            pausePanelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (pausePanelCanvasGroup == null)
            {
                pausePanelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
        }
        
        // 初始化屏幕调暗遮罩
        if (dimScreen && dimMask != null)
        {
            dimMask.color = new Color(0f, 0f, 0f, dimOpacity);
            dimMask.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        // 显示暂停菜单
        ShowPauseMenu();
        
        // 调暗游戏界面
        if (dimScreen && dimMask != null)
        {
            dimMask.gameObject.SetActive(true);
        }
        
        Debug.Log("游戏已暂停");
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        // 隐藏暂停菜单
        HidePauseMenu();
        
        // 取消屏幕调暗
        if (dimScreen && dimMask != null)
        {
            dimMask.gameObject.SetActive(false);
        }
        
        Debug.Log("游戏已恢复");
    }

    /// <summary>
    /// 显示暂停菜单
    /// </summary>
    void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            if (pausePanelCanvasGroup != null)
            {
                pausePanelCanvasGroup.alpha = 1f;
                pausePanelCanvasGroup.interactable = true;
                pausePanelCanvasGroup.blocksRaycasts = true;
            }
        }
    }

    /// <summary>
    /// 隐藏暂停菜单
    /// </summary>
    void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            if (pausePanelCanvasGroup != null)
            {
                pausePanelCanvasGroup.alpha = 0f;
                pausePanelCanvasGroup.interactable = false;
                pausePanelCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    /// <summary>
    /// 获取当前暂停状态
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
}