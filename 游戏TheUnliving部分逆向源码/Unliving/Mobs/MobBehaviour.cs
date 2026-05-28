using System;
using System.Runtime.CompilerServices;
using Common;
using Common.DataBinding;
using Common.Editor;
using Common.Factories;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using Game.Factories;
using Game.Localization;
using Game.PassiveAbilities;
using Game.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.GameSession.PlayerLeveling;
using Unliving.GameSession.PlayerMobsLeveling;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;
using Unliving.PassiveAbilities;

namespace Unliving.Mobs
{
	// Token: 0x020001C3 RID: 451
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody2D))]
	public sealed class MobBehaviour : BaseGameMob, PrototypeBasedFactory<MobBehaviour.FactoryPrototype, IGameMob>.IInitializableByFactory, IRevivableGameMob, IPlayerLevelingEXPSource, ILevelablePlayerMob, IGameMob, IBuffableObject
	{
		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000DC1 RID: 3521 RVA: 0x0002BCFF File Offset: 0x00029EFF
		// (set) Token: 0x06000DC2 RID: 3522 RVA: 0x0002BD07 File Offset: 0x00029F07
		public MobBehaviour.ID ObjectID { get; set; }

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000DC3 RID: 3523 RVA: 0x0002BD10 File Offset: 0x00029F10
		// (set) Token: 0x06000DC4 RID: 3524 RVA: 0x0002BD22 File Offset: 0x00029F22
		public MobBehaviour.ID ZombieMobID
		{
			get
			{
				if (!base.IsPlayerMob)
				{
					return this._zombieMobID;
				}
				return MobBehaviour.ID.None;
			}
			set
			{
				this._zombieMobID = value;
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06000DC5 RID: 3525 RVA: 0x0002BD2B File Offset: 0x00029F2B
		// (set) Token: 0x06000DC6 RID: 3526 RVA: 0x0002BD46 File Offset: 0x00029F46
		[StatProperty(MobStatID.MobAggressionRadius)]
		public float AggressionRadius
		{
			get
			{
				if (this.aiControllerParams == null)
				{
					return 0f;
				}
				return this.aiControllerParams.TargetSearchRadius;
			}
			private set
			{
				if (this.aiControllerParams == null)
				{
					return;
				}
				this.aiControllerParams.TargetSearchRadius = value;
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06000DC7 RID: 3527 RVA: 0x0002BD5D File Offset: 0x00029F5D
		// (set) Token: 0x06000DC8 RID: 3528 RVA: 0x0002BD65 File Offset: 0x00029F65
		[StatProperty(MobStatID.MobActivationCost)]
		public float ActivationCost
		{
			get
			{
				return this.activationCost;
			}
			set
			{
				this.activationCost = value;
			}
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06000DC9 RID: 3529 RVA: 0x0002BD6E File Offset: 0x00029F6E
		// (set) Token: 0x06000DCA RID: 3530 RVA: 0x0002BD76 File Offset: 0x00029F76
		[StatProperty(MobStatID.MobActivationReward)]
		public float ActivationReward
		{
			get
			{
				return this.activationReward;
			}
			set
			{
				this.activationReward = value;
			}
		}

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06000DCB RID: 3531 RVA: 0x0002BD7F File Offset: 0x00029F7F
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06000DCC RID: 3532 RVA: 0x0002BD82 File Offset: 0x00029F82
		public override bool IsCharacter
		{
			get
			{
				return this._isCharacter;
			}
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06000DCD RID: 3533 RVA: 0x0002BD8A File Offset: 0x00029F8A
		public override GameMobMotionControllerBase MotionController
		{
			get
			{
				return this.motionController;
			}
		}

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06000DCE RID: 3534 RVA: 0x0002BD92 File Offset: 0x00029F92
		public override GameMobAIController AIController
		{
			get
			{
				return this.aiController;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06000DCF RID: 3535 RVA: 0x0002BD9A File Offset: 0x00029F9A
		public override GameAbilitiesController AbilitiesController
		{
			get
			{
				return this.abilitiesController;
			}
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000DD0 RID: 3536 RVA: 0x0002BDA2 File Offset: 0x00029FA2
		public override BasePassiveAbilitiesController PassiveAbilitiesController
		{
			get
			{
				return this.passiveAbilitiesController;
			}
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06000DD1 RID: 3537 RVA: 0x0002BDAA File Offset: 0x00029FAA
		public override StatsControllerBase<MobStatModifier> StatsController
		{
			get
			{
				return this.statsController;
			}
		}

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06000DD2 RID: 3538 RVA: 0x0002BDB2 File Offset: 0x00029FB2
		public bool IsRevivable
		{
			get
			{
				return this.ZombieMobID > MobBehaviour.ID.None;
			}
		}

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06000DD3 RID: 3539 RVA: 0x0002BDBD File Offset: 0x00029FBD
		public bool IsBoss
		{
			get
			{
				return this._isBoss;
			}
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000DD4 RID: 3540 RVA: 0x0002BDC5 File Offset: 0x00029FC5
		public MobBehaviourSpawner Spawner
		{
			get
			{
				return this.spawner;
			}
		}

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000DD5 RID: 3541 RVA: 0x0002BDCD File Offset: 0x00029FCD
		int IPlayerLevelingEXPSource.EXPAmount
		{
			get
			{
				return this.revivingEXPAmount;
			}
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06000DD6 RID: 3542 RVA: 0x0002BDD5 File Offset: 0x00029FD5
		int ILevelablePlayerMob.MobType
		{
			get
			{
				if (!base.IsPlayerMob)
				{
					return 0;
				}
				return (int)this.ObjectID;
			}
		}

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06000DD7 RID: 3543 RVA: 0x0002BDE7 File Offset: 0x00029FE7
		// (set) Token: 0x06000DD8 RID: 3544 RVA: 0x0002BDEF File Offset: 0x00029FEF
		int ILevelablePlayerMob.MobLevel
		{
			get
			{
				return this.mobLevel;
			}
			set
			{
				if (this.mobLevel == value)
				{
					return;
				}
				this.mobLevel = value;
				Action<int> mobLevelChanged = this.MobLevelChanged;
				if (mobLevelChanged == null)
				{
					return;
				}
				mobLevelChanged(this.mobLevel);
			}
		}

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000DD9 RID: 3545 RVA: 0x0002BE18 File Offset: 0x0002A018
		protected override bool CanGenerateResources
		{
			get
			{
				return true;
			}
		}

		// Token: 0x14000099 RID: 153
		// (add) Token: 0x06000DDA RID: 3546 RVA: 0x0002BE1C File Offset: 0x0002A01C
		// (remove) Token: 0x06000DDB RID: 3547 RVA: 0x0002BE54 File Offset: 0x0002A054
		public event Action<int> MobLevelChanged;

		// Token: 0x1400009A RID: 154
		// (add) Token: 0x06000DDC RID: 3548 RVA: 0x0002BE8C File Offset: 0x0002A08C
		// (remove) Token: 0x06000DDD RID: 3549 RVA: 0x0002BEC4 File Offset: 0x0002A0C4
		public event Action<BaseGameMob, BaseGameMob> Revived;

		// Token: 0x06000DDE RID: 3550 RVA: 0x0002BEFC File Offset: 0x0002A0FC
		private void CreateStatsController()
		{
			this.statsController = new StatsController(this, true);
			for (int i = 0; i < MobBehaviour.MobProxyStatsID.Length; i++)
			{
				this.statsController.AddStat(new MobProxyStat(MobBehaviour.MobProxyStatsID[i], this));
			}
		}

		// Token: 0x06000DDF RID: 3551 RVA: 0x0002BF44 File Offset: 0x0002A144
		private BuffsBasedStatus[] GetSpecialAIStatuses()
		{
			BuffsBasedStatus[] array = Array.Empty<BuffsBasedStatus>();
			if (this.specialAIStatusesBuilders != null && base.BuffsController != null)
			{
				array = new BuffsBasedStatus[this.specialAIStatusesBuilders.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = this.specialAIStatusesBuilders[i].CreateBuffsBasedStatus(base.BuffsController);
				}
			}
			return array;
		}

		// Token: 0x06000DE0 RID: 3552 RVA: 0x0002BF9A File Offset: 0x0002A19A
		private bool HasRevivableGroup()
		{
			return this.group == null || GameMobGroupController.IsDeadGroup(this.group);
		}

		// Token: 0x06000DE1 RID: 3553 RVA: 0x0002BFB1 File Offset: 0x0002A1B1
		private void PrepareRevivedMob(BaseGameMob mob)
		{
			mob.PrepareRevivedMob(this, base.Position);
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x0002BFC0 File Offset: 0x0002A1C0
		protected override LocationObjectType GetLocationObjectType()
		{
			LocationObjectType locationObjectType = base.GetLocationObjectType();
			if (this.IsBoss)
			{
				locationObjectType |= LocationObjectType.BossMob;
			}
			return locationObjectType;
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x0002BFE8 File Offset: 0x0002A1E8
		public bool CanBeRevived(BaseGameMob reviver, object context)
		{
			bool flag;
			if (!this.suppressGroupDependentReviving)
			{
				NecromancyAbilityEffect necromancyAbilityEffect = context as NecromancyAbilityEffect;
				flag = (necromancyAbilityEffect != null && necromancyAbilityEffect.canAffectIndividualMobs);
			}
			else
			{
				flag = true;
			}
			bool flag2 = flag;
			bool flag3 = base.IsKilled || !base.HitPointsController.IsAlive;
			return this.ZombieMobID > MobBehaviour.ID.None && flag3 && reviver.IsValidReviver(this) && reviver.Faction != this.Faction && (flag2 || this.HasRevivableGroup());
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x0002C060 File Offset: 0x0002A260
		public BaseGameMob Revive(BaseGameMob reviver, object context, bool destroySourceMob = true)
		{
			if (this.CanBeRevived(reviver, context))
			{
				BaseGameMob baseGameMob = reviver.Group.AddMob((int)this._zombieMobID, new Action<BaseGameMob>(this.PrepareRevivedMob));
				if (baseGameMob != null)
				{
					Action<BaseGameMob, BaseGameMob> revived = this.Revived;
					if (revived != null)
					{
						revived(reviver, baseGameMob);
					}
					if (destroySourceMob)
					{
						this.DestroyMob();
					}
					return baseGameMob;
				}
			}
			return null;
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x0002C0C0 File Offset: 0x0002A2C0
		public override void DestroyMob()
		{
			if (!this.deadMobPrefab.IsNull())
			{
				UnityEngine.Object.Instantiate<GameObject>(this.deadMobPrefab, base.transform.parent).transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			}
			base.DestroyMob();
		}

		// Token: 0x06000DE6 RID: 3558 RVA: 0x0002C118 File Offset: 0x0002A318
		protected override void OnDamageApplied(IDamageable damagedObject, float damageAmount)
		{
			if (!base.IsPlayerMob || this.Group == null)
			{
				return;
			}
			IGameMob leader = this.Group.Leader;
			if (this != leader)
			{
				IGroupMobDamageFeedbackReceiver groupMobDamageFeedbackReceiver = leader as IGroupMobDamageFeedbackReceiver;
				if (groupMobDamageFeedbackReceiver != null)
				{
					groupMobDamageFeedbackReceiver.OnGroupMobDamageApplied(damagedObject, damageAmount);
				}
			}
		}

		// Token: 0x06000DE7 RID: 3559 RVA: 0x0002C158 File Offset: 0x0002A358
		public override void Initialize(IGame currentGame)
		{
			this.hasControllers = false;
			base.Initialize(currentGame);
			currentGame.BindDataDirectly(ref this.ExposedParams);
		}

		// Token: 0x06000DE8 RID: 3560 RVA: 0x0002C178 File Offset: 0x0002A378
		public void InitializeControllers(MobAttackData mobAttackInfo, int enemyMobLayers, GameMobAIControllerParams aiControllerParams)
		{
			if (this.hasControllers)
			{
				return;
			}
			this.aiControllerParams = aiControllerParams;
			this.CreateStatsController();
			if (base.NavMeshAgent != null)
			{
				this.motionController = new GameMobMotionController(this)
				{
					ImpulseDamping = this.impulseDamping
				};
			}
			this.abilitiesController = new MobAbilitiesController(this, base.CurrentGame.Services.Get<IGameAbilitiesFactory>());
			if (aiControllerParams != null)
			{
				this.aiController = new GameMobAIController(this, aiControllerParams, this.GetSpecialAIStatuses());
			}
			this.abilitiesController.Initialize(mobAttackInfo, enemyMobLayers);
			this.passiveAbilitiesController = new PassiveAbilitiesController(this, null);
			this.hasControllers = true;
		}

		// Token: 0x06000DE9 RID: 3561 RVA: 0x0002C214 File Offset: 0x0002A414
		protected override void OnMobInitialization()
		{
			base.OnMobInitialization();
			if (!this.hasControllers)
			{
				this.InitializeControllers(new MobAttackData(), 0, new GameMobAIControllerParams());
			}
			if (this.motionController != null)
			{
				this.motionController.CurrentHangingPlatform = this.initialHangingPlatform;
			}
			MobHealthController mobHealthController = base.HitPointsController as MobHealthController;
			bool? flag;
			if (mobHealthController == null)
			{
				flag = null;
			}
			else
			{
				MobHealthController.DecayControllerParams decayControllerParams = mobHealthController.decayControllerParams;
				flag = ((decayControllerParams != null) ? new bool?(decayControllerParams.IsValid()) : null);
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault())
			{
				StatsController statsController = this.statsController;
				if (statsController == null)
				{
					return;
				}
				statsController.AddStat(new MobRottingSpeedStat(this));
			}
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x0002C2B4 File Offset: 0x0002A4B4
		void PrototypeBasedFactory<MobBehaviour.FactoryPrototype, IGameMob>.IInitializableByFactory.OnCreatedByFactory(IObjectFactory<IGameMob> factory, IBaseObjectDescription args, MobBehaviour.FactoryPrototype mobData)
		{
			MobAttackData mobAttackData = (mobData != null) ? mobData.attackData : null;
			MobBehaviour.FactoryArgs factoryArgs = args as MobBehaviour.FactoryArgs;
			GameMobSpawningInfo gameMobSpawningInfo = (factoryArgs != null) ? factoryArgs.spawnerInfo : null;
			GameMobAIControllerParams gameMobAIControllerParams = (factoryArgs == null || !factoryArgs.isBrainlessMob) ? new GameMobAIControllerParams() : null;
			float num = (factoryArgs != null) ? factoryArgs.hitPointsAmountOverride : 0f;
			if (mobData != null)
			{
				if (num == 0f)
				{
					num = mobData.Health;
				}
				base.Speed = mobData.Speed;
			}
			if (num != 0f)
			{
				base.MaxHitPoints = num;
			}
			if (gameMobAIControllerParams != null)
			{
				if (this.ExposedParams != null)
				{
					gameMobAIControllerParams.aggressionObstacleLayers = this.ExposedParams.AgressionObstaclesLayers;
				}
				if (mobAttackData != null)
				{
					gameMobAIControllerParams.TargetSearchRadius = mobAttackData.AgressionRadius;
					gameMobAIControllerParams.targetSelectionMethod = mobAttackData.TargetSelectionMethod;
					gameMobAIControllerParams.priorityTargetSelector = mobAttackData.PriorityTargetSelector;
					gameMobAIControllerParams.attackTargetChasingSpeedMultiplier = mobAttackData.AttackTargetChasingSpeedMultiplier;
					gameMobAIControllerParams.fearStateSpeedMultiplier = mobAttackData.RetreatStateSpeedMultiplier;
					gameMobAIControllerParams.maxTargetFollowingDuration = mobAttackData.MinAttackTargetHoldTime;
				}
				if (gameMobSpawningInfo != null)
				{
					gameMobAIControllerParams.isAggressiveByDefault = gameMobSpawningInfo.isAggressiveMob;
					gameMobAIControllerParams.hasResponseAggression = gameMobSpawningInfo.isAggressionReactiveMob;
					gameMobAIControllerParams.shareAggression = gameMobSpawningInfo.canShareAggression;
					gameMobAIControllerParams.usePlayerAsDefaultAttackTarget = gameMobSpawningInfo.usePlayerAsDefaultAttackTarget;
					if (gameMobSpawningInfo.aggressionRadiusOverride > 0f)
					{
						gameMobAIControllerParams.TargetSearchRadius = gameMobSpawningInfo.aggressionRadiusOverride;
					}
				}
				if (this.enemyMobLayers != 0)
				{
					gameMobAIControllerParams.attackLayers = this.enemyMobLayers;
				}
			}
			this.InitializeControllers(mobAttackData, this.enemyMobLayers, gameMobAIControllerParams);
			this.spawner = (((gameMobSpawningInfo != null) ? gameMobSpawningInfo.spawner : null) as MobBehaviourSpawner);
			MobBehaviourSpawner mobBehaviourSpawner = this.spawner;
			this.initialHangingPlatform = ((mobBehaviourSpawner != null) ? mobBehaviourSpawner.HangingMobPlatform : null);
			if (this.motionController != null)
			{
				this.motionController.movementBlockingMobLayers = this.crowdObstaclesLayers;
				if (this.spawner != null)
				{
					this.motionController.maxWanderingDistance = this.spawner.MaxPursuitDistance;
					this.motionController.CurrentMovementPointLimiter = this.spawner.MobsMovementPointLimiter;
					this.motionController.maxGroupDestinationQuitDistance = this.spawner.maxForcedDestinationMoveAwayDistance;
				}
			}
			base.InitializeMob();
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x0002C4BE File Offset: 0x0002A6BE
		protected override void Update()
		{
			base.Update();
			GameMobMotionController gameMobMotionController = this.motionController;
			if (gameMobMotionController != null)
			{
				gameMobMotionController.OnUpdate();
			}
			if (this.aiController != null)
			{
				this.aiController.OnUpdate();
				if (base.IsDeferredUpdateStep)
				{
					this.aiController.OnDeferredUpdate();
				}
			}
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x0002C4FD File Offset: 0x0002A6FD
		private void FixedUpdate()
		{
			GameMobMotionController gameMobMotionController = this.motionController;
			if (gameMobMotionController == null)
			{
				return;
			}
			gameMobMotionController.OnFixedUpdate();
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x0002C50F File Offset: 0x0002A70F
		private void LateUpdate()
		{
			GameMobMotionController gameMobMotionController = this.motionController;
			if (gameMobMotionController == null)
			{
				return;
			}
			gameMobMotionController.OnLateUpdate();
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x0002C524 File Offset: 0x0002A724
		protected override void OnKilled(IDamageable hitPointsController)
		{
			LocationChunk locationChunk = base.CurrentLocationChunk as LocationChunk;
			LocationChunkMobsGridController locationChunkMobsGridController = (locationChunk != null) ? locationChunk.MobsGrid : null;
			base.OnKilled(hitPointsController);
			if (locationChunkMobsGridController != null)
			{
				if (base.IsDestroyed || !this.IsRevivable)
				{
					locationChunkMobsGridController.UnregisterMob(this);
					return;
				}
				locationChunkMobsGridController.RegisterMob(this);
			}
		}

		// Token: 0x04000806 RID: 2054
		private static readonly MobStatID[] MobProxyStatsID = new MobStatID[]
		{
			MobStatID.MobDamage,
			MobStatID.MobAttackSpeed,
			MobStatID.AbilityCooldown,
			MobStatID.MobActivationDamage,
			MobStatID.MobActivationBuffsDuration
		};

		// Token: 0x0400080A RID: 2058
		[SerializeField]
		[Tooltip("Является ли данный моб персонажем. Тотемы, например, персонажами не являются.")]
		private bool _isCharacter = true;

		// Token: 0x0400080B RID: 2059
		[SerializeField]
		private bool _isBoss;

		// Token: 0x0400080C RID: 2060
		[SerializeField]
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		private MobBehaviour.ID _zombieMobID;

		// Token: 0x0400080D RID: 2061
		public bool suppressGroupDependentReviving;

		// Token: 0x0400080E RID: 2062
		[SerializeField]
		private float activationCost;

		// Token: 0x0400080F RID: 2063
		public float activationEnergyReturnAmount;

		// Token: 0x04000810 RID: 2064
		[SerializeField]
		private float activationReward;

		// Token: 0x04000811 RID: 2065
		public MobActivationAbilityType activationType;

		// Token: 0x04000812 RID: 2066
		public LayerMask crowdObstaclesLayers;

		// Token: 0x04000813 RID: 2067
		public MobBehaviour.ExposedParameters ExposedParams;

		// Token: 0x04000814 RID: 2068
		[EnumPopup]
		public AbilityID MeleeAttackAbility;

		// Token: 0x04000815 RID: 2069
		[EnumPopup]
		public AbilityID RangeAttackAbility;

		// Token: 0x04000816 RID: 2070
		[EnumPopup]
		public AbilityID Slot3;

		// Token: 0x04000817 RID: 2071
		[EnumPopup]
		public AbilityID Slot4;

		// Token: 0x04000818 RID: 2072
		[EnumPopup]
		public AbilityID Slot5;

		// Token: 0x04000819 RID: 2073
		[EnumPopup]
		public AbilityID Slot6;

		// Token: 0x0400081A RID: 2074
		public BuffsBasedStatusBuilderAsset[] specialAIStatusesBuilders;

		// Token: 0x0400081B RID: 2075
		public GameObject deadMobPrefab;

		// Token: 0x0400081C RID: 2076
		public int revivingEXPAmount = 1;

		// Token: 0x0400081D RID: 2077
		private int mobLevel;

		// Token: 0x0400081E RID: 2078
		private MobBehaviourSpawner spawner;

		// Token: 0x0400081F RID: 2079
		private GameMobMotionController motionController;

		// Token: 0x04000820 RID: 2080
		private GameMobAIController aiController;

		// Token: 0x04000821 RID: 2081
		private MobAbilitiesController abilitiesController;

		// Token: 0x04000822 RID: 2082
		private PassiveAbilitiesController passiveAbilitiesController;

		// Token: 0x04000823 RID: 2083
		private StatsController statsController;

		// Token: 0x04000824 RID: 2084
		private GameMobAIControllerParams aiControllerParams;

		// Token: 0x04000825 RID: 2085
		private bool hasControllers;

		// Token: 0x04000826 RID: 2086
		private IGameMobsHangingPlatform initialHangingPlatform;

		// Token: 0x02000488 RID: 1160
		[LocalizationObject(LocalizationPrefix.mob_)]
		[Serializable]
		public sealed class FactoryPrototype : IUnityObjectDescription, IBaseObjectDescription, INamed
		{
			// Token: 0x17000756 RID: 1878
			// (get) Token: 0x0600242F RID: 9263 RVA: 0x0007002B File Offset: 0x0006E22B
			// (set) Token: 0x06002430 RID: 9264 RVA: 0x00070033 File Offset: 0x0006E233
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.objectID;
				}
				set
				{
					this.objectID = (MobBehaviour.ID)value;
				}
			}

			// Token: 0x17000757 RID: 1879
			// (get) Token: 0x06002431 RID: 9265 RVA: 0x0007003C File Offset: 0x0006E23C
			// (set) Token: 0x06002432 RID: 9266 RVA: 0x00070069 File Offset: 0x0006E269
			string INamed.Name
			{
				get
				{
					string result = string.Empty;
					if (!this.objectPrefab.IsNull())
					{
						result = this.objectPrefab.name;
					}
					return result;
				}
				set
				{
				}
			}

			// Token: 0x17000758 RID: 1880
			// (get) Token: 0x06002433 RID: 9267 RVA: 0x0007006B File Offset: 0x0006E26B
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					return this.objectPrefab;
				}
			}

			// Token: 0x06002434 RID: 9268 RVA: 0x00070074 File Offset: 0x0006E274
			[return: TupleElementNames(new string[]
			{
				"abilityID",
				"damage"
			})]
			public ValueTuple<AbilityID, float> GetMainAttackAbilityInfo()
			{
				if (this.MeleeAttackAbility != AbilityID.None)
				{
					return new ValueTuple<AbilityID, float>(this.MeleeAttackAbility, this.attackData.MeleeAttackDamage);
				}
				if (this.RangeAttackAbility != AbilityID.None)
				{
					return new ValueTuple<AbilityID, float>(this.RangeAttackAbility, this.attackData.RangeAttackDamage);
				}
				return new ValueTuple<AbilityID, float>(AbilityID.None, 0f);
			}

			// Token: 0x06002435 RID: 9269 RVA: 0x000700CC File Offset: 0x0006E2CC
			public int GetAbilities(out AbilityID[] abilities)
			{
				int result = 0;
				abilities = MobBehaviour.FactoryPrototype.AbilityIDBuffer;
				if (this.MeleeAttackAbility != AbilityID.None)
				{
					abilities[result++] = this.MeleeAttackAbility;
				}
				if (this.RangeAttackAbility != AbilityID.None)
				{
					abilities[result++] = this.RangeAttackAbility;
				}
				if (this.Slot3 != AbilityID.None)
				{
					abilities[result++] = this.Slot3;
				}
				if (this.Slot4 != AbilityID.None)
				{
					abilities[result++] = this.Slot4;
				}
				if (this.Slot5 != AbilityID.None)
				{
					abilities[result++] = this.Slot5;
				}
				if (this.Slot6 != AbilityID.None)
				{
					abilities[result++] = this.Slot6;
				}
				if (this.AuraEffectAbility != AbilityID.None)
				{
					abilities[result++] = this.AuraEffectAbility;
				}
				return result;
			}

			// Token: 0x040017B8 RID: 6072
			private static readonly AbilityID[] AbilityIDBuffer = new AbilityID[8];

			// Token: 0x040017B9 RID: 6073
			[LocalizationID]
			[EnumPopup]
			[ExportIDField]
			public MobBehaviour.ID objectID;

			// Token: 0x040017BA RID: 6074
			[ExportField("PrefabName")]
			public GameObject objectPrefab;

			// Token: 0x040017BB RID: 6075
			[FormerlySerializedAs("MobsHealth")]
			[ExportField("Health")]
			public float Health;

			// Token: 0x040017BC RID: 6076
			[FormerlySerializedAs("MobsSpeed")]
			[ExportField("Speed")]
			public float Speed;

			// Token: 0x040017BD RID: 6077
			[ExportClassField]
			public GameMobImpulseDampingParams impulseDamping;

			// Token: 0x040017BE RID: 6078
			[ExportField("CrowdPassPriorityOverride")]
			public int crowdPassPriorityOverride;

			// Token: 0x040017BF RID: 6079
			public bool disableCrowdInteraction;

			// Token: 0x040017C0 RID: 6080
			[Header("Зомби моб")]
			[ObjectFactoryIDPopup(typeof(MobBehaviour))]
			[ExportField("Revive Mob")]
			public MobBehaviour.ID reviveID;

			// Token: 0x040017C1 RID: 6081
			public int revivingEXPAmount = 1;

			// Token: 0x040017C2 RID: 6082
			public float activationCost;

			// Token: 0x040017C3 RID: 6083
			public float activationEnergyReturnAmount;

			// Token: 0x040017C4 RID: 6084
			public int activationReward;

			// Token: 0x040017C5 RID: 6085
			[ExportClassField]
			public MobAttackData attackData;

			// Token: 0x040017C6 RID: 6086
			[Header("Способности")]
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("MeleeAbility")]
			public AbilityID MeleeAttackAbility;

			// Token: 0x040017C7 RID: 6087
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("RangeAbility")]
			public AbilityID RangeAttackAbility;

			// Token: 0x040017C8 RID: 6088
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("Ability Slot 3")]
			public AbilityID Slot3;

			// Token: 0x040017C9 RID: 6089
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("Ability Slot 4")]
			public AbilityID Slot4;

			// Token: 0x040017CA RID: 6090
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("Ability Slot 5")]
			public AbilityID Slot5;

			// Token: 0x040017CB RID: 6091
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("Ability Slot 6")]
			public AbilityID Slot6;

			// Token: 0x040017CC RID: 6092
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			[ExportField("AuraEffectAbility")]
			public AbilityID AuraEffectAbility;

			// Token: 0x040017CD RID: 6093
			[Header("Метаданные")]
			[ExportField("Name")]
			[LocalizationTitle]
			[HideInInspector]
			public string MobName;
		}

		// Token: 0x02000489 RID: 1161
		public sealed class FactoryArgs : GameMobsFactoryArgsBase
		{
			// Token: 0x17000759 RID: 1881
			// (get) Token: 0x06002438 RID: 9272 RVA: 0x00070199 File Offset: 0x0006E399
			// (set) Token: 0x06002439 RID: 9273 RVA: 0x000701A1 File Offset: 0x0006E3A1
			public override int ObjectID
			{
				get
				{
					return (int)this.mobID;
				}
				set
				{
					this.mobID = (MobBehaviour.ID)value;
				}
			}

			// Token: 0x040017CE RID: 6094
			public MobBehaviour.ID mobID;

			// Token: 0x040017CF RID: 6095
			public bool isBrainlessMob;
		}

		// Token: 0x0200048A RID: 1162
		[LocalizationObject(LocalizationPrefix.mob_)]
		public enum ID
		{
			// Token: 0x040017D1 RID: 6097
			None,
			// Token: 0x040017D2 RID: 6098
			Mob01,
			// Token: 0x040017D3 RID: 6099
			Mob02,
			// Token: 0x040017D4 RID: 6100
			Mob03,
			// Token: 0x040017D5 RID: 6101
			Mob04,
			// Token: 0x040017D6 RID: 6102
			Mob05,
			// Token: 0x040017D7 RID: 6103
			Mob06,
			// Token: 0x040017D8 RID: 6104
			Mob07,
			// Token: 0x040017D9 RID: 6105
			Mob08,
			// Token: 0x040017DA RID: 6106
			Mob09,
			// Token: 0x040017DB RID: 6107
			Mob10,
			// Token: 0x040017DC RID: 6108
			Mob11,
			// Token: 0x040017DD RID: 6109
			Mob12,
			// Token: 0x040017DE RID: 6110
			Mob13,
			// Token: 0x040017DF RID: 6111
			Mob14,
			// Token: 0x040017E0 RID: 6112
			Mob15,
			// Token: 0x040017E1 RID: 6113
			Mob16,
			// Token: 0x040017E2 RID: 6114
			Mob17,
			// Token: 0x040017E3 RID: 6115
			Mob18,
			// Token: 0x040017E4 RID: 6116
			Mob19,
			// Token: 0x040017E5 RID: 6117
			Mob20,
			// Token: 0x040017E6 RID: 6118
			Mob21,
			// Token: 0x040017E7 RID: 6119
			Mob22,
			// Token: 0x040017E8 RID: 6120
			Mob23,
			// Token: 0x040017E9 RID: 6121
			Mob24,
			// Token: 0x040017EA RID: 6122
			Mob25,
			// Token: 0x040017EB RID: 6123
			Mob26,
			// Token: 0x040017EC RID: 6124
			Mob27,
			// Token: 0x040017ED RID: 6125
			Mob28,
			// Token: 0x040017EE RID: 6126
			Mob29,
			// Token: 0x040017EF RID: 6127
			Mob30,
			// Token: 0x040017F0 RID: 6128
			Mob31,
			// Token: 0x040017F1 RID: 6129
			Mob32,
			// Token: 0x040017F2 RID: 6130
			Mob33,
			// Token: 0x040017F3 RID: 6131
			Mob34,
			// Token: 0x040017F4 RID: 6132
			Mob35,
			// Token: 0x040017F5 RID: 6133
			Mob36,
			// Token: 0x040017F6 RID: 6134
			Mob37,
			// Token: 0x040017F7 RID: 6135
			Mob38,
			// Token: 0x040017F8 RID: 6136
			Mob39,
			// Token: 0x040017F9 RID: 6137
			Mob40,
			// Token: 0x040017FA RID: 6138
			Mob41,
			// Token: 0x040017FB RID: 6139
			Mob42,
			// Token: 0x040017FC RID: 6140
			Mob43,
			// Token: 0x040017FD RID: 6141
			Mob44,
			// Token: 0x040017FE RID: 6142
			Mob45,
			// Token: 0x040017FF RID: 6143
			Mob46,
			// Token: 0x04001800 RID: 6144
			Mob47,
			// Token: 0x04001801 RID: 6145
			Mob48,
			// Token: 0x04001802 RID: 6146
			Mob49,
			// Token: 0x04001803 RID: 6147
			Mob50,
			// Token: 0x04001804 RID: 6148
			Mob51,
			// Token: 0x04001805 RID: 6149
			Mob52,
			// Token: 0x04001806 RID: 6150
			Mob53,
			// Token: 0x04001807 RID: 6151
			Mob54,
			// Token: 0x04001808 RID: 6152
			Mob55,
			// Token: 0x04001809 RID: 6153
			Mob56,
			// Token: 0x0400180A RID: 6154
			Mob57,
			// Token: 0x0400180B RID: 6155
			Mob58,
			// Token: 0x0400180C RID: 6156
			Mob59,
			// Token: 0x0400180D RID: 6157
			Mob60,
			// Token: 0x0400180E RID: 6158
			Mob61,
			// Token: 0x0400180F RID: 6159
			Mob62,
			// Token: 0x04001810 RID: 6160
			Mob63,
			// Token: 0x04001811 RID: 6161
			Mob64,
			// Token: 0x04001812 RID: 6162
			Mob65,
			// Token: 0x04001813 RID: 6163
			Mob66,
			// Token: 0x04001814 RID: 6164
			Mob67,
			// Token: 0x04001815 RID: 6165
			Mob68,
			// Token: 0x04001816 RID: 6166
			Mob69,
			// Token: 0x04001817 RID: 6167
			Mob70,
			// Token: 0x04001818 RID: 6168
			Mob71,
			// Token: 0x04001819 RID: 6169
			Mob72,
			// Token: 0x0400181A RID: 6170
			Mob73,
			// Token: 0x0400181B RID: 6171
			Mob74,
			// Token: 0x0400181C RID: 6172
			Mob75,
			// Token: 0x0400181D RID: 6173
			Mob76,
			// Token: 0x0400181E RID: 6174
			Mob77,
			// Token: 0x0400181F RID: 6175
			Mob78,
			// Token: 0x04001820 RID: 6176
			Mob79,
			// Token: 0x04001821 RID: 6177
			Mob80,
			// Token: 0x04001822 RID: 6178
			Mob81,
			// Token: 0x04001823 RID: 6179
			Mob82,
			// Token: 0x04001824 RID: 6180
			Mob83,
			// Token: 0x04001825 RID: 6181
			Mob84,
			// Token: 0x04001826 RID: 6182
			Mob85,
			// Token: 0x04001827 RID: 6183
			Mob86,
			// Token: 0x04001828 RID: 6184
			Mob87,
			// Token: 0x04001829 RID: 6185
			Mob88,
			// Token: 0x0400182A RID: 6186
			Mob89,
			// Token: 0x0400182B RID: 6187
			Mob90,
			// Token: 0x0400182C RID: 6188
			Mob91,
			// Token: 0x0400182D RID: 6189
			Mob92,
			// Token: 0x0400182E RID: 6190
			Mob93,
			// Token: 0x0400182F RID: 6191
			Mob94,
			// Token: 0x04001830 RID: 6192
			Mob95,
			// Token: 0x04001831 RID: 6193
			Mob96,
			// Token: 0x04001832 RID: 6194
			Mob97,
			// Token: 0x04001833 RID: 6195
			Mob98,
			// Token: 0x04001834 RID: 6196
			Mob99,
			// Token: 0x04001835 RID: 6197
			Mob100,
			// Token: 0x04001836 RID: 6198
			Mob101,
			// Token: 0x04001837 RID: 6199
			Mob102,
			// Token: 0x04001838 RID: 6200
			Mob103,
			// Token: 0x04001839 RID: 6201
			Mob104,
			// Token: 0x0400183A RID: 6202
			Mob105,
			// Token: 0x0400183B RID: 6203
			Mob106,
			// Token: 0x0400183C RID: 6204
			Mob107,
			// Token: 0x0400183D RID: 6205
			Mob108,
			// Token: 0x0400183E RID: 6206
			Mob109,
			// Token: 0x0400183F RID: 6207
			Mob110,
			// Token: 0x04001840 RID: 6208
			Mob111,
			// Token: 0x04001841 RID: 6209
			Mob112,
			// Token: 0x04001842 RID: 6210
			Mob113,
			// Token: 0x04001843 RID: 6211
			Mob114,
			// Token: 0x04001844 RID: 6212
			Mob115,
			// Token: 0x04001845 RID: 6213
			Mob116,
			// Token: 0x04001846 RID: 6214
			Mob117,
			// Token: 0x04001847 RID: 6215
			Mob118,
			// Token: 0x04001848 RID: 6216
			Mob119,
			// Token: 0x04001849 RID: 6217
			Mob120,
			// Token: 0x0400184A RID: 6218
			Mob121,
			// Token: 0x0400184B RID: 6219
			Mob122,
			// Token: 0x0400184C RID: 6220
			Mob123,
			// Token: 0x0400184D RID: 6221
			Mob124,
			// Token: 0x0400184E RID: 6222
			Mob125,
			// Token: 0x0400184F RID: 6223
			Mob126,
			// Token: 0x04001850 RID: 6224
			Mob127,
			// Token: 0x04001851 RID: 6225
			Mob128,
			// Token: 0x04001852 RID: 6226
			Mob129,
			// Token: 0x04001853 RID: 6227
			Mob130,
			// Token: 0x04001854 RID: 6228
			Mob131,
			// Token: 0x04001855 RID: 6229
			Mob132,
			// Token: 0x04001856 RID: 6230
			Mob133,
			// Token: 0x04001857 RID: 6231
			Mob134,
			// Token: 0x04001858 RID: 6232
			Mob135,
			// Token: 0x04001859 RID: 6233
			Mob136,
			// Token: 0x0400185A RID: 6234
			Mob137,
			// Token: 0x0400185B RID: 6235
			Mob138,
			// Token: 0x0400185C RID: 6236
			Mob139,
			// Token: 0x0400185D RID: 6237
			Mob140,
			// Token: 0x0400185E RID: 6238
			Mob141,
			// Token: 0x0400185F RID: 6239
			Mob142,
			// Token: 0x04001860 RID: 6240
			Mob143,
			// Token: 0x04001861 RID: 6241
			Mob144,
			// Token: 0x04001862 RID: 6242
			Mob145,
			// Token: 0x04001863 RID: 6243
			Mob146,
			// Token: 0x04001864 RID: 6244
			Mob147,
			// Token: 0x04001865 RID: 6245
			Mob148,
			// Token: 0x04001866 RID: 6246
			Mob149,
			// Token: 0x04001867 RID: 6247
			Mob150,
			// Token: 0x04001868 RID: 6248
			Mob151,
			// Token: 0x04001869 RID: 6249
			Mob152,
			// Token: 0x0400186A RID: 6250
			Mob153,
			// Token: 0x0400186B RID: 6251
			Mob154,
			// Token: 0x0400186C RID: 6252
			Mob155,
			// Token: 0x0400186D RID: 6253
			Mob156,
			// Token: 0x0400186E RID: 6254
			Mob157,
			// Token: 0x0400186F RID: 6255
			Mob158,
			// Token: 0x04001870 RID: 6256
			Mob159,
			// Token: 0x04001871 RID: 6257
			Mob160,
			// Token: 0x04001872 RID: 6258
			Mob161,
			// Token: 0x04001873 RID: 6259
			Mob162,
			// Token: 0x04001874 RID: 6260
			Mob163,
			// Token: 0x04001875 RID: 6261
			Mob164,
			// Token: 0x04001876 RID: 6262
			Mob165,
			// Token: 0x04001877 RID: 6263
			Mob166,
			// Token: 0x04001878 RID: 6264
			Mob167,
			// Token: 0x04001879 RID: 6265
			Mob168,
			// Token: 0x0400187A RID: 6266
			Mob169,
			// Token: 0x0400187B RID: 6267
			Mob170,
			// Token: 0x0400187C RID: 6268
			Mob171,
			// Token: 0x0400187D RID: 6269
			Mob172,
			// Token: 0x0400187E RID: 6270
			Mob173,
			// Token: 0x0400187F RID: 6271
			Mob174,
			// Token: 0x04001880 RID: 6272
			Mob175,
			// Token: 0x04001881 RID: 6273
			Mob176,
			// Token: 0x04001882 RID: 6274
			Mob177,
			// Token: 0x04001883 RID: 6275
			Mob178,
			// Token: 0x04001884 RID: 6276
			Mob179,
			// Token: 0x04001885 RID: 6277
			Mob180,
			// Token: 0x04001886 RID: 6278
			Mob181,
			// Token: 0x04001887 RID: 6279
			Mob182,
			// Token: 0x04001888 RID: 6280
			Mob183,
			// Token: 0x04001889 RID: 6281
			Mob184,
			// Token: 0x0400188A RID: 6282
			Mob185,
			// Token: 0x0400188B RID: 6283
			Mob186,
			// Token: 0x0400188C RID: 6284
			Mob187,
			// Token: 0x0400188D RID: 6285
			Mob188,
			// Token: 0x0400188E RID: 6286
			Mob189,
			// Token: 0x0400188F RID: 6287
			Mob190,
			// Token: 0x04001890 RID: 6288
			Mob191,
			// Token: 0x04001891 RID: 6289
			Mob192,
			// Token: 0x04001892 RID: 6290
			Mob193,
			// Token: 0x04001893 RID: 6291
			Mob194,
			// Token: 0x04001894 RID: 6292
			Mob195,
			// Token: 0x04001895 RID: 6293
			Mob196,
			// Token: 0x04001896 RID: 6294
			Mob197,
			// Token: 0x04001897 RID: 6295
			Mob198,
			// Token: 0x04001898 RID: 6296
			Mob199,
			// Token: 0x04001899 RID: 6297
			Mob200,
			// Token: 0x0400189A RID: 6298
			Mob201,
			// Token: 0x0400189B RID: 6299
			Mob202,
			// Token: 0x0400189C RID: 6300
			Mob203,
			// Token: 0x0400189D RID: 6301
			Mob204,
			// Token: 0x0400189E RID: 6302
			Mob205,
			// Token: 0x0400189F RID: 6303
			Mob206,
			// Token: 0x040018A0 RID: 6304
			Mob207,
			// Token: 0x040018A1 RID: 6305
			Mob208,
			// Token: 0x040018A2 RID: 6306
			Mob209
		}

		// Token: 0x0200048B RID: 1163
		[Serializable]
		public sealed class ExposedParameters : BaseBindableData
		{
			// Token: 0x1700075A RID: 1882
			// (get) Token: 0x0600243B RID: 9275 RVA: 0x000701B2 File Offset: 0x0006E3B2
			// (set) Token: 0x0600243C RID: 9276 RVA: 0x000701BA File Offset: 0x0006E3BA
			public LayerMask AgressionObstaclesLayers
			{
				get
				{
					return this._agressionObstaclesLayers;
				}
				set
				{
					DataBindingUtility.UpdatePropertyValue<LayerMask>(ref this._agressionObstaclesLayers, value, this, "AgressionObstaclesLayers");
				}
			}

			// Token: 0x040018A3 RID: 6307
			[Header("Маска слоев препятствий, через которые не действует агрессия")]
			[SerializeField]
			[PropertyField("AgressionObstaclesLayers")]
			private LayerMask _agressionObstaclesLayers;
		}
	}
}
