using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 区域名称显示UI组件
/// 当玩家进入新区域时，在屏幕中央显示区域名称
/// 支持淡入淡出动画效果
/// </summary>
public class AreaNameUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("区域名称文本")]
    public Text areaNameText;
    
    [Tooltip("背景遮罩")]
    public Image backgroundMask;
    
    [Tooltip("副标题文本（可选）")]
    public Text subtitleText;

    [Header("动画配置")]
    [Tooltip("淡入时间")]
    public float fadeInDuration = 0.5f;
    
    [Tooltip("显示持续时间")]
    public float displayDuration = 3f;
    
    [Tooltip("淡出时间")]
    public float fadeOutDuration = 0.5f;

    [Header("样式配置")]
    [Tooltip("区域名称字体大小")]
    public int areaNameFontSize = 48;
    
    [Tooltip("副标题字体大小")]
    public int subtitleFontSize = 24;
    
    [Tooltip("文本颜色")]
    public Color textColor = Color.white;
    
    [Tooltip("背景颜色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.5f);

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;
    private string currentAreaName = "";

    void Start()
    {
        EnsureContainerLayout();
        InitializeUI();
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnMapAreaChanged += HandleMapAreaChanged;
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnMapAreaChanged -= HandleMapAreaChanged;
    }

    void HandleMapAreaChanged(string areaName)
    {
        if (string.IsNullOrEmpty(areaName)) return;
        // 同一区域不重复弹出
        if (areaName == currentAreaName && canvasGroup != null && canvasGroup.alpha > 0f) return;
        ShowAreaName(areaName);
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（屏幕中央）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800, 200);
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        // 获取或创建CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始隐藏
        canvasGroup.alpha = 0;

        // 如果UI引用为空，创建默认UI
        if (areaNameText == null)
        {
            CreateDefaultUI();
        }

        // 应用样式配置
        ApplyStyleSettings();
    }

    /// <summary>
    /// 显示区域名称
    /// </summary>
    /// <param name="areaName">区域名称</param>
    /// <param name="subtitle">副标题（可选）</param>
    public void ShowAreaName(string areaName, string subtitle = "")
    {
        // 如果正在显示，先停止当前协程
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        currentAreaName = areaName;
        
        // 更新文本
        if (areaNameText != null)
        {
            areaNameText.text = areaName;
        }
        
        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
            subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
        }
        
        // 开始显示动画
        displayCoroutine = StartCoroutine(DisplayAreaNameSequence());
    }

    /// <summary>
    /// 立即隐藏区域名称
    /// </summary>
    public void HideAreaName()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
        
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;
    }

    /// <summary>
    /// 显示区域名称序列（淡入 -> 持续显示 -> 淡出）
    /// </summary>
    IEnumerator DisplayAreaNameSequence()
    {
        gameObject.SetActive(true);

        if (canvasGroup == null) yield break;

        // 淡入
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;
        
        // 持续显示
        yield return new WaitForSecondsRealtime(displayDuration);
        
        // 淡出
        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        
        gameObject.SetActive(false);
        displayCoroutine = null;
    }

    /// <summary>
    /// 应用样式配置
    /// </summary>
    void ApplyStyleSettings()
    {
        if (areaNameText != null)
        {
            areaNameText.fontSize = areaNameFontSize;
            areaNameText.color = textColor;
        }
        
        if (subtitleText != null)
        {
            subtitleText.fontSize = subtitleFontSize;
            subtitleText.color = new Color(textColor.r, textColor.g, textColor.b, 0.8f);
        }
        
        if (backgroundMask != null)
        {
            backgroundMask.color = backgroundColor;
        }
    }

    /// <summary>
    /// 创建默认UI
    /// </summary>
    void CreateDefaultUI()
    {
        // 创建背景遮罩
        GameObject bgObj = new GameObject("BackgroundMask");
        bgObj.transform.SetParent(transform, false);
        backgroundMask = bgObj.AddComponent<Image>();
        backgroundMask.color = backgroundColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(800, 200);
        bgRect.anchoredPosition = Vector2.zero;
        
        // 创建区域名称文本
        GameObject nameObj = new GameObject("AreaNameText");
        nameObj.transform.SetParent(bgObj.transform, false);
        areaNameText = nameObj.AddComponent<Text>();
        areaNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        areaNameText.fontSize = areaNameFontSize;
        areaNameText.alignment = TextAnchor.MiddleCenter;
        areaNameText.color = textColor;
        areaNameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        areaNameText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(20, 0);
        nameRect.offsetMax = new Vector2(-20, -10);
        
        // 创建副标题文本
        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(bgObj.transform, false);
        subtitleText = subtitleObj.AddComponent<Text>();
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = new Color(textColor.r, textColor.g, textColor.b, 0.8f);
        subtitleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        subtitleText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0, 0);
        subtitleRect.anchorMax = new Vector2(1, 0.5f);
        subtitleRect.offsetMin = new Vector2(20, 10);
        subtitleRect.offsetMax = new Vector2(-20, 0);
        
        subtitleObj.SetActive(false);
    }
}
