using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GameSystems;

public class AlchemyUI : MonoBehaviour
{
    [Header("系统引用")]
    public AlchemySystem alchemySystem; // 炼金系统的引用
    public CauldronSystem cauldronSystem; // 炼药锅系统的引用
    public InventorySystem inventorySystem; // 背包系统的引用
    
    [Header("UI元素")]
    public GameObject alchemyPanel;         // 炼金主面板
    public GameObject itemSlotPrefab;       // 物品格子预制体（共用）
    public GameObject itemTooltip;          // 物品提示框
    public GameObject itemInfoPanel;        // 物品信息显示面板
    
    [Header("面板布局")]
    public GameObject consumablesPanel;     // 消耗品/药剂栏面板
    public GameObject materialsPanel;       // 材料栏面板
    
    [Header("炼金操作面板")]
    public GameObject[] prepareAlchemyBaskets; // 炼金材料准备篮（4个）
    public GameObject alchemyProducePanel;     // 炼金产出面板
    
    [Header("产出配置")]
    public Vector2 produceCellSize = new Vector2(100, 96); // 产出格子大小
    public Vector2 produceSpacing = new Vector2(8, 8);     // 产出格子间距
    
    [Header("网格布局")]
    public GridLayoutGroup consumablesGrid; // 消耗品栏网格布局
    public GridLayoutGroup materialsGrid;   // 材料栏网格布局
    public GridLayoutGroup[] basketGrids;   // 准备篮网格布局（4个）
    public GridLayoutGroup produceGrid;     // 产出面板网格布局
    
    [Header("材料栏格子配置")]
    public Vector2 materialCellSize = new Vector2(100, 96); // 材料栏格子大小
    public Vector2 materialSpacing = new Vector2(16, 16);     // 材料栏格子间距
    
    [Header("消耗品栏格子配置")]
    public Vector2 consumableCellSize = new Vector2(88, 80); // 消耗品栏格子大小
    public Vector2 consumableSpacing = new Vector2(16, 16);     // 消耗品栏格子间距
    
    [Header("准备篮格子配置")]
    public Vector2 basketCellSize = new Vector2(100, 96);    // 准备篮格子大小
    public Vector2 basketSpacing = new Vector2(8, 8);         // 准备篮格子间距
    
    [Header("当前状态")]
    private List<GameObject> consumableSlots = new List<GameObject>();
    private List<GameObject> materialSlots = new List<GameObject>();
    private List<GameObject>[] basketSlots = new List<GameObject>[4]; // 每个准备篮的格子列表
    private List<GameObject> produceSlots = new List<GameObject>(); // 产出面板的格子列表
    
    // 产出存储
    private List<object> produceItems = new List<object>(); // 存储产出的物品（可以是MaterialTypeEnum或RecipeType）
    private RecipeType selectedRecipe = RecipeType.SoulStabilizer;
    private MaterialTypeEnum selectedMaterial = MaterialTypeEnum.RottenWood;
    private int currentBasketIndex = 0; // 当前选中的材料准备篮索引

    // 自由炼金相关
    private MaterialTypeEnum[] basketMaterials = new MaterialTypeEnum[4]; // 每个准备篮中当前的材料
    private int[] basketMaterialCounts = new int[4]; // 每个准备篮中材料的数量（支持堆叠）
    private bool[] basketIsEmpty = new bool[4]; // 标记每个篮子是否为空（解决RottenWood既是材料又是空标记的问题）
    private float lastClickTime = 0f; // 上次点击时间（用于检测双击）
    private MaterialTypeEnum lastClickedMaterial = MaterialTypeEnum.RottenWood; // 上次点击的材料
    private bool hasValidLastClick = false; // 标记是否有有效的上次点击（解决RottenWood初始值问题）
    
    [Header("提示框防抖设置")]
    public float tooltipHideDelay = 0.08f;              // 隐藏延迟（秒）- 防止子物体切换时闪烁
    private UnityEngine.Coroutine hideTooltipCoroutine = null;       // 隐藏提示框的协程
    private bool isTooltipVisible = false;               // 提示框当前可见性状态
    private object currentHoveredItem = null;     // 当前悬停的物品
    
    [Header("炼药锅交互")]
    private bool playerInCauldronRange = false; // 玩家是否在炼药锅范围内
    private bool isAlchemyPanelOpen = false;    // 炼金面板是否打开
    
    void Start()
    {
        if (alchemySystem == null)
        {
            alchemySystem = FindObjectOfType<AlchemySystem>();
        }
        
        if (cauldronSystem == null)
        {
            cauldronSystem = FindObjectOfType<CauldronSystem>();
        }
        
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        // 检查itemTooltip引用
        if (itemTooltip == null)
        {
            Debug.LogError("[AlchemyUI] ❌ itemTooltip 未在Inspector中设置！");
        }
        else
        {
            Debug.Log("[AlchemyUI] ✅ itemTooltip 引用已设置: " + itemTooltip.name);
        }
        
        // 检查tooltipHideDelay值
        Debug.Log("[AlchemyUI] ⏱️ tooltipHideDelay: " + tooltipHideDelay + "秒");
        
        // 初始化准备篮数组
        for (int i = 0; i < 4; i++)
        {
            basketSlots[i] = new List<GameObject>();
            basketMaterials[i] = MaterialTypeEnum.RottenWood; // 初始化材料值
            basketMaterialCounts[i] = 0; // 初始化材料数量
            basketIsEmpty[i] = true; // 标记篮子为空
        }
        
        InitializeGridSettings();
        UpdateAlchemyUI();
        HideAlchemyPanel();
        
        // 添加炼金面板点击事件，点击空白区域时隐藏物品信息面板
        SetupAlchemyPanelClickEvent();
    }
    
