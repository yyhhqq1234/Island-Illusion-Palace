using UnityEngine;
using System.Collections.Generic;
using GameSystems;

/// <summary>
/// 材料生成点配置 - 包含位置和权重
/// </summary>
[System.Serializable]
public class MaterialSpawnPointEntry
{
    [Tooltip("生成点位置")]
    public Transform point;

    [Tooltip("选择权重（越高越容易被选中）")]
    public int weight = 1;

    [Tooltip("生成点名称（用于调试）")]
    public string name = "MaterialSpawnPoint";
}

/// <summary>
/// 炼金材料生成管理器 - 参考PlayerSpawnManager的结构设计
/// </summary>
public class MaterialSpawnManager : MonoBehaviour
{
    [Header("基本设置")]
    [Tooltip("是否启用自动生成")]
    public bool autoSpawn = true;
    [Tooltip("材料预制体列表")]
    public List<GameObject> materialPrefabs;
    [Tooltip("材料生成父对象")]
    public Transform materialSpawnParent;

    [Header("生成区域设置")]
    [Tooltip("生成范围半径")]
    public float spawnRadius = 5f;

    [Header("生成位置")]
    [Tooltip("生成点配置列表")]
    public MaterialSpawnPointEntry[] spawnPointEntries;

    [Header("生成参数")]
    [Tooltip("障碍物图层")]
    public LayerMask obstacleLayer;
    [Tooltip("最大生成尝试次数")]
    public int maxSpawnAttempts = 10;
    [Tooltip("生成点附近距离阈值（检查密度用）")]
    public float spawnPointProximityThreshold = 10f;
    [Tooltip("生成点附近最大材料数量")]
    public int maxMaterialsPerSpawnPoint = 1;
    [Tooltip("最大材料数量")]
    public int maxMaterialCount = 20;
    [Tooltip("生成间隔（秒）")]
    public float spawnInterval = 30f;

    // 私有变量
    private List<GameObject> currentMaterials = new List<GameObject>();
    private float spawnTimer = 0f;
    private Dictionary<Transform, GameObject> spawnPointToMaterial = new Dictionary<Transform, GameObject>();

    private void Start()
    {
        if (spawnPointEntries == null || spawnPointEntries.Length == 0)
        {
            Debug.LogWarning("[MaterialSpawnManager] 未配置生成点（spawnPointEntries 为空），材料不会自动生成。请在 Inspector 中添加生成点。");
        }

        if (autoSpawn)
        {
            SpawnInitialMaterials();
        }

        Debug.Log("[MaterialSpawnManager] 材料生成管理器已初始化");
    }

