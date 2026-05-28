using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000340 RID: 832
	[Serializable]
	public class WhiteScreenEffectBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B3B RID: 6971 RVA: 0x00056100 File Offset: 0x00054300
		private void MoveUpWatchedSprites(bool isWhitescreen)
		{
			string sortingLayerName = isWhitescreen ? "UI" : "Dynamic Perspective";
			int num = 10001;
			num = (isWhitescreen ? num : (-num));
			foreach (SpriteRenderer spriteRenderer in this.spritesToWhitescreen)
			{
				spriteRenderer.sortingLayerName = sortingLayerName;
				spriteRenderer.sortingOrder += num;
			}
		}

		// Token: 0x06001B3C RID: 6972 RVA: 0x00056180 File Offset: 0x00054380
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (this.firstFrame)
			{
				this.MoveUpWatchedSprites(true);
				this.firstFrame = false;
			}
		}

		// Token: 0x06001B3D RID: 6973 RVA: 0x00056198 File Offset: 0x00054398
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (this.firstFrame)
			{
				return;
			}
			double duration = playable.GetDuration<Playable>();
			double num = playable.GetTime<Playable>() + (double)info.deltaTime;
			bool flag = info.effectivePlayState == PlayState.Paused && num > duration;
			bool flag2 = playable.GetGraph<Playable>().GetRootPlayable(0).IsDone<Playable>();
			if (flag || flag2)
			{
				this.MoveUpWatchedSprites(false);
				this.firstFrame = true;
			}
		}

		// Token: 0x04000F56 RID: 3926
		public List<SpriteRenderer> spritesToWhitescreen = new List<SpriteRenderer>();

		// Token: 0x04000F57 RID: 3927
		private bool firstFrame = true;
	}
}
