using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unliving.Mobs;

namespace Unliving.Cutscenes
{
	// Token: 0x0200031E RID: 798
	[Serializable]
	public class WatchedSpawnersForAnimators
	{
		// Token: 0x06001AD3 RID: 6867 RVA: 0x000545A9 File Offset: 0x000527A9
		public void BindThisMobToAnimationTrack(BaseGameMob mob)
		{
			mob.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			this.timelineController.SetGenericBinding(this.track, mob.Animator);
		}

		// Token: 0x06001AD4 RID: 6868 RVA: 0x000545D0 File Offset: 0x000527D0
		public void ProcessSpawnedGroup()
		{
			int num = 0;
			foreach (BaseGameMob mob in this.spawner.SpawnedGroup.Mobs)
			{
				this.BindThisMobToAnimationTrack(mob);
				num++;
			}
		}

		// Token: 0x04000EFF RID: 3839
		public string animatorTrackName;

		// Token: 0x04000F00 RID: 3840
		public MobBehaviourSpawner spawner;

		// Token: 0x04000F01 RID: 3841
		[HideInInspector]
		public AnimationTrack track;

		// Token: 0x04000F02 RID: 3842
		[HideInInspector]
		public PlayableDirector timelineController;
	}
}
