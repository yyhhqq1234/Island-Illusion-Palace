public interface IDynamicDifficulty
{
    float WinStreak { get; }
    float LoseStreak { get; }
    void RecordBattleResult(bool won);
    void RecordAreaClear(float elapsedTime);
    void AdjustDifficulty(float multiplier, string reason);
}
