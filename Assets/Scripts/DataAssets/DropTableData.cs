using UnityEngine;
using System;
using System.Collections.Generic;
using GameSystems;

[CreateAssetMenu(fileName = "DropTable_", menuName = "IIP/Drop Table")]
public class DropTableData : ScriptableObject
{
    public List<DropEntry> entries = new List<DropEntry>();

    [Serializable]
    public class DropEntry
    {
        public string itemName = "";
        public DropType type = DropType.Soul;
        public int minAmount = 1;
        public int maxAmount = 5;
        [Range(0f, 1f)] public float probability = 1f;
        public MaterialTypeEnum materialType;
        public RecipeType recipeType;
        public SoulCoreQuality soulCoreQuality;
    }

    public enum DropType { Soul, SoulEssence, Material, Recipe, SoulCore, MemoryFragment }

    public List<DropRollResult> RollLoot(float luckMultiplier = 1f)
    {
        var results = new List<DropRollResult>();
        foreach (var entry in entries)
        {
            float adjustedProb = Mathf.Min(entry.probability * luckMultiplier, 1f);
            if (UnityEngine.Random.value <= adjustedProb)
            {
                int amount = UnityEngine.Random.Range(entry.minAmount, entry.maxAmount + 1);
                results.Add(new DropRollResult(entry, amount));
            }
        }
        return results;
    }
}

[Serializable]
public class DropRollResult
{
    public string itemName;
    public DropTableData.DropType type;
    public int amount;
    public MaterialTypeEnum materialType;
    public RecipeType recipeType;
    public SoulCoreQuality soulCoreQuality;

    public DropRollResult(DropTableData.DropEntry entry, int amount)
    {
        itemName = entry.itemName;
        type = entry.type;
        this.amount = amount;
        materialType = entry.materialType;
        recipeType = entry.recipeType;
        soulCoreQuality = entry.soulCoreQuality;
    }
}
