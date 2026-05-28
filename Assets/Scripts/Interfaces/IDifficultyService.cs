using System.Collections.Generic;

namespace IIP.Difficulty
{
    public interface IDifficultyService
    {
        DifficultyMode CurrentMode { get; }
        float GetEnemyHealthMultiplier();
        float GetEnemyDamageMultiplier();
        float GetSoulDropMultiplier();
        float GetBurdenGainMultiplier();
        bool IsEnemyBerserkEnabled();
        bool IsTimeCollapseEnabled();
        bool CanSelectMode(DifficultyMode mode);
        void SetMode(DifficultyMode mode);
    }
}
