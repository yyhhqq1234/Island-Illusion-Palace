using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200033E RID: 830
	[Serializable]
	public class SlowMotionClip : PlayableAsset, ITimelineClipAsset
	{
		// Token: 0x17000599 RID: 1433
		// (get) Token: 0x06001B37 RID: 6967 RVA: 0x000560CD File Offset: 0x000542CD
		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.Blending;
			}
		}

		// Token: 0x06001B38 RID: 6968 RVA: 0x000560D1 File Offset: 0x000542D1
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<SlowMotionBehaviour>.Create(graph, this.controls, 0);
		}

		// Token: 0x04000F55 RID: 3925
		[SerializeField]
		private SlowMotionBehaviour controls = new SlowMotionBehaviour();
	}
}
