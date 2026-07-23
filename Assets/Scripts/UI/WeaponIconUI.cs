using UnityEngine;
using UnityEngine.UI;
using IIPUI;

/// <summary>
/// 武器图标 + 闪避冷却环形指示器（合并组件，事件驱动）
/// 位置：屏幕右下角，武器图标 56×56，外围环绕闪避冷却指示器（外径68px，环宽4px）。
/// 订阅 GlobalEventManager.OnWeaponChanged（刷新武器图标/名称）
///       GlobalEventManager.OnDashCooldownChanged（刷新环形指示器）
/// </summary>
public class WeaponIconUI : MonoBehaviour
{
    [Header("UI组件")]
    [Tooltip("武器图标背景")]
    public Image background;

    [Tooltip("武器图标")]
    public Image weaponIcon;

    [Tooltip("武器名称文本")]
    public Text weaponNameText;

    [Tooltip("武器类型文本")]
    public Text weaponTypeText;

    [Tooltip("闪避冷却环形指示器（Filled Radial360）")]
    public Image dashCooldownRing;

    [Tooltip("闪避快捷键提示文本")]
    public Text dashKeyHintText;

    [Header("样式配置")]
    [Tooltip("武器图标尺寸（正方形边长）")]
    public float iconSize = IIPUIStyle.SlotSizeStandard;

    [Tooltip("环形指示器外径")]
    public float ringOuterSize = 68f;

    [Tooltip("背景颜色")]
    public Color backgroundColor = IIPUIStyle.WeaponIconBackground;

    [Tooltip("就绪状态环形颜色")]
    public Color ringReadyColor = IIPUIStyle.CooldownRingReady;

    [Tooltip("冷却中环形颜色")]
    public Color ringCooldownColor = IIPUIStyle.CooldownRingBusy;

    private WeaponSystem weaponSystem;
    private PlayerController playerController;
    private float maxDashCooldown = 1f;
    private bool dataRefreshed = false;

    /// <summary>HUD 整体放大系数（与其它 HUD 组件一致）</summary>
    const float HudScale = 1.4f;

    void Start()
    {
        EnsureContainerLayout();
        weaponSystem = FindObjectOfType<WeaponSystem>();
        playerController = FindObjectOfType<PlayerController>();

        if (weaponSystem == null)
            Debug.LogWarning("[WeaponIconUI] 未找到 WeaponSystem");
        if (playerController == null)
            Debug.LogWarning("[WeaponIconUI] 未找到 PlayerController");

        InitializeUI();

        // 主动拉一次武器初值
        if (weaponSystem != null)
            RefreshWeapon(weaponSystem.currentWeaponType, weaponSystem.enhancementLevel);

        // 主动拉一次闪避冷却初值（默认就绪）
        if (playerController != null)
        {
            maxDashCooldown = playerController.dashCooldown;
            RefreshDash(0f, maxDashCooldown);
        }
        else
        {
            RefreshDash(0f, maxDashCooldown);
        }
    }

    void Update()
    {
        // 玩家预制体可能晚于 HUD Start，延迟补拉一次初值
        // weaponSystem 就绪即视为数据就绪；playerController 仅用于读 dashCooldown，找不到时用默认值
        if (!dataRefreshed)
        {
            if (weaponSystem == null) weaponSystem = FindObjectOfType<WeaponSystem>();
            if (playerController == null) playerController = FindObjectOfType<PlayerController>();

            if (weaponSystem != null)
            {
                RefreshWeapon(weaponSystem.currentWeaponType, weaponSystem.enhancementLevel);
                dataRefreshed = true;
            }
            if (playerController != null)
            {
                maxDashCooldown = playerController.dashCooldown;
                RefreshDash(0f, maxDashCooldown);
            }
        }
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
        {
            GlobalEventManager.Instance.OnWeaponChanged += HandleWeaponChanged;
            GlobalEventManager.Instance.OnDashCooldownChanged += HandleDashCooldownChanged;
        }
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
        {
            GlobalEventManager.Instance.OnWeaponChanged -= HandleWeaponChanged;
            GlobalEventManager.Instance.OnDashCooldownChanged -= HandleDashCooldownChanged;
        }
    }

    void HandleWeaponChanged(WeaponType weaponType, int enhancementLevel)
    {
        RefreshWeapon(weaponType, enhancementLevel);
    }

    void HandleDashCooldownChanged(float remaining, float total)
    {
        if (total > 0f) maxDashCooldown = total;
        RefreshDash(remaining, maxDashCooldown);
    }

    /// <summary>刷新武器显示</summary>
    void RefreshWeapon(WeaponType weaponType, int enhancementLevel)
    {
        var weaponData = weaponSystem != null ? weaponSystem.weaponData : null;

        if (weaponData != null)
        {
            if (weaponIcon != null)
            {
                if (weaponData.icon != null)
                {
                    weaponIcon.sprite = weaponData.icon;
                    weaponIcon.color = Color.white;
                }
                else
                {
                    weaponIcon.sprite = null;
                    weaponIcon.color = Color.clear;
                }
            }

            if (weaponNameText != null)
            {
                string name = !string.IsNullOrEmpty(weaponData.weaponName) ? weaponData.weaponName : weaponType.ToString();
                weaponNameText.text = enhancementLevel > 0 ? $"{name} +{enhancementLevel}" : name;
            }

            if (weaponTypeText != null)
            {
                weaponTypeText.text = GetWeaponTypeText(weaponType);
            }
        }
        else
        {
            if (weaponIcon != null)
            {
                weaponIcon.sprite = null;
                weaponIcon.color = Color.clear;
            }

            if (weaponNameText != null)
            {
                weaponNameText.text = enhancementLevel > 0 ? $"{GetWeaponTypeText(weaponType)} +{enhancementLevel}" : "无武器";
            }

            if (weaponTypeText != null)
            {
                weaponTypeText.text = GetWeaponTypeText(weaponType);
            }
        }
    }

