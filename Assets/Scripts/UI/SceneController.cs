using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [Header("主菜单按钮")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("面板引用")]
    [Tooltip("设置面板（默认隐藏）")]
    [SerializeField] private GameObject settingsPanel;
    [Tooltip("设置面板返回按钮")]
    [SerializeField] private Button settingsBackButton;
    [Tooltip("退出确认面板（默认隐藏）")]
    [SerializeField] private GameObject quitConfirmPanel;
    [Tooltip("确认退出按钮")]
    [SerializeField] private Button confirmQuitButton;
    [Tooltip("取消退出按钮")]
    [SerializeField] private Button cancelQuitButton;

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartGame);
            startButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueGame);
            continueButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnOpenSettings);
            settingsButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitRequested);
            quitButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }

        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(OnCloseSettings);
            settingsBackButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }

        if (confirmQuitButton != null)
        {
            confirmQuitButton.onClick.AddListener(OnConfirmQuit);
            confirmQuitButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }
        if (cancelQuitButton != null)
        {
            cancelQuitButton.onClick.AddListener(OnCancelQuit);
            cancelQuitButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);
    }

    /// <summary>新游戏 — 进入碎片</summary>
    public void OnStartGame()
    {
        IIPBootstrap.SwitchToScene(IIPConstants.SceneGamePlay);
    }

    /// <summary>继续游戏 — 从上次营火处继续</summary>
    public void OnContinueGame()
    {
        var saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.LoadGame(1);
            IIPBootstrap.SwitchToScene(IIPConstants.SceneGamePlay);
        }
        else
        {
            Debug.LogWarning("[SceneController] SaveSystem未找到，直接进入游戏");
            IIPBootstrap.SwitchToScene(IIPConstants.SceneGamePlay);
        }
    }

    /// <summary>打开设置面板</summary>
    public void OnOpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    /// <summary>关闭设置面板</summary>
    public void OnCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>请求退出 — 显示确认面板</summary>
    public void OnQuitRequested()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(true);
        else
            OnConfirmQuit();
    }

    /// <summary>确认退出 — 真正退出游戏</summary>
    public void OnConfirmQuit()
    {
        IIPBootstrap.Events.TriggerGameQuit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>取消退出 — 隐藏确认面板</summary>
    public void OnCancelQuit()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);
    }
}
