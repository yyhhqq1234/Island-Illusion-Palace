using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Damage;
using Game.Damage.Projectiles;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.Motion;
using Unliving.Mobs.Motion.KinematicMotions;

namespace Unliving.Abilities
{
	// Token: 0x020003B3 RID: 947
	[CreateAssetMenu(fileName = "PullingProjectileAbility", menuName = "Abilities/Pulling Projectile Ability")]
	public sealed class PullingProjectileAbility : ProjectileAbilityBase, GameAbilityExtensions.IExplicitUsingContextAbility, IInterruptableAction
	{
		// Token: 0x1700064E RID: 1614
		// (get) Token: 0x06001F5E RID: 8030 RVA: 0x00062E29 File Offset: 0x00061029
		// (set) Token: 0x06001F5F RID: 8031 RVA: 0x00062E31 File Offset: 0x00061031
		public override int ID { get; set; }

		// Token: 0x1700064F RID: 1615
		// (get) Token: 0x06001F60 RID: 8032 RVA: 0x00062E3A File Offset: 0x0006103A
		// (set) Token: 0x06001F61 RID: 8033 RVA: 0x00062E42 File Offset: 0x00061042
		public override int Type
		{
			get
			{
				return (int)this.abilityType;
			}
			set
			{
				this.abilityType = (AbilityTypes)value;
			}
		}

		// Token: 0x17000650 RID: 1616
		// (get) Token: 0x06001F62 RID: 8034 RVA: 0x00062E4B File Offset: 0x0006104B
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000651 RID: 1617
		// (get) Token: 0x06001F63 RID: 8035 RVA: 0x00062E4E File Offset: 0x0006104E
		public override bool CanBeUsed
		{
			get
			{
				return this.projectilePrototypeAsset != null;
			}
		}

		// Token: 0x17000652 RID: 1618
		// (get) Token: 0x06001F64 RID: 8036 RVA: 0x00062E5C File Offset: 0x0006105C
		public override bool IsContinuous
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000653 RID: 1619
		// (get) Token: 0x06001F65 RID: 8037 RVA: 0x00062E5F File Offset: 0x0006105F
		public override bool InUse
		{
			get
			{
				return this.IsPullingInProgress();
			}
		}

		// Token: 0x17000654 RID: 1620
		// (get) Token: 0x06001F66 RID: 8038 RVA: 0x00062E67 File Offset: 0x00061067
		GameAbilityUsingContext GameAbilityExtensions.IExplicitUsingContextAbility.UsingContext
		{
			get
			{
				return GameAbilityUsingContext.BattleAbility;
			}
		}

		// Token: 0x17000655 RID: 1621
		// (get) Token: 0x06001F67 RID: 8039 RVA: 0x00062E6C File Offset: 0x0006106C
		// (set) Token: 0x06001F68 RID: 8040 RVA: 0x00062E81 File Offset: 0x00061081
		public override BuffsGeneratorBuilderAsset.ReferenceBase[] BuffsGeneratorsBuilders
		{
			get
			{
				return this.buffsGenerators;
			}
			set
			{
				this.buffsGenerators = (BuffsGeneratorBuilderAsset.Reference[])value;
			}
		}

		// Token: 0x17000656 RID: 1622
		// (get) Token: 0x06001F69 RID: 8041 RVA: 0x00062E8F File Offset: 0x0006108F
		// (set) Token: 0x06001F6A RID: 8042 RVA: 0x00062EA0 File Offset: 0x000610A0
		public override DamageGenerator DamageGenerator
		{
			get
			{
				return base.DamageGenerator ?? DamageGenerator.Empty;
			}
			set
			{
				base.DamageGenerator = value;
			}
		}

		// Token: 0x17000657 RID: 1623
		// (get) Token: 0x06001F6B RID: 8043 RVA: 0x00062EA9 File Offset: 0x000610A9
		// (set) Token: 0x06001F6C RID: 8044 RVA: 0x00062EB1 File Offset: 0x000610B1
		public bool TryPullOnProjectileHit
		{
			get
			{
				return this._tryPullOnProjectileHit;
			}
			set
			{
				this._tryPullOnProjectileHit = value;
			}
		}

