using System;
using Game.Core;
using UnityEngine;

namespace Unliving.Player
{
	// Token: 0x02000162 RID: 354
	[ExecuteInEditMode]
	public sealed class PlayerFollowCameraShaker : GameBehaviourBase
	{
		// Token: 0x060009E4 RID: 2532 RVA: 0x00021D18 File Offset: 0x0001FF18
		private void Start()
		{
			if (base.CurrentGame == null)
			{
				return;
			}
			PlayerCameraFollow playerCameraFollow = base.CurrentGame.Services.Get<PlayerCameraFollow>();
			if (playerCameraFollow != null)
			{
				playerCameraFollow.AddShakeImpulse(this.shakeImpulse);
			}
		}

		// Token: 0x040005CC RID: 1484
		public PlayerCameraFollow.ShakeImpulse shakeImpulse;
	}
}
