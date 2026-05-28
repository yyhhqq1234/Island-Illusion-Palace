using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Factories;
using Common.PivotGroup;
using Common.UnityExtensions;
using Game.Core;
using Game.Factories;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs.Animation;
using Unliving.Mobs.Motion;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001C5 RID: 453
	public sealed class MobBehaviourSpawner : ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>, IGameMobsSpawner, ILocationObject, IProgressBasedAction
	{
		// Token: 0x06000DF8 RID: 3576 RVA: 0x0002C694 File Offset: 0x0002A894
		private static Vector2 GetRandomOffset(IGameMob spawnedMob)
		{
			Vector2 a = new Vector2
			{
				x = UnityEngine.Random.value * 2f - 1f,
				y = UnityEngine.Random.value * 2f - 1f
			};
			a.Normalize();
			return a * spawnedMob.Radius;
		}

		// Token: 0x06000DF9 RID: 3577 RVA: 0x0002C6EE File Offset: 0x0002A8EE
		public static void AddRandomOffset(IGameMob spawnedMob)
		{
			spawnedMob.Position += MobBehaviourSpawner.GetRandomOffset(spawnedMob);
		}

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x0002C707 File Offset: 0x0002A907
		// (set) Token: 0x06000DFB RID: 3579 RVA: 0x0002C70F File Offset: 0x0002A90F
		public MobBehaviourSpawner.MobSpawningInfoItem[] InitialSpawningInfo
		{
			get
			{
				return this.initialSpawningInfo;
			}
			set
			{
				this.initialSpawningInfo = value;
			}
		}

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x06000DFC RID: 3580 RVA: 0x0002C718 File Offset: 0x0002A918
		public int MinMobCount
		{
			get
			{
				return this._minMobCount;
			}
		}

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000DFD RID: 3581 RVA: 0x0002C720 File Offset: 0x0002A920
		public int MaxMobCount
		{
			get
			{
				return this._maxMobCount;
			}
		}

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000DFE RID: 3582 RVA: 0x0002C728 File Offset: 0x0002A928
		// (set) Token: 0x06000DFF RID: 3583 RVA: 0x0002C730 File Offset: 0x0002A930
		public bool SpawnEnvironmentMobs
		{
			get
			{
				return this.spawnEnvironmentMobs;
			}
			set
			{
				this.spawnEnvironmentMobs = value;
			}
		}

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06000E00 RID: 3584 RVA: 0x0002C739 File Offset: 0x0002A939
		// (set) Token: 0x06000E01 RID: 3585 RVA: 0x0002C741 File Offset: 0x0002A941
		public bool SpawnAggressiveMobs
		{
			get
			{
				return this._spawnAggressiveMobs;
			}
			set
			{
				this._spawnAggressiveMobs = value;
			}
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x06000E02 RID: 3586 RVA: 0x0002C74A File Offset: 0x0002A94A
		// (set) Token: 0x06000E03 RID: 3587 RVA: 0x0002C752 File Offset: 0x0002A952
		public bool SpawnMobsWithResponseAggression
		{
			get
			{
				return this._spawnMobsWithResponseAggression;
			}
			set
			{
				this._spawnMobsWithResponseAggression = value;
			}
		}

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000E04 RID: 3588 RVA: 0x0002C75B File Offset: 0x0002A95B
		// (set) Token: 0x06000E05 RID: 3589 RVA: 0x0002C763 File Offset: 0x0002A963
		public bool SpawnedMobsCanShareAggression
		{
			get
			{
				return this._spawnedMobsCanShareAggression;
			}
			set
			{
				this._spawnedMobsCanShareAggression = value;
			}
		}

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000E06 RID: 3590 RVA: 0x0002C76C File Offset: 0x0002A96C
		// (set) Token: 0x06000E07 RID: 3591 RVA: 0x0002C774 File Offset: 0x0002A974
		public float MaxPursuitDistance
		{
			get
			{
				return this._maxPursuitDistance;
			}
			set
			{
				this._maxPursuitDistance = value;
			}
		}

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x06000E08 RID: 3592 RVA: 0x0002C77D File Offset: 0x0002A97D
		// (set) Token: 0x06000E09 RID: 3593 RVA: 0x0002C785 File Offset: 0x0002A985
		public IGameMobMovementPointLimiter MobsMovementPointLimiter
		{
			get
			{
				return this.mobsMovementPointLimiter;
			}
			set
			{
				this.mobsMovementPointLimiter = value;
			}
		}

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x06000E0A RID: 3594 RVA: 0x0002C78E File Offset: 0x0002A98E
		// (set) Token: 0x06000E0B RID: 3595 RVA: 0x0002C796 File Offset: 0x0002A996
		public IGameMobsHangingPlatform HangingMobPlatform
		{
			get
			{
				return this.hangingMobPlatform;
			}
			set
			{
				this.hangingMobPlatform = value;
			}
		}

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x06000E0C RID: 3596 RVA: 0x0002C79F File Offset: 0x0002A99F
		// (set) Token: 0x06000E0D RID: 3597 RVA: 0x0002C7A7 File Offset: 0x0002A9A7
		ILocationChunk ILocationObject.CurrentLocationChunk
		{
			get
			{
				return this.currentLocationChunk;
			}
			set
			{
				this.currentLocationChunk = value;
			}
		}

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000E0E RID: 3598 RVA: 0x0002C7B0 File Offset: 0x0002A9B0
		ILocationChunk IGameMobsSpawner.SpawningChunk
		{
			get
			{
				return this.currentLocationChunk;
			}
		}

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x06000E0F RID: 3599 RVA: 0x0002C7B8 File Offset: 0x0002A9B8
		public ILocationChunk InitialLocationChunk
		{
			get
			{
				return this.initialLocationChunk;
			}
		}

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000E10 RID: 3600 RVA: 0x0002C7C0 File Offset: 0x0002A9C0
		public bool IsBossSpawner
		{
			get
			{
				return this.isBossSpawner;
			}
		}

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000E11 RID: 3601 RVA: 0x0002C7C8 File Offset: 0x0002A9C8
		public IGameMobsFactory MobsFactory
		{
			get
			{
				return (IGameMobsFactory)this.targetFactory;
			}
		}

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000E12 RID: 3602 RVA: 0x0002C7D5 File Offset: 0x0002A9D5
		public int RemainingSpawningCount
		{
			get
			{
				return this.remainingSpawningCount;
			}
		}

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000E13 RID: 3603 RVA: 0x0002C7DD File Offset: 0x0002A9DD
		public int SpawnedMobsCount
		{
			get
			{
				return this.initialSpawningCount - this.remainingSpawningCount;
			}
		}

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000E14 RID: 3604 RVA: 0x0002C7EC File Offset: 0x0002A9EC
		public GameMobFactions GroupOwner
		{
			get
			{
				return this._groupOwner;
			}
		}

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06000E15 RID: 3605 RVA: 0x0002C7F4 File Offset: 0x0002A9F4
		public GameMobsGroupControllerBase SpawnedGroup
		{
			get
			{
				return this.spawnedGroup;
			}
		}

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06000E16 RID: 3606 RVA: 0x0002C7FC File Offset: 0x0002A9FC
		public bool IsSpawningStarted
		{
			get
			{
				return this.isSpawningStarted;
			}
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000E17 RID: 3607 RVA: 0x0002C804 File Offset: 0x0002AA04
		public bool IsGroupSpawned
		{
			get
			{
				return this.isGroupSpawned;
			}
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000E18 RID: 3608 RVA: 0x0002C80C File Offset: 0x0002AA0C
		public float MaxGroupHitPointsSum
		{
			get
			{
				return this.maxGroupHitPointsSum;
			}
		}

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06000E19 RID: 3609 RVA: 0x0002C814 File Offset: 0x0002AA14
		public bool IsWaitingForGroupRespawn
		{
			get
			{
				return this.groupRespawnCoroutine != null;
			}
		}

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06000E1A RID: 3610 RVA: 0x0002C820 File Offset: 0x0002AA20
		int? ILocationObject.LocationObjectType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000E1B RID: 3611 RVA: 0x0002C836 File Offset: 0x0002AA36
		Vector2 ILocationObject.Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000E1C RID: 3612 RVA: 0x0002C848 File Offset: 0x0002AA48
		float ILocationObject.Orientation
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000E1D RID: 3613 RVA: 0x0002C84F File Offset: 0x0002AA4F
		float IProgressBasedAction.CurrentProgress
		{
			get
			{
				return this.spawningProgress;
			}
		}

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000E1E RID: 3614 RVA: 0x0002C857 File Offset: 0x0002AA57
		bool ILocationObject.IsDynamicLocationObject
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1400009B RID: 155
		// (add) Token: 0x06000E1F RID: 3615 RVA: 0x0002C85C File Offset: 0x0002AA5C
		// (remove) Token: 0x06000E20 RID: 3616 RVA: 0x0002C894 File Offset: 0x0002AA94
		public event Action<MobBehaviourSpawner> SpawningStarted;

		// Token: 0x1400009C RID: 156
		// (add) Token: 0x06000E21 RID: 3617 RVA: 0x0002C8CC File Offset: 0x0002AACC
		// (remove) Token: 0x06000E22 RID: 3618 RVA: 0x0002C904 File Offset: 0x0002AB04
		public event Action<BaseGameMob> GroupMobSpawned;

		// Token: 0x1400009D RID: 157
		// (add) Token: 0x06000E23 RID: 3619 RVA: 0x0002C93C File Offset: 0x0002AB3C
		// (remove) Token: 0x06000E24 RID: 3620 RVA: 0x0002C974 File Offset: 0x0002AB74
		public event Action<GameMobsGroupControllerBase> GroupSpawned;

		// Token: 0x1400009E RID: 158
		// (add) Token: 0x06000E25 RID: 3621 RVA: 0x0002C9AC File Offset: 0x0002ABAC
		// (remove) Token: 0x06000E26 RID: 3622 RVA: 0x0002C9E4 File Offset: 0x0002ABE4
		public event Action<MobBehaviourSpawner, GameMobsGroupControllerBase> GroupDestroyed;

		// Token: 0x06000E27 RID: 3623 RVA: 0x0002CA1C File Offset: 0x0002AC1C
		private async void TryRegisterInLocationChunk()
		{
			if (base.CurrentGame == null)
			{
				await base.CurrentGame.WaitGameSessionInitialization();
			}
			if (this.currentLocationChunk == null && !this.IsPlayerGroupSpawner())
			{
				IGameLocationProvider gameLocationProvider;
				if (this.initialLocationChunk != null)
				{
					this.initialLocationChunk.AddEnvironmentObject(this);
				}
				else if (base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
				{
					GameLocation currentLocation = gameLocationProvider.CurrentLocation;
					if (currentLocation != null)
					{
						ILocationChunk locationChunkAtPoint = currentLocation.GetLocationChunkAtPoint(base.transform.position, false);
						if (locationChunkAtPoint != null)
						{
							locationChunkAtPoint.AddEnvironmentObject(this);
						}
					}
				}
			}
		}

		// Token: 0x06000E28 RID: 3624 RVA: 0x0002CA55 File Offset: 0x0002AC55
		private void UpdateSpawningCount()
		{
			this.remainingSpawningCount = UnityEngine.Random.Range(Mathf.Min(this._minMobCount, this._maxMobCount), Mathf.Max(this._minMobCount, this._maxMobCount));
			this.initialSpawningCount = this.remainingSpawningCount;
		}

		// Token: 0x06000E29 RID: 3625 RVA: 0x0002CA90 File Offset: 0x0002AC90
		private void UpdateNextSpawningFrame()
		{
			this.nextSpawningFrame = (this.useAsyncGroupSpawning ? (Time.frameCount + UnityEngine.Random.Range(1, 4)) : 0);
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x0002CAB0 File Offset: 0x0002ACB0
		private void UpdateNextSpawningTime()
		{
			this.nextSpawningTime = ((this.mobSpawnTimeout > 0f) ? (Time.time + this.mobSpawnTimeout) : 0f);
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x0002CAD8 File Offset: 0x0002ACD8
		private void SetMobsGroup(GameMobsGroupControllerBase group)
		{
			group.GroupMobsSpawner = this;
			group.Faction = this._groupOwner;
			GameMobGroupController gameMobGroupController = group as GameMobGroupController;
			if (gameMobGroupController != null)
			{
				gameMobGroupController.CombatSupportCallHPThreshold = this.combatSupportCallHPThreshold;
			}
			this.spawnedGroup = group;
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x0002CB18 File Offset: 0x0002AD18
		private MobBehaviour SpawnIndividualMob(MobBehaviour.ID id, Vector3 position, MobBehaviourSpawner.MobSpawningInfoItem spawningIterationInfo)
		{
			if (GameApplication.IsGameStateChanging)
			{
				return null;
			}
			IGameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager) && gameSessionManager.IsVictoryStateReached)
			{
				return null;
			}
			if (!this.isReadyForSpawning)
			{
				return null;
			}
			if (this.placedInGeneratedLocation && (this.currentLocationChunk == null || !this.currentLocationChunk.ContainsPoint(position)))
			{
				return null;
			}
			bool flag = this.currentLocationChunk.HasType(LocationChunk.TypeID.BossChunk) || this.currentLocationChunk.HasType(LocationChunk.TypeID.ArenaDeadEnd);
			MobBehaviourSpawner.MobsFactoryArgs.mobID = id;
			MobBehaviourSpawner.MobsFactoryArgs.mobFaction = this._groupOwner;
			MobBehaviourSpawner.MobsFactoryArgs.spawnPosition = position;
			MobBehaviourSpawner.MobsFactoryArgs.isEnvironmentMob = this.spawnEnvironmentMobs;
			if (spawningIterationInfo != null)
			{
				MobBehaviourSpawner.MobsFactoryArgs.hitPointsAmountOverride = spawningIterationInfo.hitPointsAmountOverride;
			}
			else
			{
				MobBehaviourSpawner.MobsFactoryArgs.hitPointsAmountOverride = 0f;
			}
			MobBehaviourSpawner.MobSpawnerInfo.spawner = this;
			MobBehaviourSpawner.MobSpawnerInfo.isAggressiveMob = this._spawnAggressiveMobs;
			MobBehaviourSpawner.MobSpawnerInfo.isAggressionReactiveMob = this._spawnMobsWithResponseAggression;
			MobBehaviourSpawner.MobSpawnerInfo.canShareAggression = this._spawnedMobsCanShareAggression;
			MobBehaviourSpawner.MobSpawnerInfo.aggressionRadiusOverride = this.aggressionRadiusOverride;
			MobBehaviourSpawner.MobSpawnerInfo.usePlayerAsDefaultAttackTarget = (flag || this.forceSelectPlayerAsDefaultTarget);
			MobBehaviour mobBehaviour = this.targetFactory.Create(MobBehaviourSpawner.MobsFactoryArgs) as MobBehaviour;
			this.TryOverrideAdditionalMobParams(mobBehaviour);
			return mobBehaviour;
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x0002CC73 File Offset: 0x0002AE73
		private bool HasFixedSpawningPivots()
		{
			return this.fixedSpawningPivots != null && this.fixedSpawningPivots.Length != 0;
		}

		// Token: 0x06000E2E RID: 3630 RVA: 0x0002CC8C File Offset: 0x0002AE8C
		private bool TryPlaceAtSpawningPivot(BaseGameMob spawnedMob)
		{
			if (this.HasFixedSpawningPivots())
			{
				int num = this.fixedSpawningPivots.Length;
				Transform[] array = this.fixedSpawningPivots;
				int num2 = this.spawningPivotIndex;
				this.spawningPivotIndex = num2 + 1;
				Transform transform = array[num2 % num];
				if (transform != null)
				{
					Vector2 vector = transform.position;
					if (this.initialSpawningCount > num || !this.spawnWithoutPositionOffset)
					{
						vector += MobBehaviourSpawner.GetRandomOffset(spawnedMob);
					}
					spawnedMob.Position = vector;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E2F RID: 3631 RVA: 0x0002CD02 File Offset: 0x0002AF02
		private IEnumerator RespawnGroupRoutine(Action<MobBehaviourSpawner> respawnStartedCallback, Action<MobBehaviourSpawner> respawnCompletedCallback)
		{
			while (!this.isGroupSpawned)
			{
				yield return null;
			}
			this.isGroupSpawned = false;
			this.isSpawningStarted = false;
			if (respawnStartedCallback != null)
			{
				respawnStartedCallback(this);
			}
			while (!this.isGroupSpawned)
			{
				this.SpawnGroup();
				yield return null;
			}
			this.groupRespawnCoroutine = null;
			if (respawnCompletedCallback != null)
			{
				respawnCompletedCallback(this);
			}
			yield break;
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x0002CD20 File Offset: 0x0002AF20
		private void SpawnGroup()
		{
			if (this.isGroupSpawned || this.initialSpawningInfo.Length == 0 || Time.frameCount < this.nextSpawningFrame || Time.time < this.nextSpawningTime)
			{
				return;
			}
			if (!this.isSpawningStarted)
			{
				base.ResetAllForcedCounts();
				if (this.remainingSpawningCount <= 0)
				{
					this.UpdateSpawningCount();
				}
				this.isSpawningStarted = true;
				LocationChunk locationChunk = this.currentLocationChunk as LocationChunk;
				if (locationChunk != null)
				{
					locationChunk.RegisterSpawnedMobsGroup(this.spawnedGroup);
				}
				Action<MobBehaviourSpawner> spawningStarted = this.SpawningStarted;
				if (spawningStarted != null)
				{
					spawningStarted(this);
				}
			}
			int num = 1;
			if (this.mobSpawnTimeout < 0f)
			{
				num = (this.useAsyncGroupSpawning ? Mathf.Min(this.remainingSpawningCount, 2) : this.remainingSpawningCount);
			}
			for (int i = 0; i < num; i++)
			{
				MobBehaviour mobBehaviour = base.Spawn();
				if (!(mobBehaviour == null))
				{
					this.maxGroupHitPointsSum += mobBehaviour.MaxHitPoints;
					if (this.spawnDeadMobs)
					{
						base.StartCoroutine(this.MobKillingRoutine(mobBehaviour));
					}
				}
			}
			this.remainingSpawningCount -= num;
			if (this.remainingSpawningCount > 0)
			{
				this.UpdateNextSpawningFrame();
				this.UpdateNextSpawningTime();
				if (this.disableGroupMovementWhileSpawning)
				{
					GameMobGroupController gameMobGroupController = this.spawnedGroup as GameMobGroupController;
					if (gameMobGroupController == null)
					{
						return;
					}
					gameMobGroupController.PlaceAsFormation(base.transform.position, 0);
				}
				return;
			}
			this.isGroupSpawned = true;
			this.spawningProgress = 1f;
			if (!this.useAsyncGroupSpawning && this.mobsMovementPointLimiter == null && !this.HasFixedSpawningPivots())
			{
				GameMobGroupController gameMobGroupController2 = this.spawnedGroup as GameMobGroupController;
				if (gameMobGroupController2 != null)
				{
					gameMobGroupController2.PlaceAsFormation(base.transform.position, 0);
				}
			}
			this.сlampedMobPositionsStabTime = 0f;
			this.spawnedGroup.MobRemoved += this.OnMobRemoved;
			Action<GameMobsGroupControllerBase> groupSpawned = this.GroupSpawned;
			if (groupSpawned == null)
			{
				return;
			}
			groupSpawned(this.spawnedGroup);
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x0002CEF7 File Offset: 0x0002B0F7
		private void OnMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			if (!this.spawnedGroup.HasMobs)
			{
				this.spawnedGroup.MobRemoved -= this.OnMobRemoved;
				Action<MobBehaviourSpawner, GameMobsGroupControllerBase> groupDestroyed = this.GroupDestroyed;
				if (groupDestroyed == null)
				{
					return;
				}
				groupDestroyed(this, this.spawnedGroup);
			}
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x0002CF34 File Offset: 0x0002B134
		private bool TryPlaceGroupInsideLimitingArea()
		{
			if (this.maxClampedMobPositionsStabTime <= 0f || this.сlampedMobPositionsStabTime >= this.maxClampedMobPositionsStabTime)
			{
				return false;
			}
			IGameMobMovementPointLimiter gameMobMovementPointLimiter = this.mobsMovementPointLimiter;
			Collider2D collider2D = (gameMobMovementPointLimiter != null) ? gameMobMovementPointLimiter.Area : null;
			if (collider2D == null)
			{
				return false;
			}
			IReadOnlyList<BaseGameMob> mobs = this.spawnedGroup.Mobs;
			for (int i = 0; i < mobs.Count; i++)
			{
				BaseGameMob baseGameMob = mobs[i];
				if (baseGameMob.IsActiveNavMeshAgent())
				{
					NavMeshAgent navMeshAgent = baseGameMob.NavMeshAgent;
					float minEdgeDistance = 0.25f;
					TaggedPivotGroup taggedPivotsGroup = baseGameMob.TaggedPivotsGroup;
					Vector2 point;
					if (taggedPivotsGroup != null)
					{
						point = taggedPivotsGroup.GroupTransform.position + taggedPivotsGroup.GetPivot(MobBehaviourSpawner.MobFootTagHash).LocalPosition;
					}
					else
					{
						point = baseGameMob.Position;
						minEdgeDistance = navMeshAgent.radius;
					}
					Vector2 v;
					if (!collider2D.IsPointInside(point, out v, minEdgeDistance))
					{
						navMeshAgent.Move(v);
					}
				}
			}
			this.сlampedMobPositionsStabTime += Time.deltaTime;
			return true;
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x0002D038 File Offset: 0x0002B238
		private async void TryOverrideAdditionalMobParams(BaseGameMob spawnedMob)
		{
			if (!this.spawnDeadMobs)
			{
				await Task.Yield();
				if (Application.isPlaying && !(spawnedMob == null) && !spawnedMob.IsKilled)
				{
					GameMobMotionControllerBase motionController = spawnedMob.MotionController;
					if (motionController != null)
					{
						if (this.spawnImmovableMobs)
						{
							motionController.IsActive = false;
						}
						GameMobMotionController gameMobMotionController = motionController as GameMobMotionController;
						if (gameMobMotionController != null)
						{
							gameMobMotionController.SetObstacleAvoidanceFlags(!this.disableObstacleAvoidance, !this.disableNavmeshEdgesAvoidance);
						}
					}
					if (!this.allowedAttackTargetsDescriptionOverride.IsBlank())
					{
						GameMobAIController aicontroller = spawnedMob.AIController;
						if (aicontroller != null)
						{
							GameMobAIControllerParams currentParams = aicontroller.CurrentParams;
							if (this.targetSelectionMethodOverride != GameMobTargetSelector.SelectionMethod.None)
							{
								currentParams.targetSelectionMethod = this.targetSelectionMethodOverride;
							}
							if (this.targetPriorityOverride != GameMobTargetSelector.PrioritySelector.Default)
							{
								currentParams.priorityTargetSelector = this.targetPriorityOverride;
							}
							this.allowedAttackTargetsDescriptionOverride.PassTo(ref currentParams.additionalAttackTargetsFilter);
						}
					}
				}
			}
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x0002D079 File Offset: 0x0002B279
		private IEnumerator MobKillingRoutine(MobBehaviour mobToKill)
		{
			yield return null;
			yield return null;
			if (!mobToKill.IsNull())
			{
				if (mobToKill.CurrentLocationChunk == null)
				{
					mobToKill.CurrentLocationChunk = this.currentLocationChunk;
				}
				mobToKill.KillMob(null);
			}
			yield break;
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x0002D08F File Offset: 0x0002B28F
		protected override IFactory GetFactory(IGame currentGame)
		{
			return currentGame.Services.Get<IGameMobsFactory>();
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x0002D09C File Offset: 0x0002B29C
		protected override object Spawn(ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem spawningInfo, IFactory targetFactory)
		{
			MobBehaviour mobBehaviour = this.SpawnIndividualMob(spawningInfo.ObjectID, this.spawnPosition, spawningInfo as MobBehaviourSpawner.MobSpawningInfoItem);
			if (mobBehaviour != null)
			{
				IGameMobMovementPointLimiter gameMobMovementPointLimiter = this.mobsMovementPointLimiter;
				Collider2D collider2D = (gameMobMovementPointLimiter != null) ? gameMobMovementPointLimiter.Area : null;
				GameMobsGroupControllerBase gameMobsGroupControllerBase = this.spawnedGroup;
				if (gameMobsGroupControllerBase != null)
				{
					gameMobsGroupControllerBase.AddMob(mobBehaviour, null);
				}
				if (!this.TryPlaceAtSpawningPivot(mobBehaviour) && this.hangingMobPlatform != null && collider2D != null)
				{
					mobBehaviour.Position = collider2D.GetRandomPointInCollider();
				}
				else if (!this.spawnWithoutPositionOffset)
				{
					MobBehaviourSpawner.AddRandomOffset(mobBehaviour);
				}
				if (this.spawningRoot != null)
				{
					mobBehaviour.transform.parent = this.spawningRoot;
				}
				GameMobAnimationController gameMobAnimationController = mobBehaviour.AnimationController as GameMobAnimationController;
				if (gameMobAnimationController != null)
				{
					gameMobAnimationController.startDirection = this.startDirection;
				}
				Action<BaseGameMob> groupMobSpawned = this.GroupMobSpawned;
				if (groupMobSpawned != null)
				{
					groupMobSpawned(mobBehaviour);
				}
			}
			return mobBehaviour;
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x0002D17D File Offset: 0x0002B37D
		public bool IsPlayerGroupSpawner()
		{
			return this._groupOwner == GameMobFactions.PLAYER;
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x0002D188 File Offset: 0x0002B388
		public int ForceGetInitialSpawningCount()
		{
			if (this.initialSpawningCount <= 0 && this.SpawnedMobsCount == 0)
			{
				this.UpdateSpawningCount();
			}
			return this.initialSpawningCount;
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x0002D1A7 File Offset: 0x0002B3A7
		public bool TrySetSpawningCounts(int minMobsCount, int maxMobsCount)
		{
			if (this.SpawnedMobsCount == 0)
			{
				this._minMobCount = minMobsCount;
				this._maxMobCount = maxMobsCount;
				this.UpdateSpawningCount();
				return true;
			}
			return false;
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x0002D1C8 File Offset: 0x0002B3C8
		public GameMobsGroupControllerBase GetOrCreateMobsGroup(Type customGroupComponentType = null)
		{
			if (this.spawnedGroup == null)
			{
				IGameMobGroupControllerProvider gameMobGroupControllerProvider;
				if (base.TryGetComponent<IGameMobGroupControllerProvider>(out gameMobGroupControllerProvider))
				{
					this.SetMobsGroup(gameMobGroupControllerProvider.GroupController);
				}
				else
				{
					Component component = base.gameObject.AddComponent(customGroupComponentType ?? typeof(GameMobGroup));
					this.SetMobsGroup(((IGameMobGroupControllerProvider)component).GroupController);
				}
			}
			return this.spawnedGroup;
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x0002D228 File Offset: 0x0002B428
		public void PrepareForSpawning(IGameMobGroupControllerProvider customGroupComponent)
		{
			if (this.isReadyForSpawning)
			{
				return;
			}
			this.spawnPosition = base.transform.position;
			this.groupComponent = (customGroupComponent ?? base.GetComponent<IGameMobGroupControllerProvider>());
			if (this.groupComponent == null)
			{
				this.groupComponent = base.gameObject.AddComponent<GameMobGroup>();
			}
			this.SetMobsGroup(this.groupComponent.GroupController);
			GameObject gameObject = (this.additionalControllersObject != null) ? this.additionalControllersObject : base.gameObject;
			if (this.mobsMovementPointLimiter == null)
			{
				gameObject.TryGetComponent<IGameMobMovementPointLimiter>(out this.mobsMovementPointLimiter);
			}
			if (this.hangingMobPlatform != null || gameObject.TryGetComponent<IGameMobsHangingPlatform>(out this.hangingMobPlatform))
			{
				this.spawnPosition = this.hangingMobPlatform.GetPosition();
			}
			this.isReadyForSpawning = true;
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x0002D2EF File Offset: 0x0002B4EF
		public MobBehaviour SpawnIndividualMob(MobBehaviour.ID id, Vector3 position)
		{
			return this.SpawnIndividualMob(id, position, null);
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x0002D2FC File Offset: 0x0002B4FC
		public async Task<GameMobsGroupControllerBase> GetGroupAsync()
		{
			await new WaitForUpdate();
			IGameMobGroupControllerProvider gameMobGroupControllerProvider = this.groupComponent;
			return (gameMobGroupControllerProvider != null) ? gameMobGroupControllerProvider.GroupController : null;
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x0002D341 File Offset: 0x0002B541
		public bool TryStartGroupRespawn(Action<MobBehaviourSpawner> respawnStartedCallback = null, Action<MobBehaviourSpawner> respawnCompletedCallback = null)
		{
			if (this.groupRespawnCoroutine == null)
			{
				this.groupRespawnCoroutine = base.StartCoroutine(this.RespawnGroupRoutine(respawnStartedCallback, respawnCompletedCallback));
				return true;
			}
			return false;
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x0002D364 File Offset: 0x0002B564
		BaseGameMob IGameMobsSpawner.SpawnMob(int mobID)
		{
			MobBehaviour mobBehaviour = this.SpawnIndividualMob((MobBehaviour.ID)mobID, base.transform.position);
			if (mobBehaviour != null)
			{
				MobBehaviourSpawner.AddRandomOffset(mobBehaviour);
			}
			return mobBehaviour;
		}

		// Token: 0x06000E40 RID: 3648 RVA: 0x0002D394 File Offset: 0x0002B594
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			Transform root = base.transform.root;
			if (root != null && root != base.transform)
			{
				root.TryGetComponent<ILocationChunk>(out this.initialLocationChunk);
			}
			Vector3 position = base.transform.position;
			position.z = 0f;
			base.transform.position = position;
			this.isReadyForSpawning = false;
			this.isSpawningStarted = false;
			this.isGroupSpawned = false;
			this.spawningProgress = 0f;
			ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem[] spawningInfo = this.initialSpawningInfo;
			base.SpawningInfo = spawningInfo;
			this.spawnOnStart &= !this._spawnGroup;
			if (this.forceIgnoreSpawningObstacles)
			{
				this._spawningObstacleLayers = 0;
			}
			else
			{
				GameMobsFactory gameMobsFactory = this.targetFactory as GameMobsFactory;
				if (gameMobsFactory != null)
				{
					GameMobFactionInfo factionInfo = gameMobsFactory.GetFactionInfo(this._groupOwner);
					if (factionInfo.IsValid())
					{
						this._spawningObstacleLayers |= factionInfo.enemyMobLayers;
					}
				}
				if (!this.IsPlayerGroupSpawner())
				{
					this._spawningObstacleLayers |= PlayerFactory.PlayerLayerMask;
				}
			}
			GameSceneManager gameSceneManager;
			if (currentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
			currentGame.Services.TryGet<IGameSessionManager>(out this.sessionManager);
			this.UpdateSpawningCount();
		}

		// Token: 0x06000E41 RID: 3649 RVA: 0x0002D4FF File Offset: 0x0002B6FF
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			this.placedInGeneratedLocation = !this.IsPlayerGroupSpawner();
			this.TryRegisterInLocationChunk();
		}

		// Token: 0x06000E42 RID: 3650 RVA: 0x0002D516 File Offset: 0x0002B716
		private void OnEnable()
		{
			this.TryRegisterInLocationChunk();
		}

		// Token: 0x06000E43 RID: 3651 RVA: 0x0002D51E File Offset: 0x0002B71E
		protected override void Start()
		{
			this.PrepareForSpawning(null);
			base.Start();
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x0002D530 File Offset: 0x0002B730
		private void Update()
		{
			if (this.isGroupSpawned)
			{
				if (this.TryPlaceGroupInsideLimitingArea())
				{
					return;
				}
				if (this._spawnOnlyOnce)
				{
					base.enabled = false;
					return;
				}
			}
			if (!this._spawnGroup || (this.sessionManager != null && this.sessionManager.CurrentSessionState != SessionState.InProgress))
			{
				return;
			}
			if (this._spawningObstacleLayers != 0 && Physics2D.OverlapCircle(base.transform.position, 3f, this._spawningObstacleLayers))
			{
				return;
			}
			this.SpawnGroup();
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x0002D5BC File Offset: 0x0002B7BC
		protected override void OnDestroy()
		{
			base.OnDestroy();
			ILocationChunk locationChunk = this.currentLocationChunk;
			if (locationChunk != null)
			{
				locationChunk.RemoveEnvironmentObject(this);
			}
			if (this.spawnedGroup != null)
			{
				this.spawnedGroup.MobRemoved -= this.OnMobRemoved;
			}
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x0002D5F5 File Offset: 0x0002B7F5
		private void OnDrawGizmosSelected()
		{
			if (this.aggressionRadiusOverride > 0f)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(base.transform.position, this.aggressionRadiusOverride);
			}
		}

		// Token: 0x04000834 RID: 2100
		private const int AsyncIterationMobsCount = 2;

		// Token: 0x04000835 RID: 2101
		private const int MaxAsyncIterationFramesDelay = 4;

		// Token: 0x04000836 RID: 2102
		private static readonly GameMobSpawningInfo MobSpawnerInfo = new GameMobSpawningInfo();

		// Token: 0x04000837 RID: 2103
		private static readonly MobBehaviour.FactoryArgs MobsFactoryArgs = new MobBehaviour.FactoryArgs
		{
			spawnerInfo = MobBehaviourSpawner.MobSpawnerInfo
		};

		// Token: 0x04000838 RID: 2104
		private static readonly int MobFootTagHash = TaggedPivotGroup.TagToHash("MobFoot");

		// Token: 0x0400083D RID: 2109
		[SerializeField]
		private bool isBossSpawner;

		// Token: 0x0400083E RID: 2110
		[SerializeField]
		private MobBehaviourSpawner.MobSpawningInfoItem[] initialSpawningInfo;

		// Token: 0x0400083F RID: 2111
		[SerializeField]
		private LayerMask _spawningObstacleLayers = 512;

		// Token: 0x04000840 RID: 2112
		public bool forceIgnoreSpawningObstacles;

		// Token: 0x04000841 RID: 2113
		[Header("Заспаунить мобов только 1 раз.")]
		[SerializeField]
		private bool _spawnOnlyOnce = true;

		// Token: 0x04000842 RID: 2114
		[Tooltip("Таймаут между спауном мобов")]
		public float mobSpawnTimeout = -1f;

		// Token: 0x04000843 RID: 2115
		public bool disableGroupMovementWhileSpawning;

		// Token: 0x04000844 RID: 2116
		[Range(0f, 300f)]
		[SerializeField]
		private int _minMobCount = 3;

		// Token: 0x04000845 RID: 2117
		[Range(0f, 300f)]
		[SerializeField]
		private int _maxMobCount = 3;

		// Token: 0x04000846 RID: 2118
		[Space]
		[Header("Агрессивные мобы?")]
		[SerializeField]
		[FormerlySerializedAs("_aggressiveMobs")]
		private bool _spawnAggressiveMobs = true;

		// Token: 0x04000847 RID: 2119
		[Header("Мобы используют ответную агрессию?")]
		[SerializeField]
		[FormerlySerializedAs("_hasResponseAggression")]
		private bool _spawnMobsWithResponseAggression = true;

		// Token: 0x04000848 RID: 2120
		[SerializeField]
		private bool _spawnedMobsCanShareAggression = true;

		// Token: 0x04000849 RID: 2121
		[Tooltip("Если больше 0, то мобам будет задано это значение в качестве радиуса агрессии.")]
		public float aggressionRadiusOverride;

		// Token: 0x0400084A RID: 2122
		[Header("Как далеко моб может уйти от свой позиции? <=0 - неограничено")]
		[SerializeField]
		private float _maxPursuitDistance = -1f;

		// Token: 0x0400084B RID: 2123
		public float maxForcedDestinationMoveAwayDistance;

		// Token: 0x0400084C RID: 2124
		[Range(0f, 1f)]
		public float combatSupportCallHPThreshold = 0.7f;

		// Token: 0x0400084D RID: 2125
		public bool forceSelectPlayerAsDefaultTarget;

		// Token: 0x0400084E RID: 2126
		[Space]
		[Tooltip("Задает будет ли отспавнен единичный моб или группа мобов.")]
		[SerializeField]
		private bool _spawnGroup = true;

		// Token: 0x0400084F RID: 2127
		[Tooltip("Спаун мобов, которые не учитываются при подсчете числа вражеских мобов на чанке. Например, курицы.")]
		[SerializeField]
		private bool spawnEnvironmentMobs;

		// Token: 0x04000850 RID: 2128
		public bool spawnImmovableMobs;

		// Token: 0x04000851 RID: 2129
		public bool spawnDeadMobs;

		// Token: 0x04000852 RID: 2130
		[SerializeField]
		private GameMobFactions _groupOwner;

		// Token: 0x04000853 RID: 2131
		public Transform[] fixedSpawningPivots;

		// Token: 0x04000854 RID: 2132
		public Transform spawningRoot;

		// Token: 0x04000855 RID: 2133
		public GameObject additionalControllersObject;

		// Token: 0x04000856 RID: 2134
		[FormerlySerializedAs("ignoreLocationChunkSpawning")]
		public bool isNotLocationChunkSpawner;

		// Token: 0x04000857 RID: 2135
		public bool useAsyncGroupSpawning = true;

		// Token: 0x04000858 RID: 2136
		[Tooltip("Спаун всех мобов одну точку - позицию спаунера")]
		public bool spawnWithoutPositionOffset;

		// Token: 0x04000859 RID: 2137
		public TransformDirection startDirection;

		// Token: 0x0400085A RID: 2138
		[Tooltip("Если используется ограничивающая мобов область, то в течение этого времени спавнер будет пытаться разместить группу в этой области.")]
		public float maxClampedMobPositionsStabTime;

		// Token: 0x0400085B RID: 2139
		[Space]
		public GameMobTargetSelector.SelectionMethod targetSelectionMethodOverride = GameMobTargetSelector.SelectionMethod.None;

		// Token: 0x0400085C RID: 2140
		public GameMobTargetSelector.PrioritySelector targetPriorityOverride = GameMobTargetSelector.PrioritySelector.Default;

		// Token: 0x0400085D RID: 2141
		public GameMobDescription allowedAttackTargetsDescriptionOverride = GameMobDescription.BlankDescription;

		// Token: 0x0400085E RID: 2142
		[Space]
		public bool disableObstacleAvoidance;

		// Token: 0x0400085F RID: 2143
		public bool disableNavmeshEdgesAvoidance;

		// Token: 0x04000860 RID: 2144
		private IGameSessionManager sessionManager;

		// Token: 0x04000861 RID: 2145
		private IGameMobMovementPointLimiter mobsMovementPointLimiter;

		// Token: 0x04000862 RID: 2146
		private IGameMobsHangingPlatform hangingMobPlatform;

		// Token: 0x04000863 RID: 2147
		private IGameMobGroupControllerProvider groupComponent;

		// Token: 0x04000864 RID: 2148
		private GameMobsGroupControllerBase spawnedGroup;

		// Token: 0x04000865 RID: 2149
		private Vector2 spawnPosition;

		// Token: 0x04000866 RID: 2150
		private int spawningPivotIndex;

		// Token: 0x04000867 RID: 2151
		private int remainingSpawningCount;

		// Token: 0x04000868 RID: 2152
		private int initialSpawningCount;

		// Token: 0x04000869 RID: 2153
		private bool placedInGeneratedLocation;

		// Token: 0x0400086A RID: 2154
		private ILocationChunk initialLocationChunk;

		// Token: 0x0400086B RID: 2155
		private ILocationChunk currentLocationChunk;

		// Token: 0x0400086C RID: 2156
		private float spawningProgress;

		// Token: 0x0400086D RID: 2157
		private float maxGroupHitPointsSum;

		// Token: 0x0400086E RID: 2158
		private bool isReadyForSpawning;

		// Token: 0x0400086F RID: 2159
		private bool isSpawningStarted;

		// Token: 0x04000870 RID: 2160
		private bool isGroupSpawned;

		// Token: 0x04000871 RID: 2161
		private int nextSpawningFrame;

		// Token: 0x04000872 RID: 2162
		private float nextSpawningTime;

		// Token: 0x04000873 RID: 2163
		private float сlampedMobPositionsStabTime;

		// Token: 0x04000874 RID: 2164
		private Coroutine groupRespawnCoroutine;

		// Token: 0x0200048C RID: 1164
		[Serializable]
		public sealed class MobSpawningInfoItem : ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem
		{
			// Token: 0x1700075B RID: 1883
			// (get) Token: 0x0600243E RID: 9278 RVA: 0x000701D6 File Offset: 0x0006E3D6
			// (set) Token: 0x0600243F RID: 9279 RVA: 0x000701DE File Offset: 0x0006E3DE
			public override MobBehaviour.ID ObjectID
			{
				get
				{
					return this._objectID;
				}
				set
				{
					this._objectID = value;
				}
			}

			// Token: 0x040018A4 RID: 6308
			[SerializeField]
			[ObjectFactoryIDPopup(typeof(IGameMob))]
			private MobBehaviour.ID _objectID;

			// Token: 0x040018A5 RID: 6309
			public float hitPointsAmountOverride;
		}
	}
}
