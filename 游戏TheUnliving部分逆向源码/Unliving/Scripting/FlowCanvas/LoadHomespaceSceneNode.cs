using System;
using FlowCanvas;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A9 RID: 169
	[Name("Load Homespace Scene", 0)]
	[Category("Unliving/Game Session")]
	public sealed class LoadHomespaceSceneNode : GameContextDependentNodeBase
	{
		// Token: 0x0600045D RID: 1117 RVA: 0x0000F498 File Offset: 0x0000D698
		private void LoadHomespaceScene(Flow flow)
		{
			GameManager gameManager;
			if (base.CurrentGame.Services.TryGet<GameManager>(out gameManager))
			{
				gameManager.LoadHomespace();
				this.flowOut.Call(flow);
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0000F4CB File Offset: 0x0000D6CB
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.LoadHomespaceScene), "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x040002C7 RID: 711
		private FlowOutput flowOut;
	}
}
