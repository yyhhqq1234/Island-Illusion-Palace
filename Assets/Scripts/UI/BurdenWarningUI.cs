using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using IIPUI;

/// <summary>
/// 负担值三级警告UI - 根据负担值显示不同级别的视觉警告
/// 一级(50-70): 黄色负担条
/// 二级(70-90): 橙色负担条 + 紫色晕影
/// 三级(90-100): 红色闪烁负担条 + 深紫晕影 + 裂纹纹理
/// </summary>
public class BurdenWarningUI : MonoBehaviour
{
    [Header("UI组件引用")]
    public Slider burdenSlider;
    public Image burdenFill;
    public Text burdenText;
    public Image burdenIcon;

    [Header("晕影效果")]
    public Image vignetteMask;
    public CanvasGroup vignetteCanvasGroup;

    [Header("裂纹效果")]
    public Image crackOverlay;
    public CanvasGroup crackCanvasGroup;

    [Header("颜色配置")]
    public Color normalColor = IIPUIStyle.HealthFill;          // 正常: 绿色
    public Color level1Color = IIPUIStyle.WarningYellow;       // 一级: 黄色
    public Color level2Color = IIPUIStyle.WarningOrange;       // 二级: 橙色
    public Color level3Color = IIPUIStyle.WarningRed;          // 三级: 红色
    public Color textColor = Color.white;

    [Header("晕影参数")]
    public Color vignetteColor = IIPUIStyle.BurdenVignette;    // 紫色 #6B2FA0
    public float vignetteFadeDuration = 0.3f;

    [Header("闪烁参数")]
    public float blinkFrequency = 2f;  // 2Hz频率
    public float blinkMinAlpha = 0.3f;
    public float blinkMaxAlpha = 1f;

    [Header("阈值配置")]
    public float level1Threshold = 50f;
    public float level2Threshold = 70f;
    public float level3Threshold = 90f;

    private BurdenSystem burdenSystem;
    private float currentBurden = 0f;
    private float maxBurden = 100f;
    private int currentWarningLevel = 0;
    private Coroutine blinkCoroutine;
    private Coroutine vignetteCoroutine;

    void Start()
    {
        EnsureContainerLayout();
        InitializeReferences();
        InitializeUI();
        EnsureBurdenBarPosition();
        // 主动拉一次初值
        if (burdenSystem != null)
        {
            currentBurden = burdenSystem.GetCurrentBurden();
            maxBurden = burdenSystem.GetMaxBurden();
            UpdateBurdenBarDisplay();
            UpdateWarningLevel();
        }
    }

    /// <summary>
    /// 强制负担条容器布局（左上角 HP/MP 正下方），免疫场景序列化的旧位置残留。
    /// 只动 BurdenBarContainer，根对象的全屏晕影/裂纹效果不受影响。
    /// </summary>
    void EnsureBurdenBarPosition()
    {
        var container = transform.Find("BurdenBarContainer") as RectTransform;
        if (container == null) return;
        // HUD 纵向堆叠：HP/MP 占 y-20~-84，间距 8 → 负担条 y=-92~-116
        container.anchorMin = new Vector2(0, 1);
        container.anchorMax = new Vector2(0, 1);
        container.pivot = new Vector2(0, 1);
        container.anchoredPosition = new Vector2(20, -92);
        container.sizeDelta = new Vector2(280, 24);
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnBurdenChanged += HandleBurdenChanged;
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnBurdenChanged -= HandleBurdenChanged;
    }

