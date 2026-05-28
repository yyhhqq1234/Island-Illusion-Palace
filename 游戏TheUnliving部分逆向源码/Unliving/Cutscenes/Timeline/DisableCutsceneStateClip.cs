using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200032E RID: 814
	public sealed class DisableCutsceneStateClip : PlayableAsset
	{
		// Token: 0x06001B08 RID: 6920 RVA: 0x000553F8 File Offset: 0x000535F8
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<DisableCutsceneStateBehaviour>.Create(graph, new DisableCutsceneStateBehaviour
			{
				cutsceneStateIsEnabled = this.cutsceneStateIsEnabled
			}, 0);
		}

		// Token: 0x04000F29 RID: 3881
		public bool cutsceneStateIsEnabled;
	}
}
