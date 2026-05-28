using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B4 RID: 180
	[Name("Use External Ability", 0)]
	[Category("Unliving/Mobs")]
	public sealed class UseExternalAbilityNode : FlowControlNode
	{
		// Token: 0x0600048D RID: 1165 RVA: 0x000100CC File Offset: 0x0000E2CC
		private void Use(Flow flow)
		{
			AbilityID value = this.abilityID.value;
			if (value != AbilityID.None)
			{
				BaseGameMob value2 = this.targetMob.value;
				IPlayerAbilitiesController playerAbilitiesController = ((value2 != null) ? value2.AbilitiesController : null) as IPlayerAbilitiesController;
				if (playerAbilitiesController != null)
				{
					UseExternalAbilityNode.AbilityUsingArgs.targetPosition = value2.Position;
					AbilityInfo abilityInfo = new AbilityInfo(value, this.abilityLevel.value);
					playerAbilitiesController.UseExternalAbility(abilityInfo, UseExternalAbilityNode.AbilityUsingArgs);
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x0001014C File Offset: 0x0000E34C
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Use), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityID = base.AddValueInput<AbilityID>("abilityID", "");
			this.abilityLevel = base.AddValueInput<int>("abilityLevel", "");
			this.abilityLevel.SetDefaultAndSerializedValue(1);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x040002F0 RID: 752
		private static readonly BaseAbility.UsingArgs AbilityUsingArgs = new BaseAbility.UsingArgs();

		// Token: 0x040002F1 RID: 753
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x040002F2 RID: 754
		private ValueInput<AbilityID> abilityID;

		// Token: 0x040002F3 RID: 755
		private ValueInput<int> abilityLevel;

		// Token: 0x040002F4 RID: 756
		private FlowOutput flowOut;
	}
}
