public interface IMultiWeekProgression
{
    int TotalCycles { get; }
    int PermanentStrength { get; }
    int PermanentVitality { get; }
    int PermanentIntelligence { get; }
    int PermanentResonance { get; }
    int PermanentPointPool { get; }
    int CalculatePermanentBonus(int cycleCount);
    void ApplyPermanentStats(int strength, int vitality, int intelligence, int resonance);
    void OnNewCycleStart();
}
