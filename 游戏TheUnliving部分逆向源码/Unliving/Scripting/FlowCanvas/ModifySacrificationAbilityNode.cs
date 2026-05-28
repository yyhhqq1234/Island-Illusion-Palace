using System;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000082 RID: 130
	[Name("Modify Sacrification Ability", 0)]
	[Category("Unliving/Abilities")]
	public sealed class ModifySacrificationAbilityNode : ModifyAbilityNodeBase
	{
		// Token: 0x06000397 RID: 919 RVA: 0x0000C35C File Offset: 0x0000A55C
		protected override bool TryModifyAbility(GameAbilitiesController abilitiesController, BaseAbility ability, Flow flow)
		{
			MobSacrificeAbility mobSacrificeAbility = ability as MobSacrificeAbility;
			if (mobSacrificeAbility != null)
			{
				mobSacrificeAbility.restoreEnergyOnSacrifice = this.restoreVitalEnergyOnSacrifice.value;
				return true;
			}
			return false;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x0000C387 File Offset: 0x0000A587
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.restoreVitalEnergyOnSacrifice = base.AddValueInput<bool>("restoreVitalEnergyOnSacrifice", "");
		}

		// Token: 0x04000223 RID: 547
		private ValueInput<bool> restoreVitalEnergyOnSacrifice;
	}
}
