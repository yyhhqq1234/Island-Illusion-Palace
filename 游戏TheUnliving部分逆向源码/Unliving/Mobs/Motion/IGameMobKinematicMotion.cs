using System;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000216 RID: 534
	public interface IGameMobKinematicMotion
	{
		// Token: 0x0600124B RID: 4683
		bool Start(GameMobMotionControllerBase motionController);

		// Token: 0x0600124C RID: 4684
		void TryResetPauseState();

		// Token: 0x0600124D RID: 4685
		void OnCompleted(GameMobMotionControllerBase motionController);
	}
}
