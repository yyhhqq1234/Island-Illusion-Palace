using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using GameSystems;

// 地图类别枚举
public enum MapCategory
{
    Natural,    // 自然地图
    Human,      // 人文地图
    Special,    // 特殊区域
    Final       // 最终区域
}

// 具体地图类型枚举
public enum MapType
{
    // 自然地图
    Forest,     // 森林
    Wasteland,  // 荒原
    Desert,     // 沙漠
    RockLand,   // 岩地
    Wetland,    // 湿地
    IceField,   // 冰原
    Volcano,    // 火山地
    
    // 人文地图
    RuinCity,   // 废墟都市
    ForgottenManor,  // 遗忘庄园
    AncientTemple,   // 古代神殿
    
    // 特殊区域
    LabFragment,     // 实验室碎片
    MemoryFragment,  // 记忆碎片区域
    
    // 最终区域
    TruthCorridor    // 真理时空回廊入口
}

public class IntegratedMapSystem : MonoBehaviour
{
    // === 小地图网格生成设置 ===
    [Header("小地图网格生成")]
    public int gridSize = 5; // 5×5网格
    
    // === 地图类别设置 ===
    [Header("地图类别")]
    public MapCategory currentMapCategory = MapCategory.Natural;
    public MapType currentMapType = MapType.Forest;
    
    // === 共用设置 ===
    [Header("生成种子")]
    public int seed = -1;
    
    [Header("多周目设置")]
    public int currentCycle = 1;
    
    [Header("时空裂隙预制件")]
    public GameObject timeRiftPrefab;
    
    [Header("宝箱预制件")]
    public GameObject treasureChestPrefab;
    
    // === 地图预制件管理 ===
    [Header("地图预制件")]
    public MapPrefabsByType mapPrefabsByType = new MapPrefabsByType();
    
    [System.Serializable]
    public class MapPrefabsByType
    {
        [Header("森林地图")]
        public MapPrefabs forest;
        
        [Header("荒原地图")]
        public MapPrefabs wasteland;
        
        [Header("沙漠地图")]
        public MapPrefabs desert;
        
        [Header("岩地地图")]
        public MapPrefabs rockLand;
        
        [Header("湿地地图")]
        public MapPrefabs wetland;
        
        [Header("冰原地图")]
        public MapPrefabs iceField;
        
        [Header("火山地图")]
        public MapPrefabs volcano;
        
        [Header("废墟都市地图")]
        public MapPrefabs ruinCity;
        
        [Header("遗忘庄园地图")]
        public MapPrefabs forgottenManor;
        
        [Header("古代神殿地图")]
        public MapPrefabs ancientTemple;
        
        [Header("实验室碎片地图")]
        public MapPrefabs labFragment;
        
        [Header("记忆碎片区域地图")]
        public MapPrefabs memoryFragment;
        
        [Header("真理时空回廊入口地图")]
        public MapPrefabs truthCorridor;
    }
    
    [System.Serializable]
    public class MapPrefabs
    {
        public GameObject safeRoomPrefab;
        public GameObject bossRoomPrefab;
        public List<GameObject> resourcePrefabs = new List<GameObject>();
        public List<GameObject> battlePrefabs = new List<GameObject>();
    }
    
    // === 内部数据 ===
    private System.Random random;
    private HealthSystem playerHealth;
    
    // 小地图网格数据
    private List<GameObject> mapPrefabs = new List<GameObject>();
    private GameObject[,] gridMaps;
    private Vector2Int safeRoomPos;
    private Vector2Int bossRoomPos;
    
    void Start()
    {
        InitializeSystem();
        GenerateMap();
    }
    
    void InitializeSystem()
    {
        // 初始化随机数生成器
        if (seed == -1)
            seed = System.DateTime.Now.GetHashCode();
        random = new System.Random(seed);
        
        // 加载玩家健康系统
        playerHealth = FindObjectOfType<HealthSystem>();
        
        // 初始化小地图网格
        LoadMapPrefabs();
    }
    
