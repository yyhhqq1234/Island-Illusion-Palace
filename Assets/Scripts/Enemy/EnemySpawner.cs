using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敌人配置 - 包含预制体和权重
/// </summary>
[System.Serializable]
public class EnemyEntry
{
    [Tooltip("敌人预制体")]
    public GameObject prefab;

    [Tooltip("生成权重（越高越容易生成）")]
    public int weight = 1;

    [Tooltip("敌人名称（用于调试）")]
    public string name = "Enemy";
}

/// <summary>
/// 生成点配置 - 包含位置和权重
/// </summary>
[System.Serializable]
public class SpawnPointEntry
{
    [Tooltip("生成点位置")]
    public Transform point;

    [Tooltip("选择权重（越高越容易被选中）")]
    public int weight = 1;

    [Tooltip("生成点名称（用于调试）")]
    public string name = "SpawnPoint";
}

/// <summary>
/// 简化的敌人生成器 - 只保留最基本的功能
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("基本设置")]
    [Tooltip("是否启用自动生成")]
    public bool autoSpawn = true;
    [Tooltip("基础生成间隔时间（秒）")]
    public float baseSpawnInterval = 3f;
    [Tooltip("生成间隔波动范围（秒）")]
    public float spawnIntervalVariance = 1f;
    [Tooltip("最大敌人数量")]
    public int maxEnemyCount = 5;

    [Header("敌人设置")]
    [Tooltip("敌人配置列表")]
    public EnemyEntry[] enemyEntries;

    [Header("生成位置")]
    [Tooltip("生成点配置列表")]
    public SpawnPointEntry[] spawnPointEntries;

    [Tooltip("生成范围半径")]
    public float spawnRadius = 5f;
    [Tooltip("生成点附近距离阈值（检查敌人密度用）")]
    public float spawnPointProximityThreshold = 10f;
    [Tooltip("生成点附近最大敌人数量")]
    public int maxEnemiesPerSpawnPoint = 2;

    // 私有变量
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextSpawnTime = 0f; // 下次生成时间

    void Start()
    {
        // 计算第一次生成时间
        CalculateNextSpawnTime();
        Debug.Log("敌人生成器已初始化");
    }

    void Update()
    {
        // 清理死亡的敌人
        CleanDeadEnemies();

        // 自动生成逻辑
        if (autoSpawn && Time.time >= nextSpawnTime && ShouldSpawnEnemy())
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// 判断是否应该生成新敌人
    /// </summary>
    bool ShouldSpawnEnemy()
    {
        // 检查敌人数量上限
        if (activeEnemies.Count >= maxEnemyCount)
            return false;

        return true;
    }

    /// <summary>
    /// 生成敌人
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyEntries == null || enemyEntries.Length == 0)
        {
            Debug.LogWarning("没有设置敌人配置");
            return;
        }

        // 根据权重选择敌人类型
        EnemyEntry selectedEnemy = SelectEnemyByWeight();
        if (selectedEnemy == null || selectedEnemy.prefab == null)
        {
            Debug.LogWarning("选中的敌人配置或预制体为空");
            return;
        }

        // 计算生成位置
        Vector3 spawnPosition = GetSpawnPosition();

        // 如果所有生成点都被占用，暂时不生成敌人
        if (spawnPosition == transform.position && spawnPointEntries != null && spawnPointEntries.Length > 0)
        {
            // 重新计算下次生成时间，但稍微提前一点以便更快重试
            nextSpawnTime = Time.time + 1f;
            Debug.Log("所有生成点附近都有太多敌人，暂时跳过本次生成");
            return;
        }

        // 生成敌人
        GameObject newEnemy = Instantiate(selectedEnemy.prefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(newEnemy);

        // 计算下次生成时间
        CalculateNextSpawnTime();

        Debug.Log($"生成了敌人: {selectedEnemy.name} 在位置 {spawnPosition}");
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        if (spawnPointEntries != null && spawnPointEntries.Length > 0)
        {
            // 根据权重选择生成点
            SpawnPointEntry selectedPoint = SelectSpawnPointByWeight();

            if (selectedPoint != null && selectedPoint.point != null)
            {
                // 在选中的生成点周围随机生成
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
                return selectedPoint.point.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            }
        }

        // 如果没有设置生成点或选中的生成点为空，使用生成器自身位置
        return transform.position;
    }

    /// <summary>
    /// 清理死亡的敌人
    /// </summary>
    void CleanDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            // 检查敌人是否死亡
            HealthSystem health = activeEnemies[i].GetComponent<HealthSystem>();
            if (health != null && health.IsDead())
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 根据权重选择敌人
    /// </summary>
    EnemyEntry SelectEnemyByWeight()
    {
        if (enemyEntries == null || enemyEntries.Length == 0)
            return null;

        // 计算总权重
        int totalWeight = 0;
        foreach (var entry in enemyEntries)
        {
            if (entry != null && entry.prefab != null)
            {
                totalWeight += Mathf.Max(1, entry.weight);
            }
        }

        if (totalWeight <= 0)
        {
            // 如果总权重为0，使用简单随机选择
            return enemyEntries[UnityEngine.Random.Range(0, enemyEntries.Length)];
        }

        // 根据权重选择
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var entry in enemyEntries)
        {
            if (entry != null && entry.prefab != null)
            {
                currentWeight += Mathf.Max(1, entry.weight);
                if (randomValue < currentWeight)
                {
                    return entry;
                }
            }
        }

        // 兜底返回第一个有效条目
        foreach (var entry in enemyEntries)
        {
            if (entry != null && entry.prefab != null)
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>
    /// 根据权重选择生成点
    /// </summary>
    SpawnPointEntry SelectSpawnPointByWeight()
    {
        if (spawnPointEntries == null || spawnPointEntries.Length == 0)
            return null;

        // 创建可用生成点列表（排除附近敌人过多的生成点）
        List<SpawnPointEntry> availablePoints = new List<SpawnPointEntry>();
        int totalWeight = 0;

        foreach (var entry in spawnPointEntries)
        {
            if (entry != null && entry.point != null)
            {
                // 检查附近敌人数量
                int nearbyEnemies = GetNearbyEnemyCount(entry.point.position, spawnPointProximityThreshold);
                if (nearbyEnemies < maxEnemiesPerSpawnPoint)
                {
                    availablePoints.Add(entry);
                    totalWeight += Mathf.Max(1, entry.weight);
                }
                else
                {
                    Debug.Log($"生成点 '{entry.name}' 附近已有 {nearbyEnemies} 个敌人，暂时跳过");
                }
            }
        }

        // 如果没有可用生成点，返回null
        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("所有生成点附近都有太多敌人，暂时无法生成新敌人");
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
    /// 计算指定位置附近的敌人数量
    /// </summary>
    /// <param name="position">检查位置</param>
    /// <param name="threshold">距离阈值</param>
    /// <returns>附近敌人数量</returns>
    int GetNearbyEnemyCount(Vector3 position, float threshold)
    {
        int count = 0;
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance <= threshold)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// 计算下次生成时间（带有随机波动）
    /// </summary>
    void CalculateNextSpawnTime()
    {
        // 确保参数有效
        float safeBaseInterval = Mathf.Max(0.1f, baseSpawnInterval);
        float safeVariance = Mathf.Max(0f, spawnIntervalVariance);

        // 在基础间隔±波动范围内随机，但确保不会小于0.1秒
        float minInterval = Mathf.Max(0.1f, safeBaseInterval - safeVariance);
        float maxInterval = safeBaseInterval + safeVariance;

        float actualInterval = UnityEngine.Random.Range(minInterval, maxInterval);

        nextSpawnTime = Time.time + actualInterval;
    }
}