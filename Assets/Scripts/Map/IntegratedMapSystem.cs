using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using GameSystems;

public enum MapCategory
{
    Natural,
    Human,
    Special,
    Final
}

public enum MapType
{
    Forest,
    Wasteland,
    Desert,
    RockLand,
    Wetland,
    IceField,
    Volcano,
    RuinCity,
    ForgottenManor,
    AncientTemple,
    LabFragment,
    MemoryFragment,
    TruthCorridor
}

public class IntegratedMapSystem : MonoBehaviour, IMapSystem, ITimeRiftProvider
{
    [Header("网格设置")]
    [Tooltip("网格大小（5x5表示25个小地图）")]
    public int gridSize = 5;

    [Header("地图分类")]
    [Tooltip("当前地图分类")]
    public MapCategory currentMapCategory = MapCategory.Natural;
    [Tooltip("当前地图类型")]
    public MapType currentMapType = MapType.Forest;

    [Header("种子设置")]
    [Tooltip("随机种子（-1表示使用时间作为种子）")]
    public int seed = -1;
    [Header("循环设置")]
    [Tooltip("当前循环次数")]
    public int currentCycle = 1;

    MapType IMapSystem.currentMapType { get => currentMapType; set => currentMapType = value; }
    MapCategory IMapSystem.currentMapCategory { get => currentMapCategory; set => currentMapCategory = value; }
    int IMapSystem.currentCycle { get => currentCycle; set => currentCycle = value; }

    float ITimeRiftProvider.BaseProbability => 0.05f;
    bool ITimeRiftProvider.IsRiftActive => false;
    void ITimeRiftProvider.OnRiftEntered(MapType dst) { Debug.Log("Rift teleport to " + dst); }

    [Header("Boss设置")]
    [Tooltip("时空守护者预制体")]
    public GameObject timeGuardianPrefab;
    [Tooltip("Boss房间预制体")]
    public GameObject bossRoomPrefab;

    [Header("传送门设置")]
    [Tooltip("时空传送门预制体")]
    public GameObject timePortalPrefab;

    [Header("时空裂隙设置")]
    [Tooltip("时空裂隙预制体")]
    public GameObject timeRiftPrefab;

    [Header("宝箱设置")]
    [Tooltip("宝箱预制体")]
    public GameObject treasureChestPrefab;

    [Header("地图预制体")]
    [Tooltip("各类型地图的预制体配置")]
    public MapPrefabsByType mapPrefabsByType = new MapPrefabsByType();

    [System.Serializable]
    public class MapPrefabsByType
    {
        [Header("自然地图")]
        public MapPrefabs forest;
        public MapPrefabs wasteland;
        public MapPrefabs desert;
        public MapPrefabs rockLand;
        public MapPrefabs wetland;
        public MapPrefabs iceField;
        public MapPrefabs volcano;

        [Header("人文地图")]
        public MapPrefabs ruinCity;
        public MapPrefabs forgottenManor;
        public MapPrefabs ancientTemple;

        [Header("特殊地图")]
        public MapPrefabs labFragment;
        public MapPrefabs memoryFragment;

        [Header("最终地图")]
        public MapPrefabs truthCorridor;
    }

    [System.Serializable]
    public class MapPrefabs
    {
        [Header("房间预制体")]
        [Tooltip("安全屋预制体")]
        public GameObject safeRoomPrefab;
        [Tooltip("Boss房间预制体")]
        public GameObject bossRoomPrefab;
        [Tooltip("资源房间预制体列表")]
        public List<GameObject> resourcePrefabs = new List<GameObject>();
        [Tooltip("战斗房间预制体列表")]
        public List<GameObject> battlePrefabs = new List<GameObject>();
    }

