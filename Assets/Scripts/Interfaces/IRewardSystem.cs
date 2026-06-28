using System.Collections.Generic;

public interface IRewardSystem
{
    void GrantSoulReward(int amount, string source);
    void GrantSoulCoreReward(EnemyAI.EnemyType enemyType, SoulCoreQuality quality);
    void GrantEssenceReward(int amount);
    void GrantMemoryFragmentReward();
    void GrantRewardsFromDropTable(DropTableData dropTable);
}