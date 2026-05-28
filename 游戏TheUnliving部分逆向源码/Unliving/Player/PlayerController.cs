using System;
using Game.Core;

namespace Unliving.Player
{
	// Token: 0x0200014D RID: 333
	[Serializable]
	public abstract class PlayerController
	{
		// Token: 0x17000180 RID: 384
		// (get) Token: 0x0600092D RID: 2349 RVA: 0x0001F350 File Offset: 0x0001D550
		public PlayerBehaviour CurrentPlayer
		{
			get
			{
				return this._currentPlayer;
			}
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0001F358 File Offset: 0x0001D558
		public PlayerController(PlayerBehaviour player, IGame currentGame)
		{
			this._currentPlayer = player;
			this._currentPlayer.Destroyed += this.OnDestroy;
		}

		// Token: 0x0600092F RID: 2351
		protected abstract void OnDestroy(object playerBehaviour);

		// Token: 0x0400052A RID: 1322
		protected PlayerBehaviour _currentPlayer;
	}
}