    private void Update()
    {
        // 自动生成材料
        if (autoSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnMaterials();
            }
        }
    }

    /// <summary>
    /// 生成初始材料
    /// </summary>
    public void SpawnInitialMaterials()
    {
        // 清理旧材料
        ClearMaterials();
        
        // 生成初始材料
        for (int i = 0; i < maxMaterialCount / 2; i++)
        {
            SpawnMaterialAtRandomPosition();
        }
    }

    /// <summary>
    /// 生成材料
    /// </summary>
    public void SpawnMaterials()
    {
        // 检查当前材料数量
        if (currentMaterials.Count >= maxMaterialCount)
        {
            return;
        }
        
        // 生成一个材料
        SpawnMaterialAtRandomPosition();
    }

    /// <summary>
    /// 在生成点生成材料（仅使用开发者配置的 spawnPointEntries，不使用随机地图位置）
    /// </summary>
    private void SpawnMaterialAtRandomPosition()
    {
        if (materialPrefabs == null || materialPrefabs.Count == 0)
        {
            Debug.LogError("[MaterialSpawnManager] 材料预制体列表未设置");
            return;
        }

        // 从生成点中选择
        MaterialSpawnPointEntry selectedPoint = SelectSpawnPointByWeight();
        if (selectedPoint == null || selectedPoint.point == null)
        {
            Debug.Log("[MaterialSpawnManager] 无可用生成点，跳过本次生成");
            return;
        }

        // 在生成点周围随机偏移
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = selectedPoint.point.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // 检查物理障碍物
        if (!IsPositionClear(spawnPosition))
        {
            Debug.Log($"[MaterialSpawnManager] 生成点 '{selectedPoint.name}' 附近位置被阻挡，跳过");
            return;
        }

        GameObject selectedPrefab = materialPrefabs[Random.Range(0, materialPrefabs.Count)];
        GameObject material = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        if (material != null)
        {
            material.SetActive(true);
            foreach (Transform child in material.transform)
                child.gameObject.SetActive(true);

            currentMaterials.Add(material);
            spawnPointToMaterial[selectedPoint.point] = material;

            if (materialSpawnParent != null)
                material.transform.parent = materialSpawnParent;

            Debug.Log($"[MaterialSpawnManager] 材料 {selectedPrefab.name} 生成于 {selectedPoint.name} → {spawnPosition}");
        }
    }

    /// <summary>
    /// 检查位置是否有物理障碍物重叠
    /// </summary>
    private bool IsPositionClear(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f, obstacleLayer);
        return colliders.Length == 0;
    }

    /// <summary>
    /// 根据权重选择生成点
    /// </summary>
    private MaterialSpawnPointEntry SelectSpawnPointByWeight()
    {
        if (spawnPointEntries == null || spawnPointEntries.Length == 0)
            return null;

        // 清理已销毁的材料映射
        CleanUpDestroyedMaterials();

        // 创建可用生成点列表（排除附近材料过多的生成点）
        List<MaterialSpawnPointEntry> availablePoints = new List<MaterialSpawnPointEntry>();
        int totalWeight = 0;

        foreach (var entry in spawnPointEntries)
        {
            if (entry != null && entry.point != null)
            {
                // 检查该生成点附近的材料数量
                int nearbyMaterials = GetNearbyMaterialCount(entry.point.position, spawnPointProximityThreshold);
                if (nearbyMaterials >= maxMaterialsPerSpawnPoint)
                {
                    Debug.Log($"生成点 '{entry.name}' 附近已有 {nearbyMaterials} 个材料，达到上限 {maxMaterialsPerSpawnPoint}，跳过");
                    continue;
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
            Debug.LogWarning("所有生成点附近材料已满或障碍物过多，暂时无法使用生成点");
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
    /// 计算指定位置附近指定距离内的材料数量
    /// </summary>
    int GetNearbyMaterialCount(Vector3 position, float threshold)
    {
        int count = 0;
        foreach (var kvp in spawnPointToMaterial)
        {
            if (kvp.Value != null)
            {
                float distance = Vector3.Distance(position, kvp.Value.transform.position);
                if (distance <= threshold)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// 检查生成点是否可用
    /// </summary>
    private bool IsSpawnPointAvailable(Vector3 position)
    {
        // 检查附近是否有障碍物
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, spawnPointProximityThreshold, obstacleLayer);
        return colliders.Length < 3; // 附近障碍物数量小于3视为可用
    }

    /// <summary>
    /// 清理已销毁的材料映射
    /// </summary>
    private void CleanUpDestroyedMaterials()
    {
        // 创建一个临时列表来存储需要移除的键
        List<Transform> keysToRemove = new List<Transform>();
        
        foreach (var kvp in spawnPointToMaterial)
        {
            if (kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        // 移除已销毁的材料映射
        foreach (var key in keysToRemove)
        {
            spawnPointToMaterial.Remove(key);
        }
    }

    /// <summary>
    /// 清理所有材料
    /// </summary>
    public void ClearMaterials()
    {
        foreach (GameObject material in currentMaterials)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
        currentMaterials.Clear();
        spawnPointToMaterial.Clear(); // 清理生成点与材料的映射
        Debug.Log("所有材料已清理");
    }

    /// <summary>
    /// 获取当前材料数量
    /// </summary>
    public int GetCurrentMaterialCount()
    {
        // 清理已销毁的材料
        currentMaterials.RemoveAll(material => material == null);
        // 清理已销毁的材料映射
        CleanUpDestroyedMaterials();
        return currentMaterials.Count;
    }

    /// <summary>
    /// 手动生成材料
    /// </summary>
    public void SpawnMaterial()
    {
        SpawnMaterialAtRandomPosition();
    }
}
