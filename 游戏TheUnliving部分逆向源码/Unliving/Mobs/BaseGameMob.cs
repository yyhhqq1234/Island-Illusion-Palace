using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.Animation;
using Common.PivotGroup;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using Game.Gameplay;
using Game.LevelGeneration;
using Game.PassiveAbilities;
using Game.Stats;
using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.AbilityResources;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;
using Unliving.Pickables;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001B6 RID: 438
	[DisallowMultipleComponent]
	[SelectionBase]
	public abstract class BaseGameMob : GameBehaviourBase, IGameMob, IBuffableObject, IBoundingCircle, IPivotGroupProvider<string>, IMovableObject, ILocationChunkVisitor, ILocationObject, IStatsOwner<MobStatModifier>, IAbilitiesEnergyProvider, IAppliedDamageFeedbackReceiver, IAbilityUsingFeedbackReceiver, IAbilityEffectReceiver, IAbilityPrepInterruptionFeedbackReceiver, IAbilityResourcesCollectingEntity, INotifyAbilityUsedOnTarget, INotifyAbilityEffectReceived
	{
		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06000CBD RID: 3261 RVA: 0x00028939 File Offset: 0x00026B39
		// (set) Token: 0x06000CBE RID: 3262 RVA: 0x00028941 File Offset: 0x00026B41
		public GameObject MobPrototypeObject { get; internal set; }

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06000CBF RID: 3263 RVA: 0x0002894A File Offset: 0x00026B4A
		public Transform Transform
		{
			get
			{
				return this.cachedTransform;
			}
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06000CC0 RID: 3264 RVA: 0x00028954 File Offset: 0x00026B54
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

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06000CC1 RID: 3265 RVA: 0x0002897A File Offset: 0x00026B7A
		string IGameMob.Name
		{
			get
			{
				return this.CachedName;
			}
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06000CC2 RID: 3266 RVA: 0x00028982 File Offset: 0x00026B82
		GameObject IGameMob.GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000CC3 RID: 3267 RVA: 0x0002898A File Offset: 0x00026B8A
		// (set) Token: 0x06000CC4 RID: 3268 RVA: 0x00028992 File Offset: 0x00026B92
		public SpriteRenderer Renderer
		{
			get
			{
				return this._renderer;
			}
			set
			{
				this._renderer = value;
			}
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000CC5 RID: 3269 RVA: 0x0002899B File Offset: 0x00026B9B
		// (set) Token: 0x06000CC6 RID: 3270 RVA: 0x000289A3 File Offset: 0x00026BA3
		public bool IsRendererVisible
		{
			get
			{
				return this.isRendererVisible;
			}
			set
			{
				if (this.isRendererVisible == value)
				{
					return;
				}
				this.isRendererVisible = value;
				Action<bool> rendererVisibilityChanged = this.RendererVisibilityChanged;
				if (rendererVisibilityChanged == null)
				{
					return;
				}
				rendererVisibilityChanged(value);
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000CC7 RID: 3271 RVA: 0x000289C7 File Offset: 0x00026BC7
		public Rigidbody2D Rigidbody
		{
			get
			{
				if (this.rigidbody == null)
				{
					base.TryGetComponent<Rigidbody2D>(out this.rigidbody);
				}
				return this.rigidbody;
			}
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000CC8 RID: 3272 RVA: 0x000289EA File Offset: 0x00026BEA
		public Collider2D HitCollider
		{
			get
			{
				if (this._hitCollider == null)
				{
					base.TryGetComponent<Collider2D>(out this._hitCollider);
				}
				return this._hitCollider;
			}
		}

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000CC9 RID: 3273 RVA: 0x00028A0D File Offset: 0x00026C0D
		public Animator Animator
		{
			get
			{
				if (this.animator == null)
				{
					base.TryGetComponent<Animator>(out this.animator);
				}
				return this.animator;
			}
		}

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000CCA RID: 3274 RVA: 0x00028A30 File Offset: 0x00026C30
		public CommonAnimationController AnimationController
		{
			get
			{
				if (this.animationController == null)
				{
					base.TryGetComponent<CommonAnimationController>(out this.animationController);
				}
				return this.animationController;
			}
		}

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000CCB RID: 3275 RVA: 0x00028A53 File Offset: 0x00026C53
		public IPickableObjectCollector PickableObjectsController
		{
			get
			{
				if (this._pickableObjectsController == null)
				{
					base.TryGetComponent<IPickableObjectCollector>(out this._pickableObjectsController);
				}
				return this._pickableObjectsController;
			}
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000CCC RID: 3276 RVA: 0x00028A70 File Offset: 0x00026C70
		// (set) Token: 0x06000CCD RID: 3277 RVA: 0x00028A78 File Offset: 0x00026C78
		public Vector2 Position
		{
			get
			{
				return this.cachedPosition;
			}
			set
			{
				base.transform.position = value;
				this.cachedPosition = value;
				if (!this.isInitialized)
				{
					return;
				}
				if (this.Rigidbody != null)
				{
					this.rigidbody.position = value;
				}
				if (this.NavMeshAgent != null)
				{
					this.navMeshAgent.Warp(value);
				}
			}
		}

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000CCE RID: 3278 RVA: 0x00028AE0 File Offset: 0x00026CE0
		// (set) Token: 0x06000CCF RID: 3279 RVA: 0x00028B44 File Offset: 0x00026D44
		public float Radius
		{
			get
			{
				if (this._radius <= 0f)
				{
					this._radius = GameplayUtility.GetMaxObstacleRadius(this, this.HitCollider);
					if (this.NavMeshAgent != null && this.navMeshAgent.radius > this._radius)
					{
						this._radius = this.navMeshAgent.radius;
					}
				}
				return this._radius;
			}
			set
			{
				this._radius = value;
			}
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06000CD0 RID: 3280 RVA: 0x00028B50 File Offset: 0x00026D50
		public Vector3 CurrentVelocity
		{
			get
			{
				if (this.MotionController != null)
				{
					return this.MotionController.CurrentVelocity;
				}
				if (this.IsActiveNavMeshAgent())
				{
					return this.navMeshAgent.velocity;
				}
				if (this.Rigidbody != null)
				{
					return this.rigidbody.velocity;
				}
				return default(Vector3);
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06000CD1 RID: 3281 RVA: 0x00028BB2 File Offset: 0x00026DB2
		// (set) Token: 0x06000CD2 RID: 3282 RVA: 0x00028BBA File Offset: 0x00026DBA
		public GameLocation CurrentLocation { get; private set; }

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000CD3 RID: 3283 RVA: 0x00028BC3 File Offset: 0x00026DC3
		// (set) Token: 0x06000CD4 RID: 3284 RVA: 0x00028BCB File Offset: 0x00026DCB
		public ILocationChunk CurrentLocationChunk
		{
			get
			{
				return this.currentLocationChunk;
			}
			set
			{
				this.lastLocationChunk = this.currentLocationChunk;
				this.currentLocationChunk = value;
			}
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000CD5 RID: 3285 RVA: 0x00028BE0 File Offset: 0x00026DE0
		// (set) Token: 0x06000CD6 RID: 3286 RVA: 0x00028BE8 File Offset: 0x00026DE8
		public virtual GameMobsGroupControllerBase Group
		{
			get
			{
				return this.group;
			}
			set
			{
				if (this.group == value)
				{
					return;
				}
				GameMobsGroupControllerBase lastGroup = this.group;
				this.group = value;
				this.OnGroupChanged(lastGroup, value);
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000CD7 RID: 3287 RVA: 0x00028C15 File Offset: 0x00026E15
		// (set) Token: 0x06000CD8 RID: 3288 RVA: 0x00028C1D File Offset: 0x00026E1D
		public GameMobsGroupControllerBase LastGroup { get; private set; }

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000CD9 RID: 3289 RVA: 0x00028C26 File Offset: 0x00026E26
		// (set) Token: 0x06000CDA RID: 3290 RVA: 0x00028C2E File Offset: 0x00026E2E
		public ProjectileAbilityBase.ProjectileLaunchPoint[] ShootingPoints
		{
			get
			{
				return this._shootingPoints;
			}
			set
			{
				this._shootingPoints = value;
			}
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000CDB RID: 3291 RVA: 0x00028C37 File Offset: 0x00026E37
		// (set) Token: 0x06000CDC RID: 3292 RVA: 0x00028C57 File Offset: 0x00026E57
		[StatProperty(MobStatID.MobHealth)]
		public float MaxHitPoints
		{
			get
			{
				if (this._hitPointsController.IsNull())
				{
					return 0f;
				}
				return this._hitPointsController.InitialHitPoints;
			}
			protected set
			{
				if (!this._hitPointsController.IsNull())
				{
					this._hitPointsController.InitialHitPoints = value;
				}
			}
		}

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000CDD RID: 3293 RVA: 0x00028C72 File Offset: 0x00026E72
		// (set) Token: 0x06000CDE RID: 3294 RVA: 0x00028C8D File Offset: 0x00026E8D
		[StatProperty(MobStatID.MobSpeed)]
		public float Speed
		{
			get
			{
				if (!this.hasNavmeshAgent)
				{
					return 0f;
				}
				return this.navMeshAgent.speed;
			}
			protected set
			{
				if (this.hasNavmeshAgent)
				{
					this.navMeshAgent.speed = value;
				}
			}
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000CDF RID: 3295 RVA: 0x00028CA3 File Offset: 0x00026EA3
		// (set) Token: 0x06000CE0 RID: 3296 RVA: 0x00028CAC File Offset: 0x00026EAC
		[StatProperty(MobStatID.MobCrowdPassPriority)]
		public float CrowdPassPriority
		{
			get
			{
				return (float)this.crowdPassPriority;
			}
			internal set
			{
				this.crowdPassPriority = (int)value;
			}
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000CE1 RID: 3297 RVA: 0x00028CB6 File Offset: 0x00026EB6
		// (set) Token: 0x06000CE2 RID: 3298 RVA: 0x00028CBE File Offset: 0x00026EBE
		public bool IsKinematic
		{
			get
			{
				return this.isKinematic;
			}
			set
			{
				this.SetKinematic(value);
			}
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000CE3 RID: 3299 RVA: 0x00028CC7 File Offset: 0x00026EC7
		// (set) Token: 0x06000CE4 RID: 3300 RVA: 0x00028CCF File Offset: 0x00026ECF
		Vector2 IBoundingCircle.Position
		{
			get
			{
				return this.HitColliderCenter;
			}
			set
			{
				this.Position = value;
			}
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000CE5 RID: 3301 RVA: 0x00028CD8 File Offset: 0x00026ED8
		// (set) Token: 0x06000CE6 RID: 3302 RVA: 0x00028CE0 File Offset: 0x00026EE0
		float IAbilityResourcesCollectingEntity.ResourcesGatheringDurationOverride
		{
			get
			{
				return this.abilityResourcesGatheringDurationOverride;
			}
			set
			{
				this.abilityResourcesGatheringDurationOverride = value;
			}
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000CE7 RID: 3303 RVA: 0x00028CE9 File Offset: 0x00026EE9
		public NavMeshAgent NavMeshAgent
		{
			get
			{
				if (!this.hasNavmeshAgent)
				{
					this.hasNavmeshAgent = base.TryGetComponent<NavMeshAgent>(out this.navMeshAgent);
				}
				return this.navMeshAgent;
			}
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000CE8 RID: 3304 RVA: 0x00028D0B File Offset: 0x00026F0B
		public bool IsInitialized
		{
			get
			{
				return this.isInitialized;
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000CE9 RID: 3305 RVA: 0x00028D13 File Offset: 0x00026F13
		public int LayerMask
		{
			get
			{
				return this.layerMask;
			}
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000CEA RID: 3306 RVA: 0x00028D1B File Offset: 0x00026F1B
		public float DeferredUpdateDelay
		{
			get
			{
				return this.deferredUpdateDelay;
			}
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000CEB RID: 3307 RVA: 0x00028D23 File Offset: 0x00026F23
		public bool IsStaticCrowdObstacle
		{
			get
			{
				return this.isCrowdObstacle && this._isStaticCrowdObstacle;
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000CEC RID: 3308 RVA: 0x00028D35 File Offset: 0x00026F35
		public IDamageable HitPointsController
		{
			get
			{
				return this._hitPointsController;
			}
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000CED RID: 3309 RVA: 0x00028D3D File Offset: 0x00026F3D
		public IBuffsController BuffsController
		{
			get
			{
				return this.buffsController;
			}
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000CEE RID: 3310 RVA: 0x00028D45 File Offset: 0x00026F45
		public AbilityResourcesGenerator ResourcesGenerator
		{
			get
			{
				return this.resourcesGenerator;
			}
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000CEF RID: 3311 RVA: 0x00028D4D File Offset: 0x00026F4D
		public AbilityEffectZone AuraEffectController
		{
			get
			{
				return this.auraEffectController;
			}
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000CF0 RID: 3312 RVA: 0x00028D55 File Offset: 0x00026F55
		public LocationChunkMobsGridController.GridAgent LocationChunkGridAgent
		{
			get
			{
				return this.locationChunkGridAgent;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000CF1 RID: 3313 RVA: 0x00028D5D File Offset: 0x00026F5D
		public Vector2 HitColliderCenter
		{
			get
			{
				return this.cachedPosition + this.hitColliderCenterOffset;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000CF2 RID: 3314 RVA: 0x00028D70 File Offset: 0x00026F70
		public TaggedPivotGroup TaggedPivotsGroup
		{
			get
			{
				return this.taggedPivotsGroup;
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000CF3 RID: 3315 RVA: 0x00028D78 File Offset: 0x00026F78
		public virtual GameMobFactions Faction
		{
			get
			{
				if (this.group == null)
				{
					return this.defaultFaction;
				}
				return this.group.Faction;
			}
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000CF4 RID: 3316 RVA: 0x00028D94 File Offset: 0x00026F94
		public virtual bool AffectLocationChunkVisibility
		{
			get
			{
				return this.isPlayerGroupMob;
			}
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000CF5 RID: 3317 RVA: 0x00028D9C File Offset: 0x00026F9C
		public ILocationChunk LastLocationChunk
		{
			get
			{
				return this.lastLocationChunk;
			}
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000CF6 RID: 3318 RVA: 0x00028DA4 File Offset: 0x00026FA4
		public bool IsChunkTransitionInProgress
		{
			get
			{
				return this.isChunkTransitionInProgress;
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000CF7 RID: 3319 RVA: 0x00028DAC File Offset: 0x00026FAC
		public bool IsPlayerMob
		{
			get
			{
				return this.isPlayerGroupMob || this.isSummonedByPlayer || this.Faction == GameMobFactions.PLAYER;
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000CF8 RID: 3320 RVA: 0x00028DCC File Offset: 0x00026FCC
		public virtual Vector2 CurrentLookDirection
		{
			get
			{
				return new Vector2
				{
					x = base.transform.localScale.x
				};
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000CF9 RID: 3321 RVA: 0x00028DF9 File Offset: 0x00026FF9
		public IReadOnlyCollection<BaseGameMob> CurrentThreateners
		{
			get
			{
				return this.currentThreateners;
			}
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000CFA RID: 3322 RVA: 0x00028E01 File Offset: 0x00027001
		public IReadOnlyCollection<BaseGameMob> CurrentAttackers
		{
			get
			{
				return this.currentAttackers;
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000CFB RID: 3323 RVA: 0x00028E09 File Offset: 0x00027009
		public bool IsCutsceneInProgress
		{
			get
			{
				return this.isCutsceneInProgress;
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000CFC RID: 3324 RVA: 0x00028E11 File Offset: 0x00027011
		public bool IsRevived
		{
			get
			{
				return this.creationType == GameMobCreationType.Revived;
			}
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000CFD RID: 3325 RVA: 0x00028E1C File Offset: 0x0002701C
		public object RevivingContext
		{
			get
			{
				return this.revivingContext;
			}
		}

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000CFE RID: 3326 RVA: 0x00028E24 File Offset: 0x00027024
		public GameMobSummoningContext SummonerInfo
		{
			get
			{
				return this.summonerInfo;
			}
		}

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000CFF RID: 3327 RVA: 0x00028E2C File Offset: 0x0002702C
		public bool IsSummoned
		{
			get
			{
				return this.creationType == GameMobCreationType.Summoned;
			}
		}

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000D00 RID: 3328 RVA: 0x00028E37 File Offset: 0x00027037
		public UnityEngine.Object Sacrificer
		{
			get
			{
				return this.sacrificer;
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000D01 RID: 3329 RVA: 0x00028E3F File Offset: 0x0002703F
		public bool IsSacrificed
		{
			get
			{
				return this.isSacrificed;
			}
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000D02 RID: 3330 RVA: 0x00028E47 File Offset: 0x00027047
		public bool IsKilled
		{
			get
			{
				return this.isKilled;
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06000D03 RID: 3331 RVA: 0x00028E4F File Offset: 0x0002704F
		Vector3 IMovableObject.Position
		{
			get
			{
				return this.cachedPosition;
			}
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06000D04 RID: 3332 RVA: 0x00028E5C File Offset: 0x0002705C
		int? ILocationObject.LocationObjectType
		{
			get
			{
				return new int?((int)((this.locationObjectType == LocationObjectType.Undefined) ? (this.locationObjectType = this.GetLocationObjectType()) : this.locationObjectType));
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06000D05 RID: 3333 RVA: 0x00028E8D File Offset: 0x0002708D
		float ILocationObject.Orientation
		{
			get
			{
				return base.transform.GetRotation2D(false);
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000D06 RID: 3334 RVA: 0x00028E9B File Offset: 0x0002709B
		IPivotGroup<string> IPivotGroupProvider<string>.PivotGroup
		{
			get
			{
				return this.taggedPivotsGroup;
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000D07 RID: 3335 RVA: 0x00028EA3 File Offset: 0x000270A3
		IStatsController<MobStatModifier> IStatsOwner<MobStatModifier>.StatsController
		{
			get
			{
				return this.StatsController;
			}
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000D08 RID: 3336 RVA: 0x00028EAB File Offset: 0x000270AB
		float IAbilitiesEnergyProvider.CurrentEnergy
		{
			get
			{
				if (!this.CanSpendEnergyOnAbilities())
				{
					return 0f;
				}
				return this._hitPointsController.CurrentHitPoints;
			}
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000D09 RID: 3337 RVA: 0x00028EC6 File Offset: 0x000270C6
		bool IGameMob.IsCrowdObstacle
		{
			get
			{
				return this.isCrowdObstacle;
			}
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000D0A RID: 3338 RVA: 0x00028ECE File Offset: 0x000270CE
		bool IGameMob.IsMinorAttackTarget
		{
			get
			{
				return this.isMinorAttackTarget;
			}
		}

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000D0B RID: 3339
		public abstract bool IsCharacter { get; }

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000D0C RID: 3340
		public abstract GameMobMotionControllerBase MotionController { get; }

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000D0D RID: 3341
		public abstract GameMobAIController AIController { get; }

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06000D0E RID: 3342
		public abstract GameAbilitiesController AbilitiesController { get; }

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06000D0F RID: 3343
		public abstract BasePassiveAbilitiesController PassiveAbilitiesController { get; }

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06000D10 RID: 3344
		public abstract StatsControllerBase<MobStatModifier> StatsController { get; }

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x06000D11 RID: 3345 RVA: 0x00028ED6 File Offset: 0x000270D6
		public virtual bool IsDynamicLocationObject
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06000D12 RID: 3346
		protected abstract bool CanGenerateResources { get; }

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06000D13 RID: 3347 RVA: 0x00028EDC File Offset: 0x000270DC
		protected virtual bool ForceRegisterInLocationChunk
		{
			get
			{
				return this.Rigidbody == null || !this.rigidbody.simulated || this.rigidbody.bodyType == RigidbodyType2D.Static || this.HitCollider == null || !this._hitCollider.enabled;
			}
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000D14 RID: 3348 RVA: 0x00028F30 File Offset: 0x00027130
		// (set) Token: 0x06000D15 RID: 3349 RVA: 0x00028F38 File Offset: 0x00027138
		private protected bool IsDeferredUpdateStep { protected get; private set; }

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000D16 RID: 3350 RVA: 0x00028F41 File Offset: 0x00027141
		protected new bool IsDestroyed
		{
			get
			{
				return this.isDestroyed;
			}
		}

		// Token: 0x14000089 RID: 137
		// (add) Token: 0x06000D17 RID: 3351 RVA: 0x00028F4C File Offset: 0x0002714C
		// (remove) Token: 0x06000D18 RID: 3352 RVA: 0x00028F84 File Offset: 0x00027184
		public event Action<ILocationChunk, ILocationChunk> LocationChunkEntered;

		// Token: 0x1400008A RID: 138
		// (add) Token: 0x06000D19 RID: 3353 RVA: 0x00028FBC File Offset: 0x000271BC
		// (remove) Token: 0x06000D1A RID: 3354 RVA: 0x00028FF4 File Offset: 0x000271F4
		public event Action<BaseGameMob, GameMobsGroupControllerBase> GroupChanged;

		// Token: 0x1400008B RID: 139
		// (add) Token: 0x06000D1B RID: 3355 RVA: 0x0002902C File Offset: 0x0002722C
		// (remove) Token: 0x06000D1C RID: 3356 RVA: 0x00029064 File Offset: 0x00027264
		public event Action<BaseGameMob, BaseGameMob.GroupModificationArgs> GroupModified;

		// Token: 0x1400008C RID: 140
		// (add) Token: 0x06000D1D RID: 3357 RVA: 0x0002909C File Offset: 0x0002729C
		// (remove) Token: 0x06000D1E RID: 3358 RVA: 0x000290D4 File Offset: 0x000272D4
		public event Action<IDamageable, float> DamageApplied;

		// Token: 0x1400008D RID: 141
		// (add) Token: 0x06000D1F RID: 3359 RVA: 0x0002910C File Offset: 0x0002730C
		// (remove) Token: 0x06000D20 RID: 3360 RVA: 0x00029144 File Offset: 0x00027344
		public event Action<IAbility, object, object> AbilityUsedOnTarget;

		// Token: 0x1400008E RID: 142
		// (add) Token: 0x06000D21 RID: 3361 RVA: 0x0002917C File Offset: 0x0002737C
		// (remove) Token: 0x06000D22 RID: 3362 RVA: 0x000291B4 File Offset: 0x000273B4
		public event Action<BaseAbility, AbilityEffectBase, float> AbilityEffectReceived;

		// Token: 0x1400008F RID: 143
		// (add) Token: 0x06000D23 RID: 3363 RVA: 0x000291EC File Offset: 0x000273EC
		// (remove) Token: 0x06000D24 RID: 3364 RVA: 0x00029224 File Offset: 0x00027424
		public event Action<IAbility> AbilityPrepInterrupted;

		// Token: 0x14000090 RID: 144
		// (add) Token: 0x06000D25 RID: 3365 RVA: 0x0002925C File Offset: 0x0002745C
		// (remove) Token: 0x06000D26 RID: 3366 RVA: 0x00029294 File Offset: 0x00027494
		public event Action<string> AnimationEventFired;

		// Token: 0x14000091 RID: 145
		// (add) Token: 0x06000D27 RID: 3367 RVA: 0x000292CC File Offset: 0x000274CC
		// (remove) Token: 0x06000D28 RID: 3368 RVA: 0x00029304 File Offset: 0x00027504
		public event Action DeferredUpdatePerformed;

		// Token: 0x14000092 RID: 146
		// (add) Token: 0x06000D29 RID: 3369 RVA: 0x0002933C File Offset: 0x0002753C
		// (remove) Token: 0x06000D2A RID: 3370 RVA: 0x00029374 File Offset: 0x00027574
		public event Action<bool> RendererVisibilityChanged;

		// Token: 0x14000093 RID: 147
		// (add) Token: 0x06000D2B RID: 3371 RVA: 0x000293AC File Offset: 0x000275AC
		// (remove) Token: 0x06000D2C RID: 3372 RVA: 0x000293E4 File Offset: 0x000275E4
		public event Action<BaseGameMob, IRevivableGameMob> RevivedMob;

		// Token: 0x14000094 RID: 148
		// (add) Token: 0x06000D2D RID: 3373 RVA: 0x0002941C File Offset: 0x0002761C
		// (remove) Token: 0x06000D2E RID: 3374 RVA: 0x00029454 File Offset: 0x00027654
		public event Action<BaseGameMob> Sacrificed;

		// Token: 0x14000095 RID: 149
		// (add) Token: 0x06000D2F RID: 3375 RVA: 0x0002948C File Offset: 0x0002768C
		// (remove) Token: 0x06000D30 RID: 3376 RVA: 0x000294C4 File Offset: 0x000276C4
		public event Action<BaseGameMob, AbilityResourcesCollector, float> CollectingAbilityResources;

		// Token: 0x14000096 RID: 150
		// (add) Token: 0x06000D31 RID: 3377 RVA: 0x000294FC File Offset: 0x000276FC
		// (remove) Token: 0x06000D32 RID: 3378 RVA: 0x00029534 File Offset: 0x00027734
		public event Action<BaseAbility, IReadOnlyList<CollectableAbilityResource>> ConsumingAbilityResources;

		// Token: 0x14000097 RID: 151
		// (add) Token: 0x06000D33 RID: 3379 RVA: 0x0002956C File Offset: 0x0002776C
		// (remove) Token: 0x06000D34 RID: 3380 RVA: 0x000295A4 File Offset: 0x000277A4
		public event Action<IGameMob> Killed;

		// Token: 0x06000D35 RID: 3381 RVA: 0x000295D9 File Offset: 0x000277D9
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void UpdateCachedPosition()
		{
			this.cachedPosition = this.cachedTransform.position;
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x000295F1 File Offset: 0x000277F1
		private void ResetLocationObjectType()
		{
			this.locationObjectType = LocationObjectType.Undefined;
		}

		// Token: 0x06000D37 RID: 3383 RVA: 0x000295FC File Offset: 0x000277FC
		private Bounds GetSelectionBounds()
		{
			Bounds result = this.selectionBounds;
			result.center += base.transform.position;
			return result;
		}

		// Token: 0x06000D38 RID: 3384 RVA: 0x00029630 File Offset: 0x00027830
		private void SetKinematic(bool isKinematic)
		{
			if (this.isKinematic == isKinematic)
			{
				return;
			}
			bool flag = !isKinematic;
			if (this.NavMeshAgent != null)
			{
				flag &= !this.isKilled;
				this.navMeshAgent.enabled = flag;
				if (flag)
				{
					if (this.IsActiveNavMeshAgent())
					{
						this.navMeshAgent.ResetPath();
						this.navMeshAgent.Warp(base.transform.position);
					}
					else
					{
						this.Position = base.transform.position;
					}
				}
			}
			this.isKinematic = isKinematic;
			this.OnKinematicStateChanged(isKinematic);
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x000296C8 File Offset: 0x000278C8
		private void SetShootingPoints(IAbility mobAbility)
		{
			if (this._shootingPoints == null || this._shootingPoints.Length == 0)
			{
				return;
			}
			ProjectileAbilityBase projectileAbilityBase = mobAbility as ProjectileAbilityBase;
			if (projectileAbilityBase != null && !projectileAbilityBase.HasProjectileLaunchPoints)
			{
				projectileAbilityBase.ProjectileLaunchPoints = this._shootingPoints;
			}
			ICompositeAbility compositeAbility = mobAbility as ICompositeAbility;
			if (compositeAbility != null)
			{
				IList<IAbility> childAbilities = compositeAbility.ChildAbilities;
				for (int i = 0; i < childAbilities.Count; i++)
				{
					this.SetShootingPoints(childAbilities[i]);
				}
			}
		}

		// Token: 0x06000D3A RID: 3386 RVA: 0x00029734 File Offset: 0x00027934
		private void ResetShootingPoints(IAbility mobAbility)
		{
			ProjectileAbilityBase projectileAbilityBase = mobAbility as ProjectileAbilityBase;
			if (projectileAbilityBase != null && projectileAbilityBase.ProjectileLaunchPoints == this._shootingPoints)
			{
				projectileAbilityBase.ProjectileLaunchPoints = null;
			}
		}

		// Token: 0x06000D3B RID: 3387 RVA: 0x00029760 File Offset: 0x00027960
		private bool CanSpendEnergyOnAbilities()
		{
			return !this.ignoreAbilitiesCost && (!this._hitPointsController.IsNull() || !this.abilitiesEnergySource.IsNull());
		}

		// Token: 0x06000D3C RID: 3388 RVA: 0x0002978C File Offset: 0x0002798C
		private void CreateResourcesGenerator()
		{
			ICollectableAbilityResourcesFactory resourcesFactory = base.CurrentGame.Services.Get<ICollectableAbilityResourcesFactory>();
			AbilityResourcesGenerator.Parameters parameters = this.resourcesGeneratorData.ToResourcesGeneratorParams(this);
			this.resourcesGenerator = new AbilityResourcesGenerator(this, parameters, this._hitPointsController, resourcesFactory);
		}

		// Token: 0x06000D3D RID: 3389 RVA: 0x000297CC File Offset: 0x000279CC
		private void SetDeadMobAsAbilityResource()
		{
			if (this.isDestroyed || (!this.forceSetDeadMobAsAbilityResource && (this.isEnvironmentMob || !this.IsCharacter || this.IsPlayerMob || this.IsSacrificed)))
			{
				return;
			}
			if (this.resourcesGenerator != null)
			{
				AbilityResourcesGenerator.GeneratedResourceInfo generatedResourceInfo;
				this.resourcesGenerator.TryGenerateCorpseResource(this, out generatedResourceInfo);
			}
		}

		// Token: 0x06000D3E RID: 3390 RVA: 0x00029824 File Offset: 0x00027A24
		private void CreateAuraEffectController()
		{
			if (this.auraEffectAbility != null)
			{
				this.auraEffectController = AbilitiesFactory.CreateAuraEffect(this, this.auraEffectAbility, null);
			}
		}

		// Token: 0x06000D3F RID: 3391 RVA: 0x0002985C File Offset: 0x00027A5C
		protected virtual LocationObjectType GetLocationObjectType()
		{
			LocationObjectType locationObjectType = this.IsPlayerMob ? LocationObjectType.PlayerMob : (this.isEnvironmentMob ? LocationObjectType.MinorMob : LocationObjectType.Mob);
			if (!this.isKilled)
			{
				return locationObjectType;
			}
			return locationObjectType | LocationObjectType.Corpse;
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x00029891 File Offset: 0x00027A91
		public bool HasEnoughEnergyForAbility(float abilityCost)
		{
			if (this.abilitiesEnergySource != null)
			{
				return this.abilitiesEnergySource.HasEnoughEnergy(abilityCost);
			}
			return this._hitPointsController == null || this._hitPointsController.CurrentHitPoints >= abilityCost;
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x000298C3 File Offset: 0x00027AC3
		protected virtual Transform GetDefaultShootingPointsTransform()
		{
			return this.cachedTransform;
		}

		// Token: 0x06000D42 RID: 3394 RVA: 0x000298CC File Offset: 0x00027ACC
		public void SetCreationType(GameMobCreationType creationType, object context)
		{
			if (creationType == GameMobCreationType.Revived)
			{
				this.revivingContext = context;
			}
			else if (creationType == GameMobCreationType.Summoned)
			{
				this.summonerInfo = (GameMobSummoningContext)context;
				IGameMob summoner = this.summonerInfo.summoner;
				this.isSummonedByPlayer = (summoner is PlayerBehaviour || summoner.IsPlayerMob);
			}
			this.creationType = creationType;
		}

		// Token: 0x06000D43 RID: 3395 RVA: 0x00029920 File Offset: 0x00027B20
		public void SetLayer(int layer)
		{
			base.gameObject.layer = layer;
			this.layerMask = 1 << layer;
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x0002993C File Offset: 0x00027B3C
		public void SetLayerOverride(int layerOverride)
		{
			if (layerOverride < -1)
			{
				layerOverride = -1;
			}
			if (this.currentLayerOverride == layerOverride)
			{
				return;
			}
			if (layerOverride < 0)
			{
				this.SetLayer(this.lastLayer);
			}
			else
			{
				if (this.currentLayerOverride < 0)
				{
					this.lastLayer = base.gameObject.layer;
				}
				this.SetLayer(layerOverride);
			}
			this.currentLayerOverride = layerOverride;
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00029994 File Offset: 0x00027B94
		public void ResetLayerOverride()
		{
			this.SetLayerOverride(-1);
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x0002999D File Offset: 0x00027B9D
		public void InitializeMob()
		{
			if (this.isInitialized)
			{
				return;
			}
			this.OnMobInitialization();
			this.isInitialized = true;
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x000299B5 File Offset: 0x00027BB5
		public void ResetLocalDrawingOrder()
		{
			if (this._renderer != null)
			{
				this._renderer.sortingOrder = this.initialLocalDrawingOrder;
			}
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x000299D8 File Offset: 0x00027BD8
		public ILocationChunk ForceGetCurrentLocationChunk()
		{
			if (this.currentLocationChunk != null)
			{
				return this.currentLocationChunk;
			}
			IGameLocationProvider gameLocationProvider;
			if (!base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
			{
				return null;
			}
			return gameLocationProvider.CurrentLocation.GetLocationChunkAtPoint(this.Position, false);
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x00029A1C File Offset: 0x00027C1C
		public virtual bool CanVisitLocationChunk(ILocationChunk locationChunk)
		{
			return locationChunk != null && !this.isKilled;
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x00029A2C File Offset: 0x00027C2C
		public void SetNavMeshAgentActive(bool isActive)
		{
			if (this.isKilled || this.NavMeshAgent == null)
			{
				return;
			}
			this.navMeshAgent.enabled = isActive;
		}

		// Token: 0x06000D4B RID: 3403 RVA: 0x00029A51 File Offset: 0x00027C51
		public bool IsActiveNavMeshAgent()
		{
			return this.hasNavmeshAgent && base.gameObject.activeInHierarchy && this.navMeshAgent.enabled && this.navMeshAgent.updatePosition && this.navMeshAgent.isOnNavMesh;
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x00029A90 File Offset: 0x00027C90
		public float GetCrowdInteractionRadius()
		{
			float radius = this.Radius;
			if (radius <= 0f)
			{
				return 0f;
			}
			return radius + 0.1f;
		}

		// Token: 0x06000D4D RID: 3405 RVA: 0x00029AB9 File Offset: 0x00027CB9
		public void SetCrowdObstacleAvoidanceEnabled(bool isEnabled)
		{
			if (this.navMeshAgent == null)
			{
				return;
			}
			this.navMeshAgent.obstacleAvoidanceType = (isEnabled ? this.initialObstacleAvoidanceType : ObstacleAvoidanceType.NoObstacleAvoidance);
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x00029AE4 File Offset: 0x00027CE4
		public virtual bool CanBeSacrificed(UnityEngine.Object sacrificer, bool checkGroup = true)
		{
			if (this.isKilled || this.isSacrificed || this.isKinematic || (!this.forceSacrifice && this.IsSummoned))
			{
				return false;
			}
			if (checkGroup)
			{
				GameMobsGroupControllerBase gameMobsGroupControllerBase = this.group;
				return ((gameMobsGroupControllerBase != null) ? gameMobsGroupControllerBase.Leader : null) == sacrificer;
			}
			return true;
		}

		// Token: 0x06000D4F RID: 3407 RVA: 0x00029B35 File Offset: 0x00027D35
		public bool Sacrifice(UnityEngine.Object sacrificer, bool killMob = true, bool checkGroup = true)
		{
			if (!this.CanBeSacrificed(sacrificer, checkGroup))
			{
				return false;
			}
			this.sacrificer = sacrificer;
			this.isSacrificed = true;
			Action<BaseGameMob> sacrificed = this.Sacrificed;
			if (sacrificed != null)
			{
				sacrificed(this);
			}
			if (killMob)
			{
				this.KillMob(sacrificer);
			}
			return true;
		}

		// Token: 0x06000D50 RID: 3408 RVA: 0x00029B6E File Offset: 0x00027D6E
		public float GetImpulseDrag()
		{
			return 22f;
		}

		// Token: 0x06000D51 RID: 3409 RVA: 0x00029B75 File Offset: 0x00027D75
		public void BlockMovement(float duration)
		{
			GameMobMotionControllerBase motionController = this.MotionController;
			if (motionController == null)
			{
				return;
			}
			motionController.FreezeMovement(duration, false);
		}

		// Token: 0x06000D52 RID: 3410 RVA: 0x00029B89 File Offset: 0x00027D89
		public void AddMovementImpulse(Vector3 impulse, bool ignoreDamping = false)
		{
			GameMobMotionControllerBase motionController = this.MotionController;
			if (motionController == null)
			{
				return;
			}
			motionController.AddImpulse(impulse, ignoreDamping);
		}

		// Token: 0x06000D53 RID: 3411 RVA: 0x00029BA2 File Offset: 0x00027DA2
		public bool HasThreateners()
		{
			return this.currentThreateners.Count != 0;
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x00029BB2 File Offset: 0x00027DB2
		public bool HasThreatener(BaseGameMob threatener)
		{
			return this.currentThreateners.Contains(threatener);
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x00029BC0 File Offset: 0x00027DC0
		public bool HasAttacker(BaseGameMob attacker)
		{
			return this.currentAttackers.Contains(attacker);
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x00029BD0 File Offset: 0x00027DD0
		public bool CanBeAttackedBy(IGameMob attacker)
		{
			if (this.isKilled || this.isKinematic || !this.isVisibleToEnemies || attacker == null)
			{
				return false;
			}
			if (this.currentAttackers.Count >= this.maxAttackersCount)
			{
				BaseGameMob baseGameMob = attacker as BaseGameMob;
				return baseGameMob != null && this.HasAttacker(baseGameMob);
			}
			return true;
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x00029C22 File Offset: 0x00027E22
		public bool AddThreatener(BaseGameMob threatener)
		{
			return this.currentThreateners.Add(threatener);
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x00029C30 File Offset: 0x00027E30
		public bool AddAttacker(BaseGameMob attacker)
		{
			return this.CanBeAttackedBy(attacker) && this.currentAttackers.Add(attacker);
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x00029C49 File Offset: 0x00027E49
		public bool RemoveThreatener(BaseGameMob threatener)
		{
			return this.currentThreateners.Remove(threatener);
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x00029C57 File Offset: 0x00027E57
		public bool RemoveAttacker(BaseGameMob attacker)
		{
			return this.currentAttackers.Remove(attacker);
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x00029C68 File Offset: 0x00027E68
		public BaseGameMob GetFirstAttacker()
		{
			if (this.currentAttackers.Count != 0)
			{
				using (HashSet<BaseGameMob>.Enumerator enumerator = this.currentAttackers.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						return enumerator.Current;
					}
				}
			}
			return null;
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x00029CC8 File Offset: 0x00027EC8
		public bool IsUnderAttack()
		{
			if (this.currentAttackers.Count != 0)
			{
				foreach (BaseGameMob baseGameMob in this.currentAttackers)
				{
					GameMobAIController aicontroller = baseGameMob.AIController;
					if (aicontroller != null && aicontroller.IsAttacking)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x00029D38 File Offset: 0x00027F38
		public void SetCutsceneActive(bool isCutsceneActive, object cutsceneContext)
		{
			if (this.isCutsceneInProgress == isCutsceneActive)
			{
				return;
			}
			this.isCutsceneInProgress = isCutsceneActive;
			if (isCutsceneActive)
			{
				GameMobMotionControllerBase motionController = this.MotionController;
				if (motionController != null)
				{
					motionController.StopKinematicMotion();
				}
			}
			IResistableDamageable resistableDamageable = this.HitPointsController as IResistableDamageable;
			if (resistableDamageable != null)
			{
				resistableDamageable.SetResistantToAnyHitPointsModification(isCutsceneActive);
			}
			if (this.NavMeshAgent != null && this.IsActiveNavMeshAgent())
			{
				this.navMeshAgent.isStopped = isCutsceneActive;
			}
			base.enabled = !isCutsceneActive;
			this.OnCutsceneStateChanged(isCutsceneActive, cutsceneContext);
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x00029DB5 File Offset: 0x00027FB5
		public void KillMob(object mobKiller)
		{
			if (this.isKilled)
			{
				return;
			}
			if (!this._hitPointsController.IsNull())
			{
				this._hitPointsController.ApplyLethalDamage(mobKiller);
				return;
			}
			this.DestroyMob();
		}

		// Token: 0x06000D5F RID: 3423 RVA: 0x00029DE0 File Offset: 0x00027FE0
		public void KillWithSummoner()
		{
			if (!this.killWithSummoner && this.summonerInfo != null)
			{
				IGameMob summoner = this.summonerInfo.summoner;
				if (summoner.IsKilled)
				{
					this.KillMob(summoner);
				}
				else
				{
					summoner.Killed += new Action<IGameMob>(this.KillMob);
				}
				this.killWithSummoner = true;
			}
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x00029E34 File Offset: 0x00028034
		public virtual void DestroyMob()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06000D61 RID: 3425 RVA: 0x00029E41 File Offset: 0x00028041
		bool IAbilitiesEnergyProvider.HasEnoughEnergy(float amount)
		{
			return !this.CanSpendEnergyOnAbilities() || this.HasEnoughEnergyForAbility(amount);
		}

		// Token: 0x06000D62 RID: 3426 RVA: 0x00029E54 File Offset: 0x00028054
		void IAbilitiesEnergyProvider.ConsumeEnergy(float amount)
		{
			if (amount <= 0f || !this.CanSpendEnergyOnAbilities() || !this.HasEnoughEnergyForAbility(amount))
			{
				return;
			}
			BaseGameMob.AbilitiesEnergyConsumingArgs.amount = amount;
			BaseGameMob.AbilitiesEnergyConsumingArgs.source = this;
			this._hitPointsController.ModifyHitPoints(this, BaseGameMob.AbilitiesEnergyConsumingArgs);
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x00029EA3 File Offset: 0x000280A3
		void IAbilitiesEnergyProvider.RestoreEnergy(float amount)
		{
			if (amount <= 0f || !this.CanSpendEnergyOnAbilities())
			{
				return;
			}
			BaseGameMob.AbilitiesEnergyRestoringArgs.amount = amount;
			this._hitPointsController.ModifyHitPoints(this, BaseGameMob.AbilitiesEnergyRestoringArgs);
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x00029ED3 File Offset: 0x000280D3
		void ILocationChunkVisitor.OnAddedToLocationChunk(ILocationChunk locationChunk)
		{
			if (this.isReadyForUpdate)
			{
				this.OnLocationChunkEntered(locationChunk);
			}
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x00029EE4 File Offset: 0x000280E4
		void ILocationChunkVisitor.OnChunkTransitionStateChanged(ILocationChunkTransitionArea transitionArea, bool isActive)
		{
			this.isChunkTransitionInProgress = isActive;
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x00029EF0 File Offset: 0x000280F0
		protected virtual void OnLocationChunkEntered(ILocationChunk newChunk)
		{
			this.locationChunkGridAgent = null;
			LocationChunkMobsGridController locationChunkMobsGridController;
			if (((Component)newChunk).TryGetComponent<LocationChunkMobsGridController>(out locationChunkMobsGridController))
			{
				this.locationChunkGridAgent = locationChunkMobsGridController.GetGridAgent(this);
			}
			if (this.lastLocationChunk != null)
			{
				this.lastLocationChunk.VisibilityChanged -= this.OnLocationChunkVisibilityChanged;
			}
			newChunk.VisibilityChanged += this.OnLocationChunkVisibilityChanged;
			this.OnLocationChunkVisibilityChanged(newChunk, newChunk.IsVisible);
			Action<ILocationChunk, ILocationChunk> locationChunkEntered = this.LocationChunkEntered;
			if (locationChunkEntered == null)
			{
				return;
			}
			locationChunkEntered(this.lastLocationChunk, newChunk);
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x00029F77 File Offset: 0x00028177
		protected virtual void OnKinematicStateChanged(bool isKinematic)
		{
		}

		// Token: 0x06000D68 RID: 3432 RVA: 0x00029F79 File Offset: 0x00028179
		public virtual void OnAnimationEventFired(string eventArg)
		{
			Action<string> animationEventFired = this.AnimationEventFired;
			if (animationEventFired == null)
			{
				return;
			}
			animationEventFired(eventArg);
		}

		// Token: 0x06000D69 RID: 3433 RVA: 0x00029F8C File Offset: 0x0002818C
		public void OnMobRevived(BaseGameMob revivedMob, IRevivableGameMob deadMob)
		{
			Action<BaseGameMob, IRevivableGameMob> revivedMob2 = this.RevivedMob;
			if (revivedMob2 == null)
			{
				return;
			}
			revivedMob2(revivedMob, deadMob);
		}

		// Token: 0x06000D6A RID: 3434 RVA: 0x00029FA0 File Offset: 0x000281A0
		protected virtual void OnLocationChunkVisibilityChanged(ILocationChunk locationChunk, bool isChunkVisible)
		{
			if (this.IsNull())
			{
				return;
			}
			if (!this.AffectLocationChunkVisibility)
			{
				base.gameObject.SetActive(isChunkVisible);
			}
		}

		// Token: 0x06000D6B RID: 3435 RVA: 0x00029FC0 File Offset: 0x000281C0
		protected virtual void OnGroupChanged(GameMobsGroupControllerBase lastGroup, GameMobsGroupControllerBase newGroup)
		{
			if (lastGroup != null)
			{
				lastGroup.MobAdded -= this.OnMobAddedToGroup;
				lastGroup.MobRemoved -= this.OnMobRemovedFromGroup;
			}
			this.LastGroup = lastGroup;
			if (newGroup != null)
			{
				this.isPlayerGroupMob = (newGroup.Leader is PlayerBehaviour);
				if (this.isPlayerGroupMob && !this.IsLocationObject(LocationObjectType.PlayerMob))
				{
					this.ResetLocationObjectType();
				}
				newGroup.MobAdded += this.OnMobAddedToGroup;
				newGroup.MobRemoved += this.OnMobRemovedFromGroup;
			}
			Action<BaseGameMob, GameMobsGroupControllerBase> groupChanged = this.GroupChanged;
			if (groupChanged == null)
			{
				return;
			}
			groupChanged(this, newGroup);
		}

		// Token: 0x06000D6C RID: 3436 RVA: 0x0002A05F File Offset: 0x0002825F
		private void OnMobAddedToGroup(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			BaseGameMob.CurrentGroupModificationArgs.group = group;
			BaseGameMob.CurrentGroupModificationArgs.affectedGroupMob = mob;
			BaseGameMob.CurrentGroupModificationArgs.mobWasRemovedFromGroup = false;
			Action<BaseGameMob, BaseGameMob.GroupModificationArgs> groupModified = this.GroupModified;
			if (groupModified == null)
			{
				return;
			}
			groupModified(this, BaseGameMob.CurrentGroupModificationArgs);
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x0002A098 File Offset: 0x00028298
		private void OnMobRemovedFromGroup(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			BaseGameMob.CurrentGroupModificationArgs.group = group;
			BaseGameMob.CurrentGroupModificationArgs.affectedGroupMob = mob;
			BaseGameMob.CurrentGroupModificationArgs.mobWasRemovedFromGroup = true;
			Action<BaseGameMob, BaseGameMob.GroupModificationArgs> groupModified = this.GroupModified;
			if (groupModified == null)
			{
				return;
			}
			groupModified(this, BaseGameMob.CurrentGroupModificationArgs);
		}

		// Token: 0x06000D6E RID: 3438 RVA: 0x0002A0D1 File Offset: 0x000282D1
		protected virtual void OnDamageApplied(IDamageable damagedObject, float damageAmount)
		{
		}

		// Token: 0x06000D6F RID: 3439 RVA: 0x0002A0D3 File Offset: 0x000282D3
		protected virtual void OnAbilityAdded(IAbility ability)
		{
			this.SetShootingPoints(ability);
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x0002A0DC File Offset: 0x000282DC
		protected virtual void OnAbilityRemoved(IAbility ability)
		{
			this.ResetShootingPoints(ability);
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x0002A0E5 File Offset: 0x000282E5
		protected virtual void OnCutsceneStateChanged(bool isCutsceneActive, object cutsceneContext)
		{
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x0002A0E7 File Offset: 0x000282E7
		void IAppliedDamageFeedbackReceiver.OnDamageApplied(IDamageable damagedObject, float damageAmount)
		{
			if (damagedObject.Behaviour == this)
			{
				return;
			}
			this.OnDamageApplied(damagedObject, damageAmount);
			Action<IDamageable, float> damageApplied = this.DamageApplied;
			if (damageApplied == null)
			{
				return;
			}
			damageApplied(damagedObject, damageAmount);
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x0002A112 File Offset: 0x00028312
		void IAbilityUsingFeedbackReceiver.OnAbilityUsed(IAbility ability, object abilityTarget, object args)
		{
			Action<IAbility, object, object> abilityUsedOnTarget = this.AbilityUsedOnTarget;
			if (abilityUsedOnTarget == null)
			{
				return;
			}
			abilityUsedOnTarget(ability, abilityTarget, args);
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x0002A127 File Offset: 0x00028327
		void IAbilityEffectReceiver.OnAbilityEffectReceived(BaseAbility ability, AbilityEffectBase abilityEffect, float effectAmount)
		{
			Action<BaseAbility, AbilityEffectBase, float> abilityEffectReceived = this.AbilityEffectReceived;
			if (abilityEffectReceived == null)
			{
				return;
			}
			abilityEffectReceived(ability, abilityEffect, effectAmount);
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x0002A13C File Offset: 0x0002833C
		void IAbilityPrepInterruptionFeedbackReceiver.OnAbilityPrepInterrupted(IAbility ability)
		{
			Action<IAbility> abilityPrepInterrupted = this.AbilityPrepInterrupted;
			if (abilityPrepInterrupted == null)
			{
				return;
			}
			abilityPrepInterrupted(ability);
		}

		// Token: 0x06000D76 RID: 3446 RVA: 0x0002A14F File Offset: 0x0002834F
		void IAbilityResourcesCollectingEntity.OnCollectingResources(AbilityResourcesCollector resourcesCollector, float collectionDuration)
		{
			Action<BaseGameMob, AbilityResourcesCollector, float> collectingAbilityResources = this.CollectingAbilityResources;
			if (collectingAbilityResources == null)
			{
				return;
			}
			collectingAbilityResources(this, resourcesCollector, collectionDuration);
		}

		// Token: 0x06000D77 RID: 3447 RVA: 0x0002A164 File Offset: 0x00028364
		void IAbilityResourcesCollectingEntity.OnConsumingResources(BaseAbility consumingAbility, IReadOnlyList<CollectableAbilityResource> resources)
		{
			Action<BaseAbility, IReadOnlyList<CollectableAbilityResource>> consumingAbilityResources = this.ConsumingAbilityResources;
			if (consumingAbilityResources == null)
			{
				return;
			}
			consumingAbilityResources(consumingAbility, resources);
		}

		// Token: 0x06000D78 RID: 3448 RVA: 0x0002A178 File Offset: 0x00028378
		public override void Initialize(IGame currentGame)
		{
			this.isInitialized = false;
			this.isKinematic = false;
			this.isPlayerGroupMob = false;
			this.isKilled = false;
			this.isDestroyed = false;
			this.cachedTransform = base.transform;
			this.currentLayerOverride = -1;
			this.UpdateCachedPosition();
			this.ResetLocationObjectType();
			base.Initialize(currentGame);
			this._hitPointsController = base.GetComponentInChildren<IDamageable>();
			this.abilitiesEnergySource = this._hitPointsController.CastOrGetComponent<IAbilitiesEnergySource>();
			if (this.NavMeshAgent != null)
			{
				this.initialObstacleAvoidanceType = this.navMeshAgent.obstacleAvoidanceType;
			}
			this.buffsController = new BaseBuffsController(this, this.ignorableBuffs);
			if (this._hitPointsController != null)
			{
				this._hitPointsController.Behaviour = this;
				this._hitPointsController.TotallyDestroyed += this.OnKilled;
			}
			currentGame.Services.TryGet<SelectableObjectsManager2D>(out this.selectionManager);
		}

		// Token: 0x06000D79 RID: 3449 RVA: 0x0002A25C File Offset: 0x0002845C
		protected virtual void OnMobInitialization()
		{
			if (this.layerMask == 0)
			{
				this.SetLayer(base.gameObject.layer);
			}
			if (this._renderer != null || base.TryGetComponent<SpriteRenderer>(out this._renderer))
			{
				this.initialLocalDrawingOrder = this._renderer.sortingOrder;
			}
			if (this.HitCollider != null)
			{
				Vector2 vector = base.transform.lossyScale;
				this.hitColliderCenterOffset = this._hitCollider.offset;
				this.hitColliderCenterOffset.x = this.hitColliderCenterOffset.x * Mathf.Abs(vector.x);
				this.hitColliderCenterOffset.y = this.hitColliderCenterOffset.y * Mathf.Abs(vector.y);
			}
			if (this.navMeshAgent != null)
			{
				this.navMeshAgent.updatePosition = true;
				this.navMeshAgent.updateRotation = false;
				this.navMeshAgent.updateUpAxis = false;
				this.navMeshAgent.enabled = true;
			}
			this.taggedPivotsGroup.Initialize(base.transform, this.taggedPivots);
			this.maxAttackersCount = 100;
			if (this.CanGenerateResources && this.resourcesGeneratorData != null)
			{
				this.CreateResourcesGenerator();
			}
			this.CreateAuraEffectController();
		}

		// Token: 0x06000D7A RID: 3450 RVA: 0x0002A38C File Offset: 0x0002858C
		protected virtual async void OnEnable()
		{
			if (base.CurrentGame == null)
			{
				await base.CurrentGame.WaitGameSessionInitialization();
			}
			this.UpdateCachedPosition();
			IGameLocationProvider gameLocationProvider;
			if (base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
			{
				this.CurrentLocation = gameLocationProvider.CurrentLocation;
			}
			if (this.selectable != null)
			{
				this.selectionManager.RegisterSelectable(this.selectable);
			}
		}

		// Token: 0x06000D7B RID: 3451 RVA: 0x0002A3C8 File Offset: 0x000285C8
		private void Start()
		{
			this.UpdateCachedPosition();
			if (this.creationType == GameMobCreationType.Undefined)
			{
				this.SetCreationType(GameMobCreationType.Default, null);
			}
			this.InitializeMob();
			if (this._shootingPoints != null)
			{
				Transform defaultShootingPointsTransform = this.GetDefaultShootingPointsTransform();
				foreach (ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint in this._shootingPoints)
				{
					if (projectileLaunchPoint.Transform == null)
					{
						projectileLaunchPoint.Transform = defaultShootingPointsTransform;
					}
				}
			}
			if (this.AbilitiesController != null)
			{
				this.cachedAbilitiesController = this.AbilitiesController;
				IReadOnlyList<BaseAbility> abilities = this.cachedAbilitiesController.Abilities;
				for (int j = 0; j < abilities.Count; j++)
				{
					BaseAbility baseAbility = abilities[j];
					if (!(baseAbility == null))
					{
						this.SetShootingPoints(baseAbility);
					}
				}
				this.cachedAbilitiesController.AbilityAdded += this.OnAbilityAdded;
				this.cachedAbilitiesController.AbilityRemoved += this.OnAbilityRemoved;
			}
			float num = UnityEngine.Random.Range((float)this.minDeferredUpdateRate, (float)this.maxDeferredUpdateRate);
			this.deferredUpdateDelay = ((num > 0f) ? (1f / num) : 0f);
			if (this.currentLocationChunk == null && this.ForceRegisterInLocationChunk)
			{
				ILocationChunk locationChunk = this.ForceGetCurrentLocationChunk();
				if (locationChunk != null)
				{
					locationChunk.AddVisitor(this);
				}
			}
			if (this.selectionManager != null && this.selectionBounds.size != default(Vector3))
			{
				this.selectable = new SelectableObjectsManager2D.Selectable(this.GetSelectionBounds(), this, false);
			}
			if (this.currentLocationChunk != null)
			{
				this.OnLocationChunkEntered(this.currentLocationChunk);
			}
			if (base.gameObject.activeInHierarchy && this.selectable != null)
			{
				this.selectionManager.RegisterSelectable(this.selectable);
			}
			IGameSessionManager gameSessionManager;
			if (!this.isEnvironmentMob && this.AbilitiesController != null && base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager))
			{
				gameSessionManager.RegisterMob(this);
			}
			this.isReadyForUpdate = true;
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x0002A5B4 File Offset: 0x000287B4
		protected virtual void Update()
		{
			this.IsDeferredUpdateStep = false;
			if (GameApplication.IsGameStateChanging)
			{
				base.enabled = false;
				return;
			}
			this.UpdateCachedPosition();
			GameAbilitiesController gameAbilitiesController = this.cachedAbilitiesController;
			if (gameAbilitiesController != null)
			{
				gameAbilitiesController.UpdateController();
			}
			BaseBuffsController baseBuffsController = this.buffsController;
			if (baseBuffsController != null)
			{
				baseBuffsController.UpdateBuffs();
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this.IsDeferredUpdateStep = (realtimeSinceStartup > this.nextDeferredUpdateTime);
			if (this.IsDeferredUpdateStep)
			{
				if (this._renderer != null)
				{
					this.IsRendererVisible = this._renderer.isVisible;
				}
				Action deferredUpdatePerformed = this.DeferredUpdatePerformed;
				if (deferredUpdatePerformed != null)
				{
					deferredUpdatePerformed();
				}
				this.nextDeferredUpdateTime = realtimeSinceStartup + this.deferredUpdateDelay;
			}
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x0002A65C File Offset: 0x0002885C
		protected virtual void OnDisable()
		{
			if (this.rigidbody != null && this.rigidbody.bodyType != RigidbodyType2D.Static)
			{
				this.rigidbody.velocity = default(Vector2);
			}
			if (this.hasNavmeshAgent)
			{
				this.navMeshAgent.velocity = default(Vector3);
			}
			if (this.selectable != null)
			{
				this.selectionManager.UnregisterSelectable(this.selectable);
			}
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x0002A6D0 File Offset: 0x000288D0
		protected virtual void OnKilled(IDamageable hitPointsController)
		{
			if (this.isKilled)
			{
				return;
			}
			this.isKilled = true;
			this.hasNavmeshAgent = false;
			GameObject gameObject = base.gameObject;
			gameObject.name += "_dead";
			if (this.auraEffectController != null)
			{
				this.auraEffectController.gameObject.SetActive(false);
			}
			if (this.selectionManager != null && this.selectable != null)
			{
				this.selectionManager.UnregisterSelectable(this.selectable);
				this.selectable = null;
			}
			if (!this.group.IsNull())
			{
				GameMobsGroupControllerBase gameMobsGroupControllerBase = (!this.IsPlayerMob) ? this.group : null;
				this.LastGroup = this.group;
				this.group.RemoveMob(this);
				this.group = gameMobsGroupControllerBase;
			}
			if (this.buffsController != null)
			{
				this.buffsController.CompleteAllBuffs();
				this.buffsController = null;
			}
			if (this.AbilitiesController != null)
			{
				this.AbilitiesController.AbilityAdded -= this.OnAbilityAdded;
				this.AbilitiesController.AbilityRemoved -= this.OnAbilityRemoved;
			}
			if (!this.currentLocationChunk.IsNull())
			{
				ILocationChunk locationChunk = this.currentLocationChunk;
				this.RemoveFromAllChunks(this.lastLocationChunk);
				if (!this.isDestroyed)
				{
					this.ResetLocationObjectType();
					locationChunk.AddEnvironmentObject(this);
				}
				else
				{
					locationChunk.VisibilityChanged -= this.OnLocationChunkVisibilityChanged;
				}
			}
			if (!this._hitPointsController.IsNull())
			{
				this._hitPointsController.TotallyDestroyed -= this.OnKilled;
			}
			if (this.killWithSummoner)
			{
				this.summonerInfo.summoner.Killed -= new Action<IGameMob>(this.KillMob);
			}
			NavMeshAgent navMeshAgent = this.NavMeshAgent;
			if (navMeshAgent != null)
			{
				navMeshAgent.enabled = false;
			}
			Rigidbody2D rigidbody2D = this.Rigidbody;
			if (rigidbody2D != null && rigidbody2D.bodyType != RigidbodyType2D.Static)
			{
				rigidbody2D.velocity = default(Vector2);
				rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			}
			if (this._hitCollider != null)
			{
				this._hitCollider.isTrigger = true;
			}
			if (!GameApplication.IsGameStateChanging)
			{
				this.SetDeadMobAsAbilityResource();
				Action<IGameMob> killed = this.Killed;
				if (killed == null)
				{
					return;
				}
				killed(this);
			}
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x0002A900 File Offset: 0x00028B00
		protected override void OnDestroy()
		{
			this.isDestroyed = true;
			base.OnDestroy();
			this.OnKilled(this._hitPointsController);
			AbilityResourcesGenerator abilityResourcesGenerator = this.resourcesGenerator;
			if (abilityResourcesGenerator != null)
			{
				abilityResourcesGenerator.Dispose();
			}
			GameAbilitiesController abilitiesController = this.AbilitiesController;
			if (abilitiesController != null)
			{
				abilitiesController.RemoveAllAbilities(null);
			}
			if (!this.currentLocationChunk.IsNull())
			{
				this.currentLocationChunk.VisibilityChanged -= this.OnLocationChunkVisibilityChanged;
				this.currentLocationChunk.RemoveEnvironmentObject(this);
			}
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x0002A984 File Offset: 0x00028B84
		protected virtual void OnDrawGizmosSelected()
		{
			if (this._radius > 0f)
			{
				Gizmos.color = Color.yellow.SetA(0.5f);
				Gizmos.DrawSphere(this.HitColliderCenter, this._radius);
			}
			Bounds bounds = this.GetSelectionBounds();
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
			ProjectileAbilityBase.DrawProjectileLaunchPointsGizmo(this._shootingPoints, this.GetDefaultShootingPointsTransform(), Color.red);
		}

		// Token: 0x04000759 RID: 1881
		private static readonly BaseGameMob.GroupModificationArgs CurrentGroupModificationArgs = new BaseGameMob.GroupModificationArgs();

		// Token: 0x0400075A RID: 1882
		private static readonly VitalEnergyHitPointsController.ConsumeVitalEnergyArgs AbilitiesEnergyConsumingArgs = new VitalEnergyHitPointsController.ConsumeVitalEnergyArgs();

		// Token: 0x0400075B RID: 1883
		private static readonly VitalEnergyHitPointsController.RestoreVitalEnergyArgs AbilitiesEnergyRestoringArgs = new VitalEnergyHitPointsController.RestoreVitalEnergyArgs();

		// Token: 0x0400075C RID: 1884
		public const float CrowdInteractionMargin = 0.1f;

		// Token: 0x04000770 RID: 1904
		[HideInInspector]
		public int minDeferredUpdateRate = 2;

		// Token: 0x04000771 RID: 1905
		[HideInInspector]
		public int maxDeferredUpdateRate = 4;

		// Token: 0x04000772 RID: 1906
		public string mobName;

		// Token: 0x04000773 RID: 1907
		public bool isEnvironmentMob;

		// Token: 0x04000774 RID: 1908
		[SerializeField]
		[Tooltip("Радиус, используемый для размещения моба в отряде и для расчетов, связанных с атакой и прочими абилити. Если < 0, то будет вычислен автоматически.")]
		protected float _radius = -1f;

		// Token: 0x04000775 RID: 1909
		[SerializeField]
		[FormerlySerializedAs("_collider")]
		protected Collider2D _hitCollider;

		// Token: 0x04000776 RID: 1910
		[SerializeField]
		[FormerlySerializedAs("MobSprite")]
		protected SpriteRenderer _renderer;

		// Token: 0x04000777 RID: 1911
		public bool isCrowdObstacle = true;

		// Token: 0x04000778 RID: 1912
		[SerializeField]
		[FormerlySerializedAs("_isMonolithicCrowdObstacle")]
		private bool _isStaticCrowdObstacle;

		// Token: 0x04000779 RID: 1913
		[SerializeField]
		private int crowdPassPriority;

		// Token: 0x0400077A RID: 1914
		public LayerMask ignorableCrowdObstaclesLayers;

		// Token: 0x0400077B RID: 1915
		public GameMobImpulseDampingParams impulseDamping = GameMobImpulseDampingParams.Default;

		// Token: 0x0400077C RID: 1916
		public Bounds selectionBounds;

		// Token: 0x0400077D RID: 1917
		[FormerlySerializedAs("defaultGroupID")]
		[Tooltip("К какой фракции будет принадлежать моб, если у него нет группы.")]
		public GameMobFactions defaultFaction = GameMobFactions.PLAYER;

		// Token: 0x0400077E RID: 1918
		public bool ignoreAbilitiesCost;

		// Token: 0x0400077F RID: 1919
		[Tooltip("Если опция активна, то моб может быть принесен в жертву даже если он является призванным.")]
		public bool forceSacrifice = true;

		// Token: 0x04000780 RID: 1920
		public bool isMinorAttackTarget;

		// Token: 0x04000781 RID: 1921
		public LayerMask enemyMobLayers;

		// Token: 0x04000782 RID: 1922
		public int maxAttackersCount = 6;

		// Token: 0x04000783 RID: 1923
		public bool isVisibleToEnemies = true;

		// Token: 0x04000784 RID: 1924
		public TaggedPivot[] taggedPivots;

		// Token: 0x04000785 RID: 1925
		[SerializeField]
		[Tooltip("Точки стрельбы для проджектайловых абилити.")]
		protected ProjectileAbilityBase.ProjectileLaunchPoint[] _shootingPoints;

		// Token: 0x04000786 RID: 1926
		public AssetBasedBuffsBlocker[] ignorableBuffs;

		// Token: 0x04000787 RID: 1927
		public BaseAbility auraEffectAbility;

		// Token: 0x04000788 RID: 1928
		public BaseGameMob.ResourcesGeneratorData resourcesGeneratorData;

		// Token: 0x04000789 RID: 1929
		public bool overrideResourcesGeneratorDataByFactory = true;

		// Token: 0x0400078A RID: 1930
		public float abilityResourcesGatheringDurationOverride;

		// Token: 0x0400078B RID: 1931
		public bool forceSetDeadMobAsAbilityResource;

		// Token: 0x0400078C RID: 1932
		protected Transform cachedTransform;

		// Token: 0x0400078D RID: 1933
		protected IDamageable _hitPointsController;

		// Token: 0x0400078E RID: 1934
		protected IAbilitiesEnergySource abilitiesEnergySource;

		// Token: 0x0400078F RID: 1935
		protected IPickableObjectCollector _pickableObjectsController;

		// Token: 0x04000790 RID: 1936
		protected Animator animator;

		// Token: 0x04000791 RID: 1937
		protected CommonAnimationController animationController;

		// Token: 0x04000792 RID: 1938
		protected NavMeshAgent navMeshAgent;

		// Token: 0x04000793 RID: 1939
		protected bool hasNavmeshAgent;

		// Token: 0x04000794 RID: 1940
		protected GameMobsGroupControllerBase group;

		// Token: 0x04000795 RID: 1941
		protected ILocationChunk currentLocationChunk;

		// Token: 0x04000796 RID: 1942
		protected ILocationChunk lastLocationChunk;

		// Token: 0x04000797 RID: 1943
		protected bool isInitialized;

		// Token: 0x04000798 RID: 1944
		private readonly HashSet<BaseGameMob> currentThreateners = new HashSet<BaseGameMob>();

		// Token: 0x04000799 RID: 1945
		private readonly HashSet<BaseGameMob> currentAttackers = new HashSet<BaseGameMob>();

		// Token: 0x0400079A RID: 1946
		private Vector2 cachedPosition;

		// Token: 0x0400079B RID: 1947
		private string cachedName;

		// Token: 0x0400079C RID: 1948
		private SelectableObjectsManager2D selectionManager;

		// Token: 0x0400079D RID: 1949
		private Rigidbody2D rigidbody;

		// Token: 0x0400079E RID: 1950
		private LocationChunkMobsGridController.GridAgent locationChunkGridAgent;

		// Token: 0x0400079F RID: 1951
		private GameAbilitiesController cachedAbilitiesController;

		// Token: 0x040007A0 RID: 1952
		private GameMobCreationType creationType;

		// Token: 0x040007A1 RID: 1953
		private int layerMask;

		// Token: 0x040007A2 RID: 1954
		private int lastLayer;

		// Token: 0x040007A3 RID: 1955
		private int currentLayerOverride;

		// Token: 0x040007A4 RID: 1956
		private LocationObjectType locationObjectType;

		// Token: 0x040007A5 RID: 1957
		private bool isChunkTransitionInProgress;

		// Token: 0x040007A6 RID: 1958
		private int initialLocalDrawingOrder;

		// Token: 0x040007A7 RID: 1959
		private ObstacleAvoidanceType initialObstacleAvoidanceType;

		// Token: 0x040007A8 RID: 1960
		private SelectableObjectsManager2D.Selectable selectable;

		// Token: 0x040007A9 RID: 1961
		private TaggedPivotGroup taggedPivotsGroup = new TaggedPivotGroup();

		// Token: 0x040007AA RID: 1962
		private BaseBuffsController buffsController;

		// Token: 0x040007AB RID: 1963
		private AbilityResourcesGenerator resourcesGenerator;

		// Token: 0x040007AC RID: 1964
		private AbilityEffectZone auraEffectController;

		// Token: 0x040007AD RID: 1965
		private float deferredUpdateDelay;

		// Token: 0x040007AE RID: 1966
		private float nextDeferredUpdateTime;

		// Token: 0x040007AF RID: 1967
		private Vector2 hitColliderCenterOffset;

		// Token: 0x040007B0 RID: 1968
		private bool isKinematic;

		// Token: 0x040007B1 RID: 1969
		private bool isPlayerGroupMob;

		// Token: 0x040007B2 RID: 1970
		private object revivingContext;

		// Token: 0x040007B3 RID: 1971
		private GameMobSummoningContext summonerInfo;

		// Token: 0x040007B4 RID: 1972
		private bool isSummonedByPlayer;

		// Token: 0x040007B5 RID: 1973
		private bool killWithSummoner;

		// Token: 0x040007B6 RID: 1974
		private bool isSacrificed;

		// Token: 0x040007B7 RID: 1975
		private UnityEngine.Object sacrificer;

		// Token: 0x040007B8 RID: 1976
		private bool isReadyForUpdate;

		// Token: 0x040007B9 RID: 1977
		private bool isCutsceneInProgress;

		// Token: 0x040007BA RID: 1978
		private bool isKilled;

		// Token: 0x040007BB RID: 1979
		private bool isDestroyed;

		// Token: 0x040007BC RID: 1980
		private bool isRendererVisible;

		// Token: 0x02000481 RID: 1153
		public abstract class ControllerBase<TMob> where TMob : BaseGameMob
		{
			// Token: 0x06002416 RID: 9238 RVA: 0x0006FB58 File Offset: 0x0006DD58
			public ControllerBase(TMob targetMob)
			{
				this.ControllerOwner = targetMob;
				this.CurrentMobThreateners = targetMob.currentThreateners;
				targetMob.Killed += this.OnOwnerKilled;
			}

			// Token: 0x06002417 RID: 9239 RVA: 0x0006FB90 File Offset: 0x0006DD90
			public ControllerBase(TMob targetMob, IGame currentGame) : this(targetMob)
			{
			}

			// Token: 0x06002418 RID: 9240 RVA: 0x0006FB99 File Offset: 0x0006DD99
			protected virtual void OnOwnerKilled(IGameMob owner)
			{
				this.ControllerOwner.Killed -= this.OnOwnerKilled;
			}

			// Token: 0x0400179F RID: 6047
			public readonly TMob ControllerOwner;

			// Token: 0x040017A0 RID: 6048
			protected readonly HashSet<BaseGameMob> CurrentMobThreateners;
		}

		// Token: 0x02000482 RID: 1154
		[Serializable]
		public sealed class ResourcesGeneratorData
		{
			// Token: 0x06002419 RID: 9241 RVA: 0x0006FBB8 File Offset: 0x0006DDB8
			public bool HasValues()
			{
				return BaseGameMob.ResourcesGeneratorData.<HasValues>g__IsValidResourcesInfo|3_0(this.aliveOwnerImpactResources) || BaseGameMob.ResourcesGeneratorData.<HasValues>g__IsValidResourcesInfo|3_0(this.undeadOwnerImpactResources) || BaseGameMob.ResourcesGeneratorData.<HasValues>g__IsValidResourcesInfo|3_0(this.ownerDeathResources);
			}

			// Token: 0x0600241A RID: 9242 RVA: 0x0006FBE4 File Offset: 0x0006DDE4
			public AbilityResourcesGenerator.Parameters ToResourcesGeneratorParams(BaseGameMob currentMob)
			{
				return new AbilityResourcesGenerator.Parameters
				{
					ownerImpactResources = (currentMob.IsUndead() ? this.undeadOwnerImpactResources : this.aliveOwnerImpactResources),
					ownerDeathResources = this.ownerDeathResources
				};
			}

			// Token: 0x0600241C RID: 9244 RVA: 0x0006FC2C File Offset: 0x0006DE2C
			[CompilerGenerated]
			internal static bool <HasValues>g__IsValidResourcesInfo|3_0(GeneratableAbilityResource[] info)
			{
				return info != null && info.Length != 0;
			}

			// Token: 0x040017A1 RID: 6049
			public GeneratableAbilityResource[] aliveOwnerImpactResources;

			// Token: 0x040017A2 RID: 6050
			public GeneratableAbilityResource[] undeadOwnerImpactResources;

			// Token: 0x040017A3 RID: 6051
			public GeneratableAbilityResource[] ownerDeathResources;
		}

		// Token: 0x02000483 RID: 1155
		public sealed class GroupModificationArgs
		{
			// Token: 0x040017A4 RID: 6052
			public GameMobsGroupControllerBase group;

			// Token: 0x040017A5 RID: 6053
			public BaseGameMob affectedGroupMob;

			// Token: 0x040017A6 RID: 6054
			public bool mobWasRemovedFromGroup;
		}
	}
}
