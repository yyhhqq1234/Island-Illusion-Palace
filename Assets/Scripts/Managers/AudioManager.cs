using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 游戏音效管理器 - Singleton + DontDestroyOnLoad
/// 通过 GlobalEventManager 订阅全局事件，实现自动 BGM 切换。
/// 任意脚本可通过 IIPBootstrap.Audio 访问。
/// </summary>
public class GameplayAudioManager : MonoBehaviour
{
    public static GameplayAudioManager Instance { get; private set; }

    [Header("=== 音频源组件 ===")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("=== 音量设置 ===")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;

    [Header("=== 场景音乐配置 ===")]
    public AudioClip mainTheme;
    public AudioClip battleMusic;
    public AudioClip bossBattleMusic;
    public AudioClip explorationMusic1;
    public AudioClip explorationMusic2;
    public AudioClip explorationMusic3;
    public AudioClip campMusic;
    public AudioClip defaultMusic;

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

    [Header("=== 状态检测（已废弃自检，改为GlobalEventManager驱动） ===")]
    public float battleDetectionRadius = 15f;
    public float stateCheckInterval = 0.5f;
    public float battleEndDelay = 3f;
    public GameObject playerPrefab;

    private AudioClip currentMusic;
    private string currentSceneName = "";
    private float musicFadeTime = 1f;
    private Coroutine musicFadeCoroutine;
    private GameObject player;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); InitializeAudioSources(); }
        else if (Instance != this) { Destroy(gameObject); }
    }

    void OnEnable()
    {
        var e = GlobalEventManager.Instance;
        e.OnSceneDidLoad += OnSceneChanged;
        e.OnMusicStateChange += OnMusicStateChange;
        e.OnPlayerEnterSafeZone += _ => GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.Camp);
        e.OnPlayerExitSafeZone  += _ => GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.Exploration);
        e.OnBattleStart += _ => GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.Battle);
        e.OnBattleEnd   += _ => GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.Exploration);
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance == null) return;
        var e = GlobalEventManager.Instance;
        e.OnSceneDidLoad -= OnSceneChanged;
        e.OnMusicStateChange -= OnMusicStateChange;
    }

    void OnSceneChanged(string sceneName) => currentSceneName = sceneName;

    void OnMusicStateChange(GlobalEventManager.MusicState state)
    {
        AudioClip target = state switch
        {
            GlobalEventManager.MusicState.MainMenu    => mainTheme,
            GlobalEventManager.MusicState.Exploration => explorationMusic1,
            GlobalEventManager.MusicState.Battle      => battleMusic,
            GlobalEventManager.MusicState.BossBattle  => bossBattleMusic,
            GlobalEventManager.MusicState.Camp        => campMusic,
            _ => defaultMusic
        };
        if (state == GlobalEventManager.MusicState.Silence) StopMusic();
        else PlayMusic(target);
        Debug.Log($"[AudioManager] BGM → {state}");
    }

    // ═══════════════════════════════════════════
    // 播放控制（公共 API）
    // ═══════════════════════════════════════════

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
        AudioClip target = track switch { 2 => explorationMusic2, 3 => explorationMusic3, _ => explorationMusic1 };
        PlayMusic(target, true);
    }

    public void PlayBattleMusic(bool isBoss = false) => PlayMusic(isBoss ? bossBattleMusic : battleMusic, true);
    public void PlayCampMusic() => PlayMusic(campMusic, true);

    // 便捷音效方法
    public void PlayPlayerAttack() => PlaySFX(playerAttackSound);
    public void PlayPlayerDeath()  => PlaySFX(playerDeathSound);
    public void PlayEnemyHit()     => PlaySFX(enemyHitSound);
    public void PlaySummonStart()  => PlaySFX(summonStartSound);
    public void PlaySummonAppear() => PlaySFX(summonAppearSound);
    public void PlaySummonAttack() => PlaySFX(summonAttackSound);
    public void PlaySummonDisappear() => PlaySFX(summonDisappearSound);
    public void PlayAlchemySuccess() => PlaySFX(alchemySuccessSound);
    public void PlayAlchemyFail()    => PlaySFX(alchemyFailSound);
    public void PlayClick()   => PlayUISound(clickSound);
    public void PlayConfirm() => PlayUISound(confirmSound);
    public void PlaySelect()  => PlayUISound(selectSound);

    // ═══════════════════════════════════════════
    // 内部
    // ═══════════════════════════════════════════

    void InitializeAudioSources()
    {
        if (musicSource == null) { var go = new GameObject("MusicSource"); go.transform.SetParent(transform); musicSource = go.AddComponent<AudioSource>(); musicSource.loop = true; musicSource.playOnAwake = false; }
        if (sfxSource   == null) { var go = new GameObject("SFXSource");   go.transform.SetParent(transform); sfxSource   = go.AddComponent<AudioSource>(); sfxSource.playOnAwake = false; }
        if (uiSource    == null) { var go = new GameObject("UISource");    go.transform.SetParent(transform); uiSource    = go.AddComponent<AudioSource>(); uiSource.playOnAwake = false; }
        UpdateVolumes();
    }

    void InitializePlayer()
    {
        if (playerPrefab != null) player = playerPrefab;
        if (player == null) player = GameObject.FindGameObjectWithTag(IIPConstants.TagPlayer);
    }

    void UpdateVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
        if (sfxSource   != null) sfxSource.volume   = sfxVolume   * masterVolume;
        if (uiSource    != null) uiSource.volume    = uiVolume    * masterVolume;
    }

    System.Collections.IEnumerator FadeMusic(AudioClip nextClip)
    {
        float startVol = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < musicFadeTime * 0.5f) { elapsed += Time.deltaTime; musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (musicFadeTime * 0.5f)); yield return null; }
        musicSource.Stop();
        currentMusic = nextClip;
        musicSource.clip = nextClip;
        musicSource.Play();
        elapsed = 0f;
        while (elapsed < musicFadeTime * 0.5f) { elapsed += Time.deltaTime; musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, elapsed / (musicFadeTime * 0.5f)); yield return null; }
        musicSource.volume = musicVolume * masterVolume;
    }
}
