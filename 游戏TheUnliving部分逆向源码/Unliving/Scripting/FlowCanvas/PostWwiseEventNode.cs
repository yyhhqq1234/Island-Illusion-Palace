using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000CC RID: 204
	[Name("Post Wwise Event", 0)]
	[Category("Unliving/Audio")]
	public sealed class PostWwiseEventNode : FlowControlNode
	{
		// Token: 0x06000503 RID: 1283 RVA: 0x00012540 File Offset: 0x00010740
		private void PostEvent(Flow flow)
		{
			if (AkSoundEngine.IsInitialized() && this.eventObject.value != null && !string.IsNullOrEmpty(this.wwiseEventName.value))
			{
				if (this.eventID == 0U)
				{
					this.eventID = AkUtilities.ShortIDGenerator.Compute(this.wwiseEventName.value);
				}
				AkSoundEngine.PostEvent(this.eventID, this.eventObject.value);
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x000125BC File Offset: 0x000107BC
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.PostEvent), "");
			this.eventObject = base.AddValueInput<GameObject>("eventObject", "");
			this.wwiseEventName = base.AddValueInput<string>("wwiseEventName", "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000373 RID: 883
		private ValueInput<GameObject> eventObject;

		// Token: 0x04000374 RID: 884
		private ValueInput<string> wwiseEventName;

		// Token: 0x04000375 RID: 885
		private FlowOutput flowOut;

		// Token: 0x04000376 RID: 886
		private uint eventID;
	}
}
