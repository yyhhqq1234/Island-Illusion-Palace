using System;
using Game.Abilities;
using Game.Factories;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.Purchasing
{
	// Token: 0x02000114 RID: 276
	public class SeveralItemsUnlockTrigger : PurchasableUnlockTriggerBase
	{
		// Token: 0x060006A8 RID: 1704 RVA: 0x00015CF4 File Offset: 0x00013EF4
		public override bool IsFired(Context context)
		{
			if (context == null)
			{
				return true;
			}
			int num = 0;
			foreach (AbilityID abilityID in this.abilities)
			{
				IPurchasable purchasable;
				if (context.purchaseManager.TryGetPurchasable(abilityID, out purchasable) && purchasable.Purchased)
				{
					num++;
					if (num >= this.unlockItemCount)
					{
						return true;
					}
				}
			}
			foreach (MobActivationModifierID mobActivationModifierID in this.modifiers)
			{
				IPurchasable purchasable;
				if (context.purchaseManager.TryGetPurchasable(mobActivationModifierID, out purchasable) && purchasable.Purchased)
				{
					num++;
					if (num >= this.unlockItemCount)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x04000412 RID: 1042
		[Tooltip("Количество предметов для срабатывания триггера")]
		public int unlockItemCount;

		// Token: 0x04000413 RID: 1043
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID[] abilities;

		// Token: 0x04000414 RID: 1044
		[ObjectFactoryIDPopup(typeof(MobActivationAbilityModifier))]
		public MobActivationModifierID[] modifiers;
	}
}
