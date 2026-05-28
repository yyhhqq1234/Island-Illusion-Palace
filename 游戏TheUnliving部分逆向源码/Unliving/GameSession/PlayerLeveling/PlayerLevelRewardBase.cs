using System;
using Common;
using UnityEngine;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B6 RID: 694
	public abstract class PlayerLevelRewardBase : IPlayerLevelReward, ICloneable, IWeighted
	{
		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x0600182A RID: 6186 RVA: 0x0004BDDA File Offset: 0x00049FDA
		// (set) Token: 0x0600182B RID: 6187 RVA: 0x0004BDE2 File Offset: 0x00049FE2
		public int RewardRank
		{
			get
			{
				return this.rewardRank;
			}
			protected set
			{
				this.rewardRank = value;
			}
		}

		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x0600182C RID: 6188 RVA: 0x0004BDEB File Offset: 0x00049FEB
		// (set) Token: 0x0600182D RID: 6189 RVA: 0x0004BDF3 File Offset: 0x00049FF3
		public float Weight
		{
			get
			{
				return this.weight;
			}
			set
			{
				this.weight = Mathf.Clamp01(value);
			}
		}

		// Token: 0x0600182E RID: 6190 RVA: 0x0004BE01 File Offset: 0x0004A001
		protected PlayerLevelRewardBase(PlayerLevelRewardBase rewardPrototype)
		{
			this.rewardRank = rewardPrototype.rewardRank;
			this.weight = rewardPrototype.weight;
		}

		// Token: 0x0600182F RID: 6191 RVA: 0x0004BE21 File Offset: 0x0004A021
		protected PlayerLevelRewardBase(float weight, int rewardRank)
		{
			this.Weight = weight;
			this.rewardRank = Mathf.Max(rewardRank, 1);
		}

		// Token: 0x06001830 RID: 6192
		public abstract object Clone();

		// Token: 0x06001831 RID: 6193
		public abstract void Take(PlayerLevelRewardCollectionContext context);

		// Token: 0x04000DA6 RID: 3494
		[SerializeField]
		private int rewardRank;

		// Token: 0x04000DA7 RID: 3495
		[SerializeField]
		[Range(0f, 1f)]
		private float weight;
	}
}
