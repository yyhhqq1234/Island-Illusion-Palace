using System;
using Game.Core;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Misc
{
	// Token: 0x02000243 RID: 579
	[DefaultExecutionOrder(50)]
	public sealed class PlayerPositionFollower : GameBehaviourBase
	{
		// Token: 0x0600139B RID: 5019 RVA: 0x0003D7C8 File Offset: 0x0003B9C8
		private void Start()
		{
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				this.currentPlayer = playerProvider.CurrentPlayer;
			}
		}

		// Token: 0x0600139C RID: 5020 RVA: 0x0003D7F5 File Offset: 0x0003B9F5
		private void LateUpdate()
		{
			if (this.currentPlayer == null)
			{
				return;
			}
			base.transform.position = this.currentPlayer.transform.position;
		}

		// Token: 0x04000B70 RID: 2928
		private PlayerBehaviour currentPlayer;
	}
}
