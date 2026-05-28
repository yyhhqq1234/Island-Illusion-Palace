using System;
using Unliving.Currencies;
using Unliving.Essence;

namespace Unliving.Purchasing
{
	// Token: 0x020000E9 RID: 233
	[Serializable]
	public class PurchasableEssence : PurchasableItem<EssenceType>
	{
		// Token: 0x060005A6 RID: 1446 RVA: 0x00013E5A File Offset: 0x0001205A
		public PurchasableEssence(EssenceType essenceType)
		{
			this.PurchaseItem = essenceType;
		}

		// Token: 0x040003CA RID: 970
		public CurrencyOperationArgs closeRewardArgs;
	}
}
