using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IIPUI;

/// <summary>
/// 召唤物状态条UI - 显示所有召唤物的HP和状态
/// 位置：屏幕左下角，垂直排列
/// </summary>
public class SummonStatusUI : MonoBehaviour
{
    [Header("UI组件")]
    [Tooltip("状态条容器（垂直布局）")]
    public RectTransform container;
    
    [Tooltip("状态条预制体")]
    public GameObject statusPrefab;
    
    [Header("样式配置")]
    [Tooltip("状态条宽度")]
    public float barWidth = 180f;
    
    [Tooltip("状态条高度")]
    public float barHeight = 32f;
    
    [Tooltip("状态条间距")]
    public float spacing = IIPUIStyle.SpacingStandard;

    [Header("颜色配置")]
    [Tooltip("HP条背景色")]
    public Color hpBackgroundColor = IIPUIStyle.BarBackgroundNeutral;

    [Tooltip("HP条填充色")]
    public Color hpFillColor = IIPUIStyle.HealthFill;

    [Tooltip("HP条低血量警告色")]
    public Color hpLowColor = IIPUIStyle.HealthFillLow;
    
    [Tooltip("文本颜色")]
    public Color textColor = Color.white;

    private SummonSystem summonSystem;
    private List<GameObject> statusBars = new List<GameObject>();
    private Dictionary<int, SummonStatusBar> statusDict = new Dictionary<int, SummonStatusBar>();
    private int lastActiveCount = -1;

    void Start()
    {
        EnsureContainerLayout();
        summonSystem = FindObjectOfType<SummonSystem>();

        if (container == null)
        {
            CreateDefaultContainer();
        }

        if (statusPrefab == null)
        {
            Debug.LogWarning("[SummonStatusUI] statusPrefab未设置，将使用默认样式");
        }

        // 主动拉一次初值
        if (summonSystem != null)
            RebuildStatusBars();
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnSummonStatusChanged += HandleSummonStatusChanged;
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnSummonStatusChanged -= HandleSummonStatusChanged;
    }

