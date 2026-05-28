using System;

namespace Unliving.Purchasing
{
	// Token: 0x02000116 RID: 278
	public abstract class UpgradesPricesBase<T> : UpgradesPricesBase where T : IPurchasable
	{
		// Token: 0x060006AC RID: 1708 RVA: 0x00015DAA File Offset: 0x00013FAA
		public override Type GetPurchasableType()
		{
			return typeof(T);
		}
	}
}
