using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameSystems
{
    public enum MapMusicType
    {
        // 自然地图
        Forest,
        Wasteland,
        Desert,
        RockyLand,
        Wetland,
        IcePlains,
        VolcanicLand,

        // 人文地图
        RuinedCity,
        ForgottenManor,
        AncientTemple,

        // 特殊区域
        LabFragment,
        MemoryFragmentArea,

        // 最终区域
        TruthTemporalCorridor,

        // 全局
        MainMenu,
        Ending
    }

    public enum BurdenLevel
    {
        Low,
        Medium,
        High
    }

    [Serializable]
    public class MapMusicConfig
    {
        public MapMusicType mapType;
        public string mapName = "";

        [Header("安全区音乐")]
        public AudioClip safeZoneMusic;

        [Header("探索音乐")]
        public AudioClip explorationMusicLowBurden;
        public AudioClip explorationMusicMediumBurden;
        public AudioClip explorationMusicHighBurden;

        [Header("战斗音乐")]
        public AudioClip battleMusic;

        [Header("Boss战斗音乐")]
        public AudioClip bossBattleMusic;

        [Header("特殊区域标记")]
        public bool isSpecialArea = false;
        public bool isFinalArea = false;
    }
}

public class GameplayAudioManager : MonoBehaviour
{
    private static GameplayAudioManager _instance;
    public static GameplayAudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameplayAudioManager>();
                if (_instance == null)
                {
                    var go = new GameObject("[IIP] GameplayAudioManager");
                    _instance = go.AddComponent<GameplayAudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("=== 音频源组件 ===")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("=== 音量设置 ===")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;

    [Header("=== 全局音乐 ===")]
    public AudioClip mainMenuMusic;
    public AudioClip endingMusic;

    [Header("=== 默认音乐（当地图未配置时使用） ===")]
    public AudioClip defaultExplorationLowBurden;
    public AudioClip defaultExplorationMediumBurden;
    public AudioClip defaultExplorationHighBurden;
    public AudioClip defaultBattleMusic;
    public AudioClip defaultBossBattleMusic;
    public AudioClip defaultSafeZoneMusic;

    [Header("=== 地图音乐配置 ===")]
    public List<GameSystems.MapMusicConfig> mapMusicConfigs = new List<GameSystems.MapMusicConfig>();

    [Header("=== 战斗音效 ===")]
    public AudioClip playerAttackSound;
    public AudioClip playerDeathSound;
    public AudioClip enemyHitSound;

    [Header("召唤音效")]
    public AudioClip summonStartSound;
    public AudioClip summonAppearSound;
    public AudioClip summonAttackSound;
    public AudioClip summonDisappearSound;

    [Header("炼金音效")]
    public AudioClip alchemySuccessSound;
    public AudioClip alchemyFailSound;

    [Header("UI音效")]
    public AudioClip clickSound;
    public AudioClip confirmSound;
    public AudioClip selectSound;

    private AudioClip currentMusic;
    private string currentSceneName = "";
    private GameSystems.MapMusicType currentMapType = GameSystems.MapMusicType.MainMenu;
    private GameSystems.BurdenLevel currentBurdenLevel = GameSystems.BurdenLevel.Low;
    private GlobalEventManager.MusicState currentMusicState = GlobalEventManager.MusicState.MainMenu;
    private float musicFadeTime = 1f;
    private Coroutine musicFadeCoroutine;

    // ═══════════════════════════════════════════
    // 音乐状态优先级系统
    // ═══════════════════════════════════════════

    [Header("=== 优先级系统 ===")]
    [Tooltip("是否启用音乐状态优先级控制（推荐开启）")]
    public bool enablePrioritySystem = true;

    /// <summary>
    /// 音乐状态优先级 — 数值越大优先级越高
    /// BossBattle 和 Camp(安全区) 同级最高，Battle 次之，Exploration 为基础态
    /// </summary>
    private System.Collections.Generic.Dictionary<GlobalEventManager.MusicState, int> statePriority = new System.Collections.Generic.Dictionary<GlobalEventManager.MusicState, int>
    {
        { GlobalEventManager.MusicState.Silence, 0 },
        { GlobalEventManager.MusicState.MainMenu, 10 },
        { GlobalEventManager.MusicState.Exploration, 20 },
        { GlobalEventManager.MusicState.Battle, 50 },
        { GlobalEventManager.MusicState.BossBattle, 100 },
        { GlobalEventManager.MusicState.Camp, 100 },
    };

    /// <summary>当前活跃的音乐状态集合（可能同时有多个，取最高优先级播放）</summary>
    private System.Collections.Generic.HashSet<GlobalEventManager.MusicState> activeMusicStates = new System.Collections.Generic.HashSet<GlobalEventManager.MusicState>();

    /// <summary>基础状态（Exploration或MainMenu），当所有高层状态都退出时回退到此</summary>
    private GlobalEventManager.MusicState baseMusicState = GlobalEventManager.MusicState.Exploration;

    /// <summary>
    /// 进入一个音乐状态（加入活跃集合，可能触发音乐切换）
    /// 高优先级会覆盖低优先级；同优先级直接替换
    /// </summary>
    void EnterMusicState(GlobalEventManager.MusicState state)
    {
        if (!enablePrioritySystem)
        {
            DirectPlayMusicState(state);
            return;
        }

        activeMusicStates.Add(state);
        UpdateMusicByPriority();
        Debug.Log($"[AudioManager] 进入状态: {state} | 当前活跃: {activeMusicStates.Count}个 | 播放: {currentMusicState}");
    }

    /// <summary>
    /// 退出一个音乐状态（从活跃集合移除，回退到次高优先级）
    /// </summary>
    void ExitMusicState(GlobalEventManager.MusicState state)
    {
        if (!enablePrioritySystem)
        {
            // 降级模式：退出Battle/Boss/Camp时直接回到Exploration
            if (state == GlobalEventManager.MusicState.Battle ||
                state == GlobalEventManager.MusicState.BossBattle ||
                state == GlobalEventManager.MusicState.Camp)
            {
                DirectPlayMusicState(GlobalEventManager.MusicState.Exploration);
            }
            return;
        }

        if (activeMusicStates.Remove(state))
        {
            UpdateMusicByPriority();
            Debug.Log($"[AudioManager] 退出状态: {state} | 当前活跃: {activeMusicStates.Count}个 | 播放: {currentMusicState}");
        }
    }

    /// <summary>
    /// 根据活跃状态集合中最高优先级的状态播放音乐
    /// </summary>
    void UpdateMusicByPriority()
    {
        GlobalEventManager.MusicState highestState = baseMusicState;
        int highestPriority = -1;

        foreach (var state in activeMusicStates)
        {
            int priority = statePriority.TryGetValue(state, out int p) ? p : 0;
            if (priority > highestPriority)
            {
                highestPriority = priority;
                highestState = state;
            }
            else if (priority == highestPriority && priority == 100)
            {
                // 同级最高（BossBattle和Camp）：后者覆盖前者（游戏逻辑上不会同时出现）
                highestState = state;
            }
        }

        if (highestState != currentMusicState)
        {
            currentMusicState = highestState;
            AudioClip target = GetTargetMusic(highestState);
            if (highestState == GlobalEventManager.MusicState.Silence) StopMusic();
            else PlayMusic(target, true);
            Debug.Log($"[AudioManager] 优先级切换 → {highestState} (Map: {currentMapType})");
        }
    }

    /// <summary>设置基础音乐状态（当无活跃高层状态时播放）</summary>
    void SetBaseMusicState(GlobalEventManager.MusicState state)
    {
        baseMusicState = state;
        // 如果当前没有更高优先级的活跃状态，立即切换
        bool hasHigherActive = false;
        int basePriority = statePriority.TryGetValue(state, out int bp) ? bp : 0;
        foreach (var s in activeMusicStates)
        {
            if (statePriority.TryGetValue(s, out int sp) && sp > basePriority)
            {
                hasHigherActive = true;
                break;
            }
        }
        if (!hasHigherActive)
        {
            currentMusicState = state;
            AudioClip target = GetTargetMusic(state);
            if (state == GlobalEventManager.MusicState.Silence) StopMusic();
            else PlayMusic(target, true);
        }
    }

    /// <summary>直接播放指定状态音乐（兼容旧模式，绕过优先级）</summary>
    void DirectPlayMusicState(GlobalEventManager.MusicState state)
    {
        currentMusicState = state;
        AudioClip target = GetTargetMusic(state);
        if (state == GlobalEventManager.MusicState.Silence) StopMusic();
        else PlayMusic(target, true);
        Debug.Log($"[AudioManager] 直接切换 → {state} (Map: {currentMapType})");
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            InitializeMapType();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 确保场景加载后能正确播放音乐
        RefreshMusicForCurrentState();
    }

    void InitializeMapType()
    {
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentMapType = GetMapTypeFromScene(currentSceneName);
        
        if (currentMapType == GameSystems.MapMusicType.MainMenu)
        {
            baseMusicState = GlobalEventManager.MusicState.MainMenu;
            currentMusicState = GlobalEventManager.MusicState.Silence;
        }
        
        Debug.Log($"[AudioManager] 初始化地图类型: {currentMapType}, 基础状态: {baseMusicState}");
    }

    void OnEnable()
    {
        var e = GlobalEventManager.Instance;
        e.OnSceneDidLoad += OnSceneChanged;
        e.OnMapTypeChanged += OnMapTypeChanged;
        e.OnMusicStateChange += OnMusicStateChange;
        e.OnBurdenChanged += OnBurdenChanged;
        e.OnPlayerEnterSafeZone += OnPlayerEnterSafeZoneHandler;
        e.OnPlayerExitSafeZone += OnPlayerExitSafeZoneHandler;
        e.OnBattleStart += OnBattleStart;
        e.OnBattleEnd += OnBattleEnd;
        e.OnBossEncounter += OnBossEncounter;
        e.OnBossDefeated += OnBossDefeated;
        e.OnAudioRequested += OnAudioRequested;
    }

    void OnDisable()
    {
        var e = GlobalEventManager.Instance;
        if (e == null) return;
        e.OnAudioRequested -= OnAudioRequested;
        e.OnSceneDidLoad -= OnSceneChanged;
        e.OnMapTypeChanged -= OnMapTypeChanged;
        e.OnMusicStateChange -= OnMusicStateChange;
        e.OnBurdenChanged -= OnBurdenChanged;
        e.OnPlayerEnterSafeZone -= OnPlayerEnterSafeZoneHandler;
        e.OnPlayerExitSafeZone -= OnPlayerExitSafeZoneHandler;
        e.OnBattleStart -= OnBattleStart;
        e.OnBattleEnd -= OnBattleEnd;
        e.OnBossEncounter -= OnBossEncounter;
        e.OnBossDefeated -= OnBossDefeated;
    }

    void OnPlayerEnterSafeZoneHandler(GameObject _) =>
        EnterMusicState(GlobalEventManager.MusicState.Camp);

    void OnPlayerExitSafeZoneHandler(GameObject _) =>
        ExitMusicState(GlobalEventManager.MusicState.Camp);

    void OnMapTypeChanged(GameSystems.MapMusicType mapType)
    {
        if (mapType != currentMapType)
        {
            currentMapType = mapType;
            Debug.Log($"[AudioManager] 接收地图类型广播: {mapType}");
            // 地图类型变化时重置所有活跃状态，切换到新的探索音乐
            if (enablePrioritySystem)
            {
                activeMusicStates.Clear();
                SetBaseMusicState(GlobalEventManager.MusicState.Exploration);
            }
            else
            {
                RefreshMusicForCurrentState();
            }
        }
    }

    void OnSceneChanged(string sceneName)
    {
        currentSceneName = sceneName;
        GameSystems.MapMusicType mapType = GetMapTypeFromScene(sceneName);
        if (mapType != currentMapType)
        {
            currentMapType = mapType;
        }

        // 场景切换时重置所有活跃高层状态
        if (enablePrioritySystem)
        {
            activeMusicStates.Clear();
            GlobalEventManager.MusicState baseState =
                (mapType == GameSystems.MapMusicType.MainMenu)
                    ? GlobalEventManager.MusicState.MainMenu
                    : GlobalEventManager.MusicState.Exploration;
            SetBaseMusicState(baseState);
        }
        else
        {
            RefreshMusicForCurrentState();
        }
    }

    void OnBurdenChanged(float burdenValue)
    {
        GameSystems.BurdenLevel newBurdenLevel = CalculateBurdenLevel(burdenValue);
        if (newBurdenLevel != currentBurdenLevel)
        {
            currentBurdenLevel = newBurdenLevel;
            if (currentMusicState == GlobalEventManager.MusicState.Exploration)
            {
                PlayExplorationMusicForCurrentMap();
            }
        }
    }

    GameSystems.BurdenLevel CalculateBurdenLevel(float burden)
    {
        if (burden <= 30f) return GameSystems.BurdenLevel.Low;
        if (burden <= 60f) return GameSystems.BurdenLevel.Medium;
        return GameSystems.BurdenLevel.High;
    }

    void OnBattleStart(GameObject enemy)
    {
        EnterMusicState(GlobalEventManager.MusicState.Battle);
    }

    void OnBossEncounter(GameObject boss)
    {
        EnterMusicState(GlobalEventManager.MusicState.BossBattle);
    }

    void OnBossDefeated(GameObject boss)
    {
        ExitMusicState(GlobalEventManager.MusicState.BossBattle);
        Debug.Log($"[AudioManager] Boss被击败，退出Boss战音乐状态");
    }

    void OnBattleEnd(GameObject enemy)
    {
        ExitMusicState(GlobalEventManager.MusicState.Battle);
    }

    void OnMusicStateChange(GlobalEventManager.MusicState state)
    {
        if (!enablePrioritySystem)
        {
            DirectPlayMusicState(state);
            return;
        }

        // 根据状态类型选择处理方式
        switch (state)
        {
            case GlobalEventManager.MusicState.Exploration:
            case GlobalEventManager.MusicState.MainMenu:
                // 基础状态：设置为底层，被高层状态压制时不立即切换
                SetBaseMusicState(state);
                break;

            case GlobalEventManager.MusicState.Battle:
            case GlobalEventManager.MusicState.BossBattle:
            case GlobalEventManager.MusicState.Camp:
                // 高层状态：进入该状态
                EnterMusicState(state);
                break;

            case GlobalEventManager.MusicState.Silence:
            default:
                // 静音和其他：直接切换（清空所有活跃状态）
                activeMusicStates.Clear();
                DirectPlayMusicState(state);
                break;
        }
    }

    AudioClip GetTargetMusic(GlobalEventManager.MusicState state)
    {
        switch (state)
        {
            case GlobalEventManager.MusicState.MainMenu:
                return mainMenuMusic;

            case GlobalEventManager.MusicState.Exploration:
                return GetExplorationMusicForCurrentMap();

            case GlobalEventManager.MusicState.Battle:
                return GetBattleMusicForCurrentMap();

            case GlobalEventManager.MusicState.BossBattle:
                return GetBossBattleMusicForCurrentMap();

            case GlobalEventManager.MusicState.Camp:
                return GetSafeZoneMusic();

            case GlobalEventManager.MusicState.Silence:
                return null;

            default:
                return defaultExplorationLowBurden;
        }
    }

    AudioClip GetSafeZoneMusic()
    {
        var config = GetMapConfig(currentMapType);
        if (config != null && config.safeZoneMusic != null)
            return config.safeZoneMusic;
        return defaultSafeZoneMusic;
    }

    AudioClip GetExplorationMusicForCurrentMap()
    {
        var config = GetMapConfig(currentMapType);
        
        if (config != null && config.isSpecialArea)
        {
            return config.explorationMusicLowBurden ?? defaultExplorationLowBurden;
        }

        if (config != null)
        {
            return currentBurdenLevel switch
            {
                GameSystems.BurdenLevel.Low => config.explorationMusicLowBurden ?? defaultExplorationLowBurden,
                GameSystems.BurdenLevel.Medium => config.explorationMusicMediumBurden ?? defaultExplorationMediumBurden,
                GameSystems.BurdenLevel.High => config.explorationMusicHighBurden ?? defaultExplorationHighBurden,
                _ => config.explorationMusicLowBurden ?? defaultExplorationLowBurden
            };
        }

        return currentBurdenLevel switch
        {
            GameSystems.BurdenLevel.Low => defaultExplorationLowBurden,
            GameSystems.BurdenLevel.Medium => defaultExplorationMediumBurden,
            GameSystems.BurdenLevel.High => defaultExplorationHighBurden,
            _ => defaultExplorationLowBurden
        };
    }

    AudioClip GetBattleMusicForCurrentMap()
    {
        var config = GetMapConfig(currentMapType);
        if (config != null && config.battleMusic != null)
            return config.battleMusic;
        return defaultBattleMusic;
    }

    AudioClip GetBossBattleMusicForCurrentMap()
    {
        var config = GetMapConfig(currentMapType);
        if (config != null && config.bossBattleMusic != null)
            return config.bossBattleMusic;
        if (defaultBossBattleMusic != null)
            return defaultBossBattleMusic;
        // 降级：无 Boss 战斗曲时回退到普通战斗曲
        Debug.LogWarning($"[AudioManager] Boss战斗曲未配置（Map:{currentMapType}），降级使用普通战斗曲");
        return GetBattleMusicForCurrentMap();
    }

    GameSystems.MapMusicConfig GetMapConfig(GameSystems.MapMusicType mapType)
    {
        return mapMusicConfigs.Find(c => c.mapType == mapType);
    }

    void PlayExplorationMusicForCurrentMap()
    {
        PlayMusic(GetExplorationMusicForCurrentMap(), true);
    }

    void RefreshMusicForCurrentState()
    {
        if (enablePrioritySystem)
        {
            // 优先级模式下重新计算最高优先级状态
            UpdateMusicByPriority();
        }
        else
        {
            PlayMusic(GetTargetMusic(currentMusicState), true);
        }
    }

    GameSystems.MapMusicType GetMapTypeFromScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[AudioManager] 场景名称为空，使用默认地图类型");
            return GameSystems.MapMusicType.MainMenu;
        }

