using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 资源管理器 - 负责自动加载和管理游戏资源
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("资源加载配置")]
    [Tooltip("是否在Start时自动加载所有资源")]
    public bool autoLoadOnStart = true;
    [Tooltip("是否使用缓存机制")]
    public bool useCache = true;

    [Header("音乐资源路径")]
    [Tooltip("主题曲音乐路径")]
    public string mainThemePath = "Assets/Music/MainTheme/I-IP主题曲.mp3";
    
    [Tooltip("战斗音乐路径")]
    public string battleMusicPath = "Assets/Music/Battle/幻宫游戏战斗曲.mp3";
    
    [Tooltip("Boss战斗音乐路径")]
    public string bossBattleMusicPath = "Assets/Music/Battle/Boss/幻宫BOSS战斗曲-1.mp3";
    
    [Tooltip("探索音乐1路径")]
    public string explorationMusic1Path = "Assets/Music/MapScene/Explore/幻宫游戏地图探索曲（理智0-30）.mp3";
    
    [Tooltip("探索音乐2路径")]
    public string explorationMusic2Path = "Assets/Music/MapScene/Explore/幻宫游戏地图探索曲（理智30-70）.mp3";
    
    [Tooltip("探索音乐3路径")]
    public string explorationMusic3Path = "Assets/Music/MapScene/Explore/幻宫游戏地图探索曲1（理智70-100）.mp3";
    
    [Tooltip("营地音乐路径")]
    public string campMusicPath = "Assets/Music/MapScene/SafeArea/幻宫游戏营地（安全区）曲(1).mp3";
    
    [Tooltip("默认背景音乐路径")]
    public string defaultMusicPath = "Assets/Music/MapScene/Explore/幻宫游戏地图探索曲1（理智70-100）.mp3";

    [Header("音效资源路径")]
    [Tooltip("玩家攻击音效路径")]
    public string playerAttackSoundPath = "Assets/Music/Battle/角色攻击.mp3";
    
    [Tooltip("玩家死亡音效路径")]
    public string playerDeathSoundPath = "Assets/Music/Battle/角色死亡.mp3";
    
    [Tooltip("敌人受击音效路径")]
    public string enemyHitSoundPath = "Assets/Music/Battle/角色攻击.mp3"; // 临时使用相同音效

    [Header("召唤音效路径")]
    [Tooltip("召唤发动音效路径")]
    public string summonStartSoundPath = "Assets/Music/Effect/Summon/召唤发动.mp3";
    
    [Tooltip("召唤物出现音效路径")]
    public string summonAppearSoundPath = "Assets/Music/Effect/Summon/召唤物出现.mp3";
    
    [Tooltip("召唤物攻击音效路径")]
    public string summonAttackSoundPath = "Assets/Music/Effect/Summon/召唤物攻击.mp3";
    
    [Tooltip("召唤物消失音效路径")]
    public string summonDisappearSoundPath = "Assets/Music/Effect/Summon/召唤物消失.mp3";

    [Header("炼金音效路径")]
    [Tooltip("炼金成功音效路径")]
    public string alchemySuccessSoundPath = "Assets/Music/Effect/Alchemy/炼金成功.mp3";
    
    [Tooltip("炼金失败音效路径")]
    public string alchemyFailSoundPath = "Assets/Music/Effect/Alchemy/炼金失败.mp3";

    [Header("UI音效路径")]
    [Tooltip("点击音效路径")]
    public string clickSoundPath = "Assets/Music/UIEffect/点击.mp3";
    
    [Tooltip("确认音效路径")]
    public string confirmSoundPath = "Assets/Music/UIEffect/确认.mp3";
    
    [Tooltip("选择音效路径")]
    public string selectSoundPath = "Assets/Music/UIEffect/选择.mp3";

    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (autoLoadOnStart)
        {
            LoadAllAudioResources();
            ConfigureAudioManager();
        }
    }

    /// <summary>
    /// 加载所有音效资源
    /// </summary>
    public void LoadAllAudioResources()
    {
        // 加载背景音乐
        LoadAudioClip(mainThemePath);
        LoadAudioClip(battleMusicPath);
        LoadAudioClip(bossBattleMusicPath);
        LoadAudioClip(explorationMusic1Path);
        LoadAudioClip(explorationMusic2Path);
        LoadAudioClip(explorationMusic3Path);
        LoadAudioClip(campMusicPath);
        LoadAudioClip(defaultMusicPath);

        // 加载战斗音效
        LoadAudioClip(playerAttackSoundPath);
        LoadAudioClip(playerDeathSoundPath);
        LoadAudioClip(enemyHitSoundPath);

        // 加载召唤音效
        LoadAudioClip(summonStartSoundPath);
        LoadAudioClip(summonAppearSoundPath);
        LoadAudioClip(summonAttackSoundPath);
        LoadAudioClip(summonDisappearSoundPath);

        // 加载炼金音效
        LoadAudioClip(alchemySuccessSoundPath);
        LoadAudioClip(alchemyFailSoundPath);

        // 加载UI音效
        LoadAudioClip(clickSoundPath);
        LoadAudioClip(confirmSoundPath);
        LoadAudioClip(selectSoundPath);

        Debug.Log("所有音效资源加载完成！");
    }

    /// <summary>
    /// 加载单个音频剪辑
    /// </summary>
    public AudioClip LoadAudioClip(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("音频路径为空，无法加载资源");
            return null;
        }

        // 检查缓存
        if (useCache && audioClipCache.ContainsKey(path))
        {
            return audioClipCache[path];
        }

        // 从资源加载
        AudioClip clip = null;

