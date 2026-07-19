using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public Color normalColor = new Color(0.4f, 0.8f, 0.4f, 1f);      // 正常: 绿色
    public Color level1Color = new Color(1f, 0.9f, 0.3f, 1f);        // 一级: 黄色
    public Color level2Color = new Color(1f, 0.6f, 0.2f, 1f);        // 二级: 橙色
    public Color level3Color = new Color(1f, 0.2f, 0.2f, 1f);        // 三级: 红色
    public Color textColor = Color.white;

    [Header("晕影参数")]
    public Color vignetteColor = new Color(0.42f, 0.18f, 0.63f, 1f); // 紫色 #6B2FA0
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
        // 主动拉一次初值
        if (burdenSystem != null)
        {
            currentBurden = burdenSystem.GetCurrentBurden();
            maxBurden = burdenSystem.GetMaxBurden();
            UpdateBurdenBarDisplay();
            UpdateWarningLevel();
        }
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
        float targetRadius = 1f;

        switch (level)
        {
            case 2:
                targetAlpha = 0.4f;
                targetRadius = 0.6f;
                vignetteMask.gameObject.SetActive(true);
                break;
            case 3:
                targetAlpha = 0.7f;
                targetRadius = 0.4f;
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
