using UnityEngine;
using System.Collections.Generic;
using GameSystems;

[CreateAssetMenu(fileName = "RecipeEffect_", menuName = "IIP/Recipe Effect Config")]
public class RecipeEffectConfig : ScriptableObject
{
    public string displayName = "";
    public RecipeType recipeType;

    [Header("直接效果")]
    public List<ScriptedAbilityEffect> effects = new List<ScriptedAbilityEffect>();

    [Header("配方元数据")]
    public RecipeTier tier;
    public int alchemicalValue;
    [TextArea] public string description;

    [Header("材料")]
    public List<MaterialRequirement> materials = new List<MaterialRequirement>();

    [System.Serializable]
    public class MaterialRequirement
    {
        public MaterialTypeEnum materialType;
        public int count = 1;
    }

    public void ExecuteAllEffects(MonoBehaviour context)
    {
        foreach (var effect in effects)
        {
            if (effect != null) effect.Execute(context);
        }
    }

    public RecipeData ToRecipeData()
    {
        Dictionary<MaterialTypeEnum, int> requiredMaterials = new Dictionary<MaterialTypeEnum, int>();
        foreach (var req in materials)
        {
            if (req.count > 0)
                requiredMaterials[req.materialType] = req.count;
        }

        return new RecipeData(recipeType, displayName, tier, requiredMaterials, description, alchemicalValue);
    }
}
