using System;
using UnityEngine;
using Unliving.Player.Upgrades;

namespace Unliving.Purchasing
{
	// Token: 0x020000F3 RID: 243
	[Serializable]
	public class PurchasablePlayerUpgrade : PurchasableItem<PlayerUpgradeID>
	{
		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x00014566 File Offset: 0x00012766
		public override bool Purchased
		{
			get
			{
				return base.Purchased || !base.Locked;
			}
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x0001457B File Offset: 0x0001277B
		public PurchasablePlayerUpgrade(PlayerUpgradeID playerUpgradeID)
		{
			this.PurchaseItem = playerUpgradeID;
		}

		// Token: 0x040003E4 RID: 996
		public ScriptableObject upgradePrototype;
	}
}
