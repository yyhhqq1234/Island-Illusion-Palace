using System.Collections.Generic;
using GameSystems;

public interface IAlchemyService
{
    Dictionary<MaterialTypeEnum, int> MaterialInventory { get; }
    List<RecipeData> AvailableRecipes { get; }
    List<RecipeType> DiscoveredRecipes { get; }
    float RecipeBaseSuccessRate { get; }
    bool CraftWithMaterials(MaterialTypeEnum[] materials);
    bool CraftRecipe(RecipeType recipeType);
    float CalculateSuccessRate(RecipeData recipe);
    List<object> GetPendingProduce();
    void ClearPendingProduce();
    MaterialData GetMaterialData(MaterialTypeEnum type);
    int GetMaterialValue(MaterialTypeEnum type);
}
