using System;
using System.Collections;
using Common.Math.Gameplay;
using Game.Damage;
using Game.GameLoop;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Mobs.Animation;
using Unliving.Mobs.Motion.KinematicMotions;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000212 RID: 530
	public abstract class GameMobMotionControllerBase : BaseGameMob.ControllerBase<BaseGameMob>
	{
		// Token: 0x060011F7 RID: 4599 RVA: 0x00038719 File Offset: 0x00036919
		public static float GetImpulseDragCoeff()
		{
			return MotionUtils.GetDragCoeff(22f);
		}

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x060011F8 RID: 4600 RVA: 0x00038725 File Offset: 0x00036925
		// (set) Token: 0x060011F9 RID: 4601 RVA: 0x0003872D File Offset: 0x0003692D
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
			set
			{
				this.isActive = value;
			}
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x060011FA RID: 4602 RVA: 0x00038736 File Offset: 0x00036936
		// (set) Token: 0x060011FB RID: 4603 RVA: 0x0003873E File Offset: 0x0003693E
		public GameMobImpulseDampingParams ImpulseDamping
		{
			get
			{
				return this.impulseDamping;
			}
			set
			{
				this.impulseDamping = value;
			}
		}

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x060011FC RID: 4604 RVA: 0x00038747 File Offset: 0x00036947
		// (set) Token: 0x060011FD RID: 4605 RVA: 0x0003874F File Offset: 0x0003694F
		public IGameMobMovementPointLimiter CurrentMovementPointLimiter { get; set; }

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x060011FE RID: 4606 RVA: 0x00038758 File Offset: 0x00036958
		// (set) Token: 0x060011FF RID: 4607 RVA: 0x00038760 File Offset: 0x00036960
		public IGameMobsHangingPlatform CurrentHangingPlatform
		{
			get
			{
				return this.currentHangingPlatform;
			}
			set
			{
				if (this.currentHangingPlatform == value)
				{
					return;
				}
				if (this.currentHangingPlatform != null)
				{
					this.currentHangingPlatform.OnMobRemoved(this.ControllerOwner);
					this.currentHangingPlatform.Destroyed -= this.OnHangingPlatformDestroyed;
				}
				if (value != null)
				{
					value.OnMobAdded(this.ControllerOwner);
					value.Destroyed += this.OnHangingPlatformDestroyed;
				}
				this.currentHangingPlatform = value;
				Action<GameMobMotionControllerBase, IGameMobsHangingPlatform> hangingPlatformChanged = this.HangingPlatformChanged;
				if (hangingPlatformChanged == null)
				{
					return;
				}
				hangingPlatformChanged(this, value);
			}
		}

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x06001200 RID: 4608 RVA: 0x000387E1 File Offset: 0x000369E1
		public bool IsMovementBlocked
		{
			get
			{
				return this.isMovementBlocked;
			}
		}

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x06001201 RID: 4609 RVA: 0x000387E9 File Offset: 0x000369E9
		public BaseGameMob CurrentBlockingMob
		{
			get
			{
				return this.currentBlockingMob;
			}
		}

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x06001202 RID: 4610 RVA: 0x000387F1 File Offset: 0x000369F1
		public Vector2 CurrentVelocity
		{
			get
			{
				return this.currentVelocity;
			}
		}

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x06001203 RID: 4611 RVA: 0x000387F9 File Offset: 0x000369F9
		public Vector3 CurrentImpulse
		{
			get
			{
				return this.currentImpulse;
			}
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x06001204 RID: 4612 RVA: 0x00038806 File Offset: 0x00036A06
		public Vector2 DesiredVelocity
		{
			get
			{
				return this.currentDesiredVelocity;
			}
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x06001205 RID: 4613 RVA: 0x0003880E File Offset: 0x00036A0E
		public float CurrentGroundDistance
		{
			get
			{
				return this.kinematicMotionHeight;
			}
		}

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x06001206 RID: 4614 RVA: 0x00038816 File Offset: 0x00036A16
		public bool IsMovementFreezed
		{
			get
			{
				return this.isMovementBlocked || this.isMovementFreezed;
			}
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06001207 RID: 4615 RVA: 0x00038828 File Offset: 0x00036A28
		public bool IsFullyStatic
		{
			get
			{
				return this.isFullyStatic;
			}
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06001208 RID: 4616 RVA: 0x00038830 File Offset: 0x00036A30
		public GameMobKinematicMotionBase CurrentKinematicMotion
		{
			get
			{
				return this.currentKinematicMotion;
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06001209 RID: 4617 RVA: 0x00038838 File Offset: 0x00036A38
		public bool HasActiveMovementLimitingArea
		{
			get
			{
				return this.CurrentMovementPointLimiter != null && this.CurrentMovementPointLimiter.IsActive;
			}
		}

		// Token: 0x140000BF RID: 191
		// (add) Token: 0x0600120A RID: 4618 RVA: 0x00038850 File Offset: 0x00036A50
		// (remove) Token: 0x0600120B RID: 4619 RVA: 0x00038888 File Offset: 0x00036A88
		public event Action<GameMobMotionControllerBase, IGameMobsHangingPlatform> HangingPlatformChanged;

		// Token: 0x140000C0 RID: 192
		// (add) Token: 0x0600120C RID: 4620 RVA: 0x000388C0 File Offset: 0x00036AC0
		// (remove) Token: 0x0600120D RID: 4621 RVA: 0x000388F8 File Offset: 0x00036AF8
		public event Action<GameMobKinematicMotionBase> KinematicMotionStarted;

		// Token: 0x140000C1 RID: 193
		// (add) Token: 0x0600120E RID: 4622 RVA: 0x00038930 File Offset: 0x00036B30
		// (remove) Token: 0x0600120F RID: 4623 RVA: 0x00038968 File Offset: 0x00036B68
		public event Action<GameMobKinematicMotionBase> KinematicMotionCompleted;

		// Token: 0x06001210 RID: 4624 RVA: 0x0003899D File Offset: 0x00036B9D
		private void UpdateLastMobPosition()
		{
			this.lastPosition = this.TransformComponent.position;
		}

		// Token: 0x06001211 RID: 4625 RVA: 0x000389B0 File Offset: 0x00036BB0
		private void UpdateMovementFreezingState()
		{
			if (this.IsMovementFreezed)
			{
				this.RigidbodyComponent.velocity = default(Vector2);
				if (this.ControllerOwner.IsActiveNavMeshAgent())
				{
					this.NavMeshAgentComponent.velocity = default(Vector3);
					this.NavMeshAgentComponent.isStopped = true;
				}
			}
			else if (this.ControllerOwner.IsActiveNavMeshAgent() && this.NavMeshAgentComponent.isStopped)
			{
				this.NavMeshAgentComponent.isStopped = false;
			}
			this.isMovementFreezed = (Time.time < this.freezeResetTime);
			this.isFullyStatic &= this.isMovementFreezed;
		}

		// Token: 0x06001212 RID: 4626 RVA: 0x00038A54 File Offset: 0x00036C54
		private void ResetImpulse()
		{
			this.currentImpulse = default(Vector2);
		}

		// Token: 0x06001213 RID: 4627 RVA: 0x00038A62 File Offset: 0x00036C62
		private bool CanStartKinematicMotion(bool isForcedMotion)
		{
			return this.isActive && (isForcedMotion || this.currentKinematicMotionCoroutine == null);
		}

		// Token: 0x06001214 RID: 4628 RVA: 0x00038A7C File Offset: 0x00036C7C
		private bool StartKinematicMotionInternal(GameMobKinematicMotionBase motion)
		{
			if (motion != null)
			{
				this.StopKinematicMotion();
				if (((IGameMobKinematicMotion)motion).Start(this))
				{
					this.currentKinematicMotion = motion;
					this.currentKinematicMotionCoroutine = this.ControllerOwner.StartCoroutine(this.KinematicMotionRoutine());
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001215 RID: 4629 RVA: 0x00038AB4 File Offset: 0x00036CB4
		private void ApplyKinematicMotionVelocity(GameMobKinematicMotionBase motion, float t, float dt, out float verticalDisplacement)
		{
			this.ResetImpulse();
			this.ResetVelocity();
			this.ResetBlockingMob();
			this.isMovementFreezed = false;
			this.isFullyStatic = false;
			this.freezeResetTime = -1f;
			verticalDisplacement = 0f;
			Vector3 vector;
			motion.Update(t, out vector);
			if (vector != default(Vector3))
			{
				verticalDisplacement = vector.z * dt;
				vector.z = 0f;
				this.Move(vector);
			}
			if (!this.ControllerOwner.enabled)
			{
				this.ApplyVelocity(dt);
				this.ControllerOwner.UpdateCachedPosition();
			}
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x00038B50 File Offset: 0x00036D50
		private void FinalizeKinematicMotion(bool isCompletedMotion)
		{
			this.ControllerOwner.IsKinematic = false;
			if (this.isKinematicMotionDamageResistActive)
			{
				((IResistableDamageable)this.ControllerOwner.HitPointsController).SetResistantToAnyHitPointsModification(false);
				this.isKinematicMotionDamageResistActive = false;
			}
			this.currentKinematicMotionCoroutine = null;
			if (isCompletedMotion)
			{
				((IGameMobKinematicMotion)this.currentKinematicMotion).OnCompleted(this);
			}
			Action<GameMobKinematicMotionBase> kinematicMotionCompleted = this.KinematicMotionCompleted;
			if (kinematicMotionCompleted != null)
			{
				kinematicMotionCompleted(this.currentKinematicMotion);
			}
			this.kinematicMotionProgress = 0f;
			this.kinematicMotionHeight = 0f;
			this.currentKinematicMotion = null;
			this.ResetVelocity();
			if (!this.ControllerOwner.enabled)
			{
				this.ApplyVelocity(default(Vector2), 0f);
			}
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x00038BFF File Offset: 0x00036DFF
		private IEnumerator KinematicMotionRoutine()
		{
			float step = 1f / this.currentKinematicMotion.duration;
			float t = 0f;
			IResistableDamageable hpController = this.ControllerOwner.HitPointsController as IResistableDamageable;
			bool tryModifyDamageResistance = hpController != null;
			if (this.ControllerOwner.IsActiveNavMeshAgent())
			{
				this.NavMeshAgentComponent.velocity = default(Vector3);
			}
			this.RigidbodyComponent.velocity = default(Vector2);
			this.kinematicMotionProgress = 0f;
			Action<GameMobKinematicMotionBase> kinematicMotionStarted = this.KinematicMotionStarted;
			if (kinematicMotionStarted != null)
			{
				kinematicMotionStarted(this.currentKinematicMotion);
			}
			while (t < 1f && !this.currentKinematicMotion.IsInterrupted)
			{
				this.ControllerOwner.IsKinematic = true;
				float deltaTime = Time.deltaTime;
				this.kinematicMotionProgress = t;
				if (tryModifyDamageResistance && this.currentKinematicMotion.BlockHitPointsModification(this.kinematicMotionProgress))
				{
					hpController.SetResistantToAnyHitPointsModification(true);
					tryModifyDamageResistance = false;
					this.isKinematicMotionDamageResistActive = true;
				}
				if (!this.currentKinematicMotion.IsPaused)
				{
					float num;
					this.ApplyKinematicMotionVelocity(this.currentKinematicMotion, this.kinematicMotionProgress, deltaTime, out num);
					this.kinematicMotionHeight += num;
					t += step * deltaTime;
				}
				else
				{
					((IGameMobKinematicMotion)this.currentKinematicMotion).TryResetPauseState();
				}
				yield return null;
			}
			float num2;
			this.ApplyKinematicMotionVelocity(this.currentKinematicMotion, 1f, 1f - this.kinematicMotionProgress, out num2);
			this.FinalizeKinematicMotion(true);
			yield break;
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x00038C10 File Offset: 0x00036E10
		private Vector2 CalculateFinalDesiredVelocity()
		{
			Vector2 result = this.desiredVelocity;
			if (this.isActive)
			{
				if (this.isFullyStatic)
				{
					this.ResetImpulse();
				}
				else
				{
					result.x += this.currentImpulse.x;
					result.y += this.currentImpulse.y;
				}
				if (!this.ControllerOwner.IsKilled && !this.IsMovementFreezed && (this.movementDirection.x != 0f || this.movementDirection.y != 0f))
				{
					float actualMobSpeed = this.GetActualMobSpeed();
					this.movementDirection.Normalize();
					result.x += this.movementDirection.x * actualMobSpeed;
					result.y += this.movementDirection.y * actualMobSpeed;
				}
			}
			else
			{
				this.ResetImpulse();
			}
			return result;
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x00038CF0 File Offset: 0x00036EF0
		private void ApplyVelocity(float dt)
		{
			this.currentDesiredVelocity = this.CalculateFinalDesiredVelocity();
			this.ApplyVelocity(this.currentDesiredVelocity, dt);
			this.currentVelocity = (this.TransformComponent.position - this.lastPosition) / Mathf.Max(dt, 1E-07f);
			this.UpdateLastMobPosition();
		}

		// Token: 0x0600121A RID: 4634 RVA: 0x00038D4D File Offset: 0x00036F4D
		protected bool IsMovementFreezeActive()
		{
			return this.isMovementFreezed;
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x00038D58 File Offset: 0x00036F58
		protected virtual void ApplyVelocity(Vector2 desiredVelocity, float dt)
		{
			if (this.ControllerOwner.IsActiveNavMeshAgent())
			{
				this.NavMeshAgentComponent.velocity = desiredVelocity;
				return;
			}
			Vector3 b = desiredVelocity;
			b.x *= dt;
			b.y *= dt;
			this.TransformComponent.position += b;
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x00038DBA File Offset: 0x00036FBA
		protected void SetBlockingMob(BaseGameMob blockingMob, bool isMovementBlocked)
		{
			this.currentBlockingMob = blockingMob;
			this.isMovementBlocked = (isMovementBlocked && blockingMob != null);
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x00038DD6 File Offset: 0x00036FD6
		protected void ResetBlockingMob()
		{
			this.SetBlockingMob(null, false);
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x00038DE0 File Offset: 0x00036FE0
		public GameMobMotionControllerBase(BaseGameMob mob) : base(mob)
		{
			this.TransformComponent = mob.transform;
			this.NavMeshAgentComponent = mob.NavMeshAgent;
			this.RigidbodyComponent = mob.Rigidbody;
			this.gameLoopAccessor = mob.CurrentGame.Services.Get<IGameLoopAccessProvider>();
			this.gameLoopAccessor.PostLateUpdatePerformed += this.ResetVelocity;
			if (this.NavMeshAgentComponent != null)
			{
				this.stoppingDistanceSquared = Mathf.Max(this.NavMeshAgentComponent.stoppingDistance, mob.Radius + 0.1f);
				this.stoppingDistanceSquared *= this.stoppingDistanceSquared;
				this.NavMeshAgentComponent.autoTraverseOffMeshLink = false;
			}
			if (this.RigidbodyComponent != null)
			{
				this.RigidbodyComponent.useFullKinematicContacts = false;
			}
			this.animationController = (mob.AnimationController as GameMobAnimationController);
			if (this.impulseDamping == null)
			{
				this.impulseDamping = GameMobImpulseDampingParams.Default;
			}
			this.isActive = true;
			this.UpdateLastMobPosition();
			mob.Destroyed += this.OnMobTotallyDestroyed;
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x00038EF2 File Offset: 0x000370F2
		public bool HasSamePlatform(BaseGameMob otherMob)
		{
			IGameMobsHangingPlatform gameMobsHangingPlatform = this.currentHangingPlatform;
			GameMobMotionControllerBase motionController = otherMob.MotionController;
			return gameMobsHangingPlatform == ((motionController != null) ? motionController.CurrentHangingPlatform : null);
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x00038F10 File Offset: 0x00037110
		public float GetActualMobSpeed()
		{
			GameMobAnimationController gameMobAnimationController = this.animationController;
			float? num = (gameMobAnimationController != null) ? gameMobAnimationController.CurrentRootMotionSpeed : null;
			if (num == null)
			{
				return this.ControllerOwner.Speed;
			}
			return num.GetValueOrDefault();
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x00038F54 File Offset: 0x00037154
		public void Move(Vector2 velocity)
		{
			this.desiredVelocity.x = this.desiredVelocity.x + velocity.x;
			this.desiredVelocity.y = this.desiredVelocity.y + velocity.y;
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x00038F80 File Offset: 0x00037180
		public void MoveInDirection(Vector2 direction)
		{
			this.movementDirection.x = this.movementDirection.x + direction.x;
			this.movementDirection.y = this.movementDirection.y + direction.y;
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x00038FAC File Offset: 0x000371AC
		public void ModifyNavMeshAgentVelocity(float velocityMultiplier)
		{
			if (velocityMultiplier == 0f || !this.NavMeshAgentComponent.enabled)
			{
				return;
			}
			this.Move(velocityMultiplier * this.NavMeshAgentComponent.velocity);
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x00038FE0 File Offset: 0x000371E0
		public void FreezeMovement(float duration = 0f, bool makeFullyStatic = false)
		{
			this.isMovementFreezed = true;
			if (makeFullyStatic)
			{
				this.isFullyStatic = true;
			}
			if (duration < 0.05f)
			{
				duration = 0.05f;
			}
			float num = Time.time + duration;
			if (num > this.freezeResetTime)
			{
				this.freezeResetTime = num;
			}
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x00039028 File Offset: 0x00037228
		public void AddImpulse(Vector2 impulse, bool ignoreDamping = false)
		{
			if (this.currentHangingPlatform != null)
			{
				return;
			}
			if (!ignoreDamping)
			{
				this.impulseDamping.Apply(ref impulse);
			}
			this.currentImpulse.x = this.currentImpulse.x + impulse.x;
			this.currentImpulse.y = this.currentImpulse.y + impulse.y;
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x00039078 File Offset: 0x00037278
		public Vector2 GetImpulsePushDisplacement(Vector2 impulse, bool ignoreDamping = false)
		{
			if (!ignoreDamping)
			{
				this.impulseDamping.Apply(ref impulse);
			}
			return MotionUtils.GetTotalImpulsePath(impulse, 22f);
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x0003909F File Offset: 0x0003729F
		public bool StartKinematicMotion(GameMobKinematicMotionBase motion, bool force = false)
		{
			return this.CanStartKinematicMotion(force) && this.StartKinematicMotionInternal(motion);
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x000390B3 File Offset: 0x000372B3
		public bool TryGetKinematicMotionProgress(out float progresss)
		{
			if (this.currentKinematicMotion != null)
			{
				progresss = this.kinematicMotionProgress;
				return true;
			}
			progresss = 0f;
			return false;
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x000390D0 File Offset: 0x000372D0
		public GameMobKinematicMotionBase MoveToPoint(Vector2 targetPoint, float speed, bool force = false, object motionContext = null)
		{
			MoveToPointMotion moveToPointMotion = new MoveToPointMotion(this, motionContext, targetPoint, speed);
			if (!this.CanStartKinematicMotion(force) || !this.StartKinematicMotionInternal(moveToPointMotion))
			{
				return null;
			}
			return moveToPointMotion;
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x000390FD File Offset: 0x000372FD
		public GameMobKinematicMotionBase MoveToPoint(Vector2 targetPoint, bool force = false, object motionContext = null)
		{
			return this.MoveToPoint(targetPoint, this.ControllerOwner.Speed, force, motionContext);
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x00039114 File Offset: 0x00037314
		public GameMobKinematicMotionBase JumpToPoint(Vector2 targetPoint, float jumpImpulse, float gravityOverride = 0f, bool force = false, object motionContext = null)
		{
			if (jumpImpulse < 0.01f)
			{
				return this.MoveToPoint(targetPoint, force, motionContext);
			}
			if (!this.CanStartKinematicMotion(force))
			{
				return null;
			}
			float gravity = (gravityOverride > 0f) ? gravityOverride : 40f;
			JumpMotion jumpMotion = new JumpMotion(this, motionContext, targetPoint, jumpImpulse, gravity);
			if (!this.StartKinematicMotionInternal(jumpMotion))
			{
				return null;
			}
			return jumpMotion;
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x0003916A File Offset: 0x0003736A
		public bool StopKinematicMotion()
		{
			if (this.currentKinematicMotionCoroutine == null)
			{
				return false;
			}
			this.currentKinematicMotion.Interrupt();
			this.ControllerOwner.StopCoroutine(this.currentKinematicMotionCoroutine);
			this.FinalizeKinematicMotion(false);
			return true;
		}

		// Token: 0x0600122D RID: 4653
		public abstract void OnUpdate();

		// Token: 0x0600122E RID: 4654 RVA: 0x0003919C File Offset: 0x0003739C
		public void OnFixedUpdate()
		{
			if (this.currentImpulse.x != 0f || this.currentImpulse.y != 0f)
			{
				this.currentImpulse *= GameMobMotionControllerBase.GetImpulseDragCoeff();
				if (this.currentImpulse.SqrMagnitude() < 1E-05f)
				{
					this.ResetImpulse();
				}
			}
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x000391FB File Offset: 0x000373FB
		public void OnLateUpdate()
		{
			this.UpdateMovementFreezingState();
			this.ApplyVelocity(Time.deltaTime);
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x00039210 File Offset: 0x00037410
		private void ResetVelocity()
		{
			if (!this.ControllerOwner.enabled)
			{
				this.currentDesiredVelocity = default(Vector2);
				this.currentVelocity = default(Vector2);
			}
			this.currentVelocity = default(Vector2);
			this.desiredVelocity = default(Vector2);
			this.movementDirection = default(Vector2);
		}

		// Token: 0x06001231 RID: 4657 RVA: 0x00039268 File Offset: 0x00037468
		private void OnHangingPlatformDestroyed(object platform)
		{
			this.IsActive = true;
			if (this.currentHangingPlatform.Height > 0f && this.StartKinematicMotion(this.currentHangingPlatform.GetFallMotion(this), true))
			{
				this.kinematicMotionHeight = this.currentHangingPlatform.Height;
			}
			this.CurrentHangingPlatform = null;
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x000392BB File Offset: 0x000374BB
		protected override void OnOwnerKilled(IGameMob owner)
		{
			this.StopKinematicMotion();
			this.CurrentHangingPlatform = null;
			this.isMovementFreezed = false;
			this.ResetBlockingMob();
			base.OnOwnerKilled(owner);
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x000392DF File Offset: 0x000374DF
		protected virtual void OnMobTotallyDestroyed(object destroyedMob)
		{
			this.gameLoopAccessor.PostLateUpdatePerformed -= this.ResetVelocity;
			this.ControllerOwner.Destroyed -= this.OnMobTotallyDestroyed;
		}

		// Token: 0x04000A4B RID: 2635
		public const float ImpulseDrag = 22f;

		// Token: 0x04000A4C RID: 2636
		public const float JumpGravity = 40f;

		// Token: 0x04000A51 RID: 2641
		public readonly NavMeshAgent NavMeshAgentComponent;

		// Token: 0x04000A52 RID: 2642
		protected readonly IGameLoopAccessProvider gameLoopAccessor;

		// Token: 0x04000A53 RID: 2643
		protected readonly Transform TransformComponent;

		// Token: 0x04000A54 RID: 2644
		protected readonly Rigidbody2D RigidbodyComponent;

		// Token: 0x04000A55 RID: 2645
		private bool isMovementBlocked;

		// Token: 0x04000A56 RID: 2646
		private readonly float stoppingDistanceSquared;

		// Token: 0x04000A57 RID: 2647
		private readonly GameMobAnimationController animationController;

		// Token: 0x04000A58 RID: 2648
		private Vector3 lastPosition;

		// Token: 0x04000A59 RID: 2649
		private Vector2 desiredVelocity;

		// Token: 0x04000A5A RID: 2650
		private Vector2 currentDesiredVelocity;

		// Token: 0x04000A5B RID: 2651
		private Vector2 currentVelocity;

		// Token: 0x04000A5C RID: 2652
		private Vector2 movementDirection;

		// Token: 0x04000A5D RID: 2653
		private GameMobImpulseDampingParams impulseDamping;

		// Token: 0x04000A5E RID: 2654
		private Vector2 currentImpulse;

		// Token: 0x04000A5F RID: 2655
		private BaseGameMob currentBlockingMob;

		// Token: 0x04000A60 RID: 2656
		private bool isMovementFreezed;

		// Token: 0x04000A61 RID: 2657
		private bool isFullyStatic;

		// Token: 0x04000A62 RID: 2658
		private float freezeResetTime;

		// Token: 0x04000A63 RID: 2659
		private GameMobKinematicMotionBase currentKinematicMotion;

		// Token: 0x04000A64 RID: 2660
		private Coroutine currentKinematicMotionCoroutine;

		// Token: 0x04000A65 RID: 2661
		private float kinematicMotionHeight;

		// Token: 0x04000A66 RID: 2662
		private float kinematicMotionProgress;

		// Token: 0x04000A67 RID: 2663
		private bool isKinematicMotionDamageResistActive;

		// Token: 0x04000A68 RID: 2664
		private IGameMobsHangingPlatform currentHangingPlatform;

		// Token: 0x04000A69 RID: 2665
		private bool isActive;
	}
}
