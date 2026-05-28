using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003CA RID: 970
	[CreateAssetMenu(fileName = "CooldownModifiersActivator", menuName = "Abilities/Modifiers Activators/Cooldown Activator")]
	public sealed class CooldownModifiersActivator : AbilityModifiersActivatorBase<CooldownModifiersActivator.Trigger>
	{
		// Token: 0x060020EE RID: 8430 RVA: 0x00067830 File Offset: 0x00065A30
		protected override CooldownModifiersActivator.Trigger CreateTrigger(BaseAbility ability)
		{
			return new CooldownModifiersActivator.Trigger();
		}

		// Token: 0x060020EF RID: 8431 RVA: 0x00067837 File Offset: 0x00065A37
		protected override bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			CooldownModifiersActivator.Trigger trigger = base.GetTrigger(args.ability);
			trigger.cooldown = this.cooldown;
			return trigger.UpdateState(args);
		}

		// Token: 0x060020F0 RID: 8432 RVA: 0x0006785A File Offset: 0x00065A5A
		public override bool TryUse(AbilityModifiersActivatorArgs args, out AbilityModifierUsingArgs modifiersUsingArgs)
		{
			modifiersUsingArgs = base.InitializeModifiersUsingArgs();
			if (base.GetTrigger(args.ability).IsFired)
			{
				base.PrepareModifiersUsingArgs(args, modifiersUsingArgs, this.usingCount, false);
				return true;
			}
			return false;
		}

		// Token: 0x060020F1 RID: 8433 RVA: 0x0006788A File Offset: 0x00065A8A
		public override void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false)
		{
			if (force || ability.WasUsed)
			{
				base.GetTrigger(ability).Reset();
			}
		}

		// Token: 0x040014A0 RID: 5280
		public float cooldown = 5f;

		// Token: 0x040014A1 RID: 5281
		public int usingCount = 1;

		// Token: 0x0200058B RID: 1419
		public sealed class Trigger : AbilityModifiersActivatorTriggerBase
		{
			// Token: 0x06002785 RID: 10117 RVA: 0x0007BC56 File Offset: 0x00079E56
			protected override bool GetNewState(AbilityModifiersActivatorArgs args)
			{
				return Time.time > this.targetTime;
			}

			// Token: 0x06002786 RID: 10118 RVA: 0x0007BC65 File Offset: 0x00079E65
			public override void Reset()
			{
				if (base.IsFired)
				{
					this.targetTime = Time.time + this.cooldown;
				}
				base.Reset();
			}

			// Token: 0x04001CCA RID: 7370
			public float cooldown;

			// Token: 0x04001CCB RID: 7371
			private float targetTime;
		}
	}
}
