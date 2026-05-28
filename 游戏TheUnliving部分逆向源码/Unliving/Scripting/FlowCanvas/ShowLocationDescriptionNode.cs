using System;
using FlowCanvas;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C7 RID: 199
	[Name("Show Location Description", 0)]
	[Category("Unliving/Test")]
	public class ShowLocationDescriptionNode : GameContextDependentNodeBase
	{
		// Token: 0x060004F6 RID: 1270 RVA: 0x00012174 File Offset: 0x00010374
		private void ShowLocationDescription(Flow flow)
		{
			this.flowOut.Call(flow);
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00012182 File Offset: 0x00010382
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ShowLocationDescription), "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000365 RID: 869
		private FlowOutput flowOut;
	}
}
