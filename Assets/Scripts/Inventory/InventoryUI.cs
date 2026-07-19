using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using GameSystems;

/// <summary>
/// 背包UI系统 - 8列网格+分类过滤+排序搜索+详情面板+底部信息栏
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("背包系统引用")]
    public InventorySystem inventorySystem; // 背包系统的引用
    
    [Header("UI元素")]
    public GameObject inventoryPanel;         // 背包主面板
    public GameObject itemSlotPrefab;         // 物品格子预制体
    public GameObject itemTooltip;            // 物品提示框
    
    [Header("统一网格布局")]
    public GameObject unifiedGridPanel;       // 统一网格面板
    public GridLayoutGroup unifiedGrid;       // 8列统一网格布局
    public ScrollRect gridScrollRect;         // 网格滚动区域
    
    [Header("分类过滤栏")]
    public GameObject filterBar;              // 分类过滤栏面板
    public Button filterAllBtn;               // 全部按钮
    public Button filterWeaponBtn;            // 武器按钮
    public Button filterConsumableBtn;        // 消耗品按钮
    public Button filterMaterialBtn;          // 材料按钮
    public Button filterSoulCoreBtn;          // 灵魂之核按钮
    public Button filterKeyItemBtn;           // 关键物品按钮
    
    [Header("分类按钮文本")]
    public Text filterAllText;                // 全部按钮文本
    public Text filterWeaponText;             // 武器按钮文本
    public Text filterConsumableText;         // 消耗品按钮文本
    public Text filterMaterialText;           // 材料按钮文本
    public Text filterSoulCoreText;           // 灵魂之核按钮文本
    public Text filterKeyItemText;            // 关键物品按钮文本
    
    [Header("排序/搜索栏")]
    public GameObject sortSearchBar;          // 排序搜索栏面板
    public Button sortNameBtn;                // 名称排序按钮
    public Button sortRarityBtn;              // 稀有度排序按钮
    public Button sortQuantityBtn;            // 数量排序按钮
    public Button sortValueBtn;               // 价值排序按钮
    public InputField searchInput;            // 搜索输入框
    public Button organizeBtn;                // 整理按钮
    
    [Header("排序按钮文本")]
    public Text sortNameText;                 // 名称排序文本
    public Text sortRarityText;               // 稀有度排序文本
    public Text sortQuantityText;             // 数量排序文本
    public Text sortValueText;                // 价值排序文本
    
    [Header("物品详情面板")]
    public GameObject itemDetailPanel;        // 物品详情面板
    public Image detailIcon;                  // 大图标(128x128)
    public Text detailName;                   // 物品名称
    public Text detailRarity;                 // 稀有度
    public Text detailType;                   // 类型
    public Text detailQuantity;               // 数量
    public Text detailDescription;            // 描述
    public Button useBtn;                     // 使用按钮
    public Button equipBtn;                   // 装备按钮
    
    [Header("底部信息栏")]
    public GameObject bottomInfoBar;          // 底部信息栏面板
    public Text soulCurrencyText;             // 灵魂货币文本
    public Text materialCountText;            // 材料总数/容量上限
    public Text burdenText;                   // 当前负重/负重上限
    
    [Header("格子尺寸配置")]
    public Vector2 cellSize = new Vector2(72, 72);      // 格子大小: 72×72像素
    public Vector2 spacing = new Vector2(8, 8);          // 格子间距: 8像素
    public int gridColumns = 8;                          // 网格列数
    public int visibleRows = 6;                          // 可见行数
    
    [Header("格子颜色配置")]
    public Color emptySlotColor = new Color(0.118f, 0.118f, 0.180f);      // #1E1E2E 空格子背景
    public Color normalSlotColor = new Color(0.165f, 0.165f, 0.227f);     // #2A2A3A 有物品格子背景
    public Color selectedBorderColor = new Color(0.267f, 0.541f, 1f);     // #448AFF 选中边框
    public Color hoverBorderColor = new Color(0.4f, 0.6f, 1f, 0.6f);      // 悬停边框
    
    [Header("当前状态")]
    private List<GameObject> allSlots = new List<GameObject>();
    private List<InventoryItem> displayedItems = new List<InventoryItem>();
    private InventoryItem hoveredItem = null;
    private InventoryItem selectedItem = null;
    private InventoryItem currentHoveredItem = null;
    
    [Header("过滤和排序状态")]
    private ItemFilterType currentFilter = ItemFilterType.All;
    private SortType currentSort = SortType.Name;
    private bool sortAscending = true;
    private string searchText = "";
    
    [Header("提示框防抖设置")]
    public float tooltipHideDelay = 0.08f;
    private Coroutine hideTooltipCoroutine = null;
    private bool isTooltipVisible = false;
    
    /// <summary>
    /// 物品过滤类型
    /// </summary>
    enum ItemFilterType
    {
        All,            // 全部
        Weapon,         // 武器
        Consumable,     // 消耗品
        Material,       // 材料
        SoulCore,       // 灵魂之核
        KeyItem         // 关键物品
    }
    
    /// <summary>
    /// 排序类型
    /// </summary>
    enum SortType
    {
        Name,           // 名称
        Rarity,         // 稀有度
        Quantity,       // 数量
        Value           // 价值
    }
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        // 检查关键UI引用
        CheckUIReferences();
        
        // 初始化网格设置
        InitializeGridSettings();
        
        // 初始化过滤和排序按钮
        InitializeFilterAndSortButtons();
        
        // 初始化搜索框
        InitializeSearchInput();
        
        // 更新UI
        UpdateInventoryUI();
        UpdateBottomInfoBar();
        
        // 隐藏UI
        HideInventory();
        HideItemDetailPanel();
        
        // 设置背包面板点击事件
        SetupInventoryPanelClickEvent();
        
        Debug.Log($"[InventoryUI] 背包UI初始化完成: {gridColumns}列×{visibleRows}行 | 格子尺寸: {cellSize.x}×{cellSize.y}px");
    }
    
    /// <summary>
    /// 检查UI引用
    /// </summary>
    void CheckUIReferences()
    {
        if (itemTooltip == null)
        {
            Debug.LogWarning("[InventoryUI] itemTooltip 未在Inspector中设置");
        }
        
        if (unifiedGrid == null)
        {
            Debug.LogWarning("[InventoryUI] unifiedGrid 未在Inspector中设置");
        }
        
        if (itemDetailPanel == null)
        {
            Debug.LogWarning("[InventoryUI] itemDetailPanel 未在Inspector中设置");
        }
    }
    
    /// <summary>
    /// 初始化网格设置
    /// </summary>
    void InitializeGridSettings()
    {
        if (unifiedGrid != null)
        {
            unifiedGrid.cellSize = cellSize;
            unifiedGrid.spacing = spacing;
            unifiedGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            unifiedGrid.constraintCount = gridColumns;
        }
    }
    
    /// <summary>
    /// 初始化过滤和排序按钮
    /// </summary>
    void InitializeFilterAndSortButtons()
    {
        // 分类过滤按钮
        if (filterAllBtn != null)
            filterAllBtn.onClick.AddListener(() => SetFilter(ItemFilterType.All));
        if (filterWeaponBtn != null)
            filterWeaponBtn.onClick.AddListener(() => SetFilter(ItemFilterType.Weapon));
        if (filterConsumableBtn != null)
            filterConsumableBtn.onClick.AddListener(() => SetFilter(ItemFilterType.Consumable));
        if (filterMaterialBtn != null)
            filterMaterialBtn.onClick.AddListener(() => SetFilter(ItemFilterType.Material));
        if (filterSoulCoreBtn != null)
            filterSoulCoreBtn.onClick.AddListener(() => SetFilter(ItemFilterType.SoulCore));
        if (filterKeyItemBtn != null)
            filterKeyItemBtn.onClick.AddListener(() => SetFilter(ItemFilterType.KeyItem));
        
        // 排序按钮
        if (sortNameBtn != null)
            sortNameBtn.onClick.AddListener(() => SetSort(SortType.Name));
        if (sortRarityBtn != null)
            sortRarityBtn.onClick.AddListener(() => SetSort(SortType.Rarity));
        if (sortQuantityBtn != null)
            sortQuantityBtn.onClick.AddListener(() => SetSort(SortType.Quantity));
        if (sortValueBtn != null)
            sortValueBtn.onClick.AddListener(() => SetSort(SortType.Value));
        
        // 整理按钮
        if (organizeBtn != null)
            organizeBtn.onClick.AddListener(OrganizeItems);
        
        // 更新过滤按钮文本
        UpdateFilterButtonText();
        UpdateSortButtonText();
    }
    
    /// <summary>
    /// 初始化搜索输入框
    /// </summary>
    void InitializeSearchInput()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(OnSearchTextChanged);
            searchInput.placeholder.gameObject.GetComponent<Text>().text = "搜索物品...";
        }
    }
    
    /// <summary>
    /// 设置过滤类型
    /// </summary>
    void SetFilter(ItemFilterType filter)
    {
        currentFilter = filter;
        UpdateFilterButtonText();
        UpdateInventoryUI();
    }
    
    /// <summary>
    /// 设置排序类型
    /// </summary>
    void SetSort(SortType sort)
    {
        if (currentSort == sort)
        {
            // 点击相同排序按钮切换升降序
            sortAscending = !sortAscending;
        }
        else
        {
            currentSort = sort;
            sortAscending = true; // 默认升序
        }
        
        UpdateSortButtonText();
        UpdateInventoryUI();
    }
    
    /// <summary>
    /// 搜索文本变化回调
    /// </summary>
    void OnSearchTextChanged(string text)
    {
        searchText = text;
        UpdateInventoryUI();
    }
    
    /// <summary>
    /// 更新过滤按钮文本（显示数量）
    /// </summary>
    void UpdateFilterButtonText()
    {
        if (inventorySystem == null) return;
        
        var allItems = inventorySystem.GetAllItems();
        
        if (filterAllText != null)
            filterAllText.text = $"全部({allItems.Count})";
        if (filterWeaponText != null)
            filterWeaponText.text = $"武器({allItems.Count(i => i.itemType == ItemType.Weapon)})";
        if (filterConsumableText != null)
            filterConsumableText.text = $"消耗品({allItems.Count(i => i.itemType == ItemType.Consumable)})";
        if (filterMaterialText != null)
            filterMaterialText.text = $"材料({allItems.Count(i => i.itemType == ItemType.Material)})";
        if (filterSoulCoreText != null)
            filterSoulCoreText.text = $"灵魂之核({GetSoulCoreCount()})";
        if (filterKeyItemText != null)
            filterKeyItemText.text = $"关键物品({GetKeyItemCount()})";
        
        // 高亮当前选中的过滤按钮
        HighlightFilterButton();
    }
    
    /// <summary>
    /// 高亮当前选中的过滤按钮
    /// </summary>
    void HighlightFilterButton()
    {
        // 重置所有按钮颜色
        ResetButtonColor(filterAllBtn);
        ResetButtonColor(filterWeaponBtn);
        ResetButtonColor(filterConsumableBtn);
        ResetButtonColor(filterMaterialBtn);
        ResetButtonColor(filterSoulCoreBtn);
        ResetButtonColor(filterKeyItemBtn);
        
        // 高亮选中的按钮
        Button selectedBtn = null;
        switch (currentFilter)
        {
            case ItemFilterType.All: selectedBtn = filterAllBtn; break;
            case ItemFilterType.Weapon: selectedBtn = filterWeaponBtn; break;
            case ItemFilterType.Consumable: selectedBtn = filterConsumableBtn; break;
            case ItemFilterType.Material: selectedBtn = filterMaterialBtn; break;
            case ItemFilterType.SoulCore: selectedBtn = filterSoulCoreBtn; break;
            case ItemFilterType.KeyItem: selectedBtn = filterKeyItemBtn; break;
        }
        
        if (selectedBtn != null)
        {
            var colors = selectedBtn.colors;
            colors.normalColor = new Color(0.3f, 0.5f, 1f, 0.3f);
            selectedBtn.colors = colors;
        }
    }
    
    /// <summary>
    /// 重置按钮颜色
    /// </summary>
    void ResetButtonColor(Button btn)
    {
        if (btn != null)
        {
            var colors = btn.colors;
            colors.normalColor = new Color(1, 1, 1, 0.1f);
            btn.colors = colors;
        }
    }
    
    /// <summary>
    /// 更新排序按钮文本
    /// </summary>
    void UpdateSortButtonText()
    {
        string arrow = sortAscending ? "↑" : "↓";
        
        if (sortNameText != null)
            sortNameText.text = $"名称{arrow}";
        if (sortRarityText != null)
            sortRarityText.text = $"稀有度{(currentSort == SortType.Rarity ? arrow : "")}";
        if (sortQuantityText != null)
            sortQuantityText.text = $"数量{(currentSort == SortType.Quantity ? arrow : "")}";
        if (sortValueText != null)
            sortValueText.text = $"价值{(currentSort == SortType.Value ? arrow : "")}";
    }
    
    /// <summary>
    /// 获取灵魂之核数量
    /// </summary>
    int GetSoulCoreCount()
    {
        if (inventorySystem == null) return 0;
        // 灵魂之核暂时返回0，后续可扩展
        return 0;
    }
    
    /// <summary>
    /// 获取关键物品数量
    /// </summary>
    int GetKeyItemCount()
    {
        if (inventorySystem == null) return 0;
        // 关键物品暂时返回记忆碎片数量
        return inventorySystem.GetAllMemoryFragments().Count;
    }
    
    /// <summary>
    /// 整理物品（按稀有度降序→类型分组→名称字母序）
    /// </summary>
    void OrganizeItems()
    {
        currentSort = SortType.Rarity;
        sortAscending = false; // 降序
        UpdateSortButtonText();
        UpdateInventoryUI();
        Debug.Log("[InventoryUI] 物品已整理");
    }
    
    void Update()
    {
        // 处理I键切换背包
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel.activeSelf)
            {
                HideInventory();
            }
            else
            {
                ShowInventory();
            }
        }
        
        // 处理ESC键关闭背包
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryPanel.activeSelf)
            {
                HideInventory();
            }
        }
    }
    
    public void ShowInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            UpdateInventoryUI();
            UpdateBottomInfoBar();
            Time.timeScale = 0f; // 暂停游戏
        }
    }
    
    public void HideInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            Time.timeScale = 1f; // 恢复游戏
            
            // 立即停止所有协程并隐藏提示框
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
    }
    
    /// <summary>
    /// 更新背包UI
    /// </summary>
    public void UpdateInventoryUI()
    {
        if (inventorySystem == null) return;
        
        // 清除现有物品格子
        ClearAllSlots();
        
        // 获取并过滤物品
        List<InventoryItem> filteredItems = GetFilteredItems();
        
        // 排序物品
        List<InventoryItem> sortedItems = SortItems(filteredItems);
        
        // 显示物品
        displayedItems = sortedItems;
        DisplayItems(sortedItems);
        
        // 更新过滤按钮文本
        UpdateFilterButtonText();
    }
    
    /// <summary>
    /// 获取过滤后的物品列表
    /// </summary>
    List<InventoryItem> GetFilteredItems()
    {
        var allItems = inventorySystem.GetAllItems();
        var filtered = new List<InventoryItem>();
        
        foreach (var item in allItems)
        {
            // 应用分类过滤
            if (!MatchFilter(item))
                continue;
            
            // 应用搜索过滤
            if (!string.IsNullOrEmpty(searchText) && !item.itemName.ToLower().Contains(searchText.ToLower()))
                continue;
            
            filtered.Add(item);
        }
        
        return filtered;
    }
    
    /// <summary>
    /// 检查物品是否匹配当前过滤条件
    /// </summary>
    bool MatchFilter(InventoryItem item)
    {
        switch (currentFilter)
        {
            case ItemFilterType.All:
                return true;
            case ItemFilterType.Weapon:
                return item.itemType == ItemType.Weapon;
            case ItemFilterType.Consumable:
                return item.itemType == ItemType.Consumable;
            case ItemFilterType.Material:
                return item.itemType == ItemType.Material;
            case ItemFilterType.SoulCore:
                // 灵魂之核逻辑（后续扩展）
                return false;
            case ItemFilterType.KeyItem:
                return item.itemType == ItemType.MemoryFragment;
            default:
                return true;
        }
    }
    
    /// <summary>
    /// 排序物品列表
    /// </summary>
    List<InventoryItem> SortItems(List<InventoryItem> items)
    {
        var sorted = new List<InventoryItem>(items);
        
        switch (currentSort)
        {
            case SortType.Name:
                sorted = sortAscending 
                    ? sorted.OrderBy(i => i.itemName).ToList()
                    : sorted.OrderByDescending(i => i.itemName).ToList();
                break;
                
            case SortType.Rarity:
                sorted = sortAscending
                    ? sorted.OrderBy(i => i.rarity).ToList()
                    : sorted.OrderByDescending(i => i.rarity).ToList();
                break;
                
            case SortType.Quantity:
                sorted = sortAscending
                    ? sorted.OrderBy(i => i.quantity).ToList()
                    : sorted.OrderByDescending(i => i.quantity).ToList();
                break;
                
            case SortType.Value:
                sorted = sortAscending
                    ? sorted.OrderBy(i => i.value).ToList()
                    : sorted.OrderByDescending(i => i.value).ToList();
                break;
        }
        
        return sorted;
    }
    
    /// <summary>
    /// 显示物品到统一网格
    /// </summary>
    void DisplayItems(List<InventoryItem> items)
    {
        if (unifiedGrid == null) return;
        
        foreach (var item in items)
        {
            CreateItemSlot(item, unifiedGrid.transform, allSlots);
        }
        
        // 填充空格子以达到可见行数
        int emptySlotsNeeded = gridColumns * visibleRows - items.Count;
        for (int i = 0; i < emptySlotsNeeded; i++)
        {
            CreateEmptySlot(unifiedGrid.transform, allSlots);
        }
    }
    
    /// <summary>
    /// 创建物品格子
    /// </summary>
    void CreateItemSlot(InventoryItem item, Transform parent, List<GameObject> slotList)
    {
        if (itemSlotPrefab == null || parent == null)
            return;
        
        GameObject slot = Instantiate(itemSlotPrefab, parent);
        slotList.Add(slot);
        
        // 设置格子样式
        SetupSlotStyle(slot, item);
        
        // 使用ItemSlot脚本来设置物品数据
        var itemSlotScript = slot.GetComponent("ItemSlot") as MonoBehaviour;
        if (itemSlotScript != null)
        {
            System.Reflection.MethodInfo setItemMethod = itemSlotScript.GetType().GetMethod("SetItem");
            if (setItemMethod != null)
            {
                setItemMethod.Invoke(itemSlotScript, new object[] { item });
            }
        }
        else
        {
            SetItemSlotManually(slot, item);
        }
        
        // 添加点击事件
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotButton.targetGraphic == null)
            {
                slotButton.targetGraphic = slotImage;
            }
            
            slotButton.onClick.AddListener(() => OnItemClick(item));
        }
        
        // 添加鼠标悬停事件
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.gameObject.AddComponent<EventTrigger>();
        }
        
        trigger.triggers.Clear();
        
        CanvasGroup parentCanvasGroup = slot.GetComponent<CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = slot.gameObject.AddComponent<CanvasGroup>();
        }
        parentCanvasGroup.blocksRaycasts = true;
        parentCanvasGroup.alpha = 1f;
        
        // 添加鼠标进入事件
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => 
        {
            OnItemHover(item);
        });
        trigger.triggers.Add(enterEntry);
        
        // 添加鼠标离开事件
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => 
        {
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitEntry);
    }
    
    /// <summary>
    /// 创建空格子
    /// </summary>
    void CreateEmptySlot(Transform parent, List<GameObject> slotList)
    {
        if (itemSlotPrefab == null || parent == null)
            return;
        
        GameObject slot = Instantiate(itemSlotPrefab, parent);
        slotList.Add(slot);
        
        // 设置空格子样式
        Image bgImage = slot.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = emptySlotColor;
        }
        
        // 禁用交互
        Button btn = slot.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = false;
        }
        
        // 清空图标
        Image iconImage = slot.GetComponentInChildren<Image>();
        if (iconImage != null && iconImage.gameObject != slot)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(0, 0, 0, 0);
        }
        
        // 清空数量文本
        Text quantityText = slot.GetComponentInChildren<Text>();
        if (quantityText != null)
        {
            quantityText.text = "";
        }
    }
    
    /// <summary>
    /// 设置格子样式
    /// </summary>
    void SetupSlotStyle(GameObject slot, InventoryItem item)
    {
        Image bgImage = slot.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = normalSlotColor;
        }
        
        // 如果是选中物品，添加边框效果
        if (selectedItem == item)
        {
            AddSelectedBorder(slot);
        }
    }
    
    /// <summary>
    /// 添加选中边框效果
    /// </summary>
    void AddSelectedBorder(GameObject slot)
    {
        // 查找或创建边框Image
        Transform borderTransform = slot.transform.Find("Border");
        Image borderImage;
        
        if (borderTransform == null)
        {
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(slot.transform, false);
            
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);
            
            borderImage = borderObj.AddComponent<Image>();
            borderImage.color = selectedBorderColor;
        }
        else
        {
            borderImage = borderTransform.GetComponent<Image>();
            if (borderImage != null)
            {
                borderImage.color = selectedBorderColor;
            }
        }
    }
    
    /// <summary>
    /// 手动设置物品格子
    /// </summary>
    void SetItemSlotManually(GameObject slot, InventoryItem item)
    {
        Image iconImage = slot.GetComponentInChildren<Image>();
        if (iconImage != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
        }
        
        Text quantityText = slot.GetComponentInChildren<Text>();
        if (quantityText != null && item.quantity > 1)
        {
            quantityText.text = item.quantity.ToString();
        }
        else if (quantityText != null)
        {
            quantityText.text = "";
        }
    }
    
    void ClearAllSlots()
    {
        foreach (var slot in allSlots)
        {
            if (slot != null)
                Destroy(slot);
        }
        allSlots.Clear();
    }
    
    void OnItemClick(InventoryItem item)
    {
        selectedItem = item;
        
        // 更新格子选中状态
        UpdateSlotSelection();
        
        // 显示物品详情面板
        ShowItemDetailPanel(item);
        
        switch (item.itemType)
        {
            case ItemType.Consumable:
                Debug.Log($"[InventoryUI] 点击使用消耗品: {item.itemName}");
                inventorySystem.UseItem(item);
                UpdateInventoryUI();
                break;
            case ItemType.Weapon:
                inventorySystem.EquipItem(item);
                UpdateInventoryUI();
                break;
            case ItemType.Material:
            case ItemType.MemoryFragment:
                // 材料和记忆碎片只能查看
                break;
        }
    }
    
    /// <summary>
    /// 更新格子选中状态
    /// </summary>
    void UpdateSlotSelection()
    {
        foreach (var slot in allSlots)
        {
            if (slot == null) continue;
            
            // 移除旧的边框
            Transform borderTransform = slot.transform.Find("Border");
            if (borderTransform != null)
            {
                Destroy(borderTransform.gameObject);
            }
        }
        
        // 为选中物品添加边框
        // 这里简化处理，实际应该根据物品找到对应的格子
    }
    
    void OnItemHover(InventoryItem item)
    {
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        if (currentHoveredItem == item && isTooltipVisible)
        {
            return;
        }
        
        currentHoveredItem = item;
        ShowTooltipImmediate(item);
    }
    
    void OnItemHoverExit()
    {
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
        }
        hideTooltipCoroutine = StartCoroutine(DelayedHideTooltip());
    }
    
    System.Collections.IEnumerator DelayedHideTooltip()
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < tooltipHideDelay)
        {
            yield return null;
        }
        
        HideTooltipImmediate();
        hideTooltipCoroutine = null;
        currentHoveredItem = null;
    }
    
    void ShowTooltipImmediate(InventoryItem item)
    {
        hoveredItem = item;
        
        if (itemTooltip == null)
            return;
        
        if (!itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(true);
            isTooltipVisible = true;
        }
        
        Text[] tooltipTexts = itemTooltip.GetComponentsInChildren<Text>();
        if (tooltipTexts.Length > 0)
        {
            tooltipTexts[0].text = item.itemName;
            
            if (tooltipTexts.Length > 1)
            {
                tooltipTexts[1].text = $"炼金价值：{item.value}";
                
                if (tooltipTexts.Length > 2)
                {
                    string rarityStr = GetRarityString(item.rarity);
                    string description = item.GetDescription();
                    tooltipTexts[2].text = $"{rarityStr} {description}";
                }
            }
        }
        
        RectTransform tooltipRect = itemTooltip.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            tooltipRect.localPosition = new Vector3(800, 400, 0);
        }
    }
    
    string GetItemTypeString(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon:
                return "武器";
            case ItemType.Consumable:
                return "消耗品";
            case ItemType.Material:
                return "材料";
            case ItemType.MemoryFragment:
                return "记忆碎片";
            default:
                return "未知";
        }
    }
    
    string GetRarityString(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "普通";
            case ItemRarity.Uncommon:
                return "精良";
            case ItemRarity.Rare:
                return "史诗";
            case ItemRarity.Legendary:
                return "传奇";
            default:
                return "未知";
        }
    }
    
    void HideTooltipImmediate()
    {
        if (itemTooltip != null && itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(false);
            isTooltipVisible = false;
        }
        
        hoveredItem = null;
    }
    
    /// <summary>
    /// 显示物品详情面板
    /// </summary>
    void ShowItemDetailPanel(InventoryItem item)
    {
        if (itemDetailPanel == null || item == null)
            return;
        
        itemDetailPanel.SetActive(true);
        
        // 设置图标
        if (detailIcon != null && item.icon != null)
        {
            detailIcon.sprite = item.icon;
        }
        
        // 设置名称
        if (detailName != null)
        {
            detailName.text = item.itemName;
        }
        
        // 设置稀有度
        if (detailRarity != null)
        {
            detailRarity.text = GetRarityString(item.rarity);
            detailRarity.color = GetRarityColor(item.rarity);
        }
        
        // 设置类型
        if (detailType != null)
        {
            detailType.text = GetItemTypeString(item.itemType);
        }
        
        // 设置数量
        if (detailQuantity != null)
        {
            detailQuantity.text = $"数量: {item.quantity}";
        }
        
        // 设置描述
        if (detailDescription != null)
        {
            detailDescription.text = item.GetDescription();
        }
        
        // 设置按钮可见性
        if (useBtn != null)
        {
            useBtn.gameObject.SetActive(item.itemType == ItemType.Consumable);
        }
        
        if (equipBtn != null)
        {
            equipBtn.gameObject.SetActive(item.itemType == ItemType.Weapon);
        }
    }
    
    /// <summary>
    /// 隐藏物品详情面板
    /// </summary>
    void HideItemDetailPanel()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
        selectedItem = null;
    }
    
    /// <summary>
    /// 获取稀有度颜色
    /// </summary>
    Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
            case ItemRarity.Uncommon:
                return new Color(0.3f, 0.8f, 0.3f); // 绿色
            case ItemRarity.Rare:
                return new Color(0.6f, 0.4f, 1f);   // 紫色
            case ItemRarity.Legendary:
                return new Color(1f, 0.8f, 0.2f);   // 金色
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// 更新底部信息栏
    /// </summary>
    void UpdateBottomInfoBar()
    {
        if (inventorySystem == null) return;
        
        // 灵魂货币
        if (soulCurrencyText != null)
        {
            soulCurrencyText.text = $"灵魂: {inventorySystem.currentSouls}";
        }
        
        // 材料总数/容量上限
        if (materialCountText != null)
        {
            int materialCount = inventorySystem.GetAllMaterials().Count;
            int maxMaterials = inventorySystem.maxMaterialSlots;
            materialCountText.text = $"材料: {materialCount}/{maxMaterials}";
        }
        
        // 负重（暂时用物品数量代替，后续可扩展）
        if (burdenText != null)
        {
            int currentWeight = inventorySystem.currentInventorySize;
            int maxWeight = inventorySystem.maxMaterialSlots + inventorySystem.maxConsumableSlots + 
                           inventorySystem.maxWeaponSlots + inventorySystem.maxMemoryFragmentSlots;
            burdenText.text = $"负重: {currentWeight}/{maxWeight}";
        }
    }
    
    public bool IsInventoryOpen()
    {
        return inventoryPanel != null && inventoryPanel.activeSelf;
    }
    
    void SetupInventoryPanelClickEvent()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("[InventoryUI] inventoryPanel 未在Inspector中设置");
            return;
        }
        
        Image panelImage = inventoryPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = inventoryPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0);
        }
        
        CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = true;
        
        EventTrigger trigger = inventoryPanel.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = inventoryPanel.AddComponent<EventTrigger>();
        }
        
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((eventData) => 
        {
            PointerEventData pointerEventData = eventData as PointerEventData;
            if (pointerEventData != null)
            {
                if (pointerEventData.pointerPress == inventoryPanel)
                {
                    HideItemDetailPanel();
                }
            }
        });
        trigger.triggers.Add(clickEntry);
    }
    
    public InventoryItem GetSelectedItem()
    {
        return selectedItem;
    }
}
