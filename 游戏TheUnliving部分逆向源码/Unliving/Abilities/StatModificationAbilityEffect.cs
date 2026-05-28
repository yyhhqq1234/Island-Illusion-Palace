using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Stats;
using UnityEngine;
using Unliving.MobsStats;
using Unliving.PassiveAbilities;

namespace Unliving.Abilities
{
	// Token: 0x02000395 RID: 917
	[Serializable]
	public sealed class StatModificationAbilityEffect : StateBasedAbilityEffect, IFormattable
	{
		// Token: 0x06001E41 RID: 7745 RVA: 0x0005FEC5 File Offset: 0x0005E0C5
		public StatModificationAbilityEffect()
		{
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x0005FECD File Offset: 0x0005E0CD
		public StatModificationAbilityEffect(StatModificationAbilityEffect effectPrototype)
		{
			this.statModifier = effectPrototype.statModifier;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E43 RID: 7747 RVA: 0x0005FEE8 File Offset: 0x0005E0E8
		protected override void SetEffectActive(Component effectTarget, bool isActive)
		{
			if (effectTarget.IsNull())
			{
				return;
			}
			IStatsOwner<MobStatModifier> statsOwner = effectTarget.CastOrGetComponent<IStatsOwner<MobStatModifier>>();
			IStatsController<MobStatModifier> statsController = (statsOwner != null) ? statsOwner.StatsController : null;
			if (statsController != null)
			{
				if (this.statModifier.modifierType == MobStatModifierType.None)
				{
					return;
				}
				MobStatID targetStat = this.statModifier.targetStat;
				if (targetStat == MobStatID.Undefined)
				{
					return;
				}
				if (isActive)
				{
					statsController.AddModifier((int)targetStat, this.statModifier);
					return;
				}
				statsController.RemoveModifier((int)targetStat, this.statModifier);
			}
		}

		// Token: 0x06001E44 RID: 7748 RVA: 0x0005FF5B File Offset: 0x0005E15B
		protected override float GetEffectAmount()
		{
			return this.statModifier.value;
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x0005FF68 File Offset: 0x0005E168
		protected override void SetEffectAmount(float newAmount)
		{
			this.statModifier.value = newAmount;
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x0005FF76 File Offset: 0x0005E176
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new StatModificationAbilityEffect((StatModificationAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E47 RID: 7751 RVA: 0x0005FF83 File Offset: 0x0005E183
		string IFormattable.ToString(string format, IFormatProvider formatProvider)
		{
			return this.statModifier.ToString();
		}

		// Token: 0x04001111 RID: 4369
		public PassiveAbility.StatModifier statModifier;
	}
}
