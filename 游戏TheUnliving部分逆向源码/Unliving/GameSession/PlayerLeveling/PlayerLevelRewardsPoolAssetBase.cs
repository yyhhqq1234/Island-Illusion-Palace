using System;
using UnityEngine;
using Unliving.DataParsing;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002BB RID: 699
	public abstract class PlayerLevelRewardsPoolAssetBase : CSVTableDataAssetBase<IPlayerLevelReward[]>
	{
		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x0600184E RID: 6222
		public abstract PlayerLevelRewardsPoolBase RewardPool { get; }

		// Token: 0x0600184F RID: 6223 RVA: 0x0004C443 File Offset: 0x0004A643
		protected sealed override TextAsset GetTableAsset()
		{
			return this.rewardsPoolTable;
		}

		// Token: 0x04000DB2 RID: 3506
		[SerializeField]
		private TextAsset rewardsPoolTable;
	}
}
