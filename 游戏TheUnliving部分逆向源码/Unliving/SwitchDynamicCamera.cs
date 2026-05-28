using System;
using Game.Core;
using Unliving.Player;

namespace Unliving
{
	// Token: 0x02000017 RID: 23
	public sealed class SwitchDynamicCamera : GameBehaviourBase
	{
		// Token: 0x06000125 RID: 293 RVA: 0x00005100 File Offset: 0x00003300
		public void SwitchCamera(bool on)
		{
			PlayerCameraFollow playerCameraFollow;
			if (base.CurrentGame.Services.TryGet<PlayerCameraFollow>(out playerCameraFollow))
			{
				playerCameraFollow.UseDynamicCamera = on;
			}
		}
	}
}
