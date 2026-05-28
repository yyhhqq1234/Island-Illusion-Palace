using System.Collections.Generic;
using GameSystems;

public interface IAlchemyService
{
    float recipeBaseSuccessRate { get; set; }
    bool CraftWithMaterials(MaterialTypeEnum[] materials);
    void ApplyRecipeEffect(RecipeType recipeType);
    MaterialData GetMaterialData(MaterialTypeEnum type);
    int GetMaterialValue(MaterialTypeEnum type);
    float CalculateSuccessRate(RecipeData recipe);
    RecipeType MatchRecipeByMaterials(MaterialTypeEnum[] materials);
}
