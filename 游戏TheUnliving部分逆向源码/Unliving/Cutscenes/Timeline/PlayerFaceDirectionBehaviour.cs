using System;
using UnityEngine;
using UnityEngine.Playables;
using Unliving.Mobs.Motion;
using Unliving.Player;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000336 RID: 822
	public sealed class PlayerFaceDirectionBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B1F RID: 6943 RVA: 0x00055AF8 File Offset: 0x00053CF8
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (this.player == null || this.targetPoint == null)
			{
				return;
			}
			if (!this.isFirstFrameHappened)
			{
				this.isFirstFrameHappened = true;
				this.player.SetCutsceneFacingDirection(this.targetPoint.position, false);
			}
		}

		// Token: 0x04000F3F RID: 3903
		public PlayerBehaviour player;

		// Token: 0x04000F40 RID: 3904
		public Transform targetPoint;

		// Token: 0x04000F41 RID: 3905
		private bool isFirstFrameHappened;

		// Token: 0x04000F42 RID: 3906
		private GameMobKinematicMotionBase currentMotion;
	}
}
