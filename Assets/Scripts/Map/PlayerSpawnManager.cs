using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家生成点配置 - 包含位置和权重
/// </summary>
[System.Serializable]
public class PlayerSpawnPointEntry
{
    [Tooltip("生成点位置")]
    public Transform point;

    [Tooltip("选择权重（越高越容易被选中）")]
    public int weight = 1;

    [Tooltip("生成点名称（用于调试）")]
    public string name = "SpawnPoint";
}

/// <summary>
/// 玩家生成管理器 - 参考EnemySpawner的结构设计
/// </summary>
public class PlayerSpawnManager : MonoBehaviour
{
    [Header("基本设置")]
    [Tooltip("是否启用自动生成")]
    public bool autoSpawn = true;
    [Tooltip("玩家预制体")]
    public GameObject playerPrefab;
    [Tooltip("玩家生成父对象")]
    public Transform playerSpawnParent;

    [Header("安全区设置")]
    [Tooltip("安全区中心位置（39x26格子的中心）")]
    public Vector2 safeRoomCenter = new Vector2(19.5f, 13f);
    [Tooltip("生成范围半径")]
    public float spawnRadius = 5f;

    [Header("生成位置")]
    [Tooltip("生成点配置列表")]
    public PlayerSpawnPointEntry[] spawnPointEntries;

    [Header("生成参数")]
    [Tooltip("障碍物图层")]
    public LayerMask obstacleLayer;
    [Tooltip("最大生成尝试次数")]
    public int maxSpawnAttempts = 10;
    [Tooltip("生成点附近距离阈值（检查障碍物密度用）")]
    public float spawnPointProximityThreshold = 10f;

    // 私有变量
    private IntegratedMapSystem mapSystem;
    private GameObject currentPlayer;

    private void Start()
    {
        mapSystem = FindObjectOfType<IntegratedMapSystem>();
        
        // 如果启用自动生成，在开始时生成玩家
        if (autoSpawn)
        {
            SpawnPlayerInSafeRoom();
        }
        
        Debug.Log("玩家生成管理器已初始化");
    }

    /// <summary>
    /// 在安全区生成玩家
    /// </summary>
    public void SpawnPlayerInSafeRoom()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("玩家预制件未设置");
            return;
        }
        
        // 清理旧玩家
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }
        
        // 计算安全区世界位置
        Vector3 safeRoomWorldPos = GetSafeRoomWorldPosition();
        
        // 找到合适的生成位置
        Vector3 spawnPosition = GetSpawnPosition(safeRoomWorldPos);
        
        // 生成玩家
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        // 确保玩家对象处于启用状态
        if (currentPlayer != null)
        {
            currentPlayer.SetActive(true);
            // 确保玩家的所有子对象也处于启用状态
            foreach (Transform child in currentPlayer.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        
        if (playerSpawnParent != null)
        {
            currentPlayer.transform.parent = playerSpawnParent;
        }
        
        Debug.Log("玩家生成在安全区：" + spawnPosition);
    }

    /// <summary>
    /// 获取安全区的世界位置
    /// </summary>
    private Vector3 GetSafeRoomWorldPosition()
    {
        if (mapSystem == null)
        {
            mapSystem = FindObjectOfType<IntegratedMapSystem>();
        }
        
        // 从地图系统获取安全区位置
        if (mapSystem != null)
        {
            // 安全区在网格中心
            Vector2Int safeRoomGridPos = new Vector2Int(2, 2); // 5x5网格的中心
            
            // 调用地图系统的GetWorldPosition方法
            return mapSystem.GetWorldPosition(safeRoomGridPos);
        }
        
        // 如果没有地图系统，使用默认安全区中心
        return new Vector3(safeRoomCenter.x, safeRoomCenter.y, 0);
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition(Vector3 fallbackPosition)
    {
        if (spawnPointEntries != null && spawnPointEntries.Length > 0)
        {
            // 根据权重选择生成点
            PlayerSpawnPointEntry selectedPoint = SelectSpawnPointByWeight();

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

        // 如果没有设置生成点或选中的生成点无效，使用安全区中心附近的位置
        return FindValidSpawnPosition(fallbackPosition);
    }

    /// <summary>
    /// 找到有效的生成位置
    /// </summary>
    private Vector3 FindValidSpawnPosition(Vector3 center)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // 在安全区中心附近随机生成位置
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = center + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // 检查位置是否有效（没有障碍物）
            if (IsValidSpawnPosition(spawnPosition))
            {
                return spawnPosition;
            }
        }
        
        // 如果没有找到合适位置，返回中心位置
        return center;
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
    private PlayerSpawnPointEntry SelectSpawnPointByWeight()
    {
        if (spawnPointEntries == null || spawnPointEntries.Length == 0)
            return null;

        // 创建可用生成点列表（排除附近障碍物过多的生成点）
        List<PlayerSpawnPointEntry> availablePoints = new List<PlayerSpawnPointEntry>();
        int totalWeight = 0;

        foreach (var entry in spawnPointEntries)
        {
            if (entry != null && entry.point != null)
            {
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
            Debug.LogWarning("所有生成点附近都有障碍物，暂时无法使用生成点");
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
        return colliders.Length < 3; // 附近障碍物数量小于3视为可用
    }

    /// <summary>
    /// 将现有玩家传送到安全区生成点（不销毁，保留所有状态）
    /// </summary>
    public void MovePlayerToSafeRoom(GameObject player)
    {
        if (player == null) return;
        Vector3 safeRoomWorldPos = GetSafeRoomWorldPosition();
        Vector3 spawnPosition = GetSpawnPosition(safeRoomWorldPos);
        player.transform.position = spawnPosition;
        currentPlayer = player;
        Debug.Log($"[PlayerSpawnManager] 玩家移动到安全区: {spawnPosition}");
    }

    /// <summary>
    /// 重置玩家位置到安全区
    /// </summary>
    public void ResetPlayerToSafeRoom()
    {
        if (currentPlayer != null)
        {
            Vector3 safeRoomWorldPos = GetSafeRoomWorldPosition();
            Vector3 spawnPosition = GetSpawnPosition(safeRoomWorldPos);
            currentPlayer.transform.position = spawnPosition;
            
            Debug.Log("玩家重置到安全区：" + spawnPosition);
        }
        else
        {
            SpawnPlayerInSafeRoom();
        }
    }

    /// <summary>
    /// 获取当前玩家
    /// </summary>
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

    /// <summary>
    /// 手动生成玩家
    /// </summary>
    public void SpawnPlayer()
    {
        SpawnPlayerInSafeRoom();
    }

    /// <summary>
    /// 清理玩家
    /// </summary>
    public void ClearPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
            Debug.Log("玩家已清理");
        }
    }
}
