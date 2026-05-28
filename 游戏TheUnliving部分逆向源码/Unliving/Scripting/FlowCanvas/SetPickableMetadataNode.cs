using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using Unliving.DropSystem;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C5 RID: 197
	[Name("Set Pickable Metadata", 0)]
	[Category("Unliving/Misc")]
	public sealed class SetPickableMetadataNode : FlowControlNode
	{
		// Token: 0x060004F0 RID: 1264 RVA: 0x00011FF0 File Offset: 0x000101F0
		private void SetMetadata(Flow flow)
		{
			CustomInteractiveObject customInteractiveObject;
			if (base.graphAgent.TryGetComponent<CustomInteractiveObject>(out customInteractiveObject))
			{
				customInteractiveObject.SetLocalizationData(customInteractiveObject.metadataKey, new string[]
				{
					this.amount.value.ToString()
				});
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x00012040 File Offset: 0x00010240
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.SetMetadata), "");
			this.amount = base.AddValueInput<int>("amount", "");
			this.amount.SetDefaultAndSerializedValue(0);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000360 RID: 864
		private ValueInput<int> amount;

		// Token: 0x04000361 RID: 865
		private FlowOutput flowOut;
	}
}
