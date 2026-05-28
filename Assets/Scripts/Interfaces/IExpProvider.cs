public interface IExpProvider
{
    int Level { get; }
    int CurrentExperience { get; }
    int FreeAttributePoints { get; }
    int GetExpRequirement(int level);
    void AddExperience(int amount);
    void LevelUp();
}
