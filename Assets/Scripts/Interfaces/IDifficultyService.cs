using IIP.Difficulty;

public interface IDifficultyService
{
    DifficultyMode currentMode { get; set; }
    float GetEnemyHealthMultiplier();
    float GetEnemyDamageMultiplier();
    float GetSoulDropMultiplier();
    float GetBurdenGainMultiplier();
    bool CanSelectMode(DifficultyMode mode);
}
