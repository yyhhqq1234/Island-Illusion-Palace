using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000334 RID: 820
	public sealed class NPCConversationClip : PlayableAsset
	{
		// Token: 0x06001B1C RID: 6940 RVA: 0x00055AAC File Offset: 0x00053CAC
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			PlayableDirector timelineController = this.playableDirectorToStop.Resolve(graph.GetResolver());
			NPCConversationBehaviour template = new NPCConversationBehaviour
			{
				timelineController = timelineController
			};
			return ScriptPlayable<NPCConversationBehaviour>.Create(graph, template, 0);
		}

		// Token: 0x04000F3E RID: 3902
		[SerializeField]
		[Tooltip("Which timeline to pause if the clip is finished, but the dialogue is still going")]
		protected ExposedReference<PlayableDirector> playableDirectorToStop;
	}
}
