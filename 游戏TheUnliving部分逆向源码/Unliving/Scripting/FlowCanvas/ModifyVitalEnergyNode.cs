using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000BE RID: 190
	[Name("Modify Vital Energy", 0)]
	[Category("Unliving/Mobs")]
	public sealed class ModifyVitalEnergyNode : FlowControlNode
	{
		// Token: 0x060004C9 RID: 1225 RVA: 0x000114CC File Offset: 0x0000F6CC
		private void ModifyVitalEnergy(Flow flow)
		{
			float num = this.amount.value;
			if (num != 0f)
			{
				BaseGameMob value = this.targetMob.value;
				VitalEnergyHitPointsController vitalEnergyHitPointsController = ((value != null) ? value.HitPointsController : null) as VitalEnergyHitPointsController;
				if (vitalEnergyHitPointsController != null)
				{
					if (this.isContinuous.value)
					{
						num *= Time.deltaTime;
					}
					HitPointsController.HPChangingArgs hpchangingArgs = (num > 0f) ? ModifyVitalEnergyNode.RestoringArgs : ModifyVitalEnergyNode.ConsumingArgs;
					hpchangingArgs.amount = num;
					vitalEnergyHitPointsController.ModifyHitPoints(base.graphAgent, hpchangingArgs);
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x00011560 File Offset: 0x0000F760
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ModifyVitalEnergy), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.amount = base.AddValueInput<float>("amount", "");
			this.isContinuous = base.AddValueInput<bool>("isContinuous", "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000336 RID: 822
		private static readonly VitalEnergyHitPointsController.RestoreVitalEnergyArgs RestoringArgs = new VitalEnergyHitPointsController.RestoreVitalEnergyArgs();

		// Token: 0x04000337 RID: 823
		private static readonly VitalEnergyHitPointsController.ConsumeVitalEnergyArgs ConsumingArgs = new VitalEnergyHitPointsController.ConsumeVitalEnergyArgs();

		// Token: 0x04000338 RID: 824
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000339 RID: 825
		private ValueInput<float> amount;

		// Token: 0x0400033A RID: 826
		private ValueInput<bool> isContinuous;

		// Token: 0x0400033B RID: 827
		private FlowOutput flowOut;
	}
}
