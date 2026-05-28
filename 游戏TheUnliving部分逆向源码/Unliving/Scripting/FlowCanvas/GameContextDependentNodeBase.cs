using System;
using FlowCanvas.Nodes;
using Game.Core;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000094 RID: 148
	public abstract class GameContextDependentNodeBase : FlowControlNode
	{
		// Token: 0x170000BD RID: 189
		// (get) Token: 0x060003F1 RID: 1009 RVA: 0x0000DAA4 File Offset: 0x0000BCA4
		protected IGame CurrentGame
		{
			get
			{
				if (this.currentGame == null && base.graphAgent != null)
				{
					GameBehaviourBase gameBehaviourBase;
					IGame game = this.currentGame = base.graphAgent.GetCurrentGame(out gameBehaviourBase);
				}
				return this.currentGame;
			}
		}

		// Token: 0x0400026F RID: 623
		private IGame currentGame;
	}
}
