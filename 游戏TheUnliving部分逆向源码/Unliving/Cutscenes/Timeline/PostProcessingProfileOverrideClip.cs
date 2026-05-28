using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x0200033B RID: 827
	[Serializable]
	public class PostProcessingProfileOverrideClip : PlayableAsset, ITimelineClipAsset
	{
		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x06001B2E RID: 6958 RVA: 0x00055FB5 File Offset: 0x000541B5
		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.None;
			}
		}

		// Token: 0x06001B2F RID: 6959 RVA: 0x00055FB8 File Offset: 0x000541B8
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<PostProcessingProfileOverrideBehaviour>.Create(graph, this.controls, 0);
		}

		// Token: 0x04000F51 RID: 3921
		[SerializeField]
		private PostProcessingProfileOverrideBehaviour controls = new PostProcessingProfileOverrideBehaviour();
	}
}
