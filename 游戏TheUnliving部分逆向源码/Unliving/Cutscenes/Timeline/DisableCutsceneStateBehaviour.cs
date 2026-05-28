using System;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200032D RID: 813
	public sealed class DisableCutsceneStateBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B06 RID: 6918 RVA: 0x0005539C File Offset: 0x0005359C
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (!this.isFirstFrameHappened)
			{
				this.isFirstFrameHappened = true;
				CutsceneBase cutsceneBase = playerData as CutsceneBase;
				if (playerData == null)
				{
					(playable.GetGraph<Playable>().GetResolver() as PlayableDirector).gameObject.TryGetComponent<CutsceneBase>(out cutsceneBase);
				}
				cutsceneBase.SetGlobalCutsceneState(this.cutsceneStateIsEnabled);
				return;
			}
		}

		// Token: 0x04000F27 RID: 3879
		public bool cutsceneStateIsEnabled;

		// Token: 0x04000F28 RID: 3880
		private bool isFirstFrameHappened;
	}
}
