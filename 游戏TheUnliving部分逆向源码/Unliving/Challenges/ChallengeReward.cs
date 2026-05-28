using System;
using Common;
using Game.Buffs;
using UnityEngine;
using UnityEngine.Events;
using Unliving.DropSystem;
using Unliving.Essence;

namespace Unliving.Challenges
{
	// Token: 0x02000350 RID: 848
	[Serializable]
	public class ChallengeReward : IWeighted
	{
		// Token: 0x170005B8 RID: 1464
		// (get) Token: 0x06001B79 RID: 7033 RVA: 0x0005673A File Offset: 0x0005493A
		// (set) Token: 0x06001B7A RID: 7034 RVA: 0x00056742 File Offset: 0x00054942
		public float Weight
		{
			get
			{
				return this.weight;
			}
			set
			{
				this.weight = value;
			}
		}

		// Token: 0x04000F6F RID: 3951
		[Range(0f, 1f)]
		public float weight;

		// Token: 0x04000F70 RID: 3952
		public UnityEvent customEvents;

		// Token: 0x04000F71 RID: 3953
		public DropSpawner dropSpawner;

		// Token: 0x04000F72 RID: 3954
		public EssenceSpawner essenceSpawner;

		// Token: 0x04000F73 RID: 3955
		public int chunksCountDuration;

		// Token: 0x04000F74 RID: 3956
		public int activatedMobsCountDuration;

		// Token: 0x04000F75 RID: 3957
		public BuffsGeneratorBuilderAsset.Reference[] rewardBuffs;
	}
}
