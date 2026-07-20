using UnityEngine;
using UnityEngine.UI;

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
    public Color expBarColor = new Color(0.3f, 0.8f, 1f, 1f);

    [Tooltip("经验条背景颜色")]
    public Color expBarBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

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
        rt.sizeDelta = new Vector2(200, 60);
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
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -80);
        containerRect.sizeDelta = new Vector2(200, 60);

        GameObject iconObj = new GameObject("LevelIcon");
        iconObj.transform.SetParent(container.transform, false);
        levelIcon = iconObj.AddComponent<Image>();
        levelIcon.color = new Color(1f, 0.8f, 0.2f, 1f);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = new Vector2(40, 40);

        GameObject levelObj = new GameObject("LevelText");
        levelObj.transform.SetParent(container.transform, false);
        levelText = levelObj.AddComponent<Text>();
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.fontSize = 18;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = levelTextColor;

        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0.5f);
        levelRect.anchorMax = new Vector2(0, 0.5f);
        levelRect.pivot = new Vector2(0, 0.5f);
        levelRect.anchoredPosition = new Vector2(45, 0);
        levelRect.sizeDelta = new Vector2(60, 30);

        GameObject expBgObj = new GameObject("ExpBarBg");
        expBgObj.transform.SetParent(container.transform, false);
        Image expBg = expBgObj.AddComponent<Image>();
        expBg.color = expBarBackgroundColor;

        RectTransform expBgRect = expBgObj.GetComponent<RectTransform>();
        expBgRect.anchorMin = new Vector2(0, 0);
        expBgRect.anchorMax = new Vector2(1, 0.4f);
        expBgRect.pivot = new Vector2(0, 0);
        expBgRect.anchoredPosition = new Vector2(110, 5);
        expBgRect.sizeDelta = new Vector2(80, 20);

        GameObject expFillObj = new GameObject("ExpBarFill");
        expFillObj.transform.SetParent(expBgObj.transform, false);
        expFill = expFillObj.AddComponent<Image>();
        expFill.color = expBarColor;
        expFill.type = Image.Type.Filled;
        expFill.fillMethod = Image.FillMethod.Horizontal;

        RectTransform expFillRect = expFillObj.GetComponent<RectTransform>();
        expFillRect.anchorMin = Vector2.zero;
        expFillRect.anchorMax = Vector2.one;
        expFillRect.offsetMin = Vector2.zero;
        expFillRect.offsetMax = Vector2.zero;

        GameObject expTextObj = new GameObject("ExpText");
        expTextObj.transform.SetParent(expBgObj.transform, false);
        expText = expTextObj.AddComponent<Text>();
        expText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        expText.fontSize = 12;
        expText.alignment = TextAnchor.MiddleCenter;
        expText.color = Color.white;

        RectTransform expTextRect = expTextObj.GetComponent<RectTransform>();
        expTextRect.anchorMin = Vector2.zero;
        expTextRect.anchorMax = Vector2.one;
        expTextRect.offsetMin = Vector2.zero;
        expTextRect.offsetMax = Vector2.zero;
    }
}
