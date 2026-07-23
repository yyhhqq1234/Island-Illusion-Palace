using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IIPUI;

/// <summary>
/// 召唤轮盘 UI —— 按住 R 显示、松开 R 确认选择。
///
/// 【历史根因修复】本组件挂在 Player 根节点上（Player.prefab），不在任何 Canvas 之下：
///   1. 旧版槽位 uGUI Image 直接建在 Player 世界坐标子节点下，无 Canvas 祖先 → uGUI 根本不渲染，轮盘永不可见；
///   2. 旧版 Awake → HideWheel() 执行 gameObject.SetActive(false)，会把整个 Player 停用
///      （仅靠 PlayerSpawnManager 生成后 SetActive(true) 兜底掩盖）。
/// 现版运行时自建 ScreenSpaceOverlay Canvas + 屏幕居中的 WheelRoot，
/// 显示/隐藏只切换 WheelRoot，绝不再碰自身 gameObject 的激活状态。
/// </summary>
public class SummonWheelUI : MonoBehaviour
{
    [Header("轮盘设置")]
    [Tooltip("轮盘半径（槽位中心到轮盘中心距离，像素）")]
    public float wheelRadius = 150f;
    [Tooltip("槽位尺寸（正方形边长，像素）")]
    public float slotSize = 80f;
    [Tooltip("槽位常态颜色")]
    public Color normalColor = IIPUIStyle.BarBackgroundNeutral;
    [Tooltip("槽位高亮颜色")]
    public Color highlightColor = IIPUIStyle.SummonWheelHighlight;
    [Tooltip("空槽颜色")]
    public Color emptyColor = IIPUIStyle.SummonWheelEmpty;

    [Header("UI组件（可空，运行时自动构建）")]
    [Tooltip("轮盘背景（历史遗留字段，运行时以自建底晕为准）")]
    public GameObject wheelBackground;
    [Tooltip("槽位父节点（为空时运行时用 WheelRoot）")]
    public Transform slotsParent;
    [Tooltip("槽位预制体（为空时运行时用工厂默认槽）")]
    public GameObject slotPrefab;
    [Tooltip("中心提示文本（为空时运行时创建）")]
    public Text centerText;

    [Header("槽位图标")]
    [Tooltip("默认槽位图标")]
    public Sprite defaultSlotIcon;

    /// <summary>
    /// 轮盘 Canvas 的排序层级。
    /// 需高于主 HUD(300)、低于系统级弹窗(500)，取 400 确保轮盘显示在 HUD 上方、弹窗下方。
    /// </summary>
    private const int WheelCanvasSortingOrder = 400;

    private SummonSystem summonSystem;
    private SummonWheelSlot[] slots = new SummonWheelSlot[3];
    private int selectedSlotIndex = -1;
    private bool isWheelOpen = false;
    private Vector2 wheelCenter;

    // 运行时自建的屏幕空间轮盘
    private Canvas wheelCanvas;
    private GameObject wheelRoot;

    private readonly Vector2[] slotPositions = new Vector2[]
    {
        new Vector2(0, 1),              // 槽位0：上（90°）
        new Vector2(-0.866f, -0.5f),    // 槽位1：左下（210°）
        new Vector2(0.866f, -0.5f)      // 槽位2：右下（330°）
    };

    void Awake()
    {
        BuildWheelCanvas();
        CreateWheelSlots();
        HideWheel();
    }

