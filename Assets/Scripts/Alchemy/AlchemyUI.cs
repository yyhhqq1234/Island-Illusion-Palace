using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using GameSystems;
using IIPUI;

/// <summary>
/// 炼金面板 UI —— 整图做底 + 程序对齐槽位版。
///
/// 架构:底图 炼金界面.png(3840×2160) 整图铺满 1440×810 面板,
/// 槽位几何由 IIPArtLayout(Python 像素识别)换算到面板局部坐标;
/// 全部节点运行时代码构建(根治场景序列化 scale=0 / 引用断链),仅保留 alchemyPanel 根引用。
///
/// 布局:左 配方栏 2×7(已发现=图标+名称,未发现="???"),右 材料栏 3×6(持有材料,单击入篮),
/// 中央魔法阵 4 菱形准备篮(点击取出 1 个),中心书本(配方/自由炼金预览 + 点击炼成 + 结果显示),
/// 底部状态行(理智=成功率 / 记忆碎片=数量+加成 / 负载=当前负担)。
///
/// 交互:单击材料入篮(取代旧双击),点击配方选中并在书本预览,点击书本或 Enter 炼成;
/// 成功:产物图标+金辉光(新发现配方叠加 NEW! 手绘标记);失败:Failure... 手绘文字 + 材料返还面板。
/// 关闭面板时自动收集未领取产出(语义与旧版一致)。
/// </summary>
public class AlchemyUI : MonoBehaviour
{
    [Header("系统引用")]
    [Tooltip("炼金系统，为空时运行时自动查找")]
    public AlchemySystem alchemySystem;
    [Tooltip("炼药锅系统，为空时运行时自动查找")]
    public CauldronSystem cauldronSystem;
    [Tooltip("背包系统，为空时运行时自动查找")]
    public InventorySystem inventorySystem;

    [Header("面板根（场景引用，内部布局由代码重建）")]
    [Tooltip("场景中的 AlchemyPanel 节点，仅保留根引用，子节点全部运行时重建")]
    public GameObject alchemyPanel;

    [Header("图标数据源")]
    [Tooltip("ItemSlot预制体（携带53项物品图标数据库），仅用于读取图标，不会实例化")]
    public GameObject itemSlotPrefab;

    [Header("提示框防抖设置")]
    [Tooltip("鼠标移出槽位后 Tooltip 延迟隐藏秒数，防止抖动")]
    public float tooltipHideDelay = 0.08f;

    // ═══════════════════════════════════════════
    // 常量
    // ═══════════════════════════════════════════

    /// <summary>面板尺寸（与底图 3840×2160 同 16:9 比例）</summary>
    static readonly Vector2 PanelSize = new Vector2(1440f, 810f);
    const string ArtBgPath = "UI/Art/AlchemyPanel_BG";
    const string FailTextPath = "UI/Art/AlchemyFail_Text";
    const string NewRecipePath = "UI/Art/NewRecipe_Text";
    const string ReturnPanelPath = "UI/Art/MaterialReturn_Panel";
    const string IconSanityPath = "UI/Art/Icon_Sanity";
    const string IconMemFragPath = "UI/Art/Icon_MemFragment";
    const string IconBurdenPath = "UI/Art/Icon_Burden";

    // ═══════════════════════════════════════════
    // 运行时构建的节点引用
    // ═══════════════════════════════════════════

    /// <summary>通用槽位（配方/材料/菱形篮复用）</summary>
    class ArtSlot
    {
        public GameObject root;
        public Image glow;       // 悬停/选中辉光
        public Image icon;
        public Text count;       // 数量角标
        public Text label;       // 名称（配方槽用）
        public RecipeData recipe;      // 配方槽数据（null=材料槽）
        public bool discovered;        // 配方是否已发现
        public MaterialTypeEnum material; // 材料槽/篮槽数据
        public bool hasContent;        // 材料槽是否有料（空槽屏蔽悬停 tooltip / 点击入篮 / 辉光）
    }

    readonly List<ArtSlot> recipeSlots = new List<ArtSlot>();
    readonly List<ArtSlot> materialSlots = new List<ArtSlot>();
    readonly ArtSlot[] basketSlots = new ArtSlot[4];

    // 材料栏滚动网格（滚轮上下滚动，行数随持有材料种类动态增长）
    IIPUI.ArtScrollGrid materialGrid;

    // 书本区（预览 / 结果 / 失败反馈）
    GameObject bookRoot;
    Image bookGlow;            // 炼成结果金辉光
    GameObject previewRoot;    // 配方/自由炼金预览
    Image previewIcon;
    Text previewName, previewRate, previewHint;
    Text previewMaterials;     // 书本下方所需材料行
    GameObject resultRoot;     // 炼成结果（产物图标 + NEW!）
    Image resultIcon;
    GameObject newBadge;
    GameObject failRoot;       // 失败反馈（Failure... + 材料返还面板）
    readonly List<Image> returnIcons = new List<Image>(); // 返还面板内的材料小图标

    // 底部状态行
    Text sanityValue, memFragValue, burdenValue;

    // Tooltip
    GameObject tooltip;
    Text tooltipName, tooltipMeta, tooltipDesc;
    RectTransform tooltipRect;

    // ═══════════════════════════════════════════
    // 状态
    // ═══════════════════════════════════════════

    // 自由炼金篮（语义与旧版一致：RottenWood 占位 + isEmpty 标记解决二义性）
    MaterialTypeEnum[] basketMaterials = new MaterialTypeEnum[4];
    int[] basketMaterialCounts = new int[4];
    bool[] basketIsEmpty = new bool[4];

    List<object> produceItems = new List<object>(); // 产出（MaterialTypeEnum 或 RecipeType）
    RecipeType selectedRecipe = RecipeType.SoulStabilizer;
    bool hasSelectedRecipe;    // 是否选中过配方（区分"未选中"与默认值）
    int currentBasketIndex;    // 兼容旧 API（SetCurrentBasket）
    bool craftResultActive;    // 书本区正显示炼成/失败结果

    // 提示框防抖
    Coroutine hideTooltipCoroutine;
    bool isTooltipVisible;
    object currentHoveredItem;

    // 炼药锅交互
    bool playerInCauldronRange;
    bool isAlchemyPanelOpen;
    bool isBuilt;

    BurdenSystem burdenSystem; // 负载显示 + 自由炼金成功率预估
    CharacterStats characterStats; // 智慧加成（自由炼金成功率预估）

    /// <summary>公共只读访问器：玩家是否在炼药锅范围内（供 PlayerController 查询以决定 E 键归属）</summary>
    public bool PlayerInCauldronRange => playerInCauldronRange;

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    void Start()
    {
        if (alchemySystem == null) alchemySystem = FindObjectOfType<AlchemySystem>();
        if (cauldronSystem == null) cauldronSystem = FindObjectOfType<CauldronSystem>();
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        if (burdenSystem == null) burdenSystem = FindObjectOfType<BurdenSystem>();
        if (characterStats == null) characterStats = FindObjectOfType<CharacterStats>();

        IIPIconLibrary.SeedFromPrefab(itemSlotPrefab);

        for (int i = 0; i < 4; i++)
        {
            basketMaterials[i] = MaterialTypeEnum.RottenWood;
            basketMaterialCounts[i] = 0;
            basketIsEmpty[i] = true;
        }

        BuildArtPanel();
        UpdateAlchemyUI();
        HideAlchemyPanel();

        Debug.Log("[AlchemyUI] 炼金UI初始化完成（美术底图 + 程序对齐槽位，配方2×7 / 材料3×6+滚轮增行 / 4菱形篮）");
    }

    void Update()
    {
        // E键开关已由 PlayerController 单点分发，此处不检测

        if (Input.GetKeyDown(KeyCode.Escape) && isAlchemyPanelOpen)
        {
            HideAlchemyPanel();
        }

        // Enter 炼成（准备篮有材料 = 自由炼金；否则按选中配方）
        if (isAlchemyPanelOpen && Input.GetKeyDown(KeyCode.Return))
        {
            if (HasAnyBasketMaterial() || hasSelectedRecipe)
                CraftSelectedRecipe();
        }

        if (isTooltipVisible && tooltipRect != null)
            UpdateTooltipPosition();
    }

