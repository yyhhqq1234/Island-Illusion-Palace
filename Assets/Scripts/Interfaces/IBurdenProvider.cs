public interface IBurdenProvider
{
    float currentBurden { get; set; }
    float maxBurden { get; }
    void AddBurden(float amount);
    void RemoveBurden(float amount);
    void ResetBurden();
    float GetBurdenResistance();
    bool IsHighBurden();
    bool IsCriticalBurden();
}
