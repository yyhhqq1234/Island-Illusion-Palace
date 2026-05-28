using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.CollectionsExtensions;
using Common.UnityExtensions;
using Game.BundlesCache;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Unliving.Gameplay;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000269 RID: 617
	[Serializable]
	public sealed class GameLocationGenerator : IGameLocationGenerator<GameLocation>, IGameLocationGenerator
	{
		// Token: 0x060014A0 RID: 5280 RVA: 0x0004151F File Offset: 0x0003F71F
		private static bool IsValidGateway(ILocationChunkGateway gateway)
		{
			return !gateway.IsNull() && !gateway.CurrentLocationChunk.IsNull();
		}

		// Token: 0x060014A1 RID: 5281 RVA: 0x00041539 File Offset: 0x0003F739
		private static bool IsConnectedGateway(ILocationChunkGateway gateway)
		{
			return GameLocationGenerator.IsValidGateway(gateway) && !gateway.NextChunkGateway.IsNull();
		}

		// Token: 0x060014A2 RID: 5282 RVA: 0x00041553 File Offset: 0x0003F753
		private static bool HaveCompatibleDirections(ILocationChunkGateway gateway0, ILocationChunkGateway gateway1)
		{
			return Vector2.Dot(gateway0.TransitionDirection, gateway1.TransitionDirection) < -0.9f;
		}

		// Token: 0x060014A3 RID: 5283 RVA: 0x0004156D File Offset: 0x0003F76D
		private static bool CanBeConnected(ILocationChunkGateway gateway0, ILocationChunkGateway gateway1)
		{
			return !GameLocationGenerator.IsConnectedGateway(gateway0) && !GameLocationGenerator.IsConnectedGateway(gateway1) && GameLocationGenerator.HaveCompatibleDirections(gateway0, gateway1);
		}

		// Token: 0x060014A4 RID: 5284 RVA: 0x00041588 File Offset: 0x0003F788
		private static Vector2 GetGatewayEdgePoint(ILocationChunkGateway gateway, float offset = 0f)
		{
			Vector2 transitionDirection = gateway.TransitionDirection;
			float num = Vector2.Dot(gateway.WorldSize, transitionDirection) * 0.5f;
			if (num < 0f)
			{
				num = -num;
			}
			return gateway.Position + (num + offset) * transitionDirection;
		}

		// Token: 0x060014A5 RID: 5285 RVA: 0x000415D0 File Offset: 0x0003F7D0
		private static ArraySegment<ILocationChunkGateway> CopyGateways(IList<ILocationChunkGateway> sourceList, bool shuffle)
		{
			int count = sourceList.Count;
			sourceList.CopyTo(GameLocationGenerator.GatewaysBuffer, 0);
			if (shuffle)
			{
				GameLocationGenerator.GatewaysBuffer.Shuffle(count);
			}
			return new ArraySegment<ILocationChunkGateway>(GameLocationGenerator.GatewaysBuffer, 0, count);
		}

		// Token: 0x060014A6 RID: 5286 RVA: 0x0004160A File Offset: 0x0003F80A
		private static void DeactivateChunk(ILocationChunk chunk)
		{
			if (!chunk.IsNull())
			{
				Component component = chunk as Component;
				if (component != null)
				{
					component.gameObject.SetActive(false);
				}
				if (chunk.IsVisible)
				{
					chunk.IsVisible = false;
				}
			}
		}

		// Token: 0x060014A7 RID: 5287 RVA: 0x0004163C File Offset: 0x0003F83C
		private static void MergeChunkComponents(ILocationChunk chunkToMerge, ILocationChunk mainChunk)
		{
			Component component = chunkToMerge as Component;
			if (component != null)
			{
				Component component2 = mainChunk as Component;
				if (component2 != null)
				{
					Grid componentInChildren = component.GetComponentInChildren<Grid>();
					component.transform.parent = component2.transform;
					if (componentInChildren != null)
					{
						Grid componentInChildren2 = component2.GetComponentInChildren<Grid>();
						if (componentInChildren2 != null)
						{
							string str = "_" + component.name;
							Transform transform = componentInChildren.transform;
							while (transform.childCount != 0)
							{
								Transform child = transform.GetChild(0);
								child.parent = componentInChildren2.transform;
								GameObject gameObject = child.gameObject;
								gameObject.name += str;
							}
							UnityEngine.Object.Destroy(componentInChildren.gameObject);
						}
					}
				}
				UnityEngine.Object.Destroy(component);
			}
		}

		// Token: 0x060014A8 RID: 5288 RVA: 0x000416F8 File Offset: 0x0003F8F8
		private static Vector2 SnapToGrid(Vector2 point, float cellSize)
		{
			return new Vector2
			{
				x = Mathf.Round(point.x / cellSize),
				y = Mathf.Round(point.y / cellSize)
			};
		}

		// Token: 0x060014A9 RID: 5289 RVA: 0x00041738 File Offset: 0x0003F938
		private static void BuildNavMeshTransitionAreas(IGameLocation location, string transitionNavMeshAgentType, float sizeAddition = 0.5f)
		{
			IReadOnlyList<ILocationChunk> chunks = location.Chunks;
			HashSet<ILocationChunkGateway> hashSet = new HashSet<ILocationChunkGateway>();
			for (int i = 0; i < chunks.Count; i++)
			{
				IList<ILocationChunkGateway> gateways = chunks[i].Gateways;
				for (int j = 0; j < gateways.Count; j++)
				{
					ILocationChunkGateway locationChunkGateway = gateways[j];
					ILocationChunkGateway nextChunkGateway = locationChunkGateway.NextChunkGateway;
					if (nextChunkGateway != null && hashSet.Add(locationChunkGateway) && hashSet.Add(nextChunkGateway))
					{
						Bounds bounds = locationChunkGateway.GetBounds();
						bounds.Encapsulate(nextChunkGateway.GetBounds());
						Vector2 transitionDirection = locationChunkGateway.TransitionDirection;
						transitionDirection.x = Mathf.Abs(transitionDirection.x);
						transitionDirection.y = Mathf.Abs(transitionDirection.y);
						bounds.size += transitionDirection * sizeAddition;
						GameObject gameObject = new GameObject("NavMeshLink");
						gameObject.layer = 2;
						Vector3 center = bounds.center;
						gameObject.transform.position = center;
						gameObject.AddComponent<BoxCollider2D>();
						LocationChunkTransitionAreaComponent locationChunkTransitionAreaComponent = gameObject.AddComponent<LocationChunkTransitionAreaComponent>();
						locationChunkTransitionAreaComponent.allowedAgentLayers = (-1 & ~PlayerFactory.PlayerLayerMask);
						locationChunkTransitionAreaComponent.agentTypeOverride = transitionNavMeshAgentType;
						locationChunkTransitionAreaComponent.SetSize(bounds.size);
						locationChunkTransitionAreaComponent.SetGateways(locationChunkGateway, nextChunkGateway);
						locationChunkGateway.TransitionArea = locationChunkTransitionAreaComponent;
						nextChunkGateway.TransitionArea = locationChunkTransitionAreaComponent;
					}
				}
			}
		}

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x060014AA RID: 5290 RVA: 0x000418AB File Offset: 0x0003FAAB
		public bool IsActive
		{
			get
			{
				return !string.IsNullOrWhiteSpace(this.chunksLabel.labelString) && this.iterations != null && this.iterations.Length != 0;
			}
		}

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x060014AB RID: 5291 RVA: 0x000418D3 File Offset: 0x0003FAD3
		// (set) Token: 0x060014AC RID: 5292 RVA: 0x000418DB File Offset: 0x0003FADB
		public Vector2 StartPosition { get; set; }

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x060014AD RID: 5293 RVA: 0x000418E4 File Offset: 0x0003FAE4
		// (set) Token: 0x060014AE RID: 5294 RVA: 0x000418EC File Offset: 0x0003FAEC
		public Transform GeneratedChunksRoot { get; set; }

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x060014AF RID: 5295 RVA: 0x000418F5 File Offset: 0x0003FAF5
		public int UsedSeed
		{
			get
			{
				return this.usedSeed;
			}
		}

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x060014B0 RID: 5296 RVA: 0x000418FD File Offset: 0x0003FAFD
		// (set) Token: 0x060014B1 RID: 5297 RVA: 0x00041905 File Offset: 0x0003FB05
		public float GenerationProgress { get; private set; }

		// Token: 0x060014B2 RID: 5298 RVA: 0x0004190E File Offset: 0x0003FB0E
		private void RemoveConnectedGateways()
		{
			this.freeGateways.RemoveAll(new Predicate<ILocationChunkGateway>(GameLocationGenerator.IsConnectedGateway));
		}

		// Token: 0x060014B3 RID: 5299 RVA: 0x00041928 File Offset: 0x0003FB28
		private void CreateGatewayIntersectionTestData(ILocationChunkGateway freeGateway)
		{
			GameLocationGenerator.<>c__DisplayClass67_0 CS$<>8__locals1 = new GameLocationGenerator.<>c__DisplayClass67_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.gatewayCollider = null;
			CS$<>8__locals1.gatewayCollider = new GameObject("_gatewayCollider")
			{
				layer = 31
			}.AddComponent<BoxCollider2D>();
			CS$<>8__locals1.gatewayCollider.size = freeGateway.WorldSize;
			CS$<>8__locals1.gatewayCollider.transform.position = freeGateway.Position;
			this.gatewayIntersectionTestData.Add(CS$<>8__locals1.gatewayCollider, freeGateway);
			freeGateway.Destroyed += CS$<>8__locals1.<CreateGatewayIntersectionTestData>g__RemoveTestData|0;
		}

		// Token: 0x060014B4 RID: 5300 RVA: 0x000419B8 File Offset: 0x0003FBB8
		private LocationChunkVisitorsWatcher CreateChunkVisitorsWatcher(GameLocationGenerator.PlacedChunkInfo chunkInfo, int visitorsLayers)
		{
			if (visitorsLayers == 0)
			{
				return null;
			}
			ILocationChunk chunk = chunkInfo.Chunk;
			string str = "Chunk";
			Component component = chunk as Component;
			GameObject gameObject = new GameObject(str + ((component != null) ? component.name : null) + "_areaCollider")
			{
				layer = 19
			};
			gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
			gameObject.transform.position = chunk.Position;
			List<Bounds> connectedChunkAreas = chunkInfo.ConnectedChunkAreas;
			bool flag = connectedChunkAreas.Count != 0;
			Bounds originalChunkBounds = chunkInfo.OriginalChunkBounds;
			originalChunkBounds.Expand(-0.2f);
			CompositeCollider2D compositeCollider2D = null;
			if (flag)
			{
				compositeCollider2D = gameObject.AddComponent<CompositeCollider2D>();
				compositeCollider2D.generationType = CompositeCollider2D.GenerationType.Manual;
				compositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;
				for (int i = 0; i < connectedChunkAreas.Count; i++)
				{
					Bounds area = connectedChunkAreas[i];
					Vector2 vector = originalChunkBounds.center - area.center;
					Vector3 b = (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) ? new Vector3(Mathf.Sign(vector.x) * 0.2f, 0f) : new Vector3(0f, Mathf.Sign(vector.y) * 0.2f);
					area.center += b;
					GameLocationGenerator.<CreateChunkVisitorsWatcher>g__CreateAreaCollider|68_0(gameObject, area, true);
				}
			}
			Collider2D collider2D = GameLocationGenerator.<CreateChunkVisitorsWatcher>g__CreateAreaCollider|68_0(gameObject, originalChunkBounds, flag);
			Collider2D collider2D2 = compositeCollider2D ?? collider2D;
			collider2D2.isTrigger = true;
			if (compositeCollider2D != null)
			{
				compositeCollider2D.GenerateGeometry();
			}
			LocationChunkVisitorsWatcher locationChunkVisitorsWatcher = gameObject.AddComponent<LocationChunkVisitorsWatcher>();
			locationChunkVisitorsWatcher.AreaCollider = collider2D2;
			locationChunkVisitorsWatcher.observableLayers = visitorsLayers;
			chunk.VisitorsWatcher = locationChunkVisitorsWatcher;
			return locationChunkVisitorsWatcher;
		}

		// Token: 0x060014B5 RID: 5301 RVA: 0x00041B6C File Offset: 0x0003FD6C
		private bool IsFreePlacementArea(Vector2 rectToPlaceMin, Vector2 rectToPlaceMax, bool checkRectIntersection)
		{
			if (checkRectIntersection)
			{
				for (int i = 0; i < this.placedChunks.Count; i++)
				{
					Vector2 vector;
					Vector2 vector2;
					this.placedChunks[i].GetChunkMinMaxPoints(out vector, out vector2);
					if (vector2.x > rectToPlaceMin.x && vector.x < rectToPlaceMax.x && vector2.y > rectToPlaceMin.y && vector.y < rectToPlaceMax.y)
					{
						return false;
					}
				}
			}
			return !Physics2D.OverlapArea(rectToPlaceMin, rectToPlaceMax, -2146959360);
		}

		// Token: 0x060014B6 RID: 5302 RVA: 0x00041BF8 File Offset: 0x0003FDF8
		private Vector2? GetChunkPlacementPosition(ILocationChunk chunkPrototype, ILocationChunkGateway freeGateway, bool checkChunksIntersection = true, bool ignoreAnyIntersection = false)
		{
			if (freeGateway.IsNull() || freeGateway.CurrentLocationChunk.IsNull())
			{
				return null;
			}
			bool flag = chunkPrototype.IsDeadEndChunk();
			if (!flag && !chunkPrototype.IsConnectingChunk && freeGateway.CurrentLocationChunk.Depth < this.averageChunksDepth)
			{
				return null;
			}
			IList<ILocationChunkGateway> gateways = chunkPrototype.Gateways;
			float num = Mathf.Clamp(this.chunksIntersectionCheckAccuracy, 0.1f, 1f);
			Vector2 position = chunkPrototype.Position;
			Vector2 worldSize = chunkPrototype.WorldSize;
			Vector2 b = 0.5f * num * worldSize;
			Vector2 b2 = freeGateway.TransitionDirection * 0.1f;
			Vector2 gatewayEdgePoint = GameLocationGenerator.GetGatewayEdgePoint(freeGateway, 0f);
			Vector2? result = null;
			if (gateways.Count != 0)
			{
				int i = 0;
				while (i < gateways.Count)
				{
					ILocationChunkGateway locationChunkGateway = gateways[i];
					if (flag)
					{
						goto IL_F4;
					}
					int? gatewayType = locationChunkGateway.GatewayType;
					int num2 = 2;
					if (!(gatewayType.GetValueOrDefault() == num2 & gatewayType != null))
					{
						goto IL_F4;
					}
					IL_165:
					i++;
					continue;
					IL_F4:
					if (!GameLocationGenerator.CanBeConnected(locationChunkGateway, freeGateway))
					{
						goto IL_165;
					}
					Vector2 gatewayEdgePoint2 = GameLocationGenerator.GetGatewayEdgePoint(locationChunkGateway, 0f);
					Vector2 a = position - gatewayEdgePoint2;
					Vector2 a2 = gatewayEdgePoint + a * num + b2;
					if (ignoreAnyIntersection || this.IsFreePlacementArea(a2 - b, a2 + b, checkChunksIntersection))
					{
						result = new Vector2?(position + (gatewayEdgePoint - gatewayEdgePoint2));
						break;
					}
					goto IL_165;
				}
				gateways.Clear();
			}
			return result;
		}

		// Token: 0x060014B7 RID: 5303 RVA: 0x00041D88 File Offset: 0x0003FF88
		private Vector2? FindChunkPlacementPosition(ILocationChunk chunkPrototype, in GameLocationGenerator.GatewaySearchArgs args)
		{
			IList<ILocationChunkGateway> list = args.GatewaysListOverride ?? this.freeGateways;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				ILocationChunkGateway locationChunkGateway = list[i];
				if (!args.ShouldBeSkipped(locationChunkGateway))
				{
					Vector2? chunkPlacementPosition = this.GetChunkPlacementPosition(chunkPrototype, locationChunkGateway, args.CheckChunkIntersections, false);
					if (chunkPlacementPosition != null)
					{
						return chunkPlacementPosition;
					}
				}
			}
			return null;
		}

		// Token: 0x060014B8 RID: 5304 RVA: 0x00041DF0 File Offset: 0x0003FFF0
		private void RegisterFreeGateway(ILocationChunkGateway gateway)
		{
			int? gatewayType = gateway.GatewayType;
			int num = 2;
			if (gatewayType.GetValueOrDefault() == num & gatewayType != null)
			{
				return;
			}
			this.freeGateways.Add(gateway);
		}

		// Token: 0x060014B9 RID: 5305 RVA: 0x00041E28 File Offset: 0x00040028
		private void ConnectGateways(ILocationChunk placedChunk)
		{
			ArraySegment<ILocationChunkGateway> arraySegment = GameLocationGenerator.CopyGateways(placedChunk.Gateways, true);
			int i = 0;
			while (i < arraySegment.Count)
			{
				ILocationChunkGateway locationChunkGateway = arraySegment.Array[i];
				Vector2 position = locationChunkGateway.Position;
				Vector2 transitionDirection = locationChunkGateway.TransitionDirection;
				float distance = locationChunkGateway.GetLocalSize().y * 0.9f;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(position, transitionDirection, distance, -2146959360);
				this.CreateGatewayIntersectionTestData(locationChunkGateway);
				ILocationChunkGateway locationChunkGateway2;
				if (!(raycastHit2D.collider != null) || !this.gatewayIntersectionTestData.TryGetValue(raycastHit2D.collider, out locationChunkGateway2) || !GameLocationGenerator.CanBeConnected(locationChunkGateway, locationChunkGateway2))
				{
					goto IL_BF;
				}
				Vector2 lhs = locationChunkGateway2.Position - position;
				lhs.Normalize();
				if (Vector2.Dot(lhs, transitionDirection) <= 0.98f)
				{
					goto IL_BF;
				}
				locationChunkGateway.NextChunkGateway = locationChunkGateway2;
				locationChunkGateway2.NextChunkGateway = locationChunkGateway;
				IL_C6:
				i++;
				continue;
				IL_BF:
				this.RegisterFreeGateway(locationChunkGateway);
				goto IL_C6;
			}
		}

		// Token: 0x060014BA RID: 5306 RVA: 0x00041F0C File Offset: 0x0004010C
		private bool TryMergeChunk(ILocationChunk chunkToMerge)
		{
			if (chunkToMerge.IsDeadEndChunk())
			{
				ILocationChunkGateway locationChunkGateway = chunkToMerge.Gateways[0];
				ILocationChunkGateway nextChunkGateway = locationChunkGateway.NextChunkGateway;
				ILocationChunk locationChunk = (nextChunkGateway != null) ? nextChunkGateway.CurrentLocationChunk : null;
				if (locationChunk != null)
				{
					Bounds bounds = chunkToMerge.GetBounds();
					Bounds bounds2 = locationChunk.GetBounds();
					bounds2.Encapsulate(bounds);
					locationChunk.UpdateBounds(bounds2);
					for (int i = 0; i < this.placedChunks.Count; i++)
					{
						if (locationChunk == this.placedChunks[i].Chunk)
						{
							this.placedChunks[i].ConnectedChunkAreas.Add(bounds);
							break;
						}
					}
					nextChunkGateway.Destroy();
					locationChunkGateway.Destroy();
					GameLocationGenerator.MergeChunkComponents(chunkToMerge, locationChunk);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060014BB RID: 5307 RVA: 0x00041FD0 File Offset: 0x000401D0
		private ILocationChunk PlaceChunk(ILocationChunk chunkPrototype, Vector2 targetPosition, bool forcePreventMerging = false, bool removeConnectedGateways = true)
		{
			ILocationChunk locationChunk = chunkPrototype.Clone();
			locationChunk.Position = targetPosition;
			this.ConnectGateways(locationChunk);
			if (forcePreventMerging || !this.TryMergeChunk(locationChunk))
			{
				Vector2 vector = targetPosition - this.StartPosition;
				locationChunk.Depth = Mathf.RoundToInt(Mathf.Abs(vector.x) + Mathf.Abs(vector.y));
				if (locationChunk.IsCoreChunk)
				{
					this.placedCoreChunksCount++;
					this.chunksDepthSum += locationChunk.Depth;
					this.averageChunksDepth = this.chunksDepthSum / this.placedCoreChunksCount;
				}
				Component component = locationChunk as Component;
				if (component != null)
				{
					component.transform.parent = this.GeneratedChunksRoot;
				}
				Physics2D.SyncTransforms();
				this.placedChunks.Add(new GameLocationGenerator.PlacedChunkInfo(locationChunk));
				if (removeConnectedGateways && this.placedChunks.Count > 1)
				{
					this.RemoveConnectedGateways();
				}
				GameLocationGenerator.DeactivateChunk(locationChunk);
				return locationChunk;
			}
			if (removeConnectedGateways)
			{
				this.RemoveConnectedGateways();
			}
			return null;
		}

		// Token: 0x060014BC RID: 5308 RVA: 0x000420C8 File Offset: 0x000402C8
		private ILocationChunk TryPlaceChunk(ILocationChunk chunkPrototype, in GameLocationGenerator.GatewaySearchArgs gatewaySearchArgs, bool forcePreventMerging = false)
		{
			Vector2? vector = this.FindChunkPlacementPosition(chunkPrototype, gatewaySearchArgs);
			if (vector != null)
			{
				return this.PlaceChunk(chunkPrototype, vector.Value, forcePreventMerging, true);
			}
			return null;
		}

		// Token: 0x060014BD RID: 5309 RVA: 0x000420FC File Offset: 0x000402FC
		private ILocationChunk TryPlaceChunk(ILocationChunk chunkPrototype, bool connectToCorridorsOnly = true)
		{
			GameLocationGenerator.GatewaySearchArgs gatewaySearchArgs = new GameLocationGenerator.GatewaySearchArgs(null, null, true);
			return this.TryPlaceChunk(chunkPrototype, gatewaySearchArgs, false);
		}

		// Token: 0x060014BE RID: 5310 RVA: 0x00042120 File Offset: 0x00040320
		private void CollectDeadEndGatewaysInfo(GameLocationGenerator.IterationInfo chunkIteration, ILocationChunk placedChunk)
		{
			if (!this.deadEndChunksPool.IsChunksLoadPathDefined)
			{
				return;
			}
			IList<ILocationChunkGateway> gateways = placedChunk.Gateways;
			for (int i = 0; i < gateways.Count; i++)
			{
				ILocationChunkGateway locationChunkGateway = gateways[i];
				int? gatewayType = locationChunkGateway.GatewayType;
				int num = 2;
				if (gatewayType.GetValueOrDefault() == num & gatewayType != null)
				{
					GameLocationGenerator.DeadEndChunkGatewayInfo item = new GameLocationGenerator.DeadEndChunkGatewayInfo(this.deadEndChunksPool, locationChunkGateway, chunkIteration.allowedDeadEndChunks);
					this.deadEndGatewaysInfo.Add(item);
				}
			}
		}

		// Token: 0x060014BF RID: 5311 RVA: 0x0004219C File Offset: 0x0004039C
		private async Task PlaceDeadEndChunks()
		{
			if (this.deadEndChunksSelector == null)
			{
				this.deadEndChunksSelector = new GameLocationGenerator.DeadEndChunksSelector(this);
			}
			if (this.deadEndGatewaysInfo.Count != 0)
			{
				await this.deadEndChunksPool.Initialize(this.deadEndGatewaysInfo.Count, this.cacheManager);
				for (int i = 0; i < this.deadEndGatewaysInfo.Count; i++)
				{
					GameLocationGenerator.DeadEndChunkGatewayInfo deadEndChunkGatewayInfo = this.deadEndGatewaysInfo[i];
					ILocationChunkGateway targetGateway = deadEndChunkGatewayInfo.TargetGateway;
					LocationChunk.TypeID[] allowedDeadEnds = deadEndChunkGatewayInfo.AllowedDeadEnds;
					this.deadEndChunksSelector.Prepare(targetGateway);
					ILocationChunk[] list;
					int num = this.deadEndChunksPool.SelectChunks(allowedDeadEnds, this.deadEndChunksSelector, out list);
					if (num != 0)
					{
						ILocationChunk randomItem = list.GetRandomItem(0, num);
						bool flag = randomItem.HasType(this.deadEndChunksPool.fallbackChunkType);
						this.PlaceChunk(randomItem, this.deadEndChunksSelector.GetDeadEndPlacementPosition(randomItem, targetGateway, flag).Value, !flag, true);
					}
					else
					{
						Debug.LogError(string.Format("Dead End placement failed for {0} {1} ({2}).", targetGateway, GameLocationGenerator.IsConnectedGateway(targetGateway), targetGateway.CurrentLocationChunk));
					}
				}
			}
			if (this.deadEndChunksPool.LoadedChunks == null)
			{
				if (!this.deadEndChunksPool.IsChunksLoadPathDefined)
				{
					this.deadEndChunksPool.chunksLabel = this.deadEndChunksLabel;
				}
				await this.deadEndChunksPool.Initialize(0, this.cacheManager);
			}
			ILocationChunk[] array;
			int num2 = this.deadEndChunksPool.SelectChunks(this.deadEndChunksPool.fallbackChunkType, null, out array);
			for (int j = 0; j < this.freeGateways.Count; j++)
			{
				ILocationChunkGateway deadEndGateway = this.freeGateways[j];
				for (int k = 0; k < num2; k++)
				{
					ILocationChunk locationChunk = array[k];
					Vector2? deadEndPlacementPosition = this.deadEndChunksSelector.GetDeadEndPlacementPosition(locationChunk, deadEndGateway, true);
					if (deadEndPlacementPosition != null)
					{
						this.PlaceChunk(locationChunk, deadEndPlacementPosition.Value, false, false);
						array.Shuffle(num2);
						break;
					}
				}
			}
			this.RemoveConnectedGateways();
		}

		// Token: 0x060014C0 RID: 5312 RVA: 0x000421E4 File Offset: 0x000403E4
		private void AddChunkComponents(ILocationChunk placedChunk, GameLocationGenerator.GenerationArgs generationArgs, out LocationChunkControllerBase chunkController)
		{
			chunkController = null;
			Component component = placedChunk as Component;
			if (component == null)
			{
				return;
			}
			LocationChunkMobsGridController mobsGrid = component.gameObject.AddComponent<LocationChunkMobsGridController>();
			LocationChunk locationChunk = placedChunk as LocationChunk;
			if (locationChunk != null)
			{
				locationChunk.MobsGrid = mobsGrid;
			}
			if (!placedChunk.IsDeadEndChunk())
			{
				if (placedChunk.HasType(LocationChunk.TypeID.BattleChunk))
				{
					GameLocationGenerator.<AddChunkComponents>g__AddAggressionController|80_0(component, generationArgs);
					EnemyLocationChunkLocker enemyLocationChunkLocker;
					if (generationArgs.LockBattleChunks && !component.TryGetComponent<EnemyLocationChunkLocker>(out enemyLocationChunkLocker))
					{
						component.gameObject.AddComponent<EnemyLocationChunkLocker>();
					}
				}
				if (placedChunk.HasType(LocationChunk.TypeID.BossChunk))
				{
					GameLocationGenerator.<AddChunkComponents>g__AddAggressionController|80_0(component, generationArgs);
					return;
				}
			}
			else if (generationArgs.LockBattleChunks)
			{
				IList<ILocationChunkGateway> gateways = placedChunk.Gateways;
				for (int i = 0; i < gateways.Count; i++)
				{
					Component component2 = gateways[i].GateController as Component;
					ChildChunkGatewayLocker childChunkGatewayLocker;
					if (component2 != null && !component2.TryGetComponent<ChildChunkGatewayLocker>(out childChunkGatewayLocker))
					{
						component2.gameObject.AddComponent<ChildChunkGatewayLocker>();
					}
				}
			}
		}

		// Token: 0x060014C1 RID: 5313 RVA: 0x000422BC File Offset: 0x000404BC
		private void SetLocationData(Vector2 boundsMin, Vector2 boundsMax)
		{
			Bounds bounds = new Bounds(this.StartPosition, default(Vector3));
			bounds.SetMinMax(boundsMin, boundsMax);
			ILocationChunk[] array = new ILocationChunk[this.placedChunks.Count];
			for (int i = 0; i < this.placedChunks.Count; i++)
			{
				array[i] = this.placedChunks[i].Chunk;
			}
			this.generatedLocation.SetData(array, bounds);
		}

		// Token: 0x060014C2 RID: 5314 RVA: 0x00042340 File Offset: 0x00040540
		private void DisposeNavMeshBuildSources()
		{
			for (int i = 0; i < this.navMeshBuildSources.Count; i++)
			{
				UnityEngine.Object sourceObject = this.navMeshBuildSources[i].sourceObject;
				if (sourceObject != null)
				{
					UnityEngine.Object.Destroy(sourceObject);
				}
			}
			this.navMeshBuildSources.Clear();
		}

		// Token: 0x060014C3 RID: 5315 RVA: 0x00042394 File Offset: 0x00040594
		public Task GenerateLocation(object args, Action<IGameLocation> callback)
		{
			GameLocationGenerator.<GenerateLocation>d__83 <GenerateLocation>d__;
			<GenerateLocation>d__.<>4__this = this;
			<GenerateLocation>d__.args = args;
			<GenerateLocation>d__.callback = callback;
			<GenerateLocation>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<GenerateLocation>d__.<>1__state = -1;
			AsyncTaskMethodBuilder <>t__builder = <GenerateLocation>d__.<>t__builder;
			<>t__builder.Start<GameLocationGenerator.<GenerateLocation>d__83>(ref <GenerateLocation>d__);
			return <GenerateLocation>d__.<>t__builder.Task;
		}

		// Token: 0x060014C4 RID: 5316 RVA: 0x000423E9 File Offset: 0x000405E9
		Task IGameLocationGenerator.GenerateLocation(object args, Action<IGameLocation> callback)
		{
			return this.GenerateLocation(args, callback);
		}

		// Token: 0x060014C5 RID: 5317 RVA: 0x000423F4 File Offset: 0x000405F4
		public void Cleanup()
		{
			this.DisposeNavMeshBuildSources();
			if (this.generatedNavMeshData != null && this.generatedNavMeshData.Length != 0)
			{
				foreach (NavMeshDataInstance navMeshDataInstance in this.generatedNavMeshData)
				{
					navMeshDataInstance.Remove();
				}
				this.generatedNavMeshData = null;
			}
			this.ClearChunksCache();
		}

		// Token: 0x060014C6 RID: 5318 RVA: 0x0004244C File Offset: 0x0004064C
		private void ClearChunksCache()
		{
			if (this.cacheManager.IsNull())
			{
				return;
			}
			this.cacheManager.ClearCache(this.chunksLabel);
			this.cacheManager.ClearCache(this.connectingChunksLabel);
			this.cacheManager.ClearCache(this.deadEndChunksPool.chunksLabel);
		}

		// Token: 0x060014C9 RID: 5321 RVA: 0x00042526 File Offset: 0x00040726
		[CompilerGenerated]
		internal static BoxCollider2D <CreateChunkVisitorsWatcher>g__CreateAreaCollider|68_0(GameObject targetObject, Bounds area, bool isComposite)
		{
			BoxCollider2D boxCollider2D = targetObject.AddComponent<BoxCollider2D>();
			boxCollider2D.offset = targetObject.transform.InverseTransformPoint(area.center);
			boxCollider2D.size = area.size;
			boxCollider2D.usedByComposite = isComposite;
			return boxCollider2D;
		}

		// Token: 0x060014CA RID: 5322 RVA: 0x00042564 File Offset: 0x00040764
		[CompilerGenerated]
		internal static void <AddChunkComponents>g__AddAggressionController|80_0(Component chunk, GameLocationGenerator.GenerationArgs args)
		{
			if (!args.cullMobAttackControllers)
			{
				return;
			}
			GameMobAICullingController gameMobAICullingController;
			if (!chunk.TryGetComponent<GameMobAICullingController>(out gameMobAICullingController))
			{
				gameMobAICullingController = chunk.gameObject.AddComponent<GameMobAICullingController>();
			}
			gameMobAICullingController.Initialize((ILocationChunk)chunk);
		}

		// Token: 0x060014CB RID: 5323 RVA: 0x0004259C File Offset: 0x0004079C
		[CompilerGenerated]
		internal static bool <GenerateLocation>g__ValidChunkSelector|83_0(ILocationChunk loadedChunk)
		{
			int? locationObjectType = loadedChunk.LocationObjectType;
			int num = 0;
			if (locationObjectType.GetValueOrDefault() >= num & locationObjectType != null)
			{
				locationObjectType = loadedChunk.LocationObjectType;
				num = 100;
				return locationObjectType.GetValueOrDefault() < num & locationObjectType != null;
			}
			return false;
		}

		// Token: 0x060014CC RID: 5324 RVA: 0x000425E7 File Offset: 0x000407E7
		[CompilerGenerated]
		internal static bool <GenerateLocation>g__ValidConnectingChunkSelector|83_1(ILocationChunk loadedChunk)
		{
			return loadedChunk.IsConnectingChunk;
		}

		// Token: 0x04000BF3 RID: 3059
		private const int IntersectionTestsLayer = 31;

		// Token: 0x04000BF4 RID: 3060
		private const int DeadEndGatewayType = 2;

		// Token: 0x04000BF5 RID: 3061
		private const int ChunkCollidersLayer = 19;

		// Token: 0x04000BF6 RID: 3062
		private const int IntersectionTestsMask = -2146959360;

		// Token: 0x04000BF7 RID: 3063
		private const string ChunkTransitionNavMeshAgentType = "TransitionAgent";

		// Token: 0x04000BF8 RID: 3064
		private static readonly ILocationChunk[] ChunksBuffer = new ILocationChunk[256];

		// Token: 0x04000BF9 RID: 3065
		private static readonly ILocationChunkGateway[] GatewaysBuffer = new ILocationChunkGateway[16];

		// Token: 0x04000BFD RID: 3069
		public GameObject testChunkPrefab;

		// Token: 0x04000BFE RID: 3070
		public bool forceLoadTestChunk;

		// Token: 0x04000BFF RID: 3071
		public AssetLabelReference chunksLabel;

		// Token: 0x04000C00 RID: 3072
		[HideInInspector]
		public AssetLabelReference connectingChunksLabel;

		// Token: 0x04000C01 RID: 3073
		[HideInInspector]
		public AssetLabelReference deadEndChunksLabel;

		// Token: 0x04000C02 RID: 3074
		[Space]
		public RandomChunksPool deadEndChunksPool;

		// Token: 0x04000C03 RID: 3075
		[Space]
		public GameLocation.TypeID locationType;

		// Token: 0x04000C04 RID: 3076
		public string levelID;

		// Token: 0x04000C05 RID: 3077
		[Range(0.1f, 1f)]
		public float chunksIntersectionCheckAccuracy = 0.5f;

		// Token: 0x04000C06 RID: 3078
		public GameLocationGenerator.IterationInfo[] iterations;

		// Token: 0x04000C07 RID: 3079
		[HideInInspector]
		public GameLocationGenerator.IterationInfo[] functionalDeadEndsIterations;

		// Token: 0x04000C08 RID: 3080
		private readonly List<ILocationChunkGateway> freeGateways = new List<ILocationChunkGateway>(256);

		// Token: 0x04000C09 RID: 3081
		private readonly List<GameLocationGenerator.PlacedChunkInfo> placedChunks = new List<GameLocationGenerator.PlacedChunkInfo>(128);

		// Token: 0x04000C0A RID: 3082
		private readonly List<GameLocationGenerator.DeadEndChunkGatewayInfo> deadEndGatewaysInfo = new List<GameLocationGenerator.DeadEndChunkGatewayInfo>(128);

		// Token: 0x04000C0B RID: 3083
		private readonly Dictionary<Collider2D, ILocationChunkGateway> gatewayIntersectionTestData = new Dictionary<Collider2D, ILocationChunkGateway>();

		// Token: 0x04000C0C RID: 3084
		private readonly List<NavMeshBuildSource> navMeshBuildSources = new List<NavMeshBuildSource>(256);

		// Token: 0x04000C0D RID: 3085
		private LocationChunksCollection mainChunksCollection;

		// Token: 0x04000C0E RID: 3086
		private ILocationChunk[] connectingChunkPrototypes;

		// Token: 0x04000C0F RID: 3087
		private GameLocationGenerator.DeadEndChunksSelector deadEndChunksSelector;

		// Token: 0x04000C10 RID: 3088
		private int placedCoreChunksCount;

		// Token: 0x04000C11 RID: 3089
		private int chunksDepthSum;

		// Token: 0x04000C12 RID: 3090
		private int averageChunksDepth;

		// Token: 0x04000C13 RID: 3091
		private NavMeshDataInstance[] generatedNavMeshData;

		// Token: 0x04000C14 RID: 3092
		[NonSerialized]
		private int usedSeed;

		// Token: 0x04000C15 RID: 3093
		private GameLocation generatedLocation;

		// Token: 0x04000C16 RID: 3094
		private IBundlesCacheManager cacheManager;

		// Token: 0x020004EA RID: 1258
		[Serializable]
		public sealed class IterationInfo
		{
			// Token: 0x0600259B RID: 9627 RVA: 0x0007500F File Offset: 0x0007320F
			public IterationInfo()
			{
				this.chunkType = LocationChunk.TypeID.StartChunk;
				this.chunkCount = 1;
				this.chunkLevel = 1;
			}

			// Token: 0x0600259C RID: 9628 RVA: 0x0007502C File Offset: 0x0007322C
			public int GetRandomChunks(LocationChunksCollection chunksCollection, ILocationChunk[] chunksBuffer, out int totalChunksCount, bool ignoreLevel = false, bool getAllChunks = false)
			{
				int num = (this.maxChunkCount > this.chunkCount) ? UnityEngine.Random.Range(this.chunkCount, this.maxChunkCount + 1) : this.chunkCount;
				if (num > 0)
				{
					int chunksLevel = ignoreLevel ? -1 : Mathf.Max(this.chunkLevel, 1);
					if (getAllChunks)
					{
						if ((totalChunksCount = chunksCollection.CopyChunksPrototypes((int)this.chunkType, chunksLevel, ref chunksBuffer, false, true)) > 0)
						{
							return num;
						}
					}
					else if ((num = chunksCollection.GetRandomChunksPrototypes(chunksBuffer, (int)this.chunkType, chunksLevel, num)) > 0)
					{
						totalChunksCount = num;
						return num;
					}
				}
				totalChunksCount = 0;
				return 0;
			}

			// Token: 0x04001A47 RID: 6727
			public LocationChunk.TypeID chunkType;

			// Token: 0x04001A48 RID: 6728
			[Range(1f, 10f)]
			public int chunkLevel;

			// Token: 0x04001A49 RID: 6729
			public int chunkCount;

			// Token: 0x04001A4A RID: 6730
			public int maxChunkCount;

			// Token: 0x04001A4B RID: 6731
			public LocationChunk.TypeID[] allowedDeadEndChunks;
		}

		// Token: 0x020004EB RID: 1259
		[Serializable]
		public sealed class GenerationArgs
		{
			// Token: 0x170007A6 RID: 1958
			// (get) Token: 0x0600259D RID: 9629 RVA: 0x000750B7 File Offset: 0x000732B7
			// (set) Token: 0x0600259E RID: 9630 RVA: 0x000750BF File Offset: 0x000732BF
			public float CorridorsTransitionThreshold0
			{
				get
				{
					return this._corridorsTransitionThreshold0;
				}
				set
				{
					this._corridorsTransitionThreshold0 = ConnectingChunkController.ClampTransitionThreshold(value);
				}
			}

			// Token: 0x170007A7 RID: 1959
			// (get) Token: 0x0600259F RID: 9631 RVA: 0x000750CD File Offset: 0x000732CD
			// (set) Token: 0x060025A0 RID: 9632 RVA: 0x000750D5 File Offset: 0x000732D5
			public float CorridorsTransitionThreshold1
			{
				get
				{
					return this._corridorsTransitionThreshold1;
				}
				set
				{
					this._corridorsTransitionThreshold1 = ConnectingChunkController.ClampTransitionThreshold(value);
				}
			}

			// Token: 0x170007A8 RID: 1960
			// (get) Token: 0x060025A1 RID: 9633 RVA: 0x000750E3 File Offset: 0x000732E3
			public bool LockBattleChunks
			{
				get
				{
					return this.lockBattleChunks && !this.lockCompletedChunks;
				}
			}

			// Token: 0x04001A4C RID: 6732
			public IBundlesCacheManager cacheManager;

			// Token: 0x04001A4D RID: 6733
			public int customSeed;

			// Token: 0x04001A4E RID: 6734
			public bool useCustomSeed;

			// Token: 0x04001A4F RID: 6735
			public LayerMask chunkVisitorsLayers = -1;

			// Token: 0x04001A50 RID: 6736
			public float chunksTransitionAreaSizeAddition = 0.5f;

			// Token: 0x04001A51 RID: 6737
			[SerializeField]
			[Range(0.1f, 0.5f)]
			private float _corridorsTransitionThreshold0 = 0.2f;

			// Token: 0x04001A52 RID: 6738
			[SerializeField]
			[Range(0.1f, 0.5f)]
			private float _corridorsTransitionThreshold1 = 0.2f;

			// Token: 0x04001A53 RID: 6739
			[Tooltip("Будут ли отключены контроллеры ИИ мобов, рейнджи которых находятся вне зоны видимости.")]
			public bool cullMobAttackControllers = true;

			// Token: 0x04001A54 RID: 6740
			public bool returnStrayedMobs = true;

			// Token: 0x04001A55 RID: 6741
			public bool generateAlwaysVisibleChunks;

			// Token: 0x04001A56 RID: 6742
			public bool lockCompletedChunks = true;

			// Token: 0x04001A57 RID: 6743
			[SerializeField]
			[FormerlySerializedAs("lockChunks")]
			private bool lockBattleChunks;

			// Token: 0x04001A58 RID: 6744
			public bool addPlayerSpawnerToTestChunk = true;
		}

		// Token: 0x020004EC RID: 1260
		private readonly struct PlacedChunkInfo
		{
			// Token: 0x170007A9 RID: 1961
			// (get) Token: 0x060025A3 RID: 9635 RVA: 0x00075154 File Offset: 0x00073354
			public List<Bounds> ConnectedChunkAreas
			{
				get
				{
					return this.connectedChunkAreas;
				}
			}

			// Token: 0x060025A4 RID: 9636 RVA: 0x0007515C File Offset: 0x0007335C
			public PlacedChunkInfo(ILocationChunk chunk)
			{
				this.Chunk = chunk;
				this.OriginalChunkBounds = chunk.GetBounds();
				this.connectedChunkAreas = new List<Bounds>(4);
			}

			// Token: 0x060025A5 RID: 9637 RVA: 0x00075180 File Offset: 0x00073380
			public void GetChunkMinMaxPoints(out Vector2 min, out Vector2 max)
			{
				min = this.OriginalChunkBounds.min;
				max = this.OriginalChunkBounds.max;
			}

			// Token: 0x04001A59 RID: 6745
			public readonly ILocationChunk Chunk;

			// Token: 0x04001A5A RID: 6746
			public readonly Bounds OriginalChunkBounds;

			// Token: 0x04001A5B RID: 6747
			private readonly List<Bounds> connectedChunkAreas;
		}

		// Token: 0x020004ED RID: 1261
		private readonly struct GatewaySearchArgs
		{
			// Token: 0x060025A6 RID: 9638 RVA: 0x000751BF File Offset: 0x000733BF
			public GatewaySearchArgs(IList<ILocationChunkGateway> gatewaysListOverride, Predicate<ILocationChunkGateway> gatewaysFilter, bool checkChunkIntersections)
			{
				this.GatewaysListOverride = gatewaysListOverride;
				this.GatewaysFilter = gatewaysFilter;
				this.CheckChunkIntersections = checkChunkIntersections;
			}

			// Token: 0x060025A7 RID: 9639 RVA: 0x000751D6 File Offset: 0x000733D6
			public bool ShouldBeSkipped(ILocationChunkGateway gateway)
			{
				return this.GatewaysFilter != null && !this.GatewaysFilter(gateway);
			}

			// Token: 0x04001A5C RID: 6748
			public static readonly GameLocationGenerator.GatewaySearchArgs Default = new GameLocationGenerator.GatewaySearchArgs(null, null, true);

			// Token: 0x04001A5D RID: 6749
			public readonly IList<ILocationChunkGateway> GatewaysListOverride;

			// Token: 0x04001A5E RID: 6750
			public readonly Predicate<ILocationChunkGateway> GatewaysFilter;

			// Token: 0x04001A5F RID: 6751
			public readonly bool CheckChunkIntersections;
		}

		// Token: 0x020004EE RID: 1262
		private sealed class ChunkLevelGatewaysSortingComparer : IComparer<ILocationChunkGateway>
		{
			// Token: 0x170007AA RID: 1962
			// (set) Token: 0x060025A9 RID: 9641 RVA: 0x00075200 File Offset: 0x00073400
			public int ChunkToPlaceLevel
			{
				set
				{
					this.chunkToPlaceLevel = value;
				}
			}

			// Token: 0x170007AB RID: 1963
			// (set) Token: 0x060025AA RID: 9642 RVA: 0x00075209 File Offset: 0x00073409
			public ILocationChunk ChunkToPlace
			{
				set
				{
					this.chunkToPlaceLevel = value.Level;
				}
			}

			// Token: 0x060025AB RID: 9643 RVA: 0x00075217 File Offset: 0x00073417
			private int GetLevelDistance(ILocationChunk chunk)
			{
				return Mathf.Abs(chunk.Level - this.chunkToPlaceLevel);
			}

			// Token: 0x060025AC RID: 9644 RVA: 0x0007522C File Offset: 0x0007342C
			public int Compare(ILocationChunkGateway gateway0, ILocationChunkGateway gateway1)
			{
				if (this.sortInAscendingOrder)
				{
					return this.GetLevelDistance(gateway0.CurrentLocationChunk).CompareTo(this.GetLevelDistance(gateway1.CurrentLocationChunk));
				}
				return this.GetLevelDistance(gateway1.CurrentLocationChunk).CompareTo(this.GetLevelDistance(gateway0.CurrentLocationChunk));
			}

			// Token: 0x04001A60 RID: 6752
			public bool sortInAscendingOrder;

			// Token: 0x04001A61 RID: 6753
			private int chunkToPlaceLevel;
		}

		// Token: 0x020004EF RID: 1263
		private sealed class DeadEndChunksSelector : RandomChunksPool.ChunksSelector
		{
			// Token: 0x060025AE RID: 9646 RVA: 0x0007528A File Offset: 0x0007348A
			private bool HasValidLevel(LocationChunk gameChunk)
			{
				return this.parentChunkLevel >= gameChunk.MinAllowedParentChunkLevel && this.parentChunkLevel <= gameChunk.MaxAllowedParentChunkLevel;
			}

			// Token: 0x060025AF RID: 9647 RVA: 0x000752AD File Offset: 0x000734AD
			public DeadEndChunksSelector(GameLocationGenerator locationGenerator)
			{
				this.locationGenerator = locationGenerator;
			}

			// Token: 0x060025B0 RID: 9648 RVA: 0x000752BC File Offset: 0x000734BC
			public void Prepare(ILocationChunkGateway targetGateway)
			{
				this.targetGateway = targetGateway;
				this.parentChunkLevel = targetGateway.CurrentLocationChunk.Level;
			}

			// Token: 0x060025B1 RID: 9649 RVA: 0x000752D6 File Offset: 0x000734D6
			public Vector2? GetDeadEndPlacementPosition(ILocationChunk deadEndPrototype, ILocationChunkGateway deadEndGateway, bool isFallbackDeadEnd)
			{
				return this.locationGenerator.GetChunkPlacementPosition(deadEndPrototype, deadEndGateway, true, isFallbackDeadEnd);
			}

			// Token: 0x060025B2 RID: 9650 RVA: 0x000752E8 File Offset: 0x000734E8
			public override bool CanBeSelected(bool isFallbackIteration, ILocationChunk chunk)
			{
				if (!isFallbackIteration)
				{
					LocationChunk locationChunk = chunk as LocationChunk;
					if (locationChunk != null && !this.HasValidLevel(locationChunk))
					{
						return false;
					}
				}
				return this.GetDeadEndPlacementPosition(chunk, this.targetGateway, isFallbackIteration) != null;
			}

			// Token: 0x04001A62 RID: 6754
			private readonly GameLocationGenerator locationGenerator;

			// Token: 0x04001A63 RID: 6755
			private ILocationChunkGateway targetGateway;

			// Token: 0x04001A64 RID: 6756
			private int parentChunkLevel;
		}

		// Token: 0x020004F0 RID: 1264
		private readonly struct DeadEndChunkGatewayInfo
		{
			// Token: 0x060025B3 RID: 9651 RVA: 0x00075324 File Offset: 0x00073524
			public DeadEndChunkGatewayInfo(RandomChunksPool deadEndChunksPool, ILocationChunkGateway targetGateway, LocationChunk.TypeID[] allowedChunkDeadEnds)
			{
				if (GameLocationGenerator.DeadEndChunkGatewayInfo.DefaultAllowedDeadEnds == null)
				{
					GameLocationGenerator.DeadEndChunkGatewayInfo.DefaultAllowedDeadEnds = new LocationChunk.TypeID[]
					{
						deadEndChunksPool.fallbackChunkType
					};
				}
				this.TargetGateway = targetGateway;
				if (allowedChunkDeadEnds != null && allowedChunkDeadEnds.Length != 0)
				{
					this.AllowedDeadEnds = new LocationChunk.TypeID[allowedChunkDeadEnds.Length];
					Array.Copy(allowedChunkDeadEnds, this.AllowedDeadEnds, allowedChunkDeadEnds.Length);
					return;
				}
				this.AllowedDeadEnds = GameLocationGenerator.DeadEndChunkGatewayInfo.DefaultAllowedDeadEnds;
			}

			// Token: 0x04001A65 RID: 6757
			public static LocationChunk.TypeID[] DefaultAllowedDeadEnds;

			// Token: 0x04001A66 RID: 6758
			public readonly ILocationChunkGateway TargetGateway;

			// Token: 0x04001A67 RID: 6759
			public readonly LocationChunk.TypeID[] AllowedDeadEnds;
		}
	}
}
