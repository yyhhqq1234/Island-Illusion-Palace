using UnityEngine;

public class NarrativeEventSystem : MonoBehaviour, INarrativeEventProvider
{
    public static NarrativeEventSystem Instance { get; private set; }

    public float LastEventBurdenLevel { get; private set; } = -1f;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        GlobalEventManager.Instance.OnBurdenChanged += OnBurdenChanged;
    }

    void OnDisable()
    {
        var gem = GlobalEventManager.Instance;
        if (gem != null) gem.OnBurdenChanged -= OnBurdenChanged;
    }

    public void OnBurdenChanged(float newBurden)
    {
        if (LastEventBurdenLevel < 0) LastEventBurdenLevel = newBurden;

        if (newBurden >= 30 && newBurden < 50 && LastEventBurdenLevel < 30)
            TriggerNarrativeEvent("墨语回忆美好时光", "NARRATIVE_GOOD_MEMORY");
        else if (newBurden >= 50 && newBurden < 70 && LastEventBurdenLevel < 50)
            TriggerNarrativeEvent("幻听莎娜呼唤", "NARRATIVE_WHISPER");
        else if (newBurden >= 70 && newBurden < 90 && LastEventBurdenLevel < 70)
            TriggerNarrativeEvent("记忆错乱混合片段", "NARRATIVE_CONFUSION");
        else if (newBurden >= 90 && LastEventBurdenLevel < 90)
            TriggerNarrativeEvent("黑暗低语（死灵圣王之眼）", "NARRATIVE_DARK_WHISPER");

        LastEventBurdenLevel = Mathf.Floor(newBurden / 10f) * 10f;
    }

    public void TriggerNarrativeEvent(string eventName, string eventID)
    {
        ShowNarrativeHint(eventName, 5f);
        GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.Silence);
        Debug.Log($"[NarrativeEvent] {eventName} → {eventID}");
    }

    public void ShowNarrativeHint(string text, float duration)
    {
        GlobalEventManager.Instance.ShowNotification(text, duration);
    }
}
