using System;
using System.Collections.Generic;

namespace Unliving.Purchasing
{
	// Token: 0x020000DF RID: 223
	public interface IPurchasablesExternalSource
	{
		// Token: 0x0600058A RID: 1418
		IList<IPurchasable> GetPurchasables();
	}
}
