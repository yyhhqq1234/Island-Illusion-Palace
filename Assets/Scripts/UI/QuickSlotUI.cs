using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IIPUI;

/// <summary>
/// 物品快捷栏UI - 显示3个快捷物品槽位（1-3键）
/// 位置：屏幕底部中央偏左，距下边距 40px
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
    public float slotSize = IIPUIStyle.SlotSizeStandard;

    [Tooltip("槽位间距")]
    public float spacing = IIPUIStyle.SpacingStandard;

    [Header("颜色配置")]
    [Tooltip("空槽位背景色")]
    public Color emptySlotColor = IIPUIStyle.QuickSlotEmpty;

    [Tooltip("有物品背景色")]
    public Color filledSlotColor = IIPUIStyle.QuickSlotFilled;

    [Tooltip("选中边框色（白色高亮）")]
    public Color selectedColor = Color.white;

    [Tooltip("冷却遮罩色")]
    public Color cooldownMaskColor = IIPUIStyle.CooldownMask;

    [Header("快捷键")]
    [Tooltip("快捷键列表（默认1-3）")]
    public KeyCode[] slotKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

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
    /// 确保容器RectTransform布局正确（底部中央偏左，距下边距 40px）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 40);
        rt.sizeDelta = new Vector2(3 * slotSize + 2 * spacing, slotSize);
    }

    void Update()
    {
        HandleInput();
        HandleScrollWheel();
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
                // 数字键直接使用对应槽位
                UseQuickSlot(i);
            }
        }
    }

    /// <summary>
    /// 鼠标滚轮切换选中槽位（不使用，仅切换高亮）
    /// </summary>
    void HandleScrollWheel()
    {
        if (slots.Count == 0) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        if (scroll > 0)
        {
            selectedSlotIndex = (selectedSlotIndex - 1 + slots.Count) % slots.Count;
        }
        else
        {
            selectedSlotIndex = (selectedSlotIndex + 1) % slots.Count;
        }
        UpdateSlotDisplay();
    }

    /// <summary>
    /// 使用快捷槽位
    /// </summary>
    public void UseQuickSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        
        // 数字键直接使用对应槽位，同时更新选中
        selectedSlotIndex = index;

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
                    float cooldownPercent = slot.cooldown > 0 ? slot.cooldownRemaining / slot.cooldown : 0;
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
        
        // 创建3个槽位（规范 1.2.7：3 格 × 56×56px）
        for (int i = 0; i < 3; i++)
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
    /// 创建默认槽位（圆角底 + 边框 + 键位标签 + 图标占位 + 数量 + 冷却遮罩）
    /// 注意：MakeSlot(border:false) 不创建内部 Border 节点，此处显式创建一个名为 "Border" 的子 Image
    /// 节点（圆角+边框色），赋给 slot.border 引用，确保 UpdateSlotDisplay 选中高亮生效。
    /// </summary>
    GameObject CreateDefaultSlot()
    {
        // 圆角底（用工厂统一外观，边框由下方 Border 节点单独控制以兼容 slot.border 引用）
        GameObject slotObj = IIPUIFactory.MakeSlot("QuickSlot", container,
            new Vector2(slotSize, slotSize), emptySlotColor,
            keyLabel: null, border: false);
        var bgImage = slotObj.GetComponent<Image>();

        // 图标占位（铺满内框，48×48 居中效果由锚点 0.12~0.88 实现）
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(slotObj.transform, false);
        Image iconImage = iconObj.GetComponent<Image>();
        iconImage.color = Color.clear;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.12f, 0.12f);
        iconRect.anchorMax = new Vector2(0.88f, 0.88f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        // 快捷键文本（左上角，雅黑）—— "1/2/3"
        IIPUIFactory.CreateLabelAnchored("KeyText", slotObj.transform,
            "", IIPUIStyle.FontSizeBody, IIPUIFactory.TextKey,
            new Vector2(0, 0.7f), new Vector2(0.4f, 1f),
            new Vector2(3, 0), new Vector2(0, -2), TextAnchor.UpperLeft);

        // 空槽提示（居中淡字，运行时由 UpdateSlotDisplay 隐藏）
        IIPUIFactory.CreateLabel("EmptyHint", slotObj.transform, "—", IIPUIStyle.FontSizeButton, IIPUIFactory.TextDim);

        // 数量角标（右下角，16pt）
        IIPUIFactory.CreateLabelAnchored("CountText", slotObj.transform,
            "", IIPUIStyle.FontSizeLabel, IIPUIFactory.TextMain,
            new Vector2(0.6f, 0f), new Vector2(1f, 0.35f),
            new Vector2(0, 2), new Vector2(-3, 0), TextAnchor.LowerRight);

        // 冷却遮罩（填充类型，从上到下扇形）
        GameObject cooldownMaskObj = new GameObject("CooldownMask", typeof(RectTransform), typeof(Image));
        cooldownMaskObj.transform.SetParent(slotObj.transform, false);
        Image cooldownMask = cooldownMaskObj.GetComponent<Image>();
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
        var cooldownText = IIPUIFactory.CreateLabel("CooldownText", slotObj.transform,
            "", IIPUIStyle.FontSizeButton, IIPUIFactory.TextMain);
        cooldownText.gameObject.SetActive(false);

        // 选中边框（默认透明，运行时由 UpdateSlotDisplay 着色为白色高亮）
        // 显式创建名为 "Border" 的子 Image 节点，圆角 9-slice，铺满父节点
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(slotObj.transform, false);
        Image borderImage = borderObj.GetComponent<Image>();
        borderImage.color = Color.clear;
        IIPUIFactory.ApplyRounded(borderImage, true);
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        // 添加按钮组件 + 悬停反馈
        Button button = slotObj.AddComponent<Button>();
        button.targetGraphic = bgImage;
        IIPUIFactory.ApplyHover(slotObj, IIPUIFactory.ButtonHover);

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
            bool itemChanged = slot.item != item;
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

            // 空槽提示文字显隐
            var emptyHint = slot.gameObject.transform.Find("EmptyHint")?.GetComponent<Text>();
            if (emptyHint != null)
            {
                emptyHint.gameObject.SetActive(item == null);
            }

            // 更新边框（选中状态：白色高亮边框）
            if (slot.border != null)
            {
                slot.border.color = (i == selectedSlotIndex) ? selectedColor : Color.clear;
            }

            // 仅在槽位内容变化时广播事件（避免滚轮切换选中时重复广播 3 次）
            if (itemChanged && GlobalEventManager.Instance != null)
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
    /// 创建默认容器（底部中央）
    /// </summary>
    void CreateDefaultContainer()
    {
        GameObject containerObj = new GameObject("QuickSlotContainer");
        containerObj.transform.SetParent(transform, false);
        
        container = containerObj.AddComponent<RectTransform>();
        container.anchorMin = new Vector2(0.5f, 0);
        container.anchorMax = new Vector2(0.5f, 0);
        container.pivot = new Vector2(0.5f, 0);
        container.anchoredPosition = new Vector2(0, 40);
        container.sizeDelta = new Vector2(3 * slotSize + 2 * spacing, slotSize);
        
        HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.childAlignment = TextAnchor.MiddleCenter;
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
