using UnityEngine;
using System;

/// <summary>
/// 负担等级枚举（供事件驱动架构使用）
/// </summary>
public enum GlobalBurdenLevel { Normal, High, Critical }

/// <summary>
/// 全局事件管理器 — Singleton + DontDestroyOnLoad
/// 所有跨场景事件的总线。任何模块通过 Instance 订阅/触发事件。
/// </summary>
public class GlobalEventManager : MonoBehaviour
{
    private static GlobalEventManager _instance;
    private static bool isQuitting = false;

    public static GlobalEventManager Instance
    {
        get
        {
            if (isQuitting) return null;
            if (_instance == null)
            {
                _instance = FindObjectOfType<GlobalEventManager>();
                if (_instance == null)
                {
                    var go = new GameObject("[IIP] GlobalEventManager");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _instance = go.AddComponent<GlobalEventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
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

    // 负担等级变化事件（替代各系统直接查询 BurdenSystem）
    public event Action<GlobalBurdenLevel> OnGlobalBurdenLevelChanged;

    public void TriggerBurdenChanged(float current) => OnBurdenChanged?.Invoke(current);
    public void TriggerGlobalBurdenLevelChanged(GlobalBurdenLevel level) => OnGlobalBurdenLevelChanged?.Invoke(level);

    private float _lastGlobalBurdenLevel = -1f;
    public void CheckBurdenThresholds(float currentBurden)
    {
        OnBurdenChanged?.Invoke(currentBurden);

        int level = currentBurden >= IIPConstants.BurdenCriticalThreshold ? 2
                  : currentBurden >= IIPConstants.BurdenHighThreshold      ? 1 : 0;

        if (level != _lastGlobalBurdenLevel)
        {
            if (level >= 2)      OnBurdenCritical?.Invoke();
            else if (level >= 1) OnBurdenHigh?.Invoke();
            else if (_lastGlobalBurdenLevel > 0) OnBurdenCleared?.Invoke();
        }
        _lastGlobalBurdenLevel = level;
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
    // HUD 数据事件（事件驱动 HUD — 替代轮询）
    // ═══════════════════════════════════════════

    // 生命值：(current, max)
    public event Action<float, float> OnPlayerHealthChanged;
    public void TriggerPlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // 魔法值：(current, max)
    public event Action<float, float> OnPlayerManaChanged;
    public void TriggerPlayerManaChanged(float current, float max) => OnPlayerManaChanged?.Invoke(current, max);

    // 等级/经验：(level, currentExp, expToNext)
    public event Action<int, int, int> OnPlayerLevelChanged;
    public void TriggerPlayerLevelChanged(int level, int currentExp, int expToNext) => OnPlayerLevelChanged?.Invoke(level, currentExp, expToNext);

    // 武器变化：(weaponType, enhancementLevel)
    public event Action<WeaponType, int> OnWeaponChanged;
    public void TriggerWeaponChanged(WeaponType weaponType, int enhancementLevel) => OnWeaponChanged?.Invoke(weaponType, enhancementLevel);

    // 背包/灵魂变化：(souls, soulEssence)
    public event Action<int, int> OnInventoryChanged;
    public void TriggerInventoryChanged(int souls, int soulEssence) => OnInventoryChanged?.Invoke(souls, soulEssence);

    // 快捷物品槽：(slotIndex, itemName, quantity, icon) — quantity<0 或 itemName 空 = 空槽
    public event Action<int, string, int, Sprite> OnQuickSlotChanged;
    public void TriggerQuickSlotChanged(int slotIndex, string itemName, int quantity, Sprite icon)
        => OnQuickSlotChanged?.Invoke(slotIndex, itemName, quantity, icon);

    // 闪避冷却：(remainingTime, totalTime) — total<=0 表示冷却结束
    public event Action<float, float> OnDashCooldownChanged;
    public void TriggerDashCooldownChanged(float remaining, float total) => OnDashCooldownChanged?.Invoke(remaining, total);

    // 召唤状态：(activeCount, maxActive, cooldownRemaining)
    public event Action<int, int, float> OnSummonStatusChanged;
    public void TriggerSummonStatusChanged(int activeCount, int maxActive, float cooldownRemaining)
        => OnSummonStatusChanged?.Invoke(activeCount, maxActive, cooldownRemaining);

    // 地图区域名变化：(areaName) — 玩家进入新区域时
    public event Action<string> OnMapAreaChanged;
    public void TriggerMapAreaChanged(string areaName) => OnMapAreaChanged?.Invoke(areaName);

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
    // 时空裂隙预览（P1-004）
    // ═══════════════════════════════════════════
    public event Action<MapType, MapType, float> OnRiftPreviewActivated;        // destA, destB, countdown
    public event Action<int> OnRiftPreviewHighlightChanged;                      // 0=A, 1=B
    public event Action<MapType> OnRiftPreviewClosed;                           // 选中的目的地
    public event Action<MapType> OnRiftPreviewTimeout;                          // 超时选中的目的地
    public event Action OnRiftPreviewCancelled;                                 // 取消预览

    public void TriggerRiftPreviewActivated(MapType destA, MapType destB, float countdown)
        => OnRiftPreviewActivated?.Invoke(destA, destB, countdown);
    public void TriggerRiftPreviewHighlightChanged(int option)
        => OnRiftPreviewHighlightChanged?.Invoke(option);
    public void TriggerRiftPreviewClosed(MapType selected)
        => OnRiftPreviewClosed?.Invoke(selected);
    public void TriggerRiftPreviewTimeout(MapType selected)
        => OnRiftPreviewTimeout?.Invoke(selected);
    public void TriggerRiftPreviewCancelled()
        => OnRiftPreviewCancelled?.Invoke();

    // ═══════════════════════════════════════════
    // 音效请求（替代反射调用 AudioManager）
    // ═══════════════════════════════════════════
    public event Action<string> OnAudioRequested;  // 方法名参数

    public void RequestAudio(string audioMethodName) => OnAudioRequested?.Invoke(audioMethodName);

    // ═══════════════════════════════════════════
    // 实体死亡（替代 HealthSystem 直接 GetComponent）
    // ═══════════════════════════════════════════
    public event Action<GameObject> OnEntityDied;

    public void TriggerEntityDied(GameObject entity) => OnEntityDied?.Invoke(entity);

    // ═══════════════════════════════════════════
    // 营火存档（P0）
    // ═══════════════════════════════════════════
    public event Action<Vector3> OnCampfireSaveRequested;  // 存档位置
    public event Action OnGameSaveCompleted;

    public void TriggerCampfireSaveRequested(Vector3 campfirePos)
        => OnCampfireSaveRequested?.Invoke(campfirePos);
    public void TriggerGameSaveCompleted()
        => OnGameSaveCompleted?.Invoke();

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
