using System;
using Unliving.Currencies;

namespace Unliving.Purchasing
{
	// Token: 0x020000E3 RID: 227
	[Serializable]
	public abstract class PurchasableCurrencyArgsModifierBase
	{
		// Token: 0x06000590 RID: 1424
		public abstract bool IsMatch(IPurchasable purchasable);

		// Token: 0x06000591 RID: 1425
		public abstract void ApplyBuyArgsModification(ref ICurrencyOperationArgs buyArgs);

		// Token: 0x06000592 RID: 1426
		public abstract void ApplyUnlockArgsModification(ref ICurrencyOperationArgs unlockArgs);
	}
}
