using UnityEngine;

public interface IMapSystem
{
    MapType CurrentMapType { get; }
    MapCategory CurrentMapCategory { get; }
    int CurrentCycle { get; }
    void GenerateMap();
    void GenerateNewMap();
    void SetMapType(MapType type);
    void SetCycle(int cycle);
    void RegisterCampfire(Vector3 position);
}

public interface ICampfireService
{
    bool IsPlayerInRange { get; }
    void OnRest();
    void OnConfigureFragments();
    void OnAlchemyAtCampfire();
}