    // ═══════════════════════════════════════════
    // 对外 API（保持不变）
    // ═══════════════════════════════════════════

    public void ShowAlchemyPanel()
    {
        if (alchemyPanel == null) return;
        if (!isBuilt) BuildArtPanel(); // 域重载/启动顺序异常时保底构建

        // 与背包面板互斥（两个全屏面板不可同屏）
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
            inventoryUI.HideInventory();

        // 从背包系统同步材料到炼金系统
        SyncMaterialsFromInventory();

        alchemyPanel.SetActive(true);
        UpdateAlchemyUI();

        // 重开面板回到材料栏滚动顶部
        if (materialGrid != null) materialGrid.ResetToTop();

        Time.timeScale = 0f; // 暂停游戏
        isAlchemyPanelOpen = true;
    }

    public void HideAlchemyPanel()
    {
        if (alchemyPanel == null) return;

        // 自动收集产出物品到背包（语义与旧版一致）
        CollectProduceItems();

        alchemyPanel.SetActive(false);
        Time.timeScale = 1f; // 恢复游戏
        isAlchemyPanelOpen = false;

        // 清空产出
        ClearProduceItems();

        // 停止提示框协程并隐藏
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        currentHoveredItem = null;
        isTooltipVisible = false;
        HideTooltipImmediate();
    }

    public bool IsAlchemyPanelOpen() => isAlchemyPanelOpen;

    public void SetPlayerInCauldronRange(bool inRange) => playerInCauldronRange = inRange;

    /// <summary>设置当前选中的准备篮（兼容旧 API；新 UI 自动堆叠无需手动选篮）</summary>
    public void SetCurrentBasket(int index)
    {
        if (index >= 0 && index < 4)
            currentBasketIndex = index;
    }

    public void UpdateAlchemyUI() => RefreshAll();

    // ═══════════════════════════════════════════
    // 材料同步（与旧版语义完全一致）
    // ═══════════════════════════════════════════

    /// <summary>从背包系统同步材料到炼金系统</summary>
    void SyncMaterialsFromInventory()
    {
        if (inventorySystem == null || alchemySystem == null) return;

        foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
            alchemySystem.materialInventory[materialType] = 0;

        var materials = inventorySystem.GetAllMaterials();
        foreach (var item in materials)
        {
            if (item.itemType == ItemType.Material)
                alchemySystem.materialInventory[item.materialType] = item.quantity;
        }
    }

    /// <summary>
    /// 将炼金系统的材料变化同步回背包系统。
    /// 只处理炼金系统向背包的材料（返还、获得），消耗已在 CraftWithMaterials 中处理。
    /// </summary>
    void SyncMaterialsToInventory()
    {
        if (inventorySystem == null || alchemySystem == null) return;

        foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
        {
            int alchemyCount = alchemySystem.materialInventory.ContainsKey(materialType)
                ? alchemySystem.materialInventory[materialType] : 0;

            if (alchemyCount > 0)
            {
                int inventoryCount = inventorySystem.GetMaterialQuantity(materialType);
                if (alchemyCount > inventoryCount)
                {
                    int difference = alchemyCount - inventoryCount;
                    inventorySystem.AddMaterial(materialType, difference);
                    Debug.Log($"[AlchemyUI] 材料返还: {materialType} × {difference}");
                }
            }
        }
    }

    // ═══════════════════════════════════════════
    // 面板构建（运行时，100% 代码）
    // ═══════════════════════════════════════════

    void BuildArtPanel()
    {
        if (alchemyPanel == null)
        {
            Debug.LogError("[AlchemyUI] alchemyPanel 未设置，无法构建炼金面板");
            return;
        }
        if (isBuilt) return;

        // 1) 重置面板根（根治场景序列化 scale=0 / 锚点漂移）
        var panelRT = (RectTransform)alchemyPanel.transform;
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = PanelSize;
        panelRT.localScale = Vector3.one;

        // 1.5) 剥掉面板根本身的底色 Image（旧序列化的暗色底会从美术底图透明区透出）
        var rootImg = alchemyPanel.GetComponent<Image>();
        if (rootImg != null) rootImg.enabled = false;

        // 2) 清空旧子树
        for (int i = panelRT.childCount - 1; i >= 0; i--)
            Destroy(panelRT.GetChild(i).gameObject);

        // 3) 美术底图（整图做底，拦截空白处点击 = 取消选中/收起结果）
        var bgGo = new GameObject("ArtBG", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(panelRT, false);
        var bgImg = bgGo.GetComponent<Image>();
        bgImg.sprite = Resources.Load<Sprite>(ArtBgPath);
        bgImg.type = Image.Type.Simple;
        bgImg.preserveAspect = false;
        bgImg.color = Color.white;
        bgImg.raycastTarget = true;
        var bgRT = (RectTransform)bgGo.transform;
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        var bgBtn = bgGo.AddComponent<Button>();
        bgBtn.transition = Selectable.Transition.None;
        bgBtn.onClick.AddListener(OnPanelBackgroundClick);

        // 3.5) 中央炼金阵深色底板（盖住从底图透明区透出的场景画面，提升槽位/文字可读性）
        BuildCenterBackdrop(panelRT);

        // 4) 左侧配方栏 2×7
        for (int row = 0; row < IIPArtLayout.AlcRecipes.rows; row++)
            for (int col = 0; col < IIPArtLayout.AlcRecipes.cols; col++)
            {
                Vector2 center, size;
                IIPArtLayout.GetSlot(IIPArtLayout.AlcRecipes, col, row,
                    IIPArtLayout.AlcW, IIPArtLayout.AlcH, PanelSize, out center, out size);
                recipeSlots.Add(MakeRecipeSlot(row * IIPArtLayout.AlcRecipes.cols + col, center, size));
            }

        // 5) 右侧材料栏 3×6（ScrollRect 滚动区：初始可见行，槽位池按需增行）
        // 间隙点击转发给"取消选中/收起结果"，与点击面板空白语义一致
        materialGrid = new IIPUI.ArtScrollGrid(panelRT, "Materials", IIPArtLayout.AlcMaterials,
            IIPArtLayout.AlcW, IIPArtLayout.AlcH, PanelSize, OnPanelBackgroundClick);
        for (int row = 0; row < IIPArtLayout.AlcMaterials.rows; row++)
            for (int col = 0; col < IIPArtLayout.AlcMaterials.cols; col++)
                materialSlots.Add(MakeMaterialSlot(row * IIPArtLayout.AlcMaterials.cols + col, col, row));

        // 6) 中央 4 菱形准备篮（北/东/南/西）
        BuildBaskets(panelRT);

        // 7) 中央书本（预览 + 炼成触发 + 结果显示）
        BuildBook(panelRT);

        // 8) 失败反馈（Failure... 文字 + 材料返还面板，默认隐藏）
        BuildFailFeedback(panelRT);

        // 9) 底部状态行（理智 / 记忆碎片 / 负载）
        BuildStatsRow(panelRT);

        // 10) Tooltip（最后创建 = 渲染最上层）
        BuildTooltip(panelRT);

        // 11) 右上角 X 关闭按钮（点击 = ESC 关闭面板）
        BuildCloseButton(panelRT);

        isBuilt = true;
    }

    /// <summary>中央炼金阵区域深色底板：按 4 菱形篮+书本包围盒外扩生成（PanelBackground alpha 0.97≥0.85），
    /// raycastTarget=false 让点击穿透给底图 Button（保持"点空白=取消选中"语义）</summary>
    void BuildCenterBackdrop(RectTransform panelRT)
    {
        // 汇总 4 篮 + 书本的面板局部包围盒
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        void Accumulate(IIPArtLayout.SlotBox box)
        {
            Vector2 c = IIPArtLayout.ToPanel(box.cx, box.cy, IIPArtLayout.AlcW, IIPArtLayout.AlcH, PanelSize);
            Vector2 s = IIPArtLayout.ScaleSize(box.w, box.h, IIPArtLayout.AlcW, IIPArtLayout.AlcH, PanelSize);
            minX = Mathf.Min(minX, c.x - s.x * 0.5f); maxX = Mathf.Max(maxX, c.x + s.x * 0.5f);
            minY = Mathf.Min(minY, c.y - s.y * 0.5f); maxY = Mathf.Max(maxY, c.y + s.y * 0.5f);
        }
        Accumulate(IIPArtLayout.AlcBasketN); Accumulate(IIPArtLayout.AlcBasketE);
        Accumulate(IIPArtLayout.AlcBasketS); Accumulate(IIPArtLayout.AlcBasketW);
        Accumulate(IIPArtLayout.AlcBook);

        const float pad = 36f; // 外扩留白，罩住手绘阵纹外圈
        var go = new GameObject("CenterBackdrop", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(panelRT, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        rt.sizeDelta = new Vector2(maxX - minX + pad * 2f, maxY - minY + pad * 2f);
        var img = go.GetComponent<Image>();
        img.color = IIPUIStyle.PanelBackground;
        img.raycastTarget = false;
        IIPUIFactory.ApplyRounded(img, true);
    }

    /// <summary>右上角 X 关闭按钮（点击 = ESC 关闭炼金面板）</summary>
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
            HideAlchemyPanel(); // 等同 ESC 语义
        });
    }

