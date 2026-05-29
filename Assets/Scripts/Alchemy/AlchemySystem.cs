using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameSystems;

public enum RecipeType
{
    None,                 // 无（非药剂）
    // 基础配方（4个，初始已知）
    SoulStabilizer,       // 灵魂稳定剂
    HealingPotion,        // 治疗药剂
    SummonDurationPotion, // 召唤持续时间药剂
    BurdenReliefIncense,  // 负担缓解熏香
    // 进阶物品配方（隐藏）
    SummonEnhancer,       // 召唤强化剂
    WeaknessInsightPotion,// 弱点洞察药剂
    MemoryResonancePotion,// 记忆共鸣药剂
    MechServantCore,      // 机械仆从核心
    EnvironmentStabilizer,// 环境稳定卷轴
    SyncEnhancerPotion,   // 同频强化药剂
    RebirthAmulet,        // 重生护符
    TimeBeacon,           // 时空道标
    TruthSeerPotion,      // 真理窥视药剂
    BurdenPurification,   // 负担净化圣水
    // 材料合成配方（隐藏）
    Material_MemoryResidue,      // 记忆残渣（11）
    Material_TimeFragment,       // 时空碎片（12）
    Material_SoulCrystal,        // 灵魂结晶（13）
    Material_CrystalizedCore,    // 晶化残核（14）
    Material_MechCore,           // 机械核心（15）
    Material_AncientTreeResin,   // 古树树脂（16）
    Material_LavaCore,           // 熔岩核心（17）
    Material_GargoyleFragment,   // 石像鬼碎片（18）
    Material_FrostShard,         // 极寒冰屑（19）
    Material_AncientDragonBonePowder, // 古龙骨粉（20）
    Material_LeylineCrystal,     // 地脉结晶（30）
    Material_AncientRuneStone,   // 远古铭文石（33）
    Material_SoulEssence         // 灵魂精华（25）- 炼金产物
}

public enum RecipeTier
{
    Basic,
    Advanced,
    Legendary
}

[System.Serializable]
public class RecipeData
{
    public RecipeType type;
    public string name;
    public RecipeTier tier;
    public Dictionary<MaterialTypeEnum, int> requiredMaterials;
    public string resultDescription;
    public int alchemicalValue;

    public RecipeData(RecipeType type, string name, RecipeTier tier, Dictionary<MaterialTypeEnum, int> requiredMaterials, string resultDescription, int alchemicalValue)
    {
        this.type = type;
        this.name = name;
        this.tier = tier;
        this.requiredMaterials = requiredMaterials;
        this.resultDescription = resultDescription;
        this.alchemicalValue = alchemicalValue;
    }
}

public class AlchemySystem : MonoBehaviour, IAlchemyService, IRecipeEffectProvider
{
    float IAlchemyService.recipeBaseSuccessRate { get => recipeBaseSuccessRate; set => recipeBaseSuccessRate = value; }
    [Header("材料库存")]
    public Dictionary<MaterialTypeEnum, int> materialInventory = new Dictionary<MaterialTypeEnum, int>();

    [Header("可用配方")]
    public List<RecipeData> availableRecipes = new List<RecipeData>();
    
    [Header("已发现配方（炼金笔记）")]
    public List<RecipeType> discoveredRecipes = new List<RecipeType>();

    [Header("炼金成功率配置")]
    [Range(0f, 1f)]
    public float recipeBaseSuccessRate = 0.7f; // 配方炼金基础成功率（默认70%）
    [Range(0f, 1f)]
    public float freeAlchemyBaseSuccessRate = 0.7f; // 自由炼金基础成功率（默认70%）
    [Range(0f, 1f)]
    public float materialCraftSuccessRate = 0.8f; // 材料合成基础成功率（默认80%）
    [Range(0f, 1f)]
    public float memoryShardBonus = 0.15f; // 记忆碎片加成（默认+15%）
    [Range(0f, 1f)]
    public float successRateMinimum = 0.05f; // 成功率下限（默认5%）
    [Range(0f, 1f)]
    public float successRateMaximum = 0.95f; // 成功率上限（默认95%）

    [Header("系统引用")]
    private PlayerController playerController;
    private HealthSystem playerHealth;
    private CharacterStats characterStats;
    private BurdenSystem burdenSystem;
    private InventorySystem inventorySystem;
    
    [Header("合成统计")]
    private int failedAttempts = 0; // 失败次数（用于新手保护）
    private const int NEWBIE_PROTECTION_COUNT = 3; // 新手保护次数
    
    // 临时产出存储
    private List<object> pendingProduce = new List<object>(); // 待收集的产出物品

    void Start()
    {
        InitializeMaterialInventory();
        InitializeRecipes();
        InitializeDiscoveredRecipes(); // 初始化已发现配方（基础配方默认已知）
        playerController = FindObjectOfType<PlayerController>();
        playerHealth = FindObjectOfType<HealthSystem>();
        characterStats = FindObjectOfType<CharacterStats>();
        burdenSystem = FindObjectOfType<BurdenSystem>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        
        // 验证关键组件
        if (playerHealth == null)
        {
            Debug.LogWarning("AlchemySystem: Player HealthSystem component not found!");
        }
        if (inventorySystem == null)
        {
            Debug.LogWarning("AlchemySystem: InventorySystem component not found!");
        }
    }

    void InitializeMaterialInventory()
    {
        foreach (MaterialTypeEnum type in System.Enum.GetValues(typeof(MaterialTypeEnum)))
        {
            materialInventory[type] = 0;
        }
    }
    
    void InitializeDiscoveredRecipes()
    {
        // 基础配方默认已知（根据设计文档，4个基础配方）
        discoveredRecipes.Add(RecipeType.SoulStabilizer);
        discoveredRecipes.Add(RecipeType.HealingPotion);
        discoveredRecipes.Add(RecipeType.SummonDurationPotion);
        discoveredRecipes.Add(RecipeType.BurdenReliefIncense);
        
        Debug.Log($"[AlchemySystem] 初始化炼金笔记，已知配方: {discoveredRecipes.Count}个");
    }

