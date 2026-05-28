using System;
using Common.Math.Random;
using Game.Abilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003CC RID: 972
	[CreateAssetMenu(fileName = "RandomModifiersActivator", menuName = "Abilities/Modifiers Activators/Random Activator")]
	public sealed class RandomModifiersActivator : AbilityModifiersActivatorBase<RandomModifiersActivator.Trigger>
	{
		// Token: 0x170006AD RID: 1709
		// (get) Token: 0x060020FB RID: 8443 RVA: 0x000679EA File Offset: 0x00065BEA
		// (set) Token: 0x060020FC RID: 8444 RVA: 0x000679F2 File Offset: 0x00065BF2
		public float ActivationProbability
		{
			get
			{
				return this.activationProbability;
			}
			set
			{
				this.activationProbability = Mathf.Clamp01(value);
			}
		}

		// Token: 0x060020FD RID: 8445 RVA: 0x00067A00 File Offset: 0x00065C00
		protected override RandomModifiersActivator.Trigger CreateTrigger(BaseAbility ability)
		{
			return new RandomModifiersActivator.Trigger
			{
				usePRD = this.usePseudoRandomDistribution,
				Probability = this.activationProbability
			};
		}

		// Token: 0x060020FE RID: 8446 RVA: 0x00067A20 File Offset: 0x00065C20
		protected override bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			RandomModifiersActivator.Trigger trigger = base.GetTrigger(args.ability);
			if (this.usingCount <= 0)
			{
				trigger.Reset();
				return false;
			}
			trigger.usePRD = this.usePseudoRandomDistribution;
			trigger.Probability = this.activationProbability;
			return trigger.UpdateState(args);
		}

		// Token: 0x060020FF RID: 8447 RVA: 0x00067A6D File Offset: 0x00065C6D
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

		// Token: 0x06002100 RID: 8448 RVA: 0x00067A9D File Offset: 0x00065C9D
		public override void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false)
		{
			if (force || ability.WasUsed)
			{
				base.GetTrigger(ability).Reset();
			}
		}

		// Token: 0x040014A4 RID: 5284
		[SerializeField]
		[FormerlySerializedAs("_activationProbability")]
		[Range(0f, 1f)]
		private float activationProbability = 0.5f;

		// Token: 0x040014A5 RID: 5285
		[Tooltip("При использовании этой опции вероятность срабатывания будет увеличиваться с каждой неудачной попыткой использования.")]
		public bool usePseudoRandomDistribution = true;

		// Token: 0x040014A6 RID: 5286
		public int usingCount = 1;

		// Token: 0x0200058C RID: 1420
		public sealed class Trigger : AbilityModifiersActivatorTriggerBase
		{
			// Token: 0x17000809 RID: 2057
			// (get) Token: 0x06002788 RID: 10120 RVA: 0x0007BC8F File Offset: 0x00079E8F
			// (set) Token: 0x06002789 RID: 10121 RVA: 0x0007BC97 File Offset: 0x00079E97
			public float Probability
			{
				get
				{
					return this.probability;
				}
				set
				{
					if (this.usePRD)
					{
						this.prdTrigger.Probability = value;
					}
					this.probability = value;
				}
			}

			// Token: 0x0600278A RID: 10122 RVA: 0x0007BCB4 File Offset: 0x00079EB4
			protected override bool GetNewState(AbilityModifiersActivatorArgs args)
			{
				if (!this.usePRD)
				{
					return UnityEngine.Random.value <= this.probability;
				}
				return this.prdTrigger.IsFired();
			}

			// Token: 0x04001CCC RID: 7372
			public bool usePRD;

			// Token: 0x04001CCD RID: 7373
			private float probability;

			// Token: 0x04001CCE RID: 7374
			private PRDTrigger prdTrigger;
		}
	}
}
