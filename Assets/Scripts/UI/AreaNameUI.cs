using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using IIPUI;

/// <summary>
/// 区域名称显示UI组件
/// 当玩家进入新区域时，在屏幕顶部中央显示区域名称
/// 动画：从上方滑入(0.3s) -> 停留(3s) -> 淡出(0.5s)
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
    public float fadeInDuration = 0.3f;

    [Tooltip("显示持续时间")]
    public float displayDuration = 3f;

    [Tooltip("淡出时间")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("滑入距离（从上方多少像素滑入）")]
    public float slideInOffset = 60f;

    [Tooltip("淡出时上移距离")]
    public float fadeOutOffset = 10f;

    [Header("样式配置")]
    [Tooltip("区域名称字体大小")]
    public int areaNameFontSize = IIPUIStyle.FontSizeAreaTitle;

    [Tooltip("副标题字体大小")]
    public int subtitleFontSize = IIPUIStyle.FontSizeSubtitle;

    [Tooltip("文本颜色")]
    public Color textColor = Color.white;

    [Tooltip("背景颜色")]
    public Color backgroundColor = IIPUIStyle.OverlayDim;

    [Header("布局配置")]
    [Tooltip("距屏幕顶部的偏移（像素）")]
    public float topOffset = -80f;

    private CanvasGroup canvasGroup;
    private RectTransform rootRect;
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
    /// 确保容器RectTransform布局正确（屏幕顶部中央）
    /// </summary>
    void EnsureContainerLayout()
    {
        rootRect = GetComponent<RectTransform>();
        if (rootRect == null) return;
        // 顶部中央锚点
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0, topOffset);
        rootRect.sizeDelta = new Vector2(600, 80);
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

        if (canvasGroup != null) canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示区域名称序列（从上方滑入 -> 持续显示 -> 淡出）
    /// </summary>
    IEnumerator DisplayAreaNameSequence()
    {
        gameObject.SetActive(true);

        if (canvasGroup == null || rootRect == null) yield break;

        // 淡入+从上方滑入(0.3s)
        float elapsed = 0;
        float startY = topOffset - slideInOffset;
        float endY = topOffset;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            rootRect.anchoredPosition = new Vector2(0, Mathf.Lerp(startY, endY, t));
            yield return null;
        }
        canvasGroup.alpha = 1;
        rootRect.anchoredPosition = new Vector2(0, endY);

        // 持续显示
        yield return new WaitForSecondsRealtime(displayDuration);

        // 淡出+轻微上移(0.5s)
        elapsed = 0;
        float fadeOutStartY = topOffset;
        float fadeOutEndY = topOffset + fadeOutOffset;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            rootRect.anchoredPosition = new Vector2(0, Mathf.Lerp(fadeOutStartY, fadeOutEndY, t));
            yield return null;
        }
        canvasGroup.alpha = 0;
        rootRect.anchoredPosition = new Vector2(0, topOffset);

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
    /// 创建默认UI（圆角背景遮罩 600x80 + 边框 + 区域名/副标题，雅黑）
    /// </summary>
    void CreateDefaultUI()
    {
        // 创建圆角背景遮罩
        GameObject bgObj = new GameObject("BackgroundMask", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(transform, false);
        backgroundMask = bgObj.GetComponent<Image>();
        backgroundMask.color = backgroundColor;
        IIPUIFactory.ApplyRounded(backgroundMask, true);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(600, 80);
        bgRect.anchoredPosition = Vector2.zero;
        IIPUIFactory.CreateBorder(bgObj.transform, IIPUIFactory.BorderBright, true);

        // 区域名称文本（雅黑加粗，铺满背景，留 padding）
        areaNameText = IIPUIFactory.CreateLabelAnchored("AreaNameText", bgObj.transform,
            "", areaNameFontSize, textColor,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(20, 10), new Vector2(-20, -10), TextAnchor.MiddleCenter);
        areaNameText.fontStyle = FontStyle.Bold;

        // 副标题文本（雅黑，底部小字，默认隐藏）
        subtitleText = IIPUIFactory.CreateLabelAnchored("SubtitleText", bgObj.transform,
            "", subtitleFontSize, new Color(textColor.r, textColor.g, textColor.b, 0.8f),
            new Vector2(0, 0), new Vector2(1, 0.35f),
            new Vector2(20, 2), new Vector2(-20, 0), TextAnchor.MiddleCenter);
        subtitleText.gameObject.SetActive(false);
    }
}
