using System;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000213 RID: 531
	public sealed class GameMobMovementPointLimiter : MonoBehaviour, IGameMobMovementPointLimiter
	{
		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06001234 RID: 4660 RVA: 0x00039310 File Offset: 0x00037510
		// (set) Token: 0x06001235 RID: 4661 RVA: 0x00039318 File Offset: 0x00037518
		public Collider2D Area
		{
			get
			{
				return this._area;
			}
			set
			{
				if (this._area != null)
				{
					return;
				}
				this._area = value;
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06001236 RID: 4662 RVA: 0x00039330 File Offset: 0x00037530
		// (set) Token: 0x06001237 RID: 4663 RVA: 0x00039338 File Offset: 0x00037538
		public bool IsActive
		{
			get
			{
				return base.enabled;
			}
			set
			{
				base.enabled = value;
			}
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x00039341 File Offset: 0x00037541
		private void Awake()
		{
			this.selfCollider = base.GetComponent<Collider2D>();
			if (this.selfCollider != null)
			{
				this.selfCollider.isTrigger = true;
			}
		}

		// Token: 0x06001239 RID: 4665 RVA: 0x0003936C File Offset: 0x0003756C
		private void Start()
		{
			if (this._area == null)
			{
				this._area = this.selfCollider;
			}
			if (this._area != null)
			{
				this._area.isTrigger = true;
				this.overlapTestLayers = 1 << this._area.gameObject.layer;
			}
		}

		// Token: 0x0600123A RID: 4666 RVA: 0x000393C8 File Offset: 0x000375C8
		public Vector2? LimitMovementPoint(Vector2 movementPoint)
		{
			if (this._area == null || !base.enabled || !this._area.enabled)
			{
				return new Vector2?(movementPoint);
			}
			if (Physics2D.OverlapPoint(movementPoint, this.overlapTestLayers) == this._area)
			{
				return new Vector2?(movementPoint);
			}
			if (this.clampPointByBoundaries)
			{
				return new Vector2?(this._area.ClosestPoint(movementPoint));
			}
			return null;
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x00039444 File Offset: 0x00037644
		private void OnDrawGizmos()
		{
			if (base.enabled && !this._area.IsNull())
			{
				PolygonCollider2D polygonCollider2D = this._area as PolygonCollider2D;
				if (polygonCollider2D != null)
				{
					Transform transform = this._area.transform;
					for (int i = 0; i < polygonCollider2D.pathCount; i++)
					{
						Vector2[] path = polygonCollider2D.GetPath(i);
						int num = path.Length;
						if (num >= 2)
						{
							for (int j = 0; j < num; j++)
							{
								int num2 = (j + 1) % num;
								Gizmos.DrawLine(transform.TransformPoint(path[j]), transform.TransformPoint(path[num2]));
							}
						}
					}
				}
			}
		}

		// Token: 0x04000A6A RID: 2666
		[SerializeField]
		private Collider2D _area;

		// Token: 0x04000A6B RID: 2667
		public bool clampPointByBoundaries;

		// Token: 0x04000A6C RID: 2668
		private Collider2D selfCollider;

		// Token: 0x04000A6D RID: 2669
		private int overlapTestLayers;
	}
}