    /// <summary>创建配方槽（图标 + 名称，未发现时整槽压暗显 "???"）</summary>
    ArtSlot MakeRecipeSlot(int index, Vector2 center, Vector2 cellSize)
    {
        var slot = new ArtSlot();
        BuildSlotRoot($"Slot_Recipe_{index}", center, cellSize, slot);

        slot.glow = MakeGlow("HoverGlow", (RectTransform)slot.root.transform, cellSize * 1.08f);

        // 图标（62% 槽位，上移至给名称留位）
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(slot.root.transform, false);
        var iconRT = (RectTransform)iconGo.transform;
        iconRT.anchorMin = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.pivot = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = new Vector2(0f, cellSize.y * 0.10f);
        iconRT.sizeDelta = cellSize * 0.58f;
        slot.icon = iconGo.GetComponent<Image>();
        slot.icon.preserveAspect = true;
        slot.icon.color = Color.clear;
        slot.icon.raycastTarget = false;

        // 名称（槽位底部,10号字保证最长配方名"召唤持续时间药剂"(8字)单行放下）
        slot.label = IIPUIFactory.CreateLabelAnchored("Name", (RectTransform)slot.root.transform, "",
            10, IIPUIStyle.TextPrimary,
            new Vector2(0f, 0f), new Vector2(1f, 0.32f),
            new Vector2(1f, -2f), new Vector2(-1f, 1f), TextAnchor.MiddleCenter);
        slot.label.horizontalOverflow = HorizontalWrapMode.Wrap;
        slot.label.verticalOverflow = VerticalWrapMode.Overflow;
        slot.label.supportRichText = false;

        WireSlotEvents(slot, () => OnRecipeSlotClick(slot),
            () => OnRecipeHover(slot), () => OnHoverExit());
        return slot;
    }

    /// <summary>创建材料槽（图标 + 数量角标；挂到滚动 Content 下，空槽无信息不响应交互）</summary>
    ArtSlot MakeMaterialSlot(int index, int col, int row)
    {
        var slot = new ArtSlot();
        Vector2 cellSize = materialGrid.CellSize;

        // 根：透明 Image 仅用于射线接收，顶锚定（Content 增高时首行位置不变）
        var root = new GameObject($"Slot_Material_{index}", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(materialGrid.Content, false);
        var rootRT = (RectTransform)root.transform;
        rootRT.anchorMin = new Vector2(0.5f, 1f);
        rootRT.anchorMax = new Vector2(0.5f, 1f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = materialGrid.GetCellAnchoredPos(col, row);
        rootRT.sizeDelta = cellSize * 0.96f;
        var rootImg = root.GetComponent<Image>();
        rootImg.color = new Color(1f, 1f, 1f, 0f);
        rootImg.raycastTarget = true;
        slot.root = root;

        slot.glow = MakeGlow("HoverGlow", rootRT, cellSize * 1.08f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(rootRT, false);
        var iconRT = (RectTransform)iconGo.transform;
        iconRT.anchorMin = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.pivot = new Vector2(0.5f, 0.5f);
        iconRT.sizeDelta = cellSize * 0.66f;
        slot.icon = iconGo.GetComponent<Image>();
        slot.icon.preserveAspect = true;
        slot.icon.color = Color.clear;
        slot.icon.raycastTarget = false;

        slot.count = IIPUIFactory.CreateLabelAnchored("Count", rootRT, "",
            15, IIPUIStyle.TextKey,
            new Vector2(0.5f, 0f), new Vector2(1f, 0.36f),
            new Vector2(0f, -1f), new Vector2(-3f, 0f), TextAnchor.MiddleRight);

        // 空槽（hasContent=false）不响应点击/悬停，保持干净空背景
        WireSlotEvents(slot, () => OnMaterialSlotClick(slot),
            () => OnMaterialHover(slot.material), () => OnHoverExit(),
            () => slot.hasContent);
        return slot;
    }

    /// <summary>槽位根：透明 Image 仅用于射线接收，略小于手绘格避免遮挡格线</summary>
    void BuildSlotRoot(string name, Vector2 center, Vector2 cellSize, ArtSlot slot)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Image));
        root.transform.SetParent(alchemyPanel.transform, false);
        var rootRT = (RectTransform)root.transform;
        rootRT.anchorMin = new Vector2(0.5f, 0.5f);
        rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = center;
        rootRT.sizeDelta = cellSize * 0.96f;
        var rootImg = root.GetComponent<Image>();
        rootImg.color = new Color(1f, 1f, 1f, 0f);
        rootImg.raycastTarget = true;
        slot.root = root;
    }

