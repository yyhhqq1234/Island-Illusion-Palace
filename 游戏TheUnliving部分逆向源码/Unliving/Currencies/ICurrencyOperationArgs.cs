using System;
using Common;

namespace Unliving.Currencies
{
	// Token: 0x0200034A RID: 842
	public interface ICurrencyOperationArgs : ICloneable<ICurrencyOperationArgs>
	{
		// Token: 0x170005AB RID: 1451
		// (get) Token: 0x06001B62 RID: 7010
		CurrencyID CurrencyID { get; }

		// Token: 0x170005AC RID: 1452
		// (get) Token: 0x06001B63 RID: 7011
		// (set) Token: 0x06001B64 RID: 7012
		float Amount { get; set; }

		// Token: 0x170005AD RID: 1453
		// (get) Token: 0x06001B65 RID: 7013
		bool Spending { get; }

		// Token: 0x170005AE RID: 1454
		// (get) Token: 0x06001B66 RID: 7014
		object Sender { get; }
	}
}
