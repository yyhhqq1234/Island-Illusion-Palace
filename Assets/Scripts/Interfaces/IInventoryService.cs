using System.Collections.Generic;
using GameSystems;

public interface IInventoryService
{
    void AddMaterial(MaterialTypeEnum type, int count);
    void RemoveMaterial(MaterialTypeEnum type, int count);
    int GetMaterialQuantity(MaterialTypeEnum type);
    void AddPotion(RecipeType type, int count);
    void RemovePotion(RecipeType type, int count);
    int GetPotionQuantity(RecipeType type);
    List<MemoryFragment> GetAllMemoryFragments();
    bool HasMemoryFragment();
    List<InventoryItem> GetAllItems();
}
