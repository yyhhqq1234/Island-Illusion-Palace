namespace IIP.Core
{
    public interface IBurdenProvider
    {
        float CurrentBurden { get; }
        float MaxBurden { get; }
        bool IsHighBurden { get; }
        bool IsCriticalBurden { get; }
        float GetBurdenResistance();
        void AddBurden(float amount);
        void RemoveBurden(float amount);
        void ResetBurden();
    }
}
