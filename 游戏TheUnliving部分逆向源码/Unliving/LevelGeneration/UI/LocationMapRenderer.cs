using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unliving.GameScene;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x02000280 RID: 640
	public class LocationMapRenderer : GameBehaviourBase
	{
		// Token: 0x060015ED RID: 5613 RVA: 0x00045C80 File Offset: 0x00043E80
		private static bool InvalidMarkersSelector(LocationMapMarker marker)
		{
			return marker.IsNull();
		}

		// Token: 0x060015EE RID: 5614 RVA: 0x00045C88 File Offset: 0x00043E88
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

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x060015EF RID: 5615 RVA: 0x00045C98 File Offset: 0x00043E98
		// (set) Token: 0x060015F0 RID: 5616 RVA: 0x00045CA0 File Offset: 0x00043EA0
		public Rect WorldLocationRect { get; private set; } = Rect.zero;

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x060015F1 RID: 5617 RVA: 0x00045CA9 File Offset: 0x00043EA9
		// (set) Token: 0x060015F2 RID: 5618 RVA: 0x00045CB1 File Offset: 0x00043EB1
		public Rect MinimapRect { get; private set; } = Rect.zero;

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x060015F3 RID: 5619 RVA: 0x00045CBA File Offset: 0x00043EBA
		// (set) Token: 0x060015F4 RID: 5620 RVA: 0x00045CD8 File Offset: 0x00043ED8
		public ILocationObject MainFocusObject
		{
			get
			{
				if (!this.mainFocusObjectMarker.IsNull())
				{
					return this.mainFocusObjectMarker.TargetLocationObject;
				}
				return null;
			}
			set
			{
				if (this.mainFocusObjectMarker == null || this.mainFocusObjectMarker.TargetLocationObject != value)
				{
					if (!this.mainFocusObjectMarker.IsNull())
					{
						this.RemoveLocationMarker(value);
					}
					this.mainFocusObjectMarker = this.AddLocationMarker(value, true);
				}
			}
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x060015F5 RID: 5621 RVA: 0x00045D24 File Offset: 0x00043F24
		// (set) Token: 0x060015F6 RID: 5622 RVA: 0x00045D2C File Offset: 0x00043F2C
		public Func<ILocationChunk, GameObject> CustomChunkElementPrefabProvider { get; set; }

		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x060015F7 RID: 5623 RVA: 0x00045D35 File Offset: 0x00043F35
		// (set) Token: 0x060015F8 RID: 5624 RVA: 0x00045D3D File Offset: 0x00043F3D
		public Func<ILocationObject, GameObject> CustomMarkerElementPrefabProvider { get; set; }

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x060015F9 RID: 5625 RVA: 0x00045D46 File Offset: 0x00043F46
		// (set) Token: 0x060015FA RID: 5626 RVA: 0x00045D50 File Offset: 0x00043F50
		public bool CenterMapByFocusObject
		{
			get
			{
				return this.centerMapByFocusObject;
			}
			set
			{
				if (!value)
				{
					this.minimapCamera.transform.position = new Vector3(this.MinimapRect.center.x, this.MinimapRect.center.y, this.minimapCamera.transform.position.z);
				}
				this.centerMapByFocusObject = value;
			}
		}

		// Token: 0x060015FB RID: 5627 RVA: 0x00045DB7 File Offset: 0x00043FB7
		private void Start()
		{
			this.tilemap = UnityEngine.Object.Instantiate<Tilemap>(this.tilemapPrefab, base.transform);
			base.CurrentGame.Services.Get<GameSceneManager>().InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x00045DF4 File Offset: 0x00043FF4
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
						this.dynamicMarkers[i].UpdateMarker();
					}
				}
				if (flag)
				{
					this.dynamicMarkers.RemoveAll(new Predicate<LocationMapMarker>(LocationMapRenderer.InvalidMarkersSelector));
				}
			}
			if (this.centerMapByFocusObject && !this.mainFocusObjectMarker.IsNull())
			{
				this.minimapCamera.transform.position = new Vector3(this.mainFocusObjectMarker.Position.x, this.mainFocusObjectMarker.Position.y, this.minimapCamera.transform.position.z);
			}
		}

		// Token: 0x060015FD RID: 5629 RVA: 0x00045EC0 File Offset: 0x000440C0
		public LocationMapRenderer.MapChunkData GetMapChunkData(ILocationChunk chunk)
		{
			LocationMapRenderer.MapChunkData result;
			if (chunk != null && this.chunkRectsDict.TryGetValue(chunk, out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x060015FE RID: 5630 RVA: 0x00045EE4 File Offset: 0x000440E4
		public RenderTexture SetRenderTextureSize(int width, int height)
		{
			return this.minimapCamera.targetTexture = new RenderTexture(width, height, this.minimapCamera.targetTexture.depth);
		}

		// Token: 0x060015FF RID: 5631 RVA: 0x00045F16 File Offset: 0x00044116
		private static Vector2 FloorToInt(Vector2 v)
		{
			return new Vector2((float)((int)v.x), (float)((int)v.y));
		}

		// Token: 0x06001600 RID: 5632 RVA: 0x00045F2D File Offset: 0x0004412D
		private void SetTile(Vector2 cell)
		{
			this.SetTile(new Vector2Int((int)cell.x, (int)cell.y));
		}

		// Token: 0x06001601 RID: 5633 RVA: 0x00045F48 File Offset: 0x00044148
		private void SetTile(Vector2Int cell)
		{
			this.tilemap.SetTile((Vector3Int)cell, this.ruletile);
		}

		// Token: 0x06001602 RID: 5634 RVA: 0x00045F64 File Offset: 0x00044164
		private void AddChunkRect(ILocationChunk chunk, LocationMapRenderer.MapChunkData data)
		{
			this.CheckMinMax(ref data, chunk);
			this.chunkRectsDict.Add(chunk, data);
			Rect rect = data.rect;
			this.MinimapRect = Rect.MinMaxRect(Mathf.Min(rect.xMin, this.MinimapRect.xMin), Mathf.Min(rect.yMin, this.MinimapRect.yMin), Mathf.Max(rect.xMax, this.MinimapRect.xMax), Mathf.Max(rect.yMax, this.MinimapRect.yMax));
		}

		// Token: 0x06001603 RID: 5635 RVA: 0x00046004 File Offset: 0x00044204
		public LocationMapMarker AddLocationMarker(ILocationObject targetObject, bool isDynamic)
		{
			if (targetObject.IsNull())
			{
				return null;
			}
			int? locationObjectType = targetObject.LocationObjectType;
			LocationObjectType? locationObjectType2 = (locationObjectType != null) ? new LocationObjectType?((LocationObjectType)locationObjectType.GetValueOrDefault()) : null;
			LocationObjectType? locationObjectType3 = locationObjectType2;
			LocationObjectType locationObjectType4 = LocationObjectType.HorizontalLocationGateway;
			if (!(locationObjectType3.GetValueOrDefault() == locationObjectType4 & locationObjectType3 != null))
			{
				locationObjectType3 = locationObjectType2;
				locationObjectType4 = LocationObjectType.VerticalLocationGateway;
				if (!(locationObjectType3.GetValueOrDefault() == locationObjectType4 & locationObjectType3 != null))
				{
					goto IL_78;
				}
			}
			if (!targetObject.CurrentLocationChunk.IsConnectingChunk)
			{
				return null;
			}
			IL_78:
			GameObject gameObject = this.CustomMarkerElementPrefabProvider(targetObject);
			if (gameObject.IsNull())
			{
				return null;
			}
			LocationMapMarker locationMapMarker = this.CreateMapElement<LocationMapMarker>(gameObject);
			if (locationMapMarker == null)
			{
				return null;
			}
			locationMapMarker.TargetLocationObject = targetObject;
			locationMapMarker.name = targetObject.GetType().ToString() + "_marker";
			if (isDynamic)
			{
				this.dynamicMarkers.Add(locationMapMarker);
			}
			else
			{
				locationMapMarker.maxMoveSpeed = float.MaxValue;
				this.staticMarkers.Add(locationMapMarker);
			}
			locationMapMarker.UpdateMarker();
			return locationMapMarker;
		}

		// Token: 0x06001604 RID: 5636 RVA: 0x00046104 File Offset: 0x00044304
		public bool RemoveLocationMarker(ILocationObject locationObject)
		{
			return this.RemoveLocationMarker(this.dynamicMarkers, locationObject) || this.RemoveLocationMarker(this.staticMarkers, locationObject);
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x00046124 File Offset: 0x00044324
		private bool RemoveLocationMarker(List<LocationMapMarker> targetList, ILocationObject locationObject)
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

		// Token: 0x06001606 RID: 5638 RVA: 0x0004616C File Offset: 0x0004436C
		private TElement CreateMapElement<TElement>(GameObject prefab) where TElement : class, ILocationMapElement
		{
			if (prefab != null)
			{
				TElement componentOrDestroy = UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform).GetComponentOrDestroy<TElement>();
				if (!componentOrDestroy.IsNull())
				{
					componentOrDestroy.CurrentMapRenderer = this;
					return componentOrDestroy;
				}
			}
			return default(TElement);
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x000461B8 File Offset: 0x000443B8
		private void CheckMinMax(ref LocationMapRenderer.MapChunkData data, ILocationChunk chunk)
		{
			foreach (ILocationChunkGateway locationChunkGateway in chunk.Gateways)
			{
				ILocationChunk nextChunk = locationChunkGateway.GetNextChunk();
				LocationMapRenderer.MapChunkData mapChunkData;
				if (nextChunk != null && this.chunkRectsDict.TryGetValue(nextChunk, out mapChunkData))
				{
					Rect rect = data.rect;
					Rect rect2 = mapChunkData.rect;
					Vector2Int zero = Vector2Int.zero;
					if (locationChunkGateway.IsHorizontalGateway())
					{
						if (locationChunkGateway.TransitionDirection.x > 0f)
						{
							zero.x += (int)rect.xMax + 1 - (int)rect2.xMin;
						}
						else
						{
							zero.x += (int)rect.xMin - 1 - (int)rect2.xMax;
						}
					}
					else if (locationChunkGateway.TransitionDirection.y > 0f)
					{
						zero.y += (int)rect.yMax + 1 - (int)rect2.yMin;
					}
					else
					{
						zero.y += (int)rect.yMin - 1 - (int)rect2.yMax;
					}
					this.tilemapOffset -= zero;
					data.rect.Set(rect.x - (float)zero.x, rect.y - (float)zero.y, rect.size.x, rect.size.y);
					if (data.gatewaysPositions != null)
					{
						for (int i = 0; i < data.gatewaysPositions.Length; i++)
						{
							data.gatewaysPositions[i] -= zero;
						}
					}
				}
			}
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x00046398 File Offset: 0x00044598
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			this.WorldLocationRect = sceneManager.GeneratedLocation.GetBoundsRect();
			foreach (ILocationChunk locationChunk in sceneManager.GeneratedLocation.Chunks)
			{
				LocationChunk locationChunk2 = locationChunk as LocationChunk;
				if (locationChunk2 != null)
				{
					this.UpdateChunkRect(locationChunk);
					locationChunk.VisitorAdded += this.OnChunkVisitorAdded;
					if (locationChunk2.IsVisible)
					{
						this.OnLocationChunkVisibilityChanged(locationChunk2, true);
					}
					else
					{
						locationChunk2.VisibilityChanged += this.OnLocationChunkVisibilityChanged;
					}
				}
			}
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x0004643C File Offset: 0x0004463C
		private void OnChunkVisitorAdded(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			if (visitor == this.MainFocusObject)
			{
				return;
			}
			int? locationObjectType = visitor.LocationObjectType;
			int num = 1;
			if (!(locationObjectType.GetValueOrDefault() == num & locationObjectType != null))
			{
				locationObjectType = visitor.LocationObjectType;
				num = 2;
				if (!(locationObjectType.GetValueOrDefault() == num & locationObjectType != null))
				{
					this.AddLocationMarker(visitor, visitor.IsDynamicLocationObject);
					return;
				}
			}
		}

		// Token: 0x0600160A RID: 5642 RVA: 0x0004649C File Offset: 0x0004469C
		private void OnLocationChunkVisibilityChanged(ILocationChunk chunk, bool visible)
		{
			foreach (ILocationObject locationObject in LocationMapRenderer.GetVisibleChunkObjects(chunk))
			{
				if (visible)
				{
					this.AddLocationMarker(locationObject, false);
				}
				else
				{
					this.RemoveLocationMarker(locationObject);
				}
			}
			if (!visible)
			{
				return;
			}
			LocationMapRenderer.MapChunkData mapChunkData;
			if (!this.chunkRectsDict.TryGetValue(chunk, out mapChunkData))
			{
				return;
			}
			Rect rect = mapChunkData.rect;
			if (chunk.IsConnectingChunk)
			{
				float num = chunk.Gateways[0].IsHorizontalGateway() ? Mathf.Abs(rect.size.y) : Mathf.Abs(rect.size.x);
				for (int i = (int)rect.xMin; i <= (int)rect.xMax; i++)
				{
					for (int j = (int)rect.yMin; j <= (int)rect.yMax; j++)
					{
						float num2 = Mathf.Abs((float)j - rect.yMin);
						float num3 = Mathf.Abs((float)j - rect.yMax);
						float num4 = Mathf.Abs((float)i - rect.xMin);
						float num5 = Mathf.Abs((float)i - rect.xMax);
						if (num <= 1f || ((num4 + 1f >= num5 || num2 >= num3) && (num4 - 1f <= num5 || num2 <= num3)))
						{
							this.SetTile(new Vector2Int(i, j));
						}
					}
				}
				return;
			}
			for (int k = (int)rect.xMin; k <= (int)rect.xMax; k++)
			{
				for (int l = (int)rect.yMin; l <= (int)rect.yMax; l++)
				{
					this.SetTile(new Vector2Int(k, l));
				}
			}
		}

		// Token: 0x0600160B RID: 5643 RVA: 0x00046670 File Offset: 0x00044870
		private void UpdateChunkRect(ILocationChunk chunk)
		{
			if (!chunk.IsConnectingChunk)
			{
				Vector2 vector;
				Vector2 vector2;
				chunk.GetWorldMinMaxPoints(out vector, out vector2);
				vector = LocationMapRenderer.FloorToInt(vector * this.minimapScale);
				Vector2Int vector2Int = (Vector2Int)this.tilemap.WorldToCell(vector) + this.tilemapOffset;
				Vector2 vector3 = LocationMapRenderer.FloorToInt(chunk.GetBounds().size * this.minimapScale);
				Rect rect = Rect.MinMaxRect((float)vector2Int.x, (float)vector2Int.y, (float)vector2Int.x + vector3.x, (float)vector2Int.y + vector3.y);
				LocationMapRenderer.MapChunkData data = new LocationMapRenderer.MapChunkData
				{
					rect = rect
				};
				this.AddChunkRect(chunk, data);
				return;
			}
			ILocationChunkGateway locationChunkGateway = chunk.Gateways[0];
			ILocationChunkGateway locationChunkGateway2 = chunk.Gateways[1];
			Vector2Int vector2Int2 = (Vector2Int)this.tilemap.WorldToCell(LocationMapRenderer.FloorToInt(locationChunkGateway.Position * this.minimapScale)) + this.tilemapOffset;
			Vector2Int vector2Int3 = (Vector2Int)this.tilemap.WorldToCell(LocationMapRenderer.FloorToInt(locationChunkGateway2.Position * this.minimapScale)) + this.tilemapOffset;
			LocationMapRenderer.MapChunkData mapChunkData = new LocationMapRenderer.MapChunkData
			{
				gatewaysPositions = new Vector2Int[2]
			};
			mapChunkData.gatewaysPositions[0] = vector2Int2;
			mapChunkData.gatewaysPositions[1] = vector2Int3;
			int num;
			int num2;
			if (locationChunkGateway.IsHorizontalGateway())
			{
				if (locationChunkGateway.TransitionDirection.x < 0f)
				{
					this.tilemapOffset.x = this.tilemapOffset.x - (mapChunkData.gatewaysPositions[1].x - mapChunkData.gatewaysPositions[0].x - this.transitionChunkLenght - 1);
					num = mapChunkData.gatewaysPositions[0].x;
					num2 = (mapChunkData.gatewaysPositions[1].x = mapChunkData.gatewaysPositions[0].x + this.transitionChunkLenght);
				}
				else
				{
					this.tilemapOffset.x = this.tilemapOffset.x - (mapChunkData.gatewaysPositions[0].x - mapChunkData.gatewaysPositions[1].x - this.transitionChunkLenght - 1);
					num = mapChunkData.gatewaysPositions[1].x;
					num2 = (mapChunkData.gatewaysPositions[0].x = mapChunkData.gatewaysPositions[1].x + this.transitionChunkLenght);
				}
				if (mapChunkData.gatewaysPositions[0].y < mapChunkData.gatewaysPositions[1].y)
				{
					mapChunkData.rect = Rect.MinMaxRect((float)num, (float)mapChunkData.gatewaysPositions[0].y, (float)num2, (float)mapChunkData.gatewaysPositions[1].y);
				}
				else
				{
					mapChunkData.rect = Rect.MinMaxRect((float)num, (float)mapChunkData.gatewaysPositions[1].y, (float)num2, (float)mapChunkData.gatewaysPositions[0].y);
				}
				if (((int)mapChunkData.rect.yMin - (int)mapChunkData.rect.yMax) % 2 == 0)
				{
					mapChunkData.rect = Rect.MinMaxRect(mapChunkData.rect.min.x, mapChunkData.rect.min.y - 1f, mapChunkData.rect.max.x, mapChunkData.rect.max.y);
				}
				this.AddChunkRect(chunk, mapChunkData);
				return;
			}
			if (locationChunkGateway.TransitionDirection.y < 0f)
			{
				this.tilemapOffset.y = this.tilemapOffset.y - (mapChunkData.gatewaysPositions[1].y - mapChunkData.gatewaysPositions[0].y - this.transitionChunkLenght - 1);
				num = mapChunkData.gatewaysPositions[0].y;
				num2 = (mapChunkData.gatewaysPositions[1].y = mapChunkData.gatewaysPositions[0].y + this.transitionChunkLenght);
			}
			else
			{
				this.tilemapOffset.y = this.tilemapOffset.y - (mapChunkData.gatewaysPositions[0].y - mapChunkData.gatewaysPositions[1].y - this.transitionChunkLenght - 1);
				num = mapChunkData.gatewaysPositions[1].y;
				num2 = (mapChunkData.gatewaysPositions[0].y = mapChunkData.gatewaysPositions[1].y + this.transitionChunkLenght);
			}
			if (mapChunkData.gatewaysPositions[0].x < mapChunkData.gatewaysPositions[1].x)
			{
				mapChunkData.rect = Rect.MinMaxRect((float)mapChunkData.gatewaysPositions[0].x, (float)num, (float)mapChunkData.gatewaysPositions[1].x, (float)num2);
			}
			else
			{
				mapChunkData.rect = Rect.MinMaxRect((float)mapChunkData.gatewaysPositions[1].x, (float)num, (float)mapChunkData.gatewaysPositions[0].x, (float)num2);
			}
			if (((int)mapChunkData.rect.xMin - (int)mapChunkData.rect.xMax) % 2 == 0)
			{
				mapChunkData.rect = Rect.MinMaxRect(mapChunkData.rect.min.x - 1f, mapChunkData.rect.min.y, mapChunkData.rect.max.x, mapChunkData.rect.max.y);
			}
			this.AddChunkRect(chunk, mapChunkData);
		}

		// Token: 0x04000CB1 RID: 3249
		public Camera minimapCamera;

		// Token: 0x04000CB2 RID: 3250
		public Tilemap tilemapPrefab;

		// Token: 0x04000CB3 RID: 3251
		public TileBase ruletile;

		// Token: 0x04000CB4 RID: 3252
		private Tilemap tilemap;

		// Token: 0x04000CB5 RID: 3253
		private Vector2Int tilemapOffset = Vector2Int.zero;

		// Token: 0x04000CB6 RID: 3254
		private LocationMapMarker mainFocusObjectMarker;

		// Token: 0x04000CB7 RID: 3255
		private bool centerMapByFocusObject = true;

		// Token: 0x04000CB8 RID: 3256
		private readonly float minimapScale = 0.2f;

		// Token: 0x04000CB9 RID: 3257
		private readonly int transitionChunkLenght = 7;

		// Token: 0x04000CBA RID: 3258
		private readonly Dictionary<ILocationChunk, LocationMapRenderer.MapChunkData> chunkRectsDict = new Dictionary<ILocationChunk, LocationMapRenderer.MapChunkData>();

		// Token: 0x04000CBB RID: 3259
		private readonly List<LocationMapMarker> staticMarkers = new List<LocationMapMarker>();

		// Token: 0x04000CBC RID: 3260
		private readonly List<LocationMapMarker> dynamicMarkers = new List<LocationMapMarker>();

		// Token: 0x02000504 RID: 1284
		public class MapChunkData
		{
			// Token: 0x04001ABE RID: 6846
			public Rect rect;

			// Token: 0x04001ABF RID: 6847
			public Vector2Int[] gatewaysPositions;
		}
	}
}
