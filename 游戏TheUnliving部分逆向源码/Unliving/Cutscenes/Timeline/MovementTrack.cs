using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000332 RID: 818
	[TrackClipType(typeof(MovementClip))]
	[TrackBindingType(typeof(Transform))]
	public class MovementTrack : TrackAsset
	{
		// Token: 0x06001B12 RID: 6930 RVA: 0x00055724 File Offset: 0x00053924
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			foreach (TimelineClip timelineClip in base.GetClips())
			{
				MovementClip movementClip = timelineClip.asset as MovementClip;
				if (movementClip)
				{
					movementClip.clipPassthrough = timelineClip;
				}
			}
			return base.CreateTrackMixer(graph, go, inputCount);
		}

		// Token: 0x06001B13 RID: 6931 RVA: 0x00055790 File Offset: 0x00053990
		public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			base.GatherProperties(director, driver);
		}
	}
}
