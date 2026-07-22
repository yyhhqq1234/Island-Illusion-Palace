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
    private const string KEY_DAMAGE_NUM = "IIP_ShowDamageNumbers";
    private const string KEY_MINIMAP = "IIP_ShowMinimap";
    private const string KEY_AUTO_PICKUP = "IIP_AutoPickup";
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
            IIPUIFactory.ApplyRounded(check, true);
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

    void Start()
    {
        LoadAndApplySettings();
        SwitchToTab("AudioVideo");
    }

    void OnEnable()
    {
        // 每次打开面板时刷新一次显示，反映外部可能的改动
        RefreshDisplayFromPrefs();
    }

    // ═══════════════════════════════════════════
    // 绑定控件事件
    // ═══════════════════════════════════════════

    void BindControls()
    {
        // Tab 按钮
        if (tabAudioVideo != null)
            tabAudioVideo.onClick.AddListener(() => SwitchToTab("AudioVideo"));
        if (tabControls != null)
            tabControls.onClick.AddListener(() => SwitchToTab("Controls"));
        if (tabAssist != null)
            tabAssist.onClick.AddListener(() => SwitchToTab("Assist"));

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
