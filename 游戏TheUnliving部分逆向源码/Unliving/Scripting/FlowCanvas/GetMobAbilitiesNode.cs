using System;
using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200009C RID: 156
	[Name("Get Mob Abilities", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetMobAbilitiesNode : FlowControlNode
	{
		// Token: 0x06000420 RID: 1056 RVA: 0x0000E760 File Offset: 0x0000C960
		private void GetAbilities(Flow flow)
		{
			if (this.targetMob.value == null)
			{
				return;
			}
			GameAbilitiesController abilitiesController = this.targetMob.value.AbilitiesController;
			if (abilitiesController != null)
			{
				IReadOnlyList<BaseAbility> abilities = abilitiesController.Abilities;
				AbilityDescription value = this.abilityDescription.value;
				bool flag = value.IsBlank();
				for (int i = 0; i < abilities.Count; i++)
				{
					BaseAbility baseAbility = abilities[i];
					if (flag || value.IsMatch(baseAbility))
					{
						this.ability = baseAbility;
						this.flowOut.Call(flow);
						if (this.getFirstAbilityOnly.value)
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x0000E800 File Offset: 0x0000CA00
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.GetAbilities), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityDescription = base.AddValueInput<AbilityDescription>("abilityDescription", "");
			this.getFirstAbilityOnly = base.AddValueInput<bool>("getFirstAbilityOnly", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<BaseAbility>("ability", () => this.ability, "");
		}

		// Token: 0x04000296 RID: 662
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000297 RID: 663
		private ValueInput<AbilityDescription> abilityDescription;

		// Token: 0x04000298 RID: 664
		private ValueInput<bool> getFirstAbilityOnly;

		// Token: 0x04000299 RID: 665
		private FlowOutput flowOut;

		// Token: 0x0400029A RID: 666
		private BaseAbility ability;
	}
}
