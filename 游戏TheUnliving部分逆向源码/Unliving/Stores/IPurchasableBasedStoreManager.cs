using System;
using Unliving.Currencies;
using Unliving.Purchasing;

namespace Unliving.Stores
{
	// Token: 0x0200004A RID: 74
	public interface IPurchasableBasedStoreManager : IStoreManager
	{
		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600024F RID: 591
		CurrencyID CurrencyID { get; }

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x06000250 RID: 592
		// (remove) Token: 0x06000251 RID: 593
		event Action<IPurchasable> StoreItemPurchased;

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x06000252 RID: 594
		// (remove) Token: 0x06000253 RID: 595
		event Action<IPurchasable> StoreItemUpgraded;

		// Token: 0x06000254 RID: 596
		bool TryPurchaseItem(IPurchasable item);

		// Token: 0x06000255 RID: 597
		bool TryUpgradeItem(IPurchasable item);
	}
}
