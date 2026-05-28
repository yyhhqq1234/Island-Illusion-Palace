using System;
using Unliving.Abilities;
using Unliving.Currencies;

namespace Unliving.Purchasing
{
	// Token: 0x020000EA RID: 234
	[Serializable]
	public class PurchasableItemAbilityActivatedContainer : UpgradablePurchasableItem<AbilityID>
	{
		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060005A7 RID: 1447 RVA: 0x00013E69 File Offset: 0x00012069
		public override bool Purchased
		{
			get
			{
				return !base.Locked;
			}
		}

		// Token: 0x040003CB RID: 971
		public CurrencyOperationArgs destructionRewardArgs = new CurrencyOperationArgs
		{
			currencyID = CurrencyID.Cinder,
			amount = 8f
		};
	}
}
