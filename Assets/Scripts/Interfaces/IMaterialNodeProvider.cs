using GameSystems;

public interface IMaterialNodeProvider
{
    MaterialTypeEnum MaterialType { get; }
    int Quantity { get; }
    float CollectTime { get; }
    bool IsCollected { get; }
    void Collect(InventorySystem inventory);
}
