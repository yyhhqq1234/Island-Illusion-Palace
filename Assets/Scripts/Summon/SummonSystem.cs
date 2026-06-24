using UnityEngine;
using System.Collections.Generic;

public enum SoulCoreQuality { Common, Elite, Boss, Gold }



public class SummonSystem : MonoBehaviour, ISummonService
{
    [Header("灵魂之核库存")]
    public List<SoulCoreData> soulCoreInventory = new List<SoulCoreData>();
    public int baseInventorySize = 10;
    public int maxInventorySize = 10;

    [Header("出战召唤物设置")]
    public List<SoulCoreData> battleSummons = new List<SoulCoreData>();
    public int maxBattleSlots = 3;

    [Header("召唤系统设置")]
    public int maxActiveSummons = 3;
    public float summonCooldown = 2f;
    public float quickSummonCooldown = 1f;
    public float summonOffsetDistance = 1.5f;

    [Header("召唤物预制体")]
    public GameObject[] summonPrefabs;

    [Header("玩家引用")]
    public Transform playerTransform;

    [Header("玩家系统引用")]
    public CharacterStats characterStats;
    public BurdenSystem burdenSystem;

    [Header("记忆碎片")]
    public List<MemoryFragment> activeFragments = new List<MemoryFragment>();
    public int maxFragmentSlots = 3;

    private List<GameObject> activeSummons = new List<GameObject>();
    private float lastSummonTime = 0f;
    private int lastSummonedSlot = 0;

