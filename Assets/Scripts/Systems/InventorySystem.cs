using UnityEngine;
using System.Collections.Generic;
using GameSystems;

public enum ItemType
{
    Material,        // 材料
    Consumable,      // 消耗品（包括药剂）
    Weapon,          // 武器
    MemoryFragment   // 记忆碎片
}

public enum InventoryCategory
{
    Materials,        // 材料栏目
    Consumables,      // 消耗品栏目
    Weapons,          // 武器栏目
    MemoryFragments   // 记忆碎片和特殊物品栏目
}

public enum ItemRarity
{
    Common,     // 普通（白色）
    Uncommon,   // 精良（蓝色）
    Rare,       // 史诗（紫色）
    Legendary   // 传奇（金色）
}

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public ItemType itemType;
    public int quantity;
    public string description;
    public int value;
    public Sprite icon;
    public bool isEquipped;
    public ItemRarity rarity;
    
    // 材料特有属性
    public MaterialTypeEnum materialType;
    
    // 药剂特有属性
    public RecipeType potionType;
    
    // 武器特有属性
    public WeaponSystem.WeaponType weaponType;
    public int weaponLevel;
    
    public InventoryItem(string name, ItemType type, int qty, string desc = "", int val = 0, ItemRarity r = ItemRarity.Common)
    {
        itemName = name;
        itemType = type;
        quantity = qty;
        description = desc;
        value = val;
        icon = null;
        isEquipped = false;
        rarity = r;
    }
    
    public bool CanStack
    {
        get { return itemType == ItemType.Material || itemType == ItemType.Consumable; }
    }
    
    public string GetDescription()
    {
        return description;
    }
}

public class InventorySystem : MonoBehaviour
{
    [Header("背包设置")]
    public int maxMaterialSlots = 24;      // 材料空间上限
    public int maxConsumableSlots = 24;    // 消耗品空间上限
    public int maxWeaponSlots = 24;        // 武器空间上限
    public int maxMemoryFragmentSlots = 24; // 记忆碎片和特殊物品空间上限（4×6=24格）
    public int currentInventorySize = 0;
    
    [Header("物品列表")]
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> materialItems = new List<InventoryItem>();
    public List<InventoryItem> consumableItems = new List<InventoryItem>();
    public List<InventoryItem> weaponItems = new List<InventoryItem>();
    public List<InventoryItem> memoryFragmentItems = new List<InventoryItem>();
    
    [Header("背包格子")]
    // 材料栏目：4层 × 6格子 = 24格子
    public InventoryItem[,] materialSlots = new InventoryItem[4, 6];
    // 消耗品栏目：4层 × 6格子 = 24格子
    public InventoryItem[,] consumableSlots = new InventoryItem[4, 6];
    // 武器栏目：4层 × 6格子 = 24格子
    public InventoryItem[,] weaponSlots = new InventoryItem[4, 6];
    // 记忆碎片和特殊物品栏目：4层 × 6格子 = 24格子
    public InventoryItem[,] memoryFragmentSlots = new InventoryItem[4, 6];
    
    [Header("系统引用")]
    private AlchemySystem alchemySystem;
    private WeaponSystem weaponSystem;
    
    void Start()
    {
        InitializeInventory();
        InitializeSystemReferences();
    }
    
    void InitializeInventory()
    {
        // 背包初始化为空状态，不添加任何初始物品
        // 玩家需要在游戏中通过拾取、炼金或购买等方式获取物品
    }
    
    void InitializeSystemReferences()
    {
        alchemySystem = FindObjectOfType<AlchemySystem>();
        weaponSystem = FindObjectOfType<WeaponSystem>();
    }
    
