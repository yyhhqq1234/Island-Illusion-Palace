using System;
using FlowCanvas;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000AA RID: 170
	[Name("Load Next Scene", 0)]
	[Category("Unliving/Game Session")]
	public sealed class LoadNextSceneNode : GameContextDependentNodeBase
	{
		// Token: 0x06000460 RID: 1120 RVA: 0x0000F508 File Offset: 0x0000D708
		private void LoadNextScene(Flow flow)
		{
			GameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<GameSessionManager>(out gameSessionManager))
			{
				gameSessionManager.LoadNextScene();
				this.flowOut.Call(flow);
			}
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0000F53B File Offset: 0x0000D73B
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.LoadNextScene), "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x040002C8 RID: 712
		private FlowOutput flowOut;
	}
}
