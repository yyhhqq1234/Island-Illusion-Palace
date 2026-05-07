using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private Button startButton;
    
    [SerializeField]
    private Button quitButton;
    
    [SerializeField]
    private string gameplaySceneName = "GamePlay";
    
    private GameplayAudioManager audioManager;

    private void Start()
    {
        // 获取音效管理器
        audioManager = FindObjectOfType<GameplayAudioManager>();

        // 绑定按钮事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            if (audioManager != null)
            {
                startButton.onClick.AddListener(() => audioManager.PlayClick());
            }
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            if (audioManager != null)
            {
                quitButton.onClick.AddListener(() => audioManager.PlayClick());
            }
        }
    }
    
    private void StartGame()
    {
        // 加载游玩场景
        SceneManager.LoadScene(gameplaySceneName);
        Debug.Log("游戏场景已加载: " + gameplaySceneName);
        
        // 切换到探索音乐
        if (GameplayAudioManager.Instance != null)
        {
            GameplayAudioManager.Instance.PlayExplorationMusic(1); // 默认第一首
        }
    }
    
    private void QuitGame()
    {
        // 退出游戏
        Application.Quit();
        
        // 在编辑器中停止播放
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
