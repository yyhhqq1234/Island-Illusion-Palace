using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000331 RID: 817
	[Serializable]
	public class MovementClip : PlayableAsset, ITimelineClipAsset, IPropertyPreview
	{
		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x06001B0E RID: 6926 RVA: 0x0005561A File Offset: 0x0005381A
		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.Blending;
			}
		}

		// Token: 0x06001B0F RID: 6927 RVA: 0x00055620 File Offset: 0x00053820
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			Transform transform = this.endPosition.Resolve(graph.GetResolver());
			if (!transform)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Could not resolve final position for clip ",
					base.name,
					" (",
					transform.name,
					")"
				}));
			}
			if (this.clipPassthrough == null)
			{
				Debug.LogError("Clip passthrough did not happen for " + base.name);
			}
			MovementBehaviour template = new MovementBehaviour
			{
				end = transform.position,
				parabolaHeight = this.parabolaHeight,
				overshootTime = (float)this.clipPassthrough.easeOutDuration,
				startOffset = this.startOffset,
				endOffset = this.endOffset
			};
			return ScriptPlayable<MovementBehaviour>.Create(graph, template, 0);
		}

		// Token: 0x06001B10 RID: 6928 RVA: 0x000556F9 File Offset: 0x000538F9
		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			driver.AddFromName<Transform>("m_LocalPosition.x");
			driver.AddFromName<Transform>("m_LocalPosition.y");
		}

		// Token: 0x04000F33 RID: 3891
		[Header("If you want overshoot, specify EaseOut duration of the clip")]
		[Space]
		public ExposedReference<Transform> endPosition;

		// Token: 0x04000F34 RID: 3892
		public float parabolaHeight = 1f;

		// Token: 0x04000F35 RID: 3893
		[Space]
		public Vector2 startOffset;

		// Token: 0x04000F36 RID: 3894
		public Vector2 endOffset;

		// Token: 0x04000F37 RID: 3895
		[NonSerialized]
		public TimelineClip clipPassthrough;
	}
}
