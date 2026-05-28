using System;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.Purchasing
{
	// Token: 0x020000E7 RID: 231
	[Serializable]
	public class PurchasableActivationModifier : PurchasableItem<MobActivationModifierID>
	{
		// Token: 0x060005A0 RID: 1440 RVA: 0x00013D4A File Offset: 0x00011F4A
		public PurchasableActivationModifier(MobActivationModifierID modifierID)
		{
			this.PurchaseItem = modifierID;
		}
	}
}
