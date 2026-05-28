using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Gameplay;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000BD RID: 189
	[Name("Modify Stamina", 0)]
	[Category("Unliving/Mobs")]
	public sealed class ModifyStaminaNode : FlowControlNode
	{
		// Token: 0x060004C6 RID: 1222 RVA: 0x000113B4 File Offset: 0x0000F5B4
		private void ModifyStamina(Flow flow)
		{
			if (this.amount.value != 0f)
			{
				GameObject value = this.targetObject.value;
				if (value != null && (this.staminaOwner != null || value.TryGetComponent<IStaminaStatOwner>(out this.staminaOwner)))
				{
					float num = this.amount.value;
					if (this.isContinuous.value)
					{
						num *= Time.deltaTime;
					}
					this.staminaOwner.CurrentStamina += num;
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x00011440 File Offset: 0x0000F640
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ModifyStamina), "");
			this.targetObject = base.AddValueInput<GameObject>("targetObject", "");
			this.amount = base.AddValueInput<float>("amount", "");
			this.isContinuous = base.AddValueInput<bool>("isContinuous", "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000331 RID: 817
		private ValueInput<GameObject> targetObject;

		// Token: 0x04000332 RID: 818
		private ValueInput<float> amount;

		// Token: 0x04000333 RID: 819
		private ValueInput<bool> isContinuous;

		// Token: 0x04000334 RID: 820
		private FlowOutput flowOut;

		// Token: 0x04000335 RID: 821
		private IStaminaStatOwner staminaOwner;
	}
}
