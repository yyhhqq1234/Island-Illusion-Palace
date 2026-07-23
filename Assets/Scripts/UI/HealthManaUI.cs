using UnityEngine;
using UnityEngine.UI;
using IIPUI;

/// <summary>
/// HP/MP 血条UI组件（事件驱动）
/// 订阅 GlobalEventManager.OnPlayerHealthChanged / OnPlayerManaChanged，
/// 仅在血量/蓝量变化时刷新，无 Update 轮询。
/// 位置：屏幕左下角，HP条在上、MP条在下。
/// </summary>
public class HealthManaUI : MonoBehaviour
{
    [Header("UI组件引用")]
    [Tooltip("HP条背景")]
    public Image hpBarBackground;

    [Tooltip("HP条填充（Filled 模式）")]
    public Image hpFill;

    [Tooltip("HP数值文本")]
    public Text hpText;

    [Tooltip("HP小标签")]
    public Text hpLabel;

    [Tooltip("MP条背景")]
    public Image mpBarBackground;

    [Tooltip("MP条填充（Filled 模式）")]
    public Image mpFill;

    [Tooltip("MP数值文本")]
    public Text mpText;

    [Tooltip("MP小标签")]
    public Text mpLabel;

    [Header("样式配置")]
    [Tooltip("HP条颜色")]
    public Color hpBarColor = IIPUIStyle.AccentRed;

    [Tooltip("MP条颜色")]
    public Color mpBarColor = IIPUIStyle.AccentCyan;

    [Tooltip("条背景颜色")]
    public Color barBackgroundColor = IIPUIStyle.BarBackgroundDark;

    [Tooltip("数值文本颜色")]
    public Color valueTextColor = Color.white;

    private HealthSystem healthSystem;
    private ManaSystem manaSystem;
    private bool dataRefreshed = false;

    void Start()
    {
        EnsureContainerLayout();
        InitializeUI();

        healthSystem = FindObjectOfType<HealthSystem>();
        manaSystem = FindObjectOfType<ManaSystem>();

        if (healthSystem != null)
        {
            RefreshHP(healthSystem.currentHealth, healthSystem.maxHealth);
            dataRefreshed = true;
        }
        else
        {
            Debug.LogWarning("[HealthManaUI] Start 时未找到 HealthSystem，将在 Update 中延迟补拉");
        }

        if (manaSystem != null)
        {
            RefreshMP(manaSystem.currentMana, manaSystem.maxMana);
        }
        else
        {
            Debug.LogWarning("[HealthManaUI] Start 时未找到 ManaSystem，将在 Update 中延迟补拉");
        }
    }

    void Update()
    {
        // 玩家预制体可能晚于 HUD Start，延迟补拉一次初值
        // 注意：manaSystem 可能在某些场景不存在（无蓝条），healthSystem 找到即视为数据就绪，避免每帧 Find
        if (!dataRefreshed)
        {
            if (healthSystem == null) healthSystem = FindObjectOfType<HealthSystem>();
            if (manaSystem == null) manaSystem = FindObjectOfType<ManaSystem>();

            if (healthSystem != null)
            {
                RefreshHP(healthSystem.currentHealth, healthSystem.maxHealth);
                dataRefreshed = true; // healthSystem 就绪即停止轮询；manaSystem 为 null 视为该场景无蓝条
            }
            if (manaSystem != null)
            {
                RefreshMP(manaSystem.currentMana, manaSystem.maxMana);
            }
        }
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
        {
            GlobalEventManager.Instance.OnPlayerHealthChanged += HandleHealthChanged;
            GlobalEventManager.Instance.OnPlayerManaChanged += HandleManaChanged;
        }
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
        {
            GlobalEventManager.Instance.OnPlayerHealthChanged -= HandleHealthChanged;
            GlobalEventManager.Instance.OnPlayerManaChanged -= HandleManaChanged;
        }
    }

    void HandleHealthChanged(float current, float max)
    {
        RefreshHP(current, max);
    }

    void HandleManaChanged(float current, float max)
    {
        RefreshMP(current, max);
    }

    /// <summary>刷新 HP 条</summary>
    void RefreshHP(float current, float max)
    {
        if (hpFill != null)
        {
            float pct = max > 0f ? current / max : 0f;
            hpFill.fillAmount = Mathf.Clamp01(pct);
        }
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }

    /// <summary>刷新 MP 条</summary>
    void RefreshMP(float current, float max)
    {
        if (mpFill != null)
        {
            float pct = max > 0f ? current / max : 0f;
            mpFill.fillAmount = Mathf.Clamp01(pct);
        }
        if (mpText != null)
        {
            mpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }

    /// <summary>确保根 RectTransform 布局（左下角，pivot 0,0）</summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 20);
        // HUD 整体放大 1.4×：280×46 → 392×64（HP 0~31 + 间隙 + MP 36~59）
        rt.sizeDelta = new Vector2(392, 64);
    }