    List<SoulCoreData> ISummonService.soulCoreInventory => soulCoreInventory;
    List<SoulCoreData> ISummonService.battleSummons => battleSummons;
    List<GameObject> ISummonService.activeSummons => activeSummons;
    int ISummonService.maxActiveSummons => maxActiveSummons;
    float ISummonService.summonCooldown => summonCooldown;
    int ISummonService.maxBattleSlots => maxBattleSlots;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }
        if (characterStats == null)
        {
            characterStats = GetComponentInParent<CharacterStats>();
        }
        if (burdenSystem == null)
        {
            burdenSystem = GetComponentInParent<BurdenSystem>();
        }
        UpdateInventoryCapacity();
    }

    void Update()
    {
        // 移除了频繁的列表清理，改为在需要时调用
    }

    public void OpenSummonWheel()
    {
        Debug.Log("打开召唤轮盘");
    }

    public void SelectSummonFromWheel(Vector2 direction)
    {
        int slotIndex = GetSlotIndexFromDirection(direction);

        if (slotIndex >= 0 && slotIndex < battleSummons.Count)
        {
            SummonFromSlot(slotIndex);
        }
        else
        {
            Debug.Log("该方向没有设置召唤物");
        }
    }

    int GetSlotIndexFromDirection(Vector2 direction)
    {
        if (direction == Vector2.up) return 0;
        if (direction == Vector2.left) return 1;
        if (direction == Vector2.right) return 2;
        return -1;
    }

    public void QuickSummon()
    {
        if (Time.time - lastSummonTime < quickSummonCooldown)
        {
            Debug.Log($"快速召唤冷却中，还需等待 {quickSummonCooldown - (Time.time - lastSummonTime):F1} 秒");
            return;
        }

        if (battleSummons.Count > 0)
        {
            int slotToSummon = Mathf.Clamp(lastSummonedSlot, 0, battleSummons.Count - 1);
            SummonFromSlot(slotToSummon);
        }
        else
        {
            Debug.Log("没有设置出战召唤物");
        }
    }

    public void SummonFromSlot(int slotIndex)
    {
        if (Time.time - lastSummonTime < summonCooldown)
        {
            Debug.Log($"召唤冷却中，还需等待 {summonCooldown - (Time.time - lastSummonTime):F1} 秒");
            return;
        }

        // 清理无效的召唤物
        UpdateActiveSummons();

        if (activeSummons.Count >= maxActiveSummons)
        {
            Debug.Log("已达最大召唤数量");
            return;
        }

        if (slotIndex >= battleSummons.Count)
        {
            Debug.Log("该槽位没有召唤物");
            return;
        }

        SoulCoreData core = battleSummons[slotIndex];
        Vector3 summonPosition = GetRandomSummonPosition();
        SummonCreature(core, summonPosition);
        lastSummonTime = Time.time;
        lastSummonedSlot = slotIndex;
    }

    public void RecallAllSummons()
    {
        // 清理无效的召唤物
        UpdateActiveSummons();
        
        Debug.Log($"召回所有召唤物，共 {activeSummons.Count} 个");

        for (int i = activeSummons.Count - 1; i >= 0; i--)
        {
            RecallSummon(activeSummons[i]);
        }
    }

    void RecallSummon(GameObject summon)
    {
        if (summon == null) return;

        var summonAI = summon.GetComponent<SummonedCreatureAI>();
        if (summonAI != null)
        {
            summonAI.MarkAsRecalled();
        }

        activeSummons.Remove(summon);
        Destroy(summon, 0.5f);
    }

    public void AddSoulCore(EnemyAI.EnemyType enemyType, SoulCoreQuality quality)
    {
        if (soulCoreInventory.Count >= maxInventorySize)
        {
            Debug.Log("灵魂之核库存已满");
            return;
        }

        SoulCoreData newCore = new SoulCoreData(enemyType, quality.ToString(), 1);
        soulCoreInventory.Add(newCore);
        Debug.Log($"获得灵魂之核：{enemyType} ({quality})");
    }

    public bool EquipToEmptySlot(SoulCoreData core)
    {
        for (int i = 0; i < maxBattleSlots; i++)
        {
            if (IsSlotEmpty(i))
            {
                SetBattleSummon(i, core);
                return true;
            }
        }
        return false;
    }

    public bool IsSlotEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= battleSummons.Count)
            return true;

        return battleSummons[slotIndex] == null;
    }

    public int GetFirstAvailableSlot()
    {
        for (int i = 0; i < maxBattleSlots; i++)
        {
            if (IsSlotEmpty(i))
                return i;
        }
        return -1;
    }

    public void SetBattleSummon(int slotIndex, SoulCoreData core)
    {
        if (slotIndex < 0 || slotIndex >= maxBattleSlots)
        {
            Debug.Log("无效的槽位索引");
            return;
        }

        while (battleSummons.Count <= slotIndex)
        {
            battleSummons.Add(null);
        }

        battleSummons[slotIndex] = core;
        Debug.Log($"设置出战召唤物槽位 {slotIndex}：{core.enemyType}");
    }

    public void SummonCreature(SoulCoreData core, Vector3 position)
    {
        EnemyAI.EnemyType enemyType = core.enemyType;
        CreatureType creatureType = CreatureTypeConverter.FromEnemyType(enemyType);
        int typeIndex = (int)enemyType;
        if (summonPrefabs == null || typeIndex < 0 || typeIndex >= summonPrefabs.Length)
        {
            Debug.LogError($"召唤失败：预制体索引 {typeIndex} 超出范围");
            return;
        }

        GameObject summonPrefab = summonPrefabs[typeIndex];
        if (summonPrefab == null)
        {
            Debug.LogError($"未找到 {enemyType} 的召唤物预制体");
            return;
        }

        GameObject summonedCreature = Instantiate(summonPrefab, position, Quaternion.identity);
        summonedCreature.tag = "SummonedCreature";

        var healthSystem = summonedCreature.GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            healthSystem = summonedCreature.AddComponent<HealthSystem>();
        }

        float baseHealth = GetBaseHealthForType(creatureType);
        healthSystem.maxHealth = baseHealth * (1 + core.level * 0.05f);

        var summonAI = summonedCreature.GetComponent<SummonedCreatureAI>();
        if (summonAI == null)
        {
            summonAI = summonedCreature.AddComponent<SummonedCreatureAI>();
        }

        summonAI.SetSoulCoreData(core);
        summonAI.SetPlayer(playerTransform);

        if (core.quality == "Common")
            summonAI.lifetime = 75f;
        else
            summonAI.isPermanent = true;

        ApplyMemoryFragmentEffects(summonedCreature, summonAI);

        EnemyAI enemyAI = summonedCreature.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        activeSummons.Add(summonedCreature);
        Debug.Log($"成功召唤 {core.enemyType} (Lv.{core.level})！");
    }

    float GetBaseHealthForType(CreatureType type)
    {
        return CreatureStatsDatabase.GetBaseHealthForType(type);
    }

    void ApplyMemoryFragmentEffects(GameObject creature, SummonedCreatureAI summonAI)
    {
        if (summonAI == null || activeFragments.Count == 0)
        {
            return;
        }
        
        // 应用记忆碎片效果到召唤物
        foreach (var fragment in activeFragments)
        {
            if (fragment == null)
            {
                continue;
            }
            
            switch (fragment.fragmentType)
            {
                case MemoryFragmentType.Childhood:
                    summonAI.healthBonus += 0.15f;
                    summonAI.cooldownReduction += 1f;
                    break;
                case MemoryFragmentType.Battle:
                    summonAI.damageBonus += 0.20f;
                    summonAI.critBonus += 0.05f;
                    break;
                case MemoryFragmentType.Emotion:
                    summonAI.defenseBonus += 0.25f;
                    summonAI.healingBonus += 0.10f;
                    break;
                case MemoryFragmentType.Knowledge:
                    summonAI.skillCooldownReduction += 0.30f;
                    summonAI.controlBonus += 0.20f;
                    break;
                case MemoryFragmentType.Sacrifice:
                    summonAI.deathExplosion = true;
                    break;
                case MemoryFragmentType.Hope:
                    summonAI.healthRegen += 0.01f;
                    break;
                case MemoryFragmentType.Truth:
                    summonAI.bossDamageBonus += 0.35f;
                    break;
            }
        }
    }

    Vector3 GetRandomSummonPosition()
    {
        // 生成随机角度
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // 计算偏移位置
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * summonOffsetDistance,
            Mathf.Sin(angle) * summonOffsetDistance,
            0f
        );
        
        // 返回最终召唤位置
        return playerTransform.position + offset;
    }

    void UpdateActiveSummons()
    {
        activeSummons.RemoveAll(s => s == null);
    }

    public int GetActiveSummonCount() => activeSummons.Count;
    public int GetSoulCoreCount() => soulCoreInventory.Count;
    public int GetRemainingSummonSlots() => maxActiveSummons - activeSummons.Count;
    public List<SoulCoreData> GetSoulCoreInventory() => new List<SoulCoreData>(soulCoreInventory);

    public void AddMemoryFragment(MemoryFragment fragment)
    {
        if (activeFragments.Count >= maxFragmentSlots)
        {
            Debug.Log("记忆碎片槽位已满");
            return;
        }

        // 计算记忆碎片的负担
        float burdenCost = CalculateFragmentBurden(fragment);
        fragment.burdenCost = burdenCost;

        // 检查负担是否超过限制
        if (burdenSystem != null)
        {
            float totalBurden = GetTotalFragmentBurden() + burdenCost;
            if (totalBurden > burdenSystem.maxBurden)
            {
                Debug.Log("激活该记忆碎片会超出负担上限");
                return;
            }

            // 添加负担
            burdenSystem.AddBurden(burdenCost);
        }

        activeFragments.Add(fragment);
        Debug.Log($"激活记忆碎片：{fragment.fragmentName}，负担：{fragment.burdenCost}");
    }

    public void RemoveMemoryFragment(MemoryFragment fragment)
    {
        if (activeFragments.Contains(fragment))
        {
            // 移除负担
            if (burdenSystem != null)
            {
                burdenSystem.RemoveBurden(fragment.burdenCost);
            }

            activeFragments.Remove(fragment);
            Debug.Log($"移除记忆碎片：{fragment.fragmentName}");
        }
    }

    float CalculateFragmentBurden(MemoryFragment fragment)
    {
        float baseBurden = 10f; // 基础负担
        float burdenResistance = 0f;

        if (burdenSystem != null)
        {
            burdenResistance = burdenSystem.GetBurdenResistance();
        }

        return baseBurden * (1 - burdenResistance);
    }

    float GetTotalFragmentBurden()
    {
        float totalBurden = 0f;
        foreach (var fragment in activeFragments)
        {
            totalBurden += fragment.burdenCost;
        }
        return totalBurden;
    }

    void UpdateInventoryCapacity()
    {
        maxInventorySize = baseInventorySize;
        if (characterStats != null)
        {
            int wisdomBonus = characterStats.wisdom / 5;
            wisdomBonus = Mathf.Min(wisdomBonus, 5); // 最大+5槽位
            maxInventorySize += wisdomBonus;
        }
        Debug.Log($"更新灵魂之核库存容量：{maxInventorySize}");
    }

    public void BreakthroughSoulCore(SoulCoreData core, int essenceCost)
    {
        if (core.CanBreakthrough())
        {
            core.Breakthrough();
            Debug.Log($"灵魂之核突破成功，消耗精华：{essenceCost}");
        }
        else
        {
            Debug.Log("灵魂之核无法突破");
        }
    }

    public int GetBreakthroughEssenceCost(SoulCoreQuality quality)
    {
        return quality switch
        {
            SoulCoreQuality.Common => 1,
            SoulCoreQuality.Elite => 2,
            SoulCoreQuality.Boss => 3,
            _ => 1
        };
    }

    public List<GameObject> GetActiveSummons() => activeSummons;
    public float GetSummonCooldownRemaining() => Mathf.Max(0f, summonCooldown - (Time.time - lastSummonTime));
    public float GetQuickSummonCooldownRemaining() => Mathf.Max(0f, quickSummonCooldown - (Time.time - lastSummonTime));
}


