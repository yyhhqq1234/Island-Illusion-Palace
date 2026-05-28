public interface IBossPhaseProvider
{
    BossPhase currentPhase { get; set; }
    void UpdatePhase();
    void OnPhaseTransition(BossPhase newPhase);
    void AddAttackPattern(string patternID);
    void SwitchWeakness();
    void ShowPhaseChangeEffect();
}