		// Token: 0x14000119 RID: 281
		// (add) Token: 0x06001F6D RID: 8045 RVA: 0x00062EBC File Offset: 0x000610BC
		// (remove) Token: 0x06001F6E RID: 8046 RVA: 0x00062EF4 File Offset: 0x000610F4
		public event Action<PullingProjectileAbility, GameMobKinematicMotionBase> PullingAttemptTaken;

		// Token: 0x1400011A RID: 282
		// (add) Token: 0x06001F6F RID: 8047 RVA: 0x00062F2C File Offset: 0x0006112C
		// (remove) Token: 0x06001F70 RID: 8048 RVA: 0x00062F64 File Offset: 0x00061164
		public event Action<PullingProjectileAbility, GameMobKinematicMotionBase> PullingMotionStarted;

		// Token: 0x1400011B RID: 283
		// (add) Token: 0x06001F71 RID: 8049 RVA: 0x00062F9C File Offset: 0x0006119C
		// (remove) Token: 0x06001F72 RID: 8050 RVA: 0x00062FD4 File Offset: 0x000611D4
		public event Action<PullingProjectileAbility, Vector2> PullingPointUpdated;

		// Token: 0x06001F73 RID: 8051 RVA: 0x00063009 File Offset: 0x00061209
		private bool CanPullSeveralTargets()
		{
			return this.canPullSeveralTargets && !this.pullOwnerToTarget;
		}

		// Token: 0x06001F74 RID: 8052 RVA: 0x0006301E File Offset: 0x0006121E
		private bool CanBePulled(IGameMob mob)
		{
			return mob != null && !mob.IsKinematic && mob.MotionController != null;
		}

		// Token: 0x06001F75 RID: 8053 RVA: 0x00063036 File Offset: 0x00061236
		private bool IsPullingInProgress()
		{
			return this.currentUsingArgs != null && (this.pullingProjectile != null || this.currentPullingCoroutine != null);
		}

		// Token: 0x06001F76 RID: 8054 RVA: 0x00063055 File Offset: 0x00061255
		private void UpdatePullingPoint(Vector2 newPoint)
		{
			Action<PullingProjectileAbility, Vector2> pullingPointUpdated = this.PullingPointUpdated;
			if (pullingPointUpdated == null)
			{
				return;
			}
			pullingPointUpdated(this, newPoint);
		}

		// Token: 0x06001F77 RID: 8055 RVA: 0x00063069 File Offset: 0x00061269
		private void StopCurrentPullingCoroutine()
		{
			if (this.currentPullingCoroutine == null)
			{
				return;
			}
			this.abilityOwner.StopCoroutine(this.currentPullingCoroutine);
			this.currentPullingCoroutine = null;
		}

		// Token: 0x06001F78 RID: 8056 RVA: 0x0006308C File Offset: 0x0006128C
		private void InterruptAnyMotion()
		{
			if (this.currentPullingCoroutine == null)
			{
				return;
			}
			for (int i = 0; i < this.currentPullingMotions.Count; i++)
			{
				this.currentPullingMotions[i].Interrupt();
			}
			this.currentPullingMotions.Clear();
			this.StopCurrentPullingCoroutine();
		}

		// Token: 0x06001F79 RID: 8057 RVA: 0x000630DC File Offset: 0x000612DC
		private MoveToPointMotion TryStartPullingMotion(GameMobMotionControllerBase motionController, Vector2 finalPosition)
		{
			if (this.CanBePulled((motionController != null) ? motionController.ControllerOwner : null))
			{
				MoveToPointMotion moveToPointMotion = new MoveToPointMotion(motionController, this, finalPosition, this.pullingSpeed);
				if (motionController.StartKinematicMotion(moveToPointMotion, true))
				{
					moveToPointMotion.PauseMotion();
					this.currentPullingMotions.Add(moveToPointMotion);
					return moveToPointMotion;
				}
			}
			return null;
		}

