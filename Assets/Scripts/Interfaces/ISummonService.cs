using System.Collections.Generic;
using UnityEngine;

public interface ISummonService
{
    List<SoulCoreData> SoulCoreInventory { get; }
    List<SoulCoreData> BattleSummons { get; }
    List<GameObject> ActiveSummons { get; }
    int MaxActiveSummons { get; }
    float SummonCooldown { get; }
    int MaxBattleSlots { get; }
    void SummonFromSlot(int slotIndex);
    void QuickSummon();
    void RecallAll();
    void AddSoulCore(EnemyAI.EnemyType enemyType, SoulCoreQuality quality);
    void SetBattleSummon(int slotIndex, SoulCoreData core);
    bool BreakthroughSoulCore(SoulCoreData core, int essenceCost);
    int GetBreakthroughEssenceCost(SoulCoreQuality quality);
}