        string lowerName = sceneName.ToLower();

        // 自然地图
        if (lowerName.Contains("forest") || lowerName.Contains("森林")) 
            return GameSystems.MapMusicType.Forest;
        if (lowerName.Contains("wasteland") || lowerName.Contains("荒原")) 
            return GameSystems.MapMusicType.Wasteland;
        if (lowerName.Contains("desert") || lowerName.Contains("沙漠")) 
            return GameSystems.MapMusicType.Desert;
        if (lowerName.Contains("rocky") || lowerName.Contains("岩地")) 
            return GameSystems.MapMusicType.RockyLand;
        if (lowerName.Contains("wetland") || lowerName.Contains("湿地") || lowerName.Contains("沼泽")) 
            return GameSystems.MapMusicType.Wetland;
        if (lowerName.Contains("ice") || lowerName.Contains("冰原")) 
            return GameSystems.MapMusicType.IcePlains;
        if (lowerName.Contains("volcanic") || lowerName.Contains("火山")) 
            return GameSystems.MapMusicType.VolcanicLand;

        // 人文地图
        if (lowerName.Contains("ruined") || lowerName.Contains("废墟")) 
            return GameSystems.MapMusicType.RuinedCity;
        if (lowerName.Contains("manor") || lowerName.Contains("遗忘庄园")) 
            return GameSystems.MapMusicType.ForgottenManor;
        if (lowerName.Contains("temple") || lowerName.Contains("神殿")) 
            return GameSystems.MapMusicType.AncientTemple;