    void HandleBurdenChanged(float burden)
    {
        currentBurden = burden;
        if (burdenSystem != null) maxBurden = burdenSystem.GetMaxBurden();
        UpdateBurdenBarDisplay();
        UpdateWarningLevel();
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（全屏覆盖）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    void InitializeReferences()
    {
        burdenSystem = FindObjectOfType<BurdenSystem>();
        
        if (burdenSystem == null)
        {
            Debug.LogWarning("[BurdenWarningUI] 未找到BurdenSystem组件");
        }
    }

    void InitializeUI()
    {
        // 字段全为空时构建默认 UI（参考其他 HUD 的 CreateDefaultUI 模式）
        if (burdenSlider == null)
        {
            CreateDefaultUI();
        }

        // 初始化晕影遮罩
        if (vignetteMask != null)
        {
            if (vignetteCanvasGroup == null)
            {
                vignetteCanvasGroup = vignetteMask.GetComponent<CanvasGroup>();
                if (vignetteCanvasGroup == null)
                {
                    vignetteCanvasGroup = vignetteMask.gameObject.AddComponent<CanvasGroup>();
                }
            }
            vignetteMask.color = vignetteColor;
            vignetteCanvasGroup.alpha = 0f;
            vignetteMask.gameObject.SetActive(false);
        }

        // 初始化裂纹覆盖层
        if (crackOverlay != null)
        {
            if (crackCanvasGroup == null)
            {
                crackCanvasGroup = crackOverlay.GetComponent<CanvasGroup>();
                if (crackCanvasGroup == null)
                {
                    crackCanvasGroup = crackOverlay.gameObject.AddComponent<CanvasGroup>();
                }
            }
            crackCanvasGroup.alpha = 0f;
            crackOverlay.gameObject.SetActive(false);
        }

        // 初始化负担条
        if (burdenFill != null)
        {
            burdenFill.color = normalColor;
        }
    }

    /// <summary>构建默认负担条 UI（圆角底 + 边框 + 图标 + Slider + 标签/数值）</summary>
    void CreateDefaultUI()
    {
        // 负担条容器：左上角 HP/MP 正下方（HUD 纵向堆叠：HP/MP 占 y-20~-84，间距 8 → 负担条 y=-92~-116）
        GameObject container = new GameObject("BurdenBarContainer");
        container.transform.SetParent(transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -92);
        containerRect.sizeDelta = new Vector2(280, 24);

        // "负担"标签（雅黑，最左侧）
        IIPUIFactory.CreateLabelAnchored("BurdenLabel", container.transform,
            "负担", IIPUIStyle.FontSizeSmall, IIPUIFactory.TextDim,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, -10), new Vector2(40, 10), TextAnchor.MiddleLeft);

        // 背景图标（圆角紫色方块 + 边框）
        GameObject iconObj = new GameObject("BurdenIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(container.transform, false);
        burdenIcon = iconObj.GetComponent<Image>();
        burdenIcon.color = IIPUIStyle.BurdenIcon;
        IIPUIFactory.ApplyRounded(burdenIcon, true);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(44, 0);
        iconRect.sizeDelta = new Vector2(20, 20);
        IIPUIFactory.CreateBorder(iconObj.transform, IIPUIFactory.BorderBright, true);

        // Slider
        GameObject sliderObj = new GameObject("BurdenSlider");
        sliderObj.transform.SetParent(container.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(0, 0.5f);
        sliderRect.offsetMin = new Vector2(70, 0);
        sliderRect.offsetMax = new Vector2(-60, 0);
        sliderRect.anchoredPosition = new Vector2(70, 0);
        sliderRect.sizeDelta = new Vector2(0, 20);

        burdenSlider = sliderObj.AddComponent<Slider>();
        burdenSlider.minValue = 0f;
        burdenSlider.maxValue = 100f;
        burdenSlider.value = 0f;
        burdenSlider.interactable = false;

        // Slider 子结构: Background / Fill Area / Fill（背景圆角+边框）
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = IIPUIStyle.BarBackgroundNeutral;
        IIPUIFactory.ApplyRounded(bgImg, true);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        IIPUIFactory.CreateBorder(sliderObj.transform, IIPUIFactory.BorderDim, true);

        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2, 2);
        fillAreaRect.offsetMax = new Vector2(-2, -2);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        burdenFill = fillObj.GetComponent<Image>();
        burdenFill.color = normalColor;
        IIPUIFactory.ApplyRounded(burdenFill, true); // 圆角 sprite，满条末端与底框一致
        burdenFill.type = Image.Type.Filled;
        burdenFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        burdenSlider.fillRect = fillRect;
        burdenSlider.targetGraphic = null;

        // 数值文本（雅黑，右侧）
        burdenText = IIPUIFactory.CreateLabelAnchored("BurdenText", container.transform,
            "0/100", IIPUIStyle.FontSizeSmall, textColor,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-56, -10), new Vector2(-2, 10), TextAnchor.MiddleRight);
    }

    void Update()
    {
        // 事件驱动：BurdenSystem 通过 OnBurdenChanged 广播，无需每帧轮询
        // 仅三级警告闪烁由协程持续驱动
    }

    void UpdateBurdenBarDisplay()
    {
        if (burdenSlider != null)
        {
            burdenSlider.value = currentBurden;
            burdenSlider.maxValue = maxBurden;
        }

        if (burdenText != null)
        {
            burdenText.text = $"{currentBurden:F0}/{maxBurden:F0}";
        }
    }

    void UpdateWarningLevel()
    {
        int newLevel = CalculateWarningLevel();

        if (newLevel != currentWarningLevel)
        {
            OnWarningLevelChanged(newLevel);
            currentWarningLevel = newLevel;
        }

        // 三级警告的闪烁效果
        if (currentWarningLevel == 3)
        {
            UpdateBlinkEffect();
        }
    }

