using System;
using Unliving.Pickables;

namespace Unliving.Purchasing
{
	// Token: 0x020000F0 RID: 240
	[Serializable]
	public class PurchasableNonFactoryItem : PurchasableItem<NonFactoryPickableType>
	{
		// Token: 0x060005C9 RID: 1481 RVA: 0x0001434D File Offset: 0x0001254D
		public PurchasableNonFactoryItem(NonFactoryPickableType itemType)
		{
			this.PurchaseItem = itemType;
		}
	}
}
