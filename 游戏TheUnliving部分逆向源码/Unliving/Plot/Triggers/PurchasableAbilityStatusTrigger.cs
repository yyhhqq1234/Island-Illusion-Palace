using System;
using Common.Editor;
using Unliving.Abilities;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F8 RID: 760
	[Serializable]
	public class PurchasableAbilityStatusTrigger : PurchasableItemStatusTriggerBase<AbilityID>
	{
		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x060019EA RID: 6634 RVA: 0x00051147 File Offset: 0x0004F347
		protected override AbilityID[] PurchasableItems
		{
			get
			{
				return this.abilities;
			}
		}

		// Token: 0x04000E70 RID: 3696
		[EnumPopup]
		public AbilityID[] abilities;
	}
}
