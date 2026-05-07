using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 炼药锅和营火生成点配置 - 包含位置、类型和权重
/// </summary>
[System.Serializable]
public class CauldronAndCampfireSpawnPointEntry
{
    [Tooltip("生成点位置")]
    public Transform point;

    [Tooltip("生成类型：炼药锅或营火")]
    public SpawnType spawnType;

    [Tooltip("选择权重（越高越容易被选中）")]
    public int weight = 1;

    [Tooltip("生成点名称（用于调试）")]
    public string name = "SpawnPoint";
}

/// <summary>
/// 生成类型枚举
/// </summary>
public enum SpawnType
{
    Cauldron,  // 炼药锅
    Campfire   // 营火
}

/// <summary>
/// 炼药锅和营火生成管理器 - 参考MaterialSpawnManager的结构设计
/// </summary>
public class CauldronAndCampfireSpawnManager : MonoBehaviour
{
    [Header("基本设置")]
    [Tooltip("是否启用自动生成")]
    public bool autoSpawn = true;

    [Header("炼药锅设置")]
    [Tooltip("炼药锅预制体")]
    public GameObject cauldronPrefab;

    [Header("营火设置")]
    [Tooltip("营火预制体")]
    public GameObject campfirePrefab;

    [Header("生成区域设置")]
    [Tooltip("生成范围半径")]
    public float spawnRadius = 2f;

    [Header("生成位置")]
    [Tooltip("生成点配置列表")]
    public CauldronAndCampfireSpawnPointEntry[] spawnPointEntries;

    [Header("生成参数")]
    [Tooltip("障碍物图层")]
    public LayerMask obstacleLayer;

    [Tooltip("最大生成尝试次数")]
    public int maxSpawnAttempts = 10;

    [Tooltip("生成点附近距离阈值（检查障碍物密度用）")]
    public float spawnPointProximityThreshold = 5f;

    [Tooltip("最大对象数量（炼药锅和营火的总数量）")]
    public int maxObjectCount = 1;

    [Tooltip("生成间隔（秒）")]
    public float spawnInterval = 60f;

    [Tooltip("生成父对象")]
    public Transform spawnParent;

    // 私有变量
    private IntegratedMapSystem mapSystem;
    private List<GameObject> currentCauldrons = new List<GameObject>();
    private List<GameObject> currentCampfires = new List<GameObject>();
    private float spawnTimer = 0f;
    private Dictionary<Transform, GameObject> spawnPointToObject = new Dictionary<Transform, GameObject>(); // 生成点与对象的映射

    private void Start()
    {
        mapSystem = FindObjectOfType<IntegratedMapSystem>();
        
        // 如果启用自动生成，在开始时生成炼药锅和营火
        if (autoSpawn)
        {
            SpawnInitialObjects();
        }
        
        Debug.Log("炼药锅和营火生成管理器已初始化");
    }