    public void GenerateMap()
    {
        GenerateGridMap();
    }
    

    
    void LoadMapPrefabs()
    {
        mapPrefabs.Clear();
        
        // 根据当前地图类型加载对应的预制件
        MapPrefabs currentMapPrefabs = GetMapPrefabsByType(currentMapType);
        
        if (currentMapPrefabs != null)
        {
            // 加载安全区
            if (currentMapPrefabs.safeRoomPrefab != null)
                mapPrefabs.Add(currentMapPrefabs.safeRoomPrefab);
            else
                Debug.LogError(currentMapType + " 安全区预制件未设置");
            
            // 加载BOSS房间
            if (currentMapPrefabs.bossRoomPrefab != null)
                mapPrefabs.Add(currentMapPrefabs.bossRoomPrefab);
            else
                Debug.LogError(currentMapType + " BOSS房间预制件未设置");
            
            // 加载资源房间
            foreach (GameObject prefab in currentMapPrefabs.resourcePrefabs)
            {
                if (prefab != null)
                    mapPrefabs.Add(prefab);
            }
            
            // 加载战斗房间
            foreach (GameObject prefab in currentMapPrefabs.battlePrefabs)
            {
                if (prefab != null)
                    mapPrefabs.Add(prefab);
            }
        }
        
        Debug.Log("加载了 " + mapPrefabs.Count + " 个地图预制件，当前地图：" + currentMapType);
    }
    
