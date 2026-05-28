using System;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.UI;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x0200027F RID: 639
	public class LocationMapMarkerUI : MonoBehaviour, ILocationMapUIElement
	{
		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x060015DA RID: 5594 RVA: 0x00045A61 File Offset: 0x00043C61
		// (set) Token: 0x060015DB RID: 5595 RVA: 0x00045A69 File Offset: 0x00043C69
		public Graphic MainRenderer
		{
			get
			{
				return this._mainRenderer;
			}
			set
			{
				this._mainRenderer = value;
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x060015DC RID: 5596 RVA: 0x00045A72 File Offset: 0x00043C72
		// (set) Token: 0x060015DD RID: 5597 RVA: 0x00045A7A File Offset: 0x00043C7A
		public bool FollowObjectOrientation
		{
			get
			{
				return this._followObjectOrientation;
			}
			set
			{
				this._followObjectOrientation = value;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x060015DE RID: 5598 RVA: 0x00045A83 File Offset: 0x00043C83
		// (set) Token: 0x060015DF RID: 5599 RVA: 0x00045A8B File Offset: 0x00043C8B
		public float OrientationOffset
		{
			get
			{
				return this._orientationOffset;
			}
			set
			{
				this._orientationOffset = value;
			}
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x060015E0 RID: 5600 RVA: 0x00045A94 File Offset: 0x00043C94
		// (set) Token: 0x060015E1 RID: 5601 RVA: 0x00045A9C File Offset: 0x00043C9C
		public bool AllowAutoDestroying
		{
			get
			{
				return this._allowAutoDestroying;
			}
			set
			{
				this._allowAutoDestroying = value;
			}
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x060015E2 RID: 5602 RVA: 0x00045AA5 File Offset: 0x00043CA5
		// (set) Token: 0x060015E3 RID: 5603 RVA: 0x00045AAD File Offset: 0x00043CAD
		public ILocationObject TargetLocationObject { get; set; }

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x060015E4 RID: 5604 RVA: 0x00045AB6 File Offset: 0x00043CB6
		// (set) Token: 0x060015E5 RID: 5605 RVA: 0x00045ABE File Offset: 0x00043CBE
		public LocationMapUI CurrentMapUI { get; set; }

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x060015E6 RID: 5606 RVA: 0x00045AC7 File Offset: 0x00043CC7
		// (set) Token: 0x060015E7 RID: 5607 RVA: 0x00045ACF File Offset: 0x00043CCF
		public RectTransform CurrentLayer { get; set; }

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x060015E8 RID: 5608 RVA: 0x00045AD8 File Offset: 0x00043CD8
		public RectTransform Transform
		{
			get
			{
				return this._transform;
			}
		}

		// Token: 0x060015E9 RID: 5609 RVA: 0x00045AE0 File Offset: 0x00043CE0
		protected virtual void Awake()
		{
			this._transform = (base.transform as RectTransform);
			LocationMapUI.SetDefaultAnchorsAndPivot(this._transform);
			this._transform.pivot = new Vector2(0.5f, 0.5f);
		}

		// Token: 0x060015EA RID: 5610 RVA: 0x00045B18 File Offset: 0x00043D18
		public void UpdateMarker(Vector2? projectionAxis = null)
		{
			if (this.TargetLocationObject.IsNull())
			{
				if (this._allowAutoDestroying)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
				return;
			}
			if (this.currentLocationChunk != this.TargetLocationObject.CurrentLocationChunk || this.normalizedRect == null)
			{
				this.currentLocationChunk = this.TargetLocationObject.CurrentLocationChunk;
				this.normalizedRect = new Rect?(this.currentLocationChunk.GetNormalizedBoundsRect());
			}
			Vector2 vector = Rect.PointToNormalized(this.CurrentMapUI.WorldLocationRect, this.TargetLocationObject.Position);
			if (projectionAxis != null && this.normalizedRect != null)
			{
				if (projectionAxis.Value.x != 0f)
				{
					vector.y = this.normalizedRect.Value.center.y;
				}
				if (projectionAxis.Value.y != 0f)
				{
					vector.x = this.normalizedRect.Value.center.x;
				}
			}
			this._transform.anchoredPosition = LocationMapUI.ScaleByLayer(this.CurrentLayer, vector);
			if (this._followObjectOrientation)
			{
				this._transform.rotation = QuaternionExtensions.Get2DRotation(this.TargetLocationObject.Orientation + this._orientationOffset);
			}
		}

		// Token: 0x060015EB RID: 5611 RVA: 0x00045C64 File Offset: 0x00043E64
		public void Destroy()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x04000CA6 RID: 3238
		[SerializeField]
		private Graphic _mainRenderer;

		// Token: 0x04000CA7 RID: 3239
		[SerializeField]
		private bool _followObjectOrientation;

		// Token: 0x04000CA8 RID: 3240
		[SerializeField]
		private float _orientationOffset;

		// Token: 0x04000CA9 RID: 3241
		[SerializeField]
		private bool _allowAutoDestroying = true;

		// Token: 0x04000CAA RID: 3242
		private RectTransform _transform;

		// Token: 0x04000CAB RID: 3243
		private Rect? normalizedRect;

		// Token: 0x04000CAC RID: 3244
		private ILocationChunk currentLocationChunk;
	}
}
