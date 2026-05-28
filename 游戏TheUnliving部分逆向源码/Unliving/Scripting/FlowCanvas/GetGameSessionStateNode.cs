using System;
using FlowCanvas;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000099 RID: 153
	[Name("Get Game Session State", 0)]
	[Category("Unliving/Game Session")]
	public sealed class GetGameSessionStateNode : GameContextDependentNodeBase
	{
		// Token: 0x0600040F RID: 1039 RVA: 0x0000E3A0 File Offset: 0x0000C5A0
		private SessionState GetGameSessionState()
		{
			GameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<GameSessionManager>(out gameSessionManager))
			{
				return gameSessionManager.CurrentSessionState;
			}
			return SessionState.Undefined;
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0000E3C9 File Offset: 0x0000C5C9
		protected override void RegisterPorts()
		{
			base.AddValueOutput<SessionState>("CurrentSessionState", new ValueHandler<SessionState>(this.GetGameSessionState), "");
		}
	}
}
