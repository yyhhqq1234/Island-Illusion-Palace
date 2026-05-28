using System;
using Common.UnityExtensions;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003BA RID: 954
	[Serializable]
	public sealed class AbilityBuffsBasedModifier : AbilityBuffsBasedModifierBase
	{
		// Token: 0x06002044 RID: 8260 RVA: 0x00065A90 File Offset: 0x00063C90
		private void InitializeBuffGenerators()
		{
			if (this.instantiatedBuffsGenerators != null)
			{
				return;
			}
			BuffsGeneratorBuilderAsset.ReferenceBase[] buffsGenerators = this.buffsGenerators;
			buffsGenerators.Instantiate(out this.instantiatedBuffsGenerators);
		}

		// Token: 0x06002045 RID: 8261 RVA: 0x00065ABC File Offset: 0x00063CBC
		private void ApplyBuffs(AbilityModifierUsingArgs modifierUsingArgs, Component target)
		{
			IBuffableObject buffableObject = target.CastOrGetComponent<IBuffableObject>();
			IBuffsController buffsController = (buffableObject != null) ? buffableObject.BuffsController : null;
			if (buffsController == null)
			{
				return;
			}
			object owner = modifierUsingArgs.targetAbility.Owner;
			for (int i = 0; i < this.instantiatedBuffsGenerators.Length; i++)
			{
				IBuffsGenerator buffsGenerator = this.instantiatedBuffsGenerators[i];
				base.SetBuffsDuration(buffsGenerator, modifierUsingArgs);
				base.SetBuffsPowerGainsActive(buffsGenerator, modifierUsingArgs, true);
				base.SetBuffsStatsActive(buffsGenerator, modifierUsingArgs, true);
				IBuff buff = buffsGenerator.GenerateBuff(owner, false);
				buffsController.AddBuff(buff);
				base.SetBuffsPowerGainsActive(buffsGenerator, modifierUsingArgs, false);
				base.SetBuffsStatsActive(buffsGenerator, modifierUsingArgs, false);
			}
		}

		// Token: 0x06002046 RID: 8262 RVA: 0x00065B47 File Offset: 0x00063D47
		protected override void OnUse(AbilityModifierUsingArgs usingArgs)
		{
			this.InitializeBuffGenerators();
			base.UseOnTargets(usingArgs, new Action<AbilityModifierUsingArgs, Component>(this.ApplyBuffs));
		}

		// Token: 0x06002047 RID: 8263 RVA: 0x00065B62 File Offset: 0x00063D62
		protected override void OnReset(AbilityModifierUsingArgs usingArgs)
		{
		}

		// Token: 0x06002048 RID: 8264 RVA: 0x00065B64 File Offset: 0x00063D64
		public AbilityBuffsBasedModifier(AbilityBuffsBasedModifier modifierPrototype) : base(modifierPrototype)
		{
		}

		// Token: 0x06002049 RID: 8265 RVA: 0x00065B6D File Offset: 0x00063D6D
		public override AbilityModifierBase Clone()
		{
			return new AbilityBuffsBasedModifier(this);
		}

		// Token: 0x0400144C RID: 5196
		private IBuffsGenerator[] instantiatedBuffsGenerators;
	}
}
