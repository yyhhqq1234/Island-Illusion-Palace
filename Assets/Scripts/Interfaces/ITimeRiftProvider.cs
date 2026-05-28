public interface ITimeRiftProvider
{
    float BaseProbability { get; }
    bool IsRiftActive { get; }
    void GenerateTimeRifts();
    void OnRiftEntered(MapType destinationType);
}
