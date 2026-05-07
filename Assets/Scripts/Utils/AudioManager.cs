using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 游戏音效管理器 - 单例模式
/// 负责管理所有游戏音效和背景音乐
/// </summary>
public class GameplayAudioManager : MonoBehaviour
{
    public static GameplayAudioManager Instance { get; private set; }

    [Header("=== 音频源组件 ===")]
    [Tooltip("背景音乐源 - 负责播放场景背景音乐")]
    public AudioSource musicSource;
    
    [Tooltip("音效源 - 负责播放战斗、召唤等音效")]
    public AudioSource sfxSource;
    
    [Tooltip("UI音效源 - 负责播放UI相关音效")]
    public AudioSource uiSource;

    [Header("=== 音量设置 ===")]
    [Tooltip("主音量控制 - 影响所有音频")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [Tooltip("背景音乐音量 - 影响场景背景音乐")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    
    [Tooltip("音效音量 - 影响战斗、召唤等音效")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Tooltip("UI音效音量 - 影响UI相关音效")]
    [Range(0f, 1f)]
    public float uiVolume = 1f;

    [Header("=== 场景音乐配置 ===")]
    [Header("主界面场景音乐")]
    [Tooltip("主界面主题曲 - 仅在主界面场景播放")]
    public AudioClip mainTheme;
    
    [Header("游戏场景音乐")]
    [Tooltip("战斗音乐 - 战斗时播放")]
    public AudioClip battleMusic;
    
    [Tooltip("Boss战斗音乐 - Boss战斗时播放")]
    public AudioClip bossBattleMusic;
    
    [Tooltip("探索音乐1 - 游戏场景默认音乐")]
    public AudioClip explorationMusic1;
    
    [Tooltip("探索音乐2 - 游戏场景备用音乐")]
    public AudioClip explorationMusic2;
    
    [Tooltip("探索音乐3 - 游戏场景备用音乐")]
    public AudioClip explorationMusic3;
    
    [Tooltip("营地音乐 - 安全区/营地场景播放")]
    public AudioClip campMusic;
    
    [Tooltip("默认背景音乐 - 其他场景默认播放")]
    public AudioClip defaultMusic;

    [Header("=== 音效配置 ===")]
    [Header("战斗音效")]
    [Tooltip("玩家攻击音效")]
    public AudioClip playerAttackSound;
    
    [Tooltip("玩家死亡音效")]
    public AudioClip playerDeathSound;
    
    [Tooltip("敌人受击音效")]
    public AudioClip enemyHitSound;

    [Header("召唤音效")]
    [Tooltip("召唤发动音效")]
    public AudioClip summonStartSound;
    
    [Tooltip("召唤物出现音效")]
    public AudioClip summonAppearSound;
    
    [Tooltip("召唤物攻击音效")]
    public AudioClip summonAttackSound;
    
    [Tooltip("召唤物消失音效")]
    public AudioClip summonDisappearSound;

    [Header("炼金音效")]
    [Tooltip("炼金成功音效")]
    public AudioClip alchemySuccessSound;
    
    [Tooltip("炼金失败音效")]
    public AudioClip alchemyFailSound;

    [Header("UI音效")]
    [Tooltip("点击音效")]
    public AudioClip clickSound;
    
    [Tooltip("确认音效")]
    public AudioClip confirmSound;
    
    [Tooltip("选择音效")]
    public AudioClip selectSound;

    private AudioClip currentMusic;
    private string currentSceneName;
    private float musicFadeTime = 1f;
    private Coroutine musicFadeCoroutine;

    [Header("=== 状态检测设置 ===")]
    [Tooltip("战斗检测范围 - 检测玩家周围敌人的半径")]
    public float battleDetectionRadius = 15f;
    
    [Tooltip("状态检测间隔 - 每隔多少秒检测一次状态")]
    public float stateCheckInterval = 0.5f;
    
    [Tooltip("战斗结束延迟 - 战斗结束后延迟多久切换回探索音乐")]
    public float battleEndDelay = 3f;
    
    [Tooltip("玩家预制体 - 用于检测玩家状态")]
    public GameObject playerPrefab;
    
    private bool isInBattle = false;
    private bool isInSafeZone = false;
    private GameObject player;
    private float lastStateCheckTime = 0f;
    private Coroutine battleEndCoroutine;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // 注册场景加载事件
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 订阅战斗事件
        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.onBattleStart += OnBattleStart;
            BattleEventManager.Instance.onBattleEnd += OnBattleEnd;
            BattleEventManager.Instance.onBossEncounter += OnBossEncounter;
        }
    }

