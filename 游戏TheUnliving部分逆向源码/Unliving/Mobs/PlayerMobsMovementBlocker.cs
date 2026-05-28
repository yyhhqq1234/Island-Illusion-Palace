using System;
using Game.Core;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001F9 RID: 505
	public class PlayerMobsMovementBlocker : GameBehaviourBase
	{
		// Token: 0x060010E1 RID: 4321 RVA: 0x00034DB0 File Offset: 0x00032FB0
		private async void Start()
		{
			await new WaitForEndOfFrame();
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				this.playerMobGroup = playerProvider.CurrentPlayer.Group;
			}
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x00034DE9 File Offset: 0x00032FE9
		public void SetActiveState(bool isActive)
		{
			this.isActive = isActive;
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x00034DF4 File Offset: 0x00032FF4
		private void Update()
		{
			if (this.isActive && this.playerMobGroup != null)
			{
				this.playerMobGroup.GroupDestination = null;
			}
		}

		// Token: 0x0400098F RID: 2447
		private GameMobsGroupControllerBase playerMobGroup;

		// Token: 0x04000990 RID: 2448
		private bool isActive;
	}
}
