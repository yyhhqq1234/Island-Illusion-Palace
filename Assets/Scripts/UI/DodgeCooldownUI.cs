using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 闪避冷却指示器UI - 显示闪避技能的冷却状态
/// 位置：屏幕右下角，快捷栏上方
/// </summary>
public class DodgeCooldownUI : MonoBehaviour
{
    [Header("UI组件")]
    [Tooltip("冷却指示器背景")]
    public Image background;
    
    [Tooltip("冷却填充遮罩")]
    public Image cooldownFill;
    
    [Tooltip("闪避图标")]
    public Image dodgeIcon;
    
    [Tooltip("冷却文本")]
    public Text cooldownText;
    
    [Tooltip("快捷键提示文本")]
    public Text keyHintText;

    [Header("样式配置")]
    [Tooltip("指示器大小")]
    public float indicatorSize = 48f;
    
    [Tooltip("可用状态颜色")]
    public Color readyColor = new Color(0.4f, 0.8f, 0.4f, 1f);
    
    [Tooltip("冷却中颜色")]
    public Color cooldownColor = new Color(0.8f, 0.4f, 0.4f, 1f);
    
    [Tooltip("背景颜色")]
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("快捷键")]
    [Tooltip("闪避快捷键（默认Space）")]
    public KeyCode dodgeKey = KeyCode.Space;

    private PlayerController playerController;
    private float maxCooldown = 1f;
    private float lastRemaining = -1f;

    void Start()
    {
        EnsureContainerLayout();
        playerController = FindObjectOfType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("[DodgeCooldownUI] 未找到PlayerController");
        }

        InitializeUI();
        // 初值：默认就绪
        Refresh(0f, maxCooldown);
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnDashCooldownChanged += HandleDashCooldownChanged;
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnDashCooldownChanged -= HandleDashCooldownChanged;
    }

    void HandleDashCooldownChanged(float remaining, float total)
    {
        if (total > 0f) maxCooldown = total;
        Refresh(remaining, maxCooldown);
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（右下角）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20, 100);
        rt.sizeDelta = new Vector2(indicatorSize, indicatorSize);
    }

    void Update()
    {
        // 事件驱动：PlayerController 通过 OnDashCooldownChanged 广播，无需每帧轮询
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        if (background == null)
        {
            CreateDefaultUI();
        }

        // 设置快捷键提示
        if (keyHintText != null)
        {
            keyHintText.text = dodgeKey.ToString();
        }

        // 获取闪避冷却时间
        if (playerController != null)
        {
            maxCooldown = playerController.dashCooldown;
        }
    }

    /// <summary>统一刷新入口</summary>
    void Refresh(float cooldownRemaining, float total)
    {
        // 跳过无意义重复刷新（冷却中每帧都会触发，但值不同；这里仅在就绪状态过滤）
        float cooldownPercent = total > 0f ? cooldownRemaining / total : 0f;

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = cooldownPercent;

            if (cooldownRemaining <= 0)
            {
                cooldownFill.color = readyColor;
                cooldownFill.gameObject.SetActive(false);
            }
            else
            {
                cooldownFill.color = cooldownColor;
                cooldownFill.gameObject.SetActive(true);
            }
        }

        if (cooldownText != null)
        {
            if (cooldownRemaining > 0)
            {
                cooldownText.text = Mathf.CeilToInt(cooldownRemaining).ToString();
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.text = "READY";
                cooldownText.gameObject.SetActive(true);
            }
        }

        if (dodgeIcon != null)
        {
            dodgeIcon.color = (cooldownRemaining <= 0) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }

        lastRemaining = cooldownRemaining;
    }

    /// <summary>
    /// 创建默认UI
    /// </summary>
    void CreateDefaultUI()
    {
        GameObject indicatorObj = new GameObject("DodgeCooldownIndicator");
        indicatorObj.transform.SetParent(transform, false);
        
        RectTransform rect = indicatorObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-20, 100);
        rect.sizeDelta = new Vector2(indicatorSize, indicatorSize);
        
        // 背景
        background = indicatorObj.AddComponent<Image>();
        background.color = backgroundColor;
        
        // 冷却填充
        GameObject fillObj = new GameObject("CooldownFill");
        fillObj.transform.SetParent(indicatorObj.transform, false);
        cooldownFill = fillObj.AddComponent<Image>();
        cooldownFill.color = cooldownColor;
        cooldownFill.type = Image.Type.Filled;
        cooldownFill.fillMethod = Image.FillMethod.Radial360;
        cooldownFill.fillAmount = 0;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // 闪避图标
        GameObject iconObj = new GameObject("DodgeIcon");
        iconObj.transform.SetParent(indicatorObj.transform, false);
        dodgeIcon = iconObj.AddComponent<Image>();
        dodgeIcon.color = Color.white;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.2f, 0.2f);
        iconRect.anchorMax = new Vector2(0.8f, 0.8f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        // 冷却文本
        GameObject textObj = new GameObject("CooldownText");
        textObj.transform.SetParent(indicatorObj.transform, false);
        cooldownText = textObj.AddComponent<Text>();
        cooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cooldownText.fontSize = 14;
        cooldownText.alignment = TextAnchor.MiddleCenter;
        cooldownText.color = Color.white;
        cooldownText.resizeTextForBestFit = true;
        cooldownText.resizeTextMinSize = 10;
        cooldownText.resizeTextMaxSize = 16;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // 快捷键提示
        GameObject keyHintObj = new GameObject("KeyHint");
        keyHintObj.transform.SetParent(indicatorObj.transform, false);
        keyHintText = keyHintObj.AddComponent<Text>();
        keyHintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        keyHintText.fontSize = 10;
        keyHintText.alignment = TextAnchor.UpperCenter;
        keyHintText.color = new Color(1, 1, 1, 0.7f);
        RectTransform keyRect = keyHintObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0, 0.85f);
        keyRect.anchorMax = new Vector2(1, 1);
        keyRect.offsetMin = Vector2.zero;
        keyRect.offsetMax = Vector2.zero;
    }
}
