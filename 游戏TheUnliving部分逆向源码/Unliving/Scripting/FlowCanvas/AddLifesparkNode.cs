using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Currencies;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000087 RID: 135
	[Name("Add Lifespark Node", 0)]
	[Category("Unliving/Mobs")]
	public sealed class AddLifesparkNode : FlowControlNode
	{
		// Token: 0x060003BE RID: 958 RVA: 0x0000CE58 File Offset: 0x0000B058
		private void AddLifespark(Flow flow)
		{
			IDamageable hitPointsController = this.gameMob.value.HitPointsController;
			AbilityInfo abilityInfo = new AbilityInfo
			{
				abilityID = this.lifesparkAbilityID.value
			};
			IContainerBasedHPController containerBasedHPController = hitPointsController as IContainerBasedHPController;
			if (containerBasedHPController != null)
			{
				containerBasedHPController.AddContainer(new HealthContainer(hitPointsController.InitialHitPoints));
			}
			IAbilityActivatedContainersController abilityActivatedContainersController = hitPointsController as IAbilityActivatedContainersController;
			if (abilityActivatedContainersController != null)
			{
				abilityActivatedContainersController.AddContainer(abilityInfo, default(CurrencyOperationArgs));
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0000CEDC File Offset: 0x0000B0DC
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.AddLifespark), "");
			this.lifesparkAbilityID = base.AddValueInput<AbilityID>("lifesparkAbilityID", "");
			this.gameMob = base.AddValueInput<IGameMob>("gameMob", "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000243 RID: 579
		private ValueInput<AbilityID> lifesparkAbilityID;

		// Token: 0x04000244 RID: 580
		private ValueInput<IGameMob> gameMob;

		// Token: 0x04000245 RID: 581
		private FlowOutput flowOut;
	}
}
