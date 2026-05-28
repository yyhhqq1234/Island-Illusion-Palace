using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.CollectionsExtensions;
using UnityEngine;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002BA RID: 698
	[Serializable]
	public class PlayerLevelRewardsPool<TReward> : PlayerLevelRewardsPoolBase where TReward : IPlayerLevelReward, ICloneable, IWeighted
	{
		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x06001841 RID: 6209 RVA: 0x0004C03D File Offset: 0x0004A23D
		// (set) Token: 0x06001842 RID: 6210 RVA: 0x0004C048 File Offset: 0x0004A248
		public float[] RewardRankProbabilities
		{
			get
			{
				return this.rewardRankProbabilities;
			}
			set
			{
				this.rewardRankProbabilities = value;
				if (this.rewardRankProbabilities == null)
				{
					return;
				}
				for (int i = 0; i < this.rewardRankProbabilities.Length; i++)
				{
					this.rewardRankProbabilities[i] = Mathf.Clamp01(this.rewardRankProbabilities[i]);
				}
			}
		}

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x06001843 RID: 6211 RVA: 0x0004C08D File Offset: 0x0004A28D
		// (set) Token: 0x06001844 RID: 6212 RVA: 0x0004C095 File Offset: 0x0004A295
		public TReward[] Rewards
		{
			get
			{
				return this.rewards;
			}
			set
			{
				this.rewards = value;
			}
		}

		// Token: 0x06001845 RID: 6213 RVA: 0x0004C0A0 File Offset: 0x0004A2A0
		private int GetRewardRankIndex()
		{
			if (this.rewardRankProbabilities != null)
			{
				float num = 0f;
				for (int i = 0; i < this.rewardRankProbabilities.Length; i++)
				{
					num += this.rewardRankProbabilities[i];
				}
				float num2 = UnityEngine.Random.value * num;
				for (int j = 0; j < this.rewardRankProbabilities.Length; j++)
				{
					float num3 = this.rewardRankProbabilities[j];
					if (num3 > 0f)
					{
						if (num2 <= num3)
						{
							return j;
						}
						num2 -= num3;
					}
				}
			}
			return 0;
		}

		// Token: 0x06001846 RID: 6214 RVA: 0x0004C114 File Offset: 0x0004A314
		public PlayerLevelRewardsPool(PlayerLevelRewardsPool<TReward> sourcePool) : base(sourcePool)
		{
			this.rewardRankProbabilities = sourcePool.rewardRankProbabilities.CloneArray<float>();
			this.rewards = sourcePool.GetClonedRewards();
		}

		// Token: 0x06001847 RID: 6215 RVA: 0x0004C13C File Offset: 0x0004A33C
		public TReward[] GetClonedRewards()
		{
			TReward[] array = new TReward[this.rewards.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (TReward)((object)this.rewards[i].Clone());
			}
			return array;
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x0004C18C File Offset: 0x0004A38C
		public override void Prepare()
		{
			if (this.rewards.Length == 0)
			{
				return;
			}
			Array.Sort<TReward>(this.rewards, new Comparison<TReward>(PlayerLevelRewardsPool<TReward>.<Prepare>g__CompareRewardsByRank|15_0));
			int num = 1;
			int num2 = 0;
			float num3 = 0f;
			if (this.rewardsByRank == null)
			{
				this.rewardsByRank = new List<PlayerLevelRewardsPool<TReward>.RankedRewardsSlice>(4);
			}
			for (int i = 0; i < this.rewards.Length; i++)
			{
				TReward treward = this.rewards[i];
				int num4 = Mathf.Max(treward.RewardRank, 1);
				num3 += treward.Weight;
				if (num4 != num)
				{
					this.rewardsByRank.Add(new PlayerLevelRewardsPool<TReward>.RankedRewardsSlice(num2, i - num2, num3));
					num2 = i;
					num = num4;
					num3 = 0f;
				}
			}
			this.rewardsByRank.Add(new PlayerLevelRewardsPool<TReward>.RankedRewardsSlice(num2, this.rewards.Length - num2, num3));
		}

		// Token: 0x06001849 RID: 6217 RVA: 0x0004C260 File Offset: 0x0004A460
		public sealed override int GetRandomRewards(int maxRewardsCount, out IReadOnlyList<IPlayerLevelReward> selectedRewards)
		{
			PlayerLevelRewardsPool<TReward>.RandomRewardsBuffer.Clear();
			PlayerLevelRewardsPool<TReward>.RewardWeightsBuffer.Clear();
			selectedRewards = PlayerLevelRewardsPool<TReward>.RandomRewardsBuffer;
			for (int i = 0; i < maxRewardsCount; i++)
			{
				int rewardRankIndex = this.GetRewardRankIndex();
				PlayerLevelRewardsPool<TReward>.RankedRewardsSlice rankedRewardsSlice = this.rewardsByRank[rewardRankIndex];
				TReward treward;
				int item;
				if (rankedRewardsSlice.currentWeightSum >= 0.001f && this.rewards.GetRandomWeightedItem(out treward, out item, rankedRewardsSlice.StartIndex, rankedRewardsSlice.Count, new float?(rankedRewardsSlice.currentWeightSum)))
				{
					PlayerLevelRewardsPool<TReward>.RandomRewardsBuffer.Add(treward);
					float weight = treward.Weight;
					rankedRewardsSlice.currentWeightSum -= weight;
					PlayerLevelRewardsPool<TReward>.RewardWeightsBuffer.Add(new ValueTuple<int, float>(item, weight));
					treward.Weight = 0f;
					this.rewardsByRank[rewardRankIndex] = rankedRewardsSlice;
				}
			}
			for (int j = 0; j < PlayerLevelRewardsPool<TReward>.RewardWeightsBuffer.Count; j++)
			{
				ValueTuple<int, float> valueTuple = PlayerLevelRewardsPool<TReward>.RewardWeightsBuffer[j];
				int item2 = valueTuple.Item1;
				float item3 = valueTuple.Item2;
				this.rewards[item2].Weight = item3;
			}
			for (int k = 0; k < this.rewardsByRank.Count; k++)
			{
				PlayerLevelRewardsPool<TReward>.RankedRewardsSlice value = this.rewardsByRank[k];
				value.ResetWeightSum();
				this.rewardsByRank[k] = value;
			}
			return PlayerLevelRewardsPool<TReward>.RandomRewardsBuffer.Count;
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x0004C3DD File Offset: 0x0004A5DD
		public override PlayerLevelRewardsPoolBase Clone()
		{
			return new PlayerLevelRewardsPool<TReward>(this);
		}

		// Token: 0x0600184B RID: 6219 RVA: 0x0004C3E5 File Offset: 0x0004A5E5
		public sealed override IEnumerator<IPlayerLevelReward> GetEnumerator()
		{
			return (IEnumerator<IPlayerLevelReward>)this.rewards.AsEnumerable<TReward>().GetEnumerator();
		}

		// Token: 0x0600184D RID: 6221 RVA: 0x0004C414 File Offset: 0x0004A614
		[CompilerGenerated]
		internal static int <Prepare>g__CompareRewardsByRank|15_0(TReward reward0, TReward reward1)
		{
			return reward0.RewardRank.CompareTo(reward1.RewardRank);
		}

		// Token: 0x04000DAD RID: 3501
		private static readonly List<IPlayerLevelReward> RandomRewardsBuffer = new List<IPlayerLevelReward>(4);

		// Token: 0x04000DAE RID: 3502
		[TupleElementNames(new string[]
		{
			"rewardIndex",
			"rewardWeight"
		})]
		private static readonly List<ValueTuple<int, float>> RewardWeightsBuffer = new List<ValueTuple<int, float>>(8);

		// Token: 0x04000DAF RID: 3503
		[SerializeField]
		[Range(0f, 1f)]
		private float[] rewardRankProbabilities;

		// Token: 0x04000DB0 RID: 3504
		[SerializeField]
		private TReward[] rewards;

		// Token: 0x04000DB1 RID: 3505
		private List<PlayerLevelRewardsPool<TReward>.RankedRewardsSlice> rewardsByRank;

		// Token: 0x02000524 RID: 1316
		private struct RankedRewardsSlice
		{
			// Token: 0x06002644 RID: 9796 RVA: 0x00077E32 File Offset: 0x00076032
			public RankedRewardsSlice(int startIndex, int count, float initialWeightSum)
			{
				this.StartIndex = startIndex;
				this.Count = count;
				this.InitialWeightSum = initialWeightSum;
				this.currentWeightSum = initialWeightSum;
			}

			// Token: 0x06002645 RID: 9797 RVA: 0x00077E50 File Offset: 0x00076050
			public void ResetWeightSum()
			{
				this.currentWeightSum = this.InitialWeightSum;
			}

			// Token: 0x04001B3C RID: 6972
			public readonly int StartIndex;

			// Token: 0x04001B3D RID: 6973
			public readonly int Count;

			// Token: 0x04001B3E RID: 6974
			public readonly float InitialWeightSum;

			// Token: 0x04001B3F RID: 6975
			public float currentWeightSum;
		}
	}
}