    void OnDisable()
    {
        // 取消注册场景加载事件
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // 取消订阅战斗事件
        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.onBattleStart -= OnBattleStart;
            BattleEventManager.Instance.onBattleEnd -= OnBattleEnd;
            BattleEventManager.Instance.onBossEncounter -= OnBossEncounter;
        }
    }

    void Start()
    {
        // 初始化当前场景名称
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // 初始化玩家引用
        InitializePlayer();
        
        // 订阅全局事件
        GlobalEventManager.Instance.OnPlayerEnterSafeZone += OnPlayerEnterSafeZone;
        GlobalEventManager.Instance.OnPlayerExitSafeZone += OnPlayerExitSafeZone;
        GlobalEventManager.Instance.OnBattleStart += OnBattleStart;
        GlobalEventManager.Instance.OnBattleEnd += OnBattleEnd;
        
        // 只在主界面场景播放主题曲
        if (mainTheme != null && currentSceneName == mainMenuSceneName)
        {
            PlayMusic(mainTheme);
            Debug.Log("[音乐初始化] 播放主界面主题曲: " + mainTheme.name);
        }
        else
        {
            // 游戏场景开始时，先检测玩家状态
            Debug.Log("[音乐初始化] 当前场景不是主界面，开始检测玩家状态");
            
            // 初始化玩家状态
            isInSafeZone = CheckInSafeZone();
            isInBattle = CheckEnemiesNearby();
            
            // 根据初始状态播放音乐
            if (isInSafeZone)
            {
                if (campMusic != null)
                {
                    PlayMusic(campMusic);
                    Debug.Log("[音乐初始化] 玩家在安全区，播放营地音乐");
                }
            }
            else if (isInBattle)
            {
                if (battleMusic != null)
                {
                    PlayMusic(battleMusic);
                    Debug.Log("[音乐初始化] 玩家处于战斗状态，播放战斗音乐");
                }
            }
            else
            {
                // 播放默认背景音乐
                if (defaultMusic != null)
                {
                    PlayMusic(defaultMusic);
                    Debug.Log("[音乐初始化] 播放默认背景音乐: " + defaultMusic.name);
                }
                else if (explorationMusic1 != null)
                {
                    PlayMusic(explorationMusic1);
                    Debug.Log("[音乐初始化] 播放探索音乐1: " + explorationMusic1.name);
                }
                else
                {
                    Debug.LogWarning("[音乐初始化] 未配置任何背景音乐");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅全局事件
        if (GlobalEventManager.Instance != null)
        {
            GlobalEventManager.Instance.OnPlayerEnterSafeZone -= OnPlayerEnterSafeZone;
            GlobalEventManager.Instance.OnPlayerExitSafeZone -= OnPlayerExitSafeZone;
            GlobalEventManager.Instance.OnBattleStart -= OnBattleStart;
            GlobalEventManager.Instance.OnBattleEnd -= OnBattleEnd;
        }
    }
    
    // 玩家进入安全区时的处理
    private void OnPlayerEnterSafeZone(GameObject player)
    {
        isInSafeZone = true;
        if (campMusic != null)
        {
            PlayMusic(campMusic, true);
            Debug.Log("[音乐切换] 进入安全区，播放营地音乐");
        }
        // 停止战斗音乐（如果在战斗中）
        isInBattle = false;
    }
    
    // 玩家离开安全区时的处理
    private void OnPlayerExitSafeZone(GameObject player)
    {
        isInSafeZone = false;
        if (!isInBattle)
        {
            PlayExplorationMusic(1);
            Debug.Log("[音乐切换] 离开安全区，播放探索音乐");
        }
    }

    void Update()
    {
        // 定期检测状态并切换音乐
        if (Time.time - lastStateCheckTime >= stateCheckInterval)
        {
            lastStateCheckTime = Time.time;
            UpdateMusicState();
        }
    }

    [Header("=== 场景名称配置 ===")]
    [Tooltip("主界面场景名称 - 与Build Settings中的场景名称一致")]
    public string mainMenuSceneName = "MainMenu";
    
    [Tooltip("游戏场景名称 - 与Build Settings中的场景名称一致")]
    public string gameplaySceneName = "GamePlay";

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 场景切换时进行场景检测
        Debug.Log("[场景切换] 从" + currentSceneName + "切换到" + scene.name);
        
        // 停止上一个场景的音乐
        if (musicSource.isPlaying)
        {
            string stoppedMusicName = currentMusic != null ? currentMusic.name : "无音乐";
            musicSource.Stop();
            Debug.Log("[音乐切换] 停止上一个场景的音乐: " + stoppedMusicName);
        }
        
        // 更新当前场景名称
        currentSceneName = scene.name;
        
        // 播放当前场景的音乐
        if (scene.name == mainMenuSceneName)
        {
            if (mainTheme != null)
            {
                PlayMusic(mainTheme);
                Debug.Log("[音乐切换] 播放主界面主题曲: " + mainTheme.name);
            }
            else
            {
                Debug.LogWarning("[音乐切换] 主界面主题曲未配置");
            }
        }
        else if (scene.name == gameplaySceneName)
        {
            // 播放探索音乐（默认第一首）
            if (explorationMusic1 != null)
            {
                PlayMusic(explorationMusic1);
                Debug.Log("[音乐切换] 播放游戏场景探索音乐1: " + explorationMusic1.name);
            }
            else
            {
                Debug.LogWarning("[音乐切换] 游戏场景探索音乐1未配置");
                // 尝试播放默认音乐
                if (defaultMusic != null)
                {
                    PlayMusic(defaultMusic);
                    Debug.Log("[音乐切换] 播放默认背景音乐: " + defaultMusic.name);
                }
            }
        }
        else
        {
            // 其他场景播放默认背景音乐
            if (defaultMusic != null)
            {
                PlayMusic(defaultMusic);
                Debug.Log("[音乐切换] 播放默认背景音乐: " + defaultMusic.name);
            }
            else if (explorationMusic1 != null)
            {
                PlayMusic(explorationMusic1);
                Debug.Log("[音乐切换] 播放探索音乐1: " + explorationMusic1.name);
            }
            else
            {
                Debug.LogWarning("[音乐切换] 未配置任何背景音乐");
            }
        }
    }

    /// <summary>
    /// 初始化音频源组件
    /// </summary>
    void InitializeAudioSources()
    {
        // 确保游戏对象有效
        if (gameObject == null) return;
        
        // 创建音乐源
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // 创建音效源
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // 创建UI音效源
        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
        }

        UpdateVolumes();
    }

    /// <summary>
    /// 更新所有音量
    /// </summary>
    void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
        if (uiSource != null)
            uiSource.volume = uiVolume * masterVolume;
    }

    // ========== 背景音乐控制 ==========

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        // 检查音乐源是否有效
        if (musicSource == null) return;
        
        // 检查音频剪辑是否有效
        if (clip == null || clip == currentMusic) return;

        // 确保淡入淡出时间为正数
        musicFadeTime = Mathf.Max(0.1f, musicFadeTime);

        // 停止之前的淡入淡出协程
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        // 播放音乐
        if (fade && musicSource.isPlaying)
        {
            musicFadeCoroutine = StartCoroutine(FadeMusic(clip));
        }
        else
        {
            currentMusic = clip;
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlayExplorationMusic(int trackNumber = 1)
    {
        AudioClip targetMusic = null;

        switch (trackNumber)
        {
            case 1:
                targetMusic = explorationMusic1;
                break;
            case 2:
                targetMusic = explorationMusic2;
                break;
            case 3:
                targetMusic = explorationMusic3;
                break;
            default:
                targetMusic = explorationMusic1;
                break;
        }

        if (targetMusic != null)
        {
            PlayMusic(targetMusic, true);
        }
    }

    /// <summary>
    /// 播放战斗音乐
    /// </summary>
    public void PlayBattleMusic(bool isBoss = false)
    {
        PlayMusic(isBoss ? bossBattleMusic : battleMusic, true);
    }

    /// <summary>
    /// 播放营地音乐
    /// </summary>
    public void PlayCampMusic()
    {
        PlayMusic(campMusic, true);
    }

    /// <summary>
    /// 音乐淡入淡出协程
    /// </summary>
    System.Collections.IEnumerator FadeMusic(AudioClip newClip)
    {
        // 检查音乐源是否有效
        if (musicSource == null || newClip == null)
        {
            yield break;
        }
        
        // 淡出
        float startVolume = musicSource.volume;
        for (float t = 0; t < musicFadeTime; t += Time.deltaTime)
        {
            if (musicSource == null) yield break;
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeTime);
            yield return null;
        }
        
        if (musicSource == null) yield break;
        musicSource.volume = 0;
        musicSource.Stop();

        // 切换音乐
        currentMusic = newClip;
        musicSource.clip = newClip;
        musicSource.Play();

        // 淡入
        for (float t = 0; t < musicFadeTime; t += Time.deltaTime)
        {
            if (musicSource == null) yield break;
            musicSource.volume = Mathf.Lerp(0, musicVolume * masterVolume, t / musicFadeTime);
            yield return null;
        }
        
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        
        // 重置协程引用
        musicFadeCoroutine = null;
    }

    // ========== 音效播放 ==========

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        }
    }

    /// <summary>
    /// 播放UI音效
    /// </summary>
    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && uiSource != null)
        {
            uiSource.PlayOneShot(clip, volume * uiVolume * masterVolume);
        }
    }

    // ========== 便捷方法 ==========

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

    // ========== 音量控制 ==========

    public void SetMasterVolume(float volume) { masterVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
    public void SetMusicVolume(float volume) { musicVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
    public void SetSFXVolume(float volume) { sfxVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
    public void SetUIVolume(float volume) { uiVolume = Mathf.Clamp01(volume); UpdateVolumes(); }

    // ========== 战斗状态管理 ==========

    void OnBattleStart()
    {
        isInBattle = true;
        
        // 停止战斗结束延迟协程
        if (battleEndCoroutine != null)
        {
            StopCoroutine(battleEndCoroutine);
            battleEndCoroutine = null;
        }
        
        // 播放战斗音乐
        if (battleMusic != null)
        {
            PlayMusic(battleMusic, true);
            Debug.Log("[战斗音乐] 进入战斗状态，播放战斗音乐");
        }
    }
    
    // 用于GlobalEventManager的战斗开始事件处理
    void OnBattleStart(GameObject enemy)
    {
        OnBattleStart();
    }

    void OnBattleEnd()
    {
        isInBattle = false;
        
        // 延迟切换回探索音乐
        if (battleEndCoroutine != null)
        {
            StopCoroutine(battleEndCoroutine);
        }
        battleEndCoroutine = StartCoroutine(DelayedSwitchToExplorationMusic());
    }
    
    // 用于GlobalEventManager的战斗结束事件处理
    void OnBattleEnd(GameObject enemy)
    {
        OnBattleEnd();
    }

    void OnBossEncounter()
    {
        isInBattle = true;
        
        // 停止战斗结束延迟协程
        if (battleEndCoroutine != null)
        {
            StopCoroutine(battleEndCoroutine);
            battleEndCoroutine = null;
        }
        
        // 播放Boss战斗音乐
        if (bossBattleMusic != null)
        {
            PlayMusic(bossBattleMusic, true);
            Debug.Log("[战斗音乐] 遭遇Boss，播放Boss战斗音乐");
        }
        else if (battleMusic != null)
        {
            PlayMusic(battleMusic, true);
            Debug.Log("[战斗音乐] 遭遇Boss，播放战斗音乐");
        }
    }

    System.Collections.IEnumerator DelayedSwitchToExplorationMusic()
    {
        yield return new WaitForSeconds(battleEndDelay);
        
        // 只有在不在战斗状态且不在安全区时才切换到探索音乐
        if (!isInBattle && !isInSafeZone)
        {
            PlayExplorationMusic(1);
            Debug.Log("[战斗音乐] 战斗结束，切换回探索音乐");
        }
        
        battleEndCoroutine = null;
    }

    // ========== 状态检测 ==========

    void InitializePlayer()
    {
        if (player == null)
        {
            // 优先使用玩家预制体来查找玩家
            if (playerPrefab != null)
            {
                // 查找场景中与预制体相同类型的游戏对象
                string playerTag = playerPrefab.tag;
                if (!string.IsNullOrEmpty(playerTag))
                {
                    player = GameObject.FindGameObjectWithTag(playerTag);
                }
            }
            
            // 如果没有设置预制体或找不到玩家，使用传统的标签查找
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
        }
    }

    bool CheckEnemiesNearby()
    {
        if (player == null) return false;
        
        // 使用Physics2D检测玩家周围的敌人
        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, battleDetectionRadius);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // 检查敌人是否在追逐玩家
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // 如果敌人在追逐状态，或者距离玩家很近（进入战斗范围）
                    if (enemyAI.isChasing)
                    {
                        return true;
                    }
                    
                    // 额外检查：如果敌人距离玩家很近（即使在巡逻），也视为战斗状态
                    float distance = Vector2.Distance(enemy.transform.position, player.transform.position);
                    if (distance <= enemyAI.attackRange * 2f)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    bool CheckInSafeZone()
    {
        if (player == null) return false;
        
        // 检测玩家是否在安全区检测器的范围内
        SafeZoneDetector[] safeZones = FindObjectsOfType<SafeZoneDetector>();
        foreach (SafeZoneDetector safeZone in safeZones)
        {
            if (safeZone.IsPlayerInSafeZone())
            {
                return true;
            }
        }
        
        // 检测玩家是否在营火范围内
        CampfireSystem[] campfires = FindObjectsOfType<CampfireSystem>();
        foreach (CampfireSystem campfire in campfires)
        {
            float distance = Vector2.Distance(player.transform.position, campfire.transform.position);
            // 假设营火的安全区半径为3单位
            if (distance <= 3f)
            {
                return true;
            }
        }
        
        // 检测玩家是否在地图系统中的安全区
        IntegratedMapSystem mapSystem = FindObjectOfType<IntegratedMapSystem>();
        if (mapSystem != null)
        {
            // 这里可以根据地图系统的具体实现来检测安全区
            // 例如，检查当前地图类型是否为安全区，或者检查玩家是否在特定的安全区域内
            // 由于没有看到具体的安全区实现，这里暂时返回false
            // 实际使用时需要根据地图系统的实现进行修改
        }
        
        return false;
    }

    void UpdateMusicState()
    {
        // 如果在主界面场景，不进行状态检测
        if (currentSceneName == mainMenuSceneName) return;
        
        // 检测玩家周围是否有敌人
        bool enemiesNearby = CheckEnemiesNearby();
        
        // 检测玩家是否在安全区
        bool inSafeZone = CheckInSafeZone();
        
        // 更新安全区状态
        bool wasInSafeZone = isInSafeZone;
        isInSafeZone = inSafeZone;
        
        // 更新战斗状态
        bool wasInBattle = isInBattle;
        isInBattle = enemiesNearby;
        
        // 如果玩家在安全区，播放营地音乐
        if (isInSafeZone && !wasInSafeZone)
        {
            // 进入安全区
            if (campMusic != null)
            {
                PlayMusic(campMusic, true);
                Debug.Log("[音乐切换] 进入安全区，播放营地音乐");
            }
            // 停止战斗音乐（如果在战斗中）
            isInBattle = false;
        }
        else if (!isInSafeZone && wasInSafeZone)
        {
            // 离开安全区
            if (!isInBattle)
            {
                PlayExplorationMusic(1);
                Debug.Log("[音乐切换] 离开安全区，播放探索音乐");
            }
        }
        
        // 如果不在安全区，检测战斗状态
        if (!isInSafeZone)
        {
            // 如果检测到敌人且当前不在战斗状态，触发战斗开始
            if (enemiesNearby && !wasInBattle)
            {
                OnBattleStart();
            }
            // 如果没有检测到敌人且当前在战斗状态，触发战斗结束
            else if (!enemiesNearby && wasInBattle)
            {
                OnBattleEnd();
            }
        }
    }
}
