using UnityEngine;
using UnityEngine.UI;
using IIPUI;

/// <summary>
/// 玩家等级+经验条UI组件（事件驱动）
/// 订阅 GlobalEventManager.OnPlayerLevelChanged，仅在等级/经验变化时刷新，无 Update 轮询。
/// 位置：屏幕左上角 HUD 纵向堆叠第三层（HP/MP → 负担 → 经验）。
/// 显隐：平时通过 CanvasGroup 隐藏（根始终激活，事件订阅不受影响）；
/// 仅当经验实际增加或升级时显示，停止涨经验约 3 秒（实时）后自动隐藏。
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

    [Header("经验条显隐")]
    [Tooltip("获得经验后经验条保持显示的时长（秒），超时自动隐藏")]
    public float expVisibleDuration = 3f;

    private CharacterStats characterStats;
    private bool statsRefreshed = false;

    // 显隐控制（CanvasGroup alpha，不禁用 GameObject，保证事件订阅存活）
    private CanvasGroup canvasGroup;
    private Coroutine hideCoroutine;
    private bool baselineSet = false; // 是否已建立经验基线（启动广播不触发显示）
    private int lastLevel = -1;
    private int lastExp = -1;

    void Start()
    {
        EnsureContainerLayout();
        InitializeUI(); // 无论 characterStats 是否存在都构建默认 UI

        characterStats = FindObjectOfType<CharacterStats>();
        if (characterStats != null)
        {
            Refresh(characterStats.level, characterStats.currentExperience, characterStats.requiredExperience);
            SetBaseline(characterStats.level, characterStats.currentExperience);
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
                SetBaseline(characterStats.level, characterStats.currentExperience);
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

        // 禁用即复位显隐状态：防止 3 秒倒计时内被禁用后再启用时经验条常显
        hideCoroutine = null;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    /// <summary>建立经验基线：启动/换场景的初值广播不触发经验条显示</summary>
    void SetBaseline(int level, int exp)
    {
        baselineSet = true;
        lastLevel = level;
        lastExp = exp;
    }

    void HandleLevelChanged(int level, int currentExp, int expToNext)
    {
        Refresh(level, currentExp, expToNext);

        // 首次事件 = CharacterStats 启动广播，仅建立基线不显示
        if (!baselineSet)
        {
            SetBaseline(level, currentExp);
            return;
        }

        // 仅"经验实际增加"或"升级"才显示（数值不变的重复广播不闪显）
        bool leveledUp = level > lastLevel;
        bool expGained = level == lastLevel && currentExp > lastExp;
        lastLevel = level;
        lastExp = currentExp;
        if (!leveledUp && !expGained) return;

        ShowExpBar();
    }

    /// <summary>显示经验条并启动自动隐藏计时（重复获得经验会重置计时）</summary>
    void ShowExpBar()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 1f;
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>实时计时隐藏：背包/炼金面板暂停（timeScale=0）期间也正常倒数</summary>
    System.Collections.IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(expVisibleDuration);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        hideCoroutine = null;
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

    /// <summary>确保容器 RectTransform 布局正确（左上角）+ CanvasGroup 显隐组件（默认隐藏）</summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        // HUD 纵向堆叠：HP/MP(y-20~-84) → 负担(y-92~-116) → 经验(顶边 y=-124)，左边距统一 20
        rt.anchoredPosition = new Vector2(20, -124);
        // HUD 整体放大 1.4×：280×60 → 392×84
        rt.sizeDelta = new Vector2(392, 84); // 与 CreateDefaultUI 内部容器一致，避免溢出

        // CanvasGroup 控制显隐（不用 SetActive，避免 OnDisable 退订事件）
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // 平时隐藏，获得经验时才显示
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
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
