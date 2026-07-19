using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 物品快捷栏UI - 显示4个快捷物品槽位（1-4键）
/// 位置：屏幕右下角，水平排列
/// </summary>
public class QuickSlotUI : MonoBehaviour
{
    [Header("UI组件")]
    [Tooltip("快捷栏容器（水平布局）")]
    public RectTransform container;
    
    [Tooltip("槽位预制体")]
    public GameObject slotPrefab;
    
    [Header("样式配置")]
    [Tooltip("槽位大小")]
    public float slotSize = 64f;
    
    [Tooltip("槽位间距")]
    public float spacing = 8f;
    
    [Header("颜色配置")]
    [Tooltip("空槽位背景色")]
    public Color emptySlotColor = new Color(0.2f, 0.2f, 0.3f, 0.7f);
    
    [Tooltip("有物品背景色")]
    public Color filledSlotColor = new Color(0.3f, 0.3f, 0.4f, 0.9f);
    
    [Tooltip("选中边框色")]
    public Color selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
    
    [Tooltip("冷却遮罩色")]
    public Color cooldownMaskColor = new Color(0f, 0f, 0f, 0.6f);

    [Header("快捷键")]
    [Tooltip("快捷键列表（默认1-4）")]
    public KeyCode[] slotKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    private InventorySystem inventorySystem;
    private List<QuickSlot> slots = new List<QuickSlot>();
    private int selectedSlotIndex = 0;

