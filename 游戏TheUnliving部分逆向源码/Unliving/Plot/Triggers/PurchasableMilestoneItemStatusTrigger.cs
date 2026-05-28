using System;
using Common.Editor;
using Unliving.Purchasing;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F7 RID: 759
	[Serializable]
	public class PurchasableMilestoneItemStatusTrigger : PurchasableItemStatusTriggerBase<MilestoneItemID>
	{
		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x060019E8 RID: 6632 RVA: 0x00051137 File Offset: 0x0004F337
		protected override MilestoneItemID[] PurchasableItems
		{
			get
			{
				return this.milestoneItems;
			}
		}

		// Token: 0x04000E6F RID: 3695
		[EnumPopup]
		public MilestoneItemID[] milestoneItems;
	}
}