    private void Update()
    {
        // 自动生成炼药锅和营火
        if (autoSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnObjects();
            }
        }
    }

    /// <summary>
    /// 生成初始对象
    /// </summary>
    public void SpawnInitialObjects()
    {
        // 清理旧对象
        ClearObjects();
        
        // 生成初始炼药锅和营火
        for (int i = 0; i < maxObjectCount; i++)
        {
            SpawnObjectAtRandomPosition();
        }
    }

    /// <summary>
    /// 生成对象
    /// </summary>
    public void SpawnObjects()
    {
        // 检查当前对象数量
        int currentTotalCount = currentCauldrons.Count + currentCampfires.Count;
        
        if (currentTotalCount >= maxObjectCount)
        {
            return;
        }
        
        // 生成一个对象
        SpawnObjectAtRandomPosition();
    }

    /// <summary>
    /// 在随机位置生成对象
    /// </summary>
    private void SpawnObjectAtRandomPosition()
    {
        // 获取生成位置和生成点
        CauldronAndCampfireSpawnPointEntry selectedPoint = null;
        Vector3 spawnPosition = GetSpawnPosition(out selectedPoint);
        
        // 根据选中的生成点类型选择预制体
        GameObject selectedPrefab = null;
        SpawnType spawnType = SpawnType.Cauldron;
        
        if (selectedPoint != null)
        {
            spawnType = selectedPoint.spawnType;
            selectedPrefab = GetPrefabByType(spawnType);
        }
        else
        {
            // 如果没有使用生成点，随机选择类型
            spawnType = Random.Range(0, 2) == 0 ? SpawnType.Cauldron : SpawnType.Campfire;
            selectedPrefab = GetPrefabByType(spawnType);
        }
        
        // 检查预制体是否有效
        if (selectedPrefab == null)
        {
            Debug.LogError("预制体未设置：" + spawnType);
            return;
        }
        
        // 检查当前数量是否已达到上限
        int currentTotalCount = currentCauldrons.Count + currentCampfires.Count;
        if (currentTotalCount >= maxObjectCount)
        {
            Debug.Log("对象数量已达上限");
            return;
        }
        
        // 生成对象
        GameObject spawnedObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        
        // 确保对象处于启用状态
        if (spawnedObject != null)
        {
            spawnedObject.SetActive(true);
            // 确保对象的所有子对象也处于启用状态
            foreach (Transform child in spawnedObject.transform)
            {
                child.gameObject.SetActive(true);
            }
            
            // 添加到当前对象列表
            if (spawnType == SpawnType.Cauldron)
            {
                currentCauldrons.Add(spawnedObject);
            }
            else if (spawnType == SpawnType.Campfire)
            {
                currentCampfires.Add(spawnedObject);
            }
            
            // 如果使用了生成点，记录生成点与对象的映射
            if (selectedPoint != null && selectedPoint.point != null)
            {
                spawnPointToObject[selectedPoint.point] = spawnedObject;
            }
            
            // 设置父对象
            if (spawnParent != null)
            {
                spawnedObject.transform.parent = spawnParent;
            }
            
            Debug.Log(spawnType + "生成在：" + spawnPosition);
        }
    }

    /// <summary>
    /// 根据类型获取预制体
    /// </summary>
    private GameObject GetPrefabByType(SpawnType type)
    {
        switch (type)
        {
            case SpawnType.Cauldron:
                return cauldronPrefab;
            case SpawnType.Campfire:
                return campfirePrefab;
            default:
                return null;
        }
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition(out CauldronAndCampfireSpawnPointEntry selectedPoint)
    {
        selectedPoint = null;
        
        // 尝试从生成点列表中选择
        if (spawnPointEntries != null && spawnPointEntries.Length > 0)
        {
            // 根据权重选择生成点
            selectedPoint = SelectSpawnPointByWeight();

            if (selectedPoint != null && selectedPoint.point != null)
            {
                // 在选中的生成点周围随机生成
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPosition = selectedPoint.point.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                
                // 检查位置是否有效
                if (IsValidSpawnPosition(spawnPosition))
                {
                    return spawnPosition;
                }
            }
        }

        // 如果没有设置生成点或选中的生成点无效，使用随机位置
        return FindValidSpawnPosition();
    }

    /// <summary>
    /// 找到有效的生成位置
    /// </summary>
    private Vector3 FindValidSpawnPosition()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // 生成随机位置
            Vector3 spawnPosition = GetRandomMapPosition();
            
            // 检查位置是否有效（没有障碍物）
            if (IsValidSpawnPosition(spawnPosition))
            {
                return spawnPosition;
            }
        }
        
        // 如果没有找到合适位置，返回默认位置
        return Vector3.zero;
    }

    /// <summary>
    /// 获取随机地图位置
    /// </summary>
    private Vector3 GetRandomMapPosition()
    {
        if (mapSystem != null)
        {
            // 从地图系统获取随机位置
            Vector2Int randomGridPos = new Vector2Int(
                Random.Range(0, mapSystem.gridSize),
                Random.Range(0, mapSystem.gridSize)
            );
            return mapSystem.GetWorldPosition(randomGridPos);
        }
        
        // 如果没有地图系统，生成随机位置
        return new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), 0);
    }

    /// <summary>
    /// 检查位置是否有效
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // 检查是否与障碍物重叠
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f, obstacleLayer);
        return colliders.Length == 0;
    }

    /// <summary>
    /// 根据权重选择生成点
    /// </summary>
    private CauldronAndCampfireSpawnPointEntry SelectSpawnPointByWeight()
    {
        if (spawnPointEntries == null || spawnPointEntries.Length == 0)
            return null;

        // 清理已销毁的对象映射
        CleanUpDestroyedObjects();

        // 创建可用生成点列表（排除附近障碍物过多的生成点和已有对象的生成点）
        List<CauldronAndCampfireSpawnPointEntry> availablePoints = new List<CauldronAndCampfireSpawnPointEntry>();
        int totalWeight = 0;

        foreach (var entry in spawnPointEntries)
        {
            if (entry != null && entry.point != null)
            {
                // 检查该生成点是否已有对象
                if (spawnPointToObject.ContainsKey(entry.point) && spawnPointToObject[entry.point] != null)
                {
                    continue; // 跳过已有对象的生成点
                }

                // 检查附近障碍物数量
                if (IsSpawnPointAvailable(entry.point.position))
                {
                    availablePoints.Add(entry);
                    totalWeight += Mathf.Max(1, entry.weight);
                }
            }
        }

        // 如果没有可用生成点，返回null
        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("所有生成点附近都有障碍物或已有对象，暂时无法使用生成点");
            return null;
        }

        if (totalWeight <= 0)
        {
            // 如果总权重为0，使用简单随机选择
            return availablePoints[UnityEngine.Random.Range(0, availablePoints.Count)];
        }

        // 根据权重从可用生成点中选择
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var entry in availablePoints)
        {
            currentWeight += Mathf.Max(1, entry.weight);
            if (randomValue < currentWeight)
            {
                return entry;
            }
        }

        // 兜底返回第一个可用条目
        return availablePoints[0];
    }

    /// <summary>
    /// 检查生成点是否可用
    /// </summary>
    private bool IsSpawnPointAvailable(Vector3 position)
    {
        // 检查附近是否有障碍物
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, spawnPointProximityThreshold, obstacleLayer);
        return colliders.Length < 2; // 附近障碍物数量小于2视为可用
    }

    /// <summary>
    /// 清理已销毁的对象映射
    /// </summary>
    private void CleanUpDestroyedObjects()
    {
        // 创建一个临时列表来存储需要移除的键
        List<Transform> keysToRemove = new List<Transform>();
        
        foreach (var kvp in spawnPointToObject)
        {
            if (kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        // 移除已销毁的对象映射
        foreach (var key in keysToRemove)
        {
            spawnPointToObject.Remove(key);
        }
    }

    /// <summary>
    /// 清理所有对象
    /// </summary>
    public void ClearObjects()
    {
        foreach (GameObject obj in currentCauldrons)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentCauldrons.Clear();
        
        foreach (GameObject obj in currentCampfires)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentCampfires.Clear();
        
        spawnPointToObject.Clear(); // 清理生成点与对象的映射
        Debug.Log("所有炼药锅和营火已清理");
    }

    /// <summary>
    /// 获取当前炼药锅数量
    /// </summary>
    public int GetCurrentCauldronCount()
    {
        // 清理已销毁的对象
        currentCauldrons.RemoveAll(obj => obj == null);
        return currentCauldrons.Count;
    }

    /// <summary>
    /// 获取当前营火数量
    /// </summary>
    public int GetCurrentCampfireCount()
    {
        // 清理已销毁的对象
        currentCampfires.RemoveAll(obj => obj == null);
        return currentCampfires.Count;
    }

    /// <summary>
    /// 手动生成对象
    /// </summary>
    public void SpawnObject()
    {
        SpawnObjectAtRandomPosition();
    }

    /// <summary>
    /// 手动生成炼药锅
    /// </summary>
    public void SpawnCauldron()
    {
        if (cauldronPrefab != null)
        {
            int currentTotalCount = currentCauldrons.Count + currentCampfires.Count;
            if (currentTotalCount >= maxObjectCount)
            {
                Debug.Log("对象数量已达上限");
                return;
            }
            
            // 创建一个临时的生成点条目
            CauldronAndCampfireSpawnPointEntry tempEntry = new CauldronAndCampfireSpawnPointEntry();
            tempEntry.spawnType = SpawnType.Cauldron;
            
            // 使用临时条目生成对象
            GameObject selectedPrefab = cauldronPrefab;
            Vector3 spawnPosition = FindValidSpawnPosition();
            
            if (spawnPosition != Vector3.zero)
            {
                GameObject spawnedObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                if (spawnedObject != null)
                {
                    spawnedObject.SetActive(true);
                    currentCauldrons.Add(spawnedObject);
                    
                    if (spawnParent != null)
                    {
                        spawnedObject.transform.parent = spawnParent;
                    }
                    
                    Debug.Log("手动生成炼药锅在：" + spawnPosition);
                }
            }
        }
    }

    /// <summary>
    /// 手动生成营火
    /// </summary>
    public void SpawnCampfire()
    {
        if (campfirePrefab != null)
        {
            int currentTotalCount = currentCauldrons.Count + currentCampfires.Count;
            if (currentTotalCount >= maxObjectCount)
            {
                Debug.Log("对象数量已达上限");
                return;
            }
            
            // 创建一个临时的生成点条目
            CauldronAndCampfireSpawnPointEntry tempEntry = new CauldronAndCampfireSpawnPointEntry();
            tempEntry.spawnType = SpawnType.Campfire;
            
            // 使用临时条目生成对象
            GameObject selectedPrefab = campfirePrefab;
            Vector3 spawnPosition = FindValidSpawnPosition();
            
            if (spawnPosition != Vector3.zero)
            {
                GameObject spawnedObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                if (spawnedObject != null)
                {
                    spawnedObject.SetActive(true);
                    currentCampfires.Add(spawnedObject);
                    
                    if (spawnParent != null)
                    {
                        spawnedObject.transform.parent = spawnParent;
                    }
                    
                    Debug.Log("手动生成营火在：" + spawnPosition);
                }
            }
        }
    }
}