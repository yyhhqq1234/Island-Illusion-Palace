using System;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B7 RID: 183
	[Name("Modify Cast Resistance Params", 0)]
	[Category("Unliving/Mobs")]
	public sealed class ModifyAbilityCastInterruptionNode : ModifyAbilityExtensionNodeBase<AbilityPreparationResistanceController>
	{
		// Token: 0x060004B0 RID: 1200 RVA: 0x00010D58 File Offset: 0x0000EF58
		protected override bool ModifyAbilityExtension(BaseAbility ability, AbilityPreparationResistanceController extension)
		{
			float[] resistanceDamageGains = extension.ResistanceDamageGains;
			extension.isActive = this.isCastModificationActive.value;
			extension.Resistance *= this.interruptionResistanceCoeff.value;
			extension.ResistanceDamage *= this.castProgressDamageCoeff.value;
			if (resistanceDamageGains != null)
			{
				for (int i = 0; i < resistanceDamageGains.Length; i++)
				{
					resistanceDamageGains[i] *= this.castProgressDamageCoeff.value;
				}
			}
			return true;
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00010DD8 File Offset: 0x0000EFD8
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.isCastModificationActive = base.AddValueInput<bool>("isCastModificationActive", "");
			this.isCastModificationActive.SetDefaultAndSerializedValue(true);
			this.interruptionResistanceCoeff = base.AddValueInput<float>("interruptionResistanceCoeff", "");
			this.interruptionResistanceCoeff.SetDefaultAndSerializedValue(1f);
			this.castProgressDamageCoeff = base.AddValueInput<float>("castProgressDamageCoeff", "");
			this.castProgressDamageCoeff.SetDefaultAndSerializedValue(1f);
		}

		// Token: 0x0400031D RID: 797
		private ValueInput<bool> isCastModificationActive;

		// Token: 0x0400031E RID: 798
		private ValueInput<float> interruptionResistanceCoeff;

		// Token: 0x0400031F RID: 799
		private ValueInput<float> castProgressDamageCoeff;
	}
}
