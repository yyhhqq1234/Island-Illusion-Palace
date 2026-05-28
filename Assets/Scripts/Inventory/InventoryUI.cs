using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using GameSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("背包系统引用")]
    public InventorySystem inventorySystem; // 背包系统的引用，用于获取物品数据
    
    [Header("UI元素")]
    public GameObject inventoryPanel;         // 背包主面板
    public GameObject itemSlotPrefab;         // 物品格子预制体
    public GameObject itemTooltip;            // 物品提示框
    public GameObject itemInfoPanel;          // 物品信息显示面板
    
    [Header("三栏物品面板")]
    public GameObject weaponsPanel;           // 武器栏面板
    public GameObject consumablesPanel;       // 消耗品/药剂栏面板
    public GameObject materialsPanel;         // 材料栏面板
    
    [Header("三栏网格布局")]
    public GridLayoutGroup weaponsGrid;       // 武器栏网格布局
    public GridLayoutGroup consumablesGrid;   // 消耗品栏网格布局
    public GridLayoutGroup materialsGrid;     // 材料栏网格布局
    
    [Header("格子尺寸配置")]
    public Vector2 cellSize = new Vector2(92, 88);      // 格子大小: 92×88像素
    public Vector2 spacing = new Vector2(18, 18);        // 格子间距: 18像素
    
    [Header("当前状态")]
    private List<GameObject> weaponSlots = new List<GameObject>();
    private List<GameObject> consumableSlots = new List<GameObject>();
    private List<GameObject> materialSlots = new List<GameObject>();
    private InventoryItem hoveredItem = null;
    private InventoryItem selectedItem = null;
    
    [Header("提示框防抖设置")]
    public float tooltipHideDelay = 0.08f;              // 隐藏延迟（秒）- 防止子物体切换时闪烁
    private Coroutine hideTooltipCoroutine = null;       // 隐藏提示框的协程
    private bool isTooltipVisible = false;               // 提示框当前可见性状态
    private InventoryItem currentHoveredItem = null;     // 当前悬停的物品
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        // 检查itemTooltip引用
        if (itemTooltip == null)
        {
            Debug.LogError("[InventoryUI] ❌ itemTooltip 未在Inspector中设置！");
        }
        else
        {
            Debug.Log("[InventoryUI] ✅ itemTooltip 引用已设置: " + itemTooltip.name);
        }
        
        // 检查tooltipHideDelay值
        Debug.Log("[InventoryUI] ⏱️ tooltipHideDelay: " + tooltipHideDelay + "秒");
        
        InitializeGridSettings();
        UpdateInventoryUI();
        HideInventory();
        
        // 添加背包面板点击事件，点击空白区域时隐藏物品信息面板
        SetupInventoryPanelClickEvent();
    }

    void InitializeGridSettings()
    {
        // 配置所有网格布局组件：4列×6行=24格
        if (weaponsGrid != null)
        {
            weaponsGrid.cellSize = cellSize;           // 92×88像素
            weaponsGrid.spacing = spacing;             // 18像素间距
            weaponsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            weaponsGrid.constraintCount = 4;           // 每行4列（共6行24格）
        }
        
        if (consumablesGrid != null)
        {
            consumablesGrid.cellSize = cellSize;
            consumablesGrid.spacing = spacing;
            consumablesGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            consumablesGrid.constraintCount = 4;       // 每行4列（共6行24格）
        }
        
        if (materialsGrid != null)
        {
            materialsGrid.cellSize = cellSize;
            materialsGrid.spacing = spacing;
            materialsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            materialsGrid.constraintCount = 4;         // 每行4列（共6行24格）
        }
        
        Debug.Log($"[InventoryUI] 📐 网格布局初始化完成: {cellSize.x}×{cellSize.y}px | 间距: {spacing.x}px | 布局: 4列×6行=24格");
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
        
        // 固定提示框位置，不再跟随鼠标移动
        // if (hoveredItem != null && itemTooltip != null && itemTooltip.activeSelf)
        // {
        //     itemTooltip.transform.position = Input.mousePosition + new Vector3(10, -10, 0);
        // }
    }
    

    
    public void ShowInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            UpdateInventoryUI();
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
            HideItemInfo();
        }
    }
    

    
    public void UpdateInventoryUI()
    {
        // 清除现有物品格子
        ClearAllSlots();
        
        // 分别显示各栏目的物品
        DisplayWeapons();
        DisplayConsumables();
        DisplayMaterials();
    }
    
    void DisplayWeapons()
    {
        if (weaponsPanel == null) return;
        
        List<InventoryItem> weapons = inventorySystem.GetAllWeapons();
        foreach (var item in weapons)
        {
            CreateItemSlot(item, weaponsPanel.transform, weaponSlots);
        }
    }
    
    void DisplayConsumables()
    {
        if (consumablesPanel == null) return;
        
        List<InventoryItem> consumables = inventorySystem.inventory.FindAll(i => i.itemType == ItemType.Consumable);
        foreach (var item in consumables)
        {
            CreateItemSlot(item, consumablesPanel.transform, consumableSlots);
        }
    }
    
    void DisplayMaterials()
    {
        if (materialsPanel == null) return;
        
        List<InventoryItem> materials = inventorySystem.GetAllMaterials();
        foreach (var item in materials)
        {
            CreateItemSlot(item, materialsPanel.transform, materialSlots);
        }
        
        // 记忆碎片和特殊物品不在UI上显示，但仍然在背包中记录
    }
    

    
    void CreateItemSlot(InventoryItem item, Transform parent, List<GameObject> slotList)
    {
        if (itemSlotPrefab == null || parent == null)
            return;
        
        GameObject slot = Instantiate(itemSlotPrefab, parent);
        slotList.Add(slot);
        
        // 使用ItemSlot脚本来设置物品数据（支持图标放大7倍显示）
        // 使用反射或动态方式获取ItemSlot组件，避免编译错误
        var itemSlotScript = slot.GetComponent("ItemSlot") as MonoBehaviour;
        if (itemSlotScript != null)
        {
            // 调用ItemSlot脚本的SetItem方法，自动处理图标、数量和视觉效果
            // 使用反射调用SetItem方法，避免编译错误
            System.Reflection.MethodInfo setItemMethod = itemSlotScript.GetType().GetMethod("SetItem");
            if (setItemMethod != null)
            {
                setItemMethod.Invoke(itemSlotScript, new object[] { item });
            }
        }
        else
        {
            // 如果没有ItemSlot脚本，使用原来的逻辑
            SetItemSlotManually(slot, item);
        }
        
        // 添加点击事件 - 绑定到父物体 ItemSlotPrefab
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            // 确保Button组件的targetGraphic是父物体的Image，而不是子物体
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotButton.targetGraphic == null)
            {
                slotButton.targetGraphic = slotImage;
            }
            
            slotButton.onClick.AddListener(() => OnItemClick(item));
        }
        
        // 添加鼠标悬停事件 - 确保绑定到父物体 ItemSlotPrefab 而非子物体 Icon
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.gameObject.AddComponent<EventTrigger>();
        }
        
        // 清除现有的事件
        trigger.triggers.Clear();
        
        // 确保父物体可以接收射线检测（用于边界判断）
        CanvasGroup parentCanvasGroup = slot.GetComponent<CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = slot.gameObject.AddComponent<CanvasGroup>();
        }
        parentCanvasGroup.blocksRaycasts = true;   // 父物体必须接收射线
        parentCanvasGroup.alpha = 1f;
        
        // 确保父物体有碰撞器
        BoxCollider2D boxCollider = slot.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = slot.gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(92, 88); // 与格子大小匹配
            Debug.Log($"[InventoryUI] 📦 为格子 {slot.name} 添加了 BoxCollider2D");
        }
        
        // 确保父物体有 Image 组件（用于射线检测）
        Image parentImage = slot.GetComponent<Image>();
        if (parentImage == null)
        {
            parentImage = slot.gameObject.AddComponent<Image>();
            parentImage.color = new Color(0, 0, 0, 0); // 透明，不影响视觉
            Debug.Log($"[InventoryUI] 🖼️ 为格子 {slot.name} 添加了 Image 组件");
        }
        
        // 添加鼠标进入事件 - 基于父物体边界
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => 
        {
            Debug.Log($"[InventoryUI] 🖱️ 鼠标进入物品格子: {item.itemName} (父物体: {slot.name})");
            OnItemHover(item);
        });
        trigger.triggers.Add(enterEntry);
        
        // 添加鼠标离开事件 - 基于父物体边界
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => 
        {
            Debug.Log($"[InventoryUI] 🖱️ 鼠标离开物品格子: {item.itemName} (父物体: {slot.name})");
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitEntry);
        
        // 添加鼠标离开窗口事件（额外保险）
        EventTrigger.Entry exitWindowEntry = new EventTrigger.Entry();
        exitWindowEntry.eventID = EventTriggerType.PointerExit;
        exitWindowEntry.callback.AddListener((eventData) => 
        {
            Debug.Log($"[InventoryUI] 🖱️ 鼠标离开窗口: {item.itemName}");
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitWindowEntry);
    }
    
    void SetItemSlotManually(GameObject slot, InventoryItem item)
    {
        // 设置物品图标（原始逻辑）
        Image iconImage = slot.GetComponentInChildren<Image>();
        if (iconImage != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
        }
        
        // 设置物品数量
        Text quantityText = slot.GetComponentInChildren<Text>();
        if (quantityText != null && item.quantity > 1)
        {
            quantityText.text = item.quantity.ToString();
        }
        else if (quantityText != null)
        {
            quantityText.text = "";
        }
        
        // 设置装备状态视觉效果
        SetSlotVisualEffect(slot, item);
    }
    
    void SetSlotVisualEffect(GameObject slot, InventoryItem item)
    {
        Image backgroundImage = slot.GetComponent<Image>();
        if (backgroundImage == null) return;
        
        // 根据物品类型设置不同的背景颜色
        switch (item.itemType)
        {
            case ItemType.Weapon:
                backgroundImage.color = new Color(0.7f, 0.8f, 1f, 0.5f); // 浅蓝色-武器
                break;
            case ItemType.Consumable:
                backgroundImage.color = new Color(0.7f, 1f, 0.7f, 0.5f); // 浅绿色-消耗品
                break;
            case ItemType.Material:
                backgroundImage.color = new Color(1f, 1f, 0.7f, 0.5f); // 浅黄色-材料
                break;
            default:
                backgroundImage.color = new Color(0.9f, 0.9f, 0.9f, 0.5f); // 默认灰色
                break;
        }
        
        // 如果已装备，添加特殊高亮效果
        if (item.isEquipped)
        {
            backgroundImage.color = new Color(0.6f, 0.9f, 0.6f, 0.8f); // 绿色表示已装备
        }
    }
    
    void ClearAllSlots()
    {
        ClearSlotList(weaponSlots);
        ClearSlotList(consumableSlots);
        ClearSlotList(materialSlots);
    }
    
    void ClearSlotList(List<GameObject> slotList)
    {
        foreach (var slot in slotList)
        {
            if (slot != null)
                Destroy(slot);
        }
        slotList.Clear();
    }
    
    void OnItemClick(InventoryItem item)
    {
        selectedItem = item;
        
        switch (item.itemType)
        {
            case ItemType.Consumable:
                // 使用消耗品
                Debug.Log($"[InventoryUI] 点击使用消耗品: {item.itemName}");
                inventorySystem.UseItem(item);
                UpdateInventoryUI();
                ShowItemInfo(item);
                break;
            case ItemType.Weapon:
                // 装备武器
                inventorySystem.EquipItem(item);
                UpdateInventoryUI();
                ShowItemInfo(item);
                break;
            case ItemType.Material:
            case ItemType.MemoryFragment:
                // 材料和记忆碎片只能查看
                ShowItemInfo(item);
                break;
        }
    }
    
    void OnItemHover(InventoryItem item)
    {
        // 1. 取消正在进行的隐藏协程（防止从子物体移入时闪烁）
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        // 2. 如果已经显示的是同一个物品，不需要重新显示
        if (currentHoveredItem == item && isTooltipVisible)
        {
            return;
        }
        
        // 3. 立即显示提示框（无延迟）
        currentHoveredItem = item;
        ShowTooltipImmediate(item);
    }
    
    void OnItemHoverExit()
    {
        // 4. 启动延迟隐藏协程（防止子物体切换时闪烁）
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            Debug.Log("[InventoryUI] ⏹️ 取消正在进行的隐藏协程");
        }
        hideTooltipCoroutine = StartCoroutine(DelayedHideTooltip());
        Debug.Log("[InventoryUI] 🚀 启动延迟隐藏协程 (延迟: " + tooltipHideDelay + "秒)");
    }
    
    System.Collections.IEnumerator DelayedHideTooltip()
    {
        // 等待一小段延迟，避免子物体切换导致的闪烁
        Debug.Log("[InventoryUI] ⏰ 开始等待隐藏延迟: " + tooltipHideDelay + "秒, Time.timeScale: " + Time.timeScale);
        
        // 即使游戏暂停也能正常工作
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < tooltipHideDelay)
        {
            yield return null;
        }
        
        // 隐藏提示框
        Debug.Log("[InventoryUI] 🎯 延迟结束，执行隐藏操作");
        HideTooltipImmediate();
        hideTooltipCoroutine = null;
        currentHoveredItem = null;
        Debug.Log("[InventoryUI] ✅ 隐藏操作完成");
    }
    
    void ShowTooltipImmediate(InventoryItem item)
    {
        hoveredItem = item;
        
        if (itemTooltip == null)
            return;
        
        // 避免重复设置active状态导致频闪
        if (!itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(true);
            isTooltipVisible = true;
        }
        
        // 设置提示内容
        // 查找所有Text组件并设置内容
        Text[] tooltipTexts = itemTooltip.GetComponentsInChildren<Text>();
        if (tooltipTexts.Length > 0)
        {
            // 第1个Text：物品名称
            tooltipTexts[0].text = item.itemName;
            
            if (tooltipTexts.Length > 1)
            {
                // 第2个Text：炼金价值
                tooltipTexts[1].text = $"炼金价值：{item.value}";
                
                if (tooltipTexts.Length > 2)
                {
                    // 第3个Text：稀有度 + 描述
                    string rarityStr = GetRarityString(item.rarity);
                    string description = item.GetDescription();
                    tooltipTexts[2].text = $"{rarityStr} {description}";
                }
            }
        }
        

        
        // 设置固定提示位置（使用localPosition确保在UI画布中正确显示）
        RectTransform tooltipRect = itemTooltip.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            tooltipRect.localPosition = new Vector3(800, 400, 0);
        }
        else
        {
            // 兼容旧版本
            itemTooltip.transform.localPosition = new Vector3(800, 400, 0);
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
        Debug.Log("[InventoryUI] 🔍 开始隐藏提示框 - itemTooltip: " + (itemTooltip != null) + ", active: " + (itemTooltip != null ? itemTooltip.activeSelf : false));
        
        if (itemTooltip != null && itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(false);
            isTooltipVisible = false;
            Debug.Log("[InventoryUI] ✅ 提示框已隐藏");
        }
        else if (itemTooltip == null)
        {
            Debug.Log("[InventoryUI] ❌ itemTooltip 引用为空");
        }
        else if (!itemTooltip.activeSelf)
        {
            Debug.Log("[InventoryUI] ℹ️ 提示框已经是隐藏状态");
        }
        
        hoveredItem = null;
        Debug.Log("[InventoryUI] ✅ hoveredItem 已清空");
    }
    
    void ShowItemInfo(InventoryItem item)
    {
        if (itemInfoPanel == null || item == null)
            return;
        
        itemInfoPanel.SetActive(true);
        
        // 显示物品详细信息
        Text[] infoTexts = itemInfoPanel.GetComponentsInChildren<Text>();
        if (infoTexts.Length > 0)
        {
            infoTexts[0].text = item.itemName; // 名称
            
            if (infoTexts.Length > 1)
            {
                infoTexts[1].text = item.GetDescription(); // 描述
                
                if (infoTexts.Length > 2)
                {
                    infoTexts[2].text = $"数量: {item.quantity}"; // 数量
                    
                    if (infoTexts.Length > 3)
                    {
                        infoTexts[3].text = $"类型: {GetItemTypeString(item.itemType)}"; // 类型
                        
                        if (infoTexts.Length > 4)
                        {
                            infoTexts[4].text = $"价值: {item.value}金币"; // 价值
                            
                            if (infoTexts.Length > 5)
                            {
                                // 特殊属性信息
                                string extraInfo = GetExtraItemInfo(item);
                                infoTexts[5].text = extraInfo;
                            }
                        }
                    }
                }
            }
        }
        
        // 设置物品图标
        Image[] infoImages = itemInfoPanel.GetComponentsInChildren<Image>();
        if (infoImages.Length > 1 && item.icon != null)
        {
            infoImages[1].sprite = item.icon; // 第二个Image组件是物品图标
        }
    }
    
    string GetExtraItemInfo(InventoryItem item)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon:
                return $"等级: {item.weaponLevel}\n状态: {(item.isEquipped ? "已装备" : "未装备")}";
                
            case ItemType.Consumable:
                return $"效果: {item.description}";
                
            case ItemType.Material:
                return $"材料类型: {item.materialType}";
                
            case ItemType.MemoryFragment:
                return "神秘的记忆碎片";
                
            default:
                return "";
        }
    }
    
    void HideItemInfo()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(false);
        }
        selectedItem = null;
    }
    
    // 检查背包是否打开
    public bool IsInventoryOpen()
    {
        return inventoryPanel != null && inventoryPanel.activeSelf;
    }
    
    void SetupInventoryPanelClickEvent()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("[InventoryUI] ❌ inventoryPanel 未在Inspector中设置，无法添加点击事件");
            return;
        }
        
        // 确保背包面板有Image组件（用于射线检测）
        Image panelImage = inventoryPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = inventoryPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0); // 透明，不影响视觉
            Debug.Log("[InventoryUI] 🖼️ 为背包面板添加了Image组件");
        }
        
        // 确保背包面板有CanvasGroup组件
        CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = true; // 确保能够接收射线检测
        
        // 添加EventTrigger组件
        EventTrigger trigger = inventoryPanel.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = inventoryPanel.AddComponent<EventTrigger>();
        }
        
        // 添加点击事件
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((eventData) => 
        {
            PointerEventData pointerEventData = eventData as PointerEventData;
            if (pointerEventData != null)
            {
                // 检查点击的对象是否是背包面板本身
                if (pointerEventData.pointerPress == inventoryPanel)
                {
                    Debug.Log("[InventoryUI] 🖱️ 点击了背包面板空白区域，隐藏物品信息面板");
                    HideItemInfo();
                }
            }
        });
        trigger.triggers.Add(clickEntry);
        
        Debug.Log("[InventoryUI] ✅ 背包面板点击事件已设置");
    }
    
    // 获取当前选中的物品
    public InventoryItem GetSelectedItem()
    {
        return selectedItem;
    }
}