    /// <summary>统一接线：点击 + 悬停进入/离开（悬停叠加紫辉光，离开恢复基线）。
    /// interactWhen 非空时，仅在谓词为真（如材料槽有料）才响应点击/悬停，空槽保持干净空背景。</summary>
    void WireSlotEvents(ArtSlot slot, UnityEngine.Events.UnityAction onClick,
        UnityEngine.Events.UnityAction onEnter, UnityEngine.Events.UnityAction onExit,
        System.Func<bool> interactWhen = null)
    {
        var btn = slot.root.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.targetGraphic = slot.root.GetComponent<Image>();
        btn.onClick.AddListener(() =>
        {
            if (interactWhen != null && !interactWhen())
            {
                // 空槽点击 = 点击空白：取消配方选中/收起结果，与间隙点击语义一致
                OnPanelBackgroundClick();
                return;
            }
            onClick();
        });

        var trigger = slot.root.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ =>
        {
            if (interactWhen != null && !interactWhen()) return;
            onEnter();
            SetSlotGlow(slot, Mathf.Max(BaselineGlow(slot), 0.40f));
        });
        trigger.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ =>
        {
            onExit();
            SetSlotGlow(slot, BaselineGlow(slot));
        });
        trigger.triggers.Add(exit);
    }

    /// <summary>槽位基线辉光：选中配方 0.70，有料篮 0.25，其余 0</summary>
    float BaselineGlow(ArtSlot slot)
    {
        for (int i = 0; i < 4; i++)
            if (basketSlots[i] == slot)
                return !basketIsEmpty[i] ? 0.25f : 0f;
        if (slot.recipe != null && slot.discovered && hasSelectedRecipe && selectedRecipe == slot.recipe.type)
            return 0.70f;
        return 0f;
    }

    /// <summary>创建圆角辉光层（铺满槽位，初始透明，灵魂紫）</summary>
    Image MakeGlow(string name, RectTransform parent, Vector2 size, bool radial = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = false;
        // 菱形/圆形交互区用径向椭圆辉光(贴合轮廓);方槽用圆角矩形
        if (radial) IIPUIFactory.ApplyRadialGlow(img);
        else IIPUIFactory.ApplyRounded(img, true);
        return img;
    }

    /// <summary>中央 4 菱形准备篮（内缩 0.72 取菱形内接区）</summary>
    void BuildBaskets(RectTransform panelRT)
    {
        IIPArtLayout.SlotBox[] boxes =
        {
            IIPArtLayout.AlcBasketN, IIPArtLayout.AlcBasketE,
            IIPArtLayout.AlcBasketS, IIPArtLayout.AlcBasketW
        };
        string[] names = { "N", "E", "S", "W" };

        for (int i = 0; i < 4; i++)
        {
            Vector2 center, size;
            IIPArtLayout.GetSlot(boxes[i], IIPArtLayout.AlcW, IIPArtLayout.AlcH, PanelSize, 0.72f,
                out center, out size);

            var slot = new ArtSlot();
            BuildSlotRoot($"Basket_{names[i]}", center, size, slot);

            // 深色底 + 描边：与手绘阵纹拉开对比（菱形内接区内用圆角矩形近似）
            var basketBg = slot.root.GetComponent<Image>();
            basketBg.color = IIPUIStyle.SlotBackground;
            IIPUIFactory.ApplyRounded(basketBg, true);
            var basketBorder = IIPUIFactory.CreateBorder(slot.root.transform, IIPUIStyle.BorderDim, true);
            basketBorder.raycastTarget = false; // 不拦截点击，交给根 Button

            slot.glow = MakeGlow("Glow", (RectTransform)slot.root.transform, size * 1.05f, radial: true);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(slot.root.transform, false);
            var iconRT = (RectTransform)iconGo.transform;
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = size * 0.72f;
            slot.icon = iconGo.GetComponent<Image>();
            slot.icon.preserveAspect = true;
            slot.icon.color = Color.clear;
            slot.icon.raycastTarget = false;

            slot.count = IIPUIFactory.CreateLabelAnchored("Count", (RectTransform)slot.root.transform, "",
                16, IIPUIStyle.TextKey,
                new Vector2(0.4f, 0f), new Vector2(1f, 0.42f),
                new Vector2(0f, 0f), new Vector2(-2f, 0f), TextAnchor.MiddleRight);

            int captured = i;
            WireSlotEvents(slot, () => OnBasketClick(captured),
                () => OnBasketHover(captured), () => OnHoverExit());
            basketSlots[i] = slot;
        }
    }

    /// <summary>中央书本：配方预览 / 炼成触发 / 结果显示三合一</summary>
    void BuildBook(RectTransform panelRT)
    {
        Vector2 center, size;
        IIPArtLayout.GetSlot(IIPArtLayout.AlcBook, IIPArtLayout.AlcW, IIPArtLayout.AlcH, PanelSize, 1.0f,
            out center, out size); // ≈187×187

        bookRoot = new GameObject("AlchemyBook", typeof(RectTransform), typeof(Image));
        bookRoot.transform.SetParent(panelRT, false);
        var bookRT = (RectTransform)bookRoot.transform;
        bookRT.anchorMin = new Vector2(0.5f, 0.5f);
        bookRT.anchorMax = new Vector2(0.5f, 0.5f);
        bookRT.pivot = new Vector2(0.5f, 0.5f);
        bookRT.anchoredPosition = center;
        bookRT.sizeDelta = size;
        var bookImg = bookRoot.GetComponent<Image>();
        bookImg.color = new Color(1f, 1f, 1f, 0f); // 透明，透出手绘书本
        bookImg.raycastTarget = true;
        var bookBtn = bookRoot.AddComponent<Button>();
        bookBtn.transition = Selectable.Transition.None;
        bookBtn.targetGraphic = bookImg;
        bookBtn.onClick.AddListener(OnBookClick);

        // 结果金辉光（默认隐藏，径向椭圆贴合书本轮廓）
        bookGlow = MakeGlow("ResultGlow", bookRT, size * 1.15f, radial: true);

        // ── 预览层 ──
        previewRoot = new GameObject("Preview", typeof(RectTransform));
        previewRoot.transform.SetParent(bookRT, false);
        var prevRT = (RectTransform)previewRoot.transform;
        prevRT.anchorMin = Vector2.zero; prevRT.anchorMax = Vector2.one;
        prevRT.offsetMin = Vector2.zero; prevRT.offsetMax = Vector2.zero;

        previewIcon = MakeCenteredIcon(prevRT, "Icon", new Vector2(0f, 30f), new Vector2(62f, 62f));
        // 书页为浅色手绘,白字不可读 → 文字移到书本上方暗底(法阵上环),自上而下:名称→成功率→提示(贴书缘)
        // book-local y:书面板中心≈+5.6,书顶缘≈+99,N篮底缘≈+203,文字带 +114..+180 落在两者之间
        previewName = MakeBookText(prevRT, "Name", 0f, 166f, 170f, 22f,
            IIPUIStyle.FontSizeBody, IIPUIStyle.TextTitle, FontStyle.Bold);
        previewRate = MakeBookText(prevRT, "Rate", 0f, 140f, 170f, 20f,
            IIPUIStyle.FontSizeSmall, IIPUIStyle.AccentCyan);
        previewHint = MakeBookText(prevRT, "Hint", 0f, 114f, 170f, 40f,
            IIPUIStyle.FontSizeKey, IIPUIStyle.TextSecondary);
        previewHint.horizontalOverflow = HorizontalWrapMode.Wrap;

        // 所需材料行（书本下方、南菱形上方的空隙,法阵下环暗底）
        previewMaterials = MakeBookText(panelRT, "RecipeMaterials", 0f, -140f, 230f, 62f,
            IIPUIStyle.FontSizeKey, IIPUIStyle.TextSecondary);
        previewMaterials.horizontalOverflow = HorizontalWrapMode.Wrap;
        previewMaterials.verticalOverflow = VerticalWrapMode.Overflow;

        // ── 结果层（产物图标 + NEW! 手绘标记，默认隐藏）──
        resultRoot = new GameObject("Result", typeof(RectTransform));
        resultRoot.transform.SetParent(bookRT, false);
        var resRT = (RectTransform)resultRoot.transform;
        resRT.anchorMin = Vector2.zero; resRT.anchorMax = Vector2.one;
        resRT.offsetMin = Vector2.zero; resRT.offsetMax = Vector2.zero;

        resultIcon = MakeCenteredIcon(resRT, "Icon", new Vector2(0f, 16f), new Vector2(76f, 76f));

        newBadge = new GameObject("NewBadge", typeof(RectTransform), typeof(Image));
        newBadge.transform.SetParent(resRT, false);
        var badgeRT = (RectTransform)newBadge.transform;
        badgeRT.anchorMin = new Vector2(1f, 1f);
        badgeRT.anchorMax = new Vector2(1f, 1f);
        badgeRT.pivot = new Vector2(0.5f, 0.5f);
        badgeRT.anchoredPosition = new Vector2(-24f, -18f);
        badgeRT.sizeDelta = new Vector2(96f, 45f); // NewRecipe_Text 裁剪后 179×84 (≈2.13) 同比例
        var badgeImg = newBadge.GetComponent<Image>();
        badgeImg.sprite = Resources.Load<Sprite>(NewRecipePath);
        badgeImg.preserveAspect = true;
        badgeImg.raycastTarget = false;

        resultRoot.SetActive(false);
    }

    /// <summary>失败反馈：Failure... 手绘文字浮于书本上方 + 材料返还面板（银丝框，内嵌返还材料图标）</summary>
    void BuildFailFeedback(RectTransform panelRT)
    {
        failRoot = new GameObject("FailFeedback", typeof(RectTransform));
        failRoot.transform.SetParent(panelRT, false);
        var failRT = (RectTransform)failRoot.transform;
        failRT.anchorMin = new Vector2(0.5f, 0.5f);
        failRT.anchorMax = new Vector2(0.5f, 0.5f);
        failRT.pivot = new Vector2(0.5f, 0.5f);
        failRT.anchoredPosition = Vector2.zero;
        failRT.sizeDelta = Vector2.zero;

        // Failure... 文字（868×361 → 200×83，书本上方）
        var failTextGo = new GameObject("FailText", typeof(RectTransform), typeof(Image));
        failTextGo.transform.SetParent(failRT, false);
        var failTextRT = (RectTransform)failTextGo.transform;
        failTextRT.pivot = new Vector2(0.5f, 0.5f);
        failTextRT.anchoredPosition = new Vector2(0f, 150f);
        failTextRT.sizeDelta = new Vector2(200f, 83f);
        var failTextImg = failTextGo.GetComponent<Image>();
        failTextImg.sprite = Resources.Load<Sprite>(FailTextPath);
        failTextImg.preserveAspect = true;
        failTextImg.raycastTarget = false;

        // 材料返还面板（951×1610 → 168×285，书本右侧，标题"~材料返还~"已烘焙）
        var returnGo = new GameObject("MaterialReturnPanel", typeof(RectTransform), typeof(Image));
        returnGo.transform.SetParent(failRT, false);
        var returnRT = (RectTransform)returnGo.transform;
        returnRT.pivot = new Vector2(0.5f, 0.5f);
        returnRT.anchoredPosition = new Vector2(196f, 40f);
        returnRT.sizeDelta = new Vector2(168f, 285f);
        var returnImg = returnGo.GetComponent<Image>();
        returnImg.sprite = Resources.Load<Sprite>(ReturnPanelPath);
        returnImg.preserveAspect = true;
        returnImg.raycastTarget = true; // 返还图标可点击收集

        // 返还材料小图标（面板内一列 4 个，点击收集）
        for (int i = 0; i < 4; i++)
        {
            var iconGo = new GameObject($"ReturnIcon_{i}", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(returnRT, false);
            var iconRT = (RectTransform)iconGo.transform;
            iconRT.anchorMin = new Vector2(0.5f, 1f);
            iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0f, -92f - i * 52f);
            iconRT.sizeDelta = new Vector2(44f, 44f);
            var icon = iconGo.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.color = Color.clear;
            returnIcons.Add(icon);

            int captured = i;
            var btn = iconGo.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = icon;
            btn.onClick.AddListener(() => CollectProduceItem(captured));
        }

        failRoot.SetActive(false);
    }

    /// <summary>底部状态行：理智(成功率) | 记忆碎片 | 负载（炼金阵正下方显眼处，手绘图标+大字号数值）</summary>
    void BuildStatsRow(RectTransform panelRT)
    {
        sanityValue = MakeStatGroup(panelRT, "Sanity", IconSanityPath, -320f, IIPUIStyle.AccentCyan);
        memFragValue = MakeStatGroup(panelRT, "MemFrag", IconMemFragPath, 0f, IIPUIStyle.AccentPurple);
        burdenValue = MakeStatGroup(panelRT, "Burden", IconBurdenPath, 320f, IIPUIStyle.AccentGold);
    }

    /// <summary>创建一组状态图标+数值（图标 44px 左，数值文本右，字号=标题级 24）</summary>
    Text MakeStatGroup(RectTransform panelRT, string name, string iconPath, float x, Color valueColor)
    {
        var group = new GameObject($"Stat_{name}", typeof(RectTransform));
        group.transform.SetParent(panelRT, false);
        var groupRT = (RectTransform)group.transform;
        groupRT.anchorMin = new Vector2(0.5f, 0.5f);
        groupRT.anchorMax = new Vector2(0.5f, 0.5f);
        groupRT.pivot = new Vector2(0.5f, 0.5f);
        groupRT.anchoredPosition = new Vector2(x, -368f);
        groupRT.sizeDelta = new Vector2(300f, 44f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(groupRT, false);
        var iconRT = (RectTransform)iconGo.transform;
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot = new Vector2(0f, 0.5f);
        iconRT.anchoredPosition = Vector2.zero;
        iconRT.sizeDelta = new Vector2(44f, 44f);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = Resources.Load<Sprite>(iconPath);
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        return IIPUIFactory.CreateLabelAnchored("Value", groupRT, "",
            IIPUIStyle.FontSizeAreaTitle, valueColor,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(52f, 0f), new Vector2(0f, 0f), TextAnchor.MiddleLeft);
    }

    /// <summary>Tooltip：深色圆角底 + 亮紫描边，跟随鼠标（与背包一致）</summary>
    void BuildTooltip(RectTransform panelRT)
    {
        tooltip = new GameObject("ItemTooltip", typeof(RectTransform), typeof(Image));
        tooltip.transform.SetParent(panelRT, false);
        tooltipRect = (RectTransform)tooltip.transform;
        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0f, 1f);
        tooltipRect.sizeDelta = new Vector2(340f, 150f);
        var bg = tooltip.GetComponent<Image>();
        bg.color = IIPUIStyle.PanelBackground;
        bg.raycastTarget = false;
        IIPUIFactory.ApplyRounded(bg, true);
        var border = IIPUIFactory.CreateBorder(tooltipRect, IIPUIStyle.BorderBright, true);
        border.raycastTarget = false;

        tooltipName = MakeTooltipText(tooltipRect, "Name", 12f, -10f,
            IIPUIStyle.FontSizeLabel, IIPUIStyle.TextTitle, FontStyle.Bold);
        tooltipMeta = MakeTooltipText(tooltipRect, "Meta", 12f, -36f,
            IIPUIStyle.FontSizeSmall, IIPUIStyle.AccentGold);
        tooltipDesc = MakeTooltipText(tooltipRect, "Desc", 12f, -62f,
            IIPUIStyle.FontSizeSmall, IIPUIStyle.TextSecondary);
        tooltipDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        var descRT = (RectTransform)tooltipDesc.transform;
        descRT.sizeDelta = new Vector2(316f, 80f);
        descRT.pivot = new Vector2(0f, 1f);

        tooltip.SetActive(false);
    }

    // ═══════════════════════════════════════════
    // 构建小工具
    // ═══════════════════════════════════════════

    Image MakeCenteredIcon(RectTransform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.color = Color.clear;
        img.raycastTarget = false;
        return img;
    }

    Text MakeBookText(RectTransform parent, string name, float x, float y, float w, float h,
        int fontSize, Color color, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        var t = go.GetComponent<Text>();
        t.font = IIPUIFont.Get();
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
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
    // 数据刷新（槽位池一次性创建，仅刷新数据）
    // ═══════════════════════════════════════════

    void RefreshAll()
    {
        if (!isBuilt || alchemySystem == null) return;
        FillRecipeSlots();
        FillMaterialSlots();
        FillBasketSlots();
        RefreshBook();
        RefreshStats();
    }

    /// <summary>配方栏：物品配方（非 Material_ 合成式），已发现亮显，未发现 "???"</summary>
    void FillRecipeSlots()
    {
        var itemRecipes = alchemySystem.availableRecipes
            .Where(r => !r.type.ToString().StartsWith("Material_"))
            .Take(recipeSlots.Count)
            .ToList();

        for (int i = 0; i < recipeSlots.Count; i++)
        {
            var slot = recipeSlots[i];
            if (i < itemRecipes.Count)
            {
                var recipe = itemRecipes[i];
                slot.recipe = recipe;
                slot.discovered = alchemySystem.discoveredRecipes.Contains(recipe.type);

                if (slot.discovered)
                {
                    Sprite sp = IIPIconLibrary.Resolve(recipe.name);
                    slot.icon.sprite = sp;
                    slot.icon.color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
                    slot.label.text = recipe.name;
                    slot.label.color = IIPUIStyle.TextPrimary;
                }
                else
                {
                    slot.icon.sprite = null;
                    slot.icon.color = Color.clear;
                    slot.label.text = "???";
                    slot.label.color = IIPUIStyle.TextSecondary;
                }

                // 选中态辉光
                bool selected = hasSelectedRecipe && selectedRecipe == recipe.type && slot.discovered;
                SetSlotGlow(slot, selected ? 0.70f : 0f);
            }
            else
            {
                slot.recipe = null;
                slot.discovered = false;
                slot.icon.sprite = null;
                slot.icon.color = Color.clear;
                slot.label.text = "";
                SetSlotGlow(slot, 0f);
            }
        }
    }

    /// <summary>材料栏：持有材料按炼金价值降序，单击入篮；超出可见行时槽位池增行，滚轮滚动可见全部</summary>
    void FillMaterialSlots()
    {
        List<KeyValuePair<MaterialTypeEnum, int>> owned = new List<KeyValuePair<MaterialTypeEnum, int>>();
        if (inventorySystem != null)
        {
            foreach (MaterialTypeEnum type in System.Enum.GetValues(typeof(MaterialTypeEnum)))
            {
                int count = inventorySystem.GetMaterialQuantity(type);
                if (count > 0) owned.Add(new KeyValuePair<MaterialTypeEnum, int>(type, count));
            }
        }
        var sorted = owned
            .OrderByDescending(kv => alchemySystem.GetMaterialValue(kv.Key))
            .ThenBy(kv => alchemySystem.GetMaterialName(kv.Key))
            .ToList();

        // 槽位池按需增行（24 种材料 > 18 可见槽时，滚动查看全部）
        int neededRows = Mathf.Max(materialGrid.VisibleRows,
            Mathf.CeilToInt(sorted.Count / (float)materialGrid.Cols));
        while (materialSlots.Count < neededRows * materialGrid.Cols)
        {
            int index = materialSlots.Count;
            materialSlots.Add(MakeMaterialSlot(index, index % materialGrid.Cols, index / materialGrid.Cols));
        }
        materialGrid.SetTotalRows(neededRows);

        for (int i = 0; i < materialSlots.Count; i++)
        {
            var slot = materialSlots[i];
            if (i < sorted.Count)
            {
                var kv = sorted[i];
                slot.material = kv.Key;
                slot.hasContent = true;
                string name = alchemySystem.GetMaterialName(kv.Key);
                Sprite sp = IIPIconLibrary.Resolve(name);
                slot.icon.sprite = sp;
                slot.icon.color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
                slot.count.text = kv.Value > 1 ? kv.Value.ToString() : "";
            }
            else
            {
                // 空槽：零信息覆盖（无图标/数量/辉光），hasContent=false 屏蔽悬停 tooltip 与点击入篮
                slot.hasContent = false;
                slot.icon.sprite = null;
                slot.icon.color = Color.clear;
                slot.count.text = "";
                SetSlotGlow(slot, 0f); // 清掉材料入篮前的悬停辉光残留
            }
        }
    }

    /// <summary>菱形篮：材料图标+堆叠数，有料时淡紫辉光</summary>
    void FillBasketSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            var slot = basketSlots[i];
            if (!basketIsEmpty[i])
            {
                string name = alchemySystem.GetMaterialName(basketMaterials[i]);
                Sprite sp = IIPIconLibrary.Resolve(name);
                slot.icon.sprite = sp;
                slot.icon.color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
                slot.count.text = basketMaterialCounts[i] > 1 ? basketMaterialCounts[i].ToString() : "";
                SetSlotGlow(slot, 0.25f); // 有料淡辉光
            }
            else
            {
                slot.icon.sprite = null;
                slot.icon.color = Color.clear;
                slot.count.text = "";
                SetSlotGlow(slot, 0f);
            }
        }
    }

    /// <summary>书本区刷新：结果优先，其次自由炼金预览，再其次配方预览，最后空态提示</summary>
    void RefreshBook()
    {
        if (craftResultActive) return; // 结果/失败反馈显示中，不覆盖

        failRoot.SetActive(false);
        resultRoot.SetActive(false);
        bookGlow.color = new Color(1f, 1f, 1f, 0f);
        previewRoot.SetActive(true);

        if (HasAnyBasketMaterial())
        {
            // 自由炼金预览
            previewIcon.sprite = null;
            previewIcon.color = Color.clear;
            previewName.text = "自由炼金";
            float rate = EstimateFreeAlchemyRate();
            previewRate.text = $"预估成功率 {Mathf.RoundToInt(rate * 100f)}%";
            previewHint.text = "点击书本 或 按 Enter 炼成";

            // 篮中材料清单
            var parts = new List<string>();
            for (int i = 0; i < 4; i++)
                if (!basketIsEmpty[i])
                {
                    string name = alchemySystem.GetMaterialName(basketMaterials[i]);
                    parts.Add(basketMaterialCounts[i] > 1 ? $"{name}×{basketMaterialCounts[i]}" : name);
                }
            previewMaterials.text = string.Join(" · ", parts);
        }
        else if (hasSelectedRecipe)
        {
            var recipe = alchemySystem.GetRecipeData(selectedRecipe);
            if (recipe != null)
            {
                Sprite sp = IIPIconLibrary.Resolve(recipe.name);
                previewIcon.sprite = sp;
                previewIcon.color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
                previewName.text = recipe.name;
                float rate = alchemySystem.CalculateSuccessRate(recipe);
                previewRate.text = $"成功率 {Mathf.RoundToInt(rate * 100f)}%";
                previewHint.text = "点击书本 或 按 Enter 炼成";
                previewMaterials.text = BuildRequiredMaterialsText(recipe);
            }
        }
        else
        {
            previewIcon.sprite = null;
            previewIcon.color = Color.clear;
            previewName.text = "";
            previewRate.text = "";
            previewHint.text = "从左侧选择配方\n或从右侧投入材料";
            previewMaterials.text = "";
        }
    }

    /// <summary>构建配方所需材料行（富文本：足料灰白，缺料红）</summary>
    string BuildRequiredMaterialsText(RecipeData recipe)
    {
        if (recipe.requiredMaterials == null) return ""; // 防御：域重载后 Dictionary 可能丢失
        var parts = new List<string>();
        foreach (var mat in recipe.requiredMaterials)
        {
            string name = alchemySystem.GetMaterialName(mat.Key);
            int have = inventorySystem != null ? inventorySystem.GetMaterialQuantity(mat.Key) : 0;
            string colorHex = have >= mat.Value ? "#A6A6BF" : "#FF3333";
            parts.Add($"<color={colorHex}>{name} {have}/{mat.Value}</color>");
        }
        return string.Join("  ", parts);
    }

    /// <summary>底部状态行：理智=当前上下文成功率 / 记忆碎片=数量+加成 / 负载=当前负担</summary>
    void RefreshStats()
    {
        // 理智：选中配方 → 真实成功率；篮中有料 → 自由炼金预估；否则 --
        if (HasAnyBasketMaterial())
            sanityValue.text = $"理智 {Mathf.RoundToInt(EstimateFreeAlchemyRate() * 100f)}%";
        else if (hasSelectedRecipe && alchemySystem.GetRecipeData(selectedRecipe) != null)
            sanityValue.text = $"理智 {Mathf.RoundToInt(alchemySystem.CalculateSuccessRate(alchemySystem.GetRecipeData(selectedRecipe)) * 100f)}%";
        else
            sanityValue.text = "理智 --";

        // 记忆碎片：数量 + 激活加成提示
        int fragCount = inventorySystem != null ? inventorySystem.GetAllMemoryFragments().Count : 0;
        memFragValue.text = fragCount > 0
            ? $"碎片 ×{fragCount}（+{Mathf.RoundToInt(alchemySystem.memoryShardBonus * 100f)}%）"
            : "碎片 ×0";

        // 负载：阈值变色
        if (burdenSystem != null)
        {
            float burden = burdenSystem.GetCurrentBurden();
            burdenValue.text = $"负载 {burden:F0}/100";
            burdenValue.color = burden >= IIPConstants.BurdenCriticalThreshold ? IIPUIStyle.WarningRed
                : burden >= IIPConstants.BurdenHighThreshold ? IIPUIStyle.WarningOrange
                : burden >= 50f ? IIPUIStyle.WarningYellow
                : IIPUIStyle.AccentGold;
        }
        else
        {
            burdenValue.text = "负载 --";
        }
    }

    /// <summary>自由炼金成功率预估（与 AlchemySystem.CalculateFreeAlchemySuccessRate 同构：基础+智慧-负担+碎片）</summary>
    float EstimateFreeAlchemyRate()
    {
        int wisdom = characterStats != null ? characterStats.wisdom : 10;
        float rate = alchemySystem.freeAlchemyBaseSuccessRate + wisdom * 0.01f;
        if (burdenSystem != null)
            rate -= burdenSystem.GetCurrentBurden() * 0.002f;
        if (inventorySystem != null && inventorySystem.GetAllMemoryFragments().Count > 0)
            rate += alchemySystem.memoryShardBonus;
        return Mathf.Clamp(rate, alchemySystem.successRateMinimum, alchemySystem.successRateMaximum);
    }

    bool HasAnyBasketMaterial()
    {
        for (int i = 0; i < 4; i++)
            if (!basketIsEmpty[i]) return true;
        return false;
    }

    void SetSlotGlow(ArtSlot slot, float alpha)
    {
        if (slot.glow == null) return;
        slot.glow.color = alpha > 0f
            ? new Color(IIPUIStyle.AccentPurple.r, IIPUIStyle.AccentPurple.g, IIPUIStyle.AccentPurple.b, alpha)
            : new Color(1f, 1f, 1f, 0f);
    }

    // ═══════════════════════════════════════════
    // 交互：配方选中 / 材料入篮 / 取篮 / 炼成
    // ═══════════════════════════════════════════

    void OnRecipeSlotClick(ArtSlot slot)
    {
        if (slot.recipe == null || !slot.discovered) return; // 未发现配方不可选中
        selectedRecipe = slot.recipe.type;
        hasSelectedRecipe = true;
        craftResultActive = false; // 收起结果，回到预览
        RefreshAll();
    }

    void OnMaterialSlotClick(ArtSlot slot)
    {
        AddMaterialToBasket(slot.material);
    }

    /// <summary>材料入篮（单击；优先同材料堆叠至 99，否则入第一个空篮）</summary>
    void AddMaterialToBasket(MaterialTypeEnum materialType)
    {
        if (inventorySystem == null) return;

        int inventoryCount = inventorySystem.GetMaterialQuantity(materialType);
        if (inventoryCount <= 0)
        {
            Debug.Log("[AlchemyUI] 材料不足，无法添加到准备篮");
            return;
        }

        // 1. 优先同材料篮堆叠
        for (int i = 0; i < 4; i++)
        {
            if (!basketIsEmpty[i] && basketMaterials[i] == materialType && basketMaterialCounts[i] < 99)
            {
                inventorySystem.RemoveMaterial(materialType, 1);
                basketMaterialCounts[i]++;
                craftResultActive = false;
                RefreshAll();
                return;
            }
        }

        // 2. 第一个空篮
        for (int i = 0; i < 4; i++)
        {
            if (basketIsEmpty[i])
            {
                inventorySystem.RemoveMaterial(materialType, 1);
                basketMaterials[i] = materialType;
                basketMaterialCounts[i] = 1;
                basketIsEmpty[i] = false;
                craftResultActive = false;
                RefreshAll();
                return;
            }
        }

        Debug.Log("[AlchemyUI] 准备篮已满，无法添加更多材料");
    }

    void OnBasketClick(int basketIndex)
    {
        if (basketIndex < 0 || basketIndex >= 4 || basketIsEmpty[basketIndex]) return;

        var materialType = basketMaterials[basketIndex];
        inventorySystem.AddMaterial(materialType, 1);
        basketMaterialCounts[basketIndex]--;
        if (basketMaterialCounts[basketIndex] <= 0)
        {
            basketMaterials[basketIndex] = MaterialTypeEnum.RottenWood;
            basketIsEmpty[basketIndex] = true;
        }
        craftResultActive = false;
        RefreshAll();
    }

    void OnBookClick()
    {
        // 结果显示中 → 点击收集首个产出；否则触发炼成
        if (craftResultActive && produceItems != null && produceItems.Count > 0)
        {
            CollectProduceItem(0);
            return;
        }
        if (HasAnyBasketMaterial() || hasSelectedRecipe)
            CraftSelectedRecipe();
    }

    void OnPanelBackgroundClick()
    {
        // 取消配方选中、收起结果反馈
        hasSelectedRecipe = false;
        craftResultActive = false;
        HideTooltipImmediate();
        RefreshAll();
    }

    /// <summary>
    /// 炼成（语义与旧版一致）：
    /// 准备篮有料 → 自由炼金 CraftWithMaterials；否则 → 配方炼金 CraftRecipe(selectedRecipe)。
    /// 新增:结果可视化（成功=产物+金辉光+NEW!；失败=Failure... + 材料返还面板）。
    /// </summary>
    public void CraftSelectedRecipe()
    {
        if (alchemySystem == null) return;

        // 收集篮中材料（按堆叠数展开）
        bool isFreeAlchemy = false;
        List<MaterialTypeEnum> basketMats = new List<MaterialTypeEnum>();
        for (int i = 0; i < 4; i++)
        {
            if (!basketIsEmpty[i])
            {
                for (int j = 0; j < basketMaterialCounts[i]; j++)
                    basketMats.Add(basketMaterials[i]);
                isFreeAlchemy = true;
            }
        }

        // 新发现检测：炼成前快照
        int discoveredBefore = alchemySystem.discoveredRecipes.Count;

        bool success;

        if (isFreeAlchemy)
        {
            // 自由炼金：同步背包材料 → 合成 → 回同步
            SyncInventoryCountsToAlchemy();
            success = alchemySystem.CraftWithMaterials(basketMats.ToArray());
            SyncMaterialsToInventory();

            ClearBaskets();
            produceItems = alchemySystem.GetPendingProduce();
            Debug.Log(success ? "[AlchemyUI] 自由炼金成功！" : "[AlchemyUI] 自由炼金失败，材料已部分返还");
        }
        else
        {
            // 配方炼金
            if (!hasSelectedRecipe)
            {
                Debug.LogWarning("[AlchemyUI] 未选中配方且准备篮为空，无法炼成");
                return;
            }

            Debug.Log($"[AlchemyUI] 开始配方炼金: {selectedRecipe}");
            SyncInventoryCountsToAlchemy();
            success = alchemySystem.CraftRecipe(selectedRecipe);
            SyncMaterialsToInventory();

            if (success)
            {
                // 药剂类配方产物直接入包（语义与旧版一致）
                if (IsPotionRecipe(selectedRecipe) && inventorySystem != null)
                    inventorySystem.AddPotion(selectedRecipe, 1);
                Debug.Log("[AlchemyUI] 配方炼金成功！");
            }
            else
            {
                Debug.Log("[AlchemyUI] 配方炼金失败，材料已部分返还");
            }

            ClearBaskets();
            produceItems = alchemySystem.GetPendingProduce();
        }

        // ── 结果可视化 ──
        bool newDiscovery = alchemySystem.discoveredRecipes.Count > discoveredBefore;
        if (success) ShowCraftSuccess(isFreeAlchemy, newDiscovery);
        else ShowCraftFailure();

        craftResultActive = true;
        RefreshAll();
    }

    /// <summary>将背包各材料数量同步到炼金系统（两模式共用的前置步骤）</summary>
    void SyncInventoryCountsToAlchemy()
    {
        if (alchemySystem == null || inventorySystem == null) return;
        foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
            alchemySystem.SetMaterialCount(materialType, inventorySystem.GetMaterialQuantity(materialType));
    }

    void ClearBaskets()
    {
        for (int i = 0; i < 4; i++)
        {
            basketMaterials[i] = MaterialTypeEnum.RottenWood;
            basketMaterialCounts[i] = 0;
            basketIsEmpty[i] = true;
        }
    }

    /// <summary>成功反馈：产物图标 + 金辉光（新发现配方叠加 NEW! 手绘标记）</summary>
    void ShowCraftSuccess(bool wasFreeAlchemy, bool newDiscovery)
    {
        previewRoot.SetActive(false);
        previewMaterials.text = "";
        failRoot.SetActive(false);
        resultRoot.SetActive(true);

        // 产物图标：自由炼金取产出首项；配方炼金取配方产物
        Sprite sp = null;
        if (wasFreeAlchemy && produceItems != null && produceItems.Count > 0)
            sp = ResolveProduceIcon(produceItems[0]);
        else if (!wasFreeAlchemy)
            sp = IIPIconLibrary.Resolve(alchemySystem.GetRecipeName(selectedRecipe));

        resultIcon.sprite = sp;
        resultIcon.color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
        newBadge.SetActive(newDiscovery);

        // 金辉光
        bookGlow.color = new Color(IIPUIStyle.AccentGold.r, IIPUIStyle.AccentGold.g, IIPUIStyle.AccentGold.b, 0.35f);
    }

    /// <summary>失败反馈：Failure... 手绘文字 + 材料返还面板（内嵌返还材料图标）</summary>
    void ShowCraftFailure()
    {
        previewRoot.SetActive(false);
        previewMaterials.text = "";
        resultRoot.SetActive(false);
        bookGlow.color = new Color(1f, 1f, 1f, 0f);
        failRoot.SetActive(true);

        // 返还材料图标（produceItems 即失败返还物）
        for (int i = 0; i < returnIcons.Count; i++)
        {
            if (produceItems != null && i < produceItems.Count)
            {
                Sprite sp = ResolveProduceIcon(produceItems[i]);
                returnIcons[i].sprite = sp;
                returnIcons[i].color = sp != null ? Color.white : IIPUIStyle.WeaponIconPlaceholder;
            }
            else
            {
                returnIcons[i].sprite = null;
                returnIcons[i].color = Color.clear;
            }
        }
    }

    /// <summary>解析产出物图标（产出可以是 MaterialTypeEnum 或 RecipeType）</summary>
    Sprite ResolveProduceIcon(object produce)
    {
        if (produce is MaterialTypeEnum materialType)
            return IIPIconLibrary.Resolve(alchemySystem.GetMaterialName(materialType));
        if (produce is RecipeType recipeType)
            return IIPIconLibrary.Resolve(alchemySystem.GetRecipeName(recipeType));
        return null;
    }

    // ═══════════════════════════════════════════
    // 产出收集（语义与旧版一致）
    // ═══════════════════════════════════════════

    /// <summary>收集单个产出到背包</summary>
    void CollectProduceItem(int index)
    {
        if (produceItems == null || index < 0 || index >= produceItems.Count) return;

        object item = produceItems[index];
        if (item == null) return;

        if (item is MaterialTypeEnum materialType)
        {
            inventorySystem.AddMaterial(materialType, 1);
            Debug.Log($"[AlchemyUI] 收集产出材料: {materialType}");
        }
        else if (item is RecipeType recipeType)
        {
            inventorySystem.AddPotion(recipeType, 1);
            Debug.Log($"[AlchemyUI] 收集产出消耗品: {recipeType}");
        }

        produceItems.RemoveAt(index);

        // 产出收完 → 收起结果反馈回到预览
        if (produceItems.Count == 0)
            craftResultActive = false;

        RefreshAll();
    }

    /// <summary>收集所有产出（关面板自动调用）</summary>
    void CollectProduceItems()
    {
        if (produceItems == null || produceItems.Count == 0) return;

        Debug.Log("[AlchemyUI] 自动收集所有产出物品...");
        foreach (object item in produceItems)
        {
            if (item is MaterialTypeEnum materialType)
                inventorySystem.AddMaterial(materialType, 1);
            else if (item is RecipeType recipeType)
                inventorySystem.AddPotion(recipeType, 1);
        }

        produceItems.Clear();
        alchemySystem.ClearPendingProduce();
    }

    void ClearProduceItems()
    {
        produceItems.Clear();
        if (alchemySystem != null) alchemySystem.ClearPendingProduce();
        craftResultActive = false;
    }

    // ═══════════════════════════════════════════
    // 悬停 Tooltip（防抖逻辑与旧版/背包一致）
    // ═══════════════════════════════════════════

    void OnRecipeHover(ArtSlot slot)
    {
        if (slot.recipe == null) return;
        CancelHideTooltip();
        if (currentHoveredItem is RecipeData && (RecipeData)currentHoveredItem == slot.recipe && isTooltipVisible)
            return;
        currentHoveredItem = slot.recipe;
        ShowTooltipImmediate(slot.recipe, slot.discovered);
    }

    void OnMaterialHover(MaterialTypeEnum materialType)
    {
        CancelHideTooltip();
        if (currentHoveredItem is MaterialTypeEnum && (MaterialTypeEnum)currentHoveredItem == materialType && isTooltipVisible)
            return;
        currentHoveredItem = materialType;
        ShowTooltipImmediate(materialType);
    }

    void OnBasketHover(int basketIndex)
    {
        if (basketIndex < 0 || basketIndex >= 4 || basketIsEmpty[basketIndex]) return;
        OnMaterialHover(basketMaterials[basketIndex]);
    }

    void OnHoverExit()
    {
        if (hideTooltipCoroutine != null) StopCoroutine(hideTooltipCoroutine);
        hideTooltipCoroutine = StartCoroutine(DelayedHideTooltip());
    }

    void CancelHideTooltip()
    {
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
    }

    System.Collections.IEnumerator DelayedHideTooltip()
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < tooltipHideDelay)
            yield return null;

        HideTooltipImmediate();
        hideTooltipCoroutine = null;
        currentHoveredItem = null;
    }

    void ShowTooltipImmediate(RecipeData recipe, bool discovered)
    {
        if (tooltip == null) return;

        tooltipName.text = discovered ? recipe.name : "???";
        tooltipMeta.text = discovered
            ? $"{GetRecipeTierString(recipe.tier)}配方 · 炼金价值 {recipe.alchemicalValue}"
            : "未发现的配方";
        tooltipDesc.text = discovered ? recipe.resultDescription : "通过自由炼金投入材料，尝试发现新的配方。";

        tooltip.SetActive(true);
        isTooltipVisible = true;
        UpdateTooltipPosition();
    }

    void ShowTooltipImmediate(MaterialTypeEnum materialType)
    {
        if (tooltip == null || alchemySystem == null) return;

        MaterialData data = alchemySystem.GetMaterialData(materialType);
        tooltipName.text = data.name;
        tooltipMeta.text = $"{GetRarityString(data.rarity)} · 炼金价值 {data.value}";
        tooltipDesc.text = data.description;

        tooltip.SetActive(true);
        isTooltipVisible = true;
        UpdateTooltipPosition();
    }

    void UpdateTooltipPosition()
    {
        var canvas = alchemyPanel.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var canvasRT = (RectTransform)canvas.transform;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, Input.mousePosition, null, out local);

        Vector2 pos = local + new Vector2(24f, -12f);

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
    // 小工具
    // ═══════════════════════════════════════════

    /// <summary>检查是否是药剂配方（产物直接入背包的那类）</summary>
    bool IsPotionRecipe(RecipeType recipeType)
    {
        switch (recipeType)
        {
            case RecipeType.HealingPotion:
            case RecipeType.SoulStabilizer:
            case RecipeType.SummonDurationPotion:
            case RecipeType.BurdenReliefIncense:
            case RecipeType.SummonEnhancer:
            case RecipeType.WeaknessInsightPotion:
            case RecipeType.MemoryResonancePotion:
            case RecipeType.BurdenPurification:
                return true;
            default:
                return false;
        }
    }

    string GetRarityString(string rarity)
    {
        switch (rarity)
        {
            case "Common": return "普通";
            case "Uncommon": return "精良";
            case "Rare": return "史诗";
            case "Legendary": return "传奇";
            default: return "未知";
        }
    }

    string GetRecipeTierString(RecipeTier tier)
    {
        switch (tier)
        {
            case RecipeTier.Basic: return "基础";
            case RecipeTier.Advanced: return "高级";
            case RecipeTier.Legendary: return "传奇";
            default: return "未知";
        }
    }
}

[System.Serializable]
public class MaterialData
{
    public MaterialTypeEnum type;
    public string name;
    public int value;
    public string description;
    public string rarity;
    public string obtainMethod;

    public MaterialData(MaterialTypeEnum type, string name, int value, string description, string rarity, string obtainMethod)
    {
        this.type = type;
        this.name = name;
        this.value = value;
        this.description = description;
        this.rarity = rarity;
        this.obtainMethod = obtainMethod;
    }
}
