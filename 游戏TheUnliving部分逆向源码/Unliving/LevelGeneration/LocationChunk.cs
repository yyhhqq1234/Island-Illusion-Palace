using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Tiles;
using Common.UnityExtensions;
using Game.LevelGeneration;
using Game.LevelGeneration.Utility;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Unliving.Mobs;

namespace Unliving.LevelGeneration
{
	// Token: 0x0200026C RID: 620
	[DefaultExecutionOrder(-120)]
	[SelectionBase]
	public sealed class LocationChunk : MonoBehaviour, ILocationChunk, ILocationObject, ICloneable<ILocationChunk>, IDestroyable
	{
		// Token: 0x060014DB RID: 5339 RVA: 0x0004289C File Offset: 0x00040A9C
		private static bool IsGroundTilemap(Tilemap tilemap)
		{
			TilemapCollider2D tilemapCollider2D;
			Renderer renderer;
			return !tilemap.TryGetComponent<TilemapCollider2D>(out tilemapCollider2D) && tilemap.TryGetComponent<Renderer>(out renderer) && renderer.sortingLayerName == "Background";
		}

		// Token: 0x060014DC RID: 5340 RVA: 0x000428D0 File Offset: 0x00040AD0
		private static int TilemapSortingComparison(Tilemap tilemap0, Tilemap tilemap1)
		{
			Renderer component = tilemap0.GetComponent<Renderer>();
			Renderer component2 = tilemap1.GetComponent<Renderer>();
			long value = (long)SortingLayer.GetLayerValueFromID(component.sortingLayerID) * 2147483647L + (long)component.sortingOrder;
			return ((long)SortingLayer.GetLayerValueFromID(component2.sortingLayerID) * 2147483647L + (long)component2.sortingOrder).CompareTo(value);
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x060014DD RID: 5341 RVA: 0x0004292B File Offset: 0x00040B2B
		// (set) Token: 0x060014DE RID: 5342 RVA: 0x00042933 File Offset: 0x00040B33
		public ILocationChunk ChunkPrototype
		{
			get
			{
				return this.chunkPrototype;
			}
			set
			{
				if (!this.isInstantiated)
				{
					Debug.LogError(string.Format("Unable to set Chunk Prototype. Instantinate {0} first.", this));
					return;
				}
				this.chunkPrototype = value;
			}
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x060014DF RID: 5343 RVA: 0x00042955 File Offset: 0x00040B55
		// (set) Token: 0x060014E0 RID: 5344 RVA: 0x00042972 File Offset: 0x00040B72
		public Vector2 Position
		{
			get
			{
				return base.transform.position + this.GetWorldCenterOffset();
			}
			set
			{
				base.transform.position = value - this.GetWorldCenterOffset();
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x060014E1 RID: 5345 RVA: 0x00042990 File Offset: 0x00040B90
		// (set) Token: 0x060014E2 RID: 5346 RVA: 0x0004299E File Offset: 0x00040B9E
		public float Orientation
		{
			get
			{
				return base.transform.GetRotation2D(false);
			}
			set
			{
				if (this._canBeRotated)
				{
					base.transform.SetRotation2D(value, false);
				}
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x060014E3 RID: 5347 RVA: 0x000429B5 File Offset: 0x00040BB5
		// (set) Token: 0x060014E4 RID: 5348 RVA: 0x000429BD File Offset: 0x00040BBD
		public int Depth { get; set; }

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x060014E5 RID: 5349 RVA: 0x000429C6 File Offset: 0x00040BC6
		// (set) Token: 0x060014E6 RID: 5350 RVA: 0x000429CE File Offset: 0x00040BCE
		public ILocationChunkVisitorsWatcher VisitorsWatcher
		{
			get
			{
				return this.visitorsWatcher;
			}
			set
			{
				this.SetVisitorsWatcher(value);
			}
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x060014E7 RID: 5351 RVA: 0x000429D7 File Offset: 0x00040BD7
		// (set) Token: 0x060014E8 RID: 5352 RVA: 0x000429DF File Offset: 0x00040BDF
		public bool IsVisible
		{
			get
			{
				return this.isVisible;
			}
			set
			{
				if (this.isVisible != value)
				{
					this.isVisible = value;
					this.UpdateVisibility();
				}
			}
		}

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x060014E9 RID: 5353 RVA: 0x000429F7 File Offset: 0x00040BF7
		// (set) Token: 0x060014EA RID: 5354 RVA: 0x000429FA File Offset: 0x00040BFA
		ILocationChunk ILocationObject.CurrentLocationChunk
		{
			get
			{
				return this;
			}
			set
			{
			}
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x060014EB RID: 5355 RVA: 0x000429FC File Offset: 0x00040BFC
		public Vector2 WorldSize
		{
			get
			{
				this.CalculateChunkBoundsData();
				if (!this._canBeRotated)
				{
					return this._size;
				}
				return base.transform.rotation.RotateVector2D(this._size, true);
			}
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x060014EC RID: 5356 RVA: 0x00042A2A File Offset: 0x00040C2A
		// (set) Token: 0x060014ED RID: 5357 RVA: 0x00042A32 File Offset: 0x00040C32
		public LocationChunkMobsGridController MobsGrid
		{
			get
			{
				return this.mobsGrid;
			}
			set
			{
				this.mobsGrid = value;
			}
		}

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x060014EE RID: 5358 RVA: 0x00042A3C File Offset: 0x00040C3C
		public string CachedName
		{
			get
			{
				string result;
				if ((result = this.cachedName) == null)
				{
					result = (this.cachedName = base.name);
				}
				return result;
			}
		}

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x060014EF RID: 5359 RVA: 0x00042A62 File Offset: 0x00040C62
		public LocationChunk.TypeID Type
		{
			get
			{
				return this._type;
			}
		}

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x060014F0 RID: 5360 RVA: 0x00042A6A File Offset: 0x00040C6A
		public int Level
		{
			get
			{
				return this._level;
			}
		}

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x060014F1 RID: 5361 RVA: 0x00042A72 File Offset: 0x00040C72
		// (set) Token: 0x060014F2 RID: 5362 RVA: 0x00042A7A File Offset: 0x00040C7A
		public int MinAllowedParentChunkLevel
		{
			get
			{
				return this.minRequiredParentChunkLevel;
			}
			set
			{
				this.minRequiredParentChunkLevel = Mathf.Clamp(value, 1, 10);
			}
		}

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x060014F3 RID: 5363 RVA: 0x00042A8B File Offset: 0x00040C8B
		// (set) Token: 0x060014F4 RID: 5364 RVA: 0x00042A93 File Offset: 0x00040C93
		public int MaxAllowedParentChunkLevel
		{
			get
			{
				return this.maxRequiredParentChunkLevel;
			}
			set
			{
				this.maxRequiredParentChunkLevel = Mathf.Clamp(value, 1, 10);
			}
		}

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x060014F5 RID: 5365 RVA: 0x00042AA4 File Offset: 0x00040CA4
		public bool CanBeRotated
		{
			get
			{
				return this._canBeRotated;
			}
		}

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x060014F6 RID: 5366 RVA: 0x00042AAC File Offset: 0x00040CAC
		public bool IsCoreChunk
		{
			get
			{
				return this._type < LocationChunk.TypeID.SellerChunk;
			}
		}

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x060014F7 RID: 5367 RVA: 0x00042AB8 File Offset: 0x00040CB8
		public bool IsConnectingChunk
		{
			get
			{
				return this._type == LocationChunk.TypeID.Corridor;
			}
		}

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x060014F8 RID: 5368 RVA: 0x00042AC4 File Offset: 0x00040CC4
		public Transform GatewaysRoot
		{
			get
			{
				if (!this._gatewaysRoot.IsNull())
				{
					return this._gatewaysRoot;
				}
				return base.transform;
			}
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x060014F9 RID: 5369 RVA: 0x00042AE0 File Offset: 0x00040CE0
		public float ChunkGridCellSize
		{
			get
			{
				return this.chunkGridCellSize;
			}
		}

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x060014FA RID: 5370 RVA: 0x00042AE8 File Offset: 0x00040CE8
		public IGameLocation CurrentLocation
		{
			get
			{
				return this.currentLocation;
			}
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x060014FB RID: 5371 RVA: 0x00042AF0 File Offset: 0x00040CF0
		public int ChunkID
		{
			get
			{
				return this.chunkID;
			}
		}

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x060014FC RID: 5372 RVA: 0x00042AF8 File Offset: 0x00040CF8
		public IList<ILocationChunkGateway> Gateways
		{
			get
			{
				if (!this.isInstantiated)
				{
					return this.GetGatewaysFromPrototype();
				}
				return this.gateways;
			}
		}

		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x060014FD RID: 5373 RVA: 0x00042B0F File Offset: 0x00040D0F
		public IList<ILocationChunkEntrancePoint> EntrancePoints
		{
			get
			{
				return this.entrancePoints;
			}
		}

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x060014FE RID: 5374 RVA: 0x00042B17 File Offset: 0x00040D17
		public IList<ILocationChunkVisitor> Visitors
		{
			get
			{
				return this.visitors;
			}
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x060014FF RID: 5375 RVA: 0x00042B1F File Offset: 0x00040D1F
		public IList<ILocationObject> EnvironmentObjects
		{
			get
			{
				return this.environmentObjects;
			}
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06001500 RID: 5376 RVA: 0x00042B27 File Offset: 0x00040D27
		public bool IsExplored
		{
			get
			{
				return this.isExplored;
			}
		}

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06001501 RID: 5377 RVA: 0x00042B2F File Offset: 0x00040D2F
		public bool IsInitialized
		{
			get
			{
				return this.isInitialized;
			}
		}

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x06001502 RID: 5378 RVA: 0x00042B37 File Offset: 0x00040D37
		public bool IsDestroyed
		{
			get
			{
				return this.isDestroyed;
			}
		}

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x06001503 RID: 5379 RVA: 0x00042B3F File Offset: 0x00040D3F
		int? ILocationObject.LocationObjectType
		{
			get
			{
				return new int?((int)this.Type);
			}
		}

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06001504 RID: 5380 RVA: 0x00042B4C File Offset: 0x00040D4C
		public Tilemap[] SortedTilemaps
		{
			get
			{
				if (this.sortedTilemaps == null && !this.grid.IsNull())
				{
					this.sortedTilemaps = this.grid.GetComponentsInChildren<Tilemap>();
					Array.Sort<Tilemap>(this.sortedTilemaps, new Comparison<Tilemap>(LocationChunk.TilemapSortingComparison));
				}
				return this.sortedTilemaps;
			}
		}

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06001505 RID: 5381 RVA: 0x00042B9C File Offset: 0x00040D9C
		public Tilemap[] SortedGroundTilemaps
		{
			get
			{
				Tilemap[] result;
				if ((result = this.sortedGroundTilemaps) == null)
				{
					result = (this.sortedGroundTilemaps = this.SortedTilemaps.Where(new Func<Tilemap, bool>(LocationChunk.IsGroundTilemap)).ToArray<Tilemap>());
				}
				return result;
			}
		}

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06001506 RID: 5382 RVA: 0x00042BD8 File Offset: 0x00040DD8
		bool ILocationObject.IsDynamicLocationObject
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06001507 RID: 5383 RVA: 0x00042BDB File Offset: 0x00040DDB
		public IReadOnlyList<GameMobsGroupControllerBase> SpawnedMobGroups
		{
			get
			{
				return this.spawnedMobGroups;
			}
		}

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06001508 RID: 5384 RVA: 0x00042BE3 File Offset: 0x00040DE3
		// (set) Token: 0x06001509 RID: 5385 RVA: 0x00042BEB File Offset: 0x00040DEB
		public IList<NavMeshData> LocalNavMeshData { get; set; }

		// Token: 0x140000C9 RID: 201
		// (add) Token: 0x0600150A RID: 5386 RVA: 0x00042BF4 File Offset: 0x00040DF4
		// (remove) Token: 0x0600150B RID: 5387 RVA: 0x00042C2C File Offset: 0x00040E2C
		public event Action<IGameLocationGenerator, LocationChunk> PlacedByLocationGenerator;

		// Token: 0x140000CA RID: 202
		// (add) Token: 0x0600150C RID: 5388 RVA: 0x00042C64 File Offset: 0x00040E64
		// (remove) Token: 0x0600150D RID: 5389 RVA: 0x00042C9C File Offset: 0x00040E9C
		public event Action<IGameLocation, int> RegisteredInLocation;

		// Token: 0x140000CB RID: 203
		// (add) Token: 0x0600150E RID: 5390 RVA: 0x00042CD4 File Offset: 0x00040ED4
		// (remove) Token: 0x0600150F RID: 5391 RVA: 0x00042D0C File Offset: 0x00040F0C
		public event Action<ILocationChunk, ILocationChunkVisitor> BeforeVisitorAdded;

		// Token: 0x140000CC RID: 204
		// (add) Token: 0x06001510 RID: 5392 RVA: 0x00042D44 File Offset: 0x00040F44
		// (remove) Token: 0x06001511 RID: 5393 RVA: 0x00042D7C File Offset: 0x00040F7C
		public event Action<ILocationChunk, ILocationChunkVisitor> VisitorAdded;

		// Token: 0x140000CD RID: 205
		// (add) Token: 0x06001512 RID: 5394 RVA: 0x00042DB4 File Offset: 0x00040FB4
		// (remove) Token: 0x06001513 RID: 5395 RVA: 0x00042DEC File Offset: 0x00040FEC
		public event Action<ILocationChunk, ILocationChunkVisitor> VisitorRemoved;

		// Token: 0x140000CE RID: 206
		// (add) Token: 0x06001514 RID: 5396 RVA: 0x00042E24 File Offset: 0x00041024
		// (remove) Token: 0x06001515 RID: 5397 RVA: 0x00042E5C File Offset: 0x0004105C
		public event Action<ILocationChunk, ILocationObject> EnvironmentObjectAdded;

		// Token: 0x140000CF RID: 207
		// (add) Token: 0x06001516 RID: 5398 RVA: 0x00042E94 File Offset: 0x00041094
		// (remove) Token: 0x06001517 RID: 5399 RVA: 0x00042ECC File Offset: 0x000410CC
		public event Action<ILocationChunk, ILocationObject> EnvironmentObjectRemoved;

		// Token: 0x140000D0 RID: 208
		// (add) Token: 0x06001518 RID: 5400 RVA: 0x00042F04 File Offset: 0x00041104
		// (remove) Token: 0x06001519 RID: 5401 RVA: 0x00042F3C File Offset: 0x0004113C
		public event Action<ILocationChunk, bool> VisibilityChanged;

		// Token: 0x140000D1 RID: 209
		// (add) Token: 0x0600151A RID: 5402 RVA: 0x00042F74 File Offset: 0x00041174
		// (remove) Token: 0x0600151B RID: 5403 RVA: 0x00042FAC File Offset: 0x000411AC
		public event Action<object> Destroyed;

		// Token: 0x0600151C RID: 5404 RVA: 0x00042FE4 File Offset: 0x000411E4
		private List<ILocationChunkGateway> GetGatewaysFromPrototype()
		{
			List<ILocationChunkGateway> list = new List<ILocationChunkGateway>(6);
			Grid componentInChildren = base.GetComponentInChildren<Grid>();
			int childCount = componentInChildren.transform.childCount;
			this.CalculateChunkBoundsData();
			for (int i = 0; i < childCount; i++)
			{
				Transform child = componentInChildren.transform.GetChild(i);
				LocationGate locationGate;
				if (child.gameObject.activeSelf && child.TryGetComponent<LocationGate>(out locationGate))
				{
					ILocationChunkGateway locationChunkGateway = locationGate.CreateChunkGateway(this);
					if (locationChunkGateway != null)
					{
						list.Add(locationChunkGateway);
					}
				}
			}
			if (list.Count == 0)
			{
				Debug.LogWarning(string.Format("{0} has no gateways.", this));
			}
			return list;
		}

		// Token: 0x0600151D RID: 5405 RVA: 0x00043074 File Offset: 0x00041274
		private Bounds GetBounds(Transform pivot0, Transform pivot1)
		{
			if (pivot0 != null && pivot1 != null)
			{
				Bounds result = default(Bounds);
				Vector2 lhs = pivot0.position;
				Vector2 rhs = pivot1.position;
				result.SetMinMax(Vector2.Min(lhs, rhs), Vector2.Max(lhs, rhs));
				return result;
			}
			return default(Bounds);
		}

		// Token: 0x0600151E RID: 5406 RVA: 0x000430E0 File Offset: 0x000412E0
		private Grid TryGetTilemapLayers(bool force = false)
		{
			if (this._tilemapLayers == null)
			{
				this._tilemapLayers = new List<Tilemap>();
			}
			else if (!force && this._tilemapLayers.Count != 0)
			{
				if (this._tilemapLayers[0].IsNull())
				{
					return null;
				}
				return this._tilemapLayers[0].layoutGrid;
			}
			Grid componentInChildren = base.GetComponentInChildren<Grid>();
			if (!componentInChildren.IsNull())
			{
				componentInChildren.GetComponentsInChildren<Tilemap>(this._tilemapLayers);
			}
			return componentInChildren;
		}

		// Token: 0x0600151F RID: 5407 RVA: 0x00043158 File Offset: 0x00041358
		private Bounds GetTotalChunkBounds()
		{
			Bounds result = this.GetBounds(this.boundsPivot0, this.boundsPivot1);
			if (result.size == default(Vector3))
			{
				this.TryGetTilemapLayers(true);
				if (this._tilemapLayers != null)
				{
					int count = this._tilemapLayers.Count;
					if (count != 0)
					{
						for (int i = 0; i < count; i++)
						{
							Tilemap tilemap = this._tilemapLayers[i];
							if (!tilemap.IsNull() && tilemap.gameObject.activeInHierarchy)
							{
								if (result.size == default(Vector3))
								{
									result = tilemap.localBounds;
								}
								else
								{
									result.Encapsulate(tilemap.localBounds);
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06001520 RID: 5408 RVA: 0x0004320F File Offset: 0x0004140F
		private void CalculateChunkBoundsData()
		{
			if (this.isBoundsDataCalculated)
			{
				return;
			}
			this.UpdateBounds(this.GetTotalChunkBounds());
			this.isBoundsDataCalculated = true;
		}

		// Token: 0x06001521 RID: 5409 RVA: 0x0004322D File Offset: 0x0004142D
		private Vector2 GetWorldCenterOffset()
		{
			this.CalculateChunkBoundsData();
			return base.transform.rotation.RotateVector2D(this.localCenterOffset, false);
		}

		// Token: 0x06001522 RID: 5410 RVA: 0x0004324C File Offset: 0x0004144C
		private void GetNavmeshBuildSources(GameObject rootObject, List<NavMeshBuildSource> targetList)
		{
			if (rootObject != null)
			{
				foreach (object obj in rootObject.transform)
				{
					Transform transform = (Transform)obj;
					NavMeshBuildSource item;
					if (!LocationChunk.<GetNavmeshBuildSources>g__IsInvalidObstacleSource|160_0(transform.gameObject) && NavMeshUtility.GetNavMeshObstacleBuildSource(transform.gameObject, out item, 1))
					{
						targetList.Add(item);
					}
					this.GetNavmeshBuildSources(transform.gameObject, targetList);
				}
			}
		}

		// Token: 0x06001523 RID: 5411 RVA: 0x000432D8 File Offset: 0x000414D8
		private void SetVisitorsWatcher(ILocationChunkVisitorsWatcher newVisitorsWatcher)
		{
			if (this.visitorsWatcher != newVisitorsWatcher)
			{
				if (this.visitorsWatcher != null)
				{
					this.visitorsWatcher.TargetLocationChunk = null;
					this.visitorsWatcher.VisitorEntered -= this.OnVisitorEntered;
					this.visitorsWatcher.VisitorExited -= this.RemoveVisitor;
				}
				if (newVisitorsWatcher != null)
				{
					newVisitorsWatcher.TargetLocationChunk = this;
					newVisitorsWatcher.VisitorEntered += this.OnVisitorEntered;
					newVisitorsWatcher.VisitorExited += this.RemoveVisitor;
				}
				this.visitorsWatcher = newVisitorsWatcher;
			}
		}

		// Token: 0x06001524 RID: 5412 RVA: 0x00043368 File Offset: 0x00041568
		private void UpdateVisibility()
		{
			IList<NavMeshData> localNavMeshData = this.LocalNavMeshData;
			if (localNavMeshData != null && this.localNavMeshDataInstances == null)
			{
				this.localNavMeshDataInstances = new NavMeshDataInstance[localNavMeshData.Count];
			}
			if (this.localNavMeshDataInstances != null)
			{
				for (int i = 0; i < this.localNavMeshDataInstances.Length; i++)
				{
					if (this.isVisible)
					{
						this.localNavMeshDataInstances[i] = NavMesh.AddNavMeshData(localNavMeshData[i]);
					}
					else
					{
						this.localNavMeshDataInstances[i].Remove();
					}
				}
			}
			base.gameObject.SetActive(this.isVisible);
			Action<ILocationChunk, bool> visibilityChanged = this.VisibilityChanged;
			if (visibilityChanged == null)
			{
				return;
			}
			visibilityChanged(this, this.isVisible);
		}

		// Token: 0x06001525 RID: 5413 RVA: 0x0004340E File Offset: 0x0004160E
		public bool IsChildObject(ILocationObject obj)
		{
			return !obj.IsNull() && obj.CurrentLocationChunk == this;
		}

		// Token: 0x06001526 RID: 5414 RVA: 0x00043423 File Offset: 0x00041623
		public bool CanBeAdded(ILocationObject obj)
		{
			return !this.isDestroyed && !obj.IsNull() && !this.IsChildObject(obj);
		}

		// Token: 0x06001527 RID: 5415 RVA: 0x00043444 File Offset: 0x00041644
		public void AddGateway(ILocationChunkGateway gateway)
		{
			if (!this.CanBeAdded(gateway))
			{
				return;
			}
			if (this.overrideGatewaysType)
			{
				LocationChunkGateway locationChunkGateway = gateway as LocationChunkGateway;
				if (locationChunkGateway != null)
				{
					locationChunkGateway.GatewayType = this.gatewaysType;
				}
			}
			gateway.LocationObjectType = new int?(gateway.IsHorizontalGateway() ? 1 : 2);
			gateway.CurrentLocationChunk = this;
			gateway.Destroyed += this.OnGatewayDestroyed;
			Component component = gateway as Component;
			if (component != null)
			{
				component.transform.parent = this.GatewaysRoot;
			}
			this.gateways.Add(gateway);
		}

		// Token: 0x06001528 RID: 5416 RVA: 0x000434D2 File Offset: 0x000416D2
		public void AddEntrancePoint(ILocationChunkEntrancePoint entrancePoint)
		{
			if (entrancePoint.IsNull())
			{
				return;
			}
			this.entrancePoints.Add(entrancePoint);
		}

		// Token: 0x06001529 RID: 5417 RVA: 0x000434EC File Offset: 0x000416EC
		public void GetNavMeshBuildSources(List<NavMeshBuildSource> targetList, Bounds? groundBoundsOverride = null)
		{
			if (this.navMeshBuildSources == null)
			{
				this.navMeshBuildSources = new List<NavMeshBuildSource>(32);
				Grid componentInChildren = base.GetComponentInChildren<Grid>();
				if (componentInChildren != null)
				{
					Transform parent = componentInChildren.transform.parent;
					if (!componentInChildren.gameObject.activeInHierarchy)
					{
						componentInChildren.transform.SetParent(null, true);
					}
					foreach (object obj in componentInChildren.transform)
					{
						LocationChunk.<GetNavMeshBuildSources>g__CollectTilemapObstacles|167_1((Transform)obj, this.navMeshBuildSources);
					}
					componentInChildren.transform.SetParent(parent, true);
				}
				Vector2 v = this.Position;
				Vector2 a = this.WorldSize;
				if (groundBoundsOverride != null)
				{
					v = groundBoundsOverride.Value.center;
					a = groundBoundsOverride.Value.size;
				}
				this.navMeshBuildSources.Add(NavMeshUtility.GetNavMeshGroundBuildSource(v, a + new Vector2(0.1f, 0.1f), 0));
				this.GetNavmeshBuildSources(base.gameObject, this.navMeshBuildSources);
			}
			targetList.AddRange(this.navMeshBuildSources);
		}

		// Token: 0x0600152A RID: 5418 RVA: 0x0004363C File Offset: 0x0004183C
		public void AddVisitor(ILocationChunkVisitor visitor)
		{
			if (!this.CanBeAdded(visitor) || !visitor.CanVisitLocationChunk(this))
			{
				return;
			}
			visitor.CurrentLocationChunk = this;
			if (this.IsLocationExplorer(visitor))
			{
				this.isExplored = true;
			}
			Action<ILocationChunk, ILocationChunkVisitor> beforeVisitorAdded = this.BeforeVisitorAdded;
			if (beforeVisitorAdded != null)
			{
				beforeVisitorAdded(this, visitor);
			}
			if (!this.visitors.Contains(visitor))
			{
				this.visitors.Add(visitor);
			}
			visitor.OnAddedToLocationChunk(this);
			Action<ILocationChunk, ILocationChunkVisitor> visitorAdded = this.VisitorAdded;
			if (visitorAdded == null)
			{
				return;
			}
			visitorAdded(this, visitor);
		}

		// Token: 0x0600152B RID: 5419 RVA: 0x000436B9 File Offset: 0x000418B9
		public void RemoveVisitor(ILocationChunkVisitor visitor)
		{
			if (!this.visitors.Remove(visitor))
			{
				return;
			}
			if (this.IsChildObject(visitor))
			{
				visitor.CurrentLocationChunk = null;
			}
			Action<ILocationChunk, ILocationChunkVisitor> visitorRemoved = this.VisitorRemoved;
			if (visitorRemoved == null)
			{
				return;
			}
			visitorRemoved(this, visitor);
		}

		// Token: 0x0600152C RID: 5420 RVA: 0x000436EC File Offset: 0x000418EC
		public void AddEnvironmentObject(ILocationObject obj)
		{
			if (!this.CanBeAdded(obj))
			{
				return;
			}
			obj.CurrentLocationChunk = this;
			this.environmentObjects.Add(obj);
			Action<ILocationChunk, ILocationObject> environmentObjectAdded = this.EnvironmentObjectAdded;
			if (environmentObjectAdded == null)
			{
				return;
			}
			environmentObjectAdded(this, obj);
		}

		// Token: 0x0600152D RID: 5421 RVA: 0x0004371D File Offset: 0x0004191D
		public void RemoveEnvironmentObject(ILocationObject obj)
		{
			if (this.environmentObjects.Remove(obj))
			{
				Action<ILocationChunk, ILocationObject> environmentObjectRemoved = this.EnvironmentObjectRemoved;
				if (environmentObjectRemoved == null)
				{
					return;
				}
				environmentObjectRemoved(this, obj);
			}
		}

		// Token: 0x0600152E RID: 5422 RVA: 0x0004373F File Offset: 0x0004193F
		public void RemoveEnvironmentObject(int objectIndex)
		{
			this.environmentObjects.RemoveAt(objectIndex);
		}

		// Token: 0x0600152F RID: 5423 RVA: 0x0004374D File Offset: 0x0004194D
		public void UpdateBounds(Bounds newBounds)
		{
			this.localCenterOffset = newBounds.center - base.transform.position;
			this._size = newBounds.size;
		}

		// Token: 0x06001530 RID: 5424 RVA: 0x00043784 File Offset: 0x00041984
		public bool GetCameraBounds(out Bounds cameraBounds)
		{
			cameraBounds = this.GetBounds(this.cameraBoundsPivot0, this.cameraBoundsPivot1);
			return cameraBounds.size != default(Vector3);
		}

		// Token: 0x06001531 RID: 5425 RVA: 0x000437C0 File Offset: 0x000419C0
		public bool TryGetGridCellPosition(Vector3 pos, out Vector3Int cellPosition)
		{
			if (this.grid != null)
			{
				pos.z = this.grid.transform.position.z;
				cellPosition = this.grid.WorldToCell(pos);
				return true;
			}
			cellPosition = default(Vector3Int);
			return false;
		}

		// Token: 0x06001532 RID: 5426 RVA: 0x00043814 File Offset: 0x00041A14
		public IMaterialUsingTile GetGroundTileAtCellPosition(Vector3Int cellPos)
		{
			Tilemap[] array = this.SortedGroundTilemaps;
			for (int i = 0; i < array.Length; i++)
			{
				IMaterialUsingTile materialUsingTile = array[i].GetTile(cellPos) as IMaterialUsingTile;
				if (materialUsingTile != null)
				{
					return materialUsingTile;
				}
			}
			return null;
		}

		// Token: 0x06001533 RID: 5427 RVA: 0x0004384B File Offset: 0x00041A4B
		public void RegisterSpawnedMobsGroup(GameMobsGroupControllerBase group)
		{
			this.spawnedMobGroups.Add(group);
		}

		// Token: 0x06001534 RID: 5428 RVA: 0x00043859 File Offset: 0x00041A59
		public ILocationChunk Clone()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(base.gameObject);
			gameObject.name = base.name + "_instance";
			ILocationChunk component = gameObject.GetComponent<ILocationChunk>();
			component.ChunkPrototype = this;
			return component;
		}

		// Token: 0x06001535 RID: 5429 RVA: 0x00043888 File Offset: 0x00041A88
		public void Destroy()
		{
			this.isDestroyed = true;
			for (int i = this.gateways.Count - 1; i >= 0; i--)
			{
				ILocationChunkGateway locationChunkGateway = this.gateways[i];
				if (!locationChunkGateway.IsNull())
				{
					locationChunkGateway.Destroy();
				}
			}
			this.gateways.Clear();
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06001536 RID: 5430 RVA: 0x000438E8 File Offset: 0x00041AE8
		public override string ToString()
		{
			return string.Format("LocationChunk (type: {0} level: {1} name: {2} deadend: {3})", new object[]
			{
				this._type,
				this._level,
				base.name,
				this.IsDeadEndChunk()
			});
		}

		// Token: 0x06001537 RID: 5431 RVA: 0x00043938 File Offset: 0x00041B38
		public void OnPlacedByLocationGenerator(IGameLocationGenerator generator)
		{
			Action<IGameLocationGenerator, LocationChunk> placedByLocationGenerator = this.PlacedByLocationGenerator;
			if (placedByLocationGenerator == null)
			{
				return;
			}
			placedByLocationGenerator(generator, this);
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x0004394C File Offset: 0x00041B4C
		public void OnRegistered(IGameLocation location, int chunkID)
		{
			this.currentLocation = location;
			this.chunkID = chunkID;
			Action<IGameLocation, int> registeredInLocation = this.RegisteredInLocation;
			if (registeredInLocation == null)
			{
				return;
			}
			registeredInLocation(location, chunkID);
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x0004396E File Offset: 0x00041B6E
		private void OnVisitorEntered(ILocationChunkVisitor visitor)
		{
			if (this.isVisible || visitor.IsLocationExplorer())
			{
				this.AddVisitor(visitor);
			}
		}

		// Token: 0x0600153A RID: 5434 RVA: 0x00043988 File Offset: 0x00041B88
		private void OnGatewayDestroyed(object gateway)
		{
			ILocationChunkGateway locationChunkGateway = gateway as ILocationChunkGateway;
			locationChunkGateway.Destroyed -= this.OnGatewayDestroyed;
			this.gateways.Remove(locationChunkGateway);
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x000439BC File Offset: 0x00041BBC
		private void Awake()
		{
			this.isInstantiated = true;
			this.isInitialized = false;
			this.isBoundsDataCalculated = false;
			this.isDestroyed = false;
			this.isExplored = false;
			this.isVisible = base.gameObject.activeSelf;
			this.chunkGridCellSize = 0f;
			this.CalculateChunkBoundsData();
			if (this.gateways == null || this.gateways.Count == 0)
			{
				foreach (object obj in this.GatewaysRoot)
				{
					Transform transform = (Transform)obj;
					ILocationChunkGateway gateway;
					if (transform.gameObject.activeSelf && transform.TryGetComponent<ILocationChunkGateway>(out gateway))
					{
						this.AddGateway(gateway);
					}
				}
			}
			if (this._tilemapLayers.Count != 0)
			{
				Tilemap tilemap = this._tilemapLayers[0];
				if (!tilemap.IsNull())
				{
					this.chunkGridCellSize = tilemap.cellSize.x;
				}
			}
			this.grid = base.GetComponentInChildren<Grid>();
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x00043AC8 File Offset: 0x00041CC8
		private void Start()
		{
			if (this.mobsGrid == null)
			{
				base.TryGetComponent<LocationChunkMobsGridController>(out this.mobsGrid);
			}
			this.isInitialized = true;
			if (Debug.isDebugBuild)
			{
				LocationChunk.TypeID type = this._type;
			}
		}

		// Token: 0x0600153D RID: 5437 RVA: 0x00043AFC File Offset: 0x00041CFC
		private void OnDestroy()
		{
			this.isDestroyed = true;
			IList<NavMeshData> localNavMeshData = this.LocalNavMeshData;
			if (this.localNavMeshDataInstances != null)
			{
				for (int i = 0; i < this.localNavMeshDataInstances.Length; i++)
				{
					this.localNavMeshDataInstances[i].Remove();
				}
			}
			if (localNavMeshData != null)
			{
				for (int j = 0; j < localNavMeshData.Count; j++)
				{
					UnityEngine.Object.DestroyImmediate(localNavMeshData[j]);
				}
				this.LocalNavMeshData = null;
			}
			Action<object> destroyed = this.Destroyed;
			if (destroyed != null)
			{
				destroyed(this);
			}
			this.VisitorsWatcher = null;
		}

		// Token: 0x0600153E RID: 5438 RVA: 0x00043B84 File Offset: 0x00041D84
		private void OnDrawGizmos()
		{
			if (this.visitors.Count == 0)
			{
				return;
			}
			Vector2 position = this.Position;
			Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
			for (int i = 0; i < this.visitors.Count; i++)
			{
				if (this.visitors[i] is Component)
				{
					Gizmos.DrawLine(position, this.visitors[i].Position);
				}
			}
		}

		// Token: 0x0600153F RID: 5439 RVA: 0x00043C0E File Offset: 0x00041E0E
		private void OnDrawGizmosSelected()
		{
			bool isPlaying = Application.isPlaying;
		}

		// Token: 0x06001541 RID: 5441 RVA: 0x00043C98 File Offset: 0x00041E98
		[CompilerGenerated]
		internal static bool <GetNavmeshBuildSources>g__IsInvalidObstacleSource|160_0(GameObject obj)
		{
			ILocationChunkVisitor locationChunkVisitor;
			NavMeshObstacle navMeshObstacle;
			Tilemap tilemap;
			return !obj.activeSelf || obj.CompareTag("ExcludeFromNavMesh") || obj.TryGetComponent<ILocationChunkVisitor>(out locationChunkVisitor) || obj.TryGetComponent<NavMeshObstacle>(out navMeshObstacle) || obj.TryGetComponent<Tilemap>(out tilemap);
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x00043CDC File Offset: 0x00041EDC
		[CompilerGenerated]
		internal static bool <GetNavMeshBuildSources>g__TryGetTilemapObstacle|167_0(Transform root, out NavMeshBuildSource tilemapSource)
		{
			Tilemap tilemap;
			if (root.TryGetComponent<Tilemap>(out tilemap) && tilemap.gameObject.activeSelf && tilemap.CompareTag("ObstaclesTilemap"))
			{
				tilemapSource = NavMeshUtility.GetNavMeshTilemapColliderBuildSource(tilemap, 1);
				if (tilemapSource.sourceObject != null)
				{
					return true;
				}
			}
			tilemapSource = default(NavMeshBuildSource);
			return false;
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x00043D34 File Offset: 0x00041F34
		[CompilerGenerated]
		internal static void <GetNavMeshBuildSources>g__CollectTilemapObstacles|167_1(Transform root, List<NavMeshBuildSource> obstacles)
		{
			NavMeshBuildSource item;
			if (LocationChunk.<GetNavMeshBuildSources>g__TryGetTilemapObstacle|167_0(root, out item))
			{
				obstacles.Add(item);
			}
			int childCount = root.childCount;
			for (int i = 0; i < childCount; i++)
			{
				NavMeshBuildSource item2;
				if (LocationChunk.<GetNavMeshBuildSources>g__TryGetTilemapObstacle|167_0(root.GetChild(i), out item2))
				{
					obstacles.Add(item2);
				}
			}
		}

		// Token: 0x04000C1E RID: 3102
		public const int ExecutionOrder = -120;

		// Token: 0x04000C1F RID: 3103
		public const int MaxLevel = 10;

		// Token: 0x04000C20 RID: 3104
		public const string ObstacleLayerTag = "ObstaclesTilemap";

		// Token: 0x04000C21 RID: 3105
		public const string BackgroundLayerName = "Background";

		// Token: 0x04000C22 RID: 3106
		private const int MinorChunksTypeStartID = 30;

		// Token: 0x04000C2E RID: 3118
		[SerializeField]
		[Tooltip("Тип чанка.")]
		private LocationChunk.TypeID _type;

		// Token: 0x04000C2F RID: 3119
		[SerializeField]
		[Range(1f, 10f)]
		[Tooltip("Уровень чанка.")]
		private int _level = 1;

		// Token: 0x04000C30 RID: 3120
		[SerializeField]
		[Range(1f, 10f)]
		private int minRequiredParentChunkLevel = 1;

		// Token: 0x04000C31 RID: 3121
		[SerializeField]
		[Range(1f, 10f)]
		private int maxRequiredParentChunkLevel = 10;

		// Token: 0x04000C32 RID: 3122
		[SerializeField]
		[Tooltip("Размер чанка. Данные значения будут заданы автоматически, если чанк имеет объекты с компонентом Tilemap.")]
		private Vector2 _size = new Vector2(5f, 5f);

		// Token: 0x04000C33 RID: 3123
		[SerializeField]
		[HideInInspector]
		private bool _canBeRotated;

		// Token: 0x04000C34 RID: 3124
		[SerializeField]
		[Tooltip("Объект, в который при генерации локации будут помещены объекты-проходы данного чанка.")]
		private Transform _gatewaysRoot;

		// Token: 0x04000C35 RID: 3125
		[Tooltip("Общий тип для всех объектов-проходов данного чанка.")]
		public LocationChunkGateway.Type gatewaysType;

		// Token: 0x04000C36 RID: 3126
		[Tooltip("Если активно, то для любых ворот чанка будет использован указанный выше тип.")]
		public bool overrideGatewaysType;

		// Token: 0x04000C37 RID: 3127
		[SerializeField]
		private List<Tilemap> _tilemapLayers;

		// Token: 0x04000C38 RID: 3128
		private Tilemap[] sortedTilemaps;

		// Token: 0x04000C39 RID: 3129
		private Tilemap[] sortedGroundTilemaps;

		// Token: 0x04000C3A RID: 3130
		public Transform boundsPivot0;

		// Token: 0x04000C3B RID: 3131
		public Transform boundsPivot1;

		// Token: 0x04000C3C RID: 3132
		public Transform cameraBoundsPivot0;

		// Token: 0x04000C3D RID: 3133
		public Transform cameraBoundsPivot1;

		// Token: 0x04000C3E RID: 3134
		private readonly List<ILocationChunkGateway> gateways = new List<ILocationChunkGateway>(6);

		// Token: 0x04000C3F RID: 3135
		private readonly List<ILocationChunkEntrancePoint> entrancePoints = new List<ILocationChunkEntrancePoint>(6);

		// Token: 0x04000C40 RID: 3136
		private readonly List<ILocationChunkVisitor> visitors = new List<ILocationChunkVisitor>(50);

		// Token: 0x04000C41 RID: 3137
		private readonly List<ILocationObject> environmentObjects = new List<ILocationObject>(50);

		// Token: 0x04000C42 RID: 3138
		private readonly List<GameMobsGroupControllerBase> spawnedMobGroups = new List<GameMobsGroupControllerBase>(16);

		// Token: 0x04000C43 RID: 3139
		private string cachedName;

		// Token: 0x04000C44 RID: 3140
		private ILocationChunk chunkPrototype;

		// Token: 0x04000C45 RID: 3141
		private IGameLocation currentLocation;

		// Token: 0x04000C46 RID: 3142
		private int chunkID;

		// Token: 0x04000C47 RID: 3143
		private ILocationChunkVisitorsWatcher visitorsWatcher;

		// Token: 0x04000C48 RID: 3144
		private float chunkGridCellSize;

		// Token: 0x04000C49 RID: 3145
		private Vector2 localCenterOffset;

		// Token: 0x04000C4A RID: 3146
		[NonSerialized]
		private List<NavMeshBuildSource> navMeshBuildSources;

		// Token: 0x04000C4B RID: 3147
		private NavMeshDataInstance[] localNavMeshDataInstances;

		// Token: 0x04000C4C RID: 3148
		private bool isVisible;

		// Token: 0x04000C4D RID: 3149
		private bool isExplored;

		// Token: 0x04000C4E RID: 3150
		[NonSerialized]
		private bool isInstantiated;

		// Token: 0x04000C4F RID: 3151
		private bool isInitialized;

		// Token: 0x04000C50 RID: 3152
		[NonSerialized]
		private bool isBoundsDataCalculated;

		// Token: 0x04000C51 RID: 3153
		private bool isDestroyed;

		// Token: 0x04000C52 RID: 3154
		private Grid grid;

		// Token: 0x04000C53 RID: 3155
		private LocationChunkMobsGridController mobsGrid;

		// Token: 0x020004F5 RID: 1269
		public enum TypeID
		{
			// Token: 0x04001A7A RID: 6778
			Undefined = -1,
			// Token: 0x04001A7B RID: 6779
			StartChunk,
			// Token: 0x04001A7C RID: 6780
			BattleChunk,
			// Token: 0x04001A7D RID: 6781
			HealingChunk,
			// Token: 0x04001A7E RID: 6782
			RewardChunk,
			// Token: 0x04001A7F RID: 6783
			BossChunk,
			// Token: 0x04001A80 RID: 6784
			TrapChunk,
			// Token: 0x04001A81 RID: 6785
			TestChunk1 = 20,
			// Token: 0x04001A82 RID: 6786
			TestChunk2,
			// Token: 0x04001A83 RID: 6787
			TestChunk3,
			// Token: 0x04001A84 RID: 6788
			TestChunk4,
			// Token: 0x04001A85 RID: 6789
			SellerChunk = 30,
			// Token: 0x04001A86 RID: 6790
			ArenaDeadEnd = 60,
			// Token: 0x04001A87 RID: 6791
			RewardDeadEnd,
			// Token: 0x04001A88 RID: 6792
			EarlyAshMerch,
			// Token: 0x04001A89 RID: 6793
			LateAshMerch,
			// Token: 0x04001A8A RID: 6794
			Corridor = 100,
			// Token: 0x04001A8B RID: 6795
			DeadEnd
		}
	}
}
