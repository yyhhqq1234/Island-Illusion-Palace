public interface IStatProvider
{
    int Strength { get; }
    int Vitality { get; }
    int Intelligence { get; }
    int Resonance { get; }
    int Level { get; }
    float PhysicalDamageBonus { get; }
    float MagicalDamageBonus { get; }
    float AlchemySuccessRateBonus { get; }
    float FragmentDiscoveryRate { get; }
    void RecalculateDerivedStats();
    void AddExperience(int amount);
    bool AllocateAttribute(AttributeType attr);
}

public enum AttributeType { Strength, Vitality, Intelligence, Resonance }
