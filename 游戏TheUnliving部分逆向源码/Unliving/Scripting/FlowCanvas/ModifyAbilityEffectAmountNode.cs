using System;
using Common;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B8 RID: 184
	[Name("Modify Ability Effect Amount", 0)]
	[Category("Unliving/Abilities")]
	public sealed class ModifyAbilityEffectAmountNode : ModifyAbilityEffectNodeBase<IAmountBased>
	{
		// Token: 0x060004B3 RID: 1203 RVA: 0x00010E64 File Offset: 0x0000F064
		protected override bool TryModifyAbilityEffect(IEffectsBasedAbility ability, IAmountBased abilityEffect)
		{
			if (abilityEffect.GetType() == this.effectType.value)
			{
				if (this.newAmount.value >= 0f)
				{
					abilityEffect.Amount = this.newAmount.value;
				}
				else
				{
					if (this.amountMultiplier.value < 0f)
					{
						return false;
					}
					abilityEffect.Amount *= this.amountMultiplier.value;
				}
				return true;
			}
			return false;
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00010EE0 File Offset: 0x0000F0E0
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.effectType = base.AddValueInput<Type>("effectType", "");
			this.newAmount = base.AddValueInput<float>("newAmount", "");
			this.newAmount.SetDefaultAndSerializedValue(-1f);
			this.amountMultiplier = base.AddValueInput<float>("amountMultiplier", "");
			this.amountMultiplier.SetDefaultAndSerializedValue(-1f);
		}

		// Token: 0x04000320 RID: 800
		private ValueInput<Type> effectType;

		// Token: 0x04000321 RID: 801
		private ValueInput<float> newAmount;

		// Token: 0x04000322 RID: 802
		private ValueInput<float> amountMultiplier;
	}
}