    // 添加材料到背包
    public bool AddMaterial(MaterialTypeEnum materialType, int quantity)
    {
        if (quantity <= 0) return false;
        
        // 查找是否已有该材料
        InventoryItem existingItem = materialItems.Find(item => 
            item.itemType == ItemType.Material && 
            item.materialType == materialType);
        
        if (existingItem != null)
        {
            // 已有该材料，增加数量
            existingItem.quantity += quantity;
            Debug.Log($"添加{quantity}个{GetMaterialName(materialType)}到背包");
            return true;
        }
        else
        {
            // 检查材料空间上限
            if (materialItems.Count >= maxMaterialSlots)
            {
                Debug.Log("材料空间已满，无法添加物品");
                return false;
            }
            
            // 没有该材料，创建新物品
            MaterialData materialData = GetMaterialData(materialType);
            
            // 根据材料稀有度设置ItemRarity
            ItemRarity rarity = ItemRarity.Common;
            switch (materialData.rarity)
            {
                case "常见":
                    rarity = ItemRarity.Common;
                    break;
                case "稀有":
                    rarity = ItemRarity.Uncommon;
                    break;
                case "史诗":
                    rarity = ItemRarity.Rare;
                    break;
                case "传奇":
                    rarity = ItemRarity.Legendary;
                    break;
            }
            
            InventoryItem newItem = new InventoryItem(
                materialData.name,
                ItemType.Material,
                quantity,
                materialData.description,
                materialData.value,
                rarity
            );
            newItem.materialType = materialType;
            
            // 尝试将材料添加到材料栏目格子
            if (AddToMaterialSlot(newItem))
            {
                inventory.Add(newItem);
                materialItems.Add(newItem);
                currentInventorySize++;
                Debug.Log($"添加{quantity}个{GetMaterialName(materialType)}到背包");
                return true;
            }
            else
            {
                Debug.Log("材料栏目已满，无法添加物品");
                return false;
            }
        }
    }
    
    // 添加药剂到背包
    public bool AddPotion(RecipeType potionType, int quantity)
    {
        if (quantity <= 0) return false;
        
        // 查找是否已有该药剂
        InventoryItem existingItem = consumableItems.Find(item => 
            item.itemType == ItemType.Consumable && 
            item.potionType == potionType);
        
        string potionName = GetPotionName(potionType);
        if (existingItem != null)
        {
            // 已有该药剂，增加数量
            existingItem.quantity += quantity;
            Debug.Log($"添加{quantity}个{potionName}到背包");
            return true;
        }
        else
        {
            // 检查消耗品空间上限
            if (consumableItems.Count >= maxConsumableSlots)
            {
                Debug.Log("消耗品空间已满，无法添加物品");
                return false;
            }
            
            // 没有该药剂，创建新物品
            string potionDesc = GetPotionDescription(potionType);
            
            // 获取药剂的炼金价值
            int potionValue = 10; // 默认价值
            AlchemySystem alchemySystem = FindObjectOfType<AlchemySystem>();
            if (alchemySystem != null)
            {
                foreach (var recipe in alchemySystem.availableRecipes)
                {
                    if (recipe.type == potionType)
                    {
                        potionValue = recipe.alchemicalValue;
                        break;
                    }
                }
            }
            
            InventoryItem newItem = new InventoryItem(
                potionName,
                ItemType.Consumable,
                quantity,
                potionDesc,
                potionValue, // 使用炼金价值
                ItemRarity.Common // 药剂默认稀有度
            );
            newItem.potionType = potionType;
            
            // 尝试将药剂添加到消耗品栏目格子
            if (AddToConsumableSlot(newItem))
            {
                inventory.Add(newItem);
                consumableItems.Add(newItem);
                currentInventorySize++;
                Debug.Log($"添加{quantity}个{potionName}到背包");
                return true;
            }
            else
            {
                Debug.Log("消耗品栏目已满，无法添加物品");
                return false;
            }
        }
    }
    
