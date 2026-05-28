using System;
using UnityEngine;
using UnityEngine.Playables;
using Unliving.Mobs.Motion;
using Unliving.Player;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000338 RID: 824
	public sealed class PlayerMoveToBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B23 RID: 6947 RVA: 0x00055BD4 File Offset: 0x00053DD4
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (this.player == null || this.targetPoint == null)
			{
				return;
			}
			if (!this.isFirstFrameHappened)
			{
				this.isFirstFrameHappened = true;
				GameMobMotionControllerBase motionController = this.player.MotionController;
				this.currentMotion = motionController.MoveToPoint(this.targetPoint.position, true, this.player);
				if (motionController == null)
				{
					Debug.LogError("Failed to initiate player motion.");
				}
				if (this.lookToward)
				{
					this.player.SetCutsceneFacingDirection(this.lookToward.position, false);
				}
			}
		}

		// Token: 0x06001B24 RID: 6948 RVA: 0x00055C74 File Offset: 0x00053E74
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			double duration = playable.GetDuration<Playable>();
			double num = playable.GetTime<Playable>() + (double)info.deltaTime;
			bool flag = info.effectivePlayState == PlayState.Paused && num > duration;
			bool flag2 = playable.GetGraph<Playable>().GetRootPlayable(0).IsDone<Playable>();
			bool flag3 = flag || flag2;
			this.rootPlayable = playable;
			if (flag3 && this.pauseTimelineExecution && Application.isPlaying)
			{
				if (this.player.MotionController.CurrentKinematicMotion == this.currentMotion)
				{
					this.player.MotionController.KinematicMotionCompleted += this.OnPlayerMotionCompleted;
					this.currentGraph.GetRootPlayable(0).SetSpeed(0.0);
					return;
				}
				if (this.lookToward)
				{
					this.player.SetCutsceneFacingDirection(this.lookToward.position, false);
				}
			}
		}

		// Token: 0x06001B25 RID: 6949 RVA: 0x00055D58 File Offset: 0x00053F58
		private void OnPlayerMotionCompleted(GameMobKinematicMotionBase motion)
		{
			if (this.currentMotion == motion)
			{
				this.currentGraph.GetRootPlayable(0).SetSpeed(1.0);
				this.currentMotion = null;
				this.player.MotionController.KinematicMotionCompleted -= this.OnPlayerMotionCompleted;
				if (this.rootPlayable.IsValid<Playable>())
				{
					PlayableDirector playableDirector = this.rootPlayable.GetGraph<Playable>().GetResolver() as PlayableDirector;
					CutsceneBase cutsceneBase;
					if (playableDirector != null && playableDirector.TryGetComponent<CutsceneBase>(out cutsceneBase))
					{
						cutsceneBase.OnConversationCompleted();
					}
				}
				if (this.lookToward)
				{
					this.player.SetCutsceneFacingDirection(this.lookToward.position, false);
				}
			}
		}

		// Token: 0x04000F44 RID: 3908
		public PlayableGraph currentGraph;

		// Token: 0x04000F45 RID: 3909
		public PlayerBehaviour player;

		// Token: 0x04000F46 RID: 3910
		public Transform targetPoint;

		// Token: 0x04000F47 RID: 3911
		public Transform lookToward;

		// Token: 0x04000F48 RID: 3912
		[Tooltip("Timeline will be paused until the movement is finished")]
		public bool pauseTimelineExecution;

		// Token: 0x04000F49 RID: 3913
		private bool isFirstFrameHappened;

		// Token: 0x04000F4A RID: 3914
		private GameMobKinematicMotionBase currentMotion;

		// Token: 0x04000F4B RID: 3915
		private Playable rootPlayable;
	}
}
