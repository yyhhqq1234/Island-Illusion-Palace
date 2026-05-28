using System;
using FlowCanvas;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200008D RID: 141
	[Name("Change Game Session State", 0)]
	[Category("Unliving/Game Session")]
	public sealed class ChangeGameSessionStateNode : GameContextDependentNodeBase
	{
		// Token: 0x060003D6 RID: 982 RVA: 0x0000D470 File Offset: 0x0000B670
		private void ChangeGameSessionState(Flow flow)
		{
			GameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<GameSessionManager>(out gameSessionManager))
			{
				gameSessionManager.SetSessionState(this.targetSessionState.value);
				this.flowOut.Call(flow);
			}
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0000D4B0 File Offset: 0x0000B6B0
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ChangeGameSessionState), "");
			this.targetSessionState = base.AddValueInput<SessionState>("targetSessionState", "");
			this.targetSessionState.SetDefaultAndSerializedValue(SessionState.Undefined);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000257 RID: 599
		private ValueInput<SessionState> targetSessionState;

		// Token: 0x04000258 RID: 600
		private FlowOutput flowOut;
	}
}
