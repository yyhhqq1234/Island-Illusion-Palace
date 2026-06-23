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
        return MaterialDatabase.GetMaterialData(type);
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