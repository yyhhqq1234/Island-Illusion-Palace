using System.Collections.Generic;
using UnityEngine;

public interface ISummonService
{
    List<SoulCoreData> soulCoreInventory { get; }
    List<SoulCoreData> battleSummons { get; }
    List<GameObject> activeSummons { get; }
    int maxActiveSummons { get; }
    float summonCooldown { get; }
    int maxBattleSlots { get; }
    void SummonFromSlot(int slotIndex);
    void QuickSummon();
    void RecallAllSummons();
    void AddSoulCore(EnemyAI.EnemyType enemyType, SoulCoreQuality quality);
    void SetBattleSummon(int slotIndex, SoulCoreData core);
    void BreakthroughSoulCore(SoulCoreData core, int essenceCost);
    int GetBreakthroughEssenceCost(SoulCoreQuality quality);
}
