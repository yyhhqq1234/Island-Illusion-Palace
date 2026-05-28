using System;
using Common;

namespace Unliving.Currencies
{
	// Token: 0x02000349 RID: 841
	public interface ICurrency : ICloneable<ICurrency>
	{
		// Token: 0x170005A8 RID: 1448
		// (get) Token: 0x06001B5E RID: 7006
		// (set) Token: 0x06001B5F RID: 7007
		float Amount { get; set; }

		// Token: 0x170005A9 RID: 1449
		// (get) Token: 0x06001B60 RID: 7008
		CurrencyID CurrencyID { get; }

		// Token: 0x170005AA RID: 1450
		// (get) Token: 0x06001B61 RID: 7009
		bool LocalProgressCurrency { get; }
	}
}