    void Update()
    {
        // 处理E键切换炼金面板
        if (playerInCauldronRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isAlchemyPanelOpen)
            {
                HideAlchemyPanel();
            }
            else
            {
                ShowAlchemyPanel();
            }
        }
        
        // 处理ESC键关闭炼金面板
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isAlchemyPanelOpen)
            {
                HideAlchemyPanel();
            }
        }
        
        // 处理Enter键进行自由炼金（准备篮中有材料时）
        if (isAlchemyPanelOpen && Input.GetKeyDown(KeyCode.Return))
        {
            // 检查准备篮中是否有材料
            bool hasMaterialsInBasket = false;
            foreach (bool isEmpty in basketIsEmpty)
            {
                if (!isEmpty)
                {
                    hasMaterialsInBasket = true;
                    break;
                }
            }
            
            if (hasMaterialsInBasket)
            {
                CraftSelectedRecipe();
            }
        }
    }
    
    void InitializeGridSettings()
    {
        // 配置消耗品网格布局
        if (consumablesGrid != null)
        {
            consumablesGrid.cellSize = consumableCellSize;
            consumablesGrid.spacing = consumableSpacing;
            consumablesGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            consumablesGrid.constraintCount = 2; // 每行2列，形成八行两列布局
        }
        
        // 配置材料网格布局
        if (materialsGrid != null)
        {
            materialsGrid.cellSize = materialCellSize;
            materialsGrid.spacing = materialSpacing;
            materialsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            materialsGrid.constraintCount = 3; // 每行3列，形成六行三列布局
        }
        
        // 配置准备篮网格布局（每个篮子只有1个格子）
        if (basketGrids != null)
        {
            for (int i = 0; i < basketGrids.Length; i++)
            {
                if (basketGrids[i] != null)
                {
                    basketGrids[i].cellSize = basketCellSize;
                    basketGrids[i].spacing = basketSpacing;
                    basketGrids[i].constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    basketGrids[i].constraintCount = 1; // 每行1列，每个篮子只显示1个格子
                }
            }
        }
        
        // 配置产出网格布局
        if (produceGrid != null)
        {
            produceGrid.cellSize = produceCellSize;
            produceGrid.spacing = produceSpacing;
            produceGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            produceGrid.constraintCount = 2; // 每行2列，形成2x2布局
        }
    }

    public void ShowAlchemyPanel()
    {
        if (alchemyPanel != null)
        {
            // 从背包系统同步材料到炼金系统
            SyncMaterialsFromInventory();
            
            alchemyPanel.SetActive(true);
            UpdateAlchemyUI();
            Time.timeScale = 0f; // 暂停游戏
            isAlchemyPanelOpen = true;
        }
    }
    
    // 从背包系统同步材料到炼金系统
    private void SyncMaterialsFromInventory()
    {
        if (inventorySystem != null && alchemySystem != null)
        {
            // 清空炼金系统的材料库存
            foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
            {
                alchemySystem.materialInventory[materialType] = 0;
            }
            
            // 从背包系统同步材料
            var materials = inventorySystem.GetAllMaterials();
            foreach (var item in materials)
            {
                if (item.itemType == ItemType.Material)
                {
                    alchemySystem.materialInventory[item.materialType] = item.quantity;
                }
            }
        }
    }
    
    // 将炼金系统的材料变化同步回背包系统
    // 注意：只同步炼金系统向背包的材料（添加），不处理背包向炼金系统的消耗
    // 材料消耗已在CraftWithMaterials中处理
    private void SyncMaterialsToInventory()
    {
        if (inventorySystem != null && alchemySystem != null)
        {
            foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
            {
                // 只处理炼金系统有额外材料的情况（返还、获得）
                int alchemyCount = alchemySystem.materialInventory.ContainsKey(materialType) 
                    ? alchemySystem.materialInventory[materialType] : 0;
                
                if (alchemyCount > 0)
                {
                    int inventoryCount = inventorySystem.GetMaterialQuantity(materialType);
                    
                    if (alchemyCount > inventoryCount)
                    {
                        int difference = alchemyCount - inventoryCount;
                        inventorySystem.AddMaterial(materialType, difference);
                        Debug.Log($"[AlchemyUI] 材料返还: {materialType} × {difference}");
                    }
                }
                // 不再从背包移除多余材料，因为消耗已在CraftWithMaterials中处理
            }
        }
    }
    
    public void HideAlchemyPanel()
    {
        if (alchemyPanel != null)
        {
            // 自动收集产出物品到背包
            CollectProduceItems();
            
            alchemyPanel.SetActive(false);
            Time.timeScale = 1f; // 恢复游戏
            isAlchemyPanelOpen = false;
            
            // 清空产出
            ClearProduceItems();
            
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
    
    public void UpdateAlchemyUI()
    {
        // 清除现有格子
        ClearAllSlots();
        
        // 显示消耗品和材料
        DisplayConsumables();
        DisplayMaterials();
        
        // 显示准备篮内容
        UpdateBasketDisplay();
        
        // 显示产出面板
        UpdateProduceDisplay();
    }
    
    void DisplayConsumables()
    {
        if (consumablesPanel == null || alchemySystem == null) return;
        
        // 显示所有可用的药剂配方
        foreach (RecipeData recipe in alchemySystem.availableRecipes)
        {
            if (IsPotionRecipe(recipe.type))
            {
                CreateConsumableSlot(recipe);
            }
        }
    }
    
    void DisplayMaterials()
    {
        if (materialsPanel == null || alchemySystem == null || inventorySystem == null) return;
        
        foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
        {
            int count = inventorySystem.GetMaterialQuantity(materialType);
            if (count > 0)
            {
                CreateMaterialSlot(materialType, count);
            }
        }
    }
    
    void CreateConsumableSlot(RecipeData recipe)
    {
        if (itemSlotPrefab == null || consumablesPanel == null) return;
        
        GameObject slot = Instantiate(itemSlotPrefab, consumablesPanel.transform);
        consumableSlots.Add(slot);
        
        // 创建InventoryItem对象
        InventoryItem inventoryItem = new InventoryItem(
            recipe.name,
            ItemType.Consumable,
            0, // 消耗品在炼金界面中显示为配方，不显示数量
            recipe.resultDescription,
            recipe.alchemicalValue,
            ItemRarity.Common // 可以根据配方等级设置稀有度
        );
        inventoryItem.potionType = recipe.type;
        
        // 设置物品到ItemSlot
        ItemSlot itemSlot = slot.GetComponent<ItemSlot>();
        if (itemSlot != null)
        {
            itemSlot.SetItem(inventoryItem);
        }
        
        // 添加点击事件
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            // 确保Button组件的targetGraphic是父物体的Image，而不是子物体
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotButton.targetGraphic == null)
            {
                slotButton.targetGraphic = slotImage;
            }
            
            slotButton.onClick.AddListener(() => OnConsumableClick(recipe.type));
        }
        
        // 添加鼠标悬停事件
        UnityEngine.EventSystems.EventTrigger trigger = slot.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 清除现有的事件
        trigger.triggers.Clear();
        
        // 确保父物体可以接收射线检测（用于边界判断）
        UnityEngine.CanvasGroup parentCanvasGroup = slot.GetComponent<UnityEngine.CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = slot.gameObject.AddComponent<UnityEngine.CanvasGroup>();
        }
        parentCanvasGroup.blocksRaycasts = true;   // 父物体必须接收射线
        parentCanvasGroup.alpha = 1f;
        
        // 确保父物体有碰撞器
        UnityEngine.BoxCollider2D boxCollider = slot.GetComponent<UnityEngine.BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = slot.gameObject.AddComponent<UnityEngine.BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new UnityEngine.Vector2(120, 120); // 与格子大小匹配
        }
        
        // 确保父物体有 Image 组件（用于射线检测）
        Image parentImage = slot.GetComponent<Image>();
        if (parentImage == null)
        {
            parentImage = slot.gameObject.AddComponent<Image>();
            parentImage.color = new UnityEngine.Color(0, 0, 0, 0); // 透明，不影响视觉
        }
        
        // 添加鼠标进入事件
        UnityEngine.EventSystems.EventTrigger.Entry enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => 
        {
            OnItemHover(recipe);
        });
        trigger.triggers.Add(enterEntry);
        
        // 添加鼠标离开事件
        UnityEngine.EventSystems.EventTrigger.Entry exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => 
        {
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitEntry);
    }
    
    void CreateMaterialSlot(MaterialTypeEnum materialType, int count)
    {
        if (itemSlotPrefab == null || materialsPanel == null) return;
        
        GameObject slot = Instantiate(itemSlotPrefab, materialsPanel.transform);
        materialSlots.Add(slot);
        
        // 获取材料数据
        MaterialData materialData = alchemySystem.GetMaterialData(materialType);
        
        // 创建InventoryItem对象
        InventoryItem inventoryItem = new InventoryItem(
            materialData.name,
            ItemType.Material,
            count,
            materialData.description,
            materialData.value,
            ItemRarity.Common // 可以根据材料稀有度设置
        );
        inventoryItem.materialType = materialType;
        
        // 设置物品到ItemSlot
        ItemSlot itemSlot = slot.GetComponent<ItemSlot>();
        if (itemSlot != null)
        {
            itemSlot.SetItem(inventoryItem);
        }
        
        // 添加点击事件
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            // 确保Button组件的targetGraphic是父物体的Image，而不是子物体
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotButton.targetGraphic == null)
            {
                slotButton.targetGraphic = slotImage;
            }
            
            slotButton.onClick.AddListener(() => OnMaterialClick(materialType));
        }
        
        // 添加鼠标悬停事件
        UnityEngine.EventSystems.EventTrigger trigger = slot.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 清除现有的事件
        trigger.triggers.Clear();
        
        // 确保父物体可以接收射线检测（用于边界判断）
        UnityEngine.CanvasGroup parentCanvasGroup = slot.GetComponent<UnityEngine.CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = slot.gameObject.AddComponent<UnityEngine.CanvasGroup>();
        }
        parentCanvasGroup.blocksRaycasts = true;   // 父物体必须接收射线
        parentCanvasGroup.alpha = 1f;
        
        // 确保父物体有碰撞器
        UnityEngine.BoxCollider2D boxCollider = slot.GetComponent<UnityEngine.BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = slot.gameObject.AddComponent<UnityEngine.BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new UnityEngine.Vector2(120, 120); // 与格子大小匹配
        }
        
        // 确保父物体有 Image 组件（用于射线检测）
        Image parentImage = slot.GetComponent<Image>();
        if (parentImage == null)
        {
            parentImage = slot.gameObject.AddComponent<Image>();
            parentImage.color = new UnityEngine.Color(0, 0, 0, 0); // 透明，不影响视觉
        }
        
        // 添加鼠标进入事件
        UnityEngine.EventSystems.EventTrigger.Entry enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => 
        {
            OnItemHover(materialType);
        });
        trigger.triggers.Add(enterEntry);
        
        // 添加鼠标离开事件
        UnityEngine.EventSystems.EventTrigger.Entry exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => 
        {
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitEntry);
    }
    
    void ClearAllSlots()
    {
        ClearSlotList(consumableSlots);
        ClearSlotList(materialSlots);
        ClearSlotList(produceSlots);
        // 清理所有准备篮的格子
        for (int i = 0; i < 4; i++)
        {
            ClearSlotList(basketSlots[i]);
        }
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
    
    void OnConsumableClick(RecipeType recipeType)
    {
        selectedRecipe = recipeType;
        // 直接执行制作操作（不再显示材料到准备篮和产出预览）
        CraftSelectedRecipe();
    }
    
    void OnMaterialClick(MaterialTypeEnum materialType)
    {
        selectedMaterial = materialType;
        ShowItemInfo(materialType);
        
        // 检测双击（两次点击间隔在0.3秒内）
        float currentTime = Time.time;
        if (hasValidLastClick && currentTime - lastClickTime < 0.3f && lastClickedMaterial == materialType)
        {
            // 双击事件：将材料添加到准备篮
            AddMaterialToBasket(materialType);
            lastClickedMaterial = MaterialTypeEnum.RottenWood; // 重置
            hasValidLastClick = false; // 重置有效点击标记
        }
        else
        {
            lastClickedMaterial = materialType;
            lastClickTime = currentTime;
            hasValidLastClick = true; // 标记有有效的上次点击
        }
    }
    
    // 将材料添加到准备篮（支持堆叠）
    void AddMaterialToBasket(MaterialTypeEnum materialType)
    {
        // 检查背包中材料是否足够
        int inventoryCount = inventorySystem.GetMaterialQuantity(materialType);
        if (inventoryCount <= 0)
        {
            Debug.Log("[AlchemyUI] 材料不足，无法添加到准备篮");
            return;
        }
        
        // 1. 优先查找相同材料的篮子进行堆叠（最多堆叠99个）
        for (int i = 0; i < basketMaterials.Length; i++)
        {
            if (!basketIsEmpty[i] && basketMaterials[i] == materialType && basketMaterialCounts[i] < 99)
            {
                // 从背包移除1个材料
                inventorySystem.RemoveMaterial(materialType, 1);
                
                // 在篮子中堆叠
                basketMaterialCounts[i]++;
                UpdateAlchemyUI();
                Debug.Log($"[AlchemyUI] 材料 {materialType} 已堆叠到准备篮 {i+1}，当前数量: {basketMaterialCounts[i]}");
                return;
            }
        }
        
        // 2. 如果没有相同材料的篮子，找第一个空篮子
        for (int i = 0; i < basketMaterials.Length; i++)
        {
            if (basketIsEmpty[i]) // 检查篮子是否为空
            {
                // 从背包移除1个材料
                inventorySystem.RemoveMaterial(materialType, 1);
                
                // 将材料放入空篮子
                basketMaterials[i] = materialType;
                basketMaterialCounts[i] = 1;
                basketIsEmpty[i] = false; // 标记篮子不为空
                UpdateAlchemyUI();
                Debug.Log($"[AlchemyUI] 材料 {materialType} 已添加到准备篮 {i+1}");
                return;
            }
        }
        
        Debug.Log("[AlchemyUI] 准备篮已满，无法添加更多材料");
    }
    
    public void CraftSelectedRecipe()
    {
        if (alchemySystem != null)
        {
            // 检查是否处于自由炼金模式（准备篮中有材料）
            bool isFreeAlchemy = false;
            List<MaterialTypeEnum> basketMats = new List<MaterialTypeEnum>();
            for (int i = 0; i < basketMaterials.Length; i++)
            {
                if (!basketIsEmpty[i])
                {
                    // 按堆叠数量添加材料（支持多个相同材料）
                    for (int j = 0; j < basketMaterialCounts[i]; j++)
                    {
                        basketMats.Add(basketMaterials[i]);
                    }
                    isFreeAlchemy = true;
                }
            }
            
            bool success = false;
            
            if (isFreeAlchemy)
            {
                // 自由炼金模式：先把背包中的材料同步到炼金系统
                if (alchemySystem != null && inventorySystem != null)
                {
                    foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
                    {
                        int count = inventorySystem.GetMaterialQuantity(materialType);
                        alchemySystem.SetMaterialCount(materialType, count);
                    }
                }
                
                // 使用准备篮中的材料进行合成
                success = alchemySystem.CraftWithMaterials(basketMats.ToArray());
                
                // 无论成功或失败，都同步材料变化到背包系统
                SyncMaterialsToInventory();
                
                if (success)
                {
                    // 清空准备篮
                    for (int i = 0; i < basketMaterials.Length; i++)
                    {
                        basketMaterials[i] = MaterialTypeEnum.RottenWood;
                        basketMaterialCounts[i] = 0;
                        basketIsEmpty[i] = true;
                    }
                    Debug.Log("[AlchemyUI] 自由炼金成功！");
                }
                else
                {
                    // 失败时也清空准备篮（材料已被消耗/返还）
                    for (int i = 0; i < basketMaterials.Length; i++)
                    {
                        basketMaterials[i] = MaterialTypeEnum.RottenWood;
                        basketMaterialCounts[i] = 0;
                        basketIsEmpty[i] = true;
                    }
                    Debug.Log("[AlchemyUI] 自由炼金失败，材料已部分返还");
                }
                
                // 获取产出物品
                produceItems = alchemySystem.GetPendingProduce();
                
                // 刷新UI显示（实时更新准备篮和材料面板）
                UpdateAlchemyUI();
            }
            else
            {
                Debug.Log($"[AlchemyUI] 开始配方炼金: {selectedRecipe}");
                
                // 配方炼金模式：先把背包中的材料同步到炼金系统
                if (alchemySystem != null && inventorySystem != null)
                {
                    Debug.Log("[AlchemyUI] 正在同步背包材料到炼金系统...");
                    foreach (MaterialTypeEnum materialType in System.Enum.GetValues(typeof(MaterialTypeEnum)))
                    {
                        int count = inventorySystem.GetMaterialQuantity(materialType);
                        if (count > 0)
                        {
                            Debug.Log($"[AlchemyUI] 设置 {materialType}: {count} 个");
                        }
                        alchemySystem.SetMaterialCount(materialType, count);
                    }
                }
                
                Debug.Log("[AlchemyUI] 调用 alchemySystem.CraftRecipe...");
                success = alchemySystem.CraftRecipe(selectedRecipe);
                Debug.Log($"[AlchemyUI] 配方炼金结果: {success}");
                
                // 无论成功或失败，都同步材料变化到背包系统
                SyncMaterialsToInventory();
                
                if (success)
                {
                    // 对于药剂类配方，添加到背包
                    if (IsPotionRecipe(selectedRecipe) && inventorySystem != null)
                    {
                        inventorySystem.AddPotion(selectedRecipe, 1);
                    }
                    
                    Debug.Log("[AlchemyUI] 配方炼金成功！");
                }
                else
                {
                    Debug.Log("[AlchemyUI] 配方炼金失败，材料已部分返还");
                }
                
                // 清空准备篮状态
                for (int i = 0; i < basketMaterials.Length; i++)
                {
                    basketMaterials[i] = MaterialTypeEnum.RottenWood;
                    basketIsEmpty[i] = true;
                }
                
                UpdateAlchemyUI();
                HideItemInfo();
            }
        }
    }
    
    // 检查是否是药剂配方
    private bool IsPotionRecipe(RecipeType recipeType)
    {
        switch (recipeType)
        {
            case RecipeType.HealingPotion:
            case RecipeType.SoulStabilizer:
            case RecipeType.SummonDurationPotion:
            case RecipeType.BurdenReliefIncense:
            case RecipeType.SummonEnhancer:
            case RecipeType.WeaknessInsightPotion:
            case RecipeType.MemoryResonancePotion:
            case RecipeType.BurdenPurification:
                return true;
            default:
                return false;
        }
    }
    
    void ShowItemInfo(RecipeType recipeType)
    {
        if (itemInfoPanel == null || alchemySystem == null || inventorySystem == null) return;
        
        itemInfoPanel.SetActive(true);
        
        // 显示消耗品详细信息
        Text[] infoTexts = itemInfoPanel.GetComponentsInChildren<Text>();
        if (infoTexts.Length > 0)
        {
            infoTexts[0].text = alchemySystem.GetRecipeName(recipeType); // 名称
            
            if (infoTexts.Length > 1)
            {
                RecipeData recipe = alchemySystem.availableRecipes.Find(r => r.type == recipeType);
                if (recipe != null)
                {
                    infoTexts[1].text = recipe.resultDescription; // 描述
                    
                    if (infoTexts.Length > 2)
                    {
                        // 显示所需材料
                        string materialsText = "所需材料:\n";
                        foreach (var material in recipe.requiredMaterials)
                        {
                            string materialName = alchemySystem.GetMaterialName(material.Key);
                            int requiredAmount = material.Value;
                            int currentAmount = inventorySystem.GetMaterialQuantity(material.Key);
                            materialsText += $"{materialName}: {currentAmount}/{requiredAmount}\n";
                        }
                        infoTexts[2].text = materialsText;
                        
                        if (infoTexts.Length > 3)
                        {
                            // 显示是否可以制作
                            bool canCraft = alchemySystem.CanCraftRecipe(recipeType);
                            infoTexts[3].text = canCraft ? "可以制作" : "材料不足";
                            infoTexts[3].color = canCraft ? Color.green : Color.red;
                        }
                    }
                }
            }
        }
    }
    
    void ShowItemInfo(MaterialTypeEnum materialType)
    {
        if (itemInfoPanel == null || alchemySystem == null || inventorySystem == null) return;
        
        itemInfoPanel.SetActive(true);
        
        // 显示材料详细信息
        Text[] infoTexts = itemInfoPanel.GetComponentsInChildren<Text>();
        if (infoTexts.Length > 0)
        {
            MaterialData materialData = alchemySystem.GetMaterialData(materialType);
            infoTexts[0].text = materialData.name; // 名称
            
            if (infoTexts.Length > 1)
            {
                infoTexts[1].text = materialData.description; // 描述
                
                if (infoTexts.Length > 2)
                {
                    infoTexts[2].text = $"价值: {materialData.value}\n获取: {materialData.obtainMethod}"; // 价值和获取方式
                    
                    if (infoTexts.Length > 3)
                    {
                        int count = inventorySystem.GetMaterialQuantity(materialType);
                        infoTexts[3].text = $"数量: {count}"; // 数量
                    }
                }
            }
        }
    }
    
    void HideTooltipImmediate()
    {
        if (itemTooltip != null && itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(false);
        }
    }
    
    void HideItemInfo()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(false);
        }
    }
    
    // 设置玩家是否在炼药锅范围内
    public void SetPlayerInCauldronRange(bool inRange)
    {
        playerInCauldronRange = inRange;
    }
    
    // 检查炼金面板是否打开
    public bool IsAlchemyPanelOpen()
    {
        return isAlchemyPanelOpen;
    }
    
    void OnItemHover(RecipeData recipe)
    {
        // 1. 取消正在进行的隐藏协程（防止从子物体移入时闪烁）
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        // 2. 如果已经显示的是同一个物品，不需要重新显示
        if (currentHoveredItem is RecipeData && (currentHoveredItem as RecipeData) == recipe && isTooltipVisible)
        {
            return;
        }
        
        // 3. 立即显示提示框（无延迟）
        currentHoveredItem = recipe;
        ShowTooltipImmediate(recipe);
    }
    
    void OnItemHover(MaterialTypeEnum materialType)
    {
        // 1. 取消正在进行的隐藏协程（防止从子物体移入时闪烁）
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        // 2. 如果已经显示的是同一个物品，不需要重新显示
        if (currentHoveredItem is MaterialTypeEnum && (MaterialTypeEnum)currentHoveredItem == materialType && isTooltipVisible)
        {
            return;
        }
        
        // 3. 立即显示提示框（无延迟）
        currentHoveredItem = materialType;
        ShowTooltipImmediate(materialType);
    }
    
    void OnItemHoverExit()
    {
        // 4. 启动延迟隐藏协程（防止子物体切换时闪烁）
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
        }
        hideTooltipCoroutine = StartCoroutine(DelayedHideTooltip());
    }
    
    System.Collections.IEnumerator DelayedHideTooltip()
    {
        // 等待一小段延迟，避免子物体切换导致的闪烁
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < tooltipHideDelay)
        {
            yield return null;
        }
        
        // 隐藏提示框
        HideTooltipImmediate();
        hideTooltipCoroutine = null;
        currentHoveredItem = null;
    }
    
    void ShowTooltipImmediate(RecipeData recipe)
    {
        if (itemTooltip == null) return;
        
        // 避免重复设置active状态导致频闪
        if (!itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(true);
            isTooltipVisible = true;
        }
        
        // 设置提示内容
        Text[] tooltipTexts = itemTooltip.GetComponentsInChildren<Text>();
        if (tooltipTexts.Length > 0)
        {
            // 第1个Text：物品名称
            tooltipTexts[0].text = recipe.name;
            
            if (tooltipTexts.Length > 1)
            {
                // 第2个Text：炼金价值
                tooltipTexts[1].text = $"炼金价值：{recipe.alchemicalValue}";
                
                if (tooltipTexts.Length > 2)
                {
                    // 第3个Text：稀有度 + 描述
                    string rarityStr = GetRecipeTierString(recipe.tier);
                    string description = recipe.resultDescription;
                    tooltipTexts[2].text = $"{rarityStr} {description}";
                }
            }
        }
        
        // 设置固定提示位置
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
    
    void ShowTooltipImmediate(MaterialTypeEnum materialType)
    {
        if (itemTooltip == null) return;
        
        // 避免重复设置active状态导致频闪
        if (!itemTooltip.activeSelf)
        {
            itemTooltip.SetActive(true);
            isTooltipVisible = true;
        }
        
        // 获取材料数据
        MaterialData materialData = alchemySystem.GetMaterialData(materialType);
        
        // 设置提示内容
        Text[] tooltipTexts = itemTooltip.GetComponentsInChildren<Text>();
        if (tooltipTexts.Length > 0)
        {
            // 第1个Text：物品名称
            tooltipTexts[0].text = materialData.name;
            
            if (tooltipTexts.Length > 1)
            {
                // 第2个Text：炼金价值
                tooltipTexts[1].text = $"炼金价值：{materialData.value}";
                
                if (tooltipTexts.Length > 2)
                {
                    // 第3个Text：稀有度 + 描述
                    string rarityStr = materialData.rarity;
                    string description = materialData.description;
                    tooltipTexts[2].text = $"{rarityStr} {description}";
                }
            }
        }
        
        // 设置固定提示位置
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
    
    string GetRarityString(string rarity)
    {
        switch (rarity)
        {
            case "Common":
                return "普通";
            case "Uncommon":
                return "精良";
            case "Rare":
                return "史诗";
            case "Legendary":
                return "传奇";
            default:
                return "未知";
        }
    }
    
    string GetRecipeTierString(RecipeTier tier)
    {
        switch (tier)
        {
            case RecipeTier.Basic:
                return "基础";
            case RecipeTier.Advanced:
                return "高级";
            case RecipeTier.Legendary:
                return "传奇";
            default:
                return "未知";
        }
    }
    
    void SetupAlchemyPanelClickEvent()
    {
        if (alchemyPanel == null)
        {
            Debug.LogWarning("[AlchemyUI] ❌ alchemyPanel 未在Inspector中设置，无法添加点击事件");
            return;
        }
        
        // 确保炼金面板有Image组件（用于射线检测）
        Image panelImage = alchemyPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = alchemyPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0); // 透明，不影响视觉
        }
        
        // 确保炼金面板有CanvasGroup组件
        UnityEngine.CanvasGroup canvasGroup = alchemyPanel.GetComponent<UnityEngine.CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = alchemyPanel.AddComponent<UnityEngine.CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = true; // 确保能够接收射线检测
        
        // 添加EventTrigger组件
        UnityEngine.EventSystems.EventTrigger trigger = alchemyPanel.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = alchemyPanel.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 添加点击事件
        UnityEngine.EventSystems.EventTrigger.Entry clickEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        clickEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((eventData) => 
        {
            UnityEngine.EventSystems.PointerEventData pointerEventData = eventData as UnityEngine.EventSystems.PointerEventData;
            if (pointerEventData != null)
            {
                // 检查点击的对象是否是炼金面板本身
                if (pointerEventData.pointerPress == alchemyPanel)
                {
                    HideItemInfo();
                }
            }
        });
        trigger.triggers.Add(clickEntry);
    }
    
    // ==================== 准备篮相关方法 ====================
    
    // 更新准备篮显示
    void UpdateBasketDisplay()
    {
        if (prepareAlchemyBaskets == null) return;
        
        for (int i = 0; i < prepareAlchemyBaskets.Length; i++)
        {
            GameObject basket = prepareAlchemyBaskets[i];
            if (basket != null)
            {
                // 清空当前篮子的格子
                ClearSlotList(basketSlots[i]);
                
                // 如果准备篮中有材料，则显示该材料（自由炼金模式）
                if (!basketIsEmpty[i])
                {
                    CreateBasketSlot(basket, basketMaterials[i], basketMaterialCounts[i], i);
                }
            }
        }
    }
    
    // 将配方材料显示到指定篮子
    void DisplayRecipeMaterialsToBasket(int basketIndex)
    {
        if (prepareAlchemyBaskets == null || basketIndex >= prepareAlchemyBaskets.Length) return;
        
        GameObject basket = prepareAlchemyBaskets[basketIndex];
        if (basket == null || alchemySystem == null) return;
        
        // 获取当前配方的材料需求
        RecipeData recipe = alchemySystem.availableRecipes.Find(r => r.type == selectedRecipe);
        if (recipe == null) return;
        
        // 将材料分配到各个篮子（每个篮子最多显示一种材料）
        int matIndex = 0;
        foreach (var material in recipe.requiredMaterials)
        {
            if (matIndex == basketIndex)
            {
                CreateBasketSlot(basket, material.Key, material.Value, basketIndex);
                break;
            }
            matIndex++;
        }
    }
    
    // 在准备篮中创建材料格子
    void CreateBasketSlot(GameObject basket, MaterialTypeEnum materialType, int amount, int basketIndex)
    {
        if (itemSlotPrefab == null || basket == null) return;
        
        GameObject slot = Instantiate(itemSlotPrefab, basket.transform);
        basketSlots[basketIndex].Add(slot);
        
        // 获取材料数据
        MaterialData materialData = alchemySystem.GetMaterialData(materialType);
        
        // 创建InventoryItem对象
        InventoryItem inventoryItem = new InventoryItem(
            materialData.name,
            ItemType.Material,
            amount,
            materialData.description,
            materialData.value,
            ItemRarity.Common
        );
        inventoryItem.materialType = materialType;
        
        // 设置物品到ItemSlot
        ItemSlot itemSlot = slot.GetComponent<ItemSlot>();
        if (itemSlot != null)
        {
            itemSlot.SetItem(inventoryItem);
        }
        
        // 添加点击事件（从篮子移除材料）
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(() => RemoveMaterialFromBasket(basketIndex, materialType));
        }
    }
    
    // 从准备篮移除材料（点击准备篮中的材料时调用，支持堆叠）
    void RemoveMaterialFromBasket(int basketIndex, MaterialTypeEnum materialType)
    {
        if (basketIndex >= 0 && basketIndex < basketMaterials.Length)
        {
            // 只有当点击的材料与篮子中的材料匹配时才移除
            if (!basketIsEmpty[basketIndex] && basketMaterials[basketIndex] == materialType)
            {
                // 将1个材料返回背包
                inventorySystem.AddMaterial(materialType, 1);
                
                // 减少篮子中的材料数量
                basketMaterialCounts[basketIndex]--;
                
                // 如果数量减到0，清空篮子
                if (basketMaterialCounts[basketIndex] <= 0)
                {
                    basketMaterials[basketIndex] = MaterialTypeEnum.RottenWood;
                    basketIsEmpty[basketIndex] = true;
                    Debug.Log($"[AlchemyUI] 材料 {materialType} 已全部从准备篮 {basketIndex+1} 返回材料面板");
                }
                else
                {
                    Debug.Log($"[AlchemyUI] 材料 {materialType} 已从准备篮 {basketIndex+1} 返回1个，剩余: {basketMaterialCounts[basketIndex]}");
                }
                
                UpdateAlchemyUI();
            }
        }
    }
    
    // 设置当前选中的准备篮
    public void SetCurrentBasket(int index)
    {
        if (index >= 0 && index < 4)
        {
            currentBasketIndex = index;
            Debug.Log($"[AlchemyUI] 当前选中篮子: {index}");
        }
    }
    
    // ==================== 产出面板相关方法 ====================
    
    // 更新产出面板显示
    void UpdateProduceDisplay()
    {
        if (produceGrid == null || alchemyProducePanel == null) return;
        
        // 清空现有格子
        ClearSlotList(produceSlots);
        
        // 如果没有产出，直接返回
        if (produceItems == null || produceItems.Count == 0) return;
        
        // 创建产出格子
        for (int i = 0; i < produceItems.Count; i++)
        {
            object item = produceItems[i];
            if (item == null) continue;
            
            // 创建格子
            GameObject slot = Instantiate(itemSlotPrefab, produceGrid.transform);
            slot.name = $"ProduceSlot_{i}";
            produceSlots.Add(slot);
            
            int itemIndex = i; // 捕获循环变量
            
            // 配置格子显示
            if (item is MaterialTypeEnum materialType)
            {
                CreateProduceMaterialSlot(slot, materialType, itemIndex);
            }
            else if (item is RecipeType recipeType)
            {
                CreateProduceRecipeSlot(slot, recipeType, itemIndex);
            }
        }
    }
    
    // 创建产出材料格子
    void CreateProduceMaterialSlot(GameObject slot, MaterialTypeEnum materialType, int itemIndex)
    {
        if (alchemySystem == null) return;
        
        // 获取材料数据
        MaterialData materialData = alchemySystem.GetMaterialData(materialType);
        
        // 创建InventoryItem对象
        InventoryItem inventoryItem = new InventoryItem(
            materialData.name,
            ItemType.Material,
            1,
            materialData.description,
            materialData.value,
            ItemRarity.Common
        );
        inventoryItem.materialType = materialType;
        
        // 设置物品到ItemSlot
        ItemSlot itemSlot = slot.GetComponent<ItemSlot>();
        if (itemSlot != null)
        {
            itemSlot.SetItem(inventoryItem);
        }
        
        // 添加点击事件（收集产出）
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            // 确保Button组件的targetGraphic是父物体的Image，而不是子物体
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotButton.targetGraphic == null)
            {
                slotButton.targetGraphic = slotImage;
            }
            
            slotButton.onClick.AddListener(() => CollectProduceItem(itemIndex));
        }
        
        // 添加鼠标悬停事件
        UnityEngine.EventSystems.EventTrigger trigger = slot.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 清除现有的事件
        trigger.triggers.Clear();
        
        // 确保父物体可以接收射线检测（用于边界判断）
        UnityEngine.CanvasGroup parentCanvasGroup = slot.GetComponent<UnityEngine.CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = slot.gameObject.AddComponent<UnityEngine.CanvasGroup>();
        }
        parentCanvasGroup.blocksRaycasts = true;
        parentCanvasGroup.alpha = 1f;
        
        // 确保父物体有碰撞器
        UnityEngine.BoxCollider2D boxCollider = slot.GetComponent<UnityEngine.BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = slot.gameObject.AddComponent<UnityEngine.BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new UnityEngine.Vector2(120, 120);
        }
        
        // 确保父物体有 Image 组件（用于射线检测）
        Image parentImage = slot.GetComponent<Image>();
        if (parentImage == null)
        {
            parentImage = slot.gameObject.AddComponent<Image>();
            parentImage.color = new UnityEngine.Color(0, 0, 0, 0);
        }
        
        // 添加鼠标进入事件
        UnityEngine.EventSystems.EventTrigger.Entry enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => 
        {
            OnItemHover(materialType);
        });
        trigger.triggers.Add(enterEntry);
        
        // 添加鼠标离开事件
        UnityEngine.EventSystems.EventTrigger.Entry exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => 
        {
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitEntry);
    }
    
    // 创建产出配方格子
    void CreateProduceRecipeSlot(GameObject slot, RecipeType recipeType, int itemIndex)
    {
        if (alchemySystem == null) return;
        
        // 获取配方数据
        RecipeData recipe = alchemySystem.GetRecipeData(recipeType);
        if (recipe == null) return;
        
        // 创建InventoryItem对象
        InventoryItem inventoryItem = new InventoryItem(
            recipe.name,
            ItemType.Consumable,
            1,
            recipe.resultDescription,
            recipe.alchemicalValue,
            ItemRarity.Common
        );
        inventoryItem.potionType = recipeType;
        
        // 设置物品到ItemSlot
        ItemSlot itemSlot = slot.GetComponent<ItemSlot>();
        if (itemSlot != null)
        {
            itemSlot.SetItem(inventoryItem);
        }
        
        // 添加点击事件（收集产出）
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            // 确保Button组件的targetGraphic是父物体的Image，而不是子物体
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotButton.targetGraphic == null)
            {
                slotButton.targetGraphic = slotImage;
            }
            
            slotButton.onClick.AddListener(() => CollectProduceItem(itemIndex));
        }
        
        // 添加鼠标悬停事件
        UnityEngine.EventSystems.EventTrigger trigger = slot.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 清除现有的事件
        trigger.triggers.Clear();
        
        // 确保父物体可以接收射线检测（用于边界判断）
        UnityEngine.CanvasGroup parentCanvasGroup = slot.GetComponent<UnityEngine.CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = slot.gameObject.AddComponent<UnityEngine.CanvasGroup>();
        }
        parentCanvasGroup.blocksRaycasts = true;
        parentCanvasGroup.alpha = 1f;
        
        // 确保父物体有碰撞器
        UnityEngine.BoxCollider2D boxCollider = slot.GetComponent<UnityEngine.BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = slot.gameObject.AddComponent<UnityEngine.BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new UnityEngine.Vector2(120, 120);
        }
        
        // 确保父物体有 Image 组件（用于射线检测）
        Image parentImage = slot.GetComponent<Image>();
        if (parentImage == null)
        {
            parentImage = slot.gameObject.AddComponent<Image>();
            parentImage.color = new UnityEngine.Color(0, 0, 0, 0);
        }
        
        // 添加鼠标进入事件
        UnityEngine.EventSystems.EventTrigger.Entry enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => 
        {
            OnItemHover(recipe);
        });
        trigger.triggers.Add(enterEntry);
        
        // 添加鼠标离开事件
        UnityEngine.EventSystems.EventTrigger.Entry exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => 
        {
            OnItemHoverExit();
        });
        trigger.triggers.Add(exitEntry);
    }
    
    // 收集单个产出物品
    void CollectProduceItem(int index)
    {
        if (index < 0 || index >= produceItems.Count) return;
        
        object item = produceItems[index];
        if (item == null) return;
        
        // 收集到背包
        if (item is MaterialTypeEnum materialType)
        {
            inventorySystem.AddMaterial(materialType, 1);
            Debug.Log($"[AlchemyUI] 收集产出材料: {materialType}");
        }
        else if (item is RecipeType recipeType)
        {
            inventorySystem.AddPotion(recipeType, 1);
            Debug.Log($"[AlchemyUI] 收集产出消耗品: {recipeType}");
        }
        
        // 从列表中移除
        produceItems.RemoveAt(index);
        
        // 刷新UI
        UpdateAlchemyUI();
    }
    
    // 收集所有产出物品
    void CollectProduceItems()
    {
        if (produceItems == null || produceItems.Count == 0) return;
        
        Debug.Log("[AlchemyUI] 自动收集所有产出物品...");
        
        // 逐个收集到背包
        foreach (object item in produceItems)
        {
            if (item is MaterialTypeEnum materialType)
            {
                inventorySystem.AddMaterial(materialType, 1);
            }
            else if (item is RecipeType recipeType)
            {
                inventorySystem.AddPotion(recipeType, 1);
            }
        }
        
        // 清空本地列表和炼金系统的列表
        produceItems.Clear();
        alchemySystem.ClearPendingProduce();
    }
    
    // 清空产出物品
    void ClearProduceItems()
    {
        produceItems.Clear();
        alchemySystem.ClearPendingProduce();
    }
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
