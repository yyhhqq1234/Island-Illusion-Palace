using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B9 RID: 697
	public abstract class PlayerLevelRewardsPoolBase : ICloneable, IEnumerable<IPlayerLevelReward>, IEnumerable, IWeighted
	{
		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x06001838 RID: 6200 RVA: 0x0004BFF8 File Offset: 0x0004A1F8
		// (set) Token: 0x06001839 RID: 6201 RVA: 0x0004C000 File Offset: 0x0004A200
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

		// Token: 0x0600183A RID: 6202 RVA: 0x0004C00E File Offset: 0x0004A20E
		protected PlayerLevelRewardsPoolBase(PlayerLevelRewardsPoolBase sourcePool)
		{
			this.weight = sourcePool.weight;
		}

		// Token: 0x0600183B RID: 6203
		public abstract void Prepare();

		// Token: 0x0600183C RID: 6204
		public abstract int GetRandomRewards(int maxRewardsCount, out IReadOnlyList<IPlayerLevelReward> rewards);

		// Token: 0x0600183D RID: 6205
		public abstract PlayerLevelRewardsPoolBase Clone();

		// Token: 0x0600183E RID: 6206 RVA: 0x0004C02D File Offset: 0x0004A22D
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		// Token: 0x0600183F RID: 6207
		public abstract IEnumerator<IPlayerLevelReward> GetEnumerator();

		// Token: 0x06001840 RID: 6208 RVA: 0x0004C035 File Offset: 0x0004A235
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x04000DAC RID: 3500
		[SerializeField]
		[Range(0f, 1f)]
		private float weight = 1f;
	}
}
