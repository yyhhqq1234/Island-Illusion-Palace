using UnityEngine;
using UnityEngine.UI;
using IIPUI;

/// <summary>
/// 玩家等级+经验条UI组件（事件驱动）
/// 订阅 GlobalEventManager.OnPlayerLevelChanged，仅在等级/经验变化时刷新，无 Update 轮询
/// </summary>
public class PlayerLevelUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("等级文本")]
    public Text levelText;

    [Tooltip("经验条填充")]
    public Image expFill;

    [Tooltip("经验值文本")]
    public Text expText;

    [Tooltip("等级图标")]
    public Image levelIcon;

    [Header("样式配置")]
    [Tooltip("经验条颜色")]
    public Color expBarColor = IIPUIStyle.AccentCyan;

    [Tooltip("经验条背景颜色")]
    public Color expBarBackgroundColor = IIPUIStyle.BarBackgroundNeutral;

    [Tooltip("等级文本颜色")]
    public Color levelTextColor = Color.white;

    private CharacterStats characterStats;
    private bool statsRefreshed = false;

    void Start()
    {
        EnsureContainerLayout();
        InitializeUI(); // 无论 characterStats 是否存在都构建默认 UI

        characterStats = FindObjectOfType<CharacterStats>();
        if (characterStats != null)
        {
            Refresh(characterStats.level, characterStats.currentExperience, characterStats.requiredExperience);
            statsRefreshed = true;
        }
        else
        {
            Debug.LogWarning("[PlayerLevelUI] Start 时未找到CharacterStats，将在Update中延迟补拉");
        }
    }

    void Update()
    {
        // 玩家预制体由 PlayerSpawnManager 生成，可能晚于 HUD Start。延迟补拉一次初值。
        if (!statsRefreshed && characterStats == null)
        {
            characterStats = FindObjectOfType<CharacterStats>();
            if (characterStats != null)
            {
                Refresh(characterStats.level, characterStats.currentExperience, characterStats.requiredExperience);
                statsRefreshed = true;
            }
        }
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnPlayerLevelChanged += HandleLevelChanged;
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnPlayerLevelChanged -= HandleLevelChanged;
    }

    void HandleLevelChanged(int level, int currentExp, int expToNext)
    {
        Refresh(level, currentExp, expToNext);
    }

    /// <summary>统一刷新入口</summary>
    void Refresh(int level, int currentExp, int expToNext)
    {
        if (levelText != null)
            levelText.text = $"Lv.{level}";

        if (expFill != null)
        {
            float expPercent = expToNext > 0 ? (float)currentExp / expToNext : 0f;
            expFill.fillAmount = Mathf.Clamp01(expPercent);
        }

        if (expText != null)
            expText.text = $"{currentExp}/{expToNext}";
    }

    /// <summary>确保容器 RectTransform 布局正确（左上角）</summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -80);
        // HUD 整体放大 1.4×：280×60 → 392×84
        rt.sizeDelta = new Vector2(392, 84); // 与 CreateDefaultUI 内部容器一致，避免溢出
    }

    /// <summary>初始化UI（缺引用时构建默认）</summary>
    void InitializeUI()
    {
        if (levelText == null || expFill == null)
        {
            CreateDefaultUI();
        }
    }

    void CreateDefaultUI()
    {
        GameObject container = new GameObject("PlayerLevelUI");
        container.transform.SetParent(transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        // 子容器相对根铺满（根已由 EnsureContainerLayout 定位到左上角 280×60），不再重复偏移
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // 等级图标（圆角金色方块 + 边框），40→56（1.4×）
        GameObject iconObj = new GameObject("LevelIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(container.transform, false);
        levelIcon = iconObj.GetComponent<Image>();
        levelIcon.color = IIPUIStyle.AccentGold;
        IIPUIFactory.ApplyRounded(levelIcon, true);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = new Vector2(56, 56);
        IIPUIFactory.CreateBorder(iconObj.transform, IIPUIFactory.BorderBright, true);
        // 图标中央 "Lv" 小字（12→17）
        IIPUIFactory.CreateLabel("IconMark", iconObj.transform, "Lv", 17, IIPUIStyle.LevelBadgeText,
            TextAnchor.MiddleCenter, FontStyle.Bold);

        // 等级文本（雅黑加粗，图标右侧，18→25）
        levelText = IIPUIFactory.CreateLabelAnchored("LevelText", container.transform,
            "", 25, levelTextColor,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(63, -21), new Vector2(147, 21), TextAnchor.MiddleLeft);
        levelText.fontStyle = FontStyle.Bold;

        // 经验条背景（圆角 + 边框），位置/厚度 ×1.4
        GameObject expBgObj = new GameObject("ExpBarBg", typeof(RectTransform), typeof(Image));
        expBgObj.transform.SetParent(container.transform, false);
        Image expBg = expBgObj.GetComponent<Image>();
        expBg.color = expBarBackgroundColor;
        IIPUIFactory.ApplyRounded(expBg, true);

        RectTransform expBgRect = expBgObj.GetComponent<RectTransform>();
        expBgRect.anchorMin = new Vector2(0, 0);
        expBgRect.anchorMax = new Vector2(1, 0.42f);
        expBgRect.pivot = new Vector2(0, 0);
        expBgRect.anchoredPosition = new Vector2(154, 7);
        expBgRect.sizeDelta = new Vector2(-154, 28);
        IIPUIFactory.CreateBorder(expBgObj.transform, IIPUIFactory.BorderDim, true);

        // 经验条填充（圆角 sprite，与底框一致）
        GameObject expFillObj = new GameObject("ExpBarFill", typeof(RectTransform), typeof(Image));
        expFillObj.transform.SetParent(expBgObj.transform, false);
        expFill = expFillObj.GetComponent<Image>();
        expFill.color = expBarColor;
        IIPUIFactory.ApplyRounded(expFill, true);
        expFill.type = Image.Type.Filled;
        expFill.fillMethod = Image.FillMethod.Horizontal;

        RectTransform expFillRect = expFillObj.GetComponent<RectTransform>();
        expFillRect.anchorMin = Vector2.zero;
        expFillRect.anchorMax = Vector2.one;
        expFillRect.offsetMin = Vector2.zero;
        expFillRect.offsetMax = Vector2.zero;

        // 经验文本（雅黑，12→17）
        expText = IIPUIFactory.CreateLabel("ExpText", expBgObj.transform, "", 17, Color.white);
    }
}
