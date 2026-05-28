using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000341 RID: 833
	[Serializable]
	public class WhiteScreenEffectClip : PlayableAsset, ITimelineClipAsset, ISerializationCallbackReceiver
	{
		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x06001B3F RID: 6975 RVA: 0x00056216 File Offset: 0x00054416
		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.Blending;
			}
		}

		// Token: 0x06001B40 RID: 6976 RVA: 0x0005621C File Offset: 0x0005441C
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			this.UnexposeSpriteRenderers(graph);
			WhiteScreenEffectBehaviour template = new WhiteScreenEffectBehaviour
			{
				spritesToWhitescreen = this.sprites
			};
			return ScriptPlayable<WhiteScreenEffectBehaviour>.Create(graph, template, 0);
		}

		// Token: 0x06001B41 RID: 6977 RVA: 0x00056250 File Offset: 0x00054450
		public void UnexposeSpriteRenderers(PlayableGraph graph)
		{
			this.sprites.Clear();
			foreach (ExposedReferenceWrapper exposedReferenceWrapper in this.spritesVisibleOnWhitescreen)
			{
				GameObject gameObject = exposedReferenceWrapper.exposedReference.Resolve(graph.GetResolver());
				if (gameObject == null)
				{
					Debug.LogError("Failed to resolve reference for WhiteScreenEffectClip");
				}
				else
				{
					SpriteRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SpriteRenderer>();
					if (componentsInChildren == null || componentsInChildren.Length < 1)
					{
						Debug.LogError("Object '" + gameObject.name + "' contains no SpriteRender's in it!");
					}
					foreach (SpriteRenderer spriteRenderer in componentsInChildren)
					{
						if (spriteRenderer.sortingLayerName == "Dynamic Perspective" && spriteRenderer.gameObject.activeSelf)
						{
							this.sprites.Add(spriteRenderer);
						}
					}
				}
			}
			CutsceneBase cutsceneBase = this.playerProvider.Resolve(graph.GetResolver());
			if (cutsceneBase != null && cutsceneBase.GetPlayer() != null)
			{
				SpriteRenderer component = cutsceneBase.GetPlayer().transform.Find("SpriteProxy/PlayerSprite").GetComponent<SpriteRenderer>();
				this.sprites.Add(component);
			}
		}

		// Token: 0x06001B42 RID: 6978 RVA: 0x000563A0 File Offset: 0x000545A0
		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			this.UnexposeSpriteRenderers(director.playableGraph);
			foreach (SpriteRenderer spriteRenderer in this.sprites)
			{
				driver.AddFromName<SpriteRenderer>(spriteRenderer.gameObject, "m_SortingLayer");
				driver.AddFromName<SpriteRenderer>(spriteRenderer.gameObject, "m_SortingOrder");
			}
		}

		// Token: 0x06001B43 RID: 6979 RVA: 0x0005641C File Offset: 0x0005461C
		public void OnBeforeSerialize()
		{
		}

		// Token: 0x06001B44 RID: 6980 RVA: 0x00056420 File Offset: 0x00054620
		public void OnAfterDeserialize()
		{
			for (int i = 0; i < this.spritesVisibleOnWhitescreen.Count; i++)
			{
				ExposedReferenceWrapper exposedReferenceWrapper = this.spritesVisibleOnWhitescreen[i];
				int num = this.spritesVisibleOnWhitescreen.Count - 1;
				while (num >= 0 && num > i)
				{
					if (object.Equals(exposedReferenceWrapper.exposedReference, this.spritesVisibleOnWhitescreen[num].exposedReference))
					{
						this.spritesVisibleOnWhitescreen[num] = new ExposedReferenceWrapper
						{
							exposedReference = default(ExposedReference<GameObject>)
						};
					}
					num--;
				}
			}
		}

		// Token: 0x04000F58 RID: 3928
		public ExposedReference<CutsceneBase> playerProvider;

		// Token: 0x04000F59 RID: 3929
		public List<ExposedReferenceWrapper> spritesVisibleOnWhitescreen = new List<ExposedReferenceWrapper>();

		// Token: 0x04000F5A RID: 3930
		private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
	}
}