    /// <summary>刷新闪避冷却环形指示器</summary>
    void RefreshDash(float remaining, float total)
    {
        bool ready = remaining <= 0f;
        float pct = total > 0f ? remaining / total : 0f;

        if (dashCooldownRing != null)
        {
            // 冷却中逆时针递减：fillClockwise=false 时 fillAmount=remaining/total，随 remaining 减少而逆时针消散
            dashCooldownRing.fillAmount = Mathf.Clamp01(pct);
            dashCooldownRing.color = ready ? ringReadyColor : ringCooldownColor;
        }

        // 就绪时图标正常，冷却中变暗
        if (weaponIcon != null)
        {
            if (ready)
            {
                // 若有 sprite 则白色显示外发光感，无 sprite 保持透明
                weaponIcon.color = (weaponIcon.sprite != null) ? Color.white : Color.clear;
            }
            else
            {
                weaponIcon.color = IIPUIStyle.WeaponIconCooldownDim;
            }
        }
    }

    /// <summary>确保根 RectTransform 布局（右下角，pivot 1,0）</summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        // HUD 整体放大 1.4×：偏移 (-40,40)→(-56,56)，环形外径 ×1.4（68→95.2）
        rt.anchoredPosition = new Vector2(-56, 56);
        rt.sizeDelta = new Vector2(ringOuterSize * HudScale, ringOuterSize * HudScale);
    }

    /// <summary>初始化UI</summary>
    void InitializeUI()
    {
        if (background == null)
        {
            CreateDefaultUI();
        }
    }

    /// <summary>获取武器类型文本</summary>
    string GetWeaponTypeText(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                return "剑";
            case WeaponType.Staff:
                return "法杖";
            case WeaponType.Scythe:
                return "镰刀";
            case WeaponType.CrystalArm:
                return "水晶武装";
            default:
                return "未知";
        }
    }

    /// <summary>
    /// 创建默认UI（环形冷却指示器 + 中心武器图标 + 顶部 Space 键位 + 下方名称/类型）
    /// </summary>
    void CreateDefaultUI()
    {
        // 根容器：右下角对齐，尺寸 = 环形外径
        GameObject root = new GameObject("WeaponDashRoot");
        root.transform.SetParent(transform, false);
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0, 0);
        rootRect.anchorMax = new Vector2(1, 1);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // ── 闪避冷却环形指示器（外径68px，铺满根；空心圆环 sprite，Radial360 扫圈）──
        GameObject ringObj = new GameObject("DashCooldownRing", typeof(RectTransform), typeof(Image));
        ringObj.transform.SetParent(root.transform, false);
        dashCooldownRing = ringObj.GetComponent<Image>();
        IIPUIFactory.ApplyRing(dashCooldownRing); // 必须先赋圆环 sprite，否则 Filled Radial360 填满整个矩形成实心方块
        dashCooldownRing.color = ringReadyColor;
        dashCooldownRing.type = Image.Type.Filled;
        dashCooldownRing.fillMethod = Image.FillMethod.Radial360;
        dashCooldownRing.fillOrigin = 0; // Radial360 的 fillOrigin 0 = Bottom（从底部起扫）
        dashCooldownRing.fillClockwise = false; // 逆时针填充，fillAmount 递减时逆时针消散（符合策划"冷却中逆时针递减"）
        dashCooldownRing.fillAmount = 1f;
        RectTransform ringRect = ringObj.GetComponent<RectTransform>();
        ringRect.anchorMin = Vector2.zero;
        ringRect.anchorMax = Vector2.one;
        ringRect.offsetMin = Vector2.zero;
        ringRect.offsetMax = Vector2.zero;

        // ── 武器图标背景（正方形 56×56，居中）──
        GameObject bgObj = new GameObject("WeaponIconBg", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(root.transform, false);
        background = bgObj.GetComponent<Image>();
        background.color = backgroundColor;
        IIPUIFactory.ApplyRounded(background, true);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        // 正方形锚点：0.15~0.85，保证四边等比留白
        bgRect.anchorMin = new Vector2(0.15f, 0.15f);
        bgRect.anchorMax = new Vector2(0.85f, 0.85f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        IIPUIFactory.CreateBorder(bgObj.transform, IIPUIFactory.BorderDim, true);

        // ── 武器图标（占位淡紫，运行时由 WeaponSystem 赋 sprite）──
        GameObject iconObj = new GameObject("WeaponIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(bgObj.transform, false);
        weaponIcon = iconObj.GetComponent<Image>();
        weaponIcon.color = IIPUIStyle.WeaponIconPlaceholder;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        // ── 顶部键位提示 "Space"（10→14，1.4×）──
        dashKeyHintText = IIPUIFactory.CreateLabelAnchored("DashKeyHint", root.transform,
            "Space", 14, IIPUIStyle.TextKeyHint,
            new Vector2(0, 0.82f), new Vector2(1, 1),
            Vector2.zero, Vector2.zero, TextAnchor.UpperCenter);

        // ── 武器名称文本（根下方外部，12→17，偏移 ×1.4）──
        weaponNameText = IIPUIFactory.CreateLabelAnchored("WeaponName", transform,
            "", 17, IIPUIFactory.TextMain,
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, -25), new Vector2(0, -3), TextAnchor.UpperCenter);

        // ── 武器类型文本（名称下方，10→14，偏移 ×1.4）──
        weaponTypeText = IIPUIFactory.CreateLabelAnchored("WeaponType", transform,
            "", 14, IIPUIFactory.TextDim,
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, -45), new Vector2(0, -25), TextAnchor.UpperCenter);
    }
}
