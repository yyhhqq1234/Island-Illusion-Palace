using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x02000281 RID: 641
	public sealed class LocationMapUI : MonoBehaviour
	{
		// Token: 0x0600160D RID: 5645 RVA: 0x00046CEA File Offset: 0x00044EEA
		private static bool InvalidMarkersSelector(LocationMapMarkerUI marker)
		{
			return marker.IsNull();
		}

		// Token: 0x0600160E RID: 5646 RVA: 0x00046CF4 File Offset: 0x00044EF4
		public static void SetDefaultAnchorsAndPivot(RectTransform targetTransform)
		{
			Vector2 vector = new Vector2(0f, 0f);
			targetTransform.pivot = vector;
			targetTransform.anchorMin = vector;
			targetTransform.anchorMax = vector;
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x00046D28 File Offset: 0x00044F28
		public static Vector2 ScaleByLayer(RectTransform layer, Vector2 vector)
		{
			return new Vector2
			{
				x = vector.x * layer.rect.size.x,
				y = vector.y * layer.rect.size.y
			};
		}

		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x06001610 RID: 5648 RVA: 0x00046D80 File Offset: 0x00044F80
		// (set) Token: 0x06001611 RID: 5649 RVA: 0x00046D88 File Offset: 0x00044F88
		public RectTransform MainLayer
		{
			get
			{
				return this._mainLayer;
			}
			set
			{
				this._mainLayer = value;
			}
		}

		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06001612 RID: 5650 RVA: 0x00046D91 File Offset: 0x00044F91
		// (set) Token: 0x06001613 RID: 5651 RVA: 0x00046D99 File Offset: 0x00044F99
		public RectTransform MarkersLayer
		{
			get
			{
				return this._markersLayer;
			}
			set
			{
				this._markersLayer = value;
			}
		}

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06001614 RID: 5652 RVA: 0x00046DA2 File Offset: 0x00044FA2
		// (set) Token: 0x06001615 RID: 5653 RVA: 0x00046DAA File Offset: 0x00044FAA
		public bool IgnoreChunkExplorationState
		{
			get
			{
				return this._ignoreChunkExplorationState;
			}
			set
			{
				this._ignoreChunkExplorationState = value;
			}
		}

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06001616 RID: 5654 RVA: 0x00046DB3 File Offset: 0x00044FB3
		// (set) Token: 0x06001617 RID: 5655 RVA: 0x00046DBB File Offset: 0x00044FBB
		public LocationMapUI.LayoutType CurrentLayoutType
		{
			get
			{
				return this._currentLayoutType;
			}
			set
			{
				this.SetLayout(value, false);
			}
		}

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x06001618 RID: 5656 RVA: 0x00046DC5 File Offset: 0x00044FC5
		// (set) Token: 0x06001619 RID: 5657 RVA: 0x00046DCD File Offset: 0x00044FCD
		public IGameLocation TargetLocation { get; set; }

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x0600161A RID: 5658 RVA: 0x00046DD6 File Offset: 0x00044FD6
		// (set) Token: 0x0600161B RID: 5659 RVA: 0x00046DDE File Offset: 0x00044FDE
		public ILocationObject MainFocusObject
		{
			get
			{
				return this._mainFocusObject;
			}
			set
			{
				if (this._mainFocusObject != value)
				{
					if (!this._mainFocusObject.IsNull())
					{
						this.RemoveLocationMarker(this._mainFocusObject);
					}
					this.AddLocationMarker(value, true);
					this._mainFocusObject = value;
				}
			}
		}

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x0600161C RID: 5660 RVA: 0x00046E12 File Offset: 0x00045012
		// (set) Token: 0x0600161D RID: 5661 RVA: 0x00046E1A File Offset: 0x0004501A
		public Func<ILocationChunk, GameObject> CustomChunkElementPrefabProvider { get; set; }

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x0600161E RID: 5662 RVA: 0x00046E23 File Offset: 0x00045023
		// (set) Token: 0x0600161F RID: 5663 RVA: 0x00046E2B File Offset: 0x0004502B
		public Func<ILocationObject, GameObject> CustomMarkerElementPrefabProvider { get; set; }

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x06001620 RID: 5664 RVA: 0x00046E34 File Offset: 0x00045034
		// (set) Token: 0x06001621 RID: 5665 RVA: 0x00046E3C File Offset: 0x0004503C
		public Predicate<ILocationObject> VisibleMarkersFilter { get; set; }

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x06001622 RID: 5666 RVA: 0x00046E45 File Offset: 0x00045045
		public RectTransform Transform
		{
			get
			{
				return this._transform;
			}
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06001623 RID: 5667 RVA: 0x00046E4D File Offset: 0x0004504D
		public Rect WorldLocationRect
		{
			get
			{
				return this._worldLocationRect;
			}
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x00046E58 File Offset: 0x00045058
		private void ApplyWorldSize(RectTransform layer)
		{
			if (layer.IsNull())
			{
				return;
			}
			layer.sizeDelta = this._worldLocationRect.size * this._initialMapScale;
			layer.sizeDelta = this._fullLayout.MapViewportSize;
			layer.anchoredPosition = Vector2.zero;
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x00046EA6 File Offset: 0x000450A6
		private void AddToLayer(ILocationMapUIElement element, RectTransform layer)
		{
			if (layer.IsNull())
			{
				throw new UnassignedReferenceException("Target Layer is unassigned.");
			}
			element.CurrentLayer = layer;
			element.Transform.SetParent(layer, false);
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x00046ECF File Offset: 0x000450CF
		private void SetLayout(LocationMapUI.LayoutType newLayoutType, bool force)
		{
			if (force || this._currentLayoutType != newLayoutType)
			{
				this.currentLayout = ((newLayoutType == LocationMapUI.LayoutType.Minimized) ? this._minimizedLayout : this._fullLayout);
				this.currentLayout.Apply();
				this._currentLayoutType = newLayoutType;
			}
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x00046F08 File Offset: 0x00045108
		private TElement CreateMapElement<TElement>(GameObject prefab) where TElement : class, ILocationMapUIElement
		{
			if (prefab != null)
			{
				TElement componentOrDestroy = UnityEngine.Object.Instantiate<GameObject>(prefab).GetComponentOrDestroy<TElement>();
				if (!componentOrDestroy.IsNull())
				{
					componentOrDestroy.CurrentMapUI = this;
					return componentOrDestroy;
				}
			}
			return default(TElement);
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x00046F50 File Offset: 0x00045150
		private void AddChunkElement(ILocationChunk targetChunk)
		{
			if (targetChunk.IsNull())
			{
				return;
			}
			Func<ILocationChunk, GameObject> customChunkElementPrefabProvider = this.CustomChunkElementPrefabProvider;
			LocationMapChunkUI locationMapChunkUI = this.CreateMapElement<LocationMapChunkUI>(((customChunkElementPrefabProvider != null) ? customChunkElementPrefabProvider(targetChunk) : null) ?? this.defaultChunkElementPrefab);
			if (locationMapChunkUI == null)
			{
				return;
			}
			RectTransform transform = locationMapChunkUI.Transform;
			Rect rect = targetChunk.GetNormalizedBoundsRect();
			if (targetChunk.IsConnectingChunk)
			{
				if (targetChunk.Gateways == null || targetChunk.Gateways.Count != 2)
				{
					Debug.LogError(string.Format("{0} has {1} gateways!", (targetChunk as LocationChunk).name, targetChunk.Gateways.Count));
				}
				else
				{
					ILocationChunkGateway gateway = targetChunk.Gateways[0];
					float num = this.corridorWidth * 0.5f;
					float num2 = num * this._worldLocationAspect;
					if (gateway.IsHorizontalGateway())
					{
						rect = Rect.MinMaxRect(rect.xMin, rect.center.y - num2, rect.xMax, rect.center.y + num2);
					}
					else
					{
						rect = Rect.MinMaxRect(rect.center.x - num, rect.yMin, rect.center.x + num, rect.yMax);
					}
				}
			}
			this.AddToLayer(locationMapChunkUI, this._mainLayer);
			transform.sizeDelta = LocationMapUI.ScaleByLayer(this._mainLayer, rect.size);
			transform.anchoredPosition = LocationMapUI.ScaleByLayer(this._mainLayer, rect.position);
			locationMapChunkUI.IgnoreChunkExplorationState = this._ignoreChunkExplorationState;
			locationMapChunkUI.TargetChunk = targetChunk;
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x000470CC File Offset: 0x000452CC
		private void AddLocationMarker(ILocationObject targetObject, GameObject defaultPrefab, bool isDynamic)
		{
			if (targetObject.IsNull() || (this.VisibleMarkersFilter != null && !this.VisibleMarkersFilter(targetObject)))
			{
				return;
			}
			GameObject prefab = (this.CustomMarkerElementPrefabProvider != null) ? this.CustomMarkerElementPrefabProvider(targetObject) : defaultPrefab;
			LocationMapMarkerUI locationMapMarkerUI = this.CreateMapElement<LocationMapMarkerUI>(prefab);
			if (locationMapMarkerUI == null)
			{
				return;
			}
			locationMapMarkerUI.TargetLocationObject = targetObject;
			locationMapMarkerUI.name = targetObject.GetType().ToString() + "_marker";
			this.AddToLayer(locationMapMarkerUI, this._markersLayer);
			if (isDynamic)
			{
				this.dynamicMarkers.Add(locationMapMarkerUI);
			}
			else
			{
				this.staticMarkers.Add(locationMapMarkerUI);
			}
			locationMapMarkerUI.UpdateMarker(this.GetProjectionAxis(targetObject));
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x0004717C File Offset: 0x0004537C
		private bool RemoveLocationMarker(List<LocationMapMarkerUI> targetList, ILocationObject locationObject)
		{
			for (int i = targetList.Count - 1; i >= 0; i--)
			{
				if (targetList[i].TargetLocationObject == locationObject)
				{
					targetList[i].Destroy();
					targetList.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x000471C1 File Offset: 0x000453C1
		public void AddLocationMarker(ILocationObject targetObject, bool isDynamic)
		{
			this.AddLocationMarker(targetObject, this.defaultLocationMarkerPrefab, isDynamic);
		}

		// Token: 0x0600162C RID: 5676 RVA: 0x000471D1 File Offset: 0x000453D1
		public void AddLocationChunkGatewayMarker(ILocationChunkGateway targetGateway)
		{
			this.AddLocationMarker(targetGateway, this.defaultGatewayMarkerPrefab, false);
		}

		// Token: 0x0600162D RID: 5677 RVA: 0x000471E1 File Offset: 0x000453E1
		public bool RemoveLocationMarker(ILocationObject locationObject)
		{
			return this.RemoveLocationMarker(this.dynamicMarkers, locationObject) || this.RemoveLocationMarker(this.staticMarkers, locationObject);
		}

		// Token: 0x0600162E RID: 5678 RVA: 0x00047201 File Offset: 0x00045401
		public void SwitchLayout()
		{
			this.CurrentLayoutType = (this._currentLayoutType + 1) % (LocationMapUI.LayoutType)2;
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x00047213 File Offset: 0x00045413
		private void Awake()
		{
			this._transform = (base.transform as RectTransform);
		}

		// Token: 0x06001630 RID: 5680 RVA: 0x00047228 File Offset: 0x00045428
		private void Start()
		{
			if (this.TargetLocation == null)
			{
				return;
			}
			this._worldLocationRect = this.TargetLocation.GetBoundsRect();
			this._worldLocationAspect = this._worldLocationRect.size.x / this._worldLocationRect.size.y;
			this._fullLayout.UpdateViewportSize(this._worldLocationAspect);
			this._minimizedLayout.UpdateViewportSize(1f);
			RectTransform mainLayer = this._mainLayer;
			this.layersRoot = (((mainLayer != null) ? mainLayer.transform.parent : null) as RectTransform);
			this.ApplyWorldSize(this._mainLayer);
			this.ApplyWorldSize(this._markersLayer);
			foreach (ILocationChunk targetChunk in this.TargetLocation.Chunks)
			{
				this.AddChunkElement(targetChunk);
			}
			this._minimizedLayout.Initialize(this);
			this._fullLayout.Initialize(this);
			this.SetLayout(this._initialLayoutType, true);
		}

		// Token: 0x06001631 RID: 5681 RVA: 0x0004733C File Offset: 0x0004553C
		private void LateUpdate()
		{
			int count = this.dynamicMarkers.Count;
			if (count != 0)
			{
				bool flag = false;
				for (int i = 0; i < count; i++)
				{
					if (this.dynamicMarkers[i].IsNull())
					{
						flag = true;
					}
					else
					{
						this.dynamicMarkers[i].UpdateMarker(this.GetProjectionAxis(this.dynamicMarkers[i].TargetLocationObject));
					}
				}
				if (flag)
				{
					this.dynamicMarkers.RemoveAll(new Predicate<LocationMapMarkerUI>(LocationMapUI.InvalidMarkersSelector));
				}
			}
			LocationMapUI.MapLayout mapLayout = this.currentLayout;
			if (mapLayout == null)
			{
				return;
			}
			mapLayout.UpdateLayout();
		}

		// Token: 0x06001632 RID: 5682 RVA: 0x000473D0 File Offset: 0x000455D0
		private Vector2? GetProjectionAxis(ILocationObject locationObject)
		{
			Vector2? result = null;
			ILocationChunk currentLocationChunk = locationObject.CurrentLocationChunk;
			if (currentLocationChunk.IsConnectingChunk && currentLocationChunk.Gateways != null && currentLocationChunk.Gateways.Count > 0)
			{
				result = new Vector2?(currentLocationChunk.Gateways[0].TransitionDirection);
			}
			return result;
		}

		// Token: 0x06001633 RID: 5683 RVA: 0x00047423 File Offset: 0x00045623
		private void OnDestroy()
		{
			this.CustomChunkElementPrefabProvider = null;
			this.CustomMarkerElementPrefabProvider = null;
		}

		// Token: 0x04000CC1 RID: 3265
		public GameObject defaultChunkElementPrefab;

		// Token: 0x04000CC2 RID: 3266
		public GameObject defaultGatewayMarkerPrefab;

		// Token: 0x04000CC3 RID: 3267
		public GameObject defaultLocationMarkerPrefab;

		// Token: 0x04000CC4 RID: 3268
		public float corridorWidth = 0.02f;

		// Token: 0x04000CC5 RID: 3269
		[SerializeField]
		private float _initialMapScale = 2f;

		// Token: 0x04000CC6 RID: 3270
		[SerializeField]
		private RectTransform _mainLayer;

		// Token: 0x04000CC7 RID: 3271
		[SerializeField]
		private RectTransform _markersLayer;

		// Token: 0x04000CC8 RID: 3272
		[SerializeField]
		private LocationMapUI.MapLayout _minimizedLayout;

		// Token: 0x04000CC9 RID: 3273
		[SerializeField]
		private LocationMapUI.MapLayout _fullLayout;

		// Token: 0x04000CCA RID: 3274
		[SerializeField]
		private LocationMapUI.LayoutType _initialLayoutType;

		// Token: 0x04000CCB RID: 3275
		[SerializeField]
		private bool _ignoreChunkExplorationState;

		// Token: 0x04000CCC RID: 3276
		private RectTransform _transform;

		// Token: 0x04000CCD RID: 3277
		private Rect _worldLocationRect;

		// Token: 0x04000CCE RID: 3278
		private float _worldLocationAspect;

		// Token: 0x04000CCF RID: 3279
		private RectTransform layersRoot;

		// Token: 0x04000CD0 RID: 3280
		private LocationMapUI.LayoutType _currentLayoutType;

		// Token: 0x04000CD1 RID: 3281
		private LocationMapUI.MapLayout currentLayout;

		// Token: 0x04000CD2 RID: 3282
		private ILocationObject _mainFocusObject;

		// Token: 0x04000CD3 RID: 3283
		private readonly List<LocationMapMarkerUI> staticMarkers = new List<LocationMapMarkerUI>();

		// Token: 0x04000CD4 RID: 3284
		private readonly List<LocationMapMarkerUI> dynamicMarkers = new List<LocationMapMarkerUI>();

		// Token: 0x02000506 RID: 1286
		[Serializable]
		public sealed class MapLayout
		{
			// Token: 0x170007B6 RID: 1974
			// (get) Token: 0x060025FA RID: 9722 RVA: 0x00076A07 File Offset: 0x00074C07
			public Vector2 MapViewportSize
			{
				get
				{
					if (this.mapViewportSize != null)
					{
						return this.mapViewportSize.Value;
					}
					return this.mapUI.Transform.sizeDelta;
				}
			}

			// Token: 0x060025FB RID: 9723 RVA: 0x00076A34 File Offset: 0x00074C34
			public void Initialize(LocationMapUI mapUI)
			{
				if (this.copyValuesFromInitialMapState)
				{
					this.maxViewportSize = mapUI.Transform.rect.size;
					this.mapPivot = mapUI.Transform.pivot;
					this.mapPosition = mapUI.Transform.anchoredPosition;
				}
				this.mapUI = mapUI;
			}

			// Token: 0x060025FC RID: 9724 RVA: 0x00076A8C File Offset: 0x00074C8C
			public void Apply()
			{
				this.mapUI.Transform.pivot = this.mapPivot;
				this.mapUI.Transform.anchorMin = this.mapPivot;
				this.mapUI.Transform.anchorMax = this.mapPivot;
				if (this.mapViewportSize != null)
				{
					this.mapUI.Transform.sizeDelta = this.mapViewportSize.Value;
				}
				this.mapUI.Transform.anchoredPosition = this.mapPosition;
				this.mapUI.layersRoot.localScale = new Vector3(this.mapContentZoom, this.mapContentZoom, this.mapContentZoom);
				foreach (object obj in this.mapUI.layersRoot)
				{
					((RectTransform)obj).anchoredPosition = default(Vector2);
				}
			}

			// Token: 0x060025FD RID: 9725 RVA: 0x00076B98 File Offset: 0x00074D98
			public void UpdateLayout()
			{
				if (this.centerMapByFocusObject && !this.mapUI.MainFocusObject.IsNull())
				{
					Vector2 b = Rect.PointToNormalized(this.mapUI._worldLocationRect, this.mapUI.MainFocusObject.Position);
					Vector2 anchoredPosition = LocationMapUI.ScaleByLayer(this.mapUI.MainLayer, new Vector2
					{
						x = 0.5f,
						y = 0.5f
					} - b);
					this.mapUI.MainLayer.anchoredPosition = anchoredPosition;
					this.mapUI.MarkersLayer.anchoredPosition = anchoredPosition;
				}
			}

			// Token: 0x060025FE RID: 9726 RVA: 0x00076C40 File Offset: 0x00074E40
			public void UpdateViewportSize(float ratio)
			{
				if (ratio >= 1f)
				{
					this.mapViewportSize = new Vector2?(new Vector2(this.maxViewportSize.x, this.maxViewportSize.x / ratio));
				}
				else
				{
					this.mapViewportSize = new Vector2?(new Vector2(this.maxViewportSize.y * ratio, this.maxViewportSize.y));
				}
				if (this.mapViewportSize.Value.x > this.maxViewportSize.x)
				{
					float num = this.maxViewportSize.x / this.mapViewportSize.Value.x;
					this.mapViewportSize = new Vector2?(new Vector2(this.maxViewportSize.x, this.maxViewportSize.y * num));
				}
				if (this.mapViewportSize.Value.y > this.maxViewportSize.y)
				{
					float num2 = this.maxViewportSize.y / this.mapViewportSize.Value.y;
					this.mapViewportSize = new Vector2?(new Vector2(this.maxViewportSize.x * num2, this.maxViewportSize.y));
				}
			}

			// Token: 0x04001AC7 RID: 6855
			public Vector2 maxViewportSize = new Vector2(400f, 400f);

			// Token: 0x04001AC8 RID: 6856
			public Vector2 mapPivot;

			// Token: 0x04001AC9 RID: 6857
			public Vector2 mapPosition = new Vector2(30f, 30f);

			// Token: 0x04001ACA RID: 6858
			public bool copyValuesFromInitialMapState;

			// Token: 0x04001ACB RID: 6859
			[Space(5f)]
			public float mapContentZoom = 1f;

			// Token: 0x04001ACC RID: 6860
			public bool centerMapByFocusObject;

			// Token: 0x04001ACD RID: 6861
			private LocationMapUI mapUI;

			// Token: 0x04001ACE RID: 6862
			private Vector2? mapViewportSize;
		}

		// Token: 0x02000507 RID: 1287
		public enum LayoutType
		{
			// Token: 0x04001AD0 RID: 6864
			Minimized,
			// Token: 0x04001AD1 RID: 6865
			Full
		}
	}
}
