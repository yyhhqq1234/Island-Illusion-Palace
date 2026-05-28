public interface IQTESystem
{
    bool QTEActive { get; }
    bool QTESuccess { get; }
    int TotalRounds { get; }
    int KeysPerRound { get; }
    int MaxErrorsAllowed { get; }
    float RoundTimeLimit { get; }
    void StartQTE();
    void FinishQTE(bool success);
    void HideQTEUI();
}
