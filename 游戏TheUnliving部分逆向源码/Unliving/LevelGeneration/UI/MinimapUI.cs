using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x02000282 RID: 642
	public class MinimapUI : MonoBehaviour
	{
		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x06001635 RID: 5685 RVA: 0x00047467 File Offset: 0x00045667
		// (set) Token: 0x06001636 RID: 5686 RVA: 0x0004746F File Offset: 0x0004566F
		public MinimapUI.MapLayout CurrentLayout { get; private set; }

		// Token: 0x06001637 RID: 5687 RVA: 0x00047478 File Offset: 0x00045678
		public void Initialize(LocationMapRenderer mapRenderer)
		{
			this.mapRenderer = mapRenderer;
			this.SetLayout(this.defaultLayoutType, true);
		}

		// Token: 0x06001638 RID: 5688 RVA: 0x0004748E File Offset: 0x0004568E
		public void SwitchLayout()
		{
			this.SetLayout((this.CurrentLayout.type + 1) % (MinimapUI.LayoutType)2, false);
		}

		// Token: 0x06001639 RID: 5689 RVA: 0x000474A8 File Offset: 0x000456A8
		private void SetLayout(MinimapUI.LayoutType newLayoutType, bool force)
		{
			if (!force)
			{
				MinimapUI.MapLayout currentLayout = this.CurrentLayout;
				if (currentLayout != null && currentLayout.type == newLayoutType)
				{
					return;
				}
			}
			MinimapUI.MapLayout currentLayout2 = this.CurrentLayout;
			if (currentLayout2 != null)
			{
				currentLayout2.Deactivate();
			}
			this.CurrentLayout = ((newLayoutType == MinimapUI.LayoutType.Minimized) ? this.minimizedLayout : this.fullLayout);
			this.CurrentLayout.Activate(this.mapRenderer);
		}

		// Token: 0x04000CD6 RID: 3286
		public MinimapUI.LayoutType defaultLayoutType;

		// Token: 0x04000CD7 RID: 3287
		public MinimapUI.MapLayout minimizedLayout;

		// Token: 0x04000CD8 RID: 3288
		public MinimapUI.MapLayout fullLayout;

		// Token: 0x04000CD9 RID: 3289
		private LocationMapRenderer mapRenderer;

		// Token: 0x02000508 RID: 1288
		public enum LayoutType
		{
			// Token: 0x04001AD3 RID: 6867
			Minimized,
			// Token: 0x04001AD4 RID: 6868
			Full
		}

		// Token: 0x02000509 RID: 1289
		[Serializable]
		public class MapLayout
		{
			// Token: 0x06002600 RID: 9728 RVA: 0x00076DA8 File Offset: 0x00074FA8
			public void Activate(LocationMapRenderer mapRenderer)
			{
				if (this.defaultSizeDelta == null)
				{
					this.defaultSizeDelta = new Vector2?(this.layoutRect.sizeDelta);
				}
				this.layoutRect.gameObject.SetActive(true);
				if (this.scaleByMinimapAspect)
				{
					float num = mapRenderer.MinimapRect.width / mapRenderer.MinimapRect.height;
					this.minimapImage.texture = mapRenderer.SetRenderTextureSize((int)((float)this.renderTexSize.y * num), this.renderTexSize.y);
					if (num > 1f)
					{
						mapRenderer.minimapCamera.orthographicSize = mapRenderer.MinimapRect.height * 0.5f + 1.5f;
					}
					else
					{
						mapRenderer.minimapCamera.orthographicSize = mapRenderer.MinimapRect.width * 0.5f + 1.5f;
					}
					num = (float)Screen.width / (float)Screen.height / num;
					if (num > 1f)
					{
						float num2 = 1f / num;
						Rect rect = new Rect(Vector2.zero, new Vector2(1f / num, 1f));
						this.layoutRect.anchorMin = new Vector2((1f - num2) / 2f, rect.yMin);
						this.layoutRect.anchorMax = new Vector2(num2 + (1f - num2) / 2f, rect.yMax);
					}
					else
					{
						Rect rect = new Rect(Vector2.zero, new Vector2(1f, num));
						this.layoutRect.anchorMin = new Vector2(rect.xMin, (1f - num) / 2f);
						this.layoutRect.anchorMax = new Vector2(rect.xMax, num + (1f - num) / 2f);
					}
					this.layoutRect.sizeDelta = Vector2.zero;
					this.layoutRect.pivot = new Vector2(0.5f, 0.5f);
				}
				else
				{
					this.minimapImage.texture = mapRenderer.SetRenderTextureSize(this.renderTexSize.x, this.renderTexSize.y);
					mapRenderer.minimapCamera.orthographicSize = this.cameraOrthographicSize;
				}
				mapRenderer.CenterMapByFocusObject = this.centerMapByFocusObject;
			}

			// Token: 0x06002601 RID: 9729 RVA: 0x00076FEC File Offset: 0x000751EC
			public void Deactivate()
			{
				this.layoutRect.gameObject.SetActive(false);
			}

			// Token: 0x04001AD5 RID: 6869
			public RectTransform layoutRect;

			// Token: 0x04001AD6 RID: 6870
			public RawImage minimapImage;

			// Token: 0x04001AD7 RID: 6871
			public Vector2Int renderTexSize;

			// Token: 0x04001AD8 RID: 6872
			public bool scaleByMinimapAspect;

			// Token: 0x04001AD9 RID: 6873
			public float cameraOrthographicSize = 10f;

			// Token: 0x04001ADA RID: 6874
			public bool centerMapByFocusObject;

			// Token: 0x04001ADB RID: 6875
			public MinimapUI.LayoutType type;

			// Token: 0x04001ADC RID: 6876
			private Vector2? defaultSizeDelta;
		}
	}
}
