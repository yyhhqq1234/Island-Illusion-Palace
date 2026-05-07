using UnityEngine;
using UnityEngine.UI;
using GameSystems;

public class InventoryTest : MonoBehaviour
{
    [Header("背包系统引用")]
    public InventorySystem inventorySystem;

    [Header("设置")]
    public KeyCode testKey = KeyCode.B;           // 测试按键
    public int addQuantity = 1;                    // 每次添加的数量
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
    }

    void Update()
    {
        // 检测是否按下测试键（B键）
        if (Input.GetKeyDown(testKey))
        {
            AddAllBasicMaterials();
        }
        
        // 按住Shift+B键可以一次性添加大量素材用于测试
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(testKey))
        {
            AddLargeAmountOfMaterials();
        }
    }

    void AddAllBasicMaterials()
    {
        if (inventorySystem == null)
        {
            Debug.LogError("未找到InventorySystem，请检查引用");
            return;
        }
        
        Debug.Log("========== 测试：添加基础炼金素材 ==========");
        
        // 添加所有基础炼金材料（10种）
        AddMaterial(MaterialTypeEnum.RottenWood, addQuantity);
        AddMaterial(MaterialTypeEnum.BoneFragments, addQuantity);
        AddMaterial(MaterialTypeEnum.StarlightGrass, addQuantity);
        AddMaterial(MaterialTypeEnum.CorruptedTissue, addQuantity);
        AddMaterial(MaterialTypeEnum.MoonlightFlower, addQuantity);
        AddMaterial(MaterialTypeEnum.RustedParts, addQuantity);
        AddMaterial(MaterialTypeEnum.SoulDust, addQuantity);
        AddMaterial(MaterialTypeEnum.CloudyDew, addQuantity);
        AddMaterial(MaterialTypeEnum.PurifyingSalt, addQuantity);
        AddMaterial(MaterialTypeEnum.WailingVine, addQuantity);

        Debug.Log("========== 基础素材添加完成 ==========");
        Debug.Log($"当前背包总物品数: {inventorySystem.currentInventorySize}");
        
        // 自动刷新背包UI
        RefreshInventoryUI();
    }

    void AddMaterial(MaterialTypeEnum materialType, int quantity)
    {
        bool success = inventorySystem.AddMaterial(materialType, quantity);
        if (success)
        {
            string materialName = GetMaterialName(materialType);
            int currentQty = inventorySystem.GetMaterialQuantity(materialType);
            Debug.Log($"✓ 添加 {materialName} +{quantity} (当前: {currentQty})");
        }
        else
        {
            string materialName = GetMaterialName(materialType);
            Debug.LogWarning($"✗ 添加 {materialName} 失败（可能已满）");
        }
    }

    void AddLargeAmountOfMaterials()
    {
        if (inventorySystem == null)
        {
            Debug.LogError("未找到InventorySystem，请检查引用");
            return;
        }
        
        Debug.Log("========== 测试：批量添加炼金素材（Shift+B）=========");
        
        int largeQuantity = 10; // 批量添加10个
        
        AddMaterial(MaterialTypeEnum.SoulDust, largeQuantity);
        AddMaterial(MaterialTypeEnum.PurifyingSalt, largeQuantity);
        AddMaterial(MaterialTypeEnum.CloudyDew, largeQuantity);
        AddMaterial(MaterialTypeEnum.StarlightGrass, largeQuantity);
        
        // 批量添加药剂
        inventorySystem.AddPotion(RecipeType.HealingPotion, 5);
        inventorySystem.AddPotion(RecipeType.SoulStabilizer, 3);
        
        Debug.Log("========== 批量素材添加完成 ==========");
        
        // 自动刷新背包UI
        RefreshInventoryUI();
    }

    string GetMaterialName(MaterialTypeEnum type)
    {
        switch (type)
        {
            // 基础材料
            case MaterialTypeEnum.RottenWood:
                return "腐朽木屑";
            case MaterialTypeEnum.BoneFragments:
                return "碎骨";
            case MaterialTypeEnum.StarlightGrass:
                return "星光草";
            case MaterialTypeEnum.CorruptedTissue:
                return "腐化组织";
            case MaterialTypeEnum.MoonlightFlower:
                return "月影花";
            case MaterialTypeEnum.RustedParts:
                return "锈蚀零件";
            case MaterialTypeEnum.SoulDust:
                return "灵魂微尘";
            case MaterialTypeEnum.CloudyDew:
                return "浑浊露珠";
            case MaterialTypeEnum.PurifyingSalt:
                return "净化盐晶";
            case MaterialTypeEnum.WailingVine:
                return "哀嚎藤蔓";

            // 稀有材料
            case MaterialTypeEnum.MemoryResidue:
                return "记忆残渣";
            case MaterialTypeEnum.TimeFragment:
                return "时空碎片";
            case MaterialTypeEnum.SoulCrystal:
                return "灵魂结晶";
            case MaterialTypeEnum.CrystalizedCore:
                return "晶化残核";
            case MaterialTypeEnum.MechCore:
                return "机械核心";
            case MaterialTypeEnum.AncientTreeResin:
                return "古树树脂";
            case MaterialTypeEnum.LavaCore:
                return "熔岩核心";
            case MaterialTypeEnum.GargoyleFragment:
                return "石像鬼碎片";
            case MaterialTypeEnum.FrostShard:
                return "极寒冰屑";
            case MaterialTypeEnum.AncientDragonBonePowder:
                return "古龙骨粉";

            // 史诗材料
            case MaterialTypeEnum.SoulEssence:
                return "灵魂精华";
            case MaterialTypeEnum.LeylineCrystal:
                return "地脉结晶";
            case MaterialTypeEnum.AncientRuneStone:
                return "远古铭文石";

            default:
                return type.ToString();
        }
    }
    
    void RefreshInventoryUI()
    {
        // 查找 InventoryUI 组件并刷新界面
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI();
            Debug.Log("✅ 背包UI已自动刷新");
        }
        else
        {
            Debug.LogWarning("⚠️ 未找到InventoryUI组件，无法自动刷新背包");
        }
    }
}
