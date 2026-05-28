using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000AF RID: 175
	[Name("Get Vital Energy Container State", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetVitalEnergyContainerStateNode : FlowControlNode
	{
		// Token: 0x06000478 RID: 1144 RVA: 0x0000FB98 File Offset: 0x0000DD98
		private void Update(Flow flow)
		{
			this.maxAmount = 0f;
			this.currentAmount = 0f;
			this.lackOfEnergy = 0f;
			this.isEmpty = false;
			VitalContainer value = this.vitalContainer.value;
			if (value != null)
			{
				this.maxAmount = value.InitialEnergyAmount;
				this.currentAmount = value.CurrentEnergyAmount;
				this.lackOfEnergy = this.maxAmount - this.currentAmount;
				this.isEmpty = (this.currentAmount <= 0f);
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x0000FC2C File Offset: 0x0000DE2C
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Update), "");
			this.vitalContainer = base.AddValueInput<VitalContainer>("vitalContainer", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<float>("maxAmount", () => this.maxAmount, "");
			base.AddValueOutput<float>("currentAmount", () => this.currentAmount, "");
			base.AddValueOutput<float>("lackOfEnergy", () => this.lackOfEnergy, "");
			base.AddValueOutput<bool>("isEmpty", () => this.isEmpty, "");
		}

		// Token: 0x040002DC RID: 732
		private ValueInput<VitalContainer> vitalContainer;

		// Token: 0x040002DD RID: 733
		private FlowOutput flowOut;

		// Token: 0x040002DE RID: 734
		private float maxAmount;

		// Token: 0x040002DF RID: 735
		private float currentAmount;

		// Token: 0x040002E0 RID: 736
		private float lackOfEnergy;

		// Token: 0x040002E1 RID: 737
		private bool isEmpty;
	}
}
