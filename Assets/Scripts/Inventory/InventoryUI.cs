using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using GameSystems;
using IIPUI;

/// <summary>
/// 背包UI系统 - 手绘美术底图 + 程序对齐槽位（三栏同屏 + 点击详情）
///
/// 【架构】面板布局 100% 运行时代码构建（免疫场景序列化问题，如 scale=0 顽疾）：
///   1. 背包界面.png 整图做底（Resources/UI/Art/InventoryPanel_BG）
///   2. 72 个交互槽位按 IIPArtLayout（Python 从底图像素识别的网格几何）逐格对齐
///   3. 空槽零覆盖（透出底图手绘格），有物品才叠加 图标/数量/稀有度晕
///   4. 悬停=灵魂紫辉光，选中=亮紫+放大，稀有度=绿/紫/金底晕
///
/// 【交互】三栏固定路由：武器(剑) / 消耗品(烧瓶) / 材料+记忆碎片(右)
///   单击槽位 → 详情面板（材料信息界面.png 银丝框做装饰）；消耗品→使用，武器→装备
///   悬停 → Tooltip 跟随鼠标；点击空白/ESC → 先关详情再关背包
///
/// 【对外 API 保持不变】ShowInventory / HideInventory / IsInventoryOpen /
///   UpdateInventoryUI / GetSelectedItem / inventoryPanel 字段
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("背包系统引用")]
    [Tooltip("背包数据系统，为空时运行时自动查找")]
    public InventorySystem inventorySystem;

    [Header("面板根（场景引用，内部布局由代码重建）")]
    [Tooltip("场景中的 InventoryPanel 节点，仅保留根引用，子节点全部运行时重建")]
    public GameObject inventoryPanel;

    [Header("提示框防抖设置")]
    [Tooltip("鼠标移出槽位后 Tooltip 延迟隐藏秒数，防止抖动")]
    public float tooltipHideDelay = 0.08f;

    [Header("图标数据源")]
    [Tooltip("ItemSlot预制体（携带53项物品图标数据库），仅用于读取图标，不会实例化")]
    public GameObject itemSlotPrefab;

    // ═══════════════════════════════════════════
    // 常量
    // ═══════════════════════════════════════════

    /// <summary>面板尺寸（与底图 960×540 同 16:9 比例，1.5×）</summary>
    static readonly Vector2 PanelSize = new Vector2(1440f, 810f);
    const string ArtBgPath = "UI/Art/InventoryPanel_BG";
    const string TooltipFramePath = "UI/Art/Tooltip_Frame";

    // ═══════════════════════════════════════════
    // 运行时构建的节点引用
    // ═══════════════════════════════════════════

    class ArtSlot
    {
        public GameObject root;
        public Image glow;        // 悬停/选中辉光
        public Image rarityGlow;  // 稀有度底晕
        public Image icon;
        public Text count;
        public InventoryItem item;
    }

    readonly List<ArtSlot> weaponSlots = new List<ArtSlot>();
    readonly List<ArtSlot> consumableSlots = new List<ArtSlot>();
    readonly List<ArtSlot> materialSlots = new List<ArtSlot>();

    // 三栏滚动网格（滚轮上下滚动，行数随物品数量动态增长，替代旧 +N 溢出标记）
    IIPUI.ArtScrollGrid weaponGrid, consumableGrid, materialGrid;

    // 详情面板
    GameObject detailPanel;
    Image detailIcon;
    Text detailName, detailRarity, detailType, detailQuantity, detailDescription;
    Button useBtn, equipBtn;
    Text useBtnText, equipBtnText;

    // Tooltip
    GameObject tooltip;
    Text tooltipName, tooltipMeta, tooltipDesc;
    RectTransform tooltipRect;

    // 底部信息栏
    Text soulCurrencyText, materialCountText, burdenText;

    // 空栏提示（某栏无物品时栏中央灰字提示）
    GameObject weaponEmptyHint, consumableEmptyHint, materialEmptyHint;

    // 状态
    InventoryItem selectedItem;
    InventoryItem currentHoveredItem;
    Coroutine hideTooltipCoroutine;
    bool isTooltipVisible;
    bool isBuilt;

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    void Start()
    {
        if (inventorySystem == null)
            inventorySystem = FindObjectOfType<InventorySystem>();

        IIPUI.IIPIconLibrary.SeedFromPrefab(itemSlotPrefab);

        BuildArtPanel();
        UpdateInventoryUI();
        UpdateBottomInfoBar();
        HideInventory();

        Debug.Log("[InventoryUI] 背包UI初始化完成（美术底图 + 程序对齐槽位，3栏×24格+滚轮增行）");
    }

    void Update()
    {
        // I键切换背包已由 PlayerController 统一分发，此处不再监听

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (detailPanel != null && detailPanel.activeSelf)
            {
                HideItemDetailPanel();
            }
            else if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                HideInventory();
            }
        }

        // Tooltip 跟随鼠标
        if (isTooltipVisible && tooltipRect != null)
            UpdateTooltipPosition();
    }

    // ═══════════════════════════════════════════
    // 对外 API（保持不变）
    // ═══════════════════════════════════════════

    public void ShowInventory()
    {
        if (inventoryPanel == null) return;
        if (!isBuilt) BuildArtPanel(); // 域重载/启动顺序异常时保底构建

        // 与炼金面板互斥（两个全屏面板不可同屏）
        var alchemyUI = FindObjectOfType<AlchemyUI>();
        if (alchemyUI != null && alchemyUI.IsAlchemyPanelOpen())
            alchemyUI.HideAlchemyPanel();

        inventoryPanel.SetActive(true);
        UpdateInventoryUI();
        UpdateBottomInfoBar();

        // 重开面板回到各栏滚动顶部
        if (weaponGrid != null) weaponGrid.ResetToTop();
        if (consumableGrid != null) consumableGrid.ResetToTop();
        if (materialGrid != null) materialGrid.ResetToTop();

        Time.timeScale = 0f; // 暂停游戏
    }

    public void HideInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
        Time.timeScale = 1f; // 恢复游戏

        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        currentHoveredItem = null;
        isTooltipVisible = false;

        HideTooltipImmediate();
        HideItemDetailPanel();
    }

    public bool IsInventoryOpen()
    {
        return inventoryPanel != null && inventoryPanel.activeSelf;
    }

    public InventoryItem GetSelectedItem()
    {
        return selectedItem;
    }

    /// <summary>刷新三栏物品显示（由背包系统变更或外部脚本调用）</summary>
    public void UpdateInventoryUI()
    {
        if (inventorySystem == null || !isBuilt) return;

        FillSection(weaponSlots, inventorySystem.GetAllWeapons(), weaponGrid, "Weapon");
        var consumablesAndOthers = inventorySystem.GetAllPotions();
        FillSection(consumableSlots, consumablesAndOthers, consumableGrid, "Consumable");

        // 材料栏：材料优先，记忆碎片填充剩余槽位
        var matsAndFrags = new List<InventoryItem>(inventorySystem.GetAllMaterials());
        matsAndFrags.AddRange(inventorySystem.GetAllMemoryFragments());
        FillSection(materialSlots, matsAndFrags, materialGrid, "Material");
    }

    // ═══════════════════════════════════════════
    // 面板构建（运行时，100% 代码）
    // ═══════════════════════════════════════════

    void BuildArtPanel()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("[InventoryUI] inventoryPanel 未设置，无法构建背包面板");
            return;
        }
        if (isBuilt) return;

        // 1) 重置面板根（根治场景序列化 scale=0 / 锚点漂移）
        var panelRT = (RectTransform)inventoryPanel.transform;
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = PanelSize;
        panelRT.localScale = Vector3.one;

        // 1.5) 剥掉面板根本身的底色 Image（旧序列化的纯色底会从美术底图透明区透出）
        var rootImg = inventoryPanel.GetComponent<Image>();
        if (rootImg != null) rootImg.enabled = false;

        // 2) 清空旧子树（旧 prefab 遗留的 TitleBar/FilterBar/...）
        for (int i = panelRT.childCount - 1; i >= 0; i--)
            Destroy(panelRT.GetChild(i).gameObject);

        // 3) 美术底图（整图做底，拦截空白处点击用于关闭详情）
        var bgGo = new GameObject("ArtBG", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(panelRT, false);
        var bgImg = bgGo.GetComponent<Image>();
        bgImg.sprite = Resources.Load<Sprite>(ArtBgPath);
        bgImg.type = Image.Type.Simple;
        bgImg.preserveAspect = false; // 面板与底图同 16:9，直接铺满
        bgImg.color = Color.white;
        bgImg.raycastTarget = true;
        var bgRT = (RectTransform)bgGo.transform;
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        var bgBtn = bgGo.AddComponent<Button>();
        bgBtn.transition = Selectable.Transition.None;
        bgBtn.onClick.AddListener(HideItemDetailPanel);

        // 4) 三栏槽位（ScrollRect 滚动区：初始可见行，槽位池按需增行）
        weaponGrid = BuildSection(weaponSlots, IIPArtLayout.InvWeapons, "Weapon");
        consumableGrid = BuildSection(consumableSlots, IIPArtLayout.InvConsumables, "Consumable");
        materialGrid = BuildSection(materialSlots, IIPArtLayout.InvMaterials, "Material");

        // 4.5) 栏目标签 + AccentGold 下划线（三栏同屏无 Tab，用文字标签替代图标猜测，提升可发现性）
        BuildSectionHeader(panelRT, weaponGrid, "武器");
        BuildSectionHeader(panelRT, consumableGrid, "消耗品");
        BuildSectionHeader(panelRT, materialGrid, "材料");

        // 4.6) 空栏提示（某栏无物品时栏中央灰字，默认隐藏）
        weaponEmptyHint = BuildEmptyHint(panelRT, weaponGrid, "尚未获得武器");
        consumableEmptyHint = BuildEmptyHint(panelRT, consumableGrid, "尚未获得消耗品");
        materialEmptyHint = BuildEmptyHint(panelRT, materialGrid, "尚未获得材料");

        // 5) 底部信息栏（面板下缘外侧）
        BuildBottomInfoBar(panelRT);

        // 6) 详情面板（银丝框装饰，默认隐藏）
        BuildDetailPanel(panelRT);

        // 7) Tooltip（最后创建 = 渲染最上层）
        BuildTooltip(panelRT);

        // 8) 右上角 X 关闭按钮（点击 = ESC 语义：先关详情，再关背包）
        BuildCloseButton(panelRT);

        isBuilt = true;
    }

    /// <summary>栏目标签：网格正上方文字 + AccentGold 下划线（三栏同屏无 Tab 选中态，用常驻标识代替）</summary>
    void BuildSectionHeader(RectTransform panelRT, IIPUI.ArtScrollGrid sg, string title)
    {
        Vector2 center = sg.Viewport.anchoredPosition;
        float topY = center.y + sg.Viewport.sizeDelta.y * 0.5f;

        var label = IIPUIFactory.CreateLabelAnchored($"Header_{title}", panelRT, title,
            IIPUIStyle.FontSizeButton, IIPUIStyle.TextTitle,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(center.x - 70f, topY + 4f), new Vector2(center.x + 70f, topY + 30f),
            TextAnchor.MiddleCenter);
        label.fontStyle = FontStyle.Bold;

        // 金色下划线（宽 56px 高 3px，贴标签底部）
        var barGo = new GameObject($"HeaderBar_{title}", typeof(RectTransform), typeof(Image));
        barGo.transform.SetParent(panelRT, false);
        var barRt = (RectTransform)barGo.transform;
        barRt.anchorMin = new Vector2(0.5f, 0.5f);
        barRt.anchorMax = new Vector2(0.5f, 0.5f);
        barRt.pivot = new Vector2(0.5f, 0.5f);
        barRt.anchoredPosition = new Vector2(center.x, topY + 2f);
        barRt.sizeDelta = new Vector2(56f, 3f);
        var barImg = barGo.GetComponent<Image>();
        barImg.color = IIPUIStyle.AccentGold;
        barImg.raycastTarget = false;
        IIPUIFactory.ApplyRounded(barImg, true);
    }

    /// <summary>空栏提示：栏中央灰色文字（挂面板层，不随滚动区滚动；默认隐藏）</summary>
    GameObject BuildEmptyHint(RectTransform panelRT, IIPUI.ArtScrollGrid sg, string hintText)
    {
        Vector2 center = sg.Viewport.anchoredPosition;
        var label = IIPUIFactory.CreateLabelAnchored($"EmptyHint_{hintText}", panelRT, hintText,
            IIPUIStyle.FontSizeLabel, IIPUIStyle.TextSecondary,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(center.x - 140f, center.y - 20f), new Vector2(center.x + 140f, center.y + 20f),
            TextAnchor.MiddleCenter);
        label.gameObject.SetActive(false);
        return label.gameObject;
    }

    /// <summary>右上角 X 关闭按钮（点击 = ESC 语义：详情打开先关详情，否则关背包）</summary>
    void BuildCloseButton(RectTransform panelRT)
    {
        var go = new GameObject("CloseButton", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(panelRT, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-12f, -12f);
        rt.sizeDelta = new Vector2(40f, 40f);
        var img = go.GetComponent<Image>();
        img.color = IIPUIStyle.ButtonNormal;
        IIPUIFactory.ApplyRounded(img, true);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.None;
        IIPUIFactory.ApplyHover(go, IIPUIStyle.ButtonHover, 1.08f);
        IIPUIFactory.CreateLabel("X", rt, "X", IIPUIStyle.FontSizeButton, IIPUIStyle.TextTitle);
        btn.onClick.AddListener(() =>
        {
            IIPBootstrap.Audio?.PlayClick();
            if (detailPanel != null && detailPanel.activeSelf)
                HideItemDetailPanel(); // 与 ESC 一致：先关详情
            else
                HideInventory();
        });
    }

    /// <summary>创建一栏滚动网格 + 可见行槽位（后续刷新时按需增行）</summary>
    IIPUI.ArtScrollGrid BuildSection(List<ArtSlot> slots, IIPArtLayout.Grid grid, string sectionName)
    {
        // 间隙点击转发给"关闭详情"，与点击面板空白语义一致
        var sg = new IIPUI.ArtScrollGrid(inventoryPanel.transform, sectionName, grid,
            IIPArtLayout.InvW, IIPArtLayout.InvH, PanelSize, HideItemDetailPanel);
        for (int row = 0; row < grid.rows; row++)
            for (int col = 0; col < grid.cols; col++)
                slots.Add(MakeSlot(sg, sectionName, row * grid.cols + col, col, row));
        return sg;
    }

    /// <summary>创建单个交互槽位（空态零视觉覆盖，透出底图手绘格；挂到滚动 Content 下）</summary>
    ArtSlot MakeSlot(IIPUI.ArtScrollGrid sg, string sectionName, int index, int col, int row)
    {
        var slot = new ArtSlot();
        Vector2 cellSize = sg.CellSize;

        // 根：透明 Image 仅用于射线接收
        var root = new GameObject($"Slot_{sectionName}_{index}", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(sg.Content, false);
        var rootRT = (RectTransform)root.transform;
        rootRT.anchorMin = new Vector2(0.5f, 1f); // 顶锚定：Content 增高时首行位置不变
        rootRT.anchorMax = new Vector2(0.5f, 1f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = sg.GetCellAnchoredPos(col, row);
        rootRT.sizeDelta = cellSize * 0.96f; // 略小于手绘格，避免遮挡格线
        var rootImg = root.GetComponent<Image>();
        rootImg.color = new Color(1f, 1f, 1f, 0f);
        rootImg.raycastTarget = true;
        slot.root = root;

        // 稀有度底晕（默认隐藏）
        slot.rarityGlow = MakeGlow("RarityGlow", rootRT, cellSize * 1.00f, new Color(1f, 1f, 1f, 0f));

        // 悬停/选中辉光（默认隐藏，灵魂紫）
        slot.glow = MakeGlow("HoverGlow", rootRT, cellSize * 1.10f, new Color(1f, 1f, 1f, 0f));

        // 图标（70% 槽位，等比）
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(rootRT, false);
        var iconRT = (RectTransform)iconGo.transform;
        iconRT.anchorMin = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.pivot = new Vector2(0.5f, 0.5f);
        iconRT.sizeDelta = cellSize * 0.70f;
        slot.icon = iconGo.GetComponent<Image>();
        slot.icon.preserveAspect = true;
        slot.icon.color = Color.clear;
        slot.icon.raycastTarget = false;

        // 数量角标（右下）
        slot.count = IIPUIFactory.CreateLabelAnchored("Count", rootRT, "", 18, IIPUIStyle.TextKey,
            new Vector2(0.55f, 0f), new Vector2(1f, 0.38f),
            new Vector2(0f, -2f), new Vector2(-4f, 0f), TextAnchor.MiddleRight);

        // 交互
        var btn = root.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.targetGraphic = rootImg;
        var captured = slot;
        btn.onClick.AddListener(() => OnSlotClick(captured));

        var trigger = root.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => OnSlotHoverEnter(captured));
        trigger.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => OnSlotHoverExit(captured));
        trigger.triggers.Add(exit);

        return slot;
    }

    /// <summary>创建圆角辉光层（铺满槽位，初始透明）</summary>
    Image MakeGlow(string name, RectTransform parent, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        IIPUIFactory.ApplyRounded(img, true);
        return img;
    }

    /// <summary>底部信息栏：灵魂 | 材料 | 负重（面板下缘外侧悬浮条）</summary>
    void BuildBottomInfoBar(RectTransform panelRT)
    {
        var bar = new GameObject("BottomInfoBar", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(panelRT, false);
        var rt = (RectTransform)bar.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -PanelSize.y * 0.5f - 26f);
        rt.sizeDelta = new Vector2(620f, 38f);
        var img = bar.GetComponent<Image>();
        img.color = IIPUIStyle.ContentBackground;
        img.raycastTarget = false;
        IIPUIFactory.ApplyRounded(img, true);

        soulCurrencyText = MakeInfoLabel("Soul", rt, -300f, 200f, IIPUIStyle.AccentCyan);
        materialCountText = MakeInfoLabel("Material", rt, -90f, 190f, IIPUIStyle.TextPrimary);
        burdenText = MakeInfoLabel("Burden", rt, 110f, 200f, IIPUIStyle.AccentGold);
    }

    Text MakeInfoLabel(string name, RectTransform parent, float x, float w, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, 0f);
        rt.sizeDelta = new Vector2(w, 30f);
        var t = go.GetComponent<Text>();
        t.font = IIPUIFont.Get();
        t.fontSize = IIPUIStyle.FontSizeBody;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    /// <summary>详情面板：深色圆角底 + 材料信息界面.png 银丝框装饰</summary>
    void BuildDetailPanel(RectTransform panelRT)
    {
        detailPanel = new GameObject("ItemDetailPanel", typeof(RectTransform), typeof(Image));
        detailPanel.transform.SetParent(panelRT, false);
        var rt = (RectTransform)detailPanel.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(250f, 0f);
        rt.sizeDelta = new Vector2(360f, 600f); // 与银丝框 951×1611 (≈0.59) 同比例
        var bg = detailPanel.GetComponent<Image>();
        bg.color = IIPUIStyle.PanelBackground;
        bg.raycastTarget = true; // 阻挡点击穿透
        IIPUIFactory.ApplyRounded(bg, true);

        // 银丝框装饰层（手绘框，微拉伸可接受）
        var frameGo = new GameObject("FiligreeFrame", typeof(RectTransform), typeof(Image));
        frameGo.transform.SetParent(rt, false);
        var frameRT = (RectTransform)frameGo.transform;
        frameRT.anchorMin = Vector2.zero; frameRT.anchorMax = Vector2.one;
        frameRT.offsetMin = new Vector2(-14f, -14f); // 框略大于面板，花纹压边
        frameRT.offsetMax = new Vector2(14f, 14f);
        var frameImg = frameGo.GetComponent<Image>();
        frameImg.sprite = Resources.Load<Sprite>(TooltipFramePath);
        frameImg.type = Image.Type.Simple;
        frameImg.preserveAspect = false;
        frameImg.color = Color.white;
        frameImg.raycastTarget = false;

        // 图标
        var iconGo = new GameObject("DetailIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(rt, false);
        var iconRT = (RectTransform)iconGo.transform;
        iconRT.anchorMin = new Vector2(0.5f, 1f);
        iconRT.anchorMax = new Vector2(0.5f, 1f);
        iconRT.pivot = new Vector2(0.5f, 1f);
        iconRT.anchoredPosition = new Vector2(0f, -42f);
        iconRT.sizeDelta = new Vector2(96f, 96f);
        detailIcon = iconGo.GetComponent<Image>();
        detailIcon.preserveAspect = true;
        detailIcon.raycastTarget = false;

        detailName = MakeDetailText(rt, "Name", 0f, -158f, 320f, 30f,
            IIPUIStyle.FontSizeButton, IIPUIStyle.TextTitle, TextAnchor.MiddleCenter, FontStyle.Bold);
        detailRarity = MakeDetailText(rt, "Rarity", 0f, -190f, 320f, 24f,
            IIPUIStyle.FontSizeLabel, IIPUIStyle.AccentGold, TextAnchor.MiddleCenter);
        detailType = MakeDetailText(rt, "Type", 0f, -216f, 320f, 22f,
            IIPUIStyle.FontSizeSmall, IIPUIStyle.TextSecondary, TextAnchor.MiddleCenter);
        detailQuantity = MakeDetailText(rt, "Quantity", 0f, -242f, 320f, 22f,
            IIPUIStyle.FontSizeSmall, IIPUIStyle.TextSecondary, TextAnchor.MiddleCenter);

        // 描述（自动换行，左上对齐；顶锚 y=-280 → local +20 起,底部留位给按钮）
        detailDescription = MakeDetailText(rt, "Description", 0f, -280f, 312f, 250f,
            IIPUIStyle.FontSizeBody, IIPUIStyle.TextPrimary, TextAnchor.UpperLeft);
        detailDescription.horizontalOverflow = HorizontalWrapMode.Wrap;

        // 使用 / 装备 按钮（底锚,中心距底缘 38px → local -262）
        useBtn = MakeDetailButton(rt, "UseBtn", "使用", new Vector2(0f, 38f), out useBtnText);
        equipBtn = MakeDetailButton(rt, "EquipBtn", "装备", new Vector2(0f, 38f), out equipBtnText);

        detailPanel.SetActive(false);
    }

    Text MakeDetailText(RectTransform parent, string name, float x, float y, float w, float h,
        int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        var t = go.GetComponent<Text>();
        t.font = IIPUIFont.Get();
        t.fontSize = fontSize;
        t.alignment = anchor;
        t.color = color;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Button MakeDetailButton(RectTransform parent, string name, string label, Vector2 pos, out Text btnText)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        // 底锚:pos.y 为相对父节点底缘的距离(面板 600 高,y=38 → 按钮中心 local -262,贴底)
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(240f, 52f);
        var img = go.GetComponent<Image>();
        img.color = IIPUIStyle.ButtonNormal;
        IIPUIFactory.ApplyRounded(img, true);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;
        IIPUIFactory.ApplyHover(go, IIPUIStyle.ButtonHover, 1.04f);
        btnText = IIPUIFactory.CreateLabel("Text", rt, label, IIPUIStyle.FontSizeButton, IIPUIStyle.TextTitle);
        return btn;
    }

    /// <summary>Tooltip：深色圆角底 + 亮紫描边，跟随鼠标</summary>
    void BuildTooltip(RectTransform panelRT)
    {
        tooltip = new GameObject("ItemTooltip", typeof(RectTransform), typeof(Image));
        tooltip.transform.SetParent(panelRT, false);
        tooltipRect = (RectTransform)tooltip.transform;
        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0f, 1f); // 左上为轴，便于往右下偏移
        tooltipRect.sizeDelta = new Vector2(340f, 150f);
        var bg = tooltip.GetComponent<Image>();
        bg.color = IIPUIStyle.PanelBackground;
        bg.raycastTarget = false;
        IIPUIFactory.ApplyRounded(bg, true);
        var border = IIPUIFactory.CreateBorder(tooltipRect, IIPUIStyle.BorderBright, true);
        border.raycastTarget = false;

        tooltipName = MakeTooltipText(tooltipRect, "Name", 12f, -10f, IIPUIStyle.FontSizeLabel,
            IIPUIStyle.TextTitle, FontStyle.Bold);
        tooltipMeta = MakeTooltipText(tooltipRect, "Meta", 12f, -36f, IIPUIStyle.FontSizeSmall,
            IIPUIStyle.AccentGold);
        tooltipDesc = MakeTooltipText(tooltipRect, "Desc", 12f, -62f, IIPUIStyle.FontSizeSmall,
            IIPUIStyle.TextSecondary);
        tooltipDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        var descRT = (RectTransform)tooltipDesc.transform;
        descRT.sizeDelta = new Vector2(316f, 80f);
        descRT.pivot = new Vector2(0f, 1f);

        tooltip.SetActive(false);
    }

    Text MakeTooltipText(RectTransform parent, string name, float x, float y, int fontSize, Color color,
        FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(316f, 24f);
        var t = go.GetComponent<Text>();
        t.font = IIPUIFont.Get();
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = color;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // ═══════════════════════════════════════════
    // 数据填充
    // ═══════════════════════════════════════════

    /// <summary>填充一栏槽位（稀有度降序→名称升序；超出可见行时槽位池增行，滚轮滚动可见全部物品）</summary>
    void FillSection(List<ArtSlot> slots, List<InventoryItem> items, IIPUI.ArtScrollGrid sg, string sectionName)
    {
        var sorted = items
            .OrderByDescending(i => ResolveRarity(i))
            .ThenBy(i => i.itemName)
            .ToList();

        // 空栏提示：该栏无物品时显示灰色引导文字
        var emptyHint = GetEmptyHint(sectionName);
        if (emptyHint != null) emptyHint.SetActive(sorted.Count == 0);

        // 槽位池按需增行（替代旧 +N 溢出标记，所有物品均可滚动看到并选中）
        int neededRows = Mathf.Max(sg.VisibleRows, Mathf.CeilToInt(sorted.Count / (float)sg.Cols));
        while (slots.Count < neededRows * sg.Cols)
        {
            int index = slots.Count;
            slots.Add(MakeSlot(sg, sectionName, index, index % sg.Cols, index / sg.Cols));
        }
        sg.SetTotalRows(neededRows);

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            if (i < sorted.Count)
            {
                var item = sorted[i];
                slot.item = item;
                // 图标:优先物品自带 icon,空则查图标库(InventorySystem 构造的 item.icon 恒为 null)
                Sprite sp = item.icon != null ? item.icon : IIPUI.IIPIconLibrary.Resolve(item.itemName);
                slot.icon.sprite = sp;
                slot.icon.color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
                slot.count.text = item.quantity > 1 ? item.quantity.ToString() : "";

                // 稀有度底晕（普通不显示）
                ItemRarity rar = ResolveRarity(item);
                if (rar > ItemRarity.Common)
                {
                    var c = GetRarityColor(rar);
                    slot.rarityGlow.color = new Color(c.r, c.g, c.b, 0.30f);
                }
                else
                {
                    slot.rarityGlow.color = new Color(1f, 1f, 1f, 0f);
                }

                // 已装备武器金色标记
                if (item.isEquipped)
                {
                    var gold = IIPUIStyle.AccentGold;
                    slot.rarityGlow.color = new Color(gold.r, gold.g, gold.b, 0.45f);
                }
            }
            else
            {
                // 空槽：零信息覆盖（无图标/数量/底晕），透出底图手绘格
                slot.item = null;
                slot.icon.sprite = null;
                slot.icon.color = Color.clear;
                slot.count.text = "";
                slot.rarityGlow.color = new Color(1f, 1f, 1f, 0f);
            }

            // 非悬停/选中态时清辉光
            if (slot.item == null || slot.item != selectedItem)
                SetGlow(slot, false, false);
        }

        // 恢复选中态
        if (selectedItem != null)
        {
            var sel = slots.FirstOrDefault(s => s.item == selectedItem);
            if (sel != null) SetGlow(sel, false, true);
        }
    }

    /// <summary>按栏名取空栏提示节点</summary>
    GameObject GetEmptyHint(string sectionName)
    {
        switch (sectionName)
        {
            case "Weapon": return weaponEmptyHint;
            case "Consumable": return consumableEmptyHint;
            case "Material": return materialEmptyHint;
            default: return null;
        }
    }

    void SetGlow(ArtSlot slot, bool hover, bool selected)
    {
        if (selected)
        {
            var c = IIPUIStyle.AccentPurple;
            slot.glow.color = new Color(c.r, c.g, c.b, 0.70f);
            slot.root.transform.localScale = Vector3.one * 1.06f;
        }
        else if (hover && slot.item != null)
        {
            var c = IIPUIStyle.AccentPurple;
            slot.glow.color = new Color(c.r, c.g, c.b, 0.40f);
            slot.root.transform.localScale = Vector3.one;
        }
        else
        {
            slot.glow.color = new Color(1f, 1f, 1f, 0f);
            slot.root.transform.localScale = Vector3.one;
        }
    }

    // ═══════════════════════════════════════════
    // 槽位交互
    // ═══════════════════════════════════════════

    void OnSlotClick(ArtSlot slot)
    {
        if (slot.item == null)
        {
            HideItemDetailPanel();
            return;
        }

        // 取消旧选中
        foreach (var s in AllSlots())
            if (s.item == selectedItem) SetGlow(s, false, false);

        selectedItem = slot.item;
        SetGlow(slot, false, true);
        ShowItemDetailPanel(slot.item);
    }

    void OnSlotHoverEnter(ArtSlot slot)
    {
        if (slot.item == null) return;

        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }

        if (slot.item != selectedItem)
            SetGlow(slot, true, false);

        if (currentHoveredItem == slot.item && isTooltipVisible) return;
        currentHoveredItem = slot.item;
        ShowTooltipImmediate(slot.item);
    }

    void OnSlotHoverExit(ArtSlot slot)
    {
        if (slot.item != selectedItem)
            SetGlow(slot, false, false);

        if (hideTooltipCoroutine != null)
            StopCoroutine(hideTooltipCoroutine);
        hideTooltipCoroutine = StartCoroutine(DelayedHideTooltip());
    }

    IEnumerable<ArtSlot> AllSlots()
    {
        foreach (var s in weaponSlots) yield return s;
        foreach (var s in consumableSlots) yield return s;
        foreach (var s in materialSlots) yield return s;
    }

    // ═══════════════════════════════════════════
    // Tooltip
    // ═══════════════════════════════════════════

    System.Collections.IEnumerator DelayedHideTooltip()
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < tooltipHideDelay)
            yield return null;

        HideTooltipImmediate();
        hideTooltipCoroutine = null;
        currentHoveredItem = null;
    }

    void ShowTooltipImmediate(InventoryItem item)
    {
        if (tooltip == null) return;

        tooltipName.text = item.itemName;
        tooltipName.color = GetRarityColor(ResolveRarity(item));
        tooltipMeta.text = $"{GetItemTypeString(item.itemType)} · {GetRarityString(ResolveRarity(item))} · 炼金价值 {item.value}";
        tooltipDesc.text = item.GetDescription();

        tooltip.SetActive(true);
        isTooltipVisible = true;
        UpdateTooltipPosition();
    }

    void UpdateTooltipPosition()
    {
        var canvas = inventoryPanel.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var canvasRT = (RectTransform)canvas.transform;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, Input.mousePosition, null, out local);

        // 面板局部坐标（面板居中于画布）
        Vector2 pos = local + new Vector2(24f, -12f);

        // 钳制在画布内（右/下边缘翻转）
        float halfW = canvasRT.rect.width * 0.5f;
        float halfH = canvasRT.rect.height * 0.5f;
        if (pos.x + tooltipRect.sizeDelta.x > halfW)
            pos.x = local.x - tooltipRect.sizeDelta.x - 24f;
        if (pos.y - tooltipRect.sizeDelta.y < -halfH)
            pos.y = local.y + tooltipRect.sizeDelta.y + 12f;

        tooltipRect.anchoredPosition = pos;
    }

    void HideTooltipImmediate()
    {
        if (tooltip != null && tooltip.activeSelf)
            tooltip.SetActive(false);
        isTooltipVisible = false;
    }

    // ═══════════════════════════════════════════
    // 详情面板
    // ═══════════════════════════════════════════

    void ShowItemDetailPanel(InventoryItem item)
    {
        if (detailPanel == null || item == null) return;

        detailPanel.SetActive(true);

        Sprite dSp = item.icon != null ? item.icon : IIPUI.IIPIconLibrary.Resolve(item.itemName);
        detailIcon.sprite = dSp;
        detailIcon.color = dSp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;

        detailName.text = item.itemName;
        detailName.color = GetRarityColor(ResolveRarity(item));
        detailRarity.text = GetRarityString(ResolveRarity(item));
        detailRarity.color = GetRarityColor(ResolveRarity(item));
        detailType.text = GetItemTypeString(item.itemType);
        detailQuantity.text = $"数量 {item.quantity} · 炼金价值 {item.value}";
        detailDescription.text = item.GetDescription();

        // 按钮：消耗品→使用，武器→装备/卸下，其余仅查看
        bool showUse = item.itemType == ItemType.Consumable;
        bool showEquip = item.itemType == ItemType.Weapon;
        useBtn.gameObject.SetActive(showUse);
        equipBtn.gameObject.SetActive(showEquip);
        if (showEquip)
            equipBtnText.text = item.isEquipped ? "卸下" : "装备";

        useBtn.onClick.RemoveAllListeners();
        equipBtn.onClick.RemoveAllListeners();
        if (showUse)
        {
            useBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[InventoryUI] 使用消耗品: {item.itemName}");
                inventorySystem.UseItem(item);
                RefreshAfterAction(item);
            });
        }
        if (showEquip)
        {
            equipBtn.onClick.AddListener(() =>
            {
                if (item.isEquipped)
                    inventorySystem.UnequipItem(item);
                else
                    inventorySystem.EquipItem(item);
                RefreshAfterAction(item);
            });
        }
    }

    /// <summary>使用/装备后刷新；物品耗尽则关闭详情</summary>
    void RefreshAfterAction(InventoryItem item)
    {
        UpdateInventoryUI();
        UpdateBottomInfoBar();

        bool stillExists = AllSlots().Any(s => s.item == item && item.quantity > 0);
        if (stillExists)
        {
            ShowItemDetailPanel(item); // 刷新数量/按钮文案
        }
        else
        {
            HideItemDetailPanel();
        }
    }

    void HideItemDetailPanel()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        foreach (var s in AllSlots())
            if (s.item == selectedItem) SetGlow(s, false, false);
        selectedItem = null;
    }

    // ═══════════════════════════════════════════
    // 底部信息栏
    // ═══════════════════════════════════════════

    void UpdateBottomInfoBar()
    {
        if (inventorySystem == null || !isBuilt) return;

        soulCurrencyText.text = $"灵魂  {inventorySystem.currentSouls}";

        int materialCount = inventorySystem.GetAllMaterials().Count;
        materialCountText.text = $"材料  {materialCount}/{inventorySystem.maxMaterialSlots}";

        int maxWeight = inventorySystem.maxMaterialSlots + inventorySystem.maxConsumableSlots +
                        inventorySystem.maxWeaponSlots + inventorySystem.maxMemoryFragmentSlots;
        burdenText.text = $"负重  {inventorySystem.currentInventorySize}/{maxWeight}";
    }

    // ═══════════════════════════════════════════
    // 文案工具
    // ═══════════════════════════════════════════

    string GetItemTypeString(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return "武器";
            case ItemType.Consumable: return "消耗品";
            case ItemType.Material: return "材料";
            case ItemType.MemoryFragment: return "记忆碎片";
            default: return "未知";
        }
    }

    string GetRarityString(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "普通";
            case ItemRarity.Uncommon: return "精良";
            case ItemRarity.Rare: return "史诗";
            case ItemRarity.Legendary: return "传奇";
            default: return "未知";
        }
    }

    /// <summary>解析物品稀有度：物品自身标记优先，Common 时回退查图标库（旧数据多未标稀有度）。</summary>
    ItemRarity ResolveRarity(InventoryItem item)
    {
        if (item == null) return ItemRarity.Common;
        if (item.rarity > ItemRarity.Common) return item.rarity;
        if (IIPUI.IIPIconLibrary.TryGetRarity(item.itemName, out var dbRar)) return dbRar;
        return item.rarity;
    }

    Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return IIPUIStyle.TextPrimary;
            case ItemRarity.Uncommon: return IIPUIStyle.RarityUncommon;
            case ItemRarity.Rare: return IIPUIStyle.RarityRare;
            case ItemRarity.Legendary: return IIPUIStyle.RarityLegendary;
            default: return IIPUIStyle.TextPrimary;
        }
    }
}
