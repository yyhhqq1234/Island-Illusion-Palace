using System;
using System.Collections.Generic;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B3 RID: 691
	public interface IPlayerLevelingManager
	{
		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x06001815 RID: 6165
		bool IsActive { get; }

		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x06001816 RID: 6166
		int CurrentPlayerLevel { get; }

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x06001817 RID: 6167
		int DesiredPlayerLevel { get; }

		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x06001818 RID: 6168
		int MaxPlayerLevel { get; }

		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x06001819 RID: 6169
		int LastPlayerLevelEXP { get; }

		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x0600181A RID: 6170
		int CurrentPlayerEXP { get; }

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x0600181B RID: 6171
		int NextPlayerLevelEXP { get; }

		// Token: 0x140000EB RID: 235
		// (add) Token: 0x0600181C RID: 6172
		// (remove) Token: 0x0600181D RID: 6173
		event Action<IPlayerLevelingManager, int, int> PlayerEXPChanged;

		// Token: 0x140000EC RID: 236
		// (add) Token: 0x0600181E RID: 6174
		// (remove) Token: 0x0600181F RID: 6175
		event Action<IPlayerLevelingManager, int> NewPlayerLevelEXPReached;

		// Token: 0x140000ED RID: 237
		// (add) Token: 0x06001820 RID: 6176
		// (remove) Token: 0x06001821 RID: 6177
		event Action<IPlayerLevelingManager> PlayerLevelRewardsReset;

		// Token: 0x140000EE RID: 238
		// (add) Token: 0x06001822 RID: 6178
		// (remove) Token: 0x06001823 RID: 6179
		event Action<IPlayerLevelingManager, int, IReadOnlyList<IPlayerLevelReward>> NewPlayerLevelRewardsGenerated;

		// Token: 0x06001824 RID: 6180
		bool AddPlayerLevelEXP(int amount, out int addedAmount);

		// Token: 0x06001825 RID: 6181
		bool RerollCurrentRewards(out IReadOnlyList<IPlayerLevelReward> updatedRewards);

		// Token: 0x06001826 RID: 6182
		bool TakeNewPlayerLevelReward(IPlayerLevelReward playerLevelReward);

		// Token: 0x06001827 RID: 6183
		void ResetPlayerLevelRewards();

		// Token: 0x06001828 RID: 6184
		void ResetPlayerLevelRewardsGenerationTime();
	}
}
