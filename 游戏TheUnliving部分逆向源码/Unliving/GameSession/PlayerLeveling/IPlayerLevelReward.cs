using System;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B0 RID: 688
	public interface IPlayerLevelReward : ICloneable
	{
		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x06001811 RID: 6161
		int RewardRank { get; }

		// Token: 0x06001812 RID: 6162
		void Take(PlayerLevelRewardCollectionContext context);
	}
}