    void Start()
    {
        summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem == null)
        {
            Debug.LogError("[SummonWheelUI] 找不到SummonSystem！");
        }
    }

    void Update()
    {
        if (isWheelOpen)
        {
            UpdateWheelSelection();
        }
    }

    /// <summary>
    /// 自建 ScreenSpaceOverlay Canvas + 屏幕居中 WheelRoot。
    /// 本组件挂在 Player 上、不在任何 Canvas 下，uGUI 必须有 Canvas 祖先才会渲染。
    /// </summary>
    void BuildWheelCanvas()
    {
        // 轮盘 Canvas：覆盖层 + 高排序，保证压过 HUD 与面板
        var canvasGo = new GameObject("SummonWheelCanvas", typeof(RectTransform), typeof(Canvas));
        canvasGo.transform.SetParent(transform, false);
        wheelCanvas = canvasGo.GetComponent<Canvas>();
        wheelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        wheelCanvas.sortingOrder = WheelCanvasSortingOrder;

        // CanvasScaler：与主 HUD 一致的 ScaleWithScreenSize，参考分辨率 1920×1080（PC 标准）
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 轮盘根：屏幕正中
        var rootGo = new GameObject("WheelRoot", typeof(RectTransform));
        rootGo.transform.SetParent(canvasGo.transform, false);
        wheelRoot = rootGo;
        var rt = (RectTransform)rootGo.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        float diameter = wheelRadius * 2f + slotSize * 2f;
        rt.sizeDelta = new Vector2(diameter, diameter);

        // 深色径向底晕：压暗轮盘区域，与游戏画面分离
        var backdropGo = new GameObject("WheelBackdrop", typeof(RectTransform), typeof(Image));
        backdropGo.transform.SetParent(rt, false);
        var backdropRt = (RectTransform)backdropGo.transform;
        backdropRt.anchorMin = Vector2.zero;
        backdropRt.anchorMax = Vector2.one;
        backdropRt.offsetMin = Vector2.zero;
        backdropRt.offsetMax = Vector2.zero;
        var backdropImg = backdropGo.GetComponent<Image>();
        backdropImg.color = new Color(0.05f, 0.04f, 0.09f, 0.85f);
        backdropImg.raycastTarget = false;
        IIPUIFactory.ApplyRadialGlow(backdropImg);

        // 外圈圆环装饰（轮盘轮廓）
        var ringGo = new GameObject("WheelRing", typeof(RectTransform), typeof(Image));
        ringGo.transform.SetParent(rt, false);
        var ringRt = (RectTransform)ringGo.transform;
        ringRt.anchorMin = new Vector2(0.5f, 0.5f);
        ringRt.anchorMax = new Vector2(0.5f, 0.5f);
        ringRt.pivot = new Vector2(0.5f, 0.5f);
        ringRt.sizeDelta = new Vector2(wheelRadius * 2f + slotSize, wheelRadius * 2f + slotSize);
        var ringImg = ringGo.GetComponent<Image>();
        ringImg.color = IIPUIStyle.BorderDim;
        ringImg.raycastTarget = false;
        IIPUIFactory.ApplyRing(ringImg);

        // 中心提示文本（场景引用为空时运行时创建）
        if (centerText == null)
        {
            centerText = IIPUIFactory.CreateLabel("CenterText", rt,
                "选择召唤物", IIPUIStyle.FontSizeLabel, IIPUIStyle.TextTitle);
        }

        // 槽位父节点默认挂 WheelRoot
        if (slotsParent == null)
        {
            slotsParent = rt;
        }
    }

    void CreateWheelSlots()
    {
        if (slotsParent == null)
        {
            // 兜底：BuildWheelCanvas 已保证 slotsParent 非空，此处防御域重载等异常时序
            GameObject parent = new GameObject("SlotsParent");
            parent.transform.SetParent(wheelRoot != null ? wheelRoot.transform : transform, false);
            parent.transform.localPosition = Vector3.zero;
            slotsParent = parent.transform;
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject slotObj = slotPrefab != null
                ? Instantiate(slotPrefab, slotsParent)
                : CreateDefaultSlot();

            slotObj.name = $"Slot_{i}";
            slotObj.transform.localPosition = slotPositions[i] * wheelRadius;
            slotObj.transform.localScale = Vector3.one;

            slots[i] = new SummonWheelSlot
            {
                gameObject = slotObj,
                image = slotObj.GetComponent<Image>(),
                text = slotObj.GetComponentInChildren<Text>(),
                index = i
            };

            if (slots[i].image != null)
            {
                slots[i].image.rectTransform.sizeDelta = new Vector2(slotSize, slotSize);
            }
        }
    }

    GameObject CreateDefaultSlot()
    {
        // 圆角底 + 边框 + 中央槽位号（统一外观）
        GameObject slot = IIPUIFactory.MakeSlot("Slot", slotsParent,
            new Vector2(slotSize, slotSize), normalColor,
            centerLabel: null, keyLabel: null, border: true);

        // 中央槽位文本（雅黑）
        IIPUIFactory.CreateLabel("Text", slot.transform, "", IIPUIStyle.FontSizeBody, Color.white);
        return slot;
    }

    /// <summary>显示轮盘（只切换 WheelRoot，不碰自身 gameObject——自身是 Player 根节点）</summary>
    public void ShowWheel()
    {
        isWheelOpen = true;
        if (wheelRoot != null)
        {
            wheelRoot.SetActive(true);
        }

        if (wheelBackground != null)
        {
            wheelBackground.SetActive(true);
        }

        wheelCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        UpdateSlotDisplay();
        UpdateSlotHighlight(-1);

        if (centerText != null)
        {
            centerText.text = "选择召唤物";
        }

        Debug.Log("[SummonWheelUI] 召唤轮盘已打开");
    }

    /// <summary>隐藏轮盘（只切换 WheelRoot，不碰自身 gameObject）</summary>
    public void HideWheel()
    {
        isWheelOpen = false;
        if (wheelRoot != null)
        {
            wheelRoot.SetActive(false);
        }

        if (wheelBackground != null)
        {
            wheelBackground.SetActive(false);
        }

        selectedSlotIndex = -1;
    }

    void UpdateWheelSelection()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 direction = mousePos - wheelCenter;

        if (direction.magnitude < slotSize * 0.5f)
        {
            selectedSlotIndex = -1;
            UpdateSlotHighlight(-1);
            if (centerText != null)
            {
                centerText.text = "取消";
            }
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        int newSelection = GetSlotIndexFromAngle(angle);

        if (newSelection != selectedSlotIndex)
        {
            selectedSlotIndex = newSelection;
            UpdateSlotHighlight(selectedSlotIndex);
            UpdateCenterText(selectedSlotIndex);
        }
    }

    /// <summary>角度 → 槽位索引。三个槽位各据 120° 扇区，扇区以槽位方位角为中心（0=上90°、1=左下210°、2=右下330°）</summary>
    int GetSlotIndexFromAngle(float angle)
    {
        if (angle >= 30f && angle < 150f) return 0;   // 上方扇区（90°±60°）
        if (angle >= 150f && angle < 270f) return 1;  // 左下扇区（210°±60°）
        return 2;                                      // 右下扇区（330°±60°，含 270°~360° 与 0°~30°）
    }

    void UpdateSlotHighlight(int highlightedIndex)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].image != null)
            {
                if (summonSystem != null && i < summonSystem.battleSummons.Count && summonSystem.battleSummons[i] != null)
                {
                    slots[i].image.color = (i == highlightedIndex) ? highlightColor : normalColor;
                }
                else
                {
                    slots[i].image.color = emptyColor;
                }
            }
        }
    }

    void UpdateSlotDisplay()
    {
        if (summonSystem == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].text != null)
            {
                if (i < summonSystem.battleSummons.Count && summonSystem.battleSummons[i] != null)
                {
                    var core = summonSystem.battleSummons[i];
                    slots[i].text.text = $"{core.enemyType}\nLv.{core.level}";
                }
                else
                {
                    slots[i].text.text = "空槽位";
                }
            }
        }
    }

    void UpdateCenterText(int slotIndex)
    {
        if (centerText == null) return;

        if (slotIndex < 0)
        {
            centerText.text = "取消";
            return;
        }

        if (summonSystem != null && slotIndex < summonSystem.battleSummons.Count && summonSystem.battleSummons[slotIndex] != null)
        {
            var core = summonSystem.battleSummons[slotIndex];
            centerText.text = $"召唤\n{core.enemyType}";
        }
        else
        {
            centerText.text = "空槽位\n无法召唤";
        }
    }

    public int GetSelectedSlot()
    {
        return selectedSlotIndex;
    }

    public bool IsSlotEmpty(int slotIndex)
    {
        if (summonSystem == null) return true;

        if (slotIndex < 0 || slotIndex >= summonSystem.battleSummons.Count)
            return true;

        return summonSystem.battleSummons[slotIndex] == null;
    }

    public bool IsWheelOpen()
    {
        return isWheelOpen;
    }

    public void RefreshDisplay()
    {
        UpdateSlotDisplay();
    }
}

public class SummonWheelSlot
{
    public GameObject gameObject;
    public Image image;
    public Text text;
    public int index;
}
