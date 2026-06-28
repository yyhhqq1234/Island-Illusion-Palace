using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour, IRewardSystem
{
    private IInventoryService inventoryService;
    private ISummonService summonService;
    private IMemoryFragmentService memoryFragmentService;

    void Start()
    {
        InitializeServices();
    }

    void InitializeServices()
    {
        var inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem != null)
            inventoryService = inventorySystem;

        var summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem != null)
            summonService = summonSystem;

        var memoryFragment = FindObjectOfType<MemoryFragmentSystem>();
        if (memoryFragment != null)
            memoryFragmentService = memoryFragment;

        Debug.Log($"[RewardManager] 服务初始化完成: Inventory={inventoryService != null}, Summon={summonService != null}, MemoryFragment={memoryFragmentService != null}");
    }

    public void GrantSoulReward(int amount, string source)
    {
        if (inventoryService != null)
        {
            inventoryService.AddSouls(amount);
            Debug.Log($"[RewardManager] 获得 {amount} 灵魂 (来源: {source})");
        }
        else
        {
            Debug.LogWarning($"[RewardManager] InventoryService 未初始化，无法发放 {amount} 灵魂");
        }
    }

    public void GrantSoulCoreReward(EnemyAI.EnemyType enemyType, SoulCoreQuality quality)
    {
        if (summonService != null)
        {
            summonService.AddSoulCore(enemyType, quality);
            Debug.Log($"[RewardManager] 获得 {enemyType} 灵魂之核 ({quality})");
        }
        else
        {
            Debug.LogWarning($"[RewardManager] SummonService 未初始化，无法发放 {enemyType} 灵魂之核");
        }
    }

    public void GrantEssenceReward(int amount)
    {
        if (inventoryService != null)
        {
            inventoryService.currentSoulEssence = Mathf.Min(
                inventoryService.currentSoulEssence + amount,
                ((InventorySystem)inventoryService).maxSoulEssence
            );
            Debug.Log($"[RewardManager] 获得 {amount} 灵魂精华");
        }
        else
        {
            Debug.LogWarning($"[RewardManager] InventoryService 未初始化，无法发放 {amount} 灵魂精华");
        }
    }

    public void GrantMemoryFragmentReward()
    {
        if (memoryFragmentService != null)
        {
            MemoryFragmentType randomType = (MemoryFragmentType)Random.Range(0, System.Enum.GetValues(typeof(MemoryFragmentType)).Length);
            memoryFragmentService.CollectFragment(randomType);
            GlobalEventManager.Instance.ShowNotification("获得：记忆碎片！", 3f);
            Debug.Log($"[RewardManager] 获得记忆碎片: {randomType}");
        }
        else
        {
            Debug.LogWarning("[RewardManager] MemoryFragmentService 未初始化，无法发放记忆碎片");
        }
    }

    public void GrantRewardsFromDropTable(DropTableData dropTable)
    {
        if (dropTable == null)
        {
            Debug.LogWarning("[RewardManager] DropTable 为空");
            return;
        }

        var results = dropTable.RollLoot();
        foreach (var result in results)
        {
            switch (result.type)
            {
                case DropTableData.DropType.Soul:
                    GrantSoulReward(result.amount, "DropTable");
                    break;
                case DropTableData.DropType.SoulEssence:
                    GrantEssenceReward(result.amount);
                    break;
                case DropTableData.DropType.SoulCore:
                    GrantSoulCoreReward(EnemyAI.EnemyType.TimeGuardian, result.soulCoreQuality);
                    break;
                case DropTableData.DropType.MemoryFragment:
                    GrantMemoryFragmentReward();
                    break;
                case DropTableData.DropType.Material:
                    if (inventoryService != null)
                        inventoryService.AddMaterial(result.materialType, result.amount);
                    break;
                case DropTableData.DropType.Recipe:
                    if (inventoryService != null)
                        inventoryService.AddPotion(result.recipeType, result.amount);
                    break;
            }
        }
    }

    public static IRewardSystem Instance
    {
        get
        {
            var manager = FindObjectOfType<RewardManager>();
            if (manager == null)
            {
                var go = new GameObject("[IIP] RewardManager");
                manager = go.AddComponent<RewardManager>();
                DontDestroyOnLoad(go);
            }
            return manager;
        }
    }
}