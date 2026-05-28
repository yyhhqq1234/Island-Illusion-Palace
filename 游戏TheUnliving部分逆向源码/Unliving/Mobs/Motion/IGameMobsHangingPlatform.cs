using System;
using Common;
using UnityEngine;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000218 RID: 536
	public interface IGameMobsHangingPlatform : IDestroyable
	{
		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06001252 RID: 4690
		float Height { get; }

		// Token: 0x06001253 RID: 4691
		Vector2 GetPosition();

		// Token: 0x06001254 RID: 4692
		GameMobKinematicMotionBase GetFallMotion(GameMobMotionControllerBase motionController);

		// Token: 0x06001255 RID: 4693
		void OnMobAdded(BaseGameMob mob);

		// Token: 0x06001256 RID: 4694
		void OnMobRemoved(BaseGameMob mob);
	}
}
