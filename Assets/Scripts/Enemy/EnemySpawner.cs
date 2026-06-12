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
    public float baseSpawnInterval = 5f;
    [Tooltip("生成间隔波动范围（秒）")]
    public float spawnIntervalVariance = 2f;
    [Tooltip("最大敌人数量（0=由区域大小自动计算）")]
    public int maxEnemyCount = 0;

    [Header("敌人设置")]
    [Tooltip("敌人配置列表")]
    public EnemyEntry[] enemyEntries;

    [Header("生成位置")]
    [Tooltip("生成点配置列表")]
    public SpawnPointEntry[] spawnPointEntries;

    [Tooltip("生成范围半径")]
    public float spawnRadius = 5f;
    [Tooltip("生成点附近距离阈值（检查敌人密度用）")]
    public float spawnPointProximityThreshold = 15f;
    [Tooltip("生成点附近最大敌人数量")]
    public int maxEnemiesPerSpawnPoint = 1;

    [Header("战斗区域限制")]
    [Tooltip("瓦片地图宽度（格）")]
    public int tileWidth = 39;
    [Tooltip("瓦片地图高度（格）")]
    public int tileHeight = 26;
    [Tooltip("区域最大敌人数量上限")]
    public int maxEnemyCountByArea = 5;

    [Header("清空滞空")]
    [Tooltip("敌人清空后进入滞空状态的冷却时间（秒）")]
    public float clearZoneCooldown = 8f;

    // 私有变量
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextSpawnTime = 0f;
    private int cachedMaxEnemyCount = 0;
    private bool isInClearZoneCooldown = false;
    private float clearZoneCooldownTimer = 0f;

    void Start()
    {
        // 缓存最大敌人数量
        CalculateAndCacheEnemyCount();

        // 计算第一次生成时间
        CalculateNextSpawnTime();
        Debug.Log($"[EnemySpawner] 已初始化 | 最大敌人数={cachedMaxEnemyCount} | 生成间隔={baseSpawnInterval}s±{spawnIntervalVariance}s | 生成点阈值={spawnPointProximityThreshold}");
    }

    void Update()
    {
        // 清理死亡的敌人
        CleanDeadEnemies();

        // 滞空冷却计时
        if (isInClearZoneCooldown)
        {
            clearZoneCooldownTimer -= Time.deltaTime;
            if (clearZoneCooldownTimer <= 0f)
            {
                ExitClearZoneCooldown();
            }
            return;
        }

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
        // 获取当前有效的最大敌人数量
        int effectiveMax = GetEffectiveMaxEnemyCount();

        // 检查敌人数量上限
        if (activeEnemies.Count >= effectiveMax)
            return false;

        return true;
    }

    /// <summary>
    /// 获取当前有效的最大敌人数量（手动 > 自动计算 > 默认值）
    /// </summary>
    int GetEffectiveMaxEnemyCount()
    {
        if (maxEnemyCount > 0)
            return maxEnemyCount;
        return cachedMaxEnemyCount;
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
        int countBefore = activeEnemies.Count;

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

        // 检测清空：有敌人被清理且当前数量归零时进入滞空
        if (countBefore > 0 && activeEnemies.Count == 0 && !isInClearZoneCooldown)
        {
            EnterClearZoneCooldown();
        }
    }

    /// <summary>
    /// 进入清空滞空状态
    /// </summary>
    void EnterClearZoneCooldown()
    {
        isInClearZoneCooldown = true;
        clearZoneCooldownTimer = clearZoneCooldown;
        Debug.Log($"[EnemySpawner] 战斗区敌人已清空，进入滞空状态 {clearZoneCooldown}秒");
    }

    /// <summary>
    /// 退出清空滞空状态，恢复正常生成
    /// </summary>
    void ExitClearZoneCooldown()
    {
        isInClearZoneCooldown = false;
        clearZoneCooldownTimer = 0f;
        // 重置下次生成时间，避免立即生成
        CalculateNextSpawnTime();
        Debug.Log("[EnemySpawner] 滞空状态结束，恢复正常敌人生成");
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

    /// <summary>
    /// 缓存最大敌人数量（直接使用 maxEnemyCountByArea）
    /// </summary>
    void CalculateAndCacheEnemyCount()
    {
        cachedMaxEnemyCount = maxEnemyCountByArea;
        Debug.Log($"[EnemySpawner] 最大敌人数={cachedMaxEnemyCount} (直接上限)");
    }

    /// <summary>
    /// 获取当前区域最大敌人数量（供外部查询）
    /// </summary>
    public int GetAreaMaxEnemyCount()
    {
        return GetEffectiveMaxEnemyCount();
    }
}