        // 特殊区域
        if (lowerName.Contains("lab") || lowerName.Contains("实验室")) 
            return GameSystems.MapMusicType.LabFragment;
        if (lowerName.Contains("memory") || lowerName.Contains("记忆碎片")) 
            return GameSystems.MapMusicType.MemoryFragmentArea;

        // 最终区域
        if (lowerName.Contains("truth") || lowerName.Contains("真理") || lowerName.Contains("时空回廊")) 
            return GameSystems.MapMusicType.TruthTemporalCorridor;

        // 主界面
        if (lowerName.Contains("mainmenu") || lowerName.Contains("menu") || lowerName.Contains("start") || lowerName.Contains("title"))
            return GameSystems.MapMusicType.MainMenu;

        Debug.LogWarning($"[AudioManager] 未识别的场景: {sceneName}，使用默认地图类型");
        return GameSystems.MapMusicType.MainMenu;
    }

    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        if (musicSource == null || clip == null || clip == currentMusic) return;
        musicFadeTime = Mathf.Max(0.1f, musicFadeTime);
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        if (fade && musicSource.isPlaying) musicFadeCoroutine = StartCoroutine(FadeMusic(clip));
        else { currentMusic = clip; musicSource.clip = clip; musicSource.Play(); }
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
        currentMusic = null;
    }

    public void PlaySFX(AudioClip clip) { if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip, sfxVolume * masterVolume); }
    public void PlayUISound(AudioClip clip) { if (uiSource != null && clip != null) uiSource.PlayOneShot(clip, uiVolume * masterVolume); }

    public void PlayExplorationMusic(int track = 1)
    {
        GameSystems.BurdenLevel burden = track switch { 2 => GameSystems.BurdenLevel.Medium, 3 => GameSystems.BurdenLevel.High, _ => GameSystems.BurdenLevel.Low };
        if (currentBurdenLevel != burden)
        {
            currentBurdenLevel = burden;
        }
        // 优先级模式下设置为基础状态；否则直接播放
        if (enablePrioritySystem)
            SetBaseMusicState(GlobalEventManager.MusicState.Exploration);
        else
            PlayMusic(GetExplorationMusicForCurrentMap(), true);
    }

    public void PlayBattleMusic(bool isBoss = false)
    {
        if (enablePrioritySystem)
        {
            if (isBoss)
                EnterMusicState(GlobalEventManager.MusicState.BossBattle);
            else
                EnterMusicState(GlobalEventManager.MusicState.Battle);
        }
        else
        {
            if (isBoss)
                PlayMusic(GetBossBattleMusicForCurrentMap(), true);
            else
                PlayMusic(GetBattleMusicForCurrentMap(), true);
        }
    }

    public void PlayCampMusic()
    {
        if (enablePrioritySystem)
            EnterMusicState(GlobalEventManager.MusicState.Camp);
        else
            PlayMusic(GetSafeZoneMusic(), true);
    }

    public void PlayMainMenuMusic()
    {
        if (enablePrioritySystem)
        {
            activeMusicStates.Clear();
            SetBaseMusicState(GlobalEventManager.MusicState.MainMenu);
        }
        else
            PlayMusic(mainMenuMusic, true);
    }

    public void PlayEndingMusic()
    {
        if (enablePrioritySystem)
        {
            activeMusicStates.Clear();
            DirectPlayMusicState(GlobalEventManager.MusicState.Silence); // 临时用静音替代，后续可加Ending状态
        }
        PlayMusic(endingMusic, true);
    }

    public void SetMapType(GameSystems.MapMusicType mapType)
    {
        GameSystems.MapMusicType oldType = currentMapType;
        if (currentMapType != mapType)
        {
            currentMapType = mapType;
            Debug.Log($"[AudioManager] 地图类型从 {oldType} 切换到 {currentMapType}");
            // 优先级模式下如果当前是基础状态则刷新音乐
            if (enablePrioritySystem)
                UpdateMusicByPriority();
            else
                RefreshMusicForCurrentState();
        }
    }

    /// <summary>
    /// 强制刷新地图类型检测并播放正确的音乐
    /// 可以在场景加载完成后或地图切换时调用
    /// </summary>
    public void RefreshMapMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameSystems.MapMusicType newMapType = GetMapTypeFromScene(sceneName);
        
        if (newMapType != currentMapType)
        {
            Debug.Log($"[AudioManager] 刷新地图音乐: 场景={sceneName}, 地图类型={newMapType}");
            currentMapType = newMapType;
            currentSceneName = sceneName;
            RefreshMusicForCurrentState();
        }
        else
        {
            Debug.Log($"[AudioManager] 地图类型未变化: {currentMapType}");
        }
    }

    /// <summary>
    /// 获取当前地图的音乐配置信息
    /// </summary>
    public string GetCurrentMapMusicInfo()
    {
        var config = GetMapConfig(currentMapType);
        if (config != null)
        {
            return $"地图: {config.mapName} ({currentMapType})\n" +
                   $"安全区: {(config.safeZoneMusic != null ? "有" : "无")}\n" +
                   $"探索曲(低): {(config.explorationMusicLowBurden != null ? "有" : "无")}\n" +
                   $"探索曲(中): {(config.explorationMusicMediumBurden != null ? "有" : "无")}\n" +
                   $"探索曲(高): {(config.explorationMusicHighBurden != null ? "有" : "无")}\n" +
                   $"战斗曲: {(config.battleMusic != null ? "有" : "无")}\n" +
                   $"Boss战: {(config.bossBattleMusic != null ? "有" : "无")}\n" +
                   $"特殊区域: {config.isSpecialArea}";
        }
        return $"地图: {currentMapType} (无配置)";
    }

    public void SetBurdenLevel(GameSystems.BurdenLevel burdenLevel)
    {
        if (currentBurdenLevel != burdenLevel)
        {
            currentBurdenLevel = burdenLevel;
            if (currentMusicState == GlobalEventManager.MusicState.Exploration)
            {
                PlayExplorationMusicForCurrentMap();
            }
        }
    }

    public GameSystems.MapMusicType GetCurrentMapType() => currentMapType;
    public GameSystems.BurdenLevel GetCurrentBurdenLevel() => currentBurdenLevel;

    public void PlayPlayerAttack() => PlaySFX(playerAttackSound);
    public void PlayPlayerDeath() => PlaySFX(playerDeathSound);
    public void PlayEnemyHit() => PlaySFX(enemyHitSound);
    public void PlaySummonStart() => PlaySFX(summonStartSound);
    public void PlaySummonAppear() => PlaySFX(summonAppearSound);
    public void PlaySummonAttack() => PlaySFX(summonAttackSound);
    public void PlaySummonDisappear() => PlaySFX(summonDisappearSound);
    public void PlayAlchemySuccess() => PlaySFX(alchemySuccessSound);
    public void PlayAlchemyFail() => PlaySFX(alchemyFailSound);
    public void PlayClick() => PlayUISound(clickSound);
    public void PlayConfirm() => PlayUISound(confirmSound);
    public void PlaySelect() => PlayUISound(selectSound);

    void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            var go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            var go = new GameObject("SFXSource");
            go.transform.SetParent(transform);
            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (uiSource == null)
        {
            var go = new GameObject("UISource");
            go.transform.SetParent(transform);
            uiSource = go.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
        }
        UpdateVolumes();
    }

    public void UpdateVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
        if (uiSource != null) uiSource.volume = uiVolume * masterVolume;
    }

    System.Collections.IEnumerator FadeMusic(AudioClip nextClip)
    {
        float startVol = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < musicFadeTime * 0.5f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (musicFadeTime * 0.5f));
            yield return null;
        }
        musicSource.Stop();
        currentMusic = nextClip;
        musicSource.clip = nextClip;
        musicSource.Play();
        elapsed = 0f;
        while (elapsed < musicFadeTime * 0.5f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, elapsed / (musicFadeTime * 0.5f));
            yield return null;
        }
        musicSource.volume = musicVolume * masterVolume;
    }

    // ═══════════════════════════════════════════
    // 事件驱动音效播放（替代反射调用）
    // ═══════════════════════════════════════════
    void OnAudioRequested(string methodName)
    {
        switch (methodName)
        {
            case "PlayPlayerDeath":
                // TODO: 实现玩家死亡音效
                Debug.Log("[AudioManager] 请求播放: PlayPlayerDeath");
                break;
            case "PlayAlchemySuccess":
                // TODO: 实现炼金成功音效
                Debug.Log("[AudioManager] 请求播放: PlayAlchemySuccess");
                break;
            case "PlayAlchemyFail":
                Debug.Log("[AudioManager] 请求播放: PlayAlchemyFail");
                break;
            case "PlayBossDefeat":
                // Boss被击败音效 — 暂用通知音替代
                Debug.Log("[AudioManager] Boss被击败！");
                break;
            default:
                Debug.LogWarning($"[AudioManager] 未知音频请求: {methodName}");
                break;
        }
    }
}
