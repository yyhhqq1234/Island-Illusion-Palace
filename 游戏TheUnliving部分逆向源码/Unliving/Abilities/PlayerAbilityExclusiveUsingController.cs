using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Abilities
{
	// Token: 0x0200037B RID: 891
	[CreateAssetMenu(fileName = "PlayerAbilityExclusiveUsingController", menuName = "Abilities/Controllers/Player Ability Exclusive Using Controller")]
	public sealed class PlayerAbilityExclusiveUsingController : AbilityExtensionAssetBase
	{
		// Token: 0x06001D51 RID: 7505 RVA: 0x0005CE31 File Offset: 0x0005B031
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.BeforeUsed += this.OnBeforeAbilityUsed;
			ability.Completed += this.OnAbilityCompleted;
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x0005CE60 File Offset: 0x0005B060
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			if (this.playerAbilitiesController != null)
			{
				this.playerAbilitiesController.SetExclusiveMode(ability, false);
			}
			ability.BeforeUsed -= this.OnBeforeAbilityUsed;
			ability.Completed -= this.OnAbilityCompleted;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D53 RID: 7507 RVA: 0x0005CEB0 File Offset: 0x0005B0B0
		protected override void OnAbilityOwnerChanged(BaseAbility currentAbility, object lastOwner, object newOwner)
		{
			PlayerBehaviour playerBehaviour = newOwner as PlayerBehaviour;
			if (playerBehaviour != null)
			{
				this.playerAbilitiesController = (playerBehaviour.AbilitiesController as PlayerAbilitiesController);
			}
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x0005CED8 File Offset: 0x0005B0D8
		private void OnBeforeAbilityUsed(IAbility ability, object usingArgs)
		{
			if (this.playerAbilitiesController == null)
			{
				return;
			}
			if (!this.currentAbility.WasUsed)
			{
				this.playerAbilitiesController.SetExclusiveMode(this.currentAbility, true);
			}
		}

		// Token: 0x06001D55 RID: 7509 RVA: 0x0005CF02 File Offset: 0x0005B102
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			if (this.playerAbilitiesController == null)
			{
				return;
			}
			this.playerAbilitiesController.SetExclusiveMode(this.currentAbility, false);
		}

		// Token: 0x04001096 RID: 4246
		private PlayerAbilitiesController playerAbilitiesController;
	}
}