    void Start()
    {
        EnsureContainerLayout();
        inventorySystem = FindObjectOfType<InventorySystem>();
        
        if (container == null)
        {
            CreateDefaultContainer();
        }
        
        InitializeSlots();
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（右下角）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20, 20);
        rt.sizeDelta = new Vector2(300, slotSize);
    }

    void Update()
    {
        HandleInput();
        UpdateCooldowns();
    }

    /// <summary>
    /// 处理快捷键输入
    /// </summary>
    void HandleInput()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                UseQuickSlot(i);
            }
        }
    }

    /// <summary>
    /// 使用快捷槽位
    /// </summary>
    public void UseQuickSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        
        var slot = slots[index];
        if (slot.item == null || slot.cooldownRemaining > 0) return;
        
        // 使用物品
        if (inventorySystem != null)
        {
            bool used = inventorySystem.UseItem(slot.item);
            if (used)
            {
                // 设置冷却
                slot.cooldownRemaining = slot.cooldown;
                Debug.Log($"[QuickSlotUI] 使用物品: {slot.item.itemName}");
            }
        }
        
        UpdateSlotDisplay();
    }

    /// <summary>
    /// 更新冷却时间
    /// </summary>
    void UpdateCooldowns()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.cooldownRemaining > 0)
            {
                slot.cooldownRemaining -= Time.deltaTime;
                if (slot.cooldownRemaining < 0) slot.cooldownRemaining = 0;
                
                // 更新冷却遮罩
                if (slot.cooldownMask != null)
                {
                    float cooldownPercent = slot.cooldownRemaining / slot.cooldown;
                    slot.cooldownMask.fillAmount = cooldownPercent;
                    slot.cooldownMask.gameObject.SetActive(cooldownPercent > 0);
                }
                
                // 更新冷却文本
                if (slot.cooldownText != null)
                {
                    if (slot.cooldownRemaining > 0)
                    {
                        slot.cooldownText.text = Mathf.CeilToInt(slot.cooldownRemaining).ToString();
                        slot.cooldownText.gameObject.SetActive(true);
                    }
                    else
                    {
                        slot.cooldownText.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 初始化槽位
    /// </summary>
    void InitializeSlots()
    {
        // 清空现有槽位
        foreach (var slot in slots)
        {
            if (slot.gameObject != null)
            {
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();
        
        // 创建4个槽位
        for (int i = 0; i < 4; i++)
        {
            var slot = CreateSlot(i);
            slots.Add(slot);
        }
        
        UpdateSlotDisplay();
    }

    /// <summary>
    /// 创建单个槽位
    /// </summary>
    QuickSlot CreateSlot(int index)
    {
        GameObject slotObj;
        
        if (slotPrefab != null)
        {
            slotObj = Instantiate(slotPrefab, container);
        }
        else
        {
            slotObj = CreateDefaultSlot();
        }
        
        slotObj.name = $"QuickSlot_{index}";
        
        // 设置布局
        RectTransform rect = slotObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(slotSize, slotSize);
        }
        
        // 获取或创建组件
        var slot = new QuickSlot();
        slot.gameObject = slotObj;
        slot.icon = slotObj.transform.Find("Icon")?.GetComponent<Image>();
        slot.keyText = slotObj.transform.Find("KeyText")?.GetComponent<Text>();
        slot.countText = slotObj.transform.Find("CountText")?.GetComponent<Text>();
        slot.cooldownMask = slotObj.transform.Find("CooldownMask")?.GetComponent<Image>();
        slot.cooldownText = slotObj.transform.Find("CooldownText")?.GetComponent<Text>();
        slot.border = slotObj.transform.Find("Border")?.GetComponent<Image>();
        
        // 设置快捷键文本
        if (slot.keyText != null)
        {
            slot.keyText.text = (index + 1).ToString();
        }
        
        // 添加点击事件
        var button = slotObj.GetComponent<Button>();
        if (button != null)
        {
            int slotIndex = index;
            button.onClick.AddListener(() => UseQuickSlot(slotIndex));
        }
        
        return slot;
    }

    /// <summary>
    /// 创建默认槽位
    /// </summary>
    GameObject CreateDefaultSlot()
    {
        GameObject slotObj = new GameObject("QuickSlot");
        slotObj.transform.SetParent(container, false);
        
        // 背景
        Image bgImage = slotObj.AddComponent<Image>();
        bgImage.color = emptySlotColor;
        
        // 图标
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        // 快捷键文本（左上角）
        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(slotObj.transform, false);
        Text keyText = keyTextObj.AddComponent<Text>();
        keyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        keyText.fontSize = 12;
        keyText.alignment = TextAnchor.UpperLeft;
        keyText.color = Color.white;
        RectTransform keyTextRect = keyTextObj.GetComponent<RectTransform>();
        keyTextRect.anchorMin = new Vector2(0, 0.7f);
        keyTextRect.anchorMax = new Vector2(0.3f, 1f);
        keyTextRect.offsetMin = new Vector2(2, 0);
        keyTextRect.offsetMax = new Vector2(0, -2);
        
        // 数量文本（右下角）
        GameObject countTextObj = new GameObject("CountText");
        countTextObj.transform.SetParent(slotObj.transform, false);
        Text countText = countTextObj.AddComponent<Text>();
        countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        countText.fontSize = 14;
        countText.alignment = TextAnchor.LowerRight;
        countText.color = Color.white;
        RectTransform countTextRect = countTextObj.GetComponent<RectTransform>();
        countTextRect.anchorMin = new Vector2(0.7f, 0f);
        countTextRect.anchorMax = new Vector2(1f, 0.3f);
        countTextRect.offsetMin = new Vector2(0, 2);
        countTextRect.offsetMax = new Vector2(-2, 0);
        
        // 冷却遮罩（填充类型）
        GameObject cooldownMaskObj = new GameObject("CooldownMask");
        cooldownMaskObj.transform.SetParent(slotObj.transform, false);
        Image cooldownMask = cooldownMaskObj.AddComponent<Image>();
        cooldownMask.color = cooldownMaskColor;
        cooldownMask.type = Image.Type.Filled;
        cooldownMask.fillMethod = Image.FillMethod.Radial360;
        cooldownMask.fillAmount = 0;
        RectTransform cooldownMaskRect = cooldownMaskObj.GetComponent<RectTransform>();
        cooldownMaskRect.anchorMin = Vector2.zero;
        cooldownMaskRect.anchorMax = Vector2.one;
        cooldownMaskRect.offsetMin = Vector2.zero;
        cooldownMaskRect.offsetMax = Vector2.zero;
        
        // 冷却文本
        GameObject cooldownTextObj = new GameObject("CooldownText");
        cooldownTextObj.transform.SetParent(slotObj.transform, false);
        Text cooldownText = cooldownTextObj.AddComponent<Text>();
        cooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cooldownText.fontSize = 20;
        cooldownText.alignment = TextAnchor.MiddleCenter;
        cooldownText.color = Color.white;
        cooldownText.gameObject.SetActive(false);
        RectTransform cooldownTextRect = cooldownTextObj.GetComponent<RectTransform>();
        cooldownTextRect.anchorMin = Vector2.zero;
        cooldownTextRect.anchorMax = Vector2.one;
        cooldownTextRect.offsetMin = Vector2.zero;
        cooldownTextRect.offsetMax = Vector2.zero;
        
        // 边框
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(slotObj.transform, false);
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = Color.clear;
        borderImage.type = Image.Type.Sliced;
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        
        // 添加按钮组件
        Button button = slotObj.AddComponent<Button>();
        button.targetGraphic = bgImage;
        
        return slotObj;
    }

    /// <summary>
    /// 更新槽位显示
    /// </summary>
    public void UpdateSlotDisplay()
    {
        if (inventorySystem == null) return;
        
        var allItems = inventorySystem.GetItems();
        var consumableItems = allItems.FindAll(item => item.itemType == ItemType.Consumable);
        
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var item = (i < consumableItems.Count) ? consumableItems[i] : null;
            slot.item = item;
            
            // 更新图标
            if (slot.icon != null)
            {
                if (item != null && item.icon != null)
                {
                    slot.icon.sprite = item.icon;
                    slot.icon.color = Color.white;
                }
                else
                {
                    slot.icon.sprite = null;
                    slot.icon.color = Color.clear;
                }
            }
            
            // 更新数量
            if (slot.countText != null)
            {
                if (item != null && item.quantity > 1)
                {
                    slot.countText.text = item.quantity.ToString();
                    slot.countText.gameObject.SetActive(true);
                }
                else
                {
                    slot.countText.gameObject.SetActive(false);
                }
            }
            
            // 更新背景色
            var bgImage = slot.gameObject.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = (item != null) ? filledSlotColor : emptySlotColor;
            }
            
            // 更新边框（选中状态）
            if (slot.border != null)
            {
                slot.border.color = (i == selectedSlotIndex) ? selectedColor : Color.clear;
            }

            // 广播槽位变化给 HUD 事件总线（保持事件驱动一致性）
            if (GlobalEventManager.Instance != null)
            {
                string itemName = item != null ? item.itemName : "";
                int qty = item != null ? item.quantity : -1;
                Sprite iconSprite = (item != null && item.icon != null) ? item.icon : null;
                GlobalEventManager.Instance.TriggerQuickSlotChanged(i, itemName, qty, iconSprite);
            }
        }
    }

    /// <summary>
    /// 设置选中槽位
    /// </summary>
    public void SetSelectedSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        selectedSlotIndex = index;
        UpdateSlotDisplay();
    }

    /// <summary>
    /// 创建默认容器
    /// </summary>
    void CreateDefaultContainer()
    {
        GameObject containerObj = new GameObject("QuickSlotContainer");
        containerObj.transform.SetParent(transform, false);
        
        container = containerObj.AddComponent<RectTransform>();
        container.anchorMin = new Vector2(1, 0);
        container.anchorMax = new Vector2(1, 0);
        container.pivot = new Vector2(1, 0);
        container.anchoredPosition = new Vector2(-20, 20);
        container.sizeDelta = new Vector2(300, slotSize);
        
        HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 0, 0);
    }
}

/// <summary>
/// 快捷槽数据
/// </summary>
[System.Serializable]
public class QuickSlot
{
    public GameObject gameObject;
    public InventoryItem item;
    public Image icon;
    public Text keyText;
    public Text countText;
    public Image cooldownMask;
    public Text cooldownText;
    public Image border;
    public float cooldown = 1f;
    public float cooldownRemaining = 0f;
}
