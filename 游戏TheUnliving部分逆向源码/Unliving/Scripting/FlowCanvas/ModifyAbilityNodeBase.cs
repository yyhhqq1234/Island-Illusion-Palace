using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000BB RID: 187
	public abstract class ModifyAbilityNodeBase : FlowControlNode
	{
		// Token: 0x060004BC RID: 1212 RVA: 0x00010FE4 File Offset: 0x0000F1E4
		private void ModifyAbilities(Flow flow)
		{
			AbilityDescription value = this.abilityFilter.value;
			bool skipFilter = value.IsBlank();
			if (this.targetAbility.value != null)
			{
				BaseGameMob baseGameMob = this.targetAbility.value.Owner as BaseGameMob;
				GameAbilitiesController controller = (baseGameMob != null) ? baseGameMob.AbilitiesController : null;
				this.<ModifyAbilities>g__TryModifyAllAbilities|5_0(controller, this.targetAbility.value, ref value, skipFilter, flow);
			}
			else if (this.targetMob.value != null)
			{
				GameAbilitiesController abilitiesController = this.targetMob.value.AbilitiesController;
				if (abilitiesController != null)
				{
					IReadOnlyList<BaseAbility> abilities = abilitiesController.Abilities;
					for (int i = 0; i < abilities.Count; i++)
					{
						this.<ModifyAbilities>g__TryModifyAllAbilities|5_0(abilitiesController, abilities[i], ref value, skipFilter, flow);
					}
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004BD RID: 1213
		protected abstract bool TryModifyAbility(GameAbilitiesController abilitiesController, BaseAbility ability, Flow flow);

		// Token: 0x060004BE RID: 1214 RVA: 0x000110BC File Offset: 0x0000F2BC
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ModifyAbilities), "");
			this.targetAbility = base.AddValueInput<BaseAbility>("targetAbility", "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityFilter = base.AddValueInput<AbilityDescription>("abilityFilter", "");
			this.abilityFilter.SetDefaultAndSerializedValue(AbilityDescription.BlankDescription);
			this.modifySingleAbilityOnly = base.AddValueInput<bool>("modifySingleAbilityOnly", "");
			this.modifySingleAbilityOnly.SetDefaultAndSerializedValue(true);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x0001117C File Offset: 0x0000F37C
		[CompilerGenerated]
		private bool <ModifyAbilities>g__TryModifyAllAbilities|5_0(GameAbilitiesController controller, BaseAbility ability, ref AbilityDescription filter, bool skipFilter, Flow flow)
		{
			bool result = (skipFilter || filter.IsMatch(ability)) && this.TryModifyAbility(controller, ability, flow);
			ICompositeAbility compositeAbility = ability as ICompositeAbility;
			IList<IAbility> list = (compositeAbility != null) ? compositeAbility.ChildAbilities : null;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					BaseAbility baseAbility = list[i] as BaseAbility;
					if (baseAbility != null && (skipFilter || filter.IsMatch(baseAbility)))
					{
						this.TryModifyAbility(controller, baseAbility, flow);
					}
				}
			}
			return result;
		}

		// Token: 0x04000323 RID: 803
		private ValueInput<BaseAbility> targetAbility;

		// Token: 0x04000324 RID: 804
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000325 RID: 805
		private ValueInput<AbilityDescription> abilityFilter;

		// Token: 0x04000326 RID: 806
		private ValueInput<bool> modifySingleAbilityOnly;

		// Token: 0x04000327 RID: 807
		private FlowOutput flowOut;
	}
}
