using System.Collections.Generic;
using GameSystems;

public interface IInventoryService
{
    bool AddMaterial(MaterialTypeEnum type, int count);
    bool RemoveMaterial(MaterialTypeEnum type, int count);
    int GetMaterialQuantity(MaterialTypeEnum type);
    bool AddPotion(RecipeType type, int count);
    int GetPotionQuantity(RecipeType type);
    List<InventoryItem> GetAllMemoryFragments();
    bool HasMemoryFragment();
    List<InventoryItem> GetAllItems();
    int currentSouls { get; set; }
    int currentSoulEssence { get; set; }
    bool HasEnoughSouls(int amount);
    void ConsumeSouls(int amount);
    void AddSouls(int amount);
}

public interface IResourceCollector
{
    int currentSouls { get; set; }
    int currentSoulEssence { get; set; }
    void CollectSoul(int amount, string source);
    void CollectSoulEssence(int amount);
}
