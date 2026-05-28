using System;
using Common.Editor;

namespace Unliving.Purchasing
{
	// Token: 0x020000EB RID: 235
	public abstract class PurchasableItem<T> : PurchasableItemBase where T : Enum
	{
		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060005A9 RID: 1449 RVA: 0x00013EAA File Offset: 0x000120AA
		public override object ObjectID
		{
			get
			{
				return this.PurchaseItem;
			}
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x00013EB7 File Offset: 0x000120B7
		public override IPurchasableData GetPurchasableData()
		{
			return new PurchasableData<T>(this.PurchaseItem, base.ItemLevel);
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x00013ECA File Offset: 0x000120CA
		public override void SetPurchasableData(IPurchasableData data)
		{
			base.ItemLevel = data.ItemLevel;
		}

		// Token: 0x040003CC RID: 972
		[EnumPopup]
		public T PurchaseItem;
	}
}
