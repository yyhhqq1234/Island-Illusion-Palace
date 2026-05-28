using System;
using Unliving.Currencies;

namespace Unliving.Purchasing
{
	// Token: 0x020000E0 RID: 224
	public interface IUpgradable
	{
		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600058B RID: 1419
		bool IsMaxLevelReached { get; }

		// Token: 0x0600058C RID: 1420
		CurrencyOperationArgs GetCurrentUpgradeCurrencyArgs();

		// Token: 0x0600058D RID: 1421
		bool TryUpgrade();
	}
}
