using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartGame);
            startButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitGame);
            quitButton.onClick.AddListener(() => IIPBootstrap.Audio?.PlayClick());
        }
    }

    void OnStartGame()
    {
        IIPBootstrap.SwitchToScene(IIPConstants.SceneGamePlay);
    }

    void OnQuitGame()
    {
        IIPBootstrap.Events.TriggerGameQuit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