    // 添加武器到背包
    public bool AddWeapon(WeaponSystem.WeaponType weaponType, int level = 1)
    {
        // 检查武器空间上限
        if (weaponItems.Count >= maxWeaponSlots)
        {
            Debug.Log("武器空间已满，无法添加物品");
            return false;
        }
        
        // 武器通常是唯一的，直接添加
        string weaponName = GetWeaponName(weaponType);
        
        // 根据武器等级设置稀有度
        ItemRarity rarity = ItemRarity.Common;
        if (level >= 10)
            rarity = ItemRarity.Legendary;
        else if (level >= 7)
            rarity = ItemRarity.Rare;
        else if (level >= 4)
            rarity = ItemRarity.Uncommon;
        
        InventoryItem newItem = new InventoryItem(
            weaponName,
            ItemType.Weapon,
            1, // 武器数量始终为1
            $"一把{weaponName}，等级{level}",
            50 + level * 10, // 武器价值
            rarity // 武器稀有度
        );
        newItem.weaponType = weaponType;
        newItem.weaponLevel = level;
        
        // 尝试将武器添加到武器栏目格子
        if (AddToWeaponSlot(newItem))
        {
            inventory.Add(newItem);
            weaponItems.Add(newItem);
            currentInventorySize++;
            Debug.Log($"添加{weaponName}到背包");
            return true;
        }
        else
        {
            Debug.Log("武器栏目已满，无法添加物品");
            return false;
        }
    }
    
    // 添加记忆碎片到背包
    public bool AddMemoryFragment(string fragmentName, int quantity = 1)
    {
        if (quantity <= 0) return false;
        
        // 查找是否已有该记忆碎片
        InventoryItem existingItem = memoryFragmentItems.Find(item => 
            item.itemType == ItemType.MemoryFragment && 
            item.itemName == fragmentName);
        
        if (existingItem != null)
        {
            // 已有该记忆碎片，增加数量
            existingItem.quantity += quantity;
            Debug.Log($"添加{quantity}个{fragmentName}到背包");
            return true;
        }
        else
        {
            // 检查记忆碎片空间上限
            if (memoryFragmentItems.Count >= maxMemoryFragmentSlots)
            {
                Debug.Log("记忆碎片空间已满，无法添加物品");
                return false;
            }
            
            // 没有该记忆碎片，创建新物品
            InventoryItem newItem = new InventoryItem(
                fragmentName,
                ItemType.MemoryFragment,
                quantity,
                "一段神秘的记忆碎片",
                100, // 记忆碎片默认价值
                ItemRarity.Uncommon // 记忆碎片默认稀有度
            );
            
            // 尝试将记忆碎片添加到记忆碎片栏目格子
            if (AddToMemoryFragmentSlot(newItem))
            {
                inventory.Add(newItem);
                memoryFragmentItems.Add(newItem);
                currentInventorySize++;
                Debug.Log($"添加{quantity}个{fragmentName}到背包");
                return true;
            }
            else
            {
                Debug.Log("记忆碎片栏目已满，无法添加物品");
                return false;
            }
        }
    }
    
    // 从背包移除物品
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        if (item == null)
        {
            Debug.Log($"背包中没有{itemName}");
            return false;
        }
        
        if (item.quantity < quantity)
        {
            Debug.Log($"{itemName}数量不足");
            return false;
        }
        
        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            RemoveFromSlot(item);
            inventory.Remove(item);
            
            // 从对应的类型列表中移除
            switch (item.itemType)
            {
                case ItemType.Material:
                    materialItems.Remove(item);
                    break;
                case ItemType.Consumable:
                    consumableItems.Remove(item);
                    break;
                case ItemType.Weapon:
                    weaponItems.Remove(item);
                    break;
                case ItemType.MemoryFragment:
                    memoryFragmentItems.Remove(item);
                    break;
            }
            
