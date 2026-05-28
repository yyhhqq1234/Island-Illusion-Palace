using System;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C2 RID: 962
	[Serializable]
	public sealed class AbilityStatModifier : AbilityModifierBase
	{
		// Token: 0x170006A8 RID: 1704
		// (get) Token: 0x060020B6 RID: 8374 RVA: 0x000670BE File Offset: 0x000652BE
		public override bool IsActive
		{
			get
			{
				return this.value.targetStat != MobStatID.Undefined;
			}
		}

		// Token: 0x060020B7 RID: 8375 RVA: 0x000670D4 File Offset: 0x000652D4
		protected override void OnUse(AbilityModifierUsingArgs usingArgs)
		{
			ITempMobStatsModifiersReceiver tempMobStatsModifiersReceiver = usingArgs.targetAbility as ITempMobStatsModifiersReceiver;
			if (tempMobStatsModifiersReceiver == null)
			{
				return;
			}
			TargetedMobStatModifier statModifier = this.value;
			statModifier.value *= (float)usingArgs.modifiersUsingCount;
			tempMobStatsModifiersReceiver.AddTempStatModifier(statModifier);
		}

		// Token: 0x060020B8 RID: 8376 RVA: 0x00067111 File Offset: 0x00065311
		protected override void OnReset(AbilityModifierUsingArgs usingArgs)
		{
		}

		// Token: 0x060020B9 RID: 8377 RVA: 0x00067113 File Offset: 0x00065313
		public AbilityStatModifier(AbilityStatModifier modifierPrototype) : base(modifierPrototype)
		{
			this.value = modifierPrototype.value;
		}

		// Token: 0x060020BA RID: 8378 RVA: 0x00067128 File Offset: 0x00065328
		public override AbilityModifierBase Clone()
		{
			return new AbilityStatModifier(this);
		}

		// Token: 0x04001485 RID: 5253
		public TargetedMobStatModifier value;
	}
}
