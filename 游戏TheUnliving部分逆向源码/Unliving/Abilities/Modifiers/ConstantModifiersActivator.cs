using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C9 RID: 969
	[CreateAssetMenu(fileName = "ConstantModifiersActivator", menuName = "Abilities/Modifiers Activators/Constant Activator")]
	public sealed class ConstantModifiersActivator : AbilityModifiersActivatorBase
	{
		// Token: 0x060020EA RID: 8426 RVA: 0x000677FF File Offset: 0x000659FF
		protected override bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			return true;
		}

		// Token: 0x060020EB RID: 8427 RVA: 0x00067805 File Offset: 0x00065A05
		public override bool TryUse(AbilityModifiersActivatorArgs args, out AbilityModifierUsingArgs modifiersUsingArgs)
		{
			modifiersUsingArgs = base.InitializeModifiersUsingArgs();
			if (!this.CanBeActivated(args))
			{
				return false;
			}
			base.PrepareModifiersUsingArgs(args, modifiersUsingArgs, 1, false);
			return true;
		}

		// Token: 0x060020EC RID: 8428 RVA: 0x00067826 File Offset: 0x00065A26
		public override void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false)
		{
		}
	}
}
