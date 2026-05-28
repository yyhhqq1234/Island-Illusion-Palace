using System;

namespace Unliving.Purchasing
{
	// Token: 0x020000EF RID: 239
	[Serializable]
	public class PurchasableMilestoneItem : PurchasableItemBase
	{
		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060005C5 RID: 1477 RVA: 0x00014328 File Offset: 0x00012528
		public override object ObjectID
		{
			get
			{
				return this.milestoneID;
			}
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00014335 File Offset: 0x00012535
		public override IPurchasableData GetPurchasableData()
		{
			return new PurchasableData<MilestoneItemID>(this.milestoneID, 1);
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00014343 File Offset: 0x00012543
		public override void SetPurchasableData(IPurchasableData data)
		{
		}

		// Token: 0x040003DC RID: 988
		public MilestoneItemID milestoneID;
	}
}
