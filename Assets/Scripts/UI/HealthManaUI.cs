using UnityEngine;
using UnityEngine.UI;
using IIPUI;

/// <summary>
/// HP/MP 血条UI组件（事件驱动）
/// 订阅 GlobalEventManager.OnPlayerHealthChanged / OnPlayerManaChanged，
/// 仅在血量/蓝量变化时刷新，无 Update 轮询。
/// 位置：屏幕左上角，HP条在上、MP条在下（HUD 纵向堆叠第一层：HP/MP → 负担 → 经验）。
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

    [Tooltip("MP条背景颜色（压暗，与青色填充拉开对比，0 MP 不呈满条观感）")]
    public Color mpBarBackgroundColor = IIPUIStyle.ManaBarBackground;

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

    /// <summary>确保根 RectTransform 布局（左上角，pivot 0,1；HUD 纵向堆叠：HP/MP → 负担 → 经验）</summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        // x=54 给左侧 HP/MP 小标签留位（标签在容器 x-34~0 区间），避免标签越出屏幕左缘；
        // 左缘标签区起点 x=20，与下方负担条/经验条容器左边距一致
        rt.anchoredPosition = new Vector2(54, -20);
        // HUD 整体放大 1.4×：280×46 → 392×64（HP 28~59 + 间隙 + MP 0~23）
        rt.sizeDelta = new Vector2(392, 64);
    }

    /// <summary>初始化UI（缺引用时构建默认）</summary>
    void InitializeUI()
    {
        if (hpFill == null || mpFill == null)
        {
            CreateDefaultUI();
        }

        // MP 条背景强制压暗（覆盖场景序列化旧色，与青色填充拉开对比）
        if (mpBarBackground != null)
        {
            mpBarBackground.color = mpBarBackgroundColor;
        }
    }

    /// <summary>构建默认 HP/MP 条 UI（圆角底+边框+Filled填充+数值文本+小标签）。HUD 1.4× 放大版。</summary>
    void CreateDefaultUI()
    {
        // HP 条容器：左上角布局 HP 在上，y=28~59（高 31，原 22×1.4）
        GameObject hpBarObj = new GameObject("HPBar", typeof(RectTransform), typeof(Image));
        hpBarObj.transform.SetParent(transform, false);
        hpBarBackground = hpBarObj.GetComponent<Image>();
        hpBarBackground.color = barBackgroundColor;
        IIPUIFactory.ApplyRounded(hpBarBackground, true);
        RectTransform hpBgRect = hpBarObj.GetComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0, 0);
        hpBgRect.anchorMax = new Vector2(1, 0);
        hpBgRect.pivot = new Vector2(0, 0);
        hpBgRect.offsetMin = new Vector2(0, 28);
        hpBgRect.offsetMax = new Vector2(0, 59);
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

        // HP 小标签（左侧外，与 HP 条同区间 y=28~59）
        hpLabel = IIPUIFactory.CreateLabelAnchored("HPLabel", transform,
            "HP", 17, IIPUIFactory.TextDim,
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(-34, 28), new Vector2(0, 59), TextAnchor.MiddleRight);

        // MP 条容器：HP 正下方，y=0~23（高 23，原 16×1.4，与 HP 留 5px 间隙）
        GameObject mpBarObj = new GameObject("MPBar", typeof(RectTransform), typeof(Image));
        mpBarObj.transform.SetParent(transform, false);
        mpBarBackground = mpBarObj.GetComponent<Image>();
        mpBarBackground.color = mpBarBackgroundColor; // 比 HP 更暗的专属底色
        IIPUIFactory.ApplyRounded(mpBarBackground, true);
        RectTransform mpBgRect = mpBarObj.GetComponent<RectTransform>();
        mpBgRect.anchorMin = new Vector2(0, 0);
        mpBgRect.anchorMax = new Vector2(1, 0);
        mpBgRect.pivot = new Vector2(0, 0);
        mpBgRect.offsetMin = new Vector2(0, 0);
        mpBgRect.offsetMax = new Vector2(0, 23);
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

        // MP 小标签（左侧外，与 MP 条同区间 y=0~23）
        mpLabel = IIPUIFactory.CreateLabelAnchored("MPLabel", transform,
            "MP", 17, IIPUIFactory.TextDim,
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(-34, 0), new Vector2(0, 23), TextAnchor.MiddleRight);
    }
}
