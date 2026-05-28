using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting
{
	// Token: 0x02000080 RID: 128
	[Name("Setup Vital Energy HP Controller", 0)]
	[Category("Unliving/Mobs")]
	public sealed class SetupVitalEnergyHPControllerNode : FlowControlNode
	{
		// Token: 0x06000390 RID: 912 RVA: 0x0000C0BC File Offset: 0x0000A2BC
		private void SetupHPController(Flow flow)
		{
			IContainerBasedHPControllerWithVitalEnergy containerBasedHPControllerWithVitalEnergy = null;
			if (this.targetMob.value != null)
			{
				containerBasedHPControllerWithVitalEnergy = (this.targetMob.value.HitPointsController as IContainerBasedHPControllerWithVitalEnergy);
			}
			else if (this.targetObject.value != null)
			{
				containerBasedHPControllerWithVitalEnergy = this.targetObject.value.GetComponent<IContainerBasedHPControllerWithVitalEnergy>();
			}
			if (containerBasedHPControllerWithVitalEnergy != null)
			{
				containerBasedHPControllerWithVitalEnergy.RestoreVitalEnergyOnHealing = this.restoreVitalEnergyOnHealing.value;
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000391 RID: 913 RVA: 0x0000C13C File Offset: 0x0000A33C
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.SetupHPController), "");
			this.targetObject = base.AddValueInput<GameObject>("targetObject", "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.restoreVitalEnergyOnHealing = base.AddValueInput<bool>("restoreVitalEnergyOnHealing", "");
			this.restoreVitalEnergyOnHealing.SetDefaultAndSerializedValue(true);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x0400021A RID: 538
		private ValueInput<GameObject> targetObject;

		// Token: 0x0400021B RID: 539
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x0400021C RID: 540
		private ValueInput<bool> restoreVitalEnergyOnHealing;

		// Token: 0x0400021D RID: 541
		private FlowOutput flowOut;
	}
}
