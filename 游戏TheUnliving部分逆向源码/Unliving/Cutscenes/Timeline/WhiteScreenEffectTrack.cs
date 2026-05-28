using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000343 RID: 835
	[TrackColor(0.4245283f, 0f, 0.3445641f)]
	[TrackClipType(typeof(WhiteScreenEffectClip))]
	[TrackClipType(typeof(AnimationPlayableAsset))]
	public class WhiteScreenEffectTrack : TrackAsset
	{
		// Token: 0x06001B47 RID: 6983 RVA: 0x000564D7 File Offset: 0x000546D7
		public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			base.GatherProperties(director, driver);
		}
	}
}
