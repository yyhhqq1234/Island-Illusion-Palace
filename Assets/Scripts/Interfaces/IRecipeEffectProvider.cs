using GameSystems;

public interface IRecipeEffectProvider
{
    void ApplyRecipeEffect(RecipeType recipeType);
    RecipeType MatchRecipeByMaterials(MaterialTypeEnum[] materials);
}
