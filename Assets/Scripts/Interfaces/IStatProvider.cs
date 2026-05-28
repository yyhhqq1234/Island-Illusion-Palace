public interface IStatProvider
{
    int strength { get; set; }
    int vitality { get; set; }
    int intelligence { get; set; }
    int resonance { get; set; }
    int level { get; }
    float physicalDamageBonus { get; }
    float magicalDamageBonus { get; }
    float alchemySuccessRateBonus { get; }
    float fragmentDiscoveryRate { get; }
    void RecalculateDerivedStats();
    void AddExperience(int amount);
}

public enum AttributeType { Strength, Vitality, Intelligence, Resonance }
