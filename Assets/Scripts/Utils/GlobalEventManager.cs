using UnityEngine;
using System;

public class GlobalEventManager : MonoBehaviour
{
    // 单例实例
    private static GlobalEventManager instance;
    
    // 获取单例实例
    public static GlobalEventManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 查找场景中是否已有GlobalEventManager
                instance = FindObjectOfType<GlobalEventManager>();
                
                // 如果没有，创建一个新的
                if (instance == null)
                {
                    GameObject go = new GameObject("GlobalEventManager");
                    instance = go.AddComponent<GlobalEventManager>();
                    // 设置为DontDestroyOnLoad，使其在场景切换时保持存在
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // 安全区事件
    public event Action<GameObject> OnPlayerEnterSafeZone;
    public event Action<GameObject> OnPlayerExitSafeZone;
    
    // 战斗事件
    public event Action<GameObject> OnBattleStart;
    public event Action<GameObject> OnBattleEnd;
    
    // 触发玩家进入安全区事件
    public void TriggerPlayerEnterSafeZone(GameObject player)
    {
        OnPlayerEnterSafeZone?.Invoke(player);
    }
    
    // 触发玩家离开安全区事件
    public void TriggerPlayerExitSafeZone(GameObject player)
    {
        OnPlayerExitSafeZone?.Invoke(player);
    }
    
    // 触发战斗开始事件
    public void TriggerBattleStart(GameObject enemy)
    {
        OnBattleStart?.Invoke(enemy);
    }
    
    // 触发战斗结束事件
    public void TriggerBattleEnd(GameObject enemy)
    {
        OnBattleEnd?.Invoke(enemy);
    }
    
    private void Awake()
    {
        // 确保只有一个实例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}