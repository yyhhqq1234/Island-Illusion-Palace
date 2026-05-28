using UnityEngine;

public class SafeZoneDetector : MonoBehaviour
{
    [Header("安全区设置")]
    [Tooltip("碰撞体引用 - 用于检测玩家进入/离开")]
    public BoxCollider2D zoneCollider;
    
    [Tooltip("是否在玩家进入时显示日志")]
    public bool showDebugLogs = true;
    
    // 进入安全区的玩家数量
    private int playerCount = 0;
    
    // 移除本地事件，使用全局事件管理器
    
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
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("[SafeZoneDetector] 未设置碰撞体引用，请在Inspector中指定BoxCollider2D");
            }
        }
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
}