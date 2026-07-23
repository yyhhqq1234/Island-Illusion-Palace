using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IIPUI;

/// <summary>
/// 设置面板控制器 - 绑定场景中手工搭建的设置面板节点，只负责逻辑与持久化。
/// 面板结构（由场景/Prefab 提供）：
///   TabContainer > Tab_0_AudioVideo / Tab_1_Controls / Tab_2_Assist
///   ContentArea  > AudioVideoContent / ControlsContent / AssistContent
///   BackButton
/// 支持音画/操作/辅助三个Tab，设置项实际生效 + PlayerPrefs持久化。
/// 不再运行时动态生成 UI（旧版会与场景手搭版冲突并触发 NRE）。
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    // ══════════════════════════════════════════
    // 回调
    // ═══════════════════════════════════════════

    [Header("回调")]
    [Tooltip("返回暂停菜单/主菜单时的回调")]
    public UnityEngine.Events.UnityEvent onBackToPauseMenu;

    // ═══════════════════════════════════════════
    // 场景节点引用（必须在 Inspector 绑定）
    // ═══════════════════════════════════════════

    [Header("Tab按钮")]
    [Tooltip("音画Tab按钮")]
    public Button tabAudioVideo;
    [Tooltip("操作Tab按钮")]
    public Button tabControls;
    [Tooltip("辅助Tab按钮")]
    public Button tabAssist;

    [Header("Tab内容面板")]
    [Tooltip("音画内容面板")]
    public GameObject contentAudioVideo;
    [Tooltip("操作内容面板")]
    public GameObject contentControls;
    [Tooltip("辅助内容面板")]
    public GameObject contentAssist;

    [Header("音画Tab - 音量滑块")]
    public Slider sliderMaster;
    public Slider sliderSFX;
    public Slider sliderMusic;
    public Slider sliderUI;
    [Tooltip("各音量滑块右侧的百分比文本（可空）")]
    public Text labelMaster;
    public Text labelSFX;
    public Text labelMusic;
    public Text labelUI;

    [Header("辅助Tab - 开关")]
    public Toggle toggleDamageNumber;
    public Toggle toggleMinimap;
    public Toggle toggleAutoPickup;
    public Toggle toggleColorblind;

    [Header("返回按钮")]
    public Button backButton;

    // ═══════════════════════════════════════════
    // 主题色（与 HoverButton / 暂停菜单统一）
    // ═══════════════════════════════════════════

    private static readonly Color TAB_ACTIVE_COLOR = IIPUI.IIPUIStyle.SettingsTabActive;
    private static readonly Color TAB_INACTIVE_COLOR = IIPUI.IIPUIStyle.SettingsTabInactive;

    // ═══════════════════════════════════════════
    // 持久化键名
    // ═══════════════════════════════════════════

    private const string KEY_MASTER_VOL = "IIP_MasterVolume";
    private const string KEY_SFX_VOL = "IIP_SFXVolume";
    private const string KEY_MUSIC_VOL = "IIP_MusicVolume";
    private const string KEY_UI_VOL = "IIP_UIVolume";
    private const string KEY_DAMAGE_NUM = IIPConstants.PrefKeyShowDamageNumbers;
    private const string KEY_MINIMAP = "IIP_ShowMinimap";
    private const string KEY_AUTO_PICKUP = IIPConstants.PrefKeyAutoPickup;
    private const string KEY_COLORBLIND = "IIP_ColorblindMode";

    private string currentTab = "AudioVideo";

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    void Awake()
    {
        BindControls();
        // 字体兜底：确保场景未跑编辑器打磨脚本时，中文也能正常显示（根治"操作"等乱码）
        IIPUI.IIPUIFont.ApplyTo(transform);
        // 视觉统一（Phase 2）：场景手搭的旧样式 → 工厂原语（深底圆角+紫辉光+雅黑）
        RestyleUI();
        // 布局重建（Phase 3）：统一卡片尺寸/滑块方向/滚动区/勾形开关，
        // 对主菜单场景实例与暂停菜单预制体实例都生效（两处都是本组件，Awake 各自执行）
        RebuildLayout();
    }

    // ═══════════════════════════════════════════
    // Phase 2 视觉统一（只改视觉：sprite/颜色/hover，不动布局与事件）
    // ═══════════════════════════════════════════

    void RestyleUI()
    {
        IIPUIFactory.StylePanelRoot(gameObject);

        // Tab 按钮 + 返回按钮（Tab 颜色由 SetTabColor 每帧语义维护，这里只刷形状/hover/字体）
        StyleTabButton(tabAudioVideo);
        StyleTabButton(tabControls);
        StyleTabButton(tabAssist);
        IIPUIFactory.StyleButton(backButton);

        // 滑块：底槽中性灰 + 紫色填充 + 亮色手柄
        StyleSlider(sliderMaster);
        StyleSlider(sliderSFX);
        StyleSlider(sliderMusic);
        StyleSlider(sliderUI);

        // 开关：底槽深底圆角 + 紫色勾选
        StyleToggle(toggleDamageNumber);
        StyleToggle(toggleMinimap);
        StyleToggle(toggleAutoPickup);
        StyleToggle(toggleColorblind);
    }

    void StyleTabButton(Button tab)
    {
        if (tab == null) return;
        var img = tab.targetGraphic as Image;
        if (img == null) img = tab.GetComponent<Image>();
        if (img != null) IIPUIFactory.ApplyRounded(img, true); // 颜色由 SetTabColor 管理
        tab.transition = Selectable.Transition.None;
        IIPUIFactory.ApplyHover(tab.gameObject, IIPUIStyle.ButtonHover);
        var label = tab.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.font = IIPUI.IIPUIFont.Get();
            label.color = IIPUIStyle.TextPrimary;
            label.fontSize = IIPUIStyle.FontSizeButton;
        }
    }

    void StyleSlider(Slider slider)
    {
        if (slider == null) return;
        var bg = slider.transform.Find("Background")?.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = IIPUIStyle.BarBackgroundNeutral;
            IIPUIFactory.ApplyRounded(bg, true);
        }
        var fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        if (fill != null)
        {
            fill.color = IIPUIStyle.AccentPurple;
            IIPUIFactory.ApplyRounded(fill, true);
        }
        var handle = slider.handleRect != null ? slider.handleRect.GetComponent<Image>() : null;
        if (handle != null)
        {
            handle.color = IIPUIStyle.BorderBright;
            IIPUIFactory.ApplyRounded(handle, true);
        }
    }

    void StyleToggle(Toggle toggle)
    {
        if (toggle == null) return;
        var check = toggle.graphic as Image; // Checkmark
        if (check != null)
        {
            check.color = IIPUIStyle.AccentPurple;
            IIPUIFactory.ApplyCheckmark(check); // 勾形勾选，替代实心方块
        }
        var bgTrans = toggle.targetGraphic as Image; // Background
        if (bgTrans != null)
        {
            bgTrans.color = IIPUIStyle.SlotBackground;
            IIPUIFactory.ApplyRounded(bgTrans, true);
        }
        var label = toggle.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.font = IIPUI.IIPUIFont.Get();
            label.color = IIPUIStyle.TextPrimary;
        }
    }

    // ═══════════════════════════════════════════
    // Phase 3 布局重建（只改 RectTransform/布局组件，不动字段绑定与事件）
    // 兼容两种节点树：
    //   A. 主菜单场景版：SettingsPanel > SettingsCard > (TitleText/TabContainer/ContentArea/BackButton)
    //   B. 暂停菜单预制体版：SettingsPanel > (TitleText/TabContainer/ContentArea/BackButton)（扁平，无卡片）
    // ═══════════════════════════════════════════

    void RebuildLayout()
    {
        // ── 1. 根节点：还原为全屏暗色遮罩（去圆角 sprite，消除四角锯齿；去旧 VLG）──
        var rootRt = transform as RectTransform;
        if (rootRt != null)
        {
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
        }
        var rootImg = GetComponent<Image>();
        if (rootImg != null)
        {
            rootImg.sprite = null;
            rootImg.type = Image.Type.Simple;
            rootImg.color = new Color(0.04f, 0.04f, 0.08f, 0.92f);
            rootImg.raycastTarget = true; // 挡住背后 UI 的点击
        }
        // RestyleUI 可能给根加了圆角边框节点，全屏遮罩不需要，隐藏
        var rootBorder = transform.Find("IIPBorder");
        if (rootBorder != null) rootBorder.gameObject.SetActive(false);
        // 预制体旧版根上挂了 VerticalLayoutGroup，会把子节点顶散，禁用
        var rootVlg = GetComponent<VerticalLayoutGroup>();
        if (rootVlg != null) rootVlg.enabled = false;

        // ── 2. SettingsCard：900×640 居中卡片（扁平结构先收编出卡片）──
        Transform card = transform.Find("SettingsCard");
        if (card == null)
        {
            var cardGo = new GameObject("SettingsCard", typeof(RectTransform), typeof(Image));
            card = cardGo.transform;
            card.SetParent(transform, false);
            // 按展示顺序收编旧的直接子节点
            string[] childNames = { "TitleText", "TabContainer", "ContentArea", "BackButton" };
            foreach (var name in childNames)
            {
                var child = transform.Find(name);
                if (child != null)
                {
                    child.SetParent(card, false);
                    // 旧节点上的 LayoutElement 是配合根 VLG 的，卡片内不再需要
                    var le = child.GetComponent<LayoutElement>();
                    if (le != null) Destroy(le);
                }
            }
        }
        var cardRt = card as RectTransform;
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(900f, 640f);
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.localScale = Vector3.one;
        IIPUIFactory.StylePanelRoot(card.gameObject); // 深底圆角 + 低调边框

        // ── 3. 卡片内部四区布局 ──
        // 标题：顶部通栏，高 56
        var title = card.Find("TitleText");
        if (title != null)
        {
            var rt = title as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0f, -64f);
            rt.offsetMax = new Vector2(0f, -12f);
            var txt = title.GetComponent<Text>();
            if (txt != null) txt.alignment = TextAnchor.MiddleCenter;
        }
        // Tab 容器：标题下方，高 48，三按钮均分
        var tabContainer = card.Find("TabContainer");
        if (tabContainer != null)
        {
            var rt = tabContainer as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(24f, -124f);
            rt.offsetMax = new Vector2(-24f, -76f);
            var hlg = tabContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = tabContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;   // 三 Tab 均分宽度
            hlg.childForceExpandHeight = true;
            var tabLe = tabContainer.GetComponent<LayoutElement>();
            if (tabLe != null) Destroy(tabLe); // 预制体旧版残留
            // Tab 按钮布局参数：等宽
            foreach (var tab in new[] { tabAudioVideo, tabControls, tabAssist })
            {
                if (tab == null) continue;
                var le = tab.GetComponent<LayoutElement>();
                if (le == null) le = tab.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.preferredWidth = -1f;
                le.minHeight = 0f;
                le.preferredHeight = -1f;
            }
        }
        // 返回按钮：底部居中 280×50
        if (backButton != null)
        {
            var rt = backButton.transform as RectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(280f, 50f);
            rt.anchoredPosition = new Vector2(0f, 22f);
            var le = backButton.GetComponent<LayoutElement>();
            if (le != null) Destroy(le);
        }
        // ESC 返回提示（卡片右下角灰字，幂等）
        if (card.Find("EscHint") == null)
        {
            IIPUIFactory.CreateLabelAnchored("EscHint", card, "ESC 返回",
                IIPUIStyle.FontSizeSmall, IIPUIStyle.TextSecondary,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-140f, 26f), new Vector2(-16f, 58f), TextAnchor.MiddleRight);
        }
        // 内容区：Tab 与返回按钮之间，四边留 24
        Transform contentArea = card.Find("ContentArea");
        if (contentArea != null)
        {
            var rt = contentArea as RectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(24f, 88f);
            rt.offsetMax = new Vector2(-24f, -140f);
            var areaLe = contentArea.GetComponent<LayoutElement>();
            if (areaLe != null) Destroy(areaLe);
            // 预制体旧版 ContentArea 上直接挂了 ScrollRect（无 Viewport 结构），移除改由逐页包装
            var oldSr = contentArea.GetComponent<ScrollRect>();
            if (oldSr != null) Destroy(oldSr);
            var areaImg = contentArea.GetComponent<Image>();
            if (areaImg != null)
            {
                areaImg.color = IIPUIStyle.ContentBackground;
                IIPUIFactory.ApplyRounded(areaImg, true);
                areaImg.raycastTarget = true; // ScrollRect 拖拽需要
            }
        }

        // ── 4. 三个内容页：滚动包装 + 行布局 ──
        SetupScrollContent(contentAudioVideo, contentArea);
        SetupScrollContent(contentControls, contentArea);
        SetupScrollContent(contentAssist, contentArea);

        // ── 5. 滑块方向强制水平（主菜单旧版曾渲染成竖条）──
        foreach (var s in new[] { sliderMaster, sliderSFX, sliderMusic, sliderUI })
            if (s != null) s.direction = Slider.Direction.LeftToRight;

        // ── 6. 隐藏色盲模式行（功能未实装，避免误导）──
        if (toggleColorblind != null && toggleColorblind.transform.parent != null)
            toggleColorblind.transform.parent.gameObject.SetActive(false);

        Debug.Log("[SettingsPanel] RebuildLayout 完成（卡片 900×640 居中 + 滚动内容区 + 勾形开关）");
    }

    /// <summary>
    /// 把内容页包装进 ScrollRect（ContentArea/Scroll_X/Viewport/Content），
    /// 内容超出可视高度时可滚动（操作Tab 7行按键不再溢出卡片）。
    /// 幂等：重复调用时复用已建包装。
    /// </summary>
    void SetupScrollContent(GameObject content, Transform contentArea)
    {
        if (content == null || contentArea == null) return;

        string wrapperName = "Scroll_" + content.name;
        Transform wrapper = contentArea.Find(wrapperName);
        RectTransform viewportRt;
        if (wrapper == null)
        {
            // 包装层：铺满内容区
            var wrapperGo = new GameObject(wrapperName, typeof(RectTransform), typeof(ScrollRect));
            wrapper = wrapperGo.transform;
            wrapper.SetParent(contentArea, false);
            var wrapperRt = (RectTransform)wrapper;
            wrapperRt.anchorMin = Vector2.zero;
            wrapperRt.anchorMax = Vector2.one;
            wrapperRt.offsetMin = Vector2.zero;
            wrapperRt.offsetMax = Vector2.zero;

            // 视口：铺满包装层，负责裁剪
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportRt = (RectTransform)viewportGo.transform;
            viewportRt.SetParent(wrapper, false);
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            // 内容页挪进视口
            content.transform.SetParent(viewportRt, false);

            var sr = wrapperGo.GetComponent<ScrollRect>();
            sr.viewport = viewportRt;
            sr.content = (RectTransform)content.transform;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 25f;
        }
        else
        {
            viewportRt = wrapper.Find("Viewport") as RectTransform;
        }

        // 内容页自身：顶部锚定 + 宽度铺满，高度由 ContentSizeFitter 决定
        var contentRt = (RectTransform)content.transform;
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.offsetMin = new Vector2(0f, contentRt.offsetMin.y);
        contentRt.offsetMax = new Vector2(0f, contentRt.offsetMax.y);
        contentRt.anchoredPosition = new Vector2(0f, 0f);

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(20, 20, 16, 16);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;   // 行铺满宽度
        vlg.childForceExpandHeight = false;

        var csf = content.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 行布局重建
        for (int i = 0; i < content.transform.childCount; i++)
            RebuildRow(content.transform.GetChild(i));
    }

    /// <summary>统一行内布局：行高 48，Label 定宽 / Slider 弹性拉横 / Value 定宽 / Toggle 32×32。</summary>
    void RebuildRow(Transform row)
    {
        var le = row.GetComponent<LayoutElement>();
        if (le == null) le = row.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 48f;
        le.preferredHeight = 48f;
        le.flexibleWidth = 1f;

        var hlg = row.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 14f;
        hlg.padding = new RectOffset(12, 12, 4, 4);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        for (int i = 0; i < row.childCount; i++)
        {
            var child = row.GetChild(i);
            var cle = child.GetComponent<LayoutElement>();
            if (cle == null) cle = child.gameObject.AddComponent<LayoutElement>();

            var slider = child.GetComponent<Slider>();
            var toggle = child.GetComponent<Toggle>();
            if (slider != null)
            {
                // 滑块：弹性占满剩余宽度，横向
                cle.minWidth = 180f;
                cle.flexibleWidth = 1f;
                cle.preferredHeight = 28f;
                slider.direction = Slider.Direction.LeftToRight;
            }
            else if (toggle != null)
            {
                // 开关：32×32 方块 + 勾形
                cle.minWidth = 32f;
                cle.preferredWidth = 32f;
                cle.preferredHeight = 32f;
                cle.flexibleWidth = 0f;
            }
            else if (child.name == "Label")
            {
                cle.preferredWidth = 150f;
                cle.preferredHeight = 40f;
                cle.flexibleWidth = 0f;
                var txt = child.GetComponent<Text>();
                if (txt != null) txt.alignment = TextAnchor.MiddleLeft;
            }
            else if (child.name == "Value")
            {
                cle.preferredWidth = 80f;
                cle.preferredHeight = 40f;
                cle.flexibleWidth = 0f;
                var txt = child.GetComponent<Text>();
                if (txt != null) txt.alignment = TextAnchor.MiddleRight;
            }
            else if (child.name == "KeyDisplay")
            {
                cle.preferredWidth = 140f;
                cle.preferredHeight = 36f;
                cle.flexibleWidth = 0f;
            }
            else
            {
                // Spacer 等：吃掉剩余空间
                cle.flexibleWidth = 1f;
                cle.preferredHeight = 1f;
            }
        }
    }

    void Start()
    {
        LoadAndApplySettings();
        SwitchToTab("AudioVideo");
    }

    void OnEnable()
    {
        // 每次打开面板时刷新一次显示，反映外部可能的改动
        RefreshDisplayFromPrefs();
        // 状态重申：重新激活时强制刷一次 Tab 颜色与内容显隐，
        // 防止上次关闭时序异常导致 Tab 高亮与内容页不一致
        SwitchToTab(currentTab);
    }

    // ═══════════════════════════════════════════
    // 绑定控件事件
    // ═══════════════════════════════════════════

    void BindControls()
    {
        // Tab 按钮（点击补音效，与 PauseMenu 按钮模式一致）
        if (tabAudioVideo != null)
            tabAudioVideo.onClick.AddListener(() => { IIPBootstrap.Audio?.PlayClick(); SwitchToTab("AudioVideo"); });
        if (tabControls != null)
            tabControls.onClick.AddListener(() => { IIPBootstrap.Audio?.PlayClick(); SwitchToTab("Controls"); });
        if (tabAssist != null)
            tabAssist.onClick.AddListener(() => { IIPBootstrap.Audio?.PlayClick(); SwitchToTab("Assist"); });

        // 返回按钮
        if (backButton != null)
            backButton.onClick.AddListener(() =>
            {
                SaveSettings();
                if (onBackToPauseMenu != null) onBackToPauseMenu.Invoke();
            });

        // 音量滑块
        BindSlider(sliderMaster, labelMaster, KEY_MASTER_VOL, ApplyMasterVolume);
        BindSlider(sliderSFX, labelSFX, KEY_SFX_VOL, ApplySFXVolume);
        BindSlider(sliderMusic, labelMusic, KEY_MUSIC_VOL, ApplyMusicVolume);
        BindSlider(sliderUI, labelUI, KEY_UI_VOL, ApplyUIVolume);

        // 开关
        BindToggle(toggleDamageNumber, KEY_DAMAGE_NUM, ApplyDamageNumberToggle);
        BindToggle(toggleMinimap, KEY_MINIMAP, ApplyMinimapToggle);
        BindToggle(toggleAutoPickup, KEY_AUTO_PICKUP, null);
        BindToggle(toggleColorblind, KEY_COLORBLIND, null);
    }

    void BindSlider(Slider slider, Text label, string prefsKey, System.Action<float> apply)
    {
        if (slider == null) return;
        slider.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetFloat(prefsKey, v);
            if (label != null) label.text = FormatSliderValue(v, 0f, 1f);
            if (apply != null) apply(v);
        });
    }

    void BindToggle(Toggle toggle, string prefsKey, System.Action<bool> apply)
    {
        if (toggle == null) return;
        toggle.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetInt(prefsKey, v ? 1 : 0);
            if (apply != null) apply(v);
        });
    }

    // ═══════════════════════════════════════════
    // Tab 切换
    // ═══════════════════════════════════════════

    void SwitchToTab(string key)
    {
        currentTab = key;

        // Tab 按钮颜色
        SetTabColor(tabAudioVideo, key == "AudioVideo");
        SetTabColor(tabControls, key == "Controls");
        SetTabColor(tabAssist, key == "Assist");

        // 内容面板
        if (contentAudioVideo != null) contentAudioVideo.SetActive(key == "AudioVideo");
        if (contentControls != null) contentControls.SetActive(key == "Controls");
        if (contentAssist != null) contentAssist.SetActive(key == "Assist");

        // 内容矮于视口时垂直居中（音画页只有 4 行滑块，顶部对齐会显得底部大片空白）
        RefreshContentCentering(contentAudioVideo);
        RefreshContentCentering(contentControls);
        RefreshContentCentering(contentAssist);
    }

    /// <summary>内容页垂直居中：内容高度不足视口时加大上 padding（不影响超高页面的滚动）</summary>
    void RefreshContentCentering(GameObject content)
    {
        if (content == null) return;
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) return;

        var contentRt = content.transform as RectTransform;
        var viewportRt = contentRt != null ? contentRt.parent as RectTransform : null;
        if (viewportRt == null) return;

        float viewportH = viewportRt.rect.height;
        if (viewportH <= 0f) return; // 布局未就绪（Awake 阶段），下次 SwitchToTab 再补

        float contentH = LayoutUtility.GetPreferredHeight(contentRt);
        int topPad = Mathf.Max(16, Mathf.RoundToInt((viewportH - contentH) * 0.5f));
        if (vlg.padding.top != topPad)
        {
            vlg.padding.top = topPad;
            LayoutRebuilder.MarkLayoutForRebuild(contentRt);
        }
    }

    void SetTabColor(Button tab, bool active)
    {
        if (tab == null) return;
        Image img = tab.targetGraphic as Image;
        if (img != null)
            img.color = active ? TAB_ACTIVE_COLOR : TAB_INACTIVE_COLOR;
    }

    // ═══════════════════════════════════════════
    // 设置项功能绑定
    // ═══════════════════════════════════════════

    void ApplyMasterVolume(float value)
    {
        var audio = GameplayAudioManager.Instance;
        if (audio != null) { audio.masterVolume = value; audio.UpdateVolumes(); }
    }

    void ApplySFXVolume(float value)
    {
        var audio = GameplayAudioManager.Instance;
        if (audio != null) { audio.sfxVolume = value; audio.UpdateVolumes(); }
    }

    void ApplyMusicVolume(float value)
    {
        var audio = GameplayAudioManager.Instance;
        if (audio != null) { audio.musicVolume = value; audio.UpdateVolumes(); }
    }

    void ApplyUIVolume(float value)
    {
        var audio = GameplayAudioManager.Instance;
        if (audio != null) { audio.uiVolume = value; audio.UpdateVolumes(); }
    }

    void ApplyDamageNumberToggle(bool enabled)
    {
        Debug.Log($"[SettingsPanel] 伤害数字显示: {(enabled ? "开启" : "关闭")}");
    }

    void ApplyMinimapToggle(bool enabled)
    {
        var minimap = FindObjectOfType<MinimapUI>();
        if (minimap != null)
        {
            minimap.gameObject.SetActive(enabled);
            Debug.Log($"[SettingsPanel] 小地图: {(enabled ? "显示" : "隐藏")}");
        }
    }

    // ═══════════════════════════════════════════
    // 持久化
    // ═══════════════════════════════════════════

    void LoadAndApplySettings()
    {
        float masterVol = PlayerPrefs.GetFloat(KEY_MASTER_VOL, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.7f);
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.6f);
        float uiVol = PlayerPrefs.GetFloat(KEY_UI_VOL, 0.8f);

        var audio = GameplayAudioManager.Instance;
        if (audio != null)
        {
            audio.masterVolume = masterVol;
            audio.sfxVolume = sfxVol;
            audio.musicVolume = musicVol;
            audio.uiVolume = uiVol;
            audio.UpdateVolumes();
        }

        bool showMinimap = PlayerPrefs.GetInt(KEY_MINIMAP, 1) == 1;
        var minimap = FindObjectOfType<MinimapUI>();
        if (minimap != null) minimap.gameObject.SetActive(showMinimap);

        RefreshDisplayFromPrefs();
    }

    /// <summary>把当前 PlayerPrefs 反映到 UI 控件显示（不触发回调）</summary>
    void RefreshDisplayFromPrefs()
    {
        SetSliderQuiet(sliderMaster, labelMaster, PlayerPrefs.GetFloat(KEY_MASTER_VOL, 0.8f));
        SetSliderQuiet(sliderSFX, labelSFX, PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.7f));
        SetSliderQuiet(sliderMusic, labelMusic, PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.6f));
        SetSliderQuiet(sliderUI, labelUI, PlayerPrefs.GetFloat(KEY_UI_VOL, 0.8f));

        SetToggleQuiet(toggleDamageNumber, PlayerPrefs.GetInt(KEY_DAMAGE_NUM, 1) == 1);
        SetToggleQuiet(toggleMinimap, PlayerPrefs.GetInt(KEY_MINIMAP, 1) == 1);
        SetToggleQuiet(toggleAutoPickup, PlayerPrefs.GetInt(KEY_AUTO_PICKUP, 0) == 1);
        SetToggleQuiet(toggleColorblind, PlayerPrefs.GetInt(KEY_COLORBLIND, 0) == 1);
    }

    void SetSliderQuiet(Slider slider, Text label, float value)
    {
        if (slider != null) slider.SetValueWithoutNotify(value);
        if (label != null) label.text = FormatSliderValue(value, 0f, 1f);
    }

    void SetToggleQuiet(Toggle toggle, bool value)
    {
        if (toggle != null) toggle.SetIsOnWithoutNotify(value);
    }

    void SaveSettings()
    {
        PlayerPrefs.Save();
        Debug.Log("[SettingsPanel] 设置已保存");
    }

    // ═══════════════════════════════════════════
    // 工具方法
    // ═══════════════════════════════════════════

    string FormatSliderValue(float value, float min, float max)
    {
        if (max <= 1f && min >= 0f)
            return Mathf.RoundToInt(value * 100) + "%";
        return value.ToString("F1");
    }
}
