using System;
using Unliving.Abilities.AbilitiesGeneration;

namespace Unliving.Purchasing
{
	// Token: 0x020000E6 RID: 230
	[Serializable]
	public class PurchasableAbilitySpecialBehaviour : PurchasableItem<AbilitySpecialBehaviourID>
	{
		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x00013D30 File Offset: 0x00011F30
		public override bool Purchased
		{
			get
			{
				return !base.Locked;
			}
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x00013D3B File Offset: 0x00011F3B
		public PurchasableAbilitySpecialBehaviour(AbilitySpecialBehaviourID specialBehaviourID)
		{
			this.PurchaseItem = specialBehaviourID;
		}
	}
}