    void HandleSummonStatusChanged(int activeCount, int maxActive, float cooldownRemaining)
    {
        // 仅当活跃数量变化时重建状态条结构；HP 细节由 Update 轻量轮询
        if (activeCount != lastActiveCount)
        {
            lastActiveCount = activeCount;
            RebuildStatusBars();
        }
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（左下角）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 100);
        rt.sizeDelta = new Vector2(200, 300);
    }

    void Update()
    {
        // 召唤物 HP 实时变化频繁，且无逐召唤物事件，保留轻量 Update 仅刷新 HP 数值（不重建结构）
        if (summonSystem == null) return;
        UpdateSummonHPOnly();
    }

    /// <summary>仅刷新现有状态条的 HP 数值（结构由事件驱动重建）</summary>
    void UpdateSummonHPOnly()
    {
        var summons = summonSystem.GetActiveSummons();
        for (int i = 0; i < statusBars.Count && i < summons.Count; i++)
        {
            if (summons[i] == null) continue;
            if (statusDict.ContainsKey(i))
                statusDict[i].UpdateStatus(summons[i]);
        }
    }

    /// <summary>根据当前活跃召唤物列表重建状态条结构</summary>
    void RebuildStatusBars()
    {
        if (summonSystem == null) return;
        var summons = summonSystem.GetActiveSummons();

        // 清理多余的状态条
        while (statusBars.Count > summons.Count)
        {
            var lastBar = statusBars[statusBars.Count - 1];
            statusBars.RemoveAt(statusBars.Count - 1);
            if (lastBar != null) Destroy(lastBar);
            // 清理字典中越界 key
            var staleKeys = new List<int>();
            foreach (var k in statusDict.Keys) if (k >= statusBars.Count) staleKeys.Add(k);
            foreach (var k in staleKeys) statusDict.Remove(k);
        }

        // 创建缺失的状态条
        for (int i = 0; i < summons.Count; i++)
        {
            if (summons[i] == null) continue;
            if (i >= statusBars.Count)
            {
                var statusBar = CreateStatusBar(summons[i], i);
                statusBars.Add(statusBar);
                statusDict[i] = statusBar.GetComponent<SummonStatusBar>();
            }
        }
    }

    /// <summary>
    /// 创建状态条
    /// </summary>
    GameObject CreateStatusBar(GameObject summon, int index)
    {
        GameObject barObj;
        
        if (statusPrefab != null)
        {
            barObj = Instantiate(statusPrefab, container);
        }
        else
        {
            barObj = CreateDefaultStatusBar();
        }
        
        barObj.name = $"SummonStatus_{index}";
        
        // 设置布局
        RectTransform rect = barObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(barWidth, barHeight);
        }
        
        // 添加或获取状态条组件
        var statusBar = barObj.GetComponent<SummonStatusBar>();
        if (statusBar == null)
        {
            statusBar = barObj.AddComponent<SummonStatusBar>();
        }
        
        statusBar.Initialize(hpBackgroundColor, hpFillColor, hpLowColor, textColor);
        
        return barObj;
    }

    /// <summary>
    /// 创建默认状态条（圆角底 + 边框 + HP背景/填充 + 名字标签）
    /// </summary>
    GameObject CreateDefaultStatusBar()
    {
        // 圆角底（铺满 VerticalLayoutGroup 分配的格子）
        GameObject barObj = new GameObject("SummonStatusBar", typeof(RectTransform), typeof(Image));
        barObj.transform.SetParent(container, false);

        Image bgImage = barObj.GetComponent<Image>();
        bgImage.color = hpBackgroundColor;
        IIPUIFactory.ApplyRounded(bgImage, true);
        IIPUIFactory.CreateBorder(barObj.transform, IIPUIFactory.BorderDim, true);

        // HP条背景（圆角内框）
        GameObject hpBgObj = new GameObject("HPBackground", typeof(RectTransform), typeof(Image));
        hpBgObj.transform.SetParent(barObj.transform, false);
        Image hpBgImage = hpBgObj.GetComponent<Image>();
        hpBgImage.color = IIPUIStyle.HealthBarInnerBackground;
        IIPUIFactory.ApplyRounded(hpBgImage, true);
        RectTransform hpBgRect = hpBgObj.GetComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0, 0);
        hpBgRect.anchorMax = new Vector2(1, 1);
        hpBgRect.offsetMin = new Vector2(4, 4);
        hpBgRect.offsetMax = new Vector2(-4, -4);

        // HP条填充（Filled 类型，避免 localScale 拉伸导致圆角变形）
        GameObject hpFillObj = new GameObject("HPFill", typeof(RectTransform), typeof(Image));
        hpFillObj.transform.SetParent(hpBgObj.transform, false);
        Image hpFillImage = hpFillObj.GetComponent<Image>();
        hpFillImage.color = hpFillColor;
        hpFillImage.type = Image.Type.Filled;
        hpFillImage.fillMethod = Image.FillMethod.Horizontal;
        hpFillImage.fillAmount = 1f;
        RectTransform hpFillRect = hpFillObj.GetComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero;
        hpFillRect.anchorMax = Vector2.one;
        hpFillRect.offsetMin = Vector2.zero;
        hpFillRect.offsetMax = Vector2.zero;

        // 名字+血量标签（雅黑）
        IIPUIFactory.CreateLabel("Text", barObj.transform, "", IIPUIStyle.FontSizeBody, textColor);

        return barObj;
    }

    /// <summary>
    /// 创建默认容器
    /// </summary>
    void CreateDefaultContainer()
    {
        GameObject containerObj = new GameObject("SummonStatusContainer");
        containerObj.transform.SetParent(transform, false);
        
        container = containerObj.AddComponent<RectTransform>();
        container.anchorMin = new Vector2(0, 0);
        container.anchorMax = new Vector2(0, 0);
        container.pivot = new Vector2(0, 0);
        container.anchoredPosition = new Vector2(20, 100);
        container.sizeDelta = new Vector2(200, 300);
        
        VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = spacing;
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        
        ContentSizeFitter csf = containerObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
}

/// <summary>
/// 单个召唤物状态条
/// </summary>
public class SummonStatusBar : MonoBehaviour
{
    private Image hpFill;
    private Text nameText;
    private Color hpFillColor;
    private Color hpLowColor;
    
    public void Initialize(Color bgColor, Color fillColor, Color lowColor, Color textColor)
    {
        hpFillColor = fillColor;
        hpLowColor = lowColor;

        var images = GetComponentsInChildren<Image>(true);
        if (images.Length >= 3)
        {
            images[0].color = bgColor; // 背景
            hpFill = images[2]; // HP填充
            hpFill.color = fillColor;
            // 确保运行时（含外部 statusPrefab）也为 Filled 类型，避免 localScale 拉伸圆角变形
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillAmount = 1f;
        }

        var texts = GetComponentsInChildren<Text>(true);
        if (texts.Length > 0)
        {
            nameText = texts[0];
            nameText.color = textColor;
        }
    }

    public void UpdateStatus(GameObject summon)
    {
        if (summon == null) return;

        var health = summon.GetComponent<HealthSystem>();
        if (health != null && hpFill != null)
        {
            float hpPercent = health.currentHealth / health.maxHealth;
            // 使用 Filled fillAmount 替代 localScale，避免圆角左右拉伸变形
            hpFill.fillAmount = hpPercent;

            // 低血量警告
            if (hpPercent < 0.3f)
            {
                hpFill.color = hpLowColor;
            }
            else
            {
                hpFill.color = hpFillColor;
            }

            // 更新文本
            if (nameText != null)
            {
                nameText.text = $"{summon.name}\n{health.currentHealth:F0}/{health.maxHealth:F0}";
            }
        }
    }
}
