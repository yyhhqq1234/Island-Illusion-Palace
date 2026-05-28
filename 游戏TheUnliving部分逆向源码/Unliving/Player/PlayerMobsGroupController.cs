using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Buffs;
using Game.Damage;
using Game.Utility;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x0200015E RID: 350
	[Serializable]
	public sealed class PlayerMobsGroupController : GameMobGroupController, IDisposable
	{
		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060009B3 RID: 2483 RVA: 0x00021068 File Offset: 0x0001F268
		// (set) Token: 0x060009B4 RID: 2484 RVA: 0x00021070 File Offset: 0x0001F270
		public override IGameMob Leader
		{
			get
			{
				return base.Leader;
			}
			set
			{
				base.Leader = value;
				this.playerMobsLayers = 0;
				if (!this.currentPlayer.IsNull())
				{
					PlayerInputController playerInputController = this.currentPlayer.PlayerInputController;
					if (playerInputController != null)
					{
						playerInputController.PlayerActionPerformed -= this.OnPlayerInputActionPerformed;
					}
					this.currentPlayer.DamageApplied -= this.OnPlayerDamageApplied;
				}
				if ((this.currentPlayer = (value as PlayerBehaviour)) != null)
				{
					IGameMobsFactory gameMobsFactory;
					if (this.currentPlayer.CurrentGame.Services.TryGet<IGameMobsFactory>(out gameMobsFactory))
					{
						this.playerMobsLayers = 1 << gameMobsFactory.GetFactionInfo(this.currentPlayer.Faction).mobsLayer;
						this.playerMobsLayers |= 1 << gameMobsFactory.GetFactionInfo(GameMobFactions.PLAYER_ALLIES).mobsLayer;
					}
					if (this.currentPlayer.PlayerInputController != null)
					{
						this.currentPlayer.PlayerInputController.PlayerActionPerformed += this.OnPlayerInputActionPerformed;
					}
					this.currentPlayer.DamageApplied += this.OnPlayerDamageApplied;
				}
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060009B5 RID: 2485 RVA: 0x00021182 File Offset: 0x0001F382
		public int PlayerMobsLayers
		{
			get
			{
				return this.playerMobsLayers;
			}
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060009B6 RID: 2486 RVA: 0x0002118A File Offset: 0x0001F38A
		public int EnemyMobsLayers
		{
			get
			{
				return this.enemyMobsLayers;
			}
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x060009B7 RID: 2487 RVA: 0x00021192 File Offset: 0x0001F392
		public bool IsForcedDestinationModeActive
		{
			get
			{
				return this.isForcedDestinationModeActive;
			}
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x060009B8 RID: 2488 RVA: 0x0002119A File Offset: 0x0001F39A
		public bool IsContinuousDestinationModeActive
		{
			get
			{
				return this.isContinuousDestinationModeActive;
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x060009B9 RID: 2489 RVA: 0x000211A2 File Offset: 0x0001F3A2
		public override bool IsFollowingGroupLeader
		{
			get
			{
				return this.isFollowingPlayer;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060009BA RID: 2490 RVA: 0x000211AA File Offset: 0x0001F3AA
		public override bool IsRetreating
		{
			get
			{
				return this.isRetreating;
			}
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x000211B4 File Offset: 0x0001F3B4
		private float GetGroupDistanceToPlayer()
		{
			if (!base.HasMobs)
			{
				return 0f;
			}
			Vector2 position = this.currentPlayer.Position;
			Rect totalGroupRect = base.TotalGroupRect;
			Vector2 min = totalGroupRect.min;
			Vector2 max = totalGroupRect.max;
			Vector2 vector = position;
			if (vector.x < min.x)
			{
				vector.x = min.x;
			}
			if (vector.x > max.x)
			{
				vector.x = max.x;
			}
			if (vector.y < min.y)
			{
				vector.y = min.y;
			}
			if (vector.y > max.y)
			{
				vector.y = max.y;
			}
			return (vector - position).SqrMagnitude();
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x00021278 File Offset: 0x0001F478
		private void UpdateMobsIDCache(BaseGameMob mob, bool addMob)
		{
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour == null)
			{
				return;
			}
			int objectID = (int)mobBehaviour.ObjectID;
			int num;
			if (!this.mobsCounters.TryGetValue(objectID, out num))
			{
				if (addMob)
				{
					this.mobsCounters.Add(objectID, 1);
					this.mobsIDCache.Add(mobBehaviour.ObjectID);
					this.mobsIDCache.Sort();
				}
				return;
			}
			if (addMob)
			{
				num++;
			}
			else
			{
				num--;
			}
			if (num != 0)
			{
				this.mobsCounters[objectID] = num;
				return;
			}
			this.mobsCounters.Remove(objectID);
			this.mobsIDCache.Remove(mobBehaviour.ObjectID);
			this.mobsIDCache.Sort();
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x0002131C File Offset: 0x0001F51C
		private void SetSummonedMobsDestination(Vector2 destination)
		{
			IReadOnlyList<GameMobsGroupControllerBase> coupledGroups = base.CoupledGroups;
			for (int i = 0; i < coupledGroups.Count; i++)
			{
				GameMobsGroupControllerBase gameMobsGroupControllerBase = coupledGroups[i];
				if (!gameMobsGroupControllerBase.InBattle && gameMobsGroupControllerBase.Leader == this.Leader)
				{
					gameMobsGroupControllerBase.GroupDestination = new Vector2?(destination);
				}
			}
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x0002136B File Offset: 0x0001F56B
		private void TryActivatePlayerFollowAttackMode(Vector2 attackDestination)
		{
			if (this.isFollowingPlayer && !this.isPlayerFollowingAttackModeActive)
			{
				this.isPlayerFollowingAttackModeActive = true;
				this.isFollowingPlayer = false;
				this.SetDestination(new Vector2?(attackDestination), null, false, false);
			}
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x0002139B File Offset: 0x0001F59B
		private void ResetPlayerFollowAttackMode()
		{
			this.isPlayerFollowingAttackModeActive = false;
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x000213A4 File Offset: 0x0001F5A4
		private void FollowPlayer(bool isCommand)
		{
			if (this.currentPlayer == null || (this.isPlayerFollowingAttackModeActive && !isCommand))
			{
				return;
			}
			this.isFollowingPlayer = true;
			this.SetDestination(new Vector2?(this.currentPlayer.Position), isCommand ? this.currentPlayer : null, false, true);
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x000213F8 File Offset: 0x0001F5F8
		private void MoveTo(Vector2 newDestination, bool isForcedDestination)
		{
			this.isFollowingPlayer = false;
			this.ResetPlayerFollowAttackMode();
			this.SetDestination(new Vector2?(newDestination), this.currentPlayer, isForcedDestination || this.HasForcedGroupDestination != isForcedDestination, isForcedDestination);
			if (isForcedDestination)
			{
				this.ResetAttackGroups();
				this.AddMobsChargeBuffs();
			}
			this.ResetMobsAutoReturnTime();
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x0002144D File Offset: 0x0001F64D
		private bool IsForcedDestination(Vector2 newDestination)
		{
			return !Physics2D.OverlapCircle(newDestination, this.attackModeCursorRadius, this.enemyMobsLayers);
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x00021469 File Offset: 0x0001F669
		private void ResetAttackGroups()
		{
			this.currentAttackGroups.Clear();
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x00021478 File Offset: 0x0001F678
		private bool TryStartChangingDestination(PlayerInputController.ActionArgs inputArgs, PlayerAction expectedAction)
		{
			if (!this.isDestinationAlteringLocked && inputArgs.HasActionFlag(expectedAction))
			{
				if (!this.isChangingDestination)
				{
					int pickableLayers = this.playerMobsLayers | 2;
					if (this.currentPlayer.PlayerInputController.ForceGetPointerOverObject(pickableLayers, new Predicate<GameObject>(PlayerMobsGroupController.<TryStartChangingDestination>g__IsPlayerMob|51_0)) != null)
					{
						this.isDestinationAlteringLocked = true;
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x000214D8 File Offset: 0x0001F6D8
		private void AddMobsChargeBuffs()
		{
			if (Time.time < this.nextMobsChargeTime)
			{
				return;
			}
			if (!this.isMobsChargeActive)
			{
				if (this.mobsChargeBuffsGenerators == null)
				{
					BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.mobsChargeBuffGeneratorAssets;
					generatorsBuilders.Instantiate(out this.mobsChargeBuffsGenerators);
				}
				float num = 0f;
				for (int i = 0; i < this.mobs.Count; i++)
				{
					IBuffsController buffsController = this.mobs[i].BuffsController;
					if (buffsController != null)
					{
						for (int j = 0; j < this.mobsChargeBuffsGenerators.Length; j++)
						{
							IBuffsGenerator buffsGenerator = this.mobsChargeBuffsGenerators[j];
							buffsController.AddBuff(buffsGenerator.GenerateBuff(this.currentPlayer, false));
							if (buffsGenerator.BuffDuration > num)
							{
								num = buffsGenerator.BuffDuration;
							}
						}
					}
				}
				this.nextMobsChargeTime = Time.time + num + 0.1f;
			}
			this.isMobsChargeActive = true;
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x000215AD File Offset: 0x0001F7AD
		private void ResetMobsAutoReturnTime()
		{
			this.mobsReturnTime = Time.time + this.mobsAutoReturnTimeout;
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x000215C4 File Offset: 0x0001F7C4
		private void UpdateMobsAutoReturn()
		{
			if (this.isFollowingPlayer)
			{
				return;
			}
			if (!this.IsGroupDestinationReached || this.InBattle)
			{
				this.ResetMobsAutoReturnTime();
				return;
			}
			if (this.mobsAutoReturnTimeout > 0f && Time.time > this.mobsReturnTime)
			{
				this.FollowPlayer();
				return;
			}
			float num = this.mobsAutoReturnMaxPlayerDistance;
			if (num > 0f && this.GetGroupDistanceToPlayer() > num * num)
			{
				this.FollowPlayer();
			}
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x00021634 File Offset: 0x0001F834
		private bool IsValidAttackTarget(BaseGameMob playerMob, IGameMob attackTarget)
		{
			if (!playerMob.IsSacrificed && !playerMob.IsKilled)
			{
				if (this.isRetreating)
				{
					return playerMob.MotionController.CurrentBlockingMob == attackTarget;
				}
				if ((this.isForcedDestinationModeActive || this.HasForcedGroupDestination) && !base.IsAttacking)
				{
					float num = Mathf.Max(this.attackModeMobsAgressionRadiusOverride, playerMob.Radius + 0.2f) + attackTarget.Radius;
					return (attackTarget.HitColliderCenter - playerMob.HitColliderCenter).SqrMagnitude() < num * num;
				}
			}
			return true;
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x000216C0 File Offset: 0x0001F8C0
		private TargetSelector<BaseGameMob>.TargetPriorityEstimation GetFocusedTargetsPriorityEstimation(BaseGameMob attackTarget)
		{
			float multiplier;
			if (GameMobTargetSelector.IsMinorTarget(attackTarget, out multiplier))
			{
				return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(0f, multiplier);
			}
			if (this.isRetreating || !this.InBattle)
			{
				return TargetSelector<BaseGameMob>.TargetPriorityEstimation.Neutral;
			}
			if (!base.IsAttacking)
			{
				return GameMobTargetSelector.GetAttackSpaceTargetPriority(attackTarget);
			}
			Vector2 vector = this.currentDestination - attackTarget.Position;
			return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(1f / Mathf.Max(vector.x * vector.x + vector.y * vector.y, 0.0001f));
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x0002174A File Offset: 0x0001F94A
		public void FollowPlayer()
		{
			this.FollowPlayer(false);
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x00021753 File Offset: 0x0001F953
		public bool HasMobWithID(MobBehaviour.ID id)
		{
			return this.mobsCounters.ContainsKey((int)id);
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x00021764 File Offset: 0x0001F964
		public int GetMobsCount(MobBehaviour.ID mobsID)
		{
			int result;
			this.mobsCounters.TryGetValue((int)mobsID, out result);
			return result;
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x00021781 File Offset: 0x0001F981
		public IReadOnlyList<MobBehaviour.ID> GetAllMobsID()
		{
			return this.mobsIDCache;
		}

		// Token: 0x060009CE RID: 2510 RVA: 0x0002178C File Offset: 0x0001F98C
		private void OnPlayerInputActionPerformed(PlayerInputController.ActionArgs args)
		{
			if (args.actionFlags.ActionsCount != 0)
			{
				if (args.HasActionFlag(PlayerAction.MOBS_RETURN_TO_PLAYER) || args.HasActionFlag(PlayerAction.MOBS_RETURN_TO_PLAYER_FORCE))
				{
					this.FollowPlayer(true);
				}
				else if (!this.isDestinationAlteringLocked && this.TryStartChangingDestination(args, PlayerAction.MOBS_TARGET_POSITION))
				{
					Vector2 worldCursorPosition = args.worldCursorPosition;
					if (args.usedInputBehaviours.Has(PlayerInputController.InputBehaviour.HOLD | PlayerInputController.InputBehaviour.FAST_HOLD))
					{
						this.isContinuousDestinationModeActive = true;
						this.isForcedDestinationModeActive = true;
					}
					else
					{
						this.isContinuousDestinationModeActive = false;
						this.isForcedDestinationModeActive = this.IsForcedDestination(worldCursorPosition);
					}
					this.isChangingDestination = true;
					this.MoveTo(worldCursorPosition, this.isForcedDestinationModeActive);
				}
			}
			if ((this.isChangingDestination || this.isDestinationAlteringLocked) && !args.HasActionFlags(PlayerMobsGroupController.DestinationActions))
			{
				if (this.isContinuousDestinationModeActive && this.isForcedDestinationModeActive)
				{
					Vector2 worldCursorPosition2 = args.worldCursorPosition;
					this.MoveTo(worldCursorPosition2, this.IsForcedDestination(worldCursorPosition2));
				}
				this.isContinuousDestinationModeActive = false;
				this.isChangingDestination = false;
				this.isForcedDestinationModeActive = false;
				this.isDestinationAlteringLocked = false;
				this.isMobsChargeActive = false;
			}
		}

		// Token: 0x060009CF RID: 2511 RVA: 0x0002188C File Offset: 0x0001FA8C
		protected override void OnGroupDestinationChanged()
		{
			this.isRetreating = false;
			if (this.GroupDestination != null)
			{
				this.SetSummonedMobsDestination(this.GroupDestination.Value);
				if (base.CurrentBattleDirection != null)
				{
					this.isRetreating = (Vector2.Dot(base.CurrentBattleDirection.Value, this.GroupDestinationDirection) < 0f);
					if (this.isRetreating)
					{
						this.ResetPlayerFollowAttackMode();
					}
				}
			}
		}

		// Token: 0x060009D0 RID: 2512 RVA: 0x00021908 File Offset: 0x0001FB08
		protected override void OnGroupDestinationReached()
		{
			this.ResetAttackGroups();
			this.isRetreating = false;
		}

		// Token: 0x060009D1 RID: 2513 RVA: 0x00021917 File Offset: 0x0001FB17
		protected override void OnBattleStateChanged(bool inBattle)
		{
			if (inBattle)
			{
				this.ResetAttackGroups();
				return;
			}
			if (this.isPlayerFollowingAttackModeActive)
			{
				this.ResetPlayerFollowAttackMode();
			}
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x00021934 File Offset: 0x0001FB34
		protected override void OnMobAdded(BaseGameMob mob)
		{
			base.OnMobAdded(mob);
			this.UpdateMobsIDCache(mob, true);
			GameMobAIController aicontroller = mob.AIController;
			if (aicontroller != null)
			{
				aicontroller.SetEnemyTargetsAdditionalValidator(new Func<BaseGameMob, IGameMob, bool>(this.IsValidAttackTarget));
			}
			GameMobAIController aicontroller2 = mob.AIController;
			if (aicontroller2 == null)
			{
				return;
			}
			aicontroller2.SetEnemyTargetsPriorityEstimatorOverride(new Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation>(this.GetFocusedTargetsPriorityEstimation));
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x00021989 File Offset: 0x0001FB89
		protected override void OnMobRemoved(BaseGameMob mob)
		{
			GameMobAIController aicontroller = mob.AIController;
			if (aicontroller != null)
			{
				aicontroller.SetEnemyTargetsAdditionalValidator(null);
			}
			GameMobAIController aicontroller2 = mob.AIController;
			if (aicontroller2 != null)
			{
				aicontroller2.SetEnemyTargetsPriorityEstimatorOverride(null);
			}
			this.UpdateMobsIDCache(mob, false);
			base.OnMobRemoved(mob);
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x000219BE File Offset: 0x0001FBBE
		private void OnPlayerDamageApplied(IDamageable damageable, float amount)
		{
			this.TryActivatePlayerFollowAttackMode(damageable.Transform.position);
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x000219D8 File Offset: 0x0001FBD8
		public override void Initialize(int groupID, GameObject groupObject, Vector2 initialGroupPosition)
		{
			base.Faction = GameMobFactions.PLAYER;
			base.Initialize(groupID, groupObject, initialGroupPosition);
			BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.mobsChargeBuffGeneratorAssets;
			generatorsBuilders.Instantiate(out this.mobsChargeBuffsGenerators);
			this.FollowPlayer(false);
			if (base.GroupMobsSpawner != null)
			{
				this.enemyMobsLayers = base.GroupMobsSpawner.MobsFactory.GetFactionInfo(base.Faction).enemyMobLayers;
			}
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x00021A3D File Offset: 0x0001FC3D
		public override void OnUpdate()
		{
			this.UpdateMobsAutoReturn();
			if (this.isFollowingPlayer)
			{
				this.FollowPlayer();
			}
			this.GetGroupDistanceToPlayer();
			base.OnUpdate();
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x00021A60 File Offset: 0x0001FC60
		void IDisposable.Dispose()
		{
			this.Leader = null;
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x00021AE8 File Offset: 0x0001FCE8
		[CompilerGenerated]
		internal static bool <TryStartChangingDestination>g__IsPlayerMob|51_0(GameObject gameObject)
		{
			BaseGameMob baseGameMob;
			return gameObject.TryGetComponent<BaseGameMob>(out baseGameMob) && baseGameMob.IsPlayerMob;
		}

		// Token: 0x040005B0 RID: 1456
		private const PlayerInputController.InputBehaviour KeyHoldBehaviours = PlayerInputController.InputBehaviour.HOLD | PlayerInputController.InputBehaviour.FAST_HOLD;

		// Token: 0x040005B1 RID: 1457
		private static readonly PlayerAction[] DestinationActions = new PlayerAction[]
		{
			PlayerAction.MOBS_TARGET_POSITION_FORCE,
			PlayerAction.PLAYER_GROUP_TARGET_POSITION,
			PlayerAction.MOBS_TARGET_POSITION
		};

		// Token: 0x040005B2 RID: 1458
		public float attackModeCursorRadius = 2f;

		// Token: 0x040005B3 RID: 1459
		public float attackModeTargetsUpdateRate = 5f;

		// Token: 0x040005B4 RID: 1460
		public float attackModeMobsAgressionRadiusOverride = 2f;

		// Token: 0x040005B5 RID: 1461
		public float mobsAutoReturnTimeout = 10f;

		// Token: 0x040005B6 RID: 1462
		public float mobsAutoReturnMaxPlayerDistance = 10f;

		// Token: 0x040005B7 RID: 1463
		[SerializeField]
		private BuffsGeneratorBuilderAsset.Reference[] mobsChargeBuffGeneratorAssets;

		// Token: 0x040005B8 RID: 1464
		private readonly Dictionary<int, int> mobsCounters = new Dictionary<int, int>(16);

		// Token: 0x040005B9 RID: 1465
		private readonly List<MobBehaviour.ID> mobsIDCache = new List<MobBehaviour.ID>(16);

		// Token: 0x040005BA RID: 1466
		private PlayerMobsGroupController.GroupIDBuffer currentAttackGroups;

		// Token: 0x040005BB RID: 1467
		private PlayerBehaviour currentPlayer;

		// Token: 0x040005BC RID: 1468
		private bool isFollowingPlayer;

		// Token: 0x040005BD RID: 1469
		private bool isPlayerFollowingAttackModeActive;

		// Token: 0x040005BE RID: 1470
		private bool isChangingDestination;

		// Token: 0x040005BF RID: 1471
		private bool isForcedDestinationModeActive;

		// Token: 0x040005C0 RID: 1472
		private bool isContinuousDestinationModeActive;

		// Token: 0x040005C1 RID: 1473
		private bool isDestinationAlteringLocked;

		// Token: 0x040005C2 RID: 1474
		private Vector2 currentDestination;

		// Token: 0x040005C3 RID: 1475
		private int playerMobsLayers;

		// Token: 0x040005C4 RID: 1476
		private int enemyMobsLayers;

		// Token: 0x040005C5 RID: 1477
		private IBuffsGenerator[] mobsChargeBuffsGenerators;

		// Token: 0x040005C6 RID: 1478
		private bool isMobsChargeActive;

		// Token: 0x040005C7 RID: 1479
		private float nextMobsChargeTime;

		// Token: 0x040005C8 RID: 1480
		private float mobsReturnTime;

		// Token: 0x040005C9 RID: 1481
		private bool isRetreating;

		// Token: 0x02000466 RID: 1126
		private struct GroupIDBuffer
		{
			// Token: 0x17000735 RID: 1845
			// (get) Token: 0x060023A0 RID: 9120 RVA: 0x0006E491 File Offset: 0x0006C691
			public int Count
			{
				get
				{
					return this.count;
				}
			}

			// Token: 0x060023A1 RID: 9121 RVA: 0x0006E49C File Offset: 0x0006C69C
			public unsafe bool Add(int groupID)
			{
				if (this.count < 6)
				{
					fixed (int* ptr = &this.groupID0)
					{
						ref int ptr2 = ref *ptr;
						int num = this.count;
						this.count = num + 1;
						*(ref ptr2 + (IntPtr)num * 4) = groupID;
					}
					return true;
				}
				return false;
			}

			// Token: 0x060023A2 RID: 9122 RVA: 0x0006E4D8 File Offset: 0x0006C6D8
			public unsafe bool Contains(int groupID)
			{
				if (this.count != 0)
				{
					fixed (int* ptr = &this.groupID0)
					{
						int* ptr2 = ptr;
						for (int i = 0; i < this.count; i++)
						{
							if (ptr2[i] == groupID)
							{
								return true;
							}
						}
					}
				}
				return false;
			}

			// Token: 0x060023A3 RID: 9123 RVA: 0x0006E518 File Offset: 0x0006C718
			public bool Contains(GameMobsGroupControllerBase group)
			{
				return this.Contains(group.GroupID);
			}

			// Token: 0x060023A4 RID: 9124 RVA: 0x0006E526 File Offset: 0x0006C726
			public void Clear()
			{
				this.count = 0;
			}

			// Token: 0x04001730 RID: 5936
			public const int MaxCount = 6;

			// Token: 0x04001731 RID: 5937
			public int groupID0;

			// Token: 0x04001732 RID: 5938
			public int groupID1;

			// Token: 0x04001733 RID: 5939
			public int groupID2;

			// Token: 0x04001734 RID: 5940
			public int groupID3;

			// Token: 0x04001735 RID: 5941
			public int groupID4;

			// Token: 0x04001736 RID: 5942
			public int groupID5;

			// Token: 0x04001737 RID: 5943
			private int count;
		}
	}
}
