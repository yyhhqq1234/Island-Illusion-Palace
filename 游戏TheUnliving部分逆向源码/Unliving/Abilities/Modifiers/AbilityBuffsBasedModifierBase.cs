using System;
using Common;
using Game.Abilities;
using Game.Buffs;
using UnityEngine.Serialization;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003BB RID: 955
	public abstract class AbilityBuffsBasedModifierBase : AbilityModifierBase
	{
		// Token: 0x17000695 RID: 1685
		// (get) Token: 0x0600204A RID: 8266 RVA: 0x00065B75 File Offset: 0x00063D75
		public override bool IsActive
		{
			get
			{
				return this.buffsGenerators != null && this.buffsGenerators.Length != 0;
			}
		}

		// Token: 0x0600204B RID: 8267 RVA: 0x00065B8B File Offset: 0x00063D8B
		protected AbilityBuffsBasedModifierBase(AbilityBuffsBasedModifierBase modifierPrototype) : base(modifierPrototype)
		{
			this.buffsGenerators = modifierPrototype.buffsGenerators;
			this.baseBuffsDuration = modifierPrototype.baseBuffsDuration;
			this.baseBuffsPowerAddition = modifierPrototype.baseBuffsPowerAddition;
		}

		// Token: 0x0600204C RID: 8268 RVA: 0x00065BB8 File Offset: 0x00063DB8
		protected float GetBuffsDuration(AbilityModifierUsingArgs modifierUsingArgs)
		{
			return this.baseBuffsDuration * (float)modifierUsingArgs.modifiersUsingCount;
		}

		// Token: 0x0600204D RID: 8269 RVA: 0x00065BC8 File Offset: 0x00063DC8
		protected void SetBuffsDuration(IBuffsGenerator buffsGenerator, AbilityModifierUsingArgs modifierUsingArgs)
		{
			float buffsDuration = this.GetBuffsDuration(modifierUsingArgs);
			if (buffsDuration > 0f)
			{
				buffsGenerator.BuffDuration = buffsDuration;
			}
		}

		// Token: 0x0600204E RID: 8270 RVA: 0x00065BEC File Offset: 0x00063DEC
		protected void SetBuffsPowerGainsActive(IBuffsGenerator buffsGenerator, AbilityModifierUsingArgs usingArgs, bool isActive)
		{
			if (this.baseBuffsPowerAddition == 0f)
			{
				return;
			}
			float num = (isActive ? this.baseBuffsPowerAddition : (-this.baseBuffsPowerAddition)) * (float)usingArgs.modifiersUsingCount;
			AbilityEffectBasedBuffsGenerator abilityEffectBasedBuffsGenerator = buffsGenerator as AbilityEffectBasedBuffsGenerator;
			if (abilityEffectBasedBuffsGenerator != null)
			{
				AbilityEffectBase[] buffEffects = abilityEffectBasedBuffsGenerator.BuffEffects;
				for (int i = 0; i < buffEffects.Length; i++)
				{
					buffEffects[i].Amount += num;
				}
				return;
			}
			IAmountBased amountBased = buffsGenerator as IAmountBased;
			if (amountBased != null)
			{
				amountBased.Amount += num;
			}
		}

		// Token: 0x0600204F RID: 8271 RVA: 0x00065C70 File Offset: 0x00063E70
		protected void SetBuffsStatsActive(IBuffsGenerator buffsGenerator, AbilityModifierUsingArgs usingArgs, bool isActive)
		{
			MobStatModifier mobStatModifier;
			bool flag = usingArgs.TryGetStatModifier(MobStatID.GroupMobsActivationModifiersDamage, out mobStatModifier);
			MobStatModifier mobStatModifier2;
			bool flag2 = usingArgs.TryGetStatModifier(MobStatID.GroupMobsActivationModifiersBuffDuration, out mobStatModifier2);
			if (!isActive)
			{
				mobStatModifier.Inverse();
				mobStatModifier2.Inverse();
			}
			if (flag)
			{
				AbilityEffectBasedBuffsGenerator abilityEffectBasedBuffsGenerator = buffsGenerator as AbilityEffectBasedBuffsGenerator;
				AbilityEffectBase[] array = (abilityEffectBasedBuffsGenerator != null) ? abilityEffectBasedBuffsGenerator.BuffEffects : null;
				if (array != null)
				{
					foreach (AbilityEffectBase abilityEffectBase in array)
					{
						if (abilityEffectBase.IsDamagingAbilityEffect())
						{
							abilityEffectBase.Amount = mobStatModifier.GetModifiedStatValue(abilityEffectBase.Amount);
						}
					}
				}
			}
			if (flag2)
			{
				buffsGenerator.BuffDuration = mobStatModifier2.GetModifiedStatValue(buffsGenerator.BuffDuration);
			}
		}

		// Token: 0x0400144D RID: 5197
		public BuffsGeneratorBuilderAsset.Reference[] buffsGenerators;

		// Token: 0x0400144E RID: 5198
		[FormerlySerializedAs("durationAddition")]
		public float baseBuffsDuration;

		// Token: 0x0400144F RID: 5199
		[FormerlySerializedAs("powerAddition")]
		public float baseBuffsPowerAddition;
	}
}