            currentInventorySize--;
        }
        
        Debug.Log($"从背包移除{quantity}个{itemName}");
        return true;
    }
    
    // 从背包移除物品（接受InventoryItem参数）
    public bool RemoveItem(InventoryItem item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.Log("物品为空");
            return false;
        }
        
        return RemoveItem(item.itemName, quantity);
    }
    
    // 从背包移除材料
    public bool RemoveMaterial(MaterialTypeEnum materialType, int quantity = 1)
    {
        InventoryItem item = materialItems.Find(i => 
            i.itemType == ItemType.Material && 
            i.materialType == materialType);
        
        if (item == null)
        {
            Debug.Log($"背包中没有{GetMaterialName(materialType)}");
            return false;
        }
        
        if (item.quantity < quantity)
        {
            Debug.Log($"{GetMaterialName(materialType)}数量不足");
            return false;
        }
        
        item.quantity -= quantity;
        if (item.quantity <= 0)
        {
            RemoveFromSlot(item);
            inventory.Remove(item);
            materialItems.Remove(item);
            currentInventorySize--;
        }
        
        Debug.Log($"从背包移除{quantity}个{GetMaterialName(materialType)}");
        return true;
    }
    
    // 使用药剂
    public bool UsePotion(RecipeType potionType)
    {
        InventoryItem item = consumableItems.Find(i => 
            i.itemType == ItemType.Consumable && 
            i.potionType == potionType);
        
        if (item == null)
        {
            Debug.Log($"背包中没有{GetPotionName(potionType)}");
            return false;
        }
        
        // 使用药剂效果
        ApplyPotionEffect(potionType);
        
        // 从背包移除一个药剂
        item.quantity--;
        if (item.quantity <= 0)
        {
            RemoveFromSlot(item);
            inventory.Remove(item);
            consumableItems.Remove(item);
            currentInventorySize--;
        }
        
        Debug.Log($"使用了{GetPotionName(potionType)}");
        return true;
    }
    
    // 装备武器
    public bool EquipWeapon(WeaponSystem.WeaponType weaponType)
    {
        InventoryItem item = inventory.Find(i => 
            i.itemType == ItemType.Weapon && 
            i.weaponType == weaponType);
        
        if (item == null)
        {
            Debug.Log($"背包中没有{GetWeaponName(weaponType)}");
            return false;
        }
        
        if (weaponSystem != null)
        {
            weaponSystem.SwitchWeapon(weaponType);
            Debug.Log($"装备了{item.itemName}");
            return true;
        }
        
        return false;
    }
    
    // 应用药剂效果
    void ApplyPotionEffect(RecipeType potionType)
    {
        if (alchemySystem != null)
        {
            alchemySystem.ApplyRecipeEffect(potionType);
        }
    }
    
    // 获取材料数据
    private MaterialData GetMaterialData(MaterialTypeEnum type)
    {
        switch (type)
        {
            // 基础材料（10种）
            case MaterialTypeEnum.RottenWood:
                return new MaterialData(type, "腐朽木屑", 1, "枯朽的木材碎片。", "常见", "森林地图采集");
            case MaterialTypeEnum.BoneFragments:
                return new MaterialData(type, "碎骨", 2, "断裂的骨骼。", "常见", "骷髅战士、腐化村民掉落");
            case MaterialTypeEnum.StarlightGrass:
                return new MaterialData(type, "星光草", 3, "夜晚发光的植物。", "常见", "草原、森林地图采集");
            case MaterialTypeEnum.CorruptedTissue:
                return new MaterialData(type, "腐化组织", 4, "被幻宫能量侵蚀的肉块。", "常见", "腐化村民、沼泽潜伏者掉落");
            case MaterialTypeEnum.MoonlightFlower:
                return new MaterialData(type, "月影花", 5, "满月夜绽放的银色花朵。", "常见", "月光下的森林、草原采集");
            case MaterialTypeEnum.RustedParts:
                return new MaterialData(type, "锈蚀零件", 6, "废弃机械的金属零件。", "常见", "机械残骸掉落，废墟地图采集");
            case MaterialTypeEnum.SoulDust:
                return new MaterialData(type, "灵魂微尘", 7, "逸散的灵魂能量微粒。", "常见", "怨魂等灵魂类敌人掉落");
            case MaterialTypeEnum.CloudyDew:
                return new MaterialData(type, "浑浊露珠", 8, "凝结时空能量的水珠。", "常见", "森林、草原、雪山地清晨采集");
            case MaterialTypeEnum.PurifyingSalt:
                return new MaterialData(type, "净化盐晶", 9, "蕴含净化能量的盐结晶。", "常见", "岩地、沙漠地图采集");
            case MaterialTypeEnum.WailingVine:
                return new MaterialData(type, "哀嚎藤蔓", 10, "触碰时会发出悲鸣的黑暗植物。", "常见", "沼泽、湿地地图采集");

            // 稀有材料（10种）
            case MaterialTypeEnum.MemoryResidue:
                return new MaterialData(type, "记忆残渣", 11, "记忆逸散的能量渣滓。", "稀有", "记忆碎片区域采集");
            case MaterialTypeEnum.TimeFragment:
                return new MaterialData(type, "时空碎片", 12, "包含不稳定时空能量的脆弱碎片。", "稀有", "时空守护者掉落，时空裂隙区域采集");
            case MaterialTypeEnum.SoulCrystal:
                return new MaterialData(type, "灵魂结晶", 13, "提纯凝固的灵魂能量块。", "稀有", "灵魂吞噬者、精英怨魂掉落");
            case MaterialTypeEnum.CrystalizedCore:
                return new MaterialData(type, "晶化残核", 14, "晶化生物遗留的惰性能量核。", "稀有", "结晶蜥蜴掉落");
            case MaterialTypeEnum.MechCore:
                return new MaterialData(type, "机械核心", 15, "古代机械的微型动力核心。", "稀有", "机械构造体掉落");
            case MaterialTypeEnum.AncientTreeResin:
                return new MaterialData(type, "古树树脂", 16, "古老树木渗出的生命力树脂。", "稀有", "森林地图特定古树采集");
            case MaterialTypeEnum.LavaCore:
                return new MaterialData(type, "熔岩核心", 17, "一小块永不熄灭的炽热能量核。", "稀有", "熔岩元素掉落");
            case MaterialTypeEnum.GargoyleFragment:
                return new MaterialData(type, "石像鬼碎片", 18, "活化雕像上剥落的带魔力石片。", "稀有", "石像鬼掉落");
            case MaterialTypeEnum.FrostShard:
                return new MaterialData(type, "极寒冰屑", 19, "永恒冻土中形成的刺骨冰屑。", "稀有", "雪山地采集，冰原狼掉落");
            case MaterialTypeEnum.AncientDragonBonePowder:
                return new MaterialData(type, "古龙骨粉", 20, "研磨自远古巨兽骨骼的能量粉末。", "稀有", "岩地、雪山地远古遗骸处采集");

            // 史诗材料（3种）
            case MaterialTypeEnum.SoulEssence:
                return new MaterialData(type, "灵魂精华", 25, "用于炼金的高纯度灵魂能量。", "史诗", "灵魂炼金（灵魂×50）");
            case MaterialTypeEnum.LeylineCrystal:
                return new MaterialData(type, "地脉结晶", 30, "大地能量脉络中凝结的坚固结晶。", "史诗", "岩地深处采集");
            case MaterialTypeEnum.AncientRuneStone:
                return new MaterialData(type, "远古铭文石", 33, "刻有失落知识的石碑碎片。", "史诗", "神殿、废墟解谜区域采集");

            // 传奇材料（1种）
            case MaterialTypeEnum.ParadoxShard:
                return new MaterialData(type, "悖时薄片", 40, "凝固的、记录时间悖论的时空片段。", "传奇", "时空守护者掉落");

            default:
                return new MaterialData(type, type.ToString(), 0, "未知材料", "未知", "未知");
        }
    }
    
    // 获取材料名称
    private string GetMaterialName(MaterialTypeEnum type)
    {
        return GetMaterialData(type).name;
    }
    
    // 获取药剂名称
    private string GetPotionName(RecipeType type)
    {
        switch (type)
        {
            case RecipeType.HealingPotion:
                return "治疗药剂";
            case RecipeType.SoulStabilizer:
                return "灵魂稳定剂";
            case RecipeType.SummonDurationPotion:
                return "召唤持续时间药剂";
            case RecipeType.BurdenReliefIncense:
                return "负担缓解熏香";
            case RecipeType.SummonEnhancer:
                return "召唤强化剂";
            case RecipeType.WeaknessInsightPotion:
                return "弱点洞察药剂";
            case RecipeType.MemoryResonancePotion:
                return "记忆共鸣药剂";
            case RecipeType.BurdenPurification:
                return "负担净化圣水";
            default:
                return type.ToString();
        }
    }
    
    // 获取药剂描述
    private string GetPotionDescription(RecipeType type)
    {
        switch (type)
        {
            case RecipeType.HealingPotion:
                return "恢复20%生命值";
            case RecipeType.SoulStabilizer:
                return "减少灵魂负担30点";
            case RecipeType.SummonDurationPotion:
                return "召唤物持续时间+60%";
            case RecipeType.BurdenReliefIncense:
                return "非战斗时，每分钟额外恢复1点负担";
            case RecipeType.SummonEnhancer:
                return "召唤物全属性+20%";
            case RecipeType.WeaknessInsightPotion:
                return "显示当前区域敌人弱点";
            case RecipeType.MemoryResonancePotion:
                return "临时激活第4个记忆碎片共鸣位";
            case RecipeType.BurdenPurification:
                return "完全清除所有灵魂负担";
            default:
                return "未知效果";
        }
    }
    
    // 获取武器名称
    private string GetWeaponName(WeaponSystem.WeaponType type)
    {
        switch (type)
        {
            case WeaponSystem.WeaponType.Sword:
                return "剑";
            case WeaponSystem.WeaponType.Staff:
                return "法杖";
            case WeaponSystem.WeaponType.Scythe:
                return "镰刀";
            case WeaponSystem.WeaponType.CrystalArm:
                return "水晶臂";
            default:
                return type.ToString();
        }
    }
    
    // 获取背包中物品数量
    public int GetItemQuantity(string itemName)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        return item != null ? item.quantity : 0;
    }
    
    // 获取背包中材料数量
    public int GetMaterialQuantity(MaterialTypeEnum materialType)
    {
        InventoryItem item = inventory.Find(i => 
            i.itemType == ItemType.Material && 
            i.materialType == materialType);
        return item != null ? item.quantity : 0;
    }
    
    // 获取背包中药剂数量
    public int GetPotionQuantity(RecipeType potionType)
    {
        InventoryItem item = inventory.Find(i => 
            i.itemType == ItemType.Consumable && 
            i.potionType == potionType);
        return item != null ? item.quantity : 0;
    }
    
    // 检查背包中是否有指定物品
    public bool HasItem(string itemName)
    {
        return inventory.Exists(i => i.itemName == itemName);
    }
    
    // 检查背包中是否有指定材料
    public bool HasMaterial(MaterialTypeEnum materialType)
    {
        return inventory.Exists(i => 
            i.itemType == ItemType.Material && 
            i.materialType == materialType);
    }
    
    // 检查背包中是否有指定药剂
    public bool HasPotion(RecipeType potionType)
    {
        return inventory.Exists(i => 
            i.itemType == ItemType.Consumable && 
            i.potionType == potionType);
    }
    
    // 获取背包中所有材料
    public List<InventoryItem> GetAllMaterials()
    {
        return materialItems;
    }
    
    // 获取背包中所有药剂
    public List<InventoryItem> GetAllPotions()
    {
        return consumableItems.FindAll(i => i.potionType.ToString() != "None");
    }
    
    // 获取背包中所有武器
    public List<InventoryItem> GetAllWeapons()
    {
        return weaponItems;
    }
    
    // 获取背包中所有记忆碎片
    public List<InventoryItem> GetAllMemoryFragments()
    {
        return memoryFragmentItems;
    }
    
    // 显示背包内容
    public void DisplayInventory()
    {
        Debug.Log("=== 背包内容 ===");
        Debug.Log($"背包容量: {currentInventorySize}/{(maxMaterialSlots + maxConsumableSlots + maxWeaponSlots + maxMemoryFragmentSlots)}");
        
        List<InventoryItem> materials = GetAllMaterials();
        if (materials.Count > 0)
        {
            Debug.Log("材料:");
            foreach (var item in materials)
            {
                Debug.Log($"  {item.itemName}: {item.quantity}");
            }
        }
        
        List<InventoryItem> consumables = inventory.FindAll(i => i.itemType == ItemType.Consumable);
        if (consumables.Count > 0)
        {
            Debug.Log("消耗品:");
            foreach (var item in consumables)
            {
                Debug.Log($"  {item.itemName}: {item.quantity}");
            }
        }
        
        List<InventoryItem> weapons = GetAllWeapons();
        if (weapons.Count > 0)
        {
            Debug.Log("武器:");
            foreach (var item in weapons)
            {
                Debug.Log($"  {item.itemName}: {item.quantity} (等级: {item.weaponLevel})");
            }
        }
        
        List<InventoryItem> memoryFragments = GetAllMemoryFragments();
        if (memoryFragments.Count > 0)
        {
            Debug.Log("记忆碎片:");
            foreach (var item in memoryFragments)
            {
                Debug.Log($"  {item.itemName}: {item.quantity}");
            }
        }
        
        Debug.Log("================");
        
        // 显示空间使用情况
        Debug.Log($"空间使用情况:");
        Debug.Log($"  材料: {materialItems.Count}/{maxMaterialSlots}");
        Debug.Log($"  消耗品: {consumableItems.Count}/{maxConsumableSlots}");
        Debug.Log($"  武器: {weaponItems.Count}/{maxWeaponSlots}");
        Debug.Log($"  记忆碎片: {memoryFragmentItems.Count}/{maxMemoryFragmentSlots}");
        Debug.Log("===============");
    }
    
    // 清空背包
    public void ClearInventory()
    {
        inventory.Clear();
        materialItems.Clear();
        consumableItems.Clear();
        weaponItems.Clear();
        memoryFragmentItems.Clear();
        currentInventorySize = 0;
        
        // 清空所有格子
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                materialSlots[layer, slot] = null;
                consumableSlots[layer, slot] = null;
                weaponSlots[layer, slot] = null;
            }
        }
        
        // 清空记忆碎片格子
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                memoryFragmentSlots[layer, slot] = null;
            }
        }
        
        Debug.Log("背包已清空");
    }
    
    // 获取背包剩余空间
    public int GetRemainingSpace()
    {
        return (maxMaterialSlots + maxConsumableSlots + maxWeaponSlots + maxMemoryFragmentSlots) - currentInventorySize;
    }
    
    // 检查背包是否已满
    public bool IsInventoryFull()
    {
        return currentInventorySize >= (maxMaterialSlots + maxConsumableSlots + maxWeaponSlots + maxMemoryFragmentSlots);
    }
    
    // 获取所有物品
    public List<InventoryItem> GetItems()
    {
        return inventory;
    }
    
    // 使用物品
    public bool UseItem(InventoryItem item)
    {
        if (item == null)
        {
            Debug.Log("[InventorySystem] 物品为空");
            return false;
        }
        
        Debug.Log($"[InventorySystem] 开始使用物品: {item.itemName}, 类型: {item.itemType}, 数量: {item.quantity}");
        
        switch (item.itemType)
        {
            case ItemType.Consumable:
                // 处理消耗品逻辑
                item.quantity--;
                Debug.Log($"[InventorySystem] 消耗品数量减少到: {item.quantity}");
                
                if (item.quantity <= 0)
                {
                    RemoveFromSlot(item);
                    inventory.Remove(item);
                    consumableItems.Remove(item);
                    currentInventorySize--;
                    Debug.Log($"[InventorySystem] 消耗品数量为0，从背包中移除: {item.itemName}");
                }
                
                // 检查是否是药剂并应用效果
                Debug.Log($"[InventorySystem] 消耗品potionType: {item.potionType}");
                if (item.potionType != RecipeType.None)
                {
                    Debug.Log($"[InventorySystem] 使用药剂: {item.itemName}，效果: {GetPotionDescription(item.potionType)}");
                    ApplyPotionEffect(item.potionType);
                }
                else
                {
                    Debug.Log($"[InventorySystem] 使用消耗品: {item.itemName}");
                }
                return true;
            default:
                Debug.Log($"[InventorySystem] {item.itemName}不能直接使用");
                return false;
        }
    }
    
    // 装备物品
    public bool EquipItem(InventoryItem item)
    {
        if (item == null || item.itemType != ItemType.Weapon)
            return false;
        
        // 先卸下相同类型的装备
        foreach (var invItem in inventory)
        {
            if (invItem.itemType == ItemType.Weapon && invItem.isEquipped)
            {
                invItem.isEquipped = false;
            }
        }
        
        item.isEquipped = true;
        
        if (weaponSystem != null)
        {
            weaponSystem.SwitchWeapon(item.weaponType);
        }
        
        Debug.Log($"装备了{item.itemName}");
        return true;
    }
    
    // 卸下物品
    public bool UnequipItem(InventoryItem item)
    {
        if (item == null || !item.isEquipped)
            return false;
        
        item.isEquipped = false;
        Debug.Log($"卸下了{item.itemName}");
        return true;
    }
    
    // 添加材料到材料栏目格子
    private bool AddToMaterialSlot(InventoryItem item)
    {
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (materialSlots[layer, slot] == null)
                {
                    materialSlots[layer, slot] = item;
                    return true;
                }
            }
        }
        return false;
    }
    
    // 添加消耗品到消耗品栏目格子
    private bool AddToConsumableSlot(InventoryItem item)
    {
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (consumableSlots[layer, slot] == null)
                {
                    consumableSlots[layer, slot] = item;
                    return true;
                }
            }
        }
        return false;
    }
    
    // 添加武器到武器栏目格子
    private bool AddToWeaponSlot(InventoryItem item)
    {
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (weaponSlots[layer, slot] == null)
                {
                    weaponSlots[layer, slot] = item;
                    return true;
                }
            }
        }
        return false;
    }
    
    // 添加记忆碎片到记忆碎片栏目格子
    private bool AddToMemoryFragmentSlot(InventoryItem item)
    {
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (memoryFragmentSlots[layer, slot] == null)
                {
                    memoryFragmentSlots[layer, slot] = item;
                    return true;
                }
            }
        }
        return false;
    }
    
    // 获取材料栏目格子
    public InventoryItem[,] GetMaterialSlots()
    {
        return materialSlots;
    }
    
    // 获取消耗品栏目格子
    public InventoryItem[,] GetConsumableSlots()
    {
        return consumableSlots;
    }
    
    // 获取武器栏目格子
    public InventoryItem[,] GetWeaponSlots()
    {
        return weaponSlots;
    }

    // 获取记忆碎片和特殊物品栏目格子
    public InventoryItem[,] GetMemoryFragmentSlots()
    {
        return memoryFragmentSlots;
    }
    
    // 从格子中移除物品
    private void RemoveFromSlot(InventoryItem item)
    {
        // 从材料栏目格子中移除
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (materialSlots[layer, slot] == item)
                {
                    materialSlots[layer, slot] = null;
                    return;
                }
            }
        }
        
        // 从消耗品栏目格子中移除
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (consumableSlots[layer, slot] == item)
                {
                    consumableSlots[layer, slot] = null;
                    return;
                }
            }
        }
        
        // 从武器栏目格子中移除
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (weaponSlots[layer, slot] == item)
                {
                    weaponSlots[layer, slot] = null;
                    return;
                }
            }
        }
        
        // 从记忆碎片栏目格子中移除
        for (int layer = 0; layer < 4; layer++)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                if (memoryFragmentSlots[layer, slot] == item)
                {
                    memoryFragmentSlots[layer, slot] = null;
                    return;
                }
            }
        }
    }
}
