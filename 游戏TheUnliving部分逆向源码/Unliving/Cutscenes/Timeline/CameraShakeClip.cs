using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000328 RID: 808
	[Serializable]
	public class CameraShakeClip : PlayableAsset, ITimelineClipAsset
	{
		// Token: 0x17000595 RID: 1429
		// (get) Token: 0x06001AF8 RID: 6904 RVA: 0x000551C1 File Offset: 0x000533C1
		// (set) Token: 0x06001AF9 RID: 6905 RVA: 0x000551C9 File Offset: 0x000533C9
		public bool SetupHappened { get; set; }

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x06001AFA RID: 6906 RVA: 0x000551D2 File Offset: 0x000533D2
		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.None;
			}
		}

		// Token: 0x06001AFB RID: 6907 RVA: 0x000551D5 File Offset: 0x000533D5
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<CameraShakeBehaviour>.Create(graph, this.controls, 0);
		}

		// Token: 0x04000F22 RID: 3874
		[SerializeField]
		private CameraShakeBehaviour controls = new CameraShakeBehaviour();
	}
}
