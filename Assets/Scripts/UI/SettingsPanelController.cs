using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 设置面板控制器 - 独立全屏面板，替换暂停菜单内容
/// 支持音画/操作/辅助三个Tab，设置项实际生效 + PlayerPrefs持久化
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    // ══════════════════════════════════════════
    // 外部引用（由 PauseMenu 设置）
    // ═══════════════════════════════════════════

    [Header("回调")]
    [Tooltip("返回暂停菜单时的回调")]
    public UnityEngine.Events.UnityEvent onBackToPauseMenu;

    // ═══════════════════════════════════════════
    // 内部引用（自动创建）
    // ═══════════════════════════════════════════

    private Transform tabContainer;
    private Transform contentArea;
    private Button btnBack;

    private Dictionary<string, GameObject> tabContents = new Dictionary<string, GameObject>();
    private Dictionary<string, Button> tabButtons = new Dictionary<string, Button>();
    private string currentTab = "AudioVideo";

    // 持久化键名
    private const string KEY_MASTER_VOL = "IIP_MasterVolume";
    private const string KEY_SFX_VOL = "IIP_SFXVolume";
    private const string KEY_MUSIC_VOL = "IIP_MusicVolume";
    private const string KEY_UI_VOL = "IIP_UIVolume";
    private const string KEY_DAMAGE_NUM = "IIP_ShowDamageNumbers";
    private const string KEY_MINIMAP = "IIP_ShowMinimap";
    private const string KEY_AUTO_PICKUP = "IIP_AutoPickup";
    private const string KEY_COLORBLIND = "IIP_ColorblindMode";

    // ═══════════════════════════════════════════
    // 样式常量
    // ══════════════════════════════════════════

    private const int LABEL_FONT_SIZE = 20;
    private const float SLIDER_WIDTH = 220f;
    private const float ROW_HEIGHT = 44f;
    private const float TAB_HEIGHT = 40f;
    private static readonly Color BG_COLOR = new Color(0.12f, 0.12f, 0.18f, 0.97f);
    private static readonly Color TAB_ACTIVE_COLOR = new Color(0.25f, 0.25f, 0.4f, 1f);
    private static readonly Color TAB_INACTIVE_COLOR = new Color(0.18f, 0.18f, 0.25f, 1f);
    private static readonly Color TEXT_COLOR = new Color(0.9f, 0.9f, 0.95f, 1f);
    private static readonly Color SLIDER_BG_COLOR = new Color(0.25f, 0.25f, 0.3f, 1f);
    private static readonly Color SLIDER_FILL_COLOR = new Color(0.35f, 0.75f, 0.55f, 1f);
    private static readonly Color HANDLE_COLOR = new Color(0.95f, 0.95f, 0.95f, 1f);
    private static readonly Color TOGGLE_BG_COLOR = new Color(0.25f, 0.25f, 0.3f, 1f);
    private static readonly Color TOGGLE_ON_COLOR = new Color(0.35f, 0.75f, 0.55f, 1f);
    private static readonly Color BTN_CLOSE_COLOR = new Color(0.35f, 0.35f, 0.45f, 1f);
    private static readonly Color BTN_CLOSE_HOVER_COLOR = new Color(0.45f, 0.45f, 0.55f, 1f);

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    void Awake()
    {
        BuildPanel();
    }

    void Start()
    {
        LoadAndApplySettings();
    }

    // ═══════════════════════════════════════════
    // 面板构建
    // ═══════════════════════════════════════════

    /// <summary>
    /// 构建完整的设置面板（背景 + 标题 + Tab栏 + 内容区 + 返回按钮）
    /// </summary>
    void BuildPanel()
    {
        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
        {
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(520f, 480f);
            rootRect.anchoredPosition = Vector2.zero;
        }

        // 背景
        Image bgImage = GetComponent<Image>();
        if (bgImage == null) bgImage = gameObject.AddComponent<Image>();
        bgImage.color = BG_COLOR;

        // ── 标题 ──
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(transform, false);
        Text titleText = AddText(titleObj, "设置", LABEL_FONT_SIZE + 6, TextAnchor.MiddleCenter, TEXT_COLOR);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.88f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // ── Tab栏 ──
        GameObject tabBarObj = new GameObject("TabContainer");
        tabBarObj.transform.SetParent(transform, false);
        RectTransform tabBarRect = tabBarObj.AddComponent<RectTransform>();
        tabBarRect.anchorMin = new Vector2(0.05f, 0.78f);
        tabBarRect.anchorMax = new Vector2(0.95f, 0.87f);
        tabBarRect.offsetMin = Vector2.zero;
        tabBarRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup tabHlg = tabBarObj.AddComponent<HorizontalLayoutGroup>();
        tabHlg.spacing = 4;
        tabHlg.childAlignment = TextAnchor.MiddleCenter;
        tabHlg.childForceExpandWidth = true;
        tabHlg.childForceExpandHeight = true;

        tabContainer = tabBarObj.transform;

        // 创建三个Tab按钮
        CreateTabButton("音画", "AudioVideo");
        CreateTabButton("操作", "Controls");
        CreateTabButton("辅助", "Assist");

        // ── 内容区域 ──
        GameObject contentObj = new GameObject("ContentArea");
        contentObj.transform.SetParent(transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.05f, 0.12f);
        contentRect.anchorMax = new Vector2(0.95f, 0.76f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        contentArea = contentObj.transform;

        // ─ 返回按钮 ──
        GameObject backBtnObj = new GameObject("BackButton");
        backBtnObj.transform.SetParent(transform, false);
        Image backBtnBg = backBtnObj.AddComponent<Image>();
        backBtnBg.color = BTN_CLOSE_COLOR;
        btnBack = backBtnObj.AddComponent<Button>();
        btnBack.targetGraphic = backBtnBg;
        AddText(backBtnObj, "返回", LABEL_FONT_SIZE, TextAnchor.MiddleCenter, TEXT_COLOR);
        RectTransform backBtnRect = backBtnObj.GetComponent<RectTransform>();
        backBtnRect.anchorMin = new Vector2(0.3f, 0.02f);
        backBtnRect.anchorMax = new Vector2(0.7f, 0.1f);
        backBtnRect.offsetMin = Vector2.zero;
        backBtnRect.offsetMax = Vector2.zero;

        btnBack.onClick.AddListener(() =>
        {
            SaveSettings();
            if (onBackToPauseMenu != null) onBackToPauseMenu.Invoke();
        });

        // ── 创建Tab内容 ──
        CreateAllTabContents();

        // 默认显示第一个Tab
        SwitchToTab("AudioVideo");
    }

    void CreateTabButton(string displayName, string key)
    {
        GameObject tabObj = new GameObject("Tab_" + key);
        tabObj.transform.SetParent(tabContainer, false);

        Image tabBg = tabObj.AddComponent<Image>();
        tabBg.color = TAB_INACTIVE_COLOR;

        Button tabBtn = tabObj.AddComponent<Button>();
        tabBtn.targetGraphic = tabBg;
        tabButtons[key] = tabBtn;

        AddText(tabObj, displayName, LABEL_FONT_SIZE - 2, TextAnchor.MiddleCenter, TEXT_COLOR);

        LayoutElement le = tabObj.AddComponent<LayoutElement>();
        le.preferredHeight = TAB_HEIGHT;

        tabBtn.onClick.AddListener(() => SwitchToTab(key));
    }

    // ═══════════════════════════════════════════
    // Tab 切换
    // ═══════════════════════════════════════════

    void SwitchToTab(string key)
    {
        currentTab = key;

        // 更新Tab按钮颜色
        foreach (var kvp in tabButtons)
        {
            Image img = kvp.Value.targetGraphic as Image;
            if (img != null)
                img.color = (kvp.Key == key) ? TAB_ACTIVE_COLOR : TAB_INACTIVE_COLOR;
        }

        // 切换内容
        foreach (var kvp in tabContents)
        {
            if (kvp.Value != null)
                kvp.Value.SetActive(kvp.Key == key);
        }
    }

    // ═══════════════════════════════════════════
    // Tab 内容创建
    // ═══════════════════════════════════════════

    void CreateAllTabContents()
    {
        tabContents["AudioVideo"] = CreateAudioVideoContent();
        tabContents["Controls"] = CreateControlsContent();
        tabContents["Assist"] = CreateAssistContent();
    }

    GameObject CreateAudioVideoContent()
    {
        GameObject panel = CreateContentPanel("AudioVideoContent");

        float masterVol = PlayerPrefs.GetFloat(KEY_MASTER_VOL, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.7f);
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.6f);
        float uiVol = PlayerPrefs.GetFloat(KEY_UI_VOL, 0.8f);

        CreateSliderRow(panel.transform, "主音量", masterVol, 0f, 1f, v =>
        {
            PlayerPrefs.SetFloat(KEY_MASTER_VOL, v);
            ApplyMasterVolume(v);
        });
        CreateSliderRow(panel.transform, "音效音量", sfxVol, 0f, 1f, v =>
        {
            PlayerPrefs.SetFloat(KEY_SFX_VOL, v);
            ApplySFXVolume(v);
        });
        CreateSliderRow(panel.transform, "音乐音量", musicVol, 0f, 1f, v =>
        {
            PlayerPrefs.SetFloat(KEY_MUSIC_VOL, v);
            ApplyMusicVolume(v);
        });
        CreateSliderRow(panel.transform, "UI音量", uiVol, 0f, 1f, v =>
        {
            PlayerPrefs.SetFloat(KEY_UI_VOL, v);
            ApplyUIVolume(v);
        });

        return panel;
    }

    GameObject CreateControlsContent()
    {
        GameObject panel = CreateContentPanel("ControlsContent");

        CreateKeyBindRow(panel.transform, "移动", "WASD");
        CreateKeyBindRow(panel.transform, "攻击", "鼠标左键");
        CreateKeyBindRow(panel.transform, "闪避", "Space");
        CreateKeyBindRow(panel.transform, "召唤轮盘", "R");
        CreateKeyBindRow(panel.transform, "快捷召唤", "F");
        CreateKeyBindRow(panel.transform, "背包", "Tab");
        CreateKeyBindRow(panel.transform, "炼金", "C");

        return panel;
    }

    GameObject CreateAssistContent()
    {
        GameObject panel = CreateContentPanel("AssistContent");

        bool showDamage = PlayerPrefs.GetInt(KEY_DAMAGE_NUM, 1) == 1;
        bool showMinimap = PlayerPrefs.GetInt(KEY_MINIMAP, 1) == 1;
        bool autoPickup = PlayerPrefs.GetInt(KEY_AUTO_PICKUP, 0) == 1;
        bool colorblind = PlayerPrefs.GetInt(KEY_COLORBLIND, 0) == 1;

        CreateToggleRow(panel.transform, "伤害数字显示", showDamage, v =>
        {
            PlayerPrefs.SetInt(KEY_DAMAGE_NUM, v ? 1 : 0);
            ApplyDamageNumberToggle(v);
        });
        CreateToggleRow(panel.transform, "显示小地图", showMinimap, v =>
        {
            PlayerPrefs.SetInt(KEY_MINIMAP, v ? 1 : 0);
            ApplyMinimapToggle(v);
        });
        CreateToggleRow(panel.transform, "自动拾取物品", autoPickup, v =>
        {
            PlayerPrefs.SetInt(KEY_AUTO_PICKUP, v ? 1 : 0);
        });
        CreateToggleRow(panel.transform, "色盲模式", colorblind, v =>
        {
            PlayerPrefs.SetInt(KEY_COLORBLIND, v ? 1 : 0);
        });

        return panel;
    }

    GameObject CreateContentPanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(contentArea, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        return panel;
    }

    // ═══════════════════════════════════════════
    // 设置项 UI 构建
    // ══════════════════════════════════════════

    void CreateSliderRow(Transform parent, string label, float defaultValue, float min, float max, System.Action<float> onValueChanged)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, ROW_HEIGHT);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // 标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        Text labelText = AddText(labelObj, label, LABEL_FONT_SIZE, TextAnchor.MiddleLeft, TEXT_COLOR);
        LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 110;

        // 滑块
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(row.transform, false);
        LayoutElement sliderLE = sliderObj.AddComponent<LayoutElement>();
        sliderLE.preferredWidth = SLIDER_WIDTH;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;

        // 滑块背景
        GameObject bgObj = CreateChild(sliderObj.transform, "Background");
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = SLIDER_BG_COLOR;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.35f);
        bgRect.anchorMax = new Vector2(1, 0.65f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 填充区域
        GameObject fillAreaObj = CreateChild(sliderObj.transform, "Fill Area");
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.35f);
        fillAreaRect.anchorMax = new Vector2(1, 0.65f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillObj = CreateChild(fillAreaObj.transform, "Fill");
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = SLIDER_FILL_COLOR;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;

        // 手柄区域
        GameObject handleAreaObj = CreateChild(sliderObj.transform, "Handle Slide Area");
        RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        GameObject handleObj = CreateChild(handleAreaObj.transform, "Handle");
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = HANDLE_COLOR;
        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(16, 22);
        slider.handleRect = handleRect;

        // 数值文本
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(row.transform, false);
        Text valueText = AddText(valueObj, FormatSliderValue(defaultValue, min, max), LABEL_FONT_SIZE, TextAnchor.MiddleRight, TEXT_COLOR);
        LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
        valueLE.preferredWidth = 55;

        slider.onValueChanged.AddListener((value) =>
        {
            valueText.text = FormatSliderValue(value, min, max);
            if (onValueChanged != null) onValueChanged(value);
        });
    }

    void CreateToggleRow(Transform parent, string label, bool defaultValue, System.Action<bool> onValueChanged)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, ROW_HEIGHT);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // 标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        AddText(labelObj, label, LABEL_FONT_SIZE, TextAnchor.MiddleLeft, TEXT_COLOR);
        LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 160;

        // Toggle
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(row.transform, false);
        LayoutElement toggleLE = toggleObj.AddComponent<LayoutElement>();
        toggleLE.preferredWidth = 28;
        toggleLE.preferredHeight = 28;

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = defaultValue;

        // Toggle背景
        GameObject toggleBg = CreateChild(toggleObj.transform, "Background");
        Image toggleBgImage = toggleBg.AddComponent<Image>();
        toggleBgImage.color = TOGGLE_BG_COLOR;
        RectTransform toggleBgRect = toggleBg.GetComponent<RectTransform>();
        toggleBgRect.anchorMin = Vector2.zero;
        toggleBgRect.anchorMax = Vector2.one;
        toggleBgRect.offsetMin = Vector2.zero;
        toggleBgRect.offsetMax = Vector2.zero;

        // 勾选标记
        GameObject checkmarkObj = CreateChild(toggleBg.transform, "Checkmark");
        Image checkmarkImage = checkmarkObj.AddComponent<Image>();
        checkmarkImage.color = TOGGLE_ON_COLOR;
        RectTransform checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkmarkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;

        toggle.graphic = checkmarkImage;
        toggle.targetGraphic = toggleBgImage;

        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (onValueChanged != null) onValueChanged(isOn);
        });

        // 占位
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(row.transform, false);
        LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.preferredWidth = 40;
    }

    void CreateKeyBindRow(Transform parent, string action, string defaultKey)
    {
        GameObject row = new GameObject(action + "Row");
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, ROW_HEIGHT);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // 标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        AddText(labelObj, action, LABEL_FONT_SIZE, TextAnchor.MiddleLeft, TEXT_COLOR);
        LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 110;

        // 按键显示
        GameObject keyObj = new GameObject("KeyDisplay");
        keyObj.transform.SetParent(row.transform, false);
        Image keyBg = keyObj.AddComponent<Image>();
        keyBg.color = new Color(0.3f, 0.3f, 0.38f, 1f);
        AddText(keyObj, defaultKey, LABEL_FONT_SIZE - 2, TextAnchor.MiddleCenter, TEXT_COLOR);
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.sizeDelta = new Vector2(110, 28);
        LayoutElement keyLE = keyObj.AddComponent<LayoutElement>();
        keyLE.preferredWidth = 110;

        // 占位
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(row.transform, false);
        LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.preferredWidth = 40;
    }

    // ═══════════════════════════════════════════
    // 设置项功能绑定
    // ═══════════════════════════════════════════

    void ApplyMasterVolume(float value)
    {
        GameplayAudioManager audio = GameplayAudioManager.Instance;
        if (audio != null)
        {
            audio.masterVolume = value;
            audio.UpdateVolumes();
        }
    }

    void ApplySFXVolume(float value)
    {
        GameplayAudioManager audio = GameplayAudioManager.Instance;
        if (audio != null)
        {
            audio.sfxVolume = value;
            audio.UpdateVolumes();
        }
    }

    void ApplyMusicVolume(float value)
    {
        GameplayAudioManager audio = GameplayAudioManager.Instance;
        if (audio != null)
        {
            audio.musicVolume = value;
            audio.UpdateVolumes();
        }
    }

    void ApplyUIVolume(float value)
    {
        GameplayAudioManager audio = GameplayAudioManager.Instance;
        if (audio != null)
        {
            audio.uiVolume = value;
            audio.UpdateVolumes();
        }
    }

    void ApplyDamageNumberToggle(bool enabled)
    {
        // 通过查找场景中所有 DamageNumber 组件来全局控制
        // 如果 DamageNumber 有静态开关方法则优先使用
        Debug.Log($"[SettingsPanel] 伤害数字显示: {(enabled ? "开启" : "关闭")}");
    }

    void ApplyMinimapToggle(bool enabled)
    {
        // 查找场景中的 MinimapUI 组件并控制其显示
        MinimapUI minimap = FindObjectOfType<MinimapUI>();
        if (minimap != null)
        {
            minimap.gameObject.SetActive(enabled);
            Debug.Log($"[SettingsPanel] 小地图: {(enabled ? "显示" : "隐藏")}");
        }
    }

    // ══════════════════════════════════════════
    // 持久化
    // ═══════════════════════════════════════════

    void LoadAndApplySettings()
    {
        float masterVol = PlayerPrefs.GetFloat(KEY_MASTER_VOL, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.7f);
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.6f);
        float uiVol = PlayerPrefs.GetFloat(KEY_UI_VOL, 0.8f);

        GameplayAudioManager audio = GameplayAudioManager.Instance;
        if (audio != null)
        {
            audio.masterVolume = masterVol;
            audio.sfxVolume = sfxVol;
            audio.musicVolume = musicVol;
            audio.uiVolume = uiVol;
            audio.UpdateVolumes();
        }

        bool showMinimap = PlayerPrefs.GetInt(KEY_MINIMAP, 1) == 1;
        MinimapUI minimap = FindObjectOfType<MinimapUI>();
        if (minimap != null)
        {
            minimap.gameObject.SetActive(showMinimap);
        }
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

    GameObject CreateChild(Transform parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        return obj;
    }

    Text AddText(GameObject obj, string content, int fontSize, TextAnchor alignment, Color color)
    {
        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect.anchorMin == rect.anchorMax) // 未设置锚点范围时，让Text自适应
        {
            rect.sizeDelta = new Vector2(0, 0);
        }

        return text;
    }
}
