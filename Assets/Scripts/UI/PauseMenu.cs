using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using IIPUI;

/// <summary>
/// 暂停菜单管理器 - 负责处理游戏暂停和恢复。
/// 作为 Prefab 直接放置在玩法场景（Forest）中，随场景加载/卸载。
/// 不再使用 DontDestroyOnLoad，不再靠场景名自禁用。
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

    [Header("暂停菜单按钮")]
    [Tooltip("继续游戏按钮")]
    public Button resumeButton;

    [Tooltip("设置按钮")]
    public Button settingsButton;

    [Tooltip("返回主菜单按钮")]
    public Button mainMenuButton;

    [Tooltip("退出游戏按钮")]
    public Button quitButton;

    [Header("确认对话框")]
    [Tooltip("确认对话框面板")]
    public GameObject confirmDialogPanel;

    [Tooltip("对话框文本")]
    public Text dialogText;

    [Tooltip("确认按钮")]
    public Button confirmButton;

    [Tooltip("取消按钮")]
    public Button cancelButton;

    [Header("设置面板")]
    [Tooltip("PauseMenu 内部自带的设置面板子节点（默认隐藏）。若为空则点击设置按钮无效。")]
    public GameObject settingsPanel;

    private SettingsPanelController settingsPanelController;
    private bool settingsPanelOpen = false;

    private bool isPaused = false;
    private CanvasGroup pausePanelCanvasGroup;
    private CanvasGroup dimMaskCanvasGroup;
    private CanvasGroup confirmDialogCanvasGroup;
    private Coroutine currentAnimation;
    private ConfirmAction pendingAction;

    // 确认动作类型
    private enum ConfirmAction
    {
        MainMenu,
        QuitGame
    }

    void Awake()
    {
        // 单例模式（不 DontDestroyOnLoad，随 Forest 场景生命周期存在）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 初始化UI组件
        InitializeUI();

        // 初始化按钮事件
        InitializeButtons();

        // 缓存设置面板控制器并绑定返回回调
        if (settingsPanel != null)
        {
            settingsPanelController = settingsPanel.GetComponent<SettingsPanelController>();
            if (settingsPanelController != null)
            {
                settingsPanelController.onBackToPauseMenu.AddListener(CloseSettingsPanel);
            }
            settingsPanel.SetActive(false);
        }

        // 启动时隐藏暂停菜单
        if (hideOnStart)
        {
            HidePauseMenuImmediate();
        }

        // 字体兜底：确保暂停面板、确认框、设置面板内的中文都正常显示（根治"操作"等乱码）
        IIPUIFont.ApplyTo(transform);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // 检测ESC键按下
        if (useESCKey && Input.GetKeyDown(KeyCode.Escape))
        {
            // 优先级1：如果确认对话框打开，ESC关闭对话框
            if (confirmDialogPanel != null && confirmDialogPanel.activeSelf)
            {
                HideConfirmDialog();
            }
            // 优先级2：如果设置面板打开，ESC关闭设置面板（返回暂停菜单）
            else if (settingsPanelOpen)
            {
                CloseSettingsPanel();
                Debug.Log("[PauseMenu] ESC关闭设置面板，返回暂停菜单");
            }
            // 优先级3：否则切换暂停状态
            else
            {
                TogglePause();
            }
        }
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    void InitializeUI()
    {
        // 初始化暂停面板CanvasGroup
        if (pausePanel != null)
        {
            pausePanelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (pausePanelCanvasGroup == null)
            {
                pausePanelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
            
            // 确保暂停面板布局符合规范：450×550 紧凑居中
            RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // 设置锚点为屏幕中心
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 设置固定尺寸 450×550
                panelRect.sizeDelta = new Vector2(450f, 550f);
                panelRect.anchoredPosition = Vector2.zero;
            }
            
            // 确保 Content 使用 VerticalLayoutGroup
            Transform contentTransform = pausePanel.transform.Find("Content");
            if (contentTransform != null)
            {
                VerticalLayoutGroup vlg = contentTransform.GetComponent<VerticalLayoutGroup>();
                if (vlg == null)
                {
                    vlg = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                vlg.spacing = 20f;
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.padding = new RectOffset(20, 20, 20, 20);
                
                // 添加 ContentSizeFitter
                ContentSizeFitter csf = contentTransform.GetComponent<ContentSizeFitter>();
                if (csf == null)
                {
                    csf = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
                }
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            
            // 确保按钮具有正确的 LayoutElement
            Button[] buttons = pausePanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                LayoutElement le = btn.GetComponent<LayoutElement>();
                if (le == null)
                {
                    le = btn.gameObject.AddComponent<LayoutElement>();
                }
                le.minHeight = 44f;
                le.preferredHeight = 50f;
            }
            
            // 确保标题具有正确的 LayoutElement
            Transform titleTransform = pausePanel.transform.Find("Title");
            if (titleTransform != null)
            {
                LayoutElement titleLE = titleTransform.GetComponent<LayoutElement>();
                if (titleLE == null)
                {
                    titleLE = titleTransform.gameObject.AddComponent<LayoutElement>();
                }
                titleLE.minHeight = 40f;
                titleLE.preferredHeight = 48f;
            }
        }
        
        // 初始化屏幕调暗遮罩CanvasGroup
        if (dimScreen && dimMask != null)
        {
            dimMaskCanvasGroup = dimMask.GetComponent<CanvasGroup>();
            if (dimMaskCanvasGroup == null)
            {
                dimMaskCanvasGroup = dimMask.gameObject.AddComponent<CanvasGroup>();
            }
            dimMask.color = new Color(0f, 0f, 0f, dimOpacity);
            
            // 确保遮罩覆盖全屏
            RectTransform maskRect = dimMask.GetComponent<RectTransform>();
            if (maskRect != null)
            {
                maskRect.anchorMin = Vector2.zero;
                maskRect.anchorMax = Vector2.one;
                maskRect.offsetMin = Vector2.zero;
                maskRect.offsetMax = Vector2.zero;
            }
        }
        
        // 初始化确认对话框CanvasGroup
        if (confirmDialogPanel != null)
        {
            confirmDialogCanvasGroup = confirmDialogPanel.GetComponent<CanvasGroup>();
            if (confirmDialogCanvasGroup == null)
            {
                confirmDialogCanvasGroup = confirmDialogPanel.AddComponent<CanvasGroup>();
            }
            confirmDialogPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 初始化按钮事件监听
    /// </summary>
    void InitializeButtons()
    {
        // 继续游戏按钮
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() =>
            {
                IIPBootstrap.Audio?.PlayClick();
                ResumeGame();
            });
        }
        
        // 设置按钮
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() =>
            {
                IIPBootstrap.Audio?.PlayClick();
                OnSettingsButtonClicked();
            });
        }
        
        // 返回主菜单按钮
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                IIPBootstrap.Audio?.PlayClick();
                ShowConfirmDialog(ConfirmAction.MainMenu);
            });
        }
        
        // 退出游戏按钮
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() =>
            {
                IIPBootstrap.Audio?.PlayClick();
                ShowConfirmDialog(ConfirmAction.QuitGame);
            });
        }
        
        // 确认对话框按钮
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() =>
            {
                IIPBootstrap.Audio?.PlayClick();
                OnConfirmAction();
            });
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(() =>
            {
                IIPBootstrap.Audio?.PlayClick();
                HideConfirmDialog();
            });
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
        
        // 触发暂停事件
        IIPBootstrap.Events?.TriggerGamePaused();
        
        // 显示暂停菜单（带动画）
        ShowPauseMenu();
        
        Debug.Log("[PauseMenu] 游戏已暂停");
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        // 触发恢复事件
        IIPBootstrap.Events?.TriggerGameResumed();
        
        // 隐藏暂停菜单（带动画）
        HidePauseMenu();
        
        Debug.Log("[PauseMenu] 游戏已恢复");
    }

    /// <summary>
    /// 显示暂停菜单（带动画）
    /// </summary>
    void ShowPauseMenu()
    {
        // 停止正在进行的动画
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        currentAnimation = StartCoroutine(ShowPauseMenuAnimation());
    }
    
    /// <summary>
    /// 显示暂停菜单动画协程
    /// </summary>
    IEnumerator ShowPauseMenuAnimation()
    {
        // 阶段1：屏幕调暗淡入（0.15s）
        if (dimScreen && dimMask != null)
        {
            dimMask.gameObject.SetActive(true);
            if (dimMaskCanvasGroup != null)
            {
                dimMaskCanvasGroup.alpha = 0f;
                float duration = 0.15f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    dimMaskCanvasGroup.alpha = Mathf.Lerp(0f, dimOpacity, elapsed / duration);
                    yield return null;
                }
                dimMaskCanvasGroup.alpha = dimOpacity;
            }
        }
        
        // 阶段2：暂停面板淡入+缩放（0.25s Back ease）
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            if (pausePanelCanvasGroup != null)
            {
                pausePanelCanvasGroup.alpha = 0f;
                pausePanelCanvasGroup.interactable = true;
                pausePanelCanvasGroup.blocksRaycasts = true;
                
                // 缩放动画：0.8 → 1.1 → 1 (Back ease)
                pausePanel.transform.localScale = Vector3.one * 0.8f;
                
                float duration = 0.15f;
                float elapsed = 0f;
                Vector3 startScale = Vector3.one * 0.8f;
                Vector3 midScale = Vector3.one * 1.1f;
                
                // 阶段2a: 0.8 → 1.1 (0.15s) - Back ease approximation
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / duration;
                    // Back ease approximation: 先超出目标值再回弹
                    float backT = t * t * (2.70158f * t - 1.70158f);
                    pausePanel.transform.localScale = Vector3.Lerp(startScale, midScale, backT);
                    pausePanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.25f);
                    yield return null;
                }
                pausePanel.transform.localScale = midScale;
                pausePanelCanvasGroup.alpha = 0.6f;
                
                // 阶段2b: 1.1 → 1 (0.1s)
                elapsed = 0f;
                duration = 0.1f;
                Vector3 endScale = Vector3.one;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    pausePanel.transform.localScale = Vector3.Lerp(midScale, endScale, elapsed / duration);
                    pausePanelCanvasGroup.alpha = Mathf.Lerp(0.6f, 1f, elapsed / duration);
                    yield return null;
                }
                pausePanel.transform.localScale = endScale;
                pausePanelCanvasGroup.alpha = 1f;
            }
        }
        
        currentAnimation = null;
    }

    /// <summary>
    /// 隐藏暂停菜单（带动画）
    /// </summary>
    void HidePauseMenu()
    {
        // 停止正在进行的动画
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        currentAnimation = StartCoroutine(HidePauseMenuAnimation());
    }
    
    /// <summary>
    /// 隐藏暂停菜单动画协程
    /// </summary>
    IEnumerator HidePauseMenuAnimation()
    {
        // 阶段1：暂停面板淡出+缩放（0.15s）
        if (pausePanel != null && pausePanelCanvasGroup != null)
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 startScale = pausePanel.transform.localScale;
            Vector3 endScale = Vector3.one * 0.9f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                pausePanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                pausePanel.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
                yield return null;
            }
            
            pausePanel.SetActive(false);
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
        }
        
        // 阶段2：屏幕调暗淡出（0.1s）
        if (dimScreen && dimMask != null && dimMaskCanvasGroup != null)
        {
            float duration = 0.1f;
            float elapsed = 0f;
            float startAlpha = dimMaskCanvasGroup.alpha;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                dimMaskCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }
            
            dimMask.gameObject.SetActive(false);
        }
        
        currentAnimation = null;
    }

    /// <summary>
    /// 立即隐藏暂停菜单（无动画）
    /// </summary>
    void HidePauseMenuImmediate()
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
        
        if (dimScreen && dimMask != null)
        {
            dimMask.gameObject.SetActive(false);
            if (dimMaskCanvasGroup != null)
            {
                dimMaskCanvasGroup.alpha = 0f;
            }
        }
        
        if (confirmDialogPanel != null)
        {
            confirmDialogPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 设置按钮点击处理 — 隐藏暂停菜单，显示设置面板
    /// </summary>
    void OnSettingsButtonClicked()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("[PauseMenu] 未绑定 settingsPanel，无法打开设置");
            return;
        }

        // 隐藏暂停面板和dimMask
        if (pausePanel != null) pausePanel.SetActive(false);
        if (dimMask != null) dimMask.gameObject.SetActive(false);

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
        settingsPanelOpen = true;

        Debug.Log("[PauseMenu] 打开设置面板");
    }

    /// <summary>
    /// 关闭设置面板，返回暂停菜单
    /// </summary>
    void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        settingsPanelOpen = false;

        // 恢复暂停面板和dimMask
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }
        if (dimScreen && dimMask != null)
        {
            dimMask.gameObject.SetActive(true);
            dimMask.transform.SetAsLastSibling();
            // dimMask 应该在 pausePanel 下面，pausePanel 在最上层
            if (pausePanel != null)
                pausePanel.transform.SetAsLastSibling();
        }

        Debug.Log("[PauseMenu] 关闭设置面板，返回暂停菜单");
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    void ShowConfirmDialog(ConfirmAction action)
    {
        pendingAction = action;
        
        // 设置对话框文本
        if (dialogText != null)
        {
            switch (action)
            {
                case ConfirmAction.MainMenu:
                    dialogText.text = "返回主菜单将丢失当前未保存的进度，确定继续吗？";
                    break;
                case ConfirmAction.QuitGame:
                    dialogText.text = "确定要退出游戏吗？";
                    break;
            }
        }
        
        // 显示确认对话框（带动画）
        if (confirmDialogPanel != null)
        {
            confirmDialogPanel.SetActive(true);
            confirmDialogPanel.transform.SetAsLastSibling(); // 确保显示在暂停面板之上
            
            if (confirmDialogCanvasGroup != null)
            {
                // 启动动画协程
                StartCoroutine(ShowConfirmDialogAnimation());
            }
        }
    }
    
    /// <summary>
    /// 显示确认对话框动画协程
    /// </summary>
    IEnumerator ShowConfirmDialogAnimation()
    {
        if (confirmDialogCanvasGroup != null)
        {
            confirmDialogCanvasGroup.alpha = 0f;
            confirmDialogCanvasGroup.interactable = true;
            confirmDialogCanvasGroup.blocksRaycasts = true;
            
            // 缩放动画：0.8 → 1.05 → 1 (Back ease)
            confirmDialogPanel.transform.localScale = Vector3.one * 0.8f;
            
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.8f;
            Vector3 midScale = Vector3.one * 1.05f;
            
            // 阶段a: 0.8 → 1.05 (0.15s) - Back ease
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float backT = t * t * (2.70158f * t - 1.70158f);
                confirmDialogPanel.transform.localScale = Vector3.Lerp(startScale, midScale, backT);
                confirmDialogCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.2f);
                yield return null;
            }
            confirmDialogPanel.transform.localScale = midScale;
            
            // 阶段b: 1.05 → 1 (0.05s)
            elapsed = 0f;
            duration = 0.05f;
            Vector3 endScale = Vector3.one;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                confirmDialogPanel.transform.localScale = Vector3.Lerp(midScale, endScale, elapsed / duration);
                yield return null;
            }
            confirmDialogPanel.transform.localScale = endScale;
            confirmDialogCanvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// 隐藏确认对话框
    /// </summary>
    void HideConfirmDialog()
    {
        // 启动动画协程
        StartCoroutine(HideConfirmDialogAnimation());
    }
    
    /// <summary>
    /// 隐藏确认对话框动画协程
    /// </summary>
    IEnumerator HideConfirmDialogAnimation()
    {
        if (confirmDialogPanel != null && confirmDialogCanvasGroup != null)
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 startScale = confirmDialogPanel.transform.localScale;
            Vector3 endScale = Vector3.one * 0.9f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                confirmDialogCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                confirmDialogPanel.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
                yield return null;
            }
            
            confirmDialogPanel.SetActive(false);
            confirmDialogCanvasGroup.interactable = false;
            confirmDialogCanvasGroup.blocksRaycasts = false;
        }
        
        pendingAction = ConfirmAction.MainMenu; // 重置
    }

    /// <summary>
    /// 确认动作执行
    /// </summary>
    void OnConfirmAction()
    {
        switch (pendingAction)
        {
            case ConfirmAction.MainMenu:
                // 恢复时间流速
                Time.timeScale = 1f;
                // 切换到主菜单场景
                IIPBootstrap.SwitchToScene(IIPConstants.SceneMainMenu);
                break;
                
            case ConfirmAction.QuitGame:
                // 退出游戏
                IIPBootstrap.Events?.TriggerGameQuit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
        
        // 关闭确认对话框
        StartCoroutine(HideConfirmDialogAfterAction());
    }
    
    /// <summary>
    /// 执行动作后关闭确认对话框（带动画）
    /// </summary>
    IEnumerator HideConfirmDialogAfterAction()
    {
        yield return StartCoroutine(HideConfirmDialogAnimation());
    }

    /// <summary>
    /// 获取当前暂停状态
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
}