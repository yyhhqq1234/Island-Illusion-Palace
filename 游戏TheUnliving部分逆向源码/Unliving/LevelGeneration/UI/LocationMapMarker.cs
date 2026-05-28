using System;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x0200027E RID: 638
	public class LocationMapMarker : MonoBehaviour, ILocationMapElement
	{
		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x060015CD RID: 5581 RVA: 0x000457B3 File Offset: 0x000439B3
		// (set) Token: 0x060015CE RID: 5582 RVA: 0x000457BB File Offset: 0x000439BB
		public bool AllowAutoDestroying
		{
			get
			{
				return this.allowAutoDestroying;
			}
			set
			{
				this.allowAutoDestroying = value;
			}
		}

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x060015CF RID: 5583 RVA: 0x000457C4 File Offset: 0x000439C4
		// (set) Token: 0x060015D0 RID: 5584 RVA: 0x000457CC File Offset: 0x000439CC
		public ILocationObject TargetLocationObject { get; set; }

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x060015D1 RID: 5585 RVA: 0x000457D5 File Offset: 0x000439D5
		public Transform Transform
		{
			get
			{
				return base.transform;
			}
		}

		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x060015D2 RID: 5586 RVA: 0x000457DD File Offset: 0x000439DD
		public Vector2 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x060015D3 RID: 5587 RVA: 0x000457EF File Offset: 0x000439EF
		// (set) Token: 0x060015D4 RID: 5588 RVA: 0x000457F7 File Offset: 0x000439F7
		public SpriteRenderer MainRenderer
		{
			get
			{
				return this.mainRenderer;
			}
			set
			{
				this.mainRenderer = value;
			}
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x060015D5 RID: 5589 RVA: 0x00045800 File Offset: 0x00043A00
		// (set) Token: 0x060015D6 RID: 5590 RVA: 0x00045808 File Offset: 0x00043A08
		public LocationMapRenderer CurrentMapRenderer { get; set; }

		// Token: 0x060015D7 RID: 5591 RVA: 0x00045814 File Offset: 0x00043A14
		public void UpdateMarker()
		{
			if (this.TargetLocationObject.IsNull())
			{
				if (this.allowAutoDestroying)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
				return;
			}
			if (this.currentLocationChunk != this.TargetLocationObject.CurrentLocationChunk)
			{
				this.currentLocationChunk = this.TargetLocationObject.CurrentLocationChunk;
				this.currentChunkMapData = this.CurrentMapRenderer.GetMapChunkData(this.currentLocationChunk);
				ILocationChunk locationChunk = this.currentLocationChunk;
				this.currentChunkRect = ((locationChunk != null) ? locationChunk.GetBoundsRect() : default(Rect));
			}
			if (this.currentLocationChunk == null)
			{
				return;
			}
			LocationObjectType value = (LocationObjectType)this.TargetLocationObject.LocationObjectType.Value;
			Vector2 vector;
			if (this.currentLocationChunk.IsConnectingChunk && value == LocationObjectType.Player)
			{
				vector = Rect.NormalizedToPoint(Rect.MinMaxRect(this.currentChunkMapData.rect.xMin, this.currentChunkMapData.rect.yMin, this.currentChunkMapData.rect.xMax + 1f, this.currentChunkMapData.rect.yMax + 1f), new Vector2(0.5f, 0.5f));
			}
			else
			{
				if (value == LocationObjectType.HorizontalLocationGateway || value == LocationObjectType.VerticalLocationGateway)
				{
					ILocationChunkGateway locationChunkGateway = this.TargetLocationObject as ILocationChunkGateway;
					if (locationChunkGateway != null)
					{
						int num = this.currentLocationChunk.Gateways.IndexOf(locationChunkGateway);
						vector = this.currentChunkMapData.gatewaysPositions[num];
						if (value == LocationObjectType.HorizontalLocationGateway)
						{
							if (locationChunkGateway.Position.x > this.currentChunkRect.center.x)
							{
								vector += Vector2.right;
								goto IL_1EC;
							}
							goto IL_1EC;
						}
						else
						{
							if (locationChunkGateway.Position.y > this.currentChunkRect.center.y)
							{
								vector += Vector2.up;
								goto IL_1EC;
							}
							goto IL_1EC;
						}
					}
				}
				Vector2 normalizedRectCoordinates = Rect.PointToNormalized(this.currentLocationChunk.GetBoundsRect(), this.TargetLocationObject.Position);
				vector = Rect.NormalizedToPoint(this.currentChunkMapData.rect, normalizedRectCoordinates);
			}
			IL_1EC:
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, this.maxMoveSpeed * Time.deltaTime);
		}

		// Token: 0x060015D8 RID: 5592 RVA: 0x00045A3A File Offset: 0x00043C3A
		public void Destroy()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x04000C9D RID: 3229
		public float maxMoveSpeed = 10f;

		// Token: 0x04000C9E RID: 3230
		[SerializeField]
		private SpriteRenderer mainRenderer;

		// Token: 0x04000C9F RID: 3231
		[SerializeField]
		private bool allowAutoDestroying = true;

		// Token: 0x04000CA0 RID: 3232
		private ILocationChunk currentLocationChunk;

		// Token: 0x04000CA1 RID: 3233
		private LocationMapRenderer.MapChunkData currentChunkMapData;

		// Token: 0x04000CA2 RID: 3234
		private Rect currentChunkRect;
	}
}
