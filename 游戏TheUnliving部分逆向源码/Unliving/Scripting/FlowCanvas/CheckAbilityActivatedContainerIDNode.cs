using System;
using FlowCanvas;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200008F RID: 143
	[Name("Check Ability Activated Container ID", 0)]
	[Category("Unliving/Abilities")]
	public sealed class CheckAbilityActivatedContainerIDNode : GameContextDependentNodeBase
	{
		// Token: 0x060003DC RID: 988 RVA: 0x0000D678 File Offset: 0x0000B878
		private void CheckAbilityID(Flow flow)
		{
			PlayerBehaviour playerBehaviour;
			if (base.CurrentGame.TryGetPlayer(out playerBehaviour))
			{
				IAbilityActivatedContainersController abilityActivatedContainersController = playerBehaviour.HitPointsController as IAbilityActivatedContainersController;
				if (abilityActivatedContainersController != null && abilityActivatedContainersController.HasAbilityContainer(this.abilityID.value))
				{
					this.result = true;
					this.flowOut.Call(flow);
					return;
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0000D6D8 File Offset: 0x0000B8D8
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.CheckAbilityID), "");
			this.abilityID = base.AddValueInput<AbilityID>("abilityID", "");
			this.abilityID.SetDefaultAndSerializedValue(AbilityID.None);
			base.AddValueOutput<bool>("Result", () => this.result, "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x0400025E RID: 606
		private ValueInput<AbilityID> abilityID;

		// Token: 0x0400025F RID: 607
		private bool result;

		// Token: 0x04000260 RID: 608
		private FlowOutput flowOut;
	}
}
