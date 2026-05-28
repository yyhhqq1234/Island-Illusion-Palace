using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200033D RID: 829
	[Serializable]
	public sealed class SlowMotionBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B32 RID: 6962 RVA: 0x00055FE8 File Offset: 0x000541E8
		private void SetSpeed(Playable playable, double speed)
		{
			if (this.director)
			{
				this.director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
				return;
			}
			playable.GetGraph<Playable>().GetRootPlayable(0).SetSpeed(speed);
		}

		// Token: 0x06001B33 RID: 6963 RVA: 0x00056032 File Offset: 0x00054232
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			this.director = (playerData as PlayableDirector);
			this.SetSpeed(playable, this.timeScale);
		}

		// Token: 0x06001B34 RID: 6964 RVA: 0x00056050 File Offset: 0x00054250
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			double duration = playable.GetDuration<Playable>();
			double num = playable.GetTime<Playable>() + (double)info.deltaTime;
			bool flag = info.effectivePlayState == PlayState.Paused && num > duration;
			bool flag2 = playable.GetGraph<Playable>().GetRootPlayable(0).IsDone<Playable>();
			if (flag || flag2)
			{
				this.SetSpeed(playable, 1.0);
			}
		}

		// Token: 0x06001B35 RID: 6965 RVA: 0x000560AD File Offset: 0x000542AD
		public override void OnGraphStart(Playable playable)
		{
			this.firstFrame = true;
		}

		// Token: 0x04000F52 RID: 3922
		[SerializeField]
		[Range(0.01f, 1f)]
		public double timeScale = 1.0;

		// Token: 0x04000F53 RID: 3923
		private bool firstFrame;

		// Token: 0x04000F54 RID: 3924
		private PlayableDirector director;
	}
}
