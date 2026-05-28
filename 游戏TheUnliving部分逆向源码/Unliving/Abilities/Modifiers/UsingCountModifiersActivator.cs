using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003CE RID: 974
	[CreateAssetMenu(fileName = "UsingCountModifiersActivator", menuName = "Abilities/Modifiers Activators/Using Count Activator")]
	public sealed class UsingCountModifiersActivator : AbilityModifiersActivatorBase<UsingCountModifiersActivator.UsingCountTrigger>
	{
		// Token: 0x06002110 RID: 8464 RVA: 0x00067D03 File Offset: 0x00065F03
		protected override UsingCountModifiersActivator.UsingCountTrigger CreateTrigger(BaseAbility ability)
		{
			return new UsingCountModifiersActivator.UsingCountTrigger();
		}

		// Token: 0x06002111 RID: 8465 RVA: 0x00067D0A File Offset: 0x00065F0A
		protected override bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			UsingCountModifiersActivator.UsingCountTrigger trigger = base.GetTrigger(args.ability);
			trigger.targetUsingCount = this.targetAbilityUsingCount;
			return trigger.UpdateState(args);
		}

		// Token: 0x06002112 RID: 8466 RVA: 0x00067D2D File Offset: 0x00065F2D
		public override bool CanBeActivated(AbilityModifiersActivatorArgs args)
		{
			return this.targetAbilityUsingCount > 0 && base.CanBeActivated(args);
		}

		// Token: 0x06002113 RID: 8467 RVA: 0x00067D41 File Offset: 0x00065F41
		public override bool TryUse(AbilityModifiersActivatorArgs args, out AbilityModifierUsingArgs modifiersUsingArgs)
		{
			modifiersUsingArgs = base.InitializeModifiersUsingArgs();
			if (base.GetTrigger(args.ability).IsFired)
			{
				base.PrepareModifiersUsingArgs(args, modifiersUsingArgs, this.modifiersUsingCount, false);
				return true;
			}
			return false;
		}

		// Token: 0x06002114 RID: 8468 RVA: 0x00067D71 File Offset: 0x00065F71
		public override void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false)
		{
			if (force || ability.WasUsed)
			{
				base.GetTrigger(ability).Reset();
			}
		}

		// Token: 0x040014AB RID: 5291
		public int targetAbilityUsingCount = 3;

		// Token: 0x040014AC RID: 5292
		public int modifiersUsingCount = 1;

		// Token: 0x0200058E RID: 1422
		public sealed class UsingCountTrigger : AbilityModifiersActivatorTriggerBase
		{
			// Token: 0x0600278C RID: 10124 RVA: 0x0007BCE4 File Offset: 0x00079EE4
			protected override bool GetNewState(AbilityModifiersActivatorArgs args)
			{
				int num = this.currentUsingCount + 1;
				this.currentUsingCount = num;
				return num % this.targetUsingCount == 0;
			}

			// Token: 0x04001CD2 RID: 7378
			public int targetUsingCount;

			// Token: 0x04001CD3 RID: 7379
			private int currentUsingCount;
		}
	}
}
