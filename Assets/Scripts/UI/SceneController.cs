using UnityEngine;
using UnityEngine.UI;
using System.IO;
using IIPUI;

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
        // 字体兜底：主菜单按钮/退出确认面板的 Text 依赖场景内嵌的序列化字体，
        // 运行时统一切到 IIPUIFont（内置字体优先，回退 OS 雅黑），消除 OS 字体依赖
        ApplyUIFontFallback();

        // 视觉统一：主菜单按钮套用工厂原语（深底圆角+紫辉光 hover+雅黑），消除样式不一致
        IIPUIFactory.StyleButton(startButton);
        IIPUIFactory.StyleButton(continueButton);
        IIPUIFactory.StyleButton(settingsButton);
        IIPUIFactory.StyleButton(quitButton);

        // 退出确认框布局重建（DialogCard 560×320 居中，按钮收进卡片内）
        RebuildQuitDialog();

        // 无存档时「继续」按钮置灰不可点
        UpdateContinueButtonState();

        // 右下角版本号 footer
        EnsureVersionFooter();

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

    /// <summary>
    /// 退出确认框布局重建：DialogCard 560×320 居中，确认/取消按钮收进卡片底部水平居中。
    /// （旧版 ButtonContainer 偏移为负，按钮飞出卡片分居屏幕两侧）
    /// </summary>
    void RebuildQuitDialog()
    {
        if (quitConfirmPanel == null) return;

        // 根：全屏暗化遮罩，无圆角 sprite
        var rootRt = quitConfirmPanel.transform as RectTransform;
        if (rootRt != null)
        {
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
        }
        var rootImg = quitConfirmPanel.GetComponent<Image>();
        if (rootImg != null)
        {
            rootImg.sprite = null;
            rootImg.type = Image.Type.Simple;
            rootImg.color = new Color(0f, 0f, 0f, 0.75f);
        }

        var card = quitConfirmPanel.transform.Find("DialogCard");
        if (card == null) return;
        var cardRt = (RectTransform)card;
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(560f, 320f);
        cardRt.anchoredPosition = Vector2.zero;
        IIPUIFactory.StylePanelRoot(card.gameObject); // 深底圆角 + 低调边框

        // 标题：顶部居中
        var title = card.Find("TitleText");
        if (title != null)
        {
            var rt = (RectTransform)title;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0f, -76f);
            rt.offsetMax = new Vector2(0f, -16f);
            var txt = title.GetComponent<Text>();
            if (txt != null)
            {
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = IIPUIStyle.TextTitle;
                txt.fontSize = IIPUIStyle.FontSizeTitle;
            }
        }

        // 正文：中部
        var msg = card.Find("MessageText");
        if (msg != null)
        {
            var rt = (RectTransform)msg;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(40f, -30f);
            rt.offsetMax = new Vector2(-40f, 40f);
            var txt = msg.GetComponent<Text>();
            if (txt != null)
            {
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = IIPUIStyle.TextPrimary;
            }
        }

        // 按钮容器：卡片底部内部，两按钮水平居中
        var btnContainer = card.Find("ButtonContainer");
        if (btnContainer != null)
        {
            var bcRt = (RectTransform)btnContainer;
            bcRt.anchorMin = new Vector2(0.5f, 0f);
            bcRt.anchorMax = new Vector2(0.5f, 0f);
            bcRt.pivot = new Vector2(0.5f, 0f);
            bcRt.sizeDelta = new Vector2(480f, 52f);
            bcRt.anchoredPosition = new Vector2(0f, 26f);

            var hlg = btnContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = btnContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 32f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            foreach (var btn in new[] { confirmQuitButton, cancelQuitButton })
            {
                if (btn == null) continue;
                var brt = (RectTransform)btn.transform;
                brt.anchorMin = new Vector2(0.5f, 0.5f);
                brt.anchorMax = new Vector2(0.5f, 0.5f);
                brt.sizeDelta = new Vector2(200f, 52f);
                var le = btn.GetComponent<LayoutElement>();
                if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
                le.minWidth = 200f;
                le.preferredWidth = 200f;
                le.minHeight = 52f;
                le.preferredHeight = 52f;
                IIPUIFactory.StyleButton(btn);
            }
        }
    }

    /// <summary>无存档时「继续」按钮置灰不可点（检测 SaveSystem 存档存在性）。</summary>
    void UpdateContinueButtonState()
    {
        if (continueButton == null) return;

        bool hasSave;
        var saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            hasSave = saveSystem.SaveSlotExists(1);
        }
        else
        {
            // 主菜单场景通常没有 SaveSystem 实例，按既定存档路径直接探测文件
            string savePath = Path.Combine(Application.persistentDataPath, "Saves", "save_1.dat");
            hasSave = File.Exists(savePath);
        }

        if (!hasSave)
        {
            continueButton.interactable = false;
            // 置灰视觉：压暗底色 + 文字弱化 + 禁用 hover 反馈
            var img = continueButton.targetGraphic as Image;
            if (img != null) img.color = IIPUIStyle.SlotBackgroundEmpty;
            var label = continueButton.GetComponentInChildren<Text>(true);
            if (label != null) label.color = IIPUIStyle.TextSecondary;
            var hb = continueButton.GetComponent<HoverButton>();
            if (hb != null) hb.enabled = false;
            Debug.Log("[SceneController] 未检测到存档，「继续」按钮已置灰");
        }
    }

    /// <summary>右下角版本号 footer（小字号，幂等创建）。</summary>
    void EnsureVersionFooter()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        if (canvas.transform.Find("VersionFooter") != null) return; // 两个 SceneController 实例防重复
        IIPUIFactory.CreateLabelAnchored("VersionFooter", canvas.transform,
            "v0.1.0 Alpha", IIPUIStyle.FontSizeSmall, IIPUIStyle.TextSecondary,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-160f, 8f), new Vector2(-12f, 28f), TextAnchor.MiddleRight);
    }

    /// <summary>对本组件引用的按钮/面板子树统一应用菜单字体。</summary>
    void ApplyUIFontFallback()
    {
        if (startButton != null) IIPUI.IIPUIFont.ApplyTo(startButton.transform);
        if (continueButton != null) IIPUI.IIPUIFont.ApplyTo(continueButton.transform);
        if (settingsButton != null) IIPUI.IIPUIFont.ApplyTo(settingsButton.transform);
        if (quitButton != null) IIPUI.IIPUIFont.ApplyTo(quitButton.transform);
        if (quitConfirmPanel != null) IIPUI.IIPUIFont.ApplyTo(quitConfirmPanel.transform);
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