    int CalculateWarningLevel()
    {
        if (currentBurden >= level3Threshold) return 3;
        if (currentBurden >= level2Threshold) return 2;
        if (currentBurden >= level1Threshold) return 1;
        return 0;
    }

    void OnWarningLevelChanged(int newLevel)
    {
        Debug.Log($"[BurdenWarningUI] 警告等级变化: {currentWarningLevel} -> {newLevel}");

        // 停止旧的闪烁效果
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // 更新负担条颜色
        UpdateBurdenBarColor(newLevel);

        // 更新晕影效果
        UpdateVignetteEffect(newLevel);

        // 更新裂纹效果
        UpdateCrackEffect(newLevel);

        // 启动新的闪烁效果（仅三级）
        if (newLevel == 3)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
        else
        {
            // 确保三级以下时负担条不闪烁
            if (burdenFill != null)
            {
                var color = burdenFill.color;
                color.a = 1f;
                burdenFill.color = color;
            }
        }
    }

    void UpdateBurdenBarColor(int level)
    {
        if (burdenFill == null) return;

        Color targetColor;
        switch (level)
        {
            case 1:
                targetColor = level1Color;
                break;
            case 2:
                targetColor = level2Color;
                break;
            case 3:
                targetColor = level3Color;
                break;
            default:
                targetColor = normalColor;
                break;
        }

        burdenFill.color = targetColor;

        // 二级以上文字变色
        if (burdenText != null)
        {
            if (level >= 2)
            {
                burdenText.color = level2Color;
                burdenText.fontStyle = FontStyle.Bold;
            }
            else
            {
                burdenText.color = textColor;
                burdenText.fontStyle = FontStyle.Normal;
            }
        }
    }

    void UpdateVignetteEffect(int level)
    {
        if (vignetteMask == null || vignetteCanvasGroup == null) return;

        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }

        float targetAlpha = 0f;

        switch (level)
        {
            case 2:
                targetAlpha = 0.4f;
                vignetteMask.gameObject.SetActive(true);
                break;
            case 3:
                targetAlpha = 0.7f;
                vignetteMask.gameObject.SetActive(true);
                break;
            default:
                vignetteMask.gameObject.SetActive(false);
                return;
        }

        vignetteCoroutine = StartCoroutine(FadeVignette(targetAlpha, vignetteFadeDuration));
    }

    IEnumerator FadeVignette(float targetAlpha, float duration)
    {
        float startAlpha = vignetteCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            vignetteCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        vignetteCanvasGroup.alpha = targetAlpha;
        vignetteCoroutine = null;
    }

    void UpdateCrackEffect(int level)
    {
        if (crackOverlay == null || crackCanvasGroup == null) return;

        if (level >= 3)
        {
            crackOverlay.gameObject.SetActive(true);
            // 裂纹不透明度与负担值联动: 90时0%, 100时60%
            float crackAlpha = Mathf.InverseLerp(level3Threshold, maxBurden, currentBurden) * 0.6f;
            crackCanvasGroup.alpha = crackAlpha;
        }
        else
        {
            crackOverlay.gameObject.SetActive(false);
            crackCanvasGroup.alpha = 0f;
        }
    }

    IEnumerator BlinkEffect()
    {
        float blinkDuration = 1f / blinkFrequency;
        float halfDuration = blinkDuration / 2f;

        while (currentWarningLevel == 3)
        {
            // 淡出
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float alpha = Mathf.Lerp(blinkMaxAlpha, blinkMinAlpha, t);
                
                if (burdenFill != null)
                {
                    var color = burdenFill.color;
                    color.a = alpha;
                    burdenFill.color = color;
                }
                yield return null;
            }

            // 淡入
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float alpha = Mathf.Lerp(blinkMinAlpha, blinkMaxAlpha, t);
                
                if (burdenFill != null)
                {
                    var color = burdenFill.color;
                    color.a = alpha;
                    burdenFill.color = color;
                }
                yield return null;
            }
        }

        blinkCoroutine = null;
    }

    void UpdateBlinkEffect()
    {
        // 闪烁效果通过协程实现，这里仅作为占位
        // 实际的闪烁逻辑在BlinkEffect协程中
    }

    /// <summary>
    /// 手动设置负担值（用于测试）
    /// </summary>
    public void SetBurdenValue(float value)
    {
        if (burdenSystem != null)
        {
            burdenSystem.SetBurden(value);
        }
    }

    /// <summary>
    /// 获取当前警告等级
    /// </summary>
    public int GetCurrentWarningLevel()
    {
        return currentWarningLevel;
    }

    void OnDestroy()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }
    }
}