    void InitializeRecipes()
    {
        Dictionary<MaterialTypeEnum, int> soulStabilizerRecipe = new Dictionary<MaterialTypeEnum, int>();
        soulStabilizerRecipe[MaterialTypeEnum.CloudyDew] = 3;
        soulStabilizerRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.SoulStabilizer, "灵魂稳定剂", RecipeTier.Basic, soulStabilizerRecipe, "减少灵魂负担30点。", 25));

        Dictionary<MaterialTypeEnum, int> healingPotionRecipe = new Dictionary<MaterialTypeEnum, int>();
        healingPotionRecipe[MaterialTypeEnum.MoonlightFlower] = 1;
        healingPotionRecipe[MaterialTypeEnum.StarlightGrass] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.HealingPotion, "治疗药剂", RecipeTier.Basic, healingPotionRecipe, "恢复20%生命值。", 8));

        Dictionary<MaterialTypeEnum, int> summonDurationPotionRecipe = new Dictionary<MaterialTypeEnum, int>();
        summonDurationPotionRecipe[MaterialTypeEnum.BoneFragments] = 3;
        summonDurationPotionRecipe[MaterialTypeEnum.SoulDust] = 2;
        availableRecipes.Add(new RecipeData(RecipeType.SummonDurationPotion, "召唤持续时间药剂", RecipeTier.Basic, summonDurationPotionRecipe, "召唤物持续时间+60%。", 20));

        Dictionary<MaterialTypeEnum, int> burdenReliefIncenseRecipe = new Dictionary<MaterialTypeEnum, int>();
        burdenReliefIncenseRecipe[MaterialTypeEnum.MoonlightFlower] = 2;
        burdenReliefIncenseRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.BurdenReliefIncense, "负担缓解熏香", RecipeTier.Basic, burdenReliefIncenseRecipe, "非战斗时，每分钟额外恢复1点负担，持续10分钟。", 19));

        Dictionary<MaterialTypeEnum, int> summonEnhancerRecipe = new Dictionary<MaterialTypeEnum, int>();
        summonEnhancerRecipe[MaterialTypeEnum.SoulEssence] = 1;
        summonEnhancerRecipe[MaterialTypeEnum.MechCore] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.SummonEnhancer, "召唤强化剂", RecipeTier.Advanced, summonEnhancerRecipe, "召唤物全属性+20%，持续10分钟。", 16));

        Dictionary<MaterialTypeEnum, int> weaknessInsightPotionRecipe = new Dictionary<MaterialTypeEnum, int>();
        weaknessInsightPotionRecipe[MaterialTypeEnum.TimeFragment] = 1;
        weaknessInsightPotionRecipe[MaterialTypeEnum.SoulCrystal] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.WeaknessInsightPotion, "弱点洞察药剂", RecipeTier.Advanced, weaknessInsightPotionRecipe, "显示当前区域敌人弱点，持续3场战斗。", 25));

        Dictionary<MaterialTypeEnum, int> memoryResonancePotionRecipe = new Dictionary<MaterialTypeEnum, int>();
        memoryResonancePotionRecipe[MaterialTypeEnum.MemoryResidue] = 3;
        memoryResonancePotionRecipe[MaterialTypeEnum.SoulCrystal] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.MemoryResonancePotion, "记忆共鸣药剂", RecipeTier.Advanced, memoryResonancePotionRecipe, "临时激活第4个记忆碎片共鸣位，持续至下次营火休息。", 46));

        Dictionary<MaterialTypeEnum, int> mechServantCoreRecipe = new Dictionary<MaterialTypeEnum, int>();
        mechServantCoreRecipe[MaterialTypeEnum.MechCore] = 1;
        mechServantCoreRecipe[MaterialTypeEnum.RustedParts] = 3;
        availableRecipes.Add(new RecipeData(RecipeType.MechServantCore, "机械仆从核心", RecipeTier.Advanced, mechServantCoreRecipe, "召唤一个机械仆从，持续整场战斗。", 33));

        Dictionary<MaterialTypeEnum, int> environmentStabilizerRecipe = new Dictionary<MaterialTypeEnum, int>();
        environmentStabilizerRecipe[MaterialTypeEnum.PurifyingSalt] = 3;
        environmentStabilizerRecipe[MaterialTypeEnum.AncientRuneStone] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.EnvironmentStabilizer, "环境稳定卷轴", RecipeTier.Advanced, environmentStabilizerRecipe, "周围不稳定环境效果失效20秒。", 60));

        Dictionary<MaterialTypeEnum, int> syncEnhancerPotionRecipe = new Dictionary<MaterialTypeEnum, int>();
        syncEnhancerPotionRecipe[MaterialTypeEnum.SoulCrystal] = 2;
        syncEnhancerPotionRecipe[MaterialTypeEnum.CrystalizedCore] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.SyncEnhancerPotion, "同频强化药剂", RecipeTier.Advanced, syncEnhancerPotionRecipe, "30秒内，召唤物获得玩家主武器的元素附魔。", 40));

        Dictionary<MaterialTypeEnum, int> rebirthAmuletRecipe = new Dictionary<MaterialTypeEnum, int>();
        rebirthAmuletRecipe[MaterialTypeEnum.LavaCore] = 1;
        rebirthAmuletRecipe[MaterialTypeEnum.AncientTreeResin] = 1;
        availableRecipes.Add(new RecipeData(RecipeType.RebirthAmulet, "重生护符", RecipeTier.Advanced, rebirthAmuletRecipe, "首次死亡时立即复活，恢复30%生命与魔法。（单局限一次）", 33));

        Dictionary<MaterialTypeEnum, int> timeBeaconRecipe = new Dictionary<MaterialTypeEnum, int>();
        timeBeaconRecipe[MaterialTypeEnum.ParadoxShard] = 1;
        timeBeaconRecipe[MaterialTypeEnum.TimeFragment] = 3;
        availableRecipes.Add(new RecipeData(RecipeType.TimeBeacon, "时空道标", RecipeTier.Legendary, timeBeaconRecipe, "在当前地图设置一个永久传送点。", 76));

        Dictionary<MaterialTypeEnum, int> truthSeerPotionRecipe = new Dictionary<MaterialTypeEnum, int>();
        truthSeerPotionRecipe[MaterialTypeEnum.ParadoxShard] = 1;
        truthSeerPotionRecipe[MaterialTypeEnum.AncientRuneStone] = 2;
        availableRecipes.Add(new RecipeData(RecipeType.TruthSeerPotion, "真理窥视药剂", RecipeTier.Legendary, truthSeerPotionRecipe, "揭示当前区域所有隐藏通道、宝箱及剧情线索。", 106));

        Dictionary<MaterialTypeEnum, int> burdenPurificationRecipe = new Dictionary<MaterialTypeEnum, int>();
        burdenPurificationRecipe[MaterialTypeEnum.LeylineCrystal] = 1;
        burdenPurificationRecipe[MaterialTypeEnum.AncientTreeResin] = 2;
        availableRecipes.Add(new RecipeData(RecipeType.BurdenPurification, "负担净化圣水", RecipeTier.Legendary, burdenPurificationRecipe, "完全清除所有灵魂负担，并在10分钟内负担增长减半。", 62));

            // ========== 材料合成配方（隐藏） ==========
            // 稀有材料（价值11-20）
            Dictionary<MaterialTypeEnum, int> memoryResidueRecipe = new Dictionary<MaterialTypeEnum, int>();
            memoryResidueRecipe[MaterialTypeEnum.SoulDust] = 1;
            memoryResidueRecipe[MaterialTypeEnum.RottenWood] = 1;
            memoryResidueRecipe[MaterialTypeEnum.CorruptedTissue] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_MemoryResidue, "记忆残渣", RecipeTier.Advanced, memoryResidueRecipe, "记忆逸散的能量渣滓，用于合成高级药剂。", 11));

            Dictionary<MaterialTypeEnum, int> timeFragmentRecipe = new Dictionary<MaterialTypeEnum, int>();
            timeFragmentRecipe[MaterialTypeEnum.CloudyDew] = 1;
            timeFragmentRecipe[MaterialTypeEnum.SoulDust] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_TimeFragment, "时空碎片", RecipeTier.Advanced, timeFragmentRecipe, "包含不稳定时空能量的脆弱碎片。", 12));

            Dictionary<MaterialTypeEnum, int> soulCrystalRecipe = new Dictionary<MaterialTypeEnum, int>();
            soulCrystalRecipe[MaterialTypeEnum.SoulDust] = 1;
            soulCrystalRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
            soulCrystalRecipe[MaterialTypeEnum.CloudyDew] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_SoulCrystal, "灵魂结晶", RecipeTier.Advanced, soulCrystalRecipe, "提纯凝固的灵魂能量块。", 13));

            Dictionary<MaterialTypeEnum, int> crystalizedCoreRecipe = new Dictionary<MaterialTypeEnum, int>();
            crystalizedCoreRecipe[MaterialTypeEnum.RustedParts] = 2;
            crystalizedCoreRecipe[MaterialTypeEnum.CloudyDew] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_CrystalizedCore, "晶化残核", RecipeTier.Advanced, crystalizedCoreRecipe, "晶化生物遗留的惰性能量核。", 14));

            Dictionary<MaterialTypeEnum, int> mechCoreRecipe = new Dictionary<MaterialTypeEnum, int>();
            mechCoreRecipe[MaterialTypeEnum.RustedParts] = 2;
            mechCoreRecipe[MaterialTypeEnum.SoulDust] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_MechCore, "机械核心", RecipeTier.Advanced, mechCoreRecipe, "古代机械的微型动力核心。", 15));

            Dictionary<MaterialTypeEnum, int> ancientTreeResinRecipe = new Dictionary<MaterialTypeEnum, int>();
            ancientTreeResinRecipe[MaterialTypeEnum.MoonlightFlower] = 2;
            ancientTreeResinRecipe[MaterialTypeEnum.StarlightGrass] = 2;
            availableRecipes.Add(new RecipeData(RecipeType.Material_AncientTreeResin, "古树树脂", RecipeTier.Advanced, ancientTreeResinRecipe, "古老树木渗出的生命力树脂。", 16));

            Dictionary<MaterialTypeEnum, int> lavaCoreRecipe = new Dictionary<MaterialTypeEnum, int>();
            lavaCoreRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
            lavaCoreRecipe[MaterialTypeEnum.SoulDust] = 2;
            lavaCoreRecipe[MaterialTypeEnum.CorruptedTissue] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_LavaCore, "熔岩核心", RecipeTier.Advanced, lavaCoreRecipe, "一小块永不熄灭的炽热能量核。", 17));

            Dictionary<MaterialTypeEnum, int> gargoyleFragmentRecipe = new Dictionary<MaterialTypeEnum, int>();
            gargoyleFragmentRecipe[MaterialTypeEnum.BoneFragments] = 2;
            gargoyleFragmentRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
            gargoyleFragmentRecipe[MaterialTypeEnum.SoulDust] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_GargoyleFragment, "石像鬼碎片", RecipeTier.Advanced, gargoyleFragmentRecipe, "活化雕像上剥落的带魔力石片。", 18));

            Dictionary<MaterialTypeEnum, int> frostShardRecipe = new Dictionary<MaterialTypeEnum, int>();
            frostShardRecipe[MaterialTypeEnum.CloudyDew] = 2;
            frostShardRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_FrostShard, "极寒冰屑", RecipeTier.Advanced, frostShardRecipe, "永恒冻土中形成的刺骨冰屑。", 19));

            Dictionary<MaterialTypeEnum, int> ancientDragonBonePowderRecipe = new Dictionary<MaterialTypeEnum, int>();
            ancientDragonBonePowderRecipe[MaterialTypeEnum.BoneFragments] = 2;
            ancientDragonBonePowderRecipe[MaterialTypeEnum.SoulDust] = 2;
            ancientDragonBonePowderRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_AncientDragonBonePowder, "古龙骨粉", RecipeTier.Advanced, ancientDragonBonePowderRecipe, "研磨自远古巨兽骨骼的能量粉末。", 20));

            // 史诗材料（价值25, 30, 33）
            Dictionary<MaterialTypeEnum, int> soulEssenceRecipe = new Dictionary<MaterialTypeEnum, int>();
            soulEssenceRecipe[MaterialTypeEnum.SoulCrystal] = 1;
            soulEssenceRecipe[MaterialTypeEnum.SoulDust] = 3;
            availableRecipes.Add(new RecipeData(RecipeType.Material_SoulEssence, "灵魂精华", RecipeTier.Advanced, soulEssenceRecipe, "用于炼金的高纯度灵魂能量。", 1));

            Dictionary<MaterialTypeEnum, int> leylineCrystalRecipe = new Dictionary<MaterialTypeEnum, int>();
            leylineCrystalRecipe[MaterialTypeEnum.CloudyDew] = 2;
            leylineCrystalRecipe[MaterialTypeEnum.PurifyingSalt] = 2;
            leylineCrystalRecipe[MaterialTypeEnum.SoulCrystal] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_LeylineCrystal, "地脉结晶", RecipeTier.Legendary, leylineCrystalRecipe, "大地能量脉络中凝结的坚固结晶。", 30));

            Dictionary<MaterialTypeEnum, int> ancientRuneStoneRecipe = new Dictionary<MaterialTypeEnum, int>();
            ancientRuneStoneRecipe[MaterialTypeEnum.TimeFragment] = 1;
            ancientRuneStoneRecipe[MaterialTypeEnum.SoulCrystal] = 1;
            ancientRuneStoneRecipe[MaterialTypeEnum.PurifyingSalt] = 1;
            availableRecipes.Add(new RecipeData(RecipeType.Material_AncientRuneStone, "远古铭文石", RecipeTier.Legendary, ancientRuneStoneRecipe, "刻有失落知识的石碑碎片。", 33));
    }

    public void AddMaterial(MaterialTypeEnum type, int amount)
    {
        // 确保数量为正数
        if (amount <= 0) return;
        
        if (materialInventory.ContainsKey(type))
        {
            materialInventory[type] += amount;
            Debug.Log("获得 " + amount + " 个 " + GetMaterialName(type));
        }
        else
        {
            // 如果材料类型不存在，添加到库存中
            materialInventory[type] = amount;
            Debug.Log("获得 " + amount + " 个 " + GetMaterialName(type));
        }
        
        // 同步到背包系统
        if (inventorySystem != null)
        {
            inventorySystem.AddMaterial(type, amount);
        }
    }
    
    // 添加消耗品到背包
    private void AddPotionToInventory(RecipeType potionType, int quantity)
    {
        if (inventorySystem != null)
        {
            inventorySystem.AddPotion(potionType, quantity);
            Debug.Log($"[AlchemySystem] 获得消耗品: {GetRecipeName(potionType)} × {quantity}");
        }
        else
        {
            Debug.LogWarning("[AlchemySystem] InventorySystem未找到，无法添加消耗品到背包");
        }
    }
    
    // 移除材料
    public void RemoveMaterial(MaterialTypeEnum type, int amount)
    {
        // 确保数量为正数
        if (amount <= 0) return;
        
        if (materialInventory.ContainsKey(type))
        {
            materialInventory[type] -= amount;
            if (materialInventory[type] <= 0)
            {
                materialInventory.Remove(type);
            }
            Debug.Log("移除 " + amount + " 个 " + GetMaterialName(type));
        }
    }
    
    // 设置材料数量（不自动同步到背包）
    public void SetMaterialCount(MaterialTypeEnum type, int amount)
    {
        if (amount <= 0)
        {
            if (materialInventory.ContainsKey(type))
            {
                materialInventory.Remove(type);
            }
        }
        else
        {
            materialInventory[type] = amount;
        }
    }

    public bool CanCraftRecipe(RecipeType recipeType)
    {
        RecipeData recipe = GetRecipeByType(recipeType);
        if (recipe == null || recipe.requiredMaterials == null || recipe.requiredMaterials.Count == 0) 
        {
            Debug.Log($"[AlchemySystem] CanCraftRecipe失败：配方不存在或无需求材料");
            return false;
        }

        Debug.Log($"[AlchemySystem] 检查配方 {recipe.name} 的材料...");
        foreach (var requiredMaterial in recipe.requiredMaterials)
        {
            MaterialTypeEnum materialType = requiredMaterial.Key;
            int requiredAmount = requiredMaterial.Value;
            int currentAmount = materialInventory.ContainsKey(materialType) ? materialInventory[materialType] : 0;
            
            Debug.Log($"[AlchemySystem] 材料 {GetMaterialName(materialType)}: 需求 {requiredAmount}, 当前 {currentAmount}");

            if (!materialInventory.ContainsKey(materialType) || materialInventory[materialType] < requiredAmount)
            {
                Debug.Log($"[AlchemySystem] CanCraftRecipe失败：{GetMaterialName(materialType)} 不足");
                return false;
            }
        }

        Debug.Log($"[AlchemySystem] CanCraftRecipe成功：可以制作 {recipe.name}");
        return true;
    }

    // 自由炼金：使用准备篮中的材料进行合成
    public bool CraftWithMaterials(MaterialTypeEnum[] materials)
    {
        // 清空待收集产出
        pendingProduce.Clear();
        
        if (materials == null || materials.Length == 0)
        {
            Debug.Log("[AlchemySystem] 准备篮中没有材料");
            PlayAudioViaReflection("PlayAlchemyFail");
            return false;
        }
        
        // 检查材料是否足够
        foreach (var mat in materials)
        {
            if (!materialInventory.ContainsKey(mat) || materialInventory[mat] <= 0)
            {
                Debug.Log("[AlchemySystem] 材料不足，无法进行自由炼金");
                PlayAudioViaReflection("PlayAlchemyFail");
                return false;
            }
        }
        
        // 计算成功率（基于材料稀有度）
        float successRate = CalculateFreeAlchemySuccessRate(materials);
        Debug.Log($"[AlchemySystem] 自由炼金成功率: {successRate * 100:F1}%");
        
        // 消耗材料
        foreach (var mat in materials)
        {
            RemoveMaterial(mat, 1);
        }
        
        // 生成随机数判断是否成功
        float random = Random.Range(0f, 1f);
        
        if (random <= successRate)
        {
            // 合成成功：随机生成一个消耗品或材料
            GenerateRandomResult(materials);
            Debug.Log("[AlchemySystem] ✅ 自由炼金成功！");
            PlayAudioViaReflection("PlayAlchemySuccess");
            return true;
        }
        else
        {
            // 合成失败：返还部分材料并获得炼金残渣
            failedAttempts++;
            RefundMaterialsForFreeAlchemy(materials);
            // 把记忆残渣加入待收集产出
            pendingProduce.Add(MaterialTypeEnum.MemoryResidue);
            Debug.Log("[AlchemySystem] ❌ 自由炼金失败");
            PlayAudioViaReflection("PlayAlchemyFail");
            return false;
        }
    }
    
    // 计算自由炼金成功率（根据设计文档）
    private float CalculateFreeAlchemySuccessRate(MaterialTypeEnum[] materials)
    {
        // 获取玩家属性
        int wisdom = characterStats != null ? characterStats.wisdom : 10;
        float burden = burdenSystem != null ? burdenSystem.currentBurden : 0f;
        bool hasMemoryShard = inventorySystem != null && inventorySystem.HasItem("MemoryShard");
        
        // 物品合成成功率：基础成功率 = 可配置基础值 + (智慧 × 1%) - (当前负担 × 0.2%)
        float baseRate = freeAlchemyBaseSuccessRate + (wisdom * 0.01f) - (burden * 0.002f);
        
        // 记忆碎片加成
        if (hasMemoryShard)
        {
            baseRate += memoryShardBonus;
        }
        
        // 计算投入总价值（用于材料合成判定）
        int totalValue = materials.Sum(m => GetMaterialValue(m));
        
        // 如果是材料合成（目标价值为11-20、30、33），使用材料合成公式
        MaterialTypeEnum targetMaterial = GetMaterialByValue(totalValue);
        if (targetMaterial != MaterialTypeEnum.RottenWood)
        {
            // 材料合成成功率：基础成功率 = 可配置基础值 - (目标材料价值 - 10) × 2%
            baseRate = materialCraftSuccessRate - (GetMaterialValue(targetMaterial) - 10) * 0.02f;
            // 记忆碎片加成
            if (hasMemoryShard)
            {
                baseRate += memoryShardBonus;
            }
        }
        
        // 成功率上下限（使用可配置的范围）
        return Mathf.Clamp(baseRate, successRateMinimum, successRateMaximum);
    }
    
    // 根据价值获取可合成的材料类型
    private MaterialTypeEnum GetMaterialByValue(int value)
    {
        // 可合成材料价值：11-20（稀有）、25（史诗）、30、33（史诗）
        switch (value)
        {
            case 11: return MaterialTypeEnum.MemoryResidue;
            case 12: return MaterialTypeEnum.TimeFragment;
            case 13: return MaterialTypeEnum.SoulCrystal;
            case 14: return MaterialTypeEnum.CrystalizedCore;
            case 15: return MaterialTypeEnum.MechCore;
            case 16: return MaterialTypeEnum.AncientTreeResin;
            case 17: return MaterialTypeEnum.LavaCore;
            case 18: return MaterialTypeEnum.GargoyleFragment;
            case 19: return MaterialTypeEnum.FrostShard;
            case 20: return MaterialTypeEnum.AncientDragonBonePowder;
            case 25: return MaterialTypeEnum.SoulEssence;
            case 30: return MaterialTypeEnum.LeylineCrystal;
            case 33: return MaterialTypeEnum.AncientRuneStone;
            default: return MaterialTypeEnum.RottenWood;
        }
    }
    
    // 自由炼金成功时生成结果（根据设计文档）
    private void GenerateRandomResult(MaterialTypeEnum[] materials)
    {
        // 计算投入总价值
        int totalValue = materials.Sum(m => GetMaterialValue(m));
        Debug.Log($"[AlchemySystem] 投入材料: {string.Join(", ", materials.Select(m => GetMaterialName(m)))}，总价值: {totalValue}");
        
        // 优先：匹配物品配方（检查材料是否匹配任何已知配方）
        RecipeType matchedRecipe = MatchRecipeByMaterials(materials);
        Debug.Log($"[AlchemySystem] 配方匹配结果: {matchedRecipe}");
        
        if (matchedRecipe != RecipeType.None) // 不是None表示匹配成功
        {
            // 应用配方效果
            ApplyRecipeEffect(matchedRecipe);
            
            // 如果是消耗品配方，添加到待收集产出
            if (!IsMaterialRecipe(matchedRecipe))
            {
                pendingProduce.Add(matchedRecipe);
            }
            else
            {
                // 材料配方也加入待收集产出
                MaterialTypeEnum materialType = GetMaterialTypeFromRecipe(matchedRecipe);
                if (materialType != MaterialTypeEnum.RottenWood)
                {
                    pendingProduce.Add(materialType);
                }
            }
            
            Debug.Log($"[AlchemySystem] ✅ 发现配方: {GetRecipeName(matchedRecipe)}");
            
            // 记录到炼金笔记
            if (!discoveredRecipes.Contains(matchedRecipe))
            {
                discoveredRecipes.Add(matchedRecipe);
                Debug.Log($"[AlchemySystem] 📖 配方已记录到炼金笔记");
            }
            
            // 大成功检查（成功率≥95%时有30%概率触发）
            float successRate = CalculateFreeAlchemySuccessRate(materials);
            if (successRate >= 0.95f && Random.Range(0f, 1f) <= 0.3f)
            {
                Debug.Log("[AlchemySystem] 🌟 大成功！效果+20%或数量+1");
                // 额外获得一个相同物品（如果是消耗品）
                if (!IsMaterialRecipe(matchedRecipe))
                {
                    pendingProduce.Add(matchedRecipe);
                    Debug.Log($"[AlchemySystem] 额外获得: {GetRecipeName(matchedRecipe)}");
                }
                else
                {
                    // 材料配方大成功额外获得材料
                    MaterialTypeEnum materialType = GetMaterialTypeFromRecipe(matchedRecipe);
                    if (materialType != MaterialTypeEnum.RottenWood)
                    {
                        pendingProduce.Add(materialType);
                        Debug.Log($"[AlchemySystem] 额外获得: {GetMaterialName(materialType)}");
                    }
                }
            }
            return;
        }
        
        // 次之：检查是否匹配材料合成（总价值匹配可合成材料）
        Debug.Log($"[AlchemySystem] 未匹配配方，检查材料合成，总价值: {totalValue}");
        MaterialTypeEnum targetMaterial = GetMaterialByValue(totalValue);
        Debug.Log($"[AlchemySystem] 材料合成目标: {GetMaterialName(targetMaterial)}");
        
        if (targetMaterial != MaterialTypeEnum.RottenWood)
        {
            pendingProduce.Add(targetMaterial);
            Debug.Log($"[AlchemySystem] ✅ 合成材料: {GetMaterialName(targetMaterial)}");
            
            // 大成功检查
            float successRate = CalculateFreeAlchemySuccessRate(materials);
            if (successRate >= 0.95f && Random.Range(0f, 1f) <= 0.3f)
            {
                pendingProduce.Add(targetMaterial);
                Debug.Log($"[AlchemySystem] 🌟 大成功！额外获得: {GetMaterialName(targetMaterial)}");
            }
            return;
        }
        
        // 失败：材料不匹配任何配方
        Debug.Log("[AlchemySystem] ❌ 材料组合无效，未能合成任何物品");
    }
    
    // 根据材料匹配配方
    public RecipeType MatchRecipeByMaterials(MaterialTypeEnum[] materials)
    {
        foreach (var recipe in availableRecipes)
        {
            if (recipe.requiredMaterials == null) continue;
            
            bool match = true;
            Dictionary<MaterialTypeEnum, int> tempMaterials = new Dictionary<MaterialTypeEnum, int>();
            
            // 统计投入材料
            foreach (var mat in materials)
            {
                if (!tempMaterials.ContainsKey(mat))
                {
                    tempMaterials[mat] = 0;
                }
                tempMaterials[mat]++;
            }
            
            Debug.Log($"[AlchemySystem] 检查配方: {recipe.name}, 需要材料: {string.Join(", ", recipe.requiredMaterials.Select(kv => kv.Key + "×" + kv.Value))}");
            Debug.Log($"[AlchemySystem] 投入材料: {string.Join(", ", tempMaterials.Select(kv => kv.Key + "×" + kv.Value))}");
            
            // 检查是否匹配配方所需材料
            foreach (var required in recipe.requiredMaterials)
            {
                if (!tempMaterials.ContainsKey(required.Key) || tempMaterials[required.Key] < required.Value)
                {
                    match = false;
                    Debug.Log($"[AlchemySystem] 配方 {recipe.name} 不匹配: 缺少 {required.Key} 或数量不足");
                    break;
                }
                tempMaterials[required.Key] -= required.Value;
            }
            
            // 检查是否有额外的材料（投入材料数量必须等于配方所需材料数量）
            int remainingCount = tempMaterials.Values.Sum();
            if (remainingCount > 0)
            {
                match = false;
                Debug.Log($"[AlchemySystem] 配方 {recipe.name} 不匹配: 有额外材料，剩余: {remainingCount}");
            }
            
            if (match)
            {
                Debug.Log($"[AlchemySystem] 配方 {recipe.name} 匹配成功!");
                return recipe.type;
            }
        }
        
        return RecipeType.None; // 返回None表示未匹配任何配方
    }
    
    // 根据价值获取随机材料
    private MaterialTypeEnum GetRandomMaterialByValue(int targetValue)
    {
        List<MaterialTypeEnum> materials = System.Enum.GetValues(typeof(MaterialTypeEnum))
            .Cast<MaterialTypeEnum>()
            .ToList();
        
        // 过滤出价值接近目标价值的材料
        var candidates = materials.Where(m => 
        {
            int value = GetMaterialValue(m);
            return value >= targetValue * 0.5f && value <= targetValue * 1.5f;
        }).ToList();
        
        if (candidates.Count == 0)
        {
            candidates = materials; // 如果没有匹配的，使用所有材料
        }
        
        return candidates[Random.Range(0, candidates.Count)];
    }
    
    // 自由炼金失败时返还材料（根据设计文档的价值匹配返还机制，支持返还多个基础材料组合）
    private void RefundMaterialsForFreeAlchemy(MaterialTypeEnum[] materials)
    {
        // 1. 计算投入总价值
        int totalValue = materials.Sum(m => GetMaterialValue(m));
        Debug.Log($"[AlchemySystem] 投入材料总价值: {totalValue}");
        
        // 2. 计算返还比例（新手保护）
        float refundRate = failedAttempts < NEWBIE_PROTECTION_COUNT 
            ? Random.Range(0.8f, 0.9f) // 新手保护：80%~90%
            : Random.Range(0.5f, 0.7f); // 正常：50%~70%
        
        // 3. 计算返还价值
        float refundValueFloat = totalValue * refundRate;
        int refundValue = Mathf.Max(1, Mathf.FloorToInt(refundValueFloat)); // 至少返还价值1
        Debug.Log($"[AlchemySystem] 返还比例: {refundRate * 100:F1}%，返还价值: {refundValue}");
        
        // 4. 获取基础材料列表（价值1-10）
        List<MaterialTypeEnum> basicMaterials = GetBasicMaterials();
        
        // 5. 按价值从低到高排序，优先使用低价值材料组合（更容易组合出目标价值）
        // 同时考虑玩家当前数量（数量少的优先）
        basicMaterials.Sort((a, b) => {
            int valueCompare = GetMaterialValue(a).CompareTo(GetMaterialValue(b));
            if (valueCompare != 0) return valueCompare;
            // 价值相同时，数量少的优先
            int countA = materialInventory.ContainsKey(a) ? materialInventory[a] : 0;
            int countB = materialInventory.ContainsKey(b) ? materialInventory[b] : 0;
            return countA.CompareTo(countB);
        });
        
        // 6. 计算返还材料（支持返还多个相同材料的组合）
        Dictionary<MaterialTypeEnum, int> refundMaterials = new Dictionary<MaterialTypeEnum, int>();
        int remainingValue = refundValue;
        
        // 使用贪心算法：从高价值到低价值尝试，尽可能多地返还材料
        for (int i = basicMaterials.Count - 1; i >= 0 && remainingValue > 0; i--)
        {
            var materialType = basicMaterials[i];
            int materialValue = GetMaterialValue(materialType);
            
            if (materialValue <= remainingValue)
            {
                // 计算可以返还的数量（最多返还3个相同材料，避免过于单一）
                int maxCount = Mathf.Min(remainingValue / materialValue, 3);
                if (maxCount > 0)
                {
                    // 随机决定实际返还数量（增加随机性）
                    int count = Random.Range(1, maxCount + 1);
                    refundMaterials[materialType] = count;
                    remainingValue -= materialValue * count;
                }
            }
        }
        
        // 7. 如果仍有剩余价值（<最小材料价值），赠送1个最低价值材料
        if (remainingValue > 0)
        {
            MaterialTypeEnum lowestMaterial = basicMaterials[0];
            if (refundMaterials.ContainsKey(lowestMaterial))
            {
                refundMaterials[lowestMaterial]++;
            }
            else
            {
                refundMaterials[lowestMaterial] = 1;
            }
        }
        
        // 8. 返还材料到库存
        foreach (var item in refundMaterials)
        {
            AddMaterial(item.Key, item.Value);
            Debug.Log($"[AlchemySystem] 返还材料: {GetMaterialName(item.Key)} × {item.Value}");
        }
        
        // 9. 输出返还总结
        int actualRefundValue = refundMaterials.Sum(item => GetMaterialValue(item.Key) * item.Value);
        Debug.Log($"[AlchemySystem] 实际返还总价值: {actualRefundValue}");
    }
    
    // 获取基础材料列表（价值1-10）
    private List<MaterialTypeEnum> GetBasicMaterials()
    {
        return new List<MaterialTypeEnum>
        {
            MaterialTypeEnum.RottenWood,     // 腐朽木屑(1)
            MaterialTypeEnum.BoneFragments,  // 碎骨(2)
            MaterialTypeEnum.StarlightGrass, // 星光草(3)
            MaterialTypeEnum.CorruptedTissue, // 腐化组织(4)
            MaterialTypeEnum.MoonlightFlower, // 月影花(5)
            MaterialTypeEnum.RustedParts,    // 锈蚀零件(6)
            MaterialTypeEnum.SoulDust,       // 灵魂微尘(7)
            MaterialTypeEnum.CloudyDew,      // 浑浊露珠(8)
            MaterialTypeEnum.PurifyingSalt,  // 净化盐晶(9)
            MaterialTypeEnum.WailingVine     // 哀嚎藤蔓(10)
        };
    }
    
    // 旧的返还方法（保留用于配方合成失败）
    private void RefundMaterialsForOldAlchemy(MaterialTypeEnum[] materials)
    {
        // 返还一半材料（向下取整）
        int refundCount = Mathf.CeilToInt(materials.Length * 0.5f);
        
        // 随机选择要返还的材料
        List<MaterialTypeEnum> shuffled = new List<MaterialTypeEnum>(materials);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        // 返还前refundCount个材料
        for (int i = 0; i < refundCount; i++)
        {
            AddMaterial(shuffled[i], 1);
            Debug.Log($"[AlchemySystem] 返还材料: {GetMaterialName(shuffled[i])}");
        }
    }
    
    public bool CraftRecipe(RecipeType recipeType)
    {
        Debug.Log($"[AlchemySystem] 开始 CraftRecipe: {recipeType}");
        if (!CanCraftRecipe(recipeType))
        {
            Debug.Log("[AlchemySystem] 材料不足，无法制作 " + GetRecipeName(recipeType));
            PlayAudioViaReflection("PlayAlchemyFail");
            return false;
        }

        RecipeData recipe = GetRecipeByType(recipeType);
        if (recipe == null) return false;

        // 计算合成成功率
        float successRate = CalculateSuccessRate(recipe);
        Debug.Log($"[AlchemySystem] 合成 {recipe.name} 成功率: {successRate * 100:F1}%");
        
        // 生成随机数判断是否成功
        float random = Random.Range(0f, 1f);
        
        if (random <= successRate)
        {
            // 合成成功
            bool isGreatSuccess = successRate >= 0.95f && Random.Range(0f, 1f) <= 0.3f; // 30%概率触发大成功
            
            // 消耗材料
            ConsumeMaterials(recipeType);
            
            // 根据配方类型应用效果或获得材料
            if (IsMaterialRecipe(recipeType))
            {
                // 材料合成 - 添加对应材料到库存
                MaterialTypeEnum materialType = GetMaterialTypeFromRecipe(recipeType);
                AddMaterial(materialType, 1);
                Debug.Log($"[AlchemySystem] 获得材料: {GetMaterialName(materialType)}");
            }
            else
            {
                // 物品合成 - 应用效果
                ApplyRecipeEffect(recipeType);
            }
            
            // 大成功效果
            if (isGreatSuccess)
            {
                Debug.Log($"[AlchemySystem] 🌟 大成功！{recipe.name} 效果+20%或数量+1");
                // 如果是材料合成，大成功额外获得一个材料
                if (IsMaterialRecipe(recipeType))
                {
                    MaterialTypeEnum materialType = GetMaterialTypeFromRecipe(recipeType);
                    AddMaterial(materialType, 1);
                    Debug.Log($"[AlchemySystem] 大成功额外获得: {GetMaterialName(materialType)}");
                }
            }
            
            // 如果是首次发现的配方，记录到炼金笔记
            if (!discoveredRecipes.Contains(recipeType))
            {
                discoveredRecipes.Add(recipeType);
                Debug.Log($"[AlchemySystem] 📖 发现新配方: {recipe.name}，已记录到炼金笔记");
            }
            
            Debug.Log($"[AlchemySystem] ✅ 成功制作 {recipe.name}!");
            PlayAudioViaReflection("PlayAlchemySuccess");
            ValidateAlchemyValue(recipe.alchemicalValue);
            
            return true;
        }
        else
        {
            // 合成失败
            failedAttempts++;
            
            // 返还材料
            RefundMaterials(recipe);
            
            // 物品合成失败时额外获得炼金残渣
            if (IsItemRecipe(recipeType))
            {
                AddMaterial(MaterialTypeEnum.MemoryResidue, 1); // 使用记忆残渣作为炼金残渣
                Debug.Log("[AlchemySystem] 获得炼金残渣（记忆残渣）×1");
            }
            
            Debug.Log($"[AlchemySystem] ❌ 合成 {recipe.name} 失败");
            PlayAudioViaReflection("PlayAlchemyFail");
            
            return false;
        }
    }
    
    // 计算合成成功率（物品合成）
    public float CalculateSuccessRate(RecipeData recipe)
    {
        float baseRate = recipeBaseSuccessRate; // 使用可配置的基础成功率
        
        // 智慧加成（如果有角色属性系统）
        if (characterStats != null)
        {
            // 假设characterStats有Wisdom属性
            // baseRate += characterStats.Wisdom * 0.01f;
        }
        
        // 负担惩罚
        if (burdenSystem != null)
        {
            baseRate -= burdenSystem.GetCurrentBurden() * 0.002f;
        }
        
        // 记忆碎片加成（如果携带任意记忆碎片）
        if (HasMemoryFragment())
        {
            baseRate += memoryShardBonus; // 使用可配置的加成值
        }
        
        // 材料合成的特殊计算
        if (IsMaterialRecipe(recipe.type))
        {
            baseRate = materialCraftSuccessRate; // 使用可配置的材料合成成功率
            baseRate -= (recipe.alchemicalValue - 10) * 0.02f;
        }
        
        // 新手保护
        if (failedAttempts < NEWBIE_PROTECTION_COUNT)
        {
            baseRate = Mathf.Lerp(baseRate, 0.9f, (NEWBIE_PROTECTION_COUNT - failedAttempts) / (float)NEWBIE_PROTECTION_COUNT);
        }
        
        // 限制成功率范围（使用可配置的范围）
        return Mathf.Clamp(baseRate, successRateMinimum, successRateMaximum);
    }
    
    // 检查是否携带记忆碎片
    private bool HasMemoryFragment()
    {
        // 检查玩家是否携带记忆碎片
        if (inventorySystem != null)
        {
            return inventorySystem.GetAllMemoryFragments().Count > 0;
        }
        return false;
    }
    
    // 判断是否是材料合成配方
    private bool IsMaterialRecipe(RecipeType type)
    {
        // 材料合成配方名称以 "Material_" 开头
        return type.ToString().StartsWith("Material_");
    }
    
    // 判断是否是物品配方
    private bool IsItemRecipe(RecipeType type)
    {
        return !IsMaterialRecipe(type);
    }
    
    // 根据配方类型获取对应的材料类型
    private MaterialTypeEnum GetMaterialTypeFromRecipe(RecipeType recipeType)
    {
        switch (recipeType)
        {
            case RecipeType.Material_MemoryResidue:
                return MaterialTypeEnum.MemoryResidue;
            case RecipeType.Material_TimeFragment:
                return MaterialTypeEnum.TimeFragment;
            case RecipeType.Material_SoulCrystal:
                return MaterialTypeEnum.SoulCrystal;
            case RecipeType.Material_CrystalizedCore:
                return MaterialTypeEnum.CrystalizedCore;
            case RecipeType.Material_MechCore:
                return MaterialTypeEnum.MechCore;
            case RecipeType.Material_AncientTreeResin:
                return MaterialTypeEnum.AncientTreeResin;
            case RecipeType.Material_LavaCore:
                return MaterialTypeEnum.LavaCore;
            case RecipeType.Material_GargoyleFragment:
                return MaterialTypeEnum.GargoyleFragment;
            case RecipeType.Material_FrostShard:
                return MaterialTypeEnum.FrostShard;
            case RecipeType.Material_AncientDragonBonePowder:
                return MaterialTypeEnum.AncientDragonBonePowder;
            case RecipeType.Material_LeylineCrystal:
                return MaterialTypeEnum.LeylineCrystal;
            case RecipeType.Material_AncientRuneStone:
                return MaterialTypeEnum.AncientRuneStone;
            case RecipeType.Material_SoulEssence:
                return MaterialTypeEnum.SoulEssence;
            default:
                return MaterialTypeEnum.RottenWood; // 默认返回基础材料
        }
    }
    
    // 从外部系统消耗材料（如InventorySystem）
    public void ConsumeMaterials(RecipeType recipeType)
    {
        RecipeData recipe = GetRecipeByType(recipeType);
        if (recipe == null) return;
        
        // 消耗材料
        foreach (var requiredMaterial in recipe.requiredMaterials)
        {
            MaterialTypeEnum materialType = requiredMaterial.Key;
            int requiredAmount = requiredMaterial.Value;
            if (materialInventory.ContainsKey(materialType))
            {
                materialInventory[materialType] = Mathf.Max(0, materialInventory[materialType] - requiredAmount);
            }
        }
    }
    
    // 材料返还机制（合成失败时返还材料）
    private void RefundMaterials(RecipeData recipe)
    {
        // 计算投入总价值
        int totalValue = CalculateTotalMaterialValue(recipe.requiredMaterials);
        Debug.Log($"[AlchemySystem] 投入材料总价值: {totalValue}");
        
        // 计算返还比例（新手保护）
        float refundRate = failedAttempts < NEWBIE_PROTECTION_COUNT 
            ? Random.Range(0.8f, 0.9f) // 新手保护：80%~90%
            : Random.Range(0.5f, 0.7f); // 正常：50%~70%
        
        // 计算返还价值
        float refundValueFloat = totalValue * refundRate;
        int refundValue = Mathf.FloorToInt(refundValueFloat);
        Debug.Log($"[AlchemySystem] 返还比例: {refundRate * 100:F1}%，返还价值: {refundValue}");
        
        // 获取基础材料列表（价值1-10）
        List<MaterialTypeEnum> basicMaterials = GetBasicMaterials();
        
        // 按价值从高到低排序，同时考虑玩家当前数量（数量少的优先）
        basicMaterials.Sort((a, b) => {
            int valueCompare = GetMaterialValue(b).CompareTo(GetMaterialValue(a));
            if (valueCompare != 0) return valueCompare;
            // 价值相同时，数量少的优先
            int countA = materialInventory.ContainsKey(a) ? materialInventory[a] : 0;
            int countB = materialInventory.ContainsKey(b) ? materialInventory[b] : 0;
            return countA.CompareTo(countB);
        });
        
        // 计算返还材料（每个材料最多返还1个）
        Dictionary<MaterialTypeEnum, int> refundMaterials = new Dictionary<MaterialTypeEnum, int>();
        int remainingValue = refundValue;
        
        foreach (var materialType in basicMaterials)
        {
            if (remainingValue <= 0) break;
            
            int materialValue = GetMaterialValue(materialType);
            if (materialValue <= remainingValue)
            {
                refundMaterials[materialType] = 1;
                remainingValue -= materialValue;
            }
        }
        
        // 如果仍有剩余价值（<1），赠送1个最低价值材料
        if (remainingValue > 0 && refundMaterials.Count > 0)
        {
            // 找到玩家数量最少的最低价值材料
            MaterialTypeEnum lowestMaterial = basicMaterials[basicMaterials.Count - 1];
            if (!refundMaterials.ContainsKey(lowestMaterial))
            {
                refundMaterials[lowestMaterial] = 1;
            }
        }
        
        // 返还材料到库存
        foreach (var item in refundMaterials)
        {
            AddMaterial(item.Key, item.Value);
            Debug.Log($"[AlchemySystem] 返还材料: {GetMaterialName(item.Key)} × {item.Value}");
        }
    }
    
    // 计算材料总价值
    private int CalculateTotalMaterialValue(Dictionary<MaterialTypeEnum, int> materials)
    {
        int totalValue = 0;
        foreach (var item in materials)
        {
            totalValue += GetMaterialValue(item.Key) * item.Value;
        }
        return totalValue;
    }
    
    public void ApplyRecipeEffect(RecipeType recipeType)
    {
        switch (recipeType)
        {
            case RecipeType.SoulStabilizer:
                if (burdenSystem != null)
                    burdenSystem.ChangeBurden(-30f);
                Debug.Log("使用灵魂稳定剂，负担减少30点。");
                break;
            case RecipeType.HealingPotion:
                if (playerHealth != null)
                    playerHealth.Heal(playerHealth.maxHealth * 0.2f);
                Debug.Log("使用治疗药剂，恢复20%生命值。");
                break;
            case RecipeType.SummonDurationPotion:
                ExtendSummonDurationByPercent(0.6f);
                Debug.Log("使用召唤持续时间药剂，召唤物持续时间+60%。");
                break;
            case RecipeType.BurdenReliefIncense:
                Debug.Log("使用负担缓解熏香，非战斗时每分钟额外恢复1点负担，持续10分钟。");
                break;
            case RecipeType.SummonEnhancer:
                Debug.Log("使用召唤强化剂，召唤物全属性+20%，持续10分钟。");
                break;
            case RecipeType.WeaknessInsightPotion:
                Debug.Log("使用弱点洞察药剂，显示当前区域敌人弱点，持续3场战斗。");
                break;
            case RecipeType.MemoryResonancePotion:
                Debug.Log("使用记忆共鸣药剂，临时激活第4个记忆碎片共鸣位。");
                break;
            case RecipeType.MechServantCore:
                Debug.Log("使用机械仆从核心，召唤一个机械仆从。");
                break;
            case RecipeType.EnvironmentStabilizer:
                Debug.Log("使用环境稳定卷轴，周围不稳定环境效果失效20秒。");
                break;
            case RecipeType.SyncEnhancerPotion:
                Debug.Log("使用同频强化药剂，30秒内召唤物获得玩家主武器元素附魔。");
                break;
            case RecipeType.RebirthAmulet:
                Debug.Log("获得重生护符，首次死亡时立即复活。");
                break;
            case RecipeType.TimeBeacon:
                Debug.Log("使用时空道标，在当前地图设置永久传送点。");
                break;
            case RecipeType.TruthSeerPotion:
                Debug.Log("使用真理窥视药剂，揭示隐藏通道、宝箱及剧情线索。");
                break;
            case RecipeType.BurdenPurification:
                if (burdenSystem != null)
                    burdenSystem.ResetBurden();
                Debug.Log("使用负担净化圣水，完全清除所有灵魂负担。");
                break;
        }
    }

    void ExtendSummonDurationByPercent(float percent)
    {
        SummonSystem summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem != null)
        {
            Debug.Log("召唤物持续时间增加 " + (percent * 100) + "%");
        }
    }

    void ValidateAlchemyValue(int value)
    {
        if (IsPrime(value))
        {
            Debug.Log("炼金价值 " + value + " 是素数，触发特殊效果！");
        }
        else
        {
            Debug.Log("炼金价值 " + value + " 不是素数。");
        }
    }

    bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        for (int i = 3; i * i <= number; i += 2)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }

    // 缓存配方字典，提高查找性能
    private Dictionary<RecipeType, RecipeData> recipeCache;
    
    // 初始化配方缓存
    private void InitializeRecipeCache()
    {
        recipeCache = new Dictionary<RecipeType, RecipeData>();
        foreach (RecipeData recipe in availableRecipes)
        {
            if (recipe != null)
            {
                recipeCache[recipe.type] = recipe;
            }
        }
    }
    
    RecipeData GetRecipeByType(RecipeType recipeType)
    {
        // 确保缓存已初始化
        if (recipeCache == null || recipeCache.Count == 0)
        {
            InitializeRecipeCache();
        }
        
        // 从缓存中查找配方
        if (recipeCache.TryGetValue(recipeType, out RecipeData cachedRecipe))
        {
            return cachedRecipe;
        }
        
        // 缓存未命中时，从列表中查找
        foreach (RecipeData recipe in availableRecipes)
        {
            if (recipe != null && recipe.type == recipeType)
            {
                // 更新缓存
                recipeCache[recipeType] = recipe;
                return recipe;
            }
        }
        
        return null;
    }

    public string GetMaterialName(MaterialTypeEnum type)
    {
        return GetMaterialData(type).name;
    }

    public string GetRecipeName(RecipeType type)
    {
        switch (type)
        {
            // 基础物品配方
            case RecipeType.SoulStabilizer: return "灵魂稳定剂";
            case RecipeType.HealingPotion: return "治疗药剂";
            case RecipeType.SummonDurationPotion: return "召唤持续时间药剂";
            case RecipeType.BurdenReliefIncense: return "负担缓解熏香";
            // 进阶物品配方
            case RecipeType.SummonEnhancer: return "召唤强化剂";
            case RecipeType.WeaknessInsightPotion: return "弱点洞察药剂";
            case RecipeType.MemoryResonancePotion: return "记忆共鸣药剂";
            case RecipeType.MechServantCore: return "机械仆从核心";
            case RecipeType.EnvironmentStabilizer: return "环境稳定卷轴";
            case RecipeType.SyncEnhancerPotion: return "同频强化药剂";
            case RecipeType.RebirthAmulet: return "重生护符";
            case RecipeType.TimeBeacon: return "时空道标";
            case RecipeType.TruthSeerPotion: return "真理窥视药剂";
            case RecipeType.BurdenPurification: return "负担净化圣水";
            // 材料合成配方
            case RecipeType.Material_MemoryResidue: return "记忆残渣";
            case RecipeType.Material_TimeFragment: return "时空碎片";
            case RecipeType.Material_SoulCrystal: return "灵魂结晶";
            case RecipeType.Material_CrystalizedCore: return "晶化残核";
            case RecipeType.Material_MechCore: return "机械核心";
            case RecipeType.Material_AncientTreeResin: return "古树树脂";
            case RecipeType.Material_LavaCore: return "熔岩核心";
            case RecipeType.Material_GargoyleFragment: return "石像鬼碎片";
            case RecipeType.Material_FrostShard: return "极寒冰屑";
            case RecipeType.Material_AncientDragonBonePowder: return "古龙骨粉";
            case RecipeType.Material_LeylineCrystal: return "地脉结晶";
            case RecipeType.Material_AncientRuneStone: return "远古铭文石";
            case RecipeType.Material_SoulEssence: return "灵魂精华";
            default: return type.ToString();
        }
    }

    public int GetMaterialCount(MaterialTypeEnum type)
    {
        if (materialInventory.ContainsKey(type))
            return materialInventory[type];
        return 0;
    }
    
    public int GetMaterialValue(MaterialTypeEnum type)
    {
        return GetMaterialData(type).value;
    }

    public RecipeData GetRecipeData(RecipeType type)
    {
        foreach (RecipeData recipe in availableRecipes)
        {
            if (recipe.type == type)
            {
                return recipe;
            }
        }
        return null;
    }
    
    public MaterialData GetMaterialData(MaterialTypeEnum type)
    {
        switch (type)
        {
            // 基础材料
            case MaterialTypeEnum.RottenWood:
                return new MaterialData(MaterialTypeEnum.RottenWood, "腐朽木屑", 1, "枯朽的木材碎片。", "常见", "森林地图采集");
            case MaterialTypeEnum.BoneFragments:
                return new MaterialData(MaterialTypeEnum.BoneFragments, "碎骨", 2, "断裂的骨骼。", "常见", "骷髅战士、腐化村民掉落");
            case MaterialTypeEnum.StarlightGrass:
                return new MaterialData(MaterialTypeEnum.StarlightGrass, "星光草", 3, "夜晚发光的植物。", "常见", "森林地图采集");
            case MaterialTypeEnum.CorruptedTissue:
                return new MaterialData(MaterialTypeEnum.CorruptedTissue, "腐化组织", 4, "被幻宫能量侵蚀的肉块。", "常见", "腐化村民、沼泽潜伏者掉落");
            case MaterialTypeEnum.MoonlightFlower:
                return new MaterialData(MaterialTypeEnum.MoonlightFlower, "月影花", 5, "满月夜绽放的银色花朵。", "常见", "月光下的森林采集");
            case MaterialTypeEnum.RustedParts:
                return new MaterialData(MaterialTypeEnum.RustedParts, "锈蚀零件", 6, "废弃机械的金属零件。", "常见", "机械残骸掉落，废墟地图采集");
            case MaterialTypeEnum.SoulDust:
                return new MaterialData(MaterialTypeEnum.SoulDust, "灵魂微尘", 7, "逸散的灵魂能量微粒。", "常见", "怨魂等灵魂类敌人掉落");
            case MaterialTypeEnum.CloudyDew:
                return new MaterialData(MaterialTypeEnum.CloudyDew, "浑浊露珠", 8, "凝结时空能量的水珠。", "常见", "森林、荒原、湿地、冰原采集");
            case MaterialTypeEnum.PurifyingSalt:
                return new MaterialData(MaterialTypeEnum.PurifyingSalt, "净化盐晶", 9, "蕴含净化能量的盐结晶。", "常见", "岩地、沙漠采集，结晶蜥蜴、石像鬼掉落");
            case MaterialTypeEnum.WailingVine:
                return new MaterialData(MaterialTypeEnum.WailingVine, "哀嚎藤蔓", 10, "触碰时会发出悲鸣的黑暗植物。", "常见", "沼泽潜伏者掉落，湿地地图采集");
            
            // 稀有材料
            case MaterialTypeEnum.MemoryResidue:
                return new MaterialData(MaterialTypeEnum.MemoryResidue, "记忆残渣", 11, "记忆逸散的能量渣滓。", "稀有", "记忆碎片区域采集，怨魂掉落（低概率）");
            case MaterialTypeEnum.TimeFragment:
                return new MaterialData(MaterialTypeEnum.TimeFragment, "时空碎片", 12, "包含不稳定时空能量的脆弱碎片。", "稀有", "时空守护者掉落，时空裂隙区域采集");
            case MaterialTypeEnum.SoulCrystal:
                return new MaterialData(MaterialTypeEnum.SoulCrystal, "灵魂结晶", 13, "提纯凝固的灵魂能量块。", "稀有", "灵魂吞噬者掉落");
            case MaterialTypeEnum.CrystalizedCore:
                return new MaterialData(MaterialTypeEnum.CrystalizedCore, "晶化残核", 14, "晶化生物遗留的惰性能量核。", "稀有", "结晶蜥蜴掉落");
            case MaterialTypeEnum.MechCore:
                return new MaterialData(MaterialTypeEnum.MechCore, "机械核心", 15, "古代机械的微型动力核心。", "稀有", "机械构造体掉落");
            case MaterialTypeEnum.AncientTreeResin:
                return new MaterialData(MaterialTypeEnum.AncientTreeResin, "古树树脂", 16, "古老树木渗出的生命力树脂。", "稀有", "森林地图特定古树采集");
            case MaterialTypeEnum.LavaCore:
                return new MaterialData(MaterialTypeEnum.LavaCore, "熔岩核心", 17, "一小块永不熄灭的炽热能量核。", "稀有", "熔岩元素掉落");
            case MaterialTypeEnum.GargoyleFragment:
                return new MaterialData(MaterialTypeEnum.GargoyleFragment, "石像鬼碎片", 18, "活化雕像上剥落的带魔力石片。", "稀有", "石像鬼掉落");
            case MaterialTypeEnum.FrostShard:
                return new MaterialData(MaterialTypeEnum.FrostShard, "极寒冰屑", 19, "永恒冻土中形成的刺骨冰屑。", "稀有", "冰原采集，冰原狼掉落");
            case MaterialTypeEnum.AncientDragonBonePowder:
                return new MaterialData(MaterialTypeEnum.AncientDragonBonePowder, "古龙骨粉", 20, "研磨自远古巨兽骨骼的能量粉末。", "稀有", "岩地、冰原远古遗骸处采集");
            
            // 史诗材料
            case MaterialTypeEnum.SoulEssence:
                return new MaterialData(MaterialTypeEnum.SoulEssence, "灵魂精华", 1, "用于炼金的高纯度灵魂能量。", "史诗", "灵魂炼金（灵魂×50）");
            case MaterialTypeEnum.LeylineCrystal:
                return new MaterialData(MaterialTypeEnum.LeylineCrystal, "地脉结晶", 30, "大地能量脉络中凝结的坚固结晶。", "史诗", "岩地深处采集");
            case MaterialTypeEnum.AncientRuneStone:
                return new MaterialData(MaterialTypeEnum.AncientRuneStone, "远古铭文石", 33, "刻有失落知识的石碑碎片。", "史诗", "神殿、废墟解谜区域采集");
            
            // 传奇材料
            case MaterialTypeEnum.ParadoxShard:
                return new MaterialData(MaterialTypeEnum.ParadoxShard, "悖时薄片", 40, "凝固的、记录时间悖论的时空片段。", "传奇", "时空守护者掉落");
            
            default:
                return new MaterialData(type, type.ToString(), 0, "未知材料", "未知", "未知");
        }
    }

    public void DisplayInventory()
    {
        Debug.Log("=== 材料库存 ===");
        foreach (var item in materialInventory)
        {
            if (item.Value > 0)
            {
                Debug.Log(GetMaterialName(item.Key) + ": " + item.Value);
            }
        }
    }

    public List<RecipeType> GetAvailableRecipes()
    {
        List<RecipeType> available = new List<RecipeType>();

        foreach (RecipeData recipe in availableRecipes)
        {
            if (CanCraftRecipe(recipe.type))
            {
                available.Add(recipe.type);
            }
        }

        return available;
    }

    private void PlayAudioViaReflection(string methodName)
    {
        try
        {
            var audioManagerType = System.Reflection.Assembly.GetExecutingAssembly().GetType("AudioManager");
            if (audioManagerType != null)
            {
                var instanceProperty = audioManagerType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var audioManagerInstance = instanceProperty.GetValue(null);
                    if (audioManagerInstance != null)
                    {
                        var method = audioManagerType.GetMethod(methodName);
                        if (method != null)
                        {
                            method.Invoke(audioManagerInstance, null);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"无法播放音效 {methodName}: " + e.Message);
        }
    }
    
    // 用于更新炼金成功率加成
    public void UpdateSuccessRateBonus(float bonus)
    {
        // 这里可以实现更新炼金成功率加成的逻辑
        Debug.Log($"炼金成功率加成更新为: {bonus}");
    }
    
    // 获取待收集的产出
    public List<object> GetPendingProduce()
    {
        return new List<object>(pendingProduce);
    }
    
    // 清空待收集产出
    public void ClearPendingProduce()
    {
        pendingProduce.Clear();
    }
    
    // 收集待收集产出到背包
    public void CollectPendingProduce()
    {
        foreach (var item in pendingProduce)
        {
            if (item is MaterialTypeEnum materialType)
            {
                AddMaterial(materialType, 1);
                Debug.Log($"[AlchemySystem] 收集产出: {GetMaterialName(materialType)}");
            }
            else if (item is RecipeType recipeType && !IsMaterialRecipe(recipeType))
            {
                AddPotionToInventory(recipeType, 1);
                Debug.Log($"[AlchemySystem] 收集产出: {GetRecipeName(recipeType)}");
            }
        }
        pendingProduce.Clear();
    }
}
