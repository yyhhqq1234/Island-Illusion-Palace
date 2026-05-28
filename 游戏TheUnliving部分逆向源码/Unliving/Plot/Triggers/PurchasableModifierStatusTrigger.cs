using System;
using Common.Editor;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F9 RID: 761
	[Serializable]
	public class PurchasableModifierStatusTrigger : PurchasableItemStatusTriggerBase<MobActivationModifierID>
	{
		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x060019EC RID: 6636 RVA: 0x00051157 File Offset: 0x0004F357
		protected override MobActivationModifierID[] PurchasableItems
		{
			get
			{
				return this.modifiers;
			}
		}

		// Token: 0x04000E71 RID: 3697
		[EnumPopup]
		public MobActivationModifierID[] modifiers;
	}
}
