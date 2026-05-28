using System;
using System.Collections.Generic;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.UI;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x0200027D RID: 637
	public sealed class LocationMapChunkUI : MonoBehaviour, ILocationMapUIElement
	{
		// Token: 0x060015B3 RID: 5555 RVA: 0x00045525 File Offset: 0x00043725
		private static IEnumerable<ILocationObject> GetVisibleChunkObjects(ILocationChunk targetChunk)
		{
			foreach (ILocationObject locationObject in targetChunk.Gateways)
			{
				if (locationObject != null)
				{
					yield return locationObject;
				}
			}
			IEnumerator<ILocationChunkGateway> enumerator = null;
			foreach (ILocationObject locationObject2 in targetChunk.EnvironmentObjects)
			{
				if (locationObject2 != null)
				{
					yield return locationObject2;
				}
			}
			IEnumerator<ILocationObject> enumerator2 = null;
			yield break;
			yield break;
		}

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x060015B4 RID: 5556 RVA: 0x00045535 File Offset: 0x00043735
		// (set) Token: 0x060015B5 RID: 5557 RVA: 0x00045557 File Offset: 0x00043757
		public Graphic MainRenderer
		{
			get
			{
				if (this._mainRenderer == null)
				{
					this._mainRenderer = base.GetComponent<Image>();
				}
				return this._mainRenderer;
			}
			set
			{
				this._mainRenderer = (value as Image);
			}
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x060015B6 RID: 5558 RVA: 0x00045565 File Offset: 0x00043765
		// (set) Token: 0x060015B7 RID: 5559 RVA: 0x0004556D File Offset: 0x0004376D
		public ILocationChunk TargetChunk { get; set; }

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x060015B8 RID: 5560 RVA: 0x00045576 File Offset: 0x00043776
		// (set) Token: 0x060015B9 RID: 5561 RVA: 0x0004557E File Offset: 0x0004377E
		public bool IsAlwaysVisible
		{
			get
			{
				return this._isAlwaysVisible;
			}
			set
			{
				if (this._isAlwaysVisible != value)
				{
					this._isAlwaysVisible = value;
					this.UpdateVisibilityState();
				}
			}
		}

		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x060015BA RID: 5562 RVA: 0x00045596 File Offset: 0x00043796
		// (set) Token: 0x060015BB RID: 5563 RVA: 0x0004559E File Offset: 0x0004379E
		public bool IgnoreChunkExplorationState
		{
			get
			{
				return this._ignoreChunkExplorationState;
			}
			set
			{
				if (this._ignoreChunkExplorationState != value)
				{
					this._ignoreChunkExplorationState = value;
					this.UpdateVisibilityState();
				}
			}
		}

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x060015BC RID: 5564 RVA: 0x000455B6 File Offset: 0x000437B6
		// (set) Token: 0x060015BD RID: 5565 RVA: 0x000455BE File Offset: 0x000437BE
		public LocationMapUI CurrentMapUI { get; set; }

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x060015BE RID: 5566 RVA: 0x000455C7 File Offset: 0x000437C7
		// (set) Token: 0x060015BF RID: 5567 RVA: 0x000455CF File Offset: 0x000437CF
		public RectTransform CurrentLayer { get; set; }

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x060015C0 RID: 5568 RVA: 0x000455D8 File Offset: 0x000437D8
		public RectTransform Transform
		{
			get
			{
				return this._transform;
			}
		}

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x060015C1 RID: 5569 RVA: 0x000455E0 File Offset: 0x000437E0
		public CanvasGroup CanvasGroup
		{
			get
			{
				return this._canvasGroup;
			}
		}

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x060015C2 RID: 5570 RVA: 0x000455E8 File Offset: 0x000437E8
		public LocationMapChunkUI.State InvisibleState
		{
			get
			{
				return this._invisibleState;
			}
		}

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x060015C3 RID: 5571 RVA: 0x000455F0 File Offset: 0x000437F0
		public LocationMapChunkUI.State VisibleState
		{
			get
			{
				return this._visibleState;
			}
		}

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x060015C4 RID: 5572 RVA: 0x000455F8 File Offset: 0x000437F8
		public LocationMapChunkUI.State ExploredState
		{
			get
			{
				return this._exploredState;
			}
		}

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x060015C5 RID: 5573 RVA: 0x00045600 File Offset: 0x00043800
		public bool IsVisible
		{
			get
			{
				return this._isAlwaysVisible || this.TargetChunk.IsVisible;
			}
		}

		// Token: 0x060015C6 RID: 5574 RVA: 0x00045618 File Offset: 0x00043818
		private void SetChunkObjectsVisible(bool isVisible)
		{
			LocationMapUI currentMapUI = this.CurrentMapUI;
			foreach (ILocationObject locationObject in LocationMapChunkUI.GetVisibleChunkObjects(this.TargetChunk))
			{
				if (isVisible)
				{
					ILocationChunkGateway locationChunkGateway = locationObject as ILocationChunkGateway;
					if (locationChunkGateway != null)
					{
						currentMapUI.AddLocationChunkGatewayMarker(locationChunkGateway);
					}
					else
					{
						currentMapUI.AddLocationMarker(locationObject, false);
					}
				}
				else
				{
					currentMapUI.RemoveLocationMarker(locationObject);
				}
			}
		}

		// Token: 0x060015C7 RID: 5575 RVA: 0x00045694 File Offset: 0x00043894
		public void UpdateVisibilityState()
		{
			if (this._mainRenderer == null || this.TargetChunk == null)
			{
				return;
			}
			LocationMapChunkUI.State state;
			if (this.IsVisible)
			{
				state = this._visibleState;
			}
			else
			{
				state = ((!this._ignoreChunkExplorationState && this.TargetChunk.IsExplored) ? this._exploredState : this._invisibleState);
			}
			state.Apply(this);
		}

		// Token: 0x060015C8 RID: 5576 RVA: 0x000456F4 File Offset: 0x000438F4
		private void OnChunkVisibilityChanged(ILocationChunk chunk, bool isChunkVisible)
		{
			this.UpdateVisibilityState();
			if (!this._isAlwaysVisible)
			{
				this.SetChunkObjectsVisible(isChunkVisible);
			}
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x0004570B File Offset: 0x0004390B
		private void Awake()
		{
			this._transform = (base.transform as RectTransform);
			this._canvasGroup = base.GetComponent<CanvasGroup>();
			LocationMapUI.SetDefaultAnchorsAndPivot(this._transform);
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x00045738 File Offset: 0x00043938
		private void Start()
		{
			if (this.TargetChunk == null)
			{
				return;
			}
			this.TargetChunk.VisibilityChanged += this.OnChunkVisibilityChanged;
			this.UpdateVisibilityState();
			if (this.IsVisible)
			{
				this.SetChunkObjectsVisible(true);
			}
			this._canvasGroup.alpha = 1f;
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x0004578A File Offset: 0x0004398A
		private void OnDestroy()
		{
			if (this.TargetChunk != null)
			{
				this.TargetChunk.VisibilityChanged -= this.OnChunkVisibilityChanged;
			}
		}

		// Token: 0x04000C93 RID: 3219
		[SerializeField]
		private Image _mainRenderer;

		// Token: 0x04000C94 RID: 3220
		[SerializeField]
		private LocationMapChunkUI.State _invisibleState;

		// Token: 0x04000C95 RID: 3221
		[SerializeField]
		private LocationMapChunkUI.State _visibleState;

		// Token: 0x04000C96 RID: 3222
		[SerializeField]
		private LocationMapChunkUI.State _exploredState;

		// Token: 0x04000C97 RID: 3223
		[SerializeField]
		private bool _isAlwaysVisible;

		// Token: 0x04000C98 RID: 3224
		[SerializeField]
		private bool _ignoreChunkExplorationState;

		// Token: 0x04000C99 RID: 3225
		private RectTransform _transform;

		// Token: 0x04000C9A RID: 3226
		private CanvasGroup _canvasGroup;

		// Token: 0x02000502 RID: 1282
		[Serializable]
		public sealed class State
		{
			// Token: 0x060025E3 RID: 9699 RVA: 0x00076504 File Offset: 0x00074704
			public void Apply(LocationMapChunkUI chunkUI)
			{
				if (chunkUI.MainRenderer != null)
				{
					Image mainRenderer = chunkUI._mainRenderer;
					mainRenderer.overrideSprite = this.sprite;
					if (chunkUI.CanvasGroup == null)
					{
						Color color = mainRenderer.color;
						color.a = this.alpha;
						mainRenderer.color = color;
						return;
					}
				}
				chunkUI.CanvasGroup.alpha = this.alpha;
			}

			// Token: 0x04001AB5 RID: 6837
			public float alpha = 1f;

			// Token: 0x04001AB6 RID: 6838
			public Sprite sprite;
		}
	}
}