    MapPrefabs GetMapPrefabsByType(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Forest: return mapPrefabsByType.forest;
            case MapType.Wasteland: return mapPrefabsByType.wasteland;
            case MapType.Desert: return mapPrefabsByType.desert;
            case MapType.RockLand: return mapPrefabsByType.rockLand;
            case MapType.Wetland: return mapPrefabsByType.wetland;
            case MapType.IceField: return mapPrefabsByType.iceField;
            case MapType.Volcano: return mapPrefabsByType.volcano;
            case MapType.RuinCity: return mapPrefabsByType.ruinCity;
            case MapType.ForgottenManor: return mapPrefabsByType.forgottenManor;
            case MapType.AncientTemple: return mapPrefabsByType.ancientTemple;
            case MapType.LabFragment: return mapPrefabsByType.labFragment;
            case MapType.MemoryFragment: return mapPrefabsByType.memoryFragment;
            case MapType.TruthCorridor: return mapPrefabsByType.truthCorridor;
            default: return null;
        }
    }
    
    void GenerateGridMap()
    {
        // 检查是否有安全区和BOSS房间预制件
        if (mapPrefabs.Count < 2)
        {
            Debug.LogError("地图预制件不足，至少需要安全区和BOSS房间预制件");
            return;
        }
        
        // 检查资源房间和战斗房间预制件
        List<GameObject> resourcePrefabs = new List<GameObject>();
        List<GameObject> battlePrefabs = new List<GameObject>();
        
        // 分离资源和战斗预制件（从索引2开始）
        for (int i = 2; i < mapPrefabs.Count; i++)
        {
            // 简单区分：偶数索引为资源，奇数索引为战斗
            if (i % 2 == 0)
                resourcePrefabs.Add(mapPrefabs[i]);
            else
                battlePrefabs.Add(mapPrefabs[i]);
        }
        
        // 检查资源和战斗房间种类是否在两个以上
        if (resourcePrefabs.Count < 2)
        {
            Debug.LogError("资源房间种类不足，至少需要2种资源房间预制件");
            return;
        }
        
        if (battlePrefabs.Count < 2)
        {
            Debug.LogError("战斗房间种类不足，至少需要2种战斗房间预制件");
            return;
        }
        
        // 清理旧地图
        ClearOldMaps();
        
        // 初始化网格
        gridMaps = new GameObject[gridSize, gridSize];
        bool[,] occupied = new bool[gridSize, gridSize];
        
        // 1. 放置安全区在中心
        safeRoomPos = new Vector2Int(gridSize / 2, gridSize / 2);
        PlaceMapAtPosition(safeRoomPos, "SafeRoom");
        occupied[safeRoomPos.x, safeRoomPos.y] = true;
        
        // 2. 放置BOSS房间在边缘
        List<Vector2Int> edgePositions = GetEdgePositions();
        bossRoomPos = edgePositions[random.Next(edgePositions.Count)];
        PlaceMapAtPosition(bossRoomPos, "BossRoom");
        occupied[bossRoomPos.x, bossRoomPos.y] = true;
        
        // 3. 计算剩余位置
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (!occupied[x, y])
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        // 4. 计算需要的资源和战斗地图数量
        int totalRemaining = availablePositions.Count;
        int minResourceMaps = 2;
        int minBattleMaps = 2;
        
        // 确保资源和战斗地图数量平衡
        int maxResourceMaps = totalRemaining - minBattleMaps;
        int maxBattleMaps = totalRemaining - minResourceMaps;
        
        int resourceCount = random.Next(minResourceMaps, maxResourceMaps + 1);
        int battleCount = totalRemaining - resourceCount;
        
        // 5. 放置资源地图
        for (int i = 0; i < resourceCount; i++)
        {
            if (availablePositions.Count == 0) break;
            
            int posIndex = random.Next(availablePositions.Count);
            Vector2Int pos = availablePositions[posIndex];
            availablePositions.RemoveAt(posIndex);
            
            int prefabIndex = random.Next(resourcePrefabs.Count);
            PlaceMapPrefabAtPosition(pos, resourcePrefabs[prefabIndex]);
        }
        
        // 6. 放置战斗地图
        foreach (Vector2Int pos in availablePositions)
        {
            int prefabIndex = random.Next(battlePrefabs.Count);
            PlaceMapPrefabAtPosition(pos, battlePrefabs[prefabIndex]);
        }
        
        Debug.Log("小地图网格生成完成: 安全区在 " + safeRoomPos + ", BOSS房间在 " + bossRoomPos);
        Debug.Log("资源地图: " + resourceCount + " 个, 战斗地图: " + battleCount + " 个");
        Debug.Log("资源房间种类: " + resourcePrefabs.Count + " 种, 战斗房间种类: " + battlePrefabs.Count + " 种");
    }
    
    List<Vector2Int> GetEdgePositions()
    {
        List<Vector2Int> edges = new List<Vector2Int>();
        
        // 上边缘
        for (int x = 0; x < gridSize; x++)
        {
            edges.Add(new Vector2Int(x, 0));
        }
        
        // 下边缘
        for (int x = 0; x < gridSize; x++)
        {
            edges.Add(new Vector2Int(x, gridSize - 1));
        }
        
        // 左边缘（排除上下角）
        for (int y = 1; y < gridSize - 1; y++)
        {
            edges.Add(new Vector2Int(0, y));
        }
        
        // 右边缘（排除上下角）
        for (int y = 1; y < gridSize - 1; y++)
        {
            edges.Add(new Vector2Int(gridSize - 1, y));
        }
        
        return edges;
    }
    
    void PlaceMapAtPosition(Vector2Int position, string mapType)
    {
        switch (mapType)
        {
            case "SafeRoom":
                PlaceMapPrefabAtPosition(position, mapPrefabs[0]);
                break;
            case "BossRoom":
                PlaceMapPrefabAtPosition(position, mapPrefabs[1]);
                break;
        }
    }
    
    void PlaceMapPrefabAtPosition(Vector2Int position, GameObject prefab)
    {
        if (prefab == null) return;
        
        Vector3 worldPosition = GetWorldPosition(position);
        GameObject mapInstance = Instantiate(prefab, worldPosition, Quaternion.identity);
        mapInstance.name = prefab.name + "_" + position.x + "_" + position.y;
        mapInstance.transform.parent = transform;
        
        gridMaps[position.x, position.y] = mapInstance;
    }
    
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        // 小地图大小：39x26格子，每个格子1x1
        float mapWidth = 39f;
        float mapHeight = 26f;
        
        // 计算世界坐标，确保整个网格中心在原点
        // 每个小地图的坐标原点是其最左下角
        float gridOffsetX = (gridSize - 1) * mapWidth / 2f;
        float gridOffsetY = (gridSize - 1) * mapHeight / 2f;
        
        float x = gridPosition.x * mapWidth - gridOffsetX;
        float y = gridPosition.y * mapHeight - gridOffsetY;
        
        return new Vector3(x, y, 0);
    }
    

    
    // === 通用方法 ===
    public void ClearOldMaps()
    {
        if (gridMaps != null)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (gridMaps[x, y] != null)
                    {
                        DestroyImmediate(gridMaps[x, y]);
                    }
                }
            }
        }
        
        // 清理所有子对象
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }
    

    
    public void GenerateNewMap()
    {
        // 清理旧地图
        ClearOldMaps();
        
        seed = -1;
        InitializeSystem();
        GenerateMap();
    }
    
    public void SetCycle(int cycle)
    {
        currentCycle = cycle;
    }
    
    public void SetMapCategory(MapCategory category)
    {
        currentMapCategory = category;
        
        // 根据地图类别设置默认地图类型
        switch (category)
        {
            case MapCategory.Natural:
                currentMapType = MapType.Forest;
                break;
            case MapCategory.Human:
                currentMapType = MapType.RuinCity;
                break;
            case MapCategory.Special:
                currentMapType = MapType.LabFragment;
                break;
            case MapCategory.Final:
                currentMapType = MapType.TruthCorridor;
                break;
        }
        
        // 调整生成参数并加载预制件
        AdjustGenerationParamsByMapType();
        LoadMapPrefabs();
    }
    
    public void SetMapType(MapType type)
    {
        currentMapType = type;
        // 根据地图类型调整生成参数
        AdjustGenerationParamsByMapType();
        // 重新加载预制件
        LoadMapPrefabs();
    }
    
    private void AdjustGenerationParamsByMapType()
    {
        // 根据不同地图类型调整生成参数
        switch (currentMapType)
        {
            case MapType.Volcano:
            case MapType.IceField:
                // 高难度地图，增加战斗地图数量
                break;
            case MapType.LabFragment:
                // 炼金主题，增加资源地图数量
                break;
            case MapType.MemoryFragment:
                // 剧情主题，特殊处理
                break;
        }
    }
    
    // 调试方法
    public void DebugPrintMapInfo()
    {
        Debug.Log("=== 小地图网格信息 ===");
        Debug.Log($"网格大小：{gridSize}x{gridSize}");
        Debug.Log($"安全区位置：{safeRoomPos}");
        Debug.Log($"BOSS房间位置：{bossRoomPos}");
        Debug.Log($"当前地图类别：{currentMapCategory}");
        Debug.Log($"当前地图类型：{currentMapType}");
    }
    

}





