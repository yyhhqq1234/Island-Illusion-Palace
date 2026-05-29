using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameSystems
{
    public enum MaterialTypeEnum
    {
        // 基础材料
        RottenWood,       // 腐朽木屑
        BoneFragments,    // 碎骨
        StarlightGrass,   // 星光草
        CorruptedTissue,  // 腐化组织
        MoonlightFlower,  // 月影花
        RustedParts,      // 锈蚀零件
        SoulDust,         // 灵魂微尘
        CloudyDew,        // 浑浊露珠
        PurifyingSalt,    // 净化盐晶
        WailingVine,      // 哀嚎藤蔓
        
        // 稀有材料
        MemoryResidue,    // 记忆残渣
        TimeFragment,     // 时空碎片
        SoulCrystal,      // 灵魂结晶
        CrystalizedCore,  // 晶化残核
        MechCore,         // 机械核心
        AncientTreeResin, // 古树树脂
        LavaCore,         // 熔岩核心
        GargoyleFragment, // 石像鬼碎片
        FrostShard,       // 极寒冰屑
        AncientDragonBonePowder, // 古龙骨粉
        
        // 史诗材料
        SoulEssence,      // 灵魂精华
        LeylineCrystal,   // 地脉结晶
        AncientRuneStone, // 远古铭文石
        
        // 传奇材料
        ParadoxShard      // 悖时薄片
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
}

public class MaterialItem : MonoBehaviour, ICollectible
{
    [Header("材料属性")]
    public GameSystems.MaterialTypeEnum materialType;
    
    [Header("动态属性")]
    [Tooltip("材料名称，如果为空则使用默认名称")]
    public string customName = "";
    
    [Tooltip("材料价值，如果为0则使用默认价值")]
    public int customValue = 0;
    
    [Tooltip("材料描述，如果为空则使用默认描述")]
    public string customDescription = "";
    
    [Tooltip("材料稀有度，如果为空则使用默认稀有度")]
    public string customRarity = "";
    
    [Tooltip("材料获取方法，如果为空则使用默认获取方法")]
    public string customObtainMethod = "";

    [Header("拾取设置")]
    public float collectRange = 2f; // 收集范围

    [Header("视觉效果")]
    public float hoverHeight = 0.1f;
    public float hoverSpeed = 1f;
    public Material outlineMaterial; // 边缘高亮材质
    public float outlineScale = 1.2f; // 轮廓缩放比例
    public float flashSpeed = 2f; // 闪烁速度

    private Vector3 initialPosition;
    private float hoverOffset;
    private float flashOffset;
    private SpriteRenderer originalRenderer;
    private Material originalMaterial;
    private GameObject outlineObject; // 轮廓对象
    private SpriteRenderer outlineRenderer; // 轮廓渲染器

    void Start()
    {
        initialPosition = transform.position;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);
        flashOffset = Random.Range(0f, Mathf.PI * 2f);

        // 添加碰撞触发器
        CircleCollider2D collider = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = collectRange;
        gameObject.tag = "Collectible";

        // 保存原始材质
        originalRenderer = GetComponent<SpriteRenderer>();
        if (originalRenderer != null)
        {
            originalMaterial = originalRenderer.material;
        }

        // 设置默认高光材料
        if (outlineMaterial == null)
        {
            string materialPath = "Assets/ArtMaterials/Effects/HighLight Material.mat";
            #if UNITY_EDITOR
            outlineMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            #endif
        }

        // 添加边缘高亮效果
        AddOutlineEffect();
    }

    void Update()
    {
        // 悬浮效果
        float hoverY = Mathf.Sin(Time.time * hoverSpeed + hoverOffset) * hoverHeight;
        transform.position = new Vector3(initialPosition.x, initialPosition.y + hoverY, initialPosition.z);

        // 闪烁效果
        if (outlineRenderer != null)
        {
            float flashAlpha = Mathf.Sin(Time.time * flashSpeed + flashOffset) * 0.5f + 0.5f; // 0到1之间的闪烁值
            Color color = outlineRenderer.color;
            color.a = flashAlpha;
            outlineRenderer.color = color;
        }
    }

    void OnEnable()
    {
        // 添加边缘高亮效果
        AddOutlineEffect();
    }

    private void AddOutlineEffect()
    {
        // 检查是否有原始渲染器
        if (originalRenderer == null)
        {
            originalRenderer = GetComponent<SpriteRenderer>();
            if (originalRenderer != null)
            {
                originalMaterial = originalRenderer.material;
            }
        }
        
        // 检查是否已经创建了轮廓对象
        if (outlineObject == null && originalRenderer != null)
        {
            // 创建一个子对象用于显示轮廓
            outlineObject = new GameObject("MaterialOutline");
            outlineObject.transform.parent = transform;
            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localScale = new Vector3(outlineScale, outlineScale, 1f); // 稍微放大以显示轮廓
            
            // 添加SpriteRenderer组件
            outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = originalRenderer.sprite;
            
            // 使用高亮材质
            if (outlineMaterial != null)
            {
                outlineRenderer.material = outlineMaterial;
            }
            
            // 确保轮廓在原始精灵下方
            outlineRenderer.sortingLayerID = originalRenderer.sortingLayerID;
            outlineRenderer.sortingOrder = originalRenderer.sortingOrder - 1;
        }
        else if (outlineObject != null)
        {
            // 确保轮廓对象始终激活
            outlineObject.SetActive(true);
            
            // 确保outlineRenderer引用正确
            if (outlineRenderer == null)
            {
                outlineRenderer = outlineObject.GetComponent<SpriteRenderer>();
            }
        }
    }

    public GameSystems.MaterialTypeEnum GetMaterialType()
    {
        return materialType;
    }

    public string GetMaterialName()
    {
        return string.IsNullOrEmpty(customName) ? GetDefaultMaterialData(materialType).name : customName;
    }

    public int GetMaterialValue()
    {
        return customValue > 0 ? customValue : GetDefaultMaterialData(materialType).value;
    }

    public string GetMaterialDescription()
    {
        return string.IsNullOrEmpty(customDescription) ? GetDefaultMaterialData(materialType).description : customDescription;
    }

    public string GetMaterialRarity()
    {
        return string.IsNullOrEmpty(customRarity) ? GetDefaultMaterialData(materialType).rarity : customRarity;
    }

    public string GetMaterialObtainMethod()
    {
        return string.IsNullOrEmpty(customObtainMethod) ? GetDefaultMaterialData(materialType).obtainMethod : customObtainMethod;
    }

    // ICollectible接口实现
    public string GetItemName()
    {
        return GetMaterialName();
    }

    public void OnCollected()
    {
        // 将材料添加到背包系统
            InventorySystem inventorySystem = FindObjectOfType<InventorySystem>();
            if (inventorySystem != null)
            {
                inventorySystem.AddMaterial(materialType, 1);
                Debug.Log($"添加材料到背包：{GetMaterialName()}");
            }
            else
            {
                // 备用：如果背包系统不存在，直接添加到炼金系统
                AlchemySystem alchemySystem = FindObjectOfType<AlchemySystem>();
                if (alchemySystem != null)
                {
                    alchemySystem.AddMaterial(materialType, 1);
                    Debug.Log($"添加材料到炼金系统：{GetMaterialName()}");
                }
            }

        Debug.Log($"材料被收集：{GetMaterialName()}");
        Destroy(gameObject);
    }

    private GameSystems.MaterialData GetDefaultMaterialData(GameSystems.MaterialTypeEnum type)
    {
        switch (type)
        {
            // 基础材料
            case GameSystems.MaterialTypeEnum.RottenWood:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.RottenWood, "腐朽木屑", 1, "枯朽的木材碎片。", "常见", "森林地图采集");
            case GameSystems.MaterialTypeEnum.BoneFragments:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.BoneFragments, "碎骨", 2, "断裂的骨骼。", "常见", "骷髅战士、腐化村民掉落");
            case GameSystems.MaterialTypeEnum.StarlightGrass:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.StarlightGrass, "星光草", 3, "夜晚发光的植物。", "常见", "森林地图采集");
            case GameSystems.MaterialTypeEnum.CorruptedTissue:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.CorruptedTissue, "腐化组织", 4, "被幻宫能量侵蚀的肉块。", "常见", "腐化村民、沼泽潜伏者掉落");
            case GameSystems.MaterialTypeEnum.MoonlightFlower:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.MoonlightFlower, "月影花", 5, "满月夜绽放的银色花朵。", "常见", "月光下的森林采集");
            case GameSystems.MaterialTypeEnum.RustedParts:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.RustedParts, "锈蚀零件", 6, "废弃机械的金属零件。", "常见", "机械残骸掉落，废墟地图采集");
            case GameSystems.MaterialTypeEnum.SoulDust:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.SoulDust, "灵魂微尘", 7, "逸散的灵魂能量微粒。", "常见", "怨魂等灵魂类敌人掉落");
            case GameSystems.MaterialTypeEnum.CloudyDew:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.CloudyDew, "浑浊露珠", 8, "凝结时空能量的水珠。", "常见", "森林、荒原、湿地、冰原采集");
            case GameSystems.MaterialTypeEnum.PurifyingSalt:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.PurifyingSalt, "净化盐晶", 9, "蕴含净化能量的盐结晶。", "常见", "岩地、沙漠采集，结晶蜥蜴、石像鬼掉落");
            case GameSystems.MaterialTypeEnum.WailingVine:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.WailingVine, "哀嚎藤蔓", 10, "触碰时会发出悲鸣的黑暗植物。", "常见", "沼泽潜伏者掉落，湿地地图采集");
            
            // 稀有材料
            case GameSystems.MaterialTypeEnum.MemoryResidue:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.MemoryResidue, "记忆残渣", 11, "记忆逸散的能量渣滓。", "稀有", "记忆碎片区域采集，怨魂掉落（低概率）");
            case GameSystems.MaterialTypeEnum.TimeFragment:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.TimeFragment, "时空碎片", 12, "包含不稳定时空能量的脆弱碎片。", "稀有", "时空守护者掉落，时空裂隙区域采集");
            case GameSystems.MaterialTypeEnum.SoulCrystal:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.SoulCrystal, "灵魂结晶", 13, "提纯凝固的灵魂能量块。", "稀有", "灵魂吞噬者掉落");
            case GameSystems.MaterialTypeEnum.CrystalizedCore:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.CrystalizedCore, "晶化残核", 14, "晶化生物遗留的惰性能量核。", "稀有", "结晶蜥蜴掉落");
            case GameSystems.MaterialTypeEnum.MechCore:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.MechCore, "机械核心", 15, "古代机械的微型动力核心。", "稀有", "机械构造体掉落");
            case GameSystems.MaterialTypeEnum.AncientTreeResin:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.AncientTreeResin, "古树树脂", 16, "古老树木渗出的生命力树脂。", "稀有", "森林地图特定古树采集");
            case GameSystems.MaterialTypeEnum.LavaCore:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.LavaCore, "熔岩核心", 17, "一小块永不熄灭的炽热能量核。", "稀有", "熔岩元素掉落");
            case GameSystems.MaterialTypeEnum.GargoyleFragment:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.GargoyleFragment, "石像鬼碎片", 18, "活化雕像上剥落的带魔力石片。", "稀有", "石像鬼掉落");
            case GameSystems.MaterialTypeEnum.FrostShard:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.FrostShard, "极寒冰屑", 19, "永恒冻土中形成的刺骨冰屑。", "稀有", "冰原采集，冰原狼掉落");
            case GameSystems.MaterialTypeEnum.AncientDragonBonePowder:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.AncientDragonBonePowder, "古龙骨粉", 20, "研磨自远古巨兽骨骼的能量粉末。", "稀有", "岩地、冰原远古遗骸处采集");
            
            // 史诗材料
            case GameSystems.MaterialTypeEnum.SoulEssence:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.SoulEssence, "灵魂精华", 1, "用于炼金的高纯度灵魂能量。", "史诗", "灵魂炼金（灵魂×50）");
            case GameSystems.MaterialTypeEnum.LeylineCrystal:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.LeylineCrystal, "地脉结晶", 30, "大地能量脉络中凝结的坚固结晶。", "史诗", "岩地深处采集");
            case GameSystems.MaterialTypeEnum.AncientRuneStone:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.AncientRuneStone, "远古铭文石", 33, "刻有失落知识的石碑碎片。", "史诗", "神殿、废墟解谜区域采集");
            
            // 传奇材料
            case GameSystems.MaterialTypeEnum.ParadoxShard:
                return new GameSystems.MaterialData(GameSystems.MaterialTypeEnum.ParadoxShard, "悖时薄片", 40, "凝固的、记录时间悖论的时空片段。", "传奇", "时空守护者掉落");
            
            default:
                return new GameSystems.MaterialData(type, type.ToString(), 0, "未知材料", "未知", "未知");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ItemCollector collector = collision.GetComponent<ItemCollector>();
            if (collector != null)
            {
                collector.SetNearbyItem(this);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ItemCollector collector = collision.GetComponent<ItemCollector>();
            if (collector != null)
            {
                collector.ClearNearbyItem(this);
            }
        }
    }

    // 绘制收集范围（仅在编辑器中可见）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}