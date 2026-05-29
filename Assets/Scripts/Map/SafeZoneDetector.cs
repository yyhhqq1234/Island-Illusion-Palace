using UnityEngine;
using System.Collections.Generic;

public class SafeZoneDetector : MonoBehaviour
{
    // 全局安全区检测器列表
    private static List<SafeZoneDetector> allSafeZones = new List<SafeZoneDetector>();
    
    [Header("安全区设置")]
    [Tooltip("碰撞体引用 - 用于检测玩家进入/离开")]
    public BoxCollider2D zoneCollider;
    
    [Tooltip("是否在玩家进入时显示日志")]
    public bool showDebugLogs = true;
    
    // 进入安全区的玩家数量
    private int playerCount = 0;
    
    // 安全区边界（缓存）
    private float leftBoundary;
    private float rightBoundary;
    private float bottomBoundary;
    private float topBoundary;
    
    // 移除本地事件，使用全局事件管理器

    private void Awake()
    {
        // 注册到全局列表
        if (!allSafeZones.Contains(this))
        {
            allSafeZones.Add(this);
        }
    }
    
    private void OnDestroy()
    {
        // 从全局列表移除
        allSafeZones.Remove(this);
    }
    
    private void Start()
    {
        // 检查是否设置了碰撞体引用
        if (zoneCollider != null)
        {
            // 确保碰撞体设置为触发器
            if (!zoneCollider.isTrigger)
            {
                zoneCollider.isTrigger = true;
                if (showDebugLogs)
                {
                    Debug.Log("[SafeZoneDetector] 将碰撞体设置为触发器");
                }
            }
            
            // 计算安全区边界
            CalculateBoundary();
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("[SafeZoneDetector] 未设置碰撞体引用，请在Inspector中指定BoxCollider2D");
            }
        }
    }
    
    void CalculateBoundary()
    {
        if (zoneCollider == null) return;
        
        Vector2 center = zoneCollider.offset;
        Vector2 size = zoneCollider.size;
        
        leftBoundary = transform.position.x + center.x - size.x / 2f;
        rightBoundary = transform.position.x + center.x + size.x / 2f;
        bottomBoundary = transform.position.y + center.y - size.y / 2f;
        topBoundary = transform.position.y + center.y + size.y / 2f;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测是否是玩家
        if (other.CompareTag("Player"))
        {
            playerCount++;
            
            if (showDebugLogs)
            {
                Debug.Log($"[SafeZoneDetector] 玩家进入安全区，当前玩家数量: {playerCount}");
            }
            
            // 触发全局进入安全区事件
            GlobalEventManager.Instance.TriggerPlayerEnterSafeZone(other.gameObject);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // 检测是否是玩家
        if (other.CompareTag("Player"))
        {
            playerCount = Mathf.Max(0, playerCount - 1);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SafeZoneDetector] 玩家离开安全区，当前玩家数量: {playerCount}");
            }
            
            // 触发全局离开安全区事件
            GlobalEventManager.Instance.TriggerPlayerExitSafeZone(other.gameObject);
        }
    }
    
    // 检查是否有玩家在安全区内
    public bool IsPlayerInSafeZone()
    {
        return playerCount > 0;
    }
    
    // 获取当前在安全区内的玩家数量
    public int GetPlayerCount()
    {
        return playerCount;
    }
    
    // 检查指定位置是否在这个安全区内
    public bool IsPositionInSafeZone(Vector2 position)
    {
        return position.x >= leftBoundary && position.x <= rightBoundary &&
               position.y >= bottomBoundary && position.y <= topBoundary;
    }
    
    // 检查指定位置是否在安全区内（全局）
    public static bool IsInAnySafeZone(Vector2 position)
    {
        foreach (var safeZone in allSafeZones)
        {
            if (safeZone.IsPositionInSafeZone(position))
            {
                return true;
            }
        }
        return false;
    }
    
    // 获取距离最近的安全区
    public static SafeZoneDetector GetNearestSafeZone(Vector2 position)
    {
        SafeZoneDetector nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var safeZone in allSafeZones)
        {
            float distance = Vector2.Distance(position, safeZone.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = safeZone;
            }
        }
        
        return nearest;
    }
    
    // 获取安全区边界（用于敌人避开）
    public static bool TryGetSafeZoneBoundary(Vector2 position, out Vector2 avoidDirection)
    {
        avoidDirection = Vector2.zero;
        
        foreach (var safeZone in allSafeZones)
        {
            if (safeZone.IsPositionInSafeZone(position))
            {
                // 计算离开安全区的方向
                Vector2 center = safeZone.transform.position;
                avoidDirection = (position - new Vector2(center.x, center.y)).normalized;
                return true;
            }
        }
        
        return false;
    }
    
    // 检查是否有任何玩家在安全区内
    public static bool IsAnyPlayerInSafeZone()
    {
        foreach (var safeZone in allSafeZones)
        {
            if (safeZone.IsPlayerInSafeZone())
            {
                return true;
            }
        }
        return false;
    }
}