    private System.Random random;
    private HealthSystem playerHealth;
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
        if (seed == -1)
            seed = System.DateTime.Now.GetHashCode();
        random = new System.Random(seed);
        playerHealth = FindObjectOfType<HealthSystem>();
        LoadMapPrefabs();
    }

    public void GenerateMap()
    {
        GenerateGridMap();
        BroadcastMapType();
    }

    void BroadcastMapType()
    {
        GameSystems.MapMusicType musicType = ConvertToMapMusicType(currentMapType);
        if (GlobalEventManager.Instance != null)
        {
            GlobalEventManager.Instance.TriggerMapTypeChanged(musicType);
        }
        Debug.Log($"[IntegratedMapSystem] Current Map Type: {currentMapType} -> Music Type: {musicType}");
    }

    GameSystems.MapMusicType ConvertToMapMusicType(MapType mapType)
    {
        return mapType switch
        {
            MapType.Forest          => GameSystems.MapMusicType.Forest,
            MapType.Wasteland       => GameSystems.MapMusicType.Wasteland,
            MapType.Desert          => GameSystems.MapMusicType.Desert,
            MapType.RockLand        => GameSystems.MapMusicType.RockyLand,
            MapType.Wetland         => GameSystems.MapMusicType.Wetland,
            MapType.IceField        => GameSystems.MapMusicType.IcePlains,
            MapType.Volcano         => GameSystems.MapMusicType.VolcanicLand,
            MapType.RuinCity        => GameSystems.MapMusicType.RuinedCity,
            MapType.ForgottenManor  => GameSystems.MapMusicType.ForgottenManor,
            MapType.AncientTemple   => GameSystems.MapMusicType.AncientTemple,
            MapType.LabFragment     => GameSystems.MapMusicType.LabFragment,
            MapType.MemoryFragment  => GameSystems.MapMusicType.MemoryFragmentArea,
            MapType.TruthCorridor   => GameSystems.MapMusicType.TruthTemporalCorridor,
            _                       => GameSystems.MapMusicType.Forest
        };
    }

    void LoadMapPrefabs()
    {
        mapPrefabs.Clear();
        MapPrefabs currentMapPrefabs = GetMapPrefabsByType(currentMapType);

        if (currentMapPrefabs != null)
        {
            if (currentMapPrefabs.safeRoomPrefab != null)
                mapPrefabs.Add(currentMapPrefabs.safeRoomPrefab);
            else
                Debug.LogError(currentMapType + " SafeRoom prefab missing");

            if (currentMapPrefabs.bossRoomPrefab != null)
                mapPrefabs.Add(currentMapPrefabs.bossRoomPrefab);
            else
                Debug.LogError(currentMapType + " BossRoom prefab missing");

            foreach (GameObject prefab in currentMapPrefabs.resourcePrefabs)
            {
                if (prefab != null)
                    mapPrefabs.Add(prefab);
            }

            foreach (GameObject prefab in currentMapPrefabs.battlePrefabs)
            {
                if (prefab != null)
                    mapPrefabs.Add(prefab);
            }
        }

        Debug.Log("Loaded " + mapPrefabs.Count + " map prefabs, type=" + currentMapType);
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
        if (mapPrefabs.Count < 2)
        {
            Debug.LogError("Not enough map prefabs (need >= 2)");
            return;
        }

        float mapWidth = 39f;
        float mapHeight = 26f;

        List<GameObject> resourcePrefabs = new List<GameObject>();
        List<GameObject> battlePrefabs = new List<GameObject>();

        for (int i = 2; i < mapPrefabs.Count; i++)
        {
            if (i % 2 == 0)
                resourcePrefabs.Add(mapPrefabs[i]);
            else
                battlePrefabs.Add(mapPrefabs[i]);
        }

        if (resourcePrefabs.Count < 2)
        {
            Debug.LogError("Need >= 2 resource room prefabs");
            return;
        }

        if (battlePrefabs.Count < 2)
        {
            Debug.LogError("Need >= 2 battle room prefabs");
            return;
        }

        ClearOldMaps();

        gridMaps = new GameObject[gridSize, gridSize];
        bool[,] occupied = new bool[gridSize, gridSize];

        safeRoomPos = new Vector2Int(gridSize / 2, gridSize / 2);
        PlaceMapAtPosition(safeRoomPos, "SafeRoom");
        occupied[safeRoomPos.x, safeRoomPos.y] = true;

        List<Vector2Int> edgePositions = GetEdgePositions();
        bossRoomPos = edgePositions[random.Next(edgePositions.Count)];
        PlaceMapAtPosition(bossRoomPos, "BossRoom");
        occupied[bossRoomPos.x, bossRoomPos.y] = true;

        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (!occupied[x, y])
                    availablePositions.Add(new Vector2Int(x, y));
            }
        }

        foreach (Vector2Int pos in availablePositions)
        {
            float roll = Random.value;
            if (roll < 0.50f && resourcePrefabs.Count > 0)
                PlaceMapPrefabAtPosition(pos, resourcePrefabs[Random.Range(0, resourcePrefabs.Count)]);
            else if (battlePrefabs.Count > 0)
                PlaceMapPrefabAtPosition(pos, battlePrefabs[Random.Range(0, battlePrefabs.Count)]);
            else
                PlaceMapPrefabAtPosition(pos, resourcePrefabs[Random.Range(0, resourcePrefabs.Count)]);
        }

        GenerateTimeRifts();
        GenerateBoundaryWalls();

        Debug.Log("Grid generated: SafeRoom=" + safeRoomPos + ", BossRoom=" + bossRoomPos);
    }

    void GenerateBoundaryWalls()
    {
        float cellWidth = 39f;
        float cellHeight = 26f;

        float leftBoundary = -(gridSize - 1) * cellWidth / 2f;
        float rightBoundary = leftBoundary + gridSize * cellWidth;
        float bottomBoundary = -(gridSize - 1) * cellHeight / 2f;
        float topBoundary = bottomBoundary + gridSize * cellHeight;

        float wallThickness = 5f;

        GameObject boundaryParent = new GameObject("BoundaryWalls");
        boundaryParent.transform.parent = transform;

        CreateWall(new Vector3((leftBoundary + rightBoundary) / 2f, topBoundary + wallThickness / 2f, 0),
                   new Vector2(rightBoundary - leftBoundary + wallThickness * 2, wallThickness),
                   boundaryParent.transform);

        CreateWall(new Vector3((leftBoundary + rightBoundary) / 2f, bottomBoundary - wallThickness / 2f, 0),
                   new Vector2(rightBoundary - leftBoundary + wallThickness * 2, wallThickness),
                   boundaryParent.transform);

        CreateWall(new Vector3(rightBoundary + wallThickness / 2f, (bottomBoundary + topBoundary) / 2f, 0),
                   new Vector2(wallThickness, topBoundary - bottomBoundary + wallThickness * 2),
                   boundaryParent.transform);

        CreateWall(new Vector3(leftBoundary - wallThickness / 2f, (bottomBoundary + topBoundary) / 2f, 0),
                   new Vector2(wallThickness, topBoundary - bottomBoundary + wallThickness * 2),
                   boundaryParent.transform);
    }

    void CreateWall(Vector3 position, Vector2 colliderSize, Transform parent)
    {
        GameObject wall = new GameObject("BoundaryWall");
        wall.transform.position = position;
        wall.transform.parent = parent;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.size = colliderSize;

        wall.layer = LayerMask.NameToLayer("Wall");
    }

    List<Vector2Int> GetEdgePositions()
    {
        List<Vector2Int> edges = new List<Vector2Int>();
        for (int x = 0; x < gridSize; x++)
        {
            edges.Add(new Vector2Int(x, 0));
            edges.Add(new Vector2Int(x, gridSize - 1));
        }
        for (int y = 1; y < gridSize - 1; y++)
        {
            edges.Add(new Vector2Int(0, y));
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
        float mapWidth = 39f;
        float mapHeight = 26f;
        float gridOffsetX = (gridSize - 1) * mapWidth / 2f;
        float gridOffsetY = (gridSize - 1) * mapHeight / 2f;
        float x = gridPosition.x * mapWidth - gridOffsetX;
        float y = gridPosition.y * mapHeight - gridOffsetY;
        return new Vector3(x, y, 0);
    }

    public void GenerateTimeRifts()
    {
        if (timeRiftPrefab == null) return;

        float baseProb = 0.05f;
        var bs = FindObjectOfType<BurdenSystem>();
        if (bs != null)
        {
            if (bs.currentBurden > 50) baseProb += 0.10f;
            if (bs.currentBurden > 70) baseProb += 0.10f;
        }
        var mf = FindObjectOfType<MemoryFragmentSystem>();
        if (mf != null) baseProb += mf.GetFragmentCount() * 0.03f;
        if (currentCycle >= 2) baseProb += 0.03f;

        if (Random.value <= baseProb)
        {
            Vector3 riftPos = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
            Instantiate(timeRiftPrefab, riftPos, Quaternion.identity);
            Debug.Log("[TimeRift] spawned, prob=" + baseProb);
        }
    }

    public void ClearOldMaps()
    {
        if (gridMaps != null)
        {
            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                    if (gridMaps[x, y] != null)
                        DestroyImmediate(gridMaps[x, y]);
        }
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
    }

    public void GenerateNewMap()
    {
        ClearOldMaps();
        seed = -1;
        InitializeSystem();
        GenerateMap();
    }

    public void SetCycle(int cycle)
    {
        currentCycle = cycle;
    }

    public void RegisterCampfire(Vector3 position)
    {
        Debug.Log("Campfire registered at " + position);
    }

    public bool IsPositionValidForRift(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);

        if (!IsValidGridPosition(gridPos))
            return false;

        GameObject mapCell = gridMaps[gridPos.x, gridPos.y];
        if (mapCell == null)
            return true;

        Tilemap collisionTilemap = GetCollisionTilemap(mapCell);
        if (collisionTilemap == null)
            return true;

        Vector3Int tilePos = WorldToTilePosition(collisionTilemap, worldPosition);
        TileBase tile = collisionTilemap.GetTile(tilePos);

        if (tile != null)
        {
            Debug.Log($"[IntegratedMapSystem] Position {worldPosition} is on obstacle in grid {gridPos}");
            return false;
        }

        return true;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        float mapWidth = 39f;
        float mapHeight = 26f;
        float gridOffsetX = (gridSize - 1) * mapWidth / 2f;
        float gridOffsetY = (gridSize - 1) * mapHeight / 2f;

        int x = Mathf.FloorToInt((worldPosition.x + gridOffsetX) / mapWidth);
        int y = Mathf.FloorToInt((worldPosition.y + gridOffsetY) / mapHeight);

        return new Vector2Int(x, y);
    }

    bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridSize && gridPos.y >= 0 && gridPos.y < gridSize;
    }

    Tilemap GetCollisionTilemap(GameObject mapCell)
    {
        Tilemap[] tilemaps = mapCell.GetComponentsInChildren<Tilemap>();
        foreach (var tilemap in tilemaps)
        {
            if (tilemap.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                tilemap.gameObject.layer == LayerMask.NameToLayer("Wall") ||
                tilemap.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                return tilemap;
            }
        }

        foreach (var tilemap in tilemaps)
        {
            if (tilemap.name.Contains("Collision") ||
                tilemap.name.Contains("Blocked") ||
                tilemap.name.Contains("Obstacle"))
            {
                return tilemap;
            }
        }

        return tilemaps.Length > 0 ? tilemaps[0] : null;
    }

    Vector3Int WorldToTilePosition(Tilemap tilemap, Vector3 worldPosition)
    {
        return tilemap.WorldToCell(worldPosition);
    }

    public void SetMapCategory(MapCategory category)
    {
        currentMapCategory = category;
        switch (category)
        {
            case MapCategory.Natural: currentMapType = MapType.Forest; break;
            case MapCategory.Human: currentMapType = MapType.RuinCity; break;
            case MapCategory.Special: currentMapType = MapType.LabFragment; break;
            case MapCategory.Final: currentMapType = MapType.TruthCorridor; break;
        }
        AdjustGenerationParamsByMapType();
        LoadMapPrefabs();
    }

    public void SetMapType(MapType type)
    {
        currentMapType = type;
        AdjustGenerationParamsByMapType();
        LoadMapPrefabs();
    }

    void AdjustGenerationParamsByMapType()
    {
        switch (currentMapType)
        {
            case MapType.Volcano:
            case MapType.IceField:
                break;
            case MapType.LabFragment:
                break;
            case MapType.MemoryFragment:
                break;
        }
    }

    public void DebugPrintMapInfo()
    {
        Debug.Log("=== Grid Info ===");
        Debug.Log("Size: " + gridSize + "x" + gridSize);
        Debug.Log("SafeRoom: " + safeRoomPos);
        Debug.Log("BossRoom: " + bossRoomPos);
        Debug.Log("Category: " + currentMapCategory);
        Debug.Log("Type: " + currentMapType);
    }
}