public class TimeGate : MonoBehaviour
{
    public bool isUnlocked = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isUnlocked)
        {
            Debug.Log("进入时空门！传送到新区域...");
            IntegratedMapSystem mapSystem = FindObjectOfType<IntegratedMapSystem>();
            if (mapSystem != null)
            {
                mapSystem.GenerateNewMap();
            }
        }
    }
}

public class TimeRift : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float roll = UnityEngine.Random.value;
            
            if (roll < 0.8f)
            {
                Debug.Log("时空裂隙传送到高难区域！");
            }
            else
            {
                Debug.Log("时空裂隙传送到记忆区域！");
            }
            
            Destroy(gameObject);
        }
    }
}

public class TreasureChest : MonoBehaviour
{
    public bool isOpened = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            isOpened = true;
            Debug.Log("打开宝箱！获得珍贵材料！");
            
            AlchemySystem alchemySystem = FindObjectOfType<AlchemySystem>();
            if (alchemySystem != null)
            {
                MaterialTypeEnum[] rareMaterials = new MaterialTypeEnum[]
                {
                    MaterialTypeEnum.SoulEssence,
                    MaterialTypeEnum.SoulCrystal,
                    MaterialTypeEnum.ParadoxShard,
                    MaterialTypeEnum.AncientTreeResin,
                    MaterialTypeEnum.LeylineCrystal
                };
                
                MaterialTypeEnum randomMaterial = rareMaterials[UnityEngine.Random.Range(0, rareMaterials.Length)];
                alchemySystem.AddMaterial(randomMaterial, UnityEngine.Random.Range(1, 3));
            }
            
            Destroy(gameObject);
        }
    }
}

// 扩展方法
public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
