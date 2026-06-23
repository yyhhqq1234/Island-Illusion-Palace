using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// IIP 启动引导器 — 挂载在每个场景的最早 GameObject 上。
/// 自动创建 / 保活所有跨场景 Singleton，并监听场景加载事件。
///
/// 调用方式（任意脚本）：
///   IIPBootstrap.Events.TriggerBattleStart(enemy);
///   IIPBootstrap.SwitchToScene("Forest");
///   IIPBootstrap.Audio.PlayClick();
/// </summary>
[DefaultExecutionOrder(-1000)]
public class IIPBootstrap : MonoBehaviour
{
    [Header("场景标识")]
    [Tooltip("当前场景类型（BGM 状态由此决定）")]
    public GlobalEventManager.MusicState defaultMusicState = GlobalEventManager.MusicState.Exploration;

    // ── 静态快速访问 ──
    public static GlobalEventManager Events  => GlobalEventManager.Instance;
    public static GameplayAudioManager Audio => GameplayAudioManager.Instance;

    // ── 场景切换（推荐统一入口） ──
    public static void SwitchToScene(string sceneName)
    {
        Events.TriggerSceneWillLoad(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public static void SwitchToScene(int buildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        string name  = System.IO.Path.GetFileNameWithoutExtension(path);
        Events.TriggerSceneWillLoad(name);
        SceneManager.LoadScene(buildIndex);
    }

    // ── 生命周期 ──
    void Awake()
    {
        EnsureSingletons();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 全局通知
        Events.TriggerSceneDidLoad(scene.name);

        // 当前场景的 IIPBootstrap 负责设置默认 BGM
        var myBootstrap = FindObjectOfType<IIPBootstrap>();
        if (myBootstrap != null && myBootstrap == this)
        {
            Events.RequestMusicState(defaultMusicState);
            Debug.Log($"[IIPBootstrap] 场景 {scene.name} 加载完毕，默认音乐: {defaultMusicState}");
        }
    }

    // ── 确保所有跨场景 Singleton 存在 ──
    private static void EnsureSingletons()
    {
        // 首次访问时自动创建 + DontDestroyOnLoad
        var evts = GlobalEventManager.Instance;
        var aud  = GameplayAudioManager.Instance;

        Debug.Log($"[IIPBootstrap] 全局管理器就绪 — Events:{evts != null} Audio:{aud != null}");
    }

    // ── Editor 快速创建 Bootstrap ──
#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/IIP/Create Bootstrap", false, 0)]
    static void CreateBootstrap()
    {
        var go = new GameObject("[IIP] Bootstrap");
        go.AddComponent<IIPBootstrap>();
        UnityEditor.Selection.activeGameObject = go;
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create IIP Bootstrap");
    }
#endif
}
