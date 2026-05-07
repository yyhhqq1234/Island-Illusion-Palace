using UnityEngine;
using UnityEngine.UI;
using GameSystems;
using System.Collections.Generic;

public class ItemSlot : MonoBehaviour
{
    [Header("UI组件引用")]
    [Tooltip("背景图像 - 自动查找子物体或自身")]
    public Image backgroundImage;        // 背景图像
    
    [Tooltip("物品图标 - 从Icon子物体自动获取，支持7倍缩放显示")]
    public Image iconImage;              // 物品图标
    
    [Tooltip("数量文本 - 从Quantity子物体自动获取")]
    public Text quantityText;            // 数量文本
    
    [Header("当前物品数据")]
    [Tooltip("当前显示的物品信息（运行时自动设置）")]
    private InventoryItem currentItem = null;
    
    [Header("图标缩放设置")]
    [Tooltip("图标的放大倍数，默认4倍以清晰显示")]
    public float iconScaleFactor = 4f;   // 图标放大倍数
    
    [Header("物品图标数据库（按分类）")]
    [Tooltip("包含所有物品名称和对应Sprite图标的数据库\n材料(24) + 消耗品(14) + 武器(15) = 总计53项")]
    public ItemIconDatabase iconDatabase;
    
    public static List<string> GetAllMaterialNames()
    {
        return new List<string>
        {
            // 基础材料（10种）
            "腐朽木屑", "碎骨", "星光草", "腐化组织", "月影花",
            "锈蚀零件", "灵魂微尘", "浑浊露珠", "净化盐晶", "哀嚎藤蔓",
            // 稀有材料（10种）
            "记忆残渣", "时空碎片", "灵魂结晶", "晶化残核", "机械核心",
            "古树树脂", "熔岩核心", "石像鬼碎片", "极寒冰屑", "古龙骨粉",
            // 史诗材料（3种）
            "灵魂精华", "地脉结晶", "远古铭文石",
            // 传奇材料（1种）
            "悖时薄片"
        };
    }
    
    public static List<string> GetAllConsumableNames()
    {
        return new List<string>
        {
            // 基础配方（4个）
            "治疗药剂", "灵魂稳定剂", "召唤持续时间药剂", "负担缓解熏香",
            // 进阶配方（7个）
            "召唤强化剂", "弱点洞察药剂", "记忆共鸣药剂", "机械仆从核心",
            "环境稳定卷轴", "同频强化药剂", "重生护符",
            // 传奇配方（3个）
            "时空道标", "真理窥视药剂", "负担净化圣水"
        };
    }
    
    public static List<string> GetAllWeaponNames()
    {
        return new List<string>
        {
            // 基础武器（5把）
            "破晓之刃", "结晶法杖", "沼泽镰刀", "晶能短刃", "灵魂引导者",
            // 进阶武器（5把）
            "熔火重剑", "冰霜法杖", "雷霆镰刃", "圣光裁决", "腐化吞噬者",
            // 特殊武器（5把）
            "时空裂隙匕首", "绯红记忆之刃", "岩地重锤", "灵魂共鸣法杖", "绯红契约"
        };
    }
    
    [System.Serializable]
    public class ItemIconDatabase
    {
        public CategoryIcons materials;
        public CategoryIcons consumables;
        public CategoryIcons weapons;
        
        public ItemIconDatabase()
        {
            materials = new CategoryIcons("材料");
            consumables = new CategoryIcons("消耗品");
            weapons = new CategoryIcons("武器");
        }
        
        public List<ItemIconData> GetAllIcons()
        {
            List<ItemIconData> allIcons = new List<ItemIconData>();
            if (materials != null) allIcons.AddRange(materials.icons);
            if (consumables != null) allIcons.AddRange(consumables.icons);
            if (weapons != null) allIcons.AddRange(weapons.icons);
            return allIcons;
        }
    }
    
    [System.Serializable]
    public class CategoryIcons
    {
        public string categoryName;
        public List<ItemIconData> icons = new List<ItemIconData>();
        
        public CategoryIcons(string name)
        {
            categoryName = name;
        }
        
        public List<string> GetAvailableNames()
        {
            switch (categoryName)
            {
                case "材料":
                    return GetAllMaterialNames();
                case "消耗品":
                    return GetAllConsumableNames();
                case "武器":
                    return GetAllWeaponNames();
                default:
                    return new List<string>();
            }
        }
    }
    
