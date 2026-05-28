using UnityEngine;

public interface IMapSystem
{
    MapType currentMapType { get; set; }
    MapCategory currentMapCategory { get; set; }
    int currentCycle { get; set; }
    void GenerateMap();
    void GenerateNewMap();
    void SetMapType(MapType type);
    void SetCycle(int cycle);
    void RegisterCampfire(Vector3 position);
}

public interface ICampfireService
{
    void OnRest();
    void OnConfigureFragments();
    void OnAlchemyAtCampfire();
}
