using UnityEngine;
using System;

/// <summary>
/// 全局事件管理器 — Singleton + DontDestroyOnLoad
/// 所有跨场景事件的总线。任何模块通过 Instance 订阅/触发事件。
/// </summary>
public class GlobalEventManager : MonoBehaviour
{
    private static GlobalEventManager _instance;
    public static GlobalEventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GlobalEventManager>();
                if (_instance == null)
                {
                    var go = new GameObject("[IIP] GlobalEventManager");
                    _instance = go.AddComponent<GlobalEventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // ═══════════════════════════════════════════
    // 场景生命周期
    // ═══════════════════════════════════════════
    public event Action<string> OnSceneWillLoad;
    public event Action<string> OnSceneDidLoad;
    public event Action OnSceneWillUnload;

    public void TriggerSceneWillLoad(string sceneName) => OnSceneWillLoad?.Invoke(sceneName);
    public void TriggerSceneDidLoad(string sceneName)  => OnSceneDidLoad?.Invoke(sceneName);
    public void TriggerSceneWillUnload()                => OnSceneWillUnload?.Invoke();

    // ═══════════════════════════════════════════
    // 游戏状态
    // ═══════════════════════════════════════════
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameQuit;
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;

    public void TriggerGamePaused()    => OnGamePaused?.Invoke();
    public void TriggerGameResumed()   => OnGameResumed?.Invoke();
    public void TriggerGameQuit()      => OnGameQuit?.Invoke();
    public void TriggerPlayerDeath()   => OnPlayerDeath?.Invoke();
    public void TriggerPlayerRespawn() => OnPlayerRespawn?.Invoke();

    // ═══════════════════════════════════════════
    // 安全区
    // ═══════════════════════════════════════════
    public event Action<GameObject> OnPlayerEnterSafeZone;
    public event Action<GameObject> OnPlayerExitSafeZone;

    public void TriggerPlayerEnterSafeZone(GameObject player) => OnPlayerEnterSafeZone?.Invoke(player);
    public void TriggerPlayerExitSafeZone(GameObject player)  => OnPlayerExitSafeZone?.Invoke(player);

    // ═══════════════════════════════════════════
    // 战斗阶段
    // ═══════════════════════════════════════════
    public event Action<GameObject> OnBattleStart;
    public event Action<GameObject> OnBattleEnd;
    public event Action<GameObject> OnEnemyDefeated;
    public event Action<GameObject> OnBossDefeated;
    public event Action<GameObject> OnBossEncounter;
    public event Action<GameObject, float> OnDamageDealt;
    public event Action<GameObject, float> OnDamageTaken;

    public void TriggerBattleStart(GameObject enemy)     => OnBattleStart?.Invoke(enemy);
    public void TriggerBattleEnd(GameObject enemy)       => OnBattleEnd?.Invoke(enemy);
    public void TriggerEnemyDefeated(GameObject enemy)   => OnEnemyDefeated?.Invoke(enemy);
    public void TriggerBossDefeated(GameObject boss)     => OnBossDefeated?.Invoke(boss);
    public void TriggerBossEncounter(GameObject boss)    => OnBossEncounter?.Invoke(boss);
    public void TriggerDamageDealt(GameObject src, float dmg) => OnDamageDealt?.Invoke(src, dmg);
    public void TriggerDamageTaken(GameObject src, float dmg) => OnDamageTaken?.Invoke(src, dmg);

    // ═══════════════════════════════════════════
    // 负担系统
    // ═══════════════════════════════════════════
    public event Action<float> OnBurdenChanged;
    public event Action OnBurdenHigh;        // 超过 HighThreshold
    public event Action OnBurdenCritical;    // 超过 CriticalThreshold
    public event Action OnBurdenCleared;     // 营火清零

    public void TriggerBurdenChanged(float current) => OnBurdenChanged?.Invoke(current);

    private float _lastBurdenLevel = -1f;
    public void CheckBurdenThresholds(float currentBurden)
    {
        OnBurdenChanged?.Invoke(currentBurden);

        int level = currentBurden >= IIPConstants.BurdenCriticalThreshold ? 2
                  : currentBurden >= IIPConstants.BurdenHighThreshold      ? 1 : 0;

        if (level != _lastBurdenLevel)
        {
            if (level >= 2)      OnBurdenCritical?.Invoke();
            else if (level >= 1) OnBurdenHigh?.Invoke();
            else if (_lastBurdenLevel > 0) OnBurdenCleared?.Invoke();
        }
        _lastBurdenLevel = level;
    }

    // ═══════════════════════════════════════════
    // 地图类型广播
    // ═══════════════════════════════════════════
    public event Action<GameSystems.MapMusicType> OnMapTypeChanged;
    
    public void TriggerMapTypeChanged(GameSystems.MapMusicType mapType) 
    {
        OnMapTypeChanged?.Invoke(mapType);
        Debug.Log($"[GlobalEventManager] 地图类型广播: {mapType}");
    }

    // ═══════════════════════════════════════════
    // 音乐 / 音效
    // ═══════════════════════════════════════════
    public event Action<MusicState> OnMusicStateChange;
    public enum MusicState { MainMenu, Exploration, Battle, BossBattle, Camp, Silence }

    public void RequestMusicState(MusicState state) => OnMusicStateChange?.Invoke(state);

    // ═══════════════════════════════════════════
    // UI / 通知
    // ═══════════════════════════════════════════
    public event Action<string, float> OnNotification;  // message, duration

    public void ShowNotification(string msg, float duration = 3f) => OnNotification?.Invoke(msg, duration);

    // ═══════════════════════════════════════════
    // Singleton 生命周期
    // ═══════════════════════════════════════════
    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
