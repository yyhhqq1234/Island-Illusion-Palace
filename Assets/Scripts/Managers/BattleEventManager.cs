using UnityEngine;
using System.Collections;

[System.Obsolete("请使用 GlobalEventManager 替代 BattleEventManager。此模块保留向后兼容，将在后续版本移除。")]
public class BattleEventManager : MonoBehaviour
{
    public static BattleEventManager Instance { get; private set; }

    // 事件定义
    public System.Action<GameObject> onEnemyDefeated;
    public System.Action<GameObject> onBossDefeated;
    public System.Action<GameObject, float> onDamageDealt;
    public System.Action<GameObject, float> onDamageTaken;
    public System.Action onPlayerDeath;
    public System.Action onBossEncounter;
    public System.Action onBattleStart;
    public System.Action onBattleEnd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // 触发敌人被击败事件
    public void TriggerEnemyDefeated(GameObject enemy)
    {
        if (enemy == null) return;
        onEnemyDefeated?.Invoke(enemy);
    }

    // 触发玩家死亡事件
    public void TriggerPlayerDeath()
    {
        onPlayerDeath?.Invoke();
    }

    // 触发BOSS遭遇事件
    public void TriggerBossEncounter()
    {
        onBossEncounter?.Invoke();
    }

    // 触发战斗开始事件
    public void TriggerBattleStart()
    {
        onBattleStart?.Invoke();
    }

    // 触发战斗结束事件
    public void TriggerBattleEnd()
    {
        onBattleEnd?.Invoke();
    }
    
    // 触发BOSS被击败事件
    public void TriggerBossDefeated(GameObject boss)
    {
        if (boss == null) return;
        onBossDefeated?.Invoke(boss);
    }
    
    // 触发造成伤害事件
    public void TriggerDamageDealt(GameObject attacker, float damage)
    {
        if (attacker == null || damage <= 0) return;
        onDamageDealt?.Invoke(attacker, damage);
    }
    
    // 触发受到伤害事件
    public void TriggerDamageTaken(GameObject victim, float damage)
    {
        if (victim == null || damage <= 0) return;
        onDamageTaken?.Invoke(victim, damage);
    }
}