using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200032A RID: 810
	public sealed class CutsceneZoneBehaviour : PlayableBehaviour
	{
		// Token: 0x06001AFE RID: 6910 RVA: 0x00055204 File Offset: 0x00053404
		private void SetTimelinePauseState(bool isPaused)
		{
			if (Application.isPlaying && this.currentPlayable.IsValid<Playable>())
			{
				this.currentPlayable.GetGraph<Playable>().GetRootPlayable(0).SetSpeed((double)(isPaused ? 0 : 1));
			}
		}

		// Token: 0x06001AFF RID: 6911 RVA: 0x00055248 File Offset: 0x00053448
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (!this.isActivated)
			{
				this.currentPlayable = playable;
				this.zone = (playerData as CutsceneZone);
				string name = this.zone.gameObject.name;
				if (this.zone == null)
				{
					Debug.LogError("Failed to get CutsceneZone for " + name + " (is it linked in the track?)");
				}
				this.isActivated = true;
			}
		}

		// Token: 0x06001B00 RID: 6912 RVA: 0x000552AC File Offset: 0x000534AC
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (!this.currentPlayable.IsValid<Playable>())
			{
				this.currentPlayable = playable;
			}
			double duration = playable.GetDuration<Playable>();
			double num = playable.GetTime<Playable>() + (double)info.deltaTime;
			bool flag = info.effectivePlayState == PlayState.Paused && num > duration;
			bool flag2 = playable.GetGraph<Playable>().GetRootPlayable(0).IsDone<Playable>();
			if ((flag || flag2) && this.zone.havePlayerInside)
			{
				this.SetTimelinePauseState(true);
				this.zone.OnInteractiveZoneExitEvent.AddListener(new UnityAction(this.OnInteractiveZoneExit));
				Debug.Log("CutsceneZoneBehaviour clip finished. Pausing timeline");
			}
		}

		// Token: 0x06001B01 RID: 6913 RVA: 0x00055347 File Offset: 0x00053547
		private void OnInteractiveZoneExit()
		{
			this.SetTimelinePauseState(false);
			this.zone.OnInteractiveZoneExitEvent.RemoveListener(new UnityAction(this.OnInteractiveZoneExit));
			Debug.Log("CutsceneZoneBehaviour OnInteractiveZoneExit event was called. Unpausing timeline");
		}

		// Token: 0x04000F24 RID: 3876
		private CutsceneZone zone;

		// Token: 0x04000F25 RID: 3877
		private Playable currentPlayable;

		// Token: 0x04000F26 RID: 3878
		private bool isActivated;
	}
}
