using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200038A RID: 906
	[Serializable]
	public sealed class MobExchangeAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001DE7 RID: 7655 RVA: 0x0005EC82 File Offset: 0x0005CE82
		public MobExchangeAbilityEffect()
		{
		}

		// Token: 0x06001DE8 RID: 7656 RVA: 0x0005EC95 File Offset: 0x0005CE95
		public MobExchangeAbilityEffect(MobExchangeAbilityEffect effectPrototype)
		{
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DE9 RID: 7657 RVA: 0x0005ECAF File Offset: 0x0005CEAF
		protected override float GetEffectAmount()
		{
			return this.vitalEnergyAmount;
		}

		// Token: 0x06001DEA RID: 7658 RVA: 0x0005ECB7 File Offset: 0x0005CEB7
		protected override void SetEffectAmount(float newAmount)
		{
			this.vitalEnergyAmount = newAmount;
		}

		// Token: 0x06001DEB RID: 7659 RVA: 0x0005ECC0 File Offset: 0x0005CEC0
		protected override bool Use(Component effectTarget, float dt)
		{
			MobBehaviour mobBehaviour = effectTarget.CastOrGetComponent<MobBehaviour>();
			if (mobBehaviour != null)
			{
				Component component = base.GetEffectOwner() as Component;
				if (this.sacrificeAbility.rewardAmountMultiplier > 0f)
				{
					ICurrencyOperationPerformer currencyOperationPerformer = component as ICurrencyOperationPerformer;
					if (currencyOperationPerformer != null)
					{
						currencyOperationPerformer.PerformCurrencyOperation(this.sacrificeAbility.CreateActivationRewardArgs(mobBehaviour));
					}
				}
				this.vitalEnergyAmount += this.vitalEnergyPerMobReward;
				mobBehaviour.KillMob(component);
			}
			return true;
		}

		// Token: 0x06001DEC RID: 7660 RVA: 0x0005ED37 File Offset: 0x0005CF37
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new MobExchangeAbilityEffect((MobExchangeAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001DED RID: 7661 RVA: 0x0005ED44 File Offset: 0x0005CF44
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			if (this.forceUseOnOwner)
			{
				Component component = base.GetEffectOwner() as Component;
				if (component != null)
				{
					base.UseOnTarget(component, abilityUsingArgs, dt);
				}
			}
			else
			{
				int targetsCount = abilityUsingArgs.TargetsCount;
				IList<Component> targetsList = abilityUsingArgs.targetsList;
				if (targetsCount != 0)
				{
					for (int i = 0; i < targetsCount; i++)
					{
						Component component2 = targetsList[i];
						if (component2 != null)
						{
							base.UseOnTarget(component2, abilityUsingArgs, dt);
						}
					}
				}
				else if (abilityUsingArgs.HasTargetObject)
				{
					base.UseOnTarget(abilityUsingArgs.targetObject, abilityUsingArgs, dt);
				}
			}
			BaseAbility currentAbility = this.currentAbility;
			IAbilitiesEnergyProvider abilitiesEnergyProvider;
			if (((currentAbility != null) ? currentAbility.OwnerBehaviour : null) != null && this.currentAbility.OwnerBehaviour.TryGetComponent<IAbilitiesEnergyProvider>(out abilitiesEnergyProvider))
			{
				abilitiesEnergyProvider.RestoreEnergy(this.vitalEnergyAmount);
			}
		}

		// Token: 0x040010DB RID: 4315
		public float vitalEnergyPerMobReward = 10f;

		// Token: 0x040010DC RID: 4316
		public MobSacrificeAbility sacrificeAbility;

		// Token: 0x040010DD RID: 4317
		private float vitalEnergyAmount;
	}
}
