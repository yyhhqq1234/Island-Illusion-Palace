using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BossRewardConfig", menuName = "IIP/Boss Reward Config")]
public class BossRewardConfig : ScriptableObject
{
    [Header("灵魂掉落")]
    public Vector2Int soulDropRange = new Vector2Int(60, 90);

    [Header("灵魂精华掉落")]
    public Vector2Int essenceDropRange = new Vector2Int(3, 6);

    [Header("记忆碎片掉落")]
    [Range(0f, 1f)]
    public float memoryFragmentDropChance = 0.05f;

    [Header("灵魂之核品质")]
    public SoulCoreQuality soulCoreQuality = SoulCoreQuality.Gold;

    [Header("自定义奖励")]
    public List<CustomRewardEntry> customRewards = new List<CustomRewardEntry>();

    [Header("掉落表引用（可选）")]
    public DropTableData dropTable;

    public int RollSoulAmount()
    {
        return Random.Range(soulDropRange.x, soulDropRange.y + 1);
    }

    public int RollEssenceAmount()
    {
        return Random.Range(essenceDropRange.x, essenceDropRange.y + 1);
    }

    public bool ShouldDropMemoryFragment()
    {
        return Random.value <= memoryFragmentDropChance;
    }
}

[System.Serializable]
public class CustomRewardEntry
{
    public string rewardId;
    public int amount;
    [Range(0f, 1f)]
    public float dropChance = 1f;

    public bool ShouldDrop()
    {
        return Random.value <= dropChance;
    }
}