#if UNITY_EDITOR
        // 编辑器模式：使用AssetDatabase加载
        clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
#else
        // 运行时模式：使用Resources加载
        string resourcesPath = path.Replace("Assets/", "").Replace(".mp3", "");
        clip = Resources.Load<AudioClip>(resourcesPath);
#endif

        if (clip != null)
        {
            if (useCache)
            {
                audioClipCache[path] = clip;
            }
            Debug.Log($"成功加载音效: {path}");
        }
        else
        {
            Debug.LogWarning($"无法加载音效: {path}");
        }

        return clip;
    }

    /// <summary>
    /// 配置AudioManager的音效资源
    /// </summary>
    public void ConfigureAudioManager()
    {
        if (GameplayAudioManager.Instance == null)
        {
            Debug.LogWarning("GameplayAudioManager未找到，无法自动配置音效资源");
            return;
        }

        GameplayAudioManager audioManager = GameplayAudioManager.Instance;

#if UNITY_EDITOR
        // 编辑器模式：自动加载资源
        // 配置背景音乐
        audioManager.mainTheme = LoadAudioClip(mainThemePath);
        audioManager.battleMusic = LoadAudioClip(battleMusicPath);
        audioManager.bossBattleMusic = LoadAudioClip(bossBattleMusicPath);
        audioManager.explorationMusic1 = LoadAudioClip(explorationMusic1Path);
        audioManager.explorationMusic2 = LoadAudioClip(explorationMusic2Path);
        audioManager.explorationMusic3 = LoadAudioClip(explorationMusic3Path);
        audioManager.campMusic = LoadAudioClip(campMusicPath);
        audioManager.defaultMusic = LoadAudioClip(defaultMusicPath);

        // 配置战斗音效
        audioManager.playerAttackSound = LoadAudioClip(playerAttackSoundPath);
        audioManager.playerDeathSound = LoadAudioClip(playerDeathSoundPath);
        audioManager.enemyHitSound = LoadAudioClip(enemyHitSoundPath);

        // 配置召唤音效
        audioManager.summonStartSound = LoadAudioClip(summonStartSoundPath);
        audioManager.summonAppearSound = LoadAudioClip(summonAppearSoundPath);
        audioManager.summonAttackSound = LoadAudioClip(summonAttackSoundPath);
        audioManager.summonDisappearSound = LoadAudioClip(summonDisappearSoundPath);

        // 配置炼金音效
        audioManager.alchemySuccessSound = LoadAudioClip(alchemySuccessSoundPath);
        audioManager.alchemyFailSound = LoadAudioClip(alchemyFailSoundPath);

        // 配置UI音效
        audioManager.clickSound = LoadAudioClip(clickSoundPath);
        audioManager.confirmSound = LoadAudioClip(confirmSoundPath);
        audioManager.selectSound = LoadAudioClip(selectSoundPath);

        Debug.Log("GameplayAudioManager音效资源配置完成！");
#else
        // 运行时模式：资源应该已经在Inspector中分配
        if (audioManager.mainTheme == null)
        {
            Debug.LogWarning("GameplayAudioManager音效资源未在Inspector中分配！请在编辑器中手动分配资源。");
        }
        else
        {
            Debug.Log("GameplayAudioManager音效资源已配置（运行时模式）。");
        }
#endif
    }

    /// <summary>
    /// 获取音频剪辑（带缓存）
    /// </summary>
    public AudioClip GetAudioClip(string path)
    {
        return LoadAudioClip(path);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        audioClipCache.Clear();
        Debug.Log("资源缓存已清除");
    }

    /// <summary>
    /// 卸载未使用的资源
    /// </summary>
    public void UnloadUnusedResources()
    {
        Resources.UnloadUnusedAssets();
        Debug.Log("未使用的资源已卸载");
    }
}