		// Token: 0x06001F7A RID: 8058 RVA: 0x0006312C File Offset: 0x0006132C
		private MoveToPointMotion TryStartPullingMotion(Component mobComponent, Vector2 pullDirection)
		{
			IGameMob gameMob = mobComponent.CastOrGetComponent<IGameMob>();
			IGameMob gameMob2 = this.pullOwnerToTarget ? this.abilityOwner : gameMob;
			if (gameMob != null && this.CanBePulled(gameMob2))
			{
				float num = Vector2.Dot(this.abilityOwner.Position - gameMob.Position, pullDirection) - (this.abilityOwner.Radius + gameMob.Radius + 0.5f);
				if (num > 0f)
				{
					GameMobMotionControllerBase motionController = gameMob2.MotionController;
					Vector2 finalPosition = this.pullOwnerToTarget ? (this.abilityOwner.Position - pullDirection * num) : (gameMob.Position + pullDirection * num);
					return this.TryStartPullingMotion(motionController, finalPosition);
				}
			}
			return null;
		}

		// Token: 0x06001F7B RID: 8059 RVA: 0x000631EC File Offset: 0x000613EC
		private Coroutine TryStartPulling(BaseAbility.UsingArgs usingArgs, Vector2 finalProjectilePosition, Vector2 pullDirection)
		{
			if (this.abilityOwner != null)
			{
				int targetsCount = usingArgs.TargetsCount;
				if (targetsCount != 0)
				{
					IList<Component> targetsList = usingArgs.targetsList;
					for (int i = 0; i < targetsCount; i++)
					{
						if (this.TryStartPullingMotion(targetsList[i], pullDirection) != null && !this.CanPullSeveralTargets())
						{
							break;
						}
					}
				}
				else if (usingArgs.HasTargetObject)
				{
					this.TryStartPullingMotion(usingArgs.targetObject, pullDirection);
				}
				else if (this.pullOwnerToTarget)
				{
					this.TryStartPullingMotion(this.abilityOwner.MotionController, finalProjectilePosition);
				}
				if (this.currentPullingMotions.Count != 0)
				{
					return this.abilityOwner.StartCoroutine(this.PullingRoutine(usingArgs, finalProjectilePosition));
				}
				if (this.returningSpeed > 0f)
				{
					return this.abilityOwner.StartCoroutine(this.ReturningRoutine(finalProjectilePosition));
				}
			}
			return null;
		}

		// Token: 0x06001F7C RID: 8060 RVA: 0x000632B0 File Offset: 0x000614B0
		private IEnumerator PullingRoutine(BaseAbility.UsingArgs usingArgs, Vector2 finalProjectilePosition)
		{
			MoveToPointMotion moveToPointMotion = this.currentPullingMotions[0];
			BaseGameMob pullingMob = moveToPointMotion.MotionController.ControllerOwner;
			Action<PullingProjectileAbility, GameMobKinematicMotionBase> pullingAttemptTaken = this.PullingAttemptTaken;
			if (pullingAttemptTaken != null)
			{
				pullingAttemptTaken(this, moveToPointMotion);
			}
			bool isWaitingForPulling = true;
			float pullingStartTime = (this.pullingStartDelay > 0f) ? (Time.time + this.pullingStartDelay) : -1f;
			for (;;)
			{
				if (pullingMob != this.abilityOwner)
				{
					this.UpdatePullingPoint(pullingMob.HitColliderCenter);
				}
				else
				{
					this.UpdatePullingPoint(finalProjectilePosition);
					if (usingArgs != null)
					{
						usingArgs.ProcessTargets(new Action<Component>(PullingProjectileAbility.<PullingRoutine>g__FreezeAbilityTarget|59_0));
					}
				}
				yield return null;
				for (int i = 0; i < this.currentPullingMotions.Count; i++)
				{
					MoveToPointMotion moveToPointMotion2 = this.currentPullingMotions[i];
					if (Time.time < pullingStartTime)
					{
						moveToPointMotion2.PauseMotion();
					}
					else
					{
						if (isWaitingForPulling)
						{
							Action<PullingProjectileAbility, GameMobKinematicMotionBase> pullingMotionStarted = this.PullingMotionStarted;
							if (pullingMotionStarted != null)
							{
								pullingMotionStarted(this, moveToPointMotion2);
							}
							isWaitingForPulling = false;
						}
						if (moveToPointMotion2.IsCompleted)
						{
							goto Block_8;
						}
					}
				}
			}
			Block_8:
			this.InterruptAnyMotion();
			yield break;
			yield break;
		}

		// Token: 0x06001F7D RID: 8061 RVA: 0x000632CD File Offset: 0x000614CD
		private IEnumerator ReturningRoutine(Vector2 startPosition)
		{
			if (this.returningSpeed > 0f)
			{
				float t = 0f;
				float maxT = (this.abilityOwner.Position - startPosition).magnitude / this.returningSpeed;
				Vector2 lastPosition = startPosition;
				Action<PullingProjectileAbility, GameMobKinematicMotionBase> pullingAttemptTaken = this.PullingAttemptTaken;
				if (pullingAttemptTaken != null)
				{
					pullingAttemptTaken(this, null);
				}
				if (this.pullingStartDelay > 0f)
				{
					while (t < this.pullingStartDelay)
					{
						yield return null;
						this.UpdatePullingPoint(startPosition);
						t += Time.deltaTime;
					}
					t = 0f;
				}
				while (t < maxT)
				{
					Vector2 vector = Vector2.Lerp(startPosition, this.abilityOwner.Position, t / maxT);
					this.UpdatePullingPoint(vector);
					if (this.canHookTargetsOnReturn && this._validObjectLayers.value != 0)
					{
						Vector2 a = vector - lastPosition;
						float magnitude = a.magnitude;
						Vector2 vector2 = a / magnitude;
						Collider2D collider = Physics2D.Raycast(lastPosition, vector2, magnitude, this._validObjectLayers).collider;
						BaseGameMob baseGameMob;
						if (collider != null && collider.TryGetComponent<BaseGameMob>(out baseGameMob) && this.CanBePulled(baseGameMob))
						{
							BaseAbility.UsingArgs usingArgs = new BaseAbility.UsingArgs
							{
								targetObject = baseGameMob,
								targetPosition = vector
							};
							if (this.CanPullSeveralTargets())
							{
								this.SetAbilityTargets(usingArgs);
							}
							Coroutine coroutine = this.TryStartPulling(usingArgs, vector, vector2);
							if (coroutine != null)
							{
								this.currentPullingCoroutine = coroutine;
								base.ApplyAbilityEffects(usingArgs, true);
								yield break;
							}
						}
						lastPosition = vector;
					}
					yield return null;
					t += Time.deltaTime;
				}
				lastPosition = default(Vector2);
			}
			this.currentPullingCoroutine = null;
			yield break;
		}

		// Token: 0x06001F7E RID: 8062 RVA: 0x000632E3 File Offset: 0x000614E3
		protected override ProjectileDataBase GetProjectileData()
		{
			return this.projectilePrototypeAsset;
		}

		// Token: 0x06001F7F RID: 8063 RVA: 0x000632EB File Offset: 0x000614EB
		protected override void SetShotsPerUsing(int newMaxShotsPerUsing, bool force = false)
		{
			this._maxShotsPerUsing = 1;
		}

		// Token: 0x06001F80 RID: 8064 RVA: 0x000632F4 File Offset: 0x000614F4
		protected override BaseAbility.ActivationErrorType GetActivationError(BaseAbility.UsingArgs usingArgs, bool isAutoUsePhase, ref object errorData)
		{
			bool flag;
			if (!this.pullOwnerToTarget)
			{
				BaseGameMob baseGameMob = usingArgs.targetObject as BaseGameMob;
				flag = (baseGameMob != null && !this.CanBePulled(baseGameMob));
			}
			else
			{
				flag = !this.CanBePulled(this.abilityOwner);
			}
			if (flag)
			{
				return BaseAbility.ActivationErrorType.UnallowedTarget;
			}
			return base.GetActivationError(usingArgs, isAutoUsePhase, ref errorData);
		}

		// Token: 0x06001F81 RID: 8065 RVA: 0x00063343 File Offset: 0x00061543
		protected override void CollectProjectileEffectTargets(BaseAbility.UsingArgs hitUsingArgs)
		{
			PullingProjectileAbility.TargetsCollectionArgs.PrepareForTargetsCollection(this, hitUsingArgs);
			this.CollectTargets(PullingProjectileAbility.TargetsCollectionArgs, hitUsingArgs);
		}

		// Token: 0x06001F82 RID: 8066 RVA: 0x00063360 File Offset: 0x00061560
		protected override bool PrepareProjectileEffectUsing(ProjectileHitInfo projectileHitInfo, BaseAbility.UsingArgs projectileEffectUsingArgs, out bool sendBuffs)
		{
			sendBuffs = true;
			BaseProjectile.HitInfo hitInfo = projectileHitInfo as BaseProjectile.HitInfo;
			if (hitInfo != null && (this._tryPullOnProjectileHit || hitInfo.IsFinalHit))
			{
				hitInfo.isEffectiveHit = true;
				projectileEffectUsingArgs.targetObject = hitInfo.hitReceiver;
				projectileEffectUsingArgs.targetPosition = hitInfo.point;
				return true;
			}
			return false;
		}

		// Token: 0x06001F83 RID: 8067 RVA: 0x000633AC File Offset: 0x000615AC
		protected override void OnPrepared(BaseAbility.UsingArgs usingArgs)
		{
			if (base.IsPostMortemAbility)
			{
				ProjectileAbility.TryAimToEnemyTarget(this, usingArgs);
			}
			base.OnPrepared(usingArgs);
		}

		// Token: 0x06001F84 RID: 8068 RVA: 0x000633C4 File Offset: 0x000615C4
		protected override bool SetCustomLaunchParams(BaseAbility.UsingArgs abilityUsingArgs, int launchPointIndex, ProjectileLaunchArgs launchArgs)
		{
			if (base.IsPostMortemAbility)
			{
				ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint;
				Vector3 position;
				Vector3 direction;
				base.GetProjectileLaunchParams(abilityUsingArgs, launchPointIndex, out projectileLaunchPoint, out position, out direction);
				launchArgs.position = position;
				launchArgs.direction = direction;
				return true;
			}
			return base.SetCustomLaunchParams(abilityUsingArgs, launchPointIndex, launchArgs);
		}

		// Token: 0x06001F85 RID: 8069 RVA: 0x00063400 File Offset: 0x00061600
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			if (this.IsPullingInProgress())
			{
				return;
			}
			base.PerformAbility(usingArgs);
		}

		// Token: 0x06001F86 RID: 8070 RVA: 0x00063412 File Offset: 0x00061612
		bool IInterruptableAction.TryInterrupt(bool force)
		{
			return false;
		}

		// Token: 0x06001F87 RID: 8071 RVA: 0x00063415 File Offset: 0x00061615
		protected override void OnOwnerChanged(object lastOwner, object newOwner)
		{
			this.InterruptAnyMotion();
			this.abilityOwner = (IGameMob)newOwner;
			base.OnOwnerChanged(lastOwner, newOwner);
		}

		// Token: 0x06001F88 RID: 8072 RVA: 0x00063431 File Offset: 0x00061631
		protected override void OnProjectileLaunched(BaseAbility.UsingArgs abilityUsingArgs, ProjectileAbilityBase.LaunchEventArgs eventArgs)
		{
			this.pullingProjectile = eventArgs.launchedProjectile;
			base.OnProjectileLaunched(abilityUsingArgs, eventArgs);
		}

		// Token: 0x06001F89 RID: 8073 RVA: 0x00063448 File Offset: 0x00061648
		protected override void OnProjectileUsingPrepared(BaseAbility.UsingArgs hitUsingArgs, ProjectileHitInfo hitInfo)
		{
			BaseProjectile.HitInfo hitInfo2 = (BaseProjectile.HitInfo)hitInfo;
			this.currentPullingCoroutine = this.TryStartPulling(hitUsingArgs, hitInfo2.point, -hitInfo2.GetProjectileLaunchDirection());
		}

		// Token: 0x06001F8A RID: 8074 RVA: 0x0006347F File Offset: 0x0006167F
		protected override void OnProjectileHit(ProjectileHitInfo hitArgs)
		{
			if (this.currentPullingCoroutine == null)
			{
				return;
			}
			base.OnProjectileHit(hitArgs);
			IProjectile projectile = this.pullingProjectile;
			if (projectile != null)
			{
				projectile.Destroy();
			}
			this.pullingProjectile = null;
		}

		// Token: 0x06001F8B RID: 8075 RVA: 0x000634A9 File Offset: 0x000616A9
		protected override void OnInitialize(object context)
		{
			this.UsingDuration = 0f;
			base.ProjectilesSpawner.UpdatePerformed += this.OnProjectilesUpdated;
			base.OnInitialize(context);
		}

		// Token: 0x06001F8C RID: 8076 RVA: 0x000634D4 File Offset: 0x000616D4
		private void OnProjectilesUpdated(IProjectilesSpawner spawner, float dt)
		{
			if (this.pullingProjectile != null)
			{
				this.UpdatePullingPoint(this.pullingProjectile.Position);
			}
			if (this.IsPullingInProgress())
			{
				base.BlockOwnerMovement();
			}
		}

		// Token: 0x06001F8D RID: 8077 RVA: 0x000634FD File Offset: 0x000616FD
		protected override void OnCompleted(BaseAbility.UsingArgs usingArgs)
		{
			this.InterruptAnyMotion();
			this.pullingProjectile = null;
			base.OnCompleted(usingArgs);
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x00063513 File Offset: 0x00061713
		protected override void OnDestroy()
		{
			base.ProjectilesSpawner.UpdatePerformed -= this.OnProjectilesUpdated;
			base.OnDestroy();
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x0006356C File Offset: 0x0006176C
		[CompilerGenerated]
		internal static void <PullingRoutine>g__FreezeAbilityTarget|59_0(Component abilityTarget)
		{
			BaseGameMob baseGameMob = abilityTarget.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob != null)
			{
				baseGameMob.BlockMovement(0f);
			}
		}

		// Token: 0x040013DD RID: 5085
		private static readonly GameLocation.MobsGatheringArgs TargetsCollectionArgs = new GameLocation.MobsGatheringArgs();

		// Token: 0x040013E2 RID: 5090
		public AbilityTypes abilityType;

		// Token: 0x040013E3 RID: 5091
		[SerializeField]
		private BuffsGeneratorBuilderAsset.Reference[] buffsGenerators;

		// Token: 0x040013E4 RID: 5092
		[SerializeField]
		private ProjectileDataBase projectilePrototypeAsset;

		// Token: 0x040013E5 RID: 5093
		[SerializeField]
		private bool _tryPullOnProjectileHit;

		// Token: 0x040013E6 RID: 5094
		public float pullingSpeed = 10f;

		// Token: 0x040013E7 RID: 5095
		public float returningSpeed = 15f;

		// Token: 0x040013E8 RID: 5096
		public float pullingStartDelay;

		// Token: 0x040013E9 RID: 5097
		public bool canHookTargetsOnReturn;

		// Token: 0x040013EA RID: 5098
		public bool canPullSeveralTargets;

		// Token: 0x040013EB RID: 5099
		public bool pullOwnerToTarget;

		// Token: 0x040013EC RID: 5100
		private readonly List<MoveToPointMotion> currentPullingMotions = new List<MoveToPointMotion>(16);

		// Token: 0x040013ED RID: 5101
		private IGameMob abilityOwner;

		// Token: 0x040013EE RID: 5102
		private IProjectile pullingProjectile;

		// Token: 0x040013EF RID: 5103
		private Coroutine currentPullingCoroutine;
	}
}
