using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B8 RID: 696
	public sealed class PlayerLevelRewardsGenerator : IPlayerLevelRewardsGenerator
	{
		// Token: 0x06001833 RID: 6195 RVA: 0x0004BE48 File Offset: 0x0004A048
		private static void CreateRewards(PlayerLevelRewardsPoolBase rewardPool, int maxRewardsCount, List<IPlayerLevelReward> targetList)
		{
			IReadOnlyList<IPlayerLevelReward> readOnlyList;
			int randomRewards = rewardPool.GetRandomRewards(maxRewardsCount, out readOnlyList);
			for (int i = 0; i < randomRewards; i++)
			{
				targetList.Add((IPlayerLevelReward)readOnlyList[i].Clone());
			}
		}

		// Token: 0x06001834 RID: 6196 RVA: 0x0004BE84 File Offset: 0x0004A084
		public PlayerLevelRewardsGenerator(IList<PlayerLevelRewardsPoolBase> sourceRewardPools)
		{
			this.rewardPools = new PlayerLevelRewardsPoolBase[sourceRewardPools.Count];
			for (int i = 0; i < sourceRewardPools.Count; i++)
			{
				this.rewardPools[i] = sourceRewardPools[i].Clone();
				this.rewardPools[i].Prepare();
				this.rewardPoolsWeight += this.rewardPools[i].Weight;
			}
		}

		// Token: 0x06001835 RID: 6197 RVA: 0x0004BEF4 File Offset: 0x0004A0F4
		public PlayerLevelRewardsGenerator(IList<PlayerLevelRewardsPoolAssetBase> sourceRewardPools)
		{
			this.rewardPools = new PlayerLevelRewardsPoolBase[sourceRewardPools.Count];
			for (int i = 0; i < sourceRewardPools.Count; i++)
			{
				this.rewardPools[i] = sourceRewardPools[i].RewardPool.Clone();
				this.rewardPools[i].Prepare();
				this.rewardPoolsWeight += this.rewardPools[i].Weight;
			}
		}

		// Token: 0x06001836 RID: 6198 RVA: 0x0004BF6C File Offset: 0x0004A16C
		public IReadOnlyList<IPlayerLevelReward> GenerateRewards(int maxRewardsCount)
		{
			PlayerLevelRewardsGenerator.GeneratedRewardsBuffer.Clear();
			if (this.rewardPools.Length > 1)
			{
				for (int i = 0; i < maxRewardsCount; i++)
				{
					PlayerLevelRewardsPoolBase rewardPool;
					if (this.rewardPools.GetRandomWeightedItem(out rewardPool, 0, 2147483647, new float?(this.rewardPoolsWeight)))
					{
						PlayerLevelRewardsGenerator.CreateRewards(rewardPool, 1, PlayerLevelRewardsGenerator.GeneratedRewardsBuffer);
					}
				}
			}
			else if (this.rewardPools.Length == 1)
			{
				PlayerLevelRewardsGenerator.CreateRewards(this.rewardPools[0], maxRewardsCount, PlayerLevelRewardsGenerator.GeneratedRewardsBuffer);
			}
			return PlayerLevelRewardsGenerator.GeneratedRewardsBuffer;
		}

		// Token: 0x04000DA9 RID: 3497
		private static readonly List<IPlayerLevelReward> GeneratedRewardsBuffer = new List<IPlayerLevelReward>(4);

		// Token: 0x04000DAA RID: 3498
		private readonly PlayerLevelRewardsPoolBase[] rewardPools;

		// Token: 0x04000DAB RID: 3499
		private readonly float rewardPoolsWeight;
	}
}