    [System.Serializable]
    public class ItemIconData
    {
        [Tooltip("物品名称")]
        public string itemName;
        [Tooltip("拖拽对应的Sprite图标到此处")]
        public Sprite icon;
        [Tooltip("物品的稀有度，用于自动设置背景颜色")]
        public ItemRarity rarity = ItemRarity.Common;
        
        public ItemIconData(string name, Sprite sprite = null, ItemRarity rarity = ItemRarity.Common)
        {
            itemName = name;
            icon = sprite;
            this.rarity = rarity;
        }
    }
    
    public static class RaritySettings
    {
        public static Color GetBackgroundColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return new Color(0.95f, 0.95f, 0.95f, 0.4f);      // 浅灰 - 40%透明
                case ItemRarity.Uncommon:
                    return new Color(0.6f, 0.75f, 1.0f, 0.5f);       // 浅蓝 - 50%透明
                case ItemRarity.Rare:
                    return new Color(0.8f, 0.6f, 1.0f, 0.6f);        // 浅紫 - 60%透明
                case ItemRarity.Legendary:
                    return new Color(1.0f, 0.85f, 0.4f, 0.7f);       // 金色 - 70%透明
                default:
                    return new Color(0.9f, 0.9f, 0.9f, 0.3f);
            }
        }
        
        public static Color GetBorderColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f, 0.6f);
                case ItemRarity.Uncommon:
                    return new Color(0.3f, 0.5f, 1.0f, 0.8f);
                case ItemRarity.Rare:
                    return new Color(0.6f, 0.3f, 1.0f, 0.9f);
                case ItemRarity.Legendary:
                    return new Color(1.0f, 0.75f, 0.2f, 1.0f);
                default:
                    return new Color(0.6f, 0.6f, 0.6f, 0.5f);
            }
        }
        
        public static float GetGlowIntensity(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return 0f;
                case ItemRarity.Uncommon: return 0.15f;
                case ItemRarity.Rare: return 0.25f;
                case ItemRarity.Legendary: return 0.35f;
                default: return 0f;
            }
        }
    }
    
    void Start()
    {
        InitializeComponents();
        InitializeDefaultDatabase();
    }
    
    void InitializeComponents()
    {
        // 1. 初始化背景图像
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                Debug.Log($"[ItemSlot] 自动找到背景图像: {backgroundImage.name}");
            }
        }
        
        // 2. 初始化物品图标（从Icon子物体获取）
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    Debug.Log($"[ItemSlot] 自动找到图标组件: {iconImage.name} (子物体)");
                    
                    // 设置图标的初始状态
                    iconImage.raycastTarget = false; // 让点击事件穿透到Button/EventTrigger
                    iconImage.preserveAspect = true;  // 保持图标比例
                    
                    // 确保 Icon 父物体也不阻挡射线检测
                    Transform iconParent = iconImage.transform.parent;
                    if (iconParent != null && iconParent.name == "Icon")
                    {
                        CanvasGroup iconCanvasGroup = iconParent.GetComponent<CanvasGroup>();
                        if (iconCanvasGroup == null)
                        {
                            iconCanvasGroup = iconParent.gameObject.AddComponent<CanvasGroup>();
                        }
                        iconCanvasGroup.blocksRaycasts = false;  // Icon容器不接收射线
                        iconCanvasGroup.alpha = 1f;              // 保持完全可见
                        
                        Debug.Log($"[ItemSlot] ✅ Icon子物体配置: raycastTarget=false | 父物体blocksRaycasts=false");
                    }
                }
            }
        }
        
        // 3. 初始化数量文本（从Quantity子物体获取）
        if (quantityText == null)
        {
            Transform quantityTransform = transform.Find("Quantity");
            if (quantityTransform != null)
            {
                quantityText = quantityTransform.GetComponent<Text>();
                if (quantityText != null)
                {
                    Debug.Log($"[ItemSlot] 自动找到数量文本: {quantityText.name} (子物体)");
                    
                    // 设置数量文本的默认样式
                    quantityText.alignment = TextAnchor.LowerRight;
                    quantityText.fontSize = 16;
                    quantityText.color = Color.white;
                }
            }
        }
        
        // 4. 应用图标缩放设置
        UpdateIconScale();
        
        // 5. 输出数据库状态信息
        LogDatabaseStatus();
    }
    
    void LogDatabaseStatus()
    {
        if (iconDatabase == null)
        {
            Debug.LogWarning($"[ItemSlot] ⚠️ 物品图标数据库未配置！请检查Inspector中的Icon Database字段");
            return;
        }
        
        int materialCount = iconDatabase.materials?.icons?.Count ?? 0;
        int consumableCount = iconDatabase.consumables?.icons?.Count ?? 0;
        int weaponCount = iconDatabase.weapons?.icons?.Count ?? 0;
        int totalCount = materialCount + consumableCount + weaponCount;
        
        int configuredIcons = CountConfiguredIcons();
        
        Debug.Log($"[ItemSlot] 📊 图标数据库状态:" +
                  $"\n  - 材料类: {materialCount}/24 项" +
                  $"\n  - 消耗品类: {consumableCount}/14 项" + 
                  $"\n  - 武器类: {weaponCount}/15 项" +
                  $"\n  - 总计: {totalCount}/53 项" +
                  $"\n  - 已配置图标: {configuredIcons}/{totalCount}");
    }
    
    int CountConfiguredIcons()
    {
        int count = 0;
        if (iconDatabase.materials?.icons != null)
        {
            foreach (var item in iconDatabase.materials.icons)
            {
                if (item.icon != null) count++;
            }
        }
        if (iconDatabase.consumables?.icons != null)
        {
            foreach (var item in iconDatabase.consumables.icons)
            {
                if (item.icon != null) count++;
            }
        }
        if (iconDatabase.weapons?.icons != null)
        {
            foreach (var item in iconDatabase.weapons.icons)
            {
                if (item.icon != null) count++;
            }
        }
        return count;
    }
    
    void InitializeDefaultDatabase()
    {
        if (iconDatabase == null)
        {
            iconDatabase = new ItemIconDatabase();
        }
        
        // 初始化材料列表（带默认稀有度）
        if (iconDatabase.materials == null || iconDatabase.materials.icons.Count == 0)
        {
            iconDatabase.materials = new CategoryIcons("材料");
            
            List<string> materialNames = GetAllMaterialNames();
            for (int i = 0; i < materialNames.Count; i++)
            {
                ItemRarity rarity;
                if (i < 10)       // 基础材料（0-9）
                    rarity = ItemRarity.Common;
                else if (i < 20)   // 稀有材料（10-19）
                    rarity = ItemRarity.Uncommon;
                else if (i < 23)   // 史诗材料（20-22）
                    rarity = ItemRarity.Rare;
                else               // 传奇材料（23）
                    rarity = ItemRarity.Legendary;
                
                iconDatabase.materials.icons.Add(new ItemIconData(materialNames[i], null, rarity));
            }
        }
        
        // 初始化消耗品列表（带默认稀有度）
        if (iconDatabase.consumables == null || iconDatabase.consumables.icons.Count == 0)
        {
            iconDatabase.consumables = new CategoryIcons("消耗品");
            
            List<string> consumableNames = GetAllConsumableNames();
            for (int i = 0; i < consumableNames.Count; i++)
            {
                ItemRarity rarity;
                if (i < 4)        // 基础配方（0-3）
                    rarity = ItemRarity.Common;
                else if (i < 11)  // 进阶配方（4-10）
                    rarity = ItemRarity.Uncommon;
                else              // 传奇配方（11-13）
                    rarity = ItemRarity.Legendary;
                
                iconDatabase.consumables.icons.Add(new ItemIconData(consumableNames[i], null, rarity));
            }
        }
        
        // 初始化武器列表（带默认稀有度）
        if (iconDatabase.weapons == null || iconDatabase.weapons.icons.Count == 0)
        {
            iconDatabase.weapons = new CategoryIcons("武器");
            
            List<string> weaponNames = GetAllWeaponNames();
            for (int i = 0; i < weaponNames.Count; i++)
            {
                ItemRarity rarity;
                if (i < 5)        // 基础武器（0-4）
                    rarity = ItemRarity.Common;
                else if (i < 10)  // 进阶武器（5-9）
                    rarity = ItemRarity.Uncommon;
                else if (i < 12)  // 史诗武器（10-11）
                    rarity = ItemRarity.Rare;
                else              // 传奇武器（12-14）
                    rarity = ItemRarity.Legendary;
                
                iconDatabase.weapons.icons.Add(new ItemIconData(weaponNames[i], null, rarity));
            }
        }
    }
    
    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        
        if (item == null)
        {
            ClearSlot();
            return;
        }
        
        UpdateIconDisplay();
        UpdateQuantityDisplay();
        UpdateVisualEffect();
    }
    
    void UpdateIconDisplay()
    {
        if (iconImage == null || currentItem == null)
            return;
        
        ItemIconData foundItem = FindItemInDatabase(currentItem.itemName);
        
        if (foundItem != null && foundItem.icon != null)
            {
                // 从数据库找到对应图标 - 显示并放大
                iconImage.sprite = foundItem.icon;
                iconImage.color = Color.white;  // Icon子物体保持纯白，不叠加颜色
                iconImage.raycastTarget = false;  // 确保不阻挡父物体的射线检测
                
                // 应用4倍缩放（或用户自定义的缩放倍数）
                ScaleIcon(iconScaleFactor);
                
                iconImage.gameObject.SetActive(true);
                
                Debug.Log($"[ItemSlot] ✅ 显示物品图标: {currentItem.itemName} (稀有度: {foundItem.rarity}, 缩放: {iconScaleFactor}倍, raycastTarget=false)");
                
                // 根据数据库中的稀有度更新父物体背景视觉效果
                ApplyRarityStyle(foundItem.rarity);
            }
            else
            {
                // 未在数据库中找到图标 - Icon子物体保持空白/透明
                iconImage.sprite = null;
                iconImage.color = Color.white;  // Icon子物体始终保持白色
                iconImage.raycastTarget = false;  // 确保不阻挡射线检测
                iconImage.gameObject.SetActive(true);  // 保持激活状态以便后续更新
            
            // 父物体背景使用类型颜色作为占位提示
            ApplyItemTypeBackgroundStyle();
            
            if (foundItem != null)
                Debug.LogWarning($"[ItemSlot] ⚠️ 物品 '{currentItem.itemName}' 已在数据库中找到，但未配置Sprite图标！");
            else
                Debug.LogWarning($"[ItemSlot] ⚠️ 未找到物品 '{currentItem.itemName}' 的图标！请检查Icon Database配置");
        }
    }
    
    ItemIconData FindItemInDatabase(string itemName)
    {
        if (iconDatabase == null)
        {
            Debug.LogError($"[ItemSlot] ❌ 图标数据库为空！无法查找物品: {itemName}");
            return null;
        }
        
        // 标准化输入名称（去除首尾空格）
        string normalizedInput = itemName?.Trim();
        
        Debug.Log($"[ItemSlot] 🔍 正在查找物品: '{normalizedInput}' (原始: '{itemName}')");
        
        // 第一轮：精确匹配（忽略首尾空格）
        List<ItemIconData> allIcons = iconDatabase.GetAllIcons();
        foreach (var iconData in allIcons)
        {
            string dbName = iconData.itemName?.Trim();
            
            if (dbName == normalizedInput)
            {
                Debug.Log($"[ItemSlot] ✅ 精确匹配成功: '{normalizedInput}' | 分类: {GetCategoryName(iconData)} | 有图标: {iconData.icon != null}");
                return iconData;
            }
        }
        
        // 第二轮：模糊匹配（忽略所有空格和大小写）
        string fuzzyInput = RemoveAllSpaces(normalizedInput).ToLower();
        Debug.Log($"[ItemSlot] 🔎 精确匹配失败，尝试模糊匹配: '{fuzzyInput}'");
        
        foreach (var iconData in allIcons)
        {
            string dbName = RemoveAllSpaces(iconData.itemName?.Trim()).ToLower();
            
            if (dbName == fuzzyInput)
            {
                Debug.LogWarning($"[ItemSlot] ⚠️ 模糊匹配成功: '{normalizedInput}' ↔ 数据库: '{iconData.itemName}' | 建议: 统一命名格式");
                return iconData;
            }
        }
        
        // 输出数据库中相似名称供参考
        ListSimilarItems(normalizedInput, allIcons);
        
        Debug.LogError($"[ItemSlot] ❌ 未找到物品: '{normalizedInput}' | 数据库总条目: {allIcons.Count}");
        return null;
    }
    
    string GetCategoryName(ItemIconData itemData)
    {
        if (iconDatabase.materials?.icons.Contains(itemData) == true) return "材料";
        if (iconDatabase.consumables?.icons.Contains(itemData) == true) return "消耗品";
        if (iconDatabase.weapons?.icons.Contains(itemData) == true) return "武器";
        return "未知";
    }
    
    string RemoveAllSpaces(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace(" ", "").Replace("　", ""); // 去除半角和全角空格
    }
    
    void ListSimilarItems(string itemName, List<ItemIconData> allItems)
    {
        Debug.Log($"[ItemSlot] 📋 数据库中包含 '{itemName}' 关键字的物品:");
        
        int foundCount = 0;
        foreach (var item in allItems)
        {
            if (item.itemName?.Contains(itemName) == true || 
                itemName?.Contains(item.itemName) == true)
            {
                Debug.Log($"  → 相似项: '{item.itemName}' ({GetCategoryName(item)})");
                foundCount++;
            }
        }
        
        if (foundCount == 0)
        {
            // 显示前5个同分类的物品作为参考
            Debug.Log($"  ❌ 无相似项。数据库部分列表:");
            int count = 0;
            foreach (var item in allItems)
            {
                if (count >= 5) break;
                Debug.Log($"  - '{item.itemName}' ({GetCategoryName(item)})");
                count++;
            }
        }
    }
    
    void UpdateQuantityDisplay()
    {
        if (quantityText == null || currentItem == null)
            return;
        
        if (currentItem.quantity > 0)
        {
            quantityText.text = currentItem.quantity.ToString();
            
            // 始终激活Quantity子物体，确保文本组件正常工作
            quantityText.gameObject.SetActive(true);
            
            // 设置文本样式
            ConfigureQuantityTextStyle();
            
            Debug.Log($"[ItemSlot] 🔢 显示物品数量: {currentItem.itemName} × {currentItem.quantity}");
        }
        else
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
    }
    
    void ConfigureQuantityTextStyle()
    {
        if (quantityText == null) return;
        
        // 确保文本样式正确配置
        quantityText.alignment = TextAnchor.LowerRight;     // 右下角对齐
        quantityText.fontSize = 16;                          // 字体大小
        quantityText.color = Color.black;                    // 黑色文字
        quantityText.raycastTarget = false;                  // 不阻挡点击事件
        
        // 配置RectTransform确保正确定位在右下角
        RectTransform textRect = quantityText.GetComponent<RectTransform>();
        if (textRect != null)
        {
            // 锚点设置为右下角
            textRect.anchorMin = new Vector2(1, 0);
            textRect.anchorMax = new Vector2(1, 0);
            textRect.pivot = new Vector2(1, 0);
            
            // 偏移量：距离右边和底边各5像素
            textRect.anchoredPosition = new Vector2(-5f, 5f);
            
            // 文本区域大小自适应
            textRect.sizeDelta = new Vector2(50f, 25f);
        }
        
        // 确保父物体（Quantity）的RectTransform也正确配置
        Transform quantityParent = quantityText.transform.parent;
        if (quantityParent != null && quantityParent.name == "Quantity")
        {
            RectTransform parentRect = quantityParent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                // Quantity父物体铺满整个格子
                parentRect.anchorMin = Vector2.zero;
                parentRect.anchorMax = Vector2.one;
                parentRect.pivot = new Vector2(0.5f, 0.5f);
                parentRect.anchoredPosition = Vector2.zero;
                parentRect.sizeDelta = Vector2.zero;
                
                // 确保不阻挡点击事件
                CanvasGroup canvasGroup = quantityParent.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = quantityParent.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.blocksRaycasts = false;  // 让点击穿透到Button
                canvasGroup.alpha = 1f;               // 保持完全可见
                
                Debug.Log($"[ItemSlot] ✅ Quantity子物体配置完成: 锚点=右下角 | 字号={quantityText.fontSize} | 位置=(-5, 5)");
            }
        }
    }
    
    void UpdateVisualEffect()
    {
        if (backgroundImage == null || currentItem == null)
            return;
        
        // 优先使用数据库中的稀有度信息
        ItemIconData itemData = FindItemInDatabase(currentItem.itemName);
        if (itemData != null)
        {
            ApplyRarityStyle(itemData.rarity);
            return;
        }
        
        // 如果数据库中没有该物品，根据物品类型设置默认样式
        switch (currentItem.itemType)
        {
            case ItemType.Weapon:
                backgroundImage.color = new Color(0.7f, 0.8f, 1f, 0.5f);
                break;
            case ItemType.Consumable:
                backgroundImage.color = new Color(0.7f, 1f, 0.7f, 0.5f);
                break;
            case ItemType.Material:
                backgroundImage.color = new Color(1f, 1f, 0.7f, 0.5f);
                break;
            default:
                backgroundImage.color = new Color(0.9f, 0.9f, 0.9f, 0.5f);
                break;
        }
        
        if (currentItem.isEquipped)
        {
            backgroundImage.color = new Color(0.6f, 0.9f, 0.6f, 0.8f);
        }
    }
    
    void ApplyRarityStyle(ItemRarity rarity)
    {
        if (backgroundImage == null) return;
        
        // 根据稀有度设置父物体背景颜色和透明度
        Color bgColor = RaritySettings.GetBackgroundColor(rarity);
        backgroundImage.color = bgColor;
        
        // 如果物品已装备，叠加绿色高亮到父物体背景
        if (currentItem != null && currentItem.isEquipped)
        {
            Color equippedColor = Color.Lerp(bgColor, new Color(0.6f, 0.9f, 0.6f, 1f), 0.5f);
            backgroundImage.color = equippedColor;
        }
        
        Debug.Log($"[ItemSlot] 🎨 父物体背景应用稀有度样式: {rarity} | 背景色: {bgColor} | 透明度: {bgColor.a:P0}");
    }
    
    void ApplyItemTypeBackgroundStyle()
    {
        if (backgroundImage == null || currentItem == null) return;
        
        // 根据物品类型设置父物体背景颜色（Icon子物体保持白色不变）
        switch (currentItem.itemType)
        {
            case ItemType.Weapon:
                backgroundImage.color = new Color(0.7f, 0.8f, 1f, 0.5f);   // 浅蓝 - 武器
                break;
            case ItemType.Consumable:
                backgroundImage.color = new Color(0.7f, 1f, 0.7f, 0.5f);   // 浅绿 - 消耗品
                break;
            case ItemType.Material:
                backgroundImage.color = new Color(1f, 1f, 0.7f, 0.5f);    // 浅黄 - 材料
                break;
            case ItemType.MemoryFragment:
                backgroundImage.color = new Color(1f, 0.7f, 0.9f, 0.5f);   // 浅粉 - 记忆碎片
                break;
            default:
                backgroundImage.color = new Color(0.9f, 0.9f, 0.9f, 0.5f);  // 浅灰 - 默认
                break;
        }
        
        // 如果已装备，叠加绿色高亮到父物体背景
        if (currentItem.isEquipped)
        {
            Color currentBg = backgroundImage.color;
            Color equippedColor = Color.Lerp(currentBg, new Color(0.6f, 0.9f, 0.6f, 0.8f), 0.5f);
            backgroundImage.color = equippedColor;
        }
        
        Debug.Log($"[ItemSlot] 🎨 父物体背景应用物品类型样式: {currentItem.itemType} | 背景色: {backgroundImage.color}");
    }
    
    public void ScaleIcon(float scaleFactor)
    {
        if (iconImage == null)
            return;
        
        iconImage.transform.localScale = Vector3.one * scaleFactor;
        
        RectTransform iconRect = iconImage.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            // 设置锚点为居中
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2((float)0.5, (float)0.5);
            
            // 设置图标默认尺寸为100x100像素
            iconRect.sizeDelta = new Vector2(100f, 100f);
            
            Debug.Log($"[ItemSlot] 📐 图标缩放: {scaleFactor}倍 | 尺寸: 100×100px | 实际显示: {100*scaleFactor}×{100*scaleFactor}px");
        }
    }
    
    void UpdateIconScale()
    {
        if (iconImage != null && iconScaleFactor != 1f)
        {
            ScaleIcon(iconScaleFactor);
        }
    }
    
    Color GetItemTypeColor(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon:
                return new Color(0.4f, 0.6f, 1f, 0.8f);
            case ItemType.Consumable:
                return new Color(0.4f, 1f, 0.6f, 0.8f);
            case ItemType.Material:
                return new Color(1f, 1f, 0.4f, 0.8f);
            case ItemType.MemoryFragment:
                return new Color(1f, 0.4f, 0.6f, 0.8f);
            default:
                return new Color(0.8f, 0.8f, 0.8f, 0.8f);
        }
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
        
        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        }
    }
    
    public InventoryItem GetItem()
    {
        return currentItem;
    }
    
    public bool HasItem()
    {
        return currentItem != null;
    }
}
