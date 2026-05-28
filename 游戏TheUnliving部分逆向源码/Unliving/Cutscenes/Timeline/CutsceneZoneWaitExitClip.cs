using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200032C RID: 812
	public sealed class CutsceneZoneWaitExitClip : PlayableAsset
	{
		// Token: 0x06001B04 RID: 6916 RVA: 0x00055386 File Offset: 0x00053586
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<CutsceneZoneBehaviour>.Create(graph, 0);
		}
	}
}
