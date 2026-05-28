using System;
using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A1 RID: 161
	[Name("Get Vital Energy State", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetVitalEnergyStateNode : FlowControlNode
	{
		// Token: 0x06000438 RID: 1080 RVA: 0x0000ED20 File Offset: 0x0000CF20
		private void Update(Flow flow)
		{
			this.vitalEnergyContainers = null;
			this.hasEnoughVitalEnergy = false;
			this.currentVitalEnergy = 0f;
			this.lackOfVitalEnergy = 0f;
			if (this.targetMob.value != null)
			{
				VitalEnergyHitPointsController vitalEnergyHitPointsController = this.targetMob.value.HitPointsController as VitalEnergyHitPointsController;
				if (vitalEnergyHitPointsController != null)
				{
					this.vitalEnergyContainers = (IList<VitalContainer>)vitalEnergyHitPointsController.VitalContainers;
					this.currentVitalEnergy = vitalEnergyHitPointsController.CurrentVitalAmount;
					this.lackOfVitalEnergy = Mathf.Max(Mathf.Abs(this.requiredAmount.value) - this.currentVitalEnergy, 0f);
					this.hasEnoughVitalEnergy = (this.lackOfVitalEnergy == 0f);
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x0000EDE8 File Offset: 0x0000CFE8
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Update), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.requiredAmount = base.AddValueInput<float>("requiredAmount", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<IList<VitalContainer>>("vitalEnergyContainers", () => this.vitalEnergyContainers, "");
			base.AddValueOutput<float>("currentVitalEnergy", () => this.currentVitalEnergy, "");
			base.AddValueOutput<float>("lackOfVitalEnergy", () => this.lackOfVitalEnergy, "");
			base.AddValueOutput<bool>("hasEnoughVitalEnergy", () => this.hasEnoughVitalEnergy, "");
		}

		// Token: 0x040002AB RID: 683
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x040002AC RID: 684
		private ValueInput<float> requiredAmount;

		// Token: 0x040002AD RID: 685
		private FlowOutput flowOut;

		// Token: 0x040002AE RID: 686
		private IList<VitalContainer> vitalEnergyContainers;

		// Token: 0x040002AF RID: 687
		private float currentVitalEnergy;

		// Token: 0x040002B0 RID: 688
		private float lackOfVitalEnergy;

		// Token: 0x040002B1 RID: 689
		private bool hasEnoughVitalEnergy;
	}
}
