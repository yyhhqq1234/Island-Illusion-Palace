using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.UnityExtensions;
using Game.Core;
using Game.Factories;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.GameScene;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000266 RID: 614
	public static class GameLocationGameplayExtensions
	{
		// Token: 0x06001479 RID: 5241 RVA: 0x00040A40 File Offset: 0x0003EC40
		public static bool IsWaitingForInitialization(this ILocationChunk locationChunk)
		{
			if (!locationChunk.IsNull())
			{
				if (!locationChunk.IsInitialized)
				{
					return true;
				}
				int count = locationChunk.EnvironmentObjects.Count;
				for (int i = 0; i < count; i++)
				{
					IProgressBasedAction progressBasedAction = locationChunk.EnvironmentObjects[i] as IProgressBasedAction;
					if (progressBasedAction != null && progressBasedAction.CurrentProgress < 1f)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600147A RID: 5242 RVA: 0x00040A9C File Offset: 0x0003EC9C
		public static bool HasType(this ILocationChunk locationChunk, LocationChunk.TypeID type)
		{
			return !locationChunk.IsNull() && locationChunk.LocationObjectType != null && locationChunk.LocationObjectType.Value == (int)type;
		}

		// Token: 0x0600147B RID: 5243 RVA: 0x00040AD4 File Offset: 0x0003ECD4
		public static bool IsDeadEndChunk(this ILocationChunk chunk)
		{
			LocationChunk.TypeID type = LocationChunk.TypeID.StartChunk;
			LocationChunk.TypeID type2 = LocationChunk.TypeID.BossChunk;
			return chunk != null && !chunk.HasType(type) && !chunk.HasType(type2) && (chunk.IsPrototype() ? chunk.Gateways : chunk.ChunkPrototype.Gateways).Count == 1;
		}

		// Token: 0x0600147C RID: 5244 RVA: 0x00040B20 File Offset: 0x0003ED20
		public static bool TryGetParentChunk(this ILocationChunk deadEndChunk, out ILocationChunk parentChunk)
		{
			if (!deadEndChunk.IsDeadEndChunk())
			{
				parentChunk = null;
				return false;
			}
			if (deadEndChunk.Gateways == null || deadEndChunk.Gateways.Count == 0)
			{
				parentChunk = deadEndChunk;
				return false;
			}
			parentChunk = deadEndChunk.Gateways[0].NextChunkGateway.CurrentLocationChunk;
			return true;
		}

		// Token: 0x0600147D RID: 5245 RVA: 0x00040B6D File Offset: 0x0003ED6D
		public static bool IsDeadEndChunk(this ILocationChunk chunk, ILocationChunk parentChunk)
		{
			return chunk != parentChunk && chunk.IsDeadEndChunk() && chunk.GetTransitionGateway(parentChunk, false) != null;
		}

		// Token: 0x0600147E RID: 5246 RVA: 0x00040B88 File Offset: 0x0003ED88
		public static PlayerSpawner AddPlayerSpawnerIfNeeded(this ILocationChunk locationChunk, PlayerBehaviour.ID playerID = PlayerBehaviour.ID.Player)
		{
			if (locationChunk == null)
			{
				return null;
			}
			Component component = (Component)locationChunk;
			PlayerSpawner playerSpawner = component.GetComponentInChildren<PlayerSpawner>(true);
			if (playerSpawner == null)
			{
				playerSpawner = new GameObject("_PlayerSpawner").AddComponent<PlayerSpawner>();
				playerSpawner.spawnOnStart = true;
				playerSpawner.SpawningInfo = new ObjectSpawnerBase<PlayerBehaviour.ID, PlayerBehaviour>.BaseSpawningInfoItem[]
				{
					new PlayerSpawner.PlayerSpawningInfoItem
					{
						ObjectID = playerID
					}
				};
				playerSpawner.transform.position = locationChunk.GetBoundsRect().center;
				playerSpawner.transform.parent = component.transform;
			}
			return playerSpawner;
		}

		// Token: 0x0600147F RID: 5247 RVA: 0x00040C14 File Offset: 0x0003EE14
		public static bool HasEnemyMobWithID(this ILocationChunk locationChunk, GameSessionManager sessionManager, MobBehaviour.ID mobID)
		{
			return locationChunk.HasMobWithID(mobID, new Predicate<GameMobFactions>(sessionManager.CurrentGameRules.IsPlayerEnemyFaction));
		}

		// Token: 0x06001480 RID: 5248 RVA: 0x00040C30 File Offset: 0x0003EE30
		public static bool HasMobWithID(this ILocationChunk locationChunk, MobBehaviour.ID mobID, Predicate<GameMobFactions> factionFilter)
		{
			if (locationChunk.IsNull())
			{
				return false;
			}
			foreach (ILocationChunkVisitor locationChunkVisitor in locationChunk.Visitors)
			{
				MobBehaviour mobBehaviour = locationChunkVisitor as MobBehaviour;
				if (mobBehaviour != null && mobBehaviour.IsAlive() && (factionFilter == null || factionFilter(mobBehaviour.Faction)))
				{
					return true;
				}
			}
			Func<ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem, bool> <>9__0;
			foreach (ILocationObject locationObject in locationChunk.EnvironmentObjects)
			{
				MobBehaviourSpawner mobBehaviourSpawner = locationObject as MobBehaviourSpawner;
				if (mobBehaviourSpawner != null)
				{
					bool result;
					if (mobBehaviourSpawner.RemainingSpawningCount > 0)
					{
						IEnumerable<ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem> spawningInfo = mobBehaviourSpawner.SpawningInfo;
						Func<ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem m) => m.ObjectID == mobID));
						}
						result = spawningInfo.Any(predicate);
					}
					else
					{
						result = false;
					}
					return result;
				}
			}
			return false;
		}

		// Token: 0x06001481 RID: 5249 RVA: 0x00040D48 File Offset: 0x0003EF48
		public static int GetMobsCount(this ILocationChunk locationChunk, bool countUnspawned, bool countMinorMobs, Predicate<GameMobFactions> factionFilter)
		{
			int num = 0;
			if (!locationChunk.IsNull())
			{
				foreach (ILocationChunkVisitor locationChunkVisitor in locationChunk.Visitors)
				{
					if (countMinorMobs || !locationChunkVisitor.IsMinorMobVisitor())
					{
						BaseGameMob baseGameMob = locationChunkVisitor as BaseGameMob;
						if (baseGameMob != null && baseGameMob.IsAlive() && GameLocationGameplayExtensions.<GetMobsCount>g__IsTargetFaction|9_0(baseGameMob.Faction, factionFilter))
						{
							num++;
						}
					}
				}
				if (countUnspawned)
				{
					foreach (ILocationObject locationObject in locationChunk.EnvironmentObjects)
					{
						IGameMobsSpawner gameMobsSpawner = locationObject as IGameMobsSpawner;
						if (gameMobsSpawner != null && (countMinorMobs || !gameMobsSpawner.SpawnEnvironmentMobs) && GameLocationGameplayExtensions.<GetMobsCount>g__IsTargetFaction|9_0(gameMobsSpawner.GroupOwner, factionFilter))
						{
							num += Math.Max(gameMobsSpawner.RemainingSpawningCount, 0);
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06001482 RID: 5250 RVA: 0x00040E44 File Offset: 0x0003F044
		public static int GetEnemyMobsCount(this ILocationChunk locationChunk, GameSessionRules sessionRules, bool countUnspawnedMobs = true, bool countMinorMobs = false)
		{
			return locationChunk.GetMobsCount(countUnspawnedMobs, countMinorMobs, new Predicate<GameMobFactions>(sessionRules.IsPlayerEnemyFaction));
		}

		// Token: 0x06001483 RID: 5251 RVA: 0x00040E5A File Offset: 0x0003F05A
		public static int GetEnemyMobsCount(this ILocationChunk locationChunk, GameSessionManager sessionManager, bool countUnspawnedMobs = true, bool countMinorMobs = false)
		{
			return locationChunk.GetMobsCount(countUnspawnedMobs, countMinorMobs, new Predicate<GameMobFactions>(sessionManager.CurrentGameRules.IsPlayerEnemyFaction));
		}

		// Token: 0x06001484 RID: 5252 RVA: 0x00040E78 File Offset: 0x0003F078
		public static bool IsLocationObject(this ILocationObject locationObject, LocationObjectType type)
		{
			return !locationObject.IsNull() && locationObject.LocationObjectType != null && (locationObject.LocationObjectType.Value & (int)type) != 0;
		}

		// Token: 0x06001485 RID: 5253 RVA: 0x00040EB2 File Offset: 0x0003F0B2
		public static bool IsPlayerVisitor(this ILocationChunkVisitor chunkVisitor)
		{
			return chunkVisitor.IsLocationObject(LocationObjectType.Player);
		}

		// Token: 0x06001486 RID: 5254 RVA: 0x00040EBB File Offset: 0x0003F0BB
		public static bool IsPlayerMobVisitor(this ILocationChunkVisitor chunkVisitor)
		{
			return chunkVisitor.IsLocationObject(LocationObjectType.PlayerMob);
		}

		// Token: 0x06001487 RID: 5255 RVA: 0x00040EC4 File Offset: 0x0003F0C4
		public static bool IsMinorMobVisitor(this ILocationChunkVisitor chunkVisitor)
		{
			return chunkVisitor.IsLocationObject(LocationObjectType.MinorMob);
		}

		// Token: 0x06001488 RID: 5256 RVA: 0x00040ECE File Offset: 0x0003F0CE
		public static bool IsEnemyMobVisitor(this ILocationChunkVisitor chunkVisitor, GameSessionManager sessionManager)
		{
			return !sessionManager.IsNull() && sessionManager.IsEnemyMob(chunkVisitor as BaseGameMob);
		}

		// Token: 0x06001489 RID: 5257 RVA: 0x00040EE8 File Offset: 0x0003F0E8
		public static ILocationChunk ForceGetCurrentChunk(this ILocationChunkVisitor chunkVisitor)
		{
			if (chunkVisitor == null)
			{
				return null;
			}
			if (chunkVisitor.CurrentLocationChunk != null)
			{
				return chunkVisitor.CurrentLocationChunk;
			}
			GameSceneManager gameSceneManager = ((GameBehaviourBase)chunkVisitor).CurrentGame.Services.Get<GameSceneManager>();
			GameLocation gameLocation = (gameSceneManager != null) ? gameSceneManager.GeneratedLocation : null;
			MonoBehaviour monoBehaviour = chunkVisitor as MonoBehaviour;
			Collider2D collider2D;
			if (monoBehaviour != null && monoBehaviour.TryGetComponent<Collider2D>(out collider2D))
			{
				Bounds bounds = collider2D.bounds;
				Vector2 vector = bounds.extents;
				return gameLocation.GetIntersectedLocationChunk(bounds.center, Mathf.Max(vector.x, vector.y));
			}
			return gameLocation.GetLocationChunkAtPoint(chunkVisitor.Position, false);
		}

		// Token: 0x0600148A RID: 5258 RVA: 0x00040F88 File Offset: 0x0003F188
		public static void RemoveFromAllChunks(this ILocationChunkVisitor chunkVisitor, ILocationChunk lastChunk)
		{
			if (chunkVisitor == null)
			{
				return;
			}
			ILocationChunk currentLocationChunk = chunkVisitor.CurrentLocationChunk;
			if (lastChunk != null)
			{
				lastChunk.RemoveVisitor(chunkVisitor);
			}
			if (lastChunk == currentLocationChunk)
			{
				return;
			}
			if (currentLocationChunk != null)
			{
				currentLocationChunk.RemoveVisitor(chunkVisitor);
			}
		}

		// Token: 0x0600148B RID: 5259 RVA: 0x00040FB9 File Offset: 0x0003F1B9
		public static bool HasPassageToChunk(this ILocationChunk currentChunk, ILocationChunk targetChunk)
		{
			return currentChunk == targetChunk || currentChunk.GetTransitionGateway(targetChunk, false) != null;
		}

		// Token: 0x0600148C RID: 5260 RVA: 0x00040FCC File Offset: 0x0003F1CC
		public static bool HasPassageToChunk(this ILocationChunk currentChunk, Vector3 targetPoint, out ILocationChunk targetChunk)
		{
			if (currentChunk == null)
			{
				targetChunk = null;
				return false;
			}
			if (currentChunk.ContainsPoint(targetPoint))
			{
				targetChunk = currentChunk;
				return true;
			}
			targetChunk = ((BaseLocation)currentChunk.CurrentLocation).GetLocationChunkAtPoint(targetPoint, false);
			return currentChunk.HasPassageToChunk(targetChunk);
		}

		// Token: 0x0600148D RID: 5261 RVA: 0x0004100A File Offset: 0x0003F20A
		public static bool IsHorizontalGateway(this ILocationChunkGateway gateway)
		{
			return gateway != null && Mathf.Abs(gateway.TransitionDirection.x) > 0f;
		}

		// Token: 0x0600148E RID: 5262 RVA: 0x00041028 File Offset: 0x0003F228
		public static bool IsEnterGateway(this ILocationChunkGateway gateway)
		{
			return gateway != null && gateway.CurrentLocationChunk.ChunkID > gateway.GetNextChunk().ChunkID;
		}

		// Token: 0x0600148F RID: 5263 RVA: 0x00041047 File Offset: 0x0003F247
		public static bool IsExitGateway(this ILocationChunkGateway gateway)
		{
			return gateway != null && gateway.CurrentLocationChunk.ChunkID < gateway.GetNextChunk().ChunkID;
		}

		// Token: 0x06001490 RID: 5264 RVA: 0x00041066 File Offset: 0x0003F266
		[CompilerGenerated]
		internal static bool <GetMobsCount>g__IsTargetFaction|9_0(GameMobFactions faction, Predicate<GameMobFactions> filter)
		{
			return filter == null || filter(faction);
		}

		// Token: 0x020004E5 RID: 1253
		public sealed class WaitForChunkInitialization : CustomYieldInstruction
		{
			// Token: 0x170007A5 RID: 1957
			// (get) Token: 0x06002593 RID: 9619 RVA: 0x00074EAA File Offset: 0x000730AA
			public override bool keepWaiting
			{
				get
				{
					return this.targetLocationChunk.IsWaitingForInitialization();
				}
			}

			// Token: 0x06002594 RID: 9620 RVA: 0x00074EB7 File Offset: 0x000730B7
			public WaitForChunkInitialization(ILocationChunk targetLocationChunk)
			{
				this.targetLocationChunk = targetLocationChunk;
			}

			// Token: 0x04001A33 RID: 6707
			private readonly ILocationChunk targetLocationChunk;
		}
	}
}
