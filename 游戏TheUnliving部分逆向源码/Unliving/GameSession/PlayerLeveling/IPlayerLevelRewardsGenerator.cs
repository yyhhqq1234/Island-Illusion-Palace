using System;
using System.Collections.Generic;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B1 RID: 689
	public interface IPlayerLevelRewardsGenerator
	{
		// Token: 0x06001813 RID: 6163
		IReadOnlyList<IPlayerLevelReward> GenerateRewards(int maxRewardsCount);
	}
}
