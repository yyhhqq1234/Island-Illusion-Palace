public interface INarrativeEventProvider
{
    float LastEventBurdenLevel { get; }
    void OnBurdenChanged(float newBurden);
    void TriggerNarrativeEvent(string eventName, string eventID);
    void ShowNarrativeHint(string text, float duration);
}