    /// <summary>初始化UI（缺引用时构建默认）</summary>
    void InitializeUI()
    {
        if (hpFill == null || mpFill == null)
        {
            CreateDefaultUI();
        }
    }

    /// <summary>构建默认 HP/MP 条 UI（圆角底+边框+Filled填充+数值文本+小标签）。HUD 1.4× 放大版。</summary>
    void CreateDefaultUI()
    {
        // HP 条容器：左下角 (0,0)，y=0~31（高 31，原 22×1.4）
        GameObject hpBarObj = new GameObject("HPBar", typeof(RectTransform), typeof(Image));
        hpBarObj.transform.SetParent(transform, false);
        hpBarBackground = hpBarObj.GetComponent<Image>();
        hpBarBackground.color = barBackgroundColor;
        IIPUIFactory.ApplyRounded(hpBarBackground, true);
        RectTransform hpBgRect = hpBarObj.GetComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0, 0);
        hpBgRect.anchorMax = new Vector2(1, 0);
        hpBgRect.pivot = new Vector2(0, 0);
        hpBgRect.offsetMin = new Vector2(0, 0);
        hpBgRect.offsetMax = new Vector2(0, 31);
        IIPUIFactory.CreateBorder(hpBarObj.transform, IIPUIFactory.BorderDim, true);

        // HP 填充（Filled Horizontal，避免 localScale 圆角变形；圆角 sprite 让满条末端与底框一致）
        GameObject hpFillObj = new GameObject("HPFill", typeof(RectTransform), typeof(Image));
        hpFillObj.transform.SetParent(hpBarObj.transform, false);
        hpFill = hpFillObj.GetComponent<Image>();
        hpFill.color = hpBarColor;
        IIPUIFactory.ApplyRounded(hpFill, true);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform hpFillRect = hpFillObj.GetComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero;
        hpFillRect.anchorMax = Vector2.one;
        hpFillRect.offsetMin = Vector2.zero;
        hpFillRect.offsetMax = Vector2.zero;

        // HP 数值文本（右侧），字号 12→17（1.4×）
        hpText = IIPUIFactory.CreateLabelAnchored("HPText", hpBarObj.transform,
            "", 17, valueTextColor,
            new Vector2(1, 0), new Vector2(1, 1),
            new Vector2(-84, 0), new Vector2(-6, 0), TextAnchor.MiddleRight);

        // HP 小标签（左侧外）
        hpLabel = IIPUIFactory.CreateLabelAnchored("HPLabel", transform,
            "HP", 17, IIPUIFactory.TextDim,
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(-34, 0), new Vector2(0, 31), TextAnchor.MiddleRight);

        // MP 条容器：左下角，y=36~59（高 23，原 16×1.4，紧贴 HP 上方留 5px 间隙）
        GameObject mpBarObj = new GameObject("MPBar", typeof(RectTransform), typeof(Image));
        mpBarObj.transform.SetParent(transform, false);
        mpBarBackground = mpBarObj.GetComponent<Image>();
        mpBarBackground.color = barBackgroundColor;
        IIPUIFactory.ApplyRounded(mpBarBackground, true);
        RectTransform mpBgRect = mpBarObj.GetComponent<RectTransform>();
        mpBgRect.anchorMin = new Vector2(0, 0);
        mpBgRect.anchorMax = new Vector2(1, 0);
        mpBgRect.pivot = new Vector2(0, 0);
        mpBgRect.offsetMin = new Vector2(0, 36);
        mpBgRect.offsetMax = new Vector2(0, 59);
        IIPUIFactory.CreateBorder(mpBarObj.transform, IIPUIFactory.BorderDim, true);

        // MP 填充（Filled Horizontal，圆角 sprite 与 HP 一致）
        GameObject mpFillObj = new GameObject("MPFill", typeof(RectTransform), typeof(Image));
        mpFillObj.transform.SetParent(mpBarObj.transform, false);
        mpFill = mpFillObj.GetComponent<Image>();
        mpFill.color = mpBarColor;
        IIPUIFactory.ApplyRounded(mpFill, true);
        mpFill.type = Image.Type.Filled;
        mpFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform mpFillRect = mpFillObj.GetComponent<RectTransform>();
        mpFillRect.anchorMin = Vector2.zero;
        mpFillRect.anchorMax = Vector2.one;
        mpFillRect.offsetMin = Vector2.zero;
        mpFillRect.offsetMax = Vector2.zero;

        // MP 数值文本（右侧）
        mpText = IIPUIFactory.CreateLabelAnchored("MPText", mpBarObj.transform,
            "", 17, valueTextColor,
            new Vector2(1, 0), new Vector2(1, 1),
            new Vector2(-84, 0), new Vector2(-6, 0), TextAnchor.MiddleRight);

        // MP 小标签（左侧外）
        mpLabel = IIPUIFactory.CreateLabelAnchored("MPLabel", transform,
            "MP", 17, IIPUIFactory.TextDim,
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(-34, 36), new Vector2(0, 59), TextAnchor.MiddleRight);
    }
}
