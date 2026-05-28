using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200033A RID: 826
	[Serializable]
	public sealed class PostProcessingProfileOverrideBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B2A RID: 6954 RVA: 0x00055F2A File Offset: 0x0005412A
		public override void OnGraphStart(Playable playable)
		{
			base.OnGraphStart(playable);
			if (Camera.main.TryGetComponent<Volume>(out this.volume))
			{
				this.defaultProfile = this.volume.profile;
			}
		}

		// Token: 0x06001B2B RID: 6955 RVA: 0x00055F58 File Offset: 0x00054158
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			base.ProcessFrame(playable, info, playerData);
			VolumeProfile volumeProfile = playerData as VolumeProfile;
			if (volumeProfile != null)
			{
				this.volume.profile = volumeProfile;
			}
		}

		// Token: 0x06001B2C RID: 6956 RVA: 0x00055F84 File Offset: 0x00054184
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			base.OnBehaviourPause(playable, info);
			if (this.volume != null)
			{
				this.volume.profile = this.defaultProfile;
			}
		}

		// Token: 0x04000F4F RID: 3919
		private VolumeProfile defaultProfile;

		// Token: 0x04000F50 RID: 3920
		private Volume volume;
	}
}
