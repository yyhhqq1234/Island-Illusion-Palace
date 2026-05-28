using System;
using Unliving.Currencies;

namespace Unliving.Purchasing
{
	// Token: 0x02000115 RID: 277
	[Serializable]
	public abstract class UpgradesPricesBase
	{
		// Token: 0x060006AA RID: 1706
		public abstract Type GetPurchasableType();

		// Token: 0x04000415 RID: 1045
		public CurrencyOperationArgs[] prices;
	}
}
