using System;
using System.Runtime.CompilerServices;
using Common;
using Common.Animation;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Unliving.Mobs.Motion;

namespace Unliving.Mobs.Animation
{
	// Token: 0x0200021D RID: 541
	public class GameMobAnimationController : CommonAnimationController
	{
		// Token: 0x06001269 RID: 4713 RVA: 0x0003A070 File Offset: 0x00038270
		public static void SetLookDirection(Transform transform, float lookDirection)
		{
			Vector3 localScale = transform.localScale;
			localScale.x = Mathf.Abs(localScale.x) * lookDirection;
			transform.localScale = localScale;
		}

		// Token: 0x0600126A RID: 4714 RVA: 0x0003A09F File Offset: 0x0003829F
		protected static bool IsIdlingSpeed(float speed)
		{
			return ((speed > 0f) ? speed : (-speed)) < 0.1f;
		}

		// Token: 0x0600126B RID: 4715 RVA: 0x0003A0B5 File Offset: 0x000382B5
		protected static bool IsIdlingVelocity(Vector2 velocity)
		{
			return GameMobAnimationController.IsIdlingSpeed(velocity.x) && GameMobAnimationController.IsIdlingSpeed(velocity.y);
		}

		// Token: 0x0600126C RID: 4716 RVA: 0x0003A0D1 File Offset: 0x000382D1
		public static bool IsEventArg(string eventArg, string expectedArg)
		{
			return string.Equals(eventArg, expectedArg, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x0600126D RID: 4717 RVA: 0x0003A0DB File Offset: 0x000382DB
		// (set) Token: 0x0600126E RID: 4718 RVA: 0x0003A0E3 File Offset: 0x000382E3
		public GameMobAnimationController.RootMotionInfo[] RootMotionData
		{
			get
			{
				return this.rootMotionData;
			}
			set
			{
				this.rootMotionData = value;
			}
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x0600126F RID: 4719 RVA: 0x0003A0EC File Offset: 0x000382EC
		// (set) Token: 0x06001270 RID: 4720 RVA: 0x0003A0F4 File Offset: 0x000382F4
		public GameMobAnimationController.AnimationEventSpawnableObject[] AnimationEventSpawnableObjects
		{
			get
			{
				return this.animationEventSpawnableObjects;
			}
			set
			{
				this.animationEventSpawnableObjects = value;
			}
		}

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06001271 RID: 4721 RVA: 0x0003A0FD File Offset: 0x000382FD
		public Vector2 SmoothedVelocity
		{
			get
			{
				return this.smoothedVelocity;
			}
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06001272 RID: 4722 RVA: 0x0003A105 File Offset: 0x00038305
		public float? CurrentRootMotionSpeed
		{
			get
			{
				return this.currentRootMotionSpeed;
			}
		}

		// Token: 0x06001273 RID: 4723 RVA: 0x0003A110 File Offset: 0x00038310
		private void InitializeAnimator()
		{
			if (this.hasAppearanceAnimation)
			{
				this.SetIdleAnimationIndex(-1);
				this._targetAnimator.SetTrigger(GameMobAnimationController.AppearanceParamID);
			}
			if (this.hasDamageAnimation)
			{
				this._targetAnimator.SetBool(GameMobAnimationController.DamageAnimationFlagParamID, true);
			}
			if (!this.hasFearAnimations)
			{
				this._targetAnimator.SetBool(GameMobAnimationController.FearParamID, false);
			}
			this._targetAnimator.SetFloat(GameMobAnimationController.OffsetParamID, UnityEngine.Random.value);
		}

		// Token: 0x06001274 RID: 4724 RVA: 0x0003A184 File Offset: 0x00038384
		private void ControlMovementAnimationSpeed(Vector2 currentVelocity)
		{
			if (this.referenceMovementSpeed <= 0f || this.IsRootMotionActive(false))
			{
				return;
			}
			float num = 1f;
			if (this.isMoving)
			{
				float magnitude = currentVelocity.magnitude;
				num = ((!GameMobAnimationController.IsIdlingSpeed(magnitude)) ? (magnitude / this.referenceMovementSpeed) : 1f);
			}
			if (num != this.animationSpeedMultiplier)
			{
				this._targetAnimator.SetFloat(GameMobAnimationController.MovementSpeedParamID, num);
				this.animationSpeedMultiplier = num;
			}
		}

		// Token: 0x06001275 RID: 4725 RVA: 0x0003A1F7 File Offset: 0x000383F7
		private void PlayDeathAnimation()
		{
			this._targetAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			this.deathStateAnimatorDisabler.DisableAnimator(this, new int?(GameMobAnimationController.DeathParamID), new Action<Animator>(this.<PlayDeathAnimation>g__SetDeathAnimationIndex|68_0));
		}

		// Token: 0x06001276 RID: 4726 RVA: 0x0003A228 File Offset: 0x00038428
		private bool IsIdleStateAvailable(GameMobAIController mobAIController)
		{
			return !this.targetMob.IsPlayerMob && (this.mobGroup == null || this.mobGroup.GroupDestination == null) && ((mobAIController != null) ? mobAIController.CurrentAbilityTarget : null) == null;
		}

		// Token: 0x06001277 RID: 4727 RVA: 0x0003A270 File Offset: 0x00038470
		private void ResetKinematicState()
		{
			if (!this.isRunningKinematicState)
			{
				return;
			}
			this.targetMob.IsKinematic = false;
			this.isRunningKinematicState = false;
		}

		// Token: 0x06001278 RID: 4728 RVA: 0x0003A290 File Offset: 0x00038490
		private void UpdateRootMotion(ref AnimatorStateInfo currentStateInfo)
		{
			this.currentRootMotionSpeed = null;
			if (this.rootMotionData == null || this.rootMotionData.Length == 0)
			{
				return;
			}
			int shortNameHash = currentStateInfo.shortNameHash;
			for (int i = 0; i < this.rootMotionData.Length; i++)
			{
				this.currentRootMotionSpeed = new float?(0f);
				ref GameMobAnimationController.RootMotionInfo ptr = ref this.rootMotionData[i];
				if (ptr.data != null && ptr.AnimationStateID == shortNameHash)
				{
					this.currentRootMotionSpeed = new float?(ptr.data.Evaluate(currentStateInfo.normalizedTime % 1f).magnitude);
				}
			}
		}

		// Token: 0x06001279 RID: 4729 RVA: 0x0003A333 File Offset: 0x00038533
		private bool IsRootMotionActive(bool checkMovementFreeze = true)
		{
			return this.currentRootMotionSpeed != null && (!checkMovementFreeze || this.motionController == null || !this.motionController.IsMovementFreezed);
		}

		// Token: 0x0600127A RID: 4730 RVA: 0x0003A35F File Offset: 0x0003855F
		protected void SetMovementXParam(float value)
		{
			this._targetAnimator.SetFloat(GameMobAnimationController.MovementXParamID, value);
		}

		// Token: 0x0600127B RID: 4731 RVA: 0x0003A374 File Offset: 0x00038574
		protected virtual void ActivateAnimator()
		{
			if (this.targetMob.IsRevived && !string.IsNullOrEmpty(this.reviveStateTriggerName))
			{
				this.SetIdleAnimationIndex(-1);
				this._targetAnimator.SetTrigger(this.reviveStateTriggerName);
			}
			this._targetAnimator.enabled = true;
			this._targetAnimator.Update(0f);
			if (this._targetAnimator.GetCurrentAnimatorStateInfo(0).tagHash == GameMobAnimationController.KinematicStateTagID)
			{
				this.targetMob.IsKinematic = true;
			}
			this.deathStateAnimatorDisabler = new AnimatorDisabler(this._targetAnimator, GameMobAnimationController.DeathStateTagID);
		}

		// Token: 0x0600127C RID: 4732 RVA: 0x0003A40C File Offset: 0x0003860C
		protected float GetCurrentLookDirectionRaw()
		{
			return Mathf.Sign(this.mobTransform.localScale.x);
		}

		// Token: 0x0600127D RID: 4733 RVA: 0x0003A424 File Offset: 0x00038624
		protected virtual Vector2 GetCurrentVelocity()
		{
			float deltaTime = Time.deltaTime;
			if (deltaTime <= 0f)
			{
				return default(Vector2);
			}
			return (this.mobTransform.position - this.lastPosition) / deltaTime;
		}

		// Token: 0x0600127E RID: 4734 RVA: 0x0003A46C File Offset: 0x0003866C
		protected virtual Vector2 UpdateMovementVisuals(Vector2 smoothedVelocity, Vector2 actualVelocity, float lookDirection, float velocityDamping = 15f)
		{
			lookDirection = Mathf.Sign(lookDirection);
			if (this.GetCurrentLookDirectionRaw() != lookDirection)
			{
				GameMobAnimationController.SetLookDirection(this.mobTransform, lookDirection);
			}
			if (this.isMoving)
			{
				if (this.IsRootMotionActive(true))
				{
					this.SetMovementXParam(Mathf.Sign(this.currentRootMotionSpeed.Value));
				}
				else
				{
					this.SetMovementXParam((Mathf.Abs(smoothedVelocity.x) > 0.1f) ? smoothedVelocity.x : smoothedVelocity.y);
				}
			}
			else
			{
				this.SetMovementXParam(0f);
			}
			return Vector2.Lerp(smoothedVelocity, actualVelocity, velocityDamping * Time.deltaTime);
		}

		// Token: 0x0600127F RID: 4735 RVA: 0x0003A501 File Offset: 0x00038701
		public void SetIdleAnimationIndex(int index)
		{
			Animator targetAnimator = base.TargetAnimator;
			if (targetAnimator == null)
			{
				return;
			}
			targetAnimator.SetInteger(GameMobAnimationController.IdleParamID, index);
		}

		// Token: 0x06001280 RID: 4736 RVA: 0x0003A519 File Offset: 0x00038719
		public void SetSacrificeAnimationIndex(int index)
		{
			this.sacrificeAnimationIndexOverride = new int?(-Mathf.Abs(index));
		}

		// Token: 0x06001281 RID: 4737 RVA: 0x0003A52D File Offset: 0x0003872D
		public void SetAnimationTrigger(int id)
		{
			Animator targetAnimator = base.TargetAnimator;
			if (targetAnimator == null)
			{
				return;
			}
			targetAnimator.SetTrigger(id);
		}

		// Token: 0x06001282 RID: 4738 RVA: 0x0003A540 File Offset: 0x00038740
		public void SetInitialLookDirection()
		{
			if (this.startDirection == TransformDirection.Random || this.targetMob.IsRevived)
			{
				this.lookDirection = Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
				return;
			}
			if (this.startDirection == TransformDirection.CurrentScaleDirection)
			{
				this.lookDirection = this.GetCurrentLookDirectionRaw();
				return;
			}
			this.lookDirection = (float)this.startDirection;
		}

		// Token: 0x06001283 RID: 4739 RVA: 0x0003A5A0 File Offset: 0x000387A0
		protected override void Reset()
		{
			this.animationStateInfoList = new CommonAnimationController.AnimationStateInfo[]
			{
				new CommonAnimationController.AnimationStateInfo
				{
					stateTag = "Attack",
					stateSpeedParameterName = "AttackSpeed",
					activationTriggerName = "Attack"
				},
				new CommonAnimationController.AnimationStateInfo
				{
					stateTag = "SecondaryAttack",
					stateSpeedParameterName = "AttackSpeed",
					activationTriggerName = "SecondaryAttack"
				},
				new CommonAnimationController.AnimationStateInfo
				{
					stateTag = "Exchange",
					stateSpeedParameterName = "DeathSpeed"
				},
				new CommonAnimationController.AnimationStateInfo
				{
					stateTag = "Death",
					stateSpeedParameterName = "DeathSpeed",
					activationTriggerName = "Death"
				}
			};
			base.Reset();
		}

		// Token: 0x06001284 RID: 4740 RVA: 0x0003A683 File Offset: 0x00038883
		private void OnMobKinematicMotionStarted(GameMobKinematicMotionBase motion)
		{
			if (this.kinematicMovementTriggerID != null)
			{
				this._targetAnimator.SetBool(this.kinematicMovementTriggerID.Value, true);
			}
		}

		// Token: 0x06001285 RID: 4741 RVA: 0x0003A6A9 File Offset: 0x000388A9
		private void OnMobKinematicMotionCompleted(GameMobKinematicMotionBase motion)
		{
			if (this.kinematicMovementTriggerID != null)
			{
				this._targetAnimator.SetBool(this.kinematicMovementTriggerID.Value, false);
			}
		}

		// Token: 0x06001286 RID: 4742 RVA: 0x0003A6CF File Offset: 0x000388CF
		private void OnMobKilled(IGameMob killedMob)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.ResetKinematicState();
			this.PlayDeathAnimation();
		}

		// Token: 0x06001287 RID: 4743 RVA: 0x0003A6EC File Offset: 0x000388EC
		protected virtual void OnMobAnimationEventFired(string eventArgument)
		{
			int num = this.animationEventSpawnableObjects.Length;
			if (num != 0)
			{
				Vector2 position = (this.targetMob.Renderer != null) ? this.targetMob.Renderer.transform.position : this.mobTransform.position;
				Component component = this.targetMob.ForceGetCurrentLocationChunk() as Component;
				Transform parent = (component != null) ? component.transform : null;
				for (int i = 0; i < num; i++)
				{
					this.animationEventSpawnableObjects[i].TrySpawnObject(eventArgument, position, parent, this.lookDirection);
				}
			}
		}

		// Token: 0x06001288 RID: 4744 RVA: 0x0003A784 File Offset: 0x00038984
		private void OnMobAbilityActivated(IAbility ability, object args)
		{
			if (this.updateLookDirection)
			{
				BaseAbility.UsingArgs usingArgs = args as BaseAbility.UsingArgs;
				if (usingArgs != null && usingArgs.HasTargetObject)
				{
					this.lookDirection = usingArgs.targetObject.transform.position.x - this.mobTransform.position.x;
				}
			}
		}

		// Token: 0x06001289 RID: 4745 RVA: 0x0003A7D8 File Offset: 0x000389D8
		protected virtual void Awake()
		{
			if (base.TargetAnimator != null)
			{
				this._targetAnimator.TryGetComponent<BaseGameMob>(out this.targetMob);
				this._targetAnimator.enabled = false;
				if (this._targetAnimator.runtimeAnimatorController == null)
				{
					Debug.LogWarning(base.name + " has Animator without Controller.");
				}
				else
				{
					this.InitializeAnimator();
				}
				if (!string.IsNullOrEmpty(this.kinematicMovementTriggerName))
				{
					this.kinematicMovementTriggerID = new int?(Animator.StringToHash(this.kinematicMovementTriggerName));
				}
				return;
			}
			base.enabled = false;
		}

		// Token: 0x0600128A RID: 4746 RVA: 0x0003A870 File Offset: 0x00038A70
		protected virtual void Start()
		{
			if (this.targetMob != null)
			{
				this.mobTransform = this.targetMob.Transform;
				this.mobRigidbody = this.targetMob.Rigidbody;
				this.mobNavMeshAgent = this.targetMob.NavMeshAgent;
				this.mobGroup = (this.targetMob.Group as GameMobGroupController);
				this.targetMob.AnimationEventFired += this.OnMobAnimationEventFired;
				this.targetMob.Killed += this.OnMobKilled;
				if (this.targetMob.MotionController != null)
				{
					this.motionController = this.targetMob.MotionController;
					this.motionController.KinematicMotionStarted += this.OnMobKinematicMotionStarted;
					this.motionController.KinematicMotionCompleted += this.OnMobKinematicMotionCompleted;
				}
				if (this.targetMob.AbilitiesController != null)
				{
					this.targetMob.AbilitiesController.AbilityActivated += this.OnMobAbilityActivated;
				}
				this.lastPosition = this.mobTransform.position;
			}
			if (this._targetAnimator != null)
			{
				this.ActivateAnimator();
			}
			this.SetInitialLookDirection();
		}

		// Token: 0x0600128B RID: 4747 RVA: 0x0003A9AC File Offset: 0x00038BAC
		protected virtual void LateUpdate()
		{
			if (this.targetMob == null || !this.targetMob.IsAlive())
			{
				this.smoothedVelocity = default(Vector2);
				this.currentRootMotionSpeed = null;
				return;
			}
			float deltaTime = Time.deltaTime;
			GameMobAIController aicontroller = this.targetMob.AIController;
			AnimatorStateInfo currentAnimatorStateInfo = this._targetAnimator.GetCurrentAnimatorStateInfo(0);
			if (this.isCutsceneMovementActive || currentAnimatorStateInfo.tagHash == GameMobAnimationController.KinematicStateTagID)
			{
				this.isRunningKinematicState = true;
				this.targetMob.IsKinematic = true;
				this.isMoving = this.isCutsceneMovementActive;
			}
			else
			{
				this.ResetKinematicState();
				if (currentAnimatorStateInfo.tagHash == GameMobAnimationController.MovementFreezingStateTagID)
				{
					this.targetMob.BlockMovement(0f);
				}
				if (this.motionController != null)
				{
					if (currentAnimatorStateInfo.tagHash == GameMobAnimationController.StartIdleTagID && this.IsIdleStateAvailable(aicontroller))
					{
						GameMobMotionController gameMobMotionController = this.motionController as GameMobMotionController;
						if (gameMobMotionController != null)
						{
							gameMobMotionController.BlockDestination();
						}
					}
					Vector2 desiredVelocity = this.motionController.DesiredVelocity;
					this.isMoving = (!this.motionController.IsMovementFreezed && this.targetMob.IsActiveNavMeshAgent() && (this.IsRootMotionActive(false) || !GameMobAnimationController.IsIdlingVelocity(desiredVelocity)));
				}
				else
				{
					this.isMoving = (this.targetMob.IsActiveNavMeshAgent() && !GameMobAnimationController.IsIdlingVelocity(this.mobNavMeshAgent.desiredVelocity));
				}
			}
			Vector2 vector = this.mobTransform.position;
			float x = vector.x;
			Vector2 currentVelocity = this.GetCurrentVelocity();
			this.lastPosition = vector;
			bool flag = aicontroller != null && aicontroller.IsScared;
			if (this.isMoving != this.lastIsMovingState)
			{
				this._targetAnimator.SetBool(GameMobAnimationController.IsMovingParamID, this.isMoving);
				this.lastIsMovingState = this.isMoving;
			}
			if (this.hasFearAnimations && flag != this.lastFearState)
			{
				this._targetAnimator.SetBool(GameMobAnimationController.FearParamID, flag);
				this.lastFearState = flag;
			}
			float num;
			if (this.updateLookDirection)
			{
				num = this.lookDirection;
				BaseAbility baseAbility = (aicontroller != null) ? aicontroller.CurrentAbility : null;
				GameMobMotionControllerBase gameMobMotionControllerBase = this.motionController;
				GameMobKinematicMotionBase gameMobKinematicMotionBase = (gameMobMotionControllerBase != null) ? gameMobMotionControllerBase.CurrentKinematicMotion : null;
				if (baseAbility != null && (baseAbility.IsActivatedOrInPrep() || this.motionController == null || this.motionController.IsMovementFreezed))
				{
					num = aicontroller.AbilityTargetPosition.x - x;
				}
				else if (this.targetMob.IsActiveNavMeshAgent() && this.mobNavMeshAgent.hasPath)
				{
					GameMobMotionController gameMobMotionController2 = this.motionController as GameMobMotionController;
					if (gameMobMotionController2 != null)
					{
						num = gameMobMotionController2.GetWaypointPosition().x - x;
					}
					else
					{
						num = this.mobNavMeshAgent.steeringTarget.x - x;
					}
				}
				else if (flag || (gameMobKinematicMotionBase != null && gameMobKinematicMotionBase.IsMotionStarter(this.targetMob)))
				{
					num = currentVelocity.x;
				}
			}
			else
			{
				num = this.GetCurrentLookDirectionRaw();
			}
			this.ControlMovementAnimationSpeed(currentVelocity);
			this.UpdateRootMotion(ref currentAnimatorStateInfo);
			if (Mathf.Abs(num) > 0.1f)
			{
				this.lookDirection = Mathf.Lerp(this.lookDirection, num, 10f * deltaTime);
			}
			this.smoothedVelocity = this.UpdateMovementVisuals(this.smoothedVelocity, currentVelocity, this.lookDirection, 15f);
		}

		// Token: 0x0600128C RID: 4748 RVA: 0x0003ACEC File Offset: 0x00038EEC
		protected virtual void OnDisable()
		{
			this.ResetKinematicState();
			if (this.deathStateAnimatorDisabler.InProgress && !base.gameObject.activeInHierarchy)
			{
				this._targetAnimator.enabled = false;
			}
		}

		// Token: 0x0600128D RID: 4749 RVA: 0x0003AD1C File Offset: 0x00038F1C
		private void OnDestroy()
		{
			if (this.motionController != null)
			{
				this.motionController.KinematicMotionStarted -= this.OnMobKinematicMotionStarted;
				this.motionController.KinematicMotionCompleted -= this.OnMobKinematicMotionCompleted;
			}
			if (!this.targetMob.IsNull())
			{
				if (this.targetMob.AbilitiesController != null)
				{
					this.targetMob.AbilitiesController.AbilityActivated -= this.OnMobAbilityActivated;
				}
				this.targetMob.AnimationEventFired -= this.OnMobAnimationEventFired;
				this.targetMob.Killed -= this.OnMobKilled;
			}
		}

		// Token: 0x06001290 RID: 4752 RVA: 0x0003AEBC File Offset: 0x000390BC
		[CompilerGenerated]
		private void <PlayDeathAnimation>g__SetDeathAnimationIndex|68_0(Animator animator)
		{
			float? num = null;
			if (this.targetMob.IsSacrificed)
			{
				if (this.sacrificeAnimationIndexOverride != null)
				{
					num = new float?((float)this.sacrificeAnimationIndexOverride.Value);
				}
				else if (this.sacrificeAnimationsCount > 0)
				{
					num = new float?((float)UnityEngine.Random.Range(-this.sacrificeAnimationsCount, 0));
				}
			}
			animator.SetFloat(GameMobAnimationController.DeathAnimationIDParamID, num ?? ((float)UnityEngine.Random.Range(0, this.deathAnimationsCount)));
		}

		// Token: 0x04000A82 RID: 2690
		public const string AttackEventArg = "attack";

		// Token: 0x04000A83 RID: 2691
		public const string SecondaryAttackEventArg = "SecondaryAttack";

		// Token: 0x04000A84 RID: 2692
		public const string DeathEventArg = "death";

		// Token: 0x04000A85 RID: 2693
		public const string FootstepEventArg = "footstep";

		// Token: 0x04000A86 RID: 2694
		private const float MovementThreshold = 0.1f;

		// Token: 0x04000A87 RID: 2695
		protected static readonly int MovementXParamID = Animator.StringToHash("MoveX");

		// Token: 0x04000A88 RID: 2696
		protected static readonly int MovementSpeedParamID = Animator.StringToHash("MovementSpeed");

		// Token: 0x04000A89 RID: 2697
		protected static readonly int IsMovingParamID = Animator.StringToHash("Moving");

		// Token: 0x04000A8A RID: 2698
		protected static readonly int OffsetParamID = Animator.StringToHash("AnimOffset");

		// Token: 0x04000A8B RID: 2699
		protected static readonly int IdleParamID = Animator.StringToHash("StartIdleIndex");

		// Token: 0x04000A8C RID: 2700
		protected static readonly int AppearanceParamID = Animator.StringToHash("Appearance");

		// Token: 0x04000A8D RID: 2701
		protected static readonly int StartIdleTagID = Animator.StringToHash("StartIdleTag");

		// Token: 0x04000A8E RID: 2702
		protected static readonly int DamageAnimationFlagParamID = Animator.StringToHash("HasDamageAnimation");

		// Token: 0x04000A8F RID: 2703
		protected static readonly int MovementFreezingStateTagID = Animator.StringToHash("FreezeMovement");

		// Token: 0x04000A90 RID: 2704
		protected static readonly int KinematicStateTagID = Animator.StringToHash("KinematicState");

		// Token: 0x04000A91 RID: 2705
		protected static readonly int FearParamID = Animator.StringToHash("Scared");

		// Token: 0x04000A92 RID: 2706
		protected static readonly int DeathParamID = Animator.StringToHash("Death");

		// Token: 0x04000A93 RID: 2707
		protected static readonly int DeathAnimationIDParamID = Animator.StringToHash("DeathID");

		// Token: 0x04000A94 RID: 2708
		protected static readonly int DeathStateTagID = Animator.StringToHash("DeathState");

		// Token: 0x04000A95 RID: 2709
		public bool updateLookDirection = true;

		// Token: 0x04000A96 RID: 2710
		public float referenceMovementSpeed;

		// Token: 0x04000A97 RID: 2711
		public string reviveStateTriggerName;

		// Token: 0x04000A98 RID: 2712
		public string kinematicMovementTriggerName;

		// Token: 0x04000A99 RID: 2713
		public bool hasAppearanceAnimation;

		// Token: 0x04000A9A RID: 2714
		public bool hasDamageAnimation;

		// Token: 0x04000A9B RID: 2715
		public bool hasFearAnimations;

		// Token: 0x04000A9C RID: 2716
		[FormerlySerializedAs("exchangeAnimationsCount")]
		public int sacrificeAnimationsCount;

		// Token: 0x04000A9D RID: 2717
		public int deathAnimationsCount = 1;

		// Token: 0x04000A9E RID: 2718
		[SerializeField]
		private GameMobAnimationController.RootMotionInfo[] rootMotionData;

		// Token: 0x04000A9F RID: 2719
		[SerializeField]
		[FormerlySerializedAs("_animationEventSpawnableObjects")]
		private GameMobAnimationController.AnimationEventSpawnableObject[] animationEventSpawnableObjects;

		// Token: 0x04000AA0 RID: 2720
		public TransformDirection startDirection;

		// Token: 0x04000AA1 RID: 2721
		[HideInInspector]
		public bool isCutsceneMovementActive;

		// Token: 0x04000AA2 RID: 2722
		protected BaseGameMob targetMob;

		// Token: 0x04000AA3 RID: 2723
		protected Transform mobTransform;

		// Token: 0x04000AA4 RID: 2724
		protected Rigidbody2D mobRigidbody;

		// Token: 0x04000AA5 RID: 2725
		protected NavMeshAgent mobNavMeshAgent;

		// Token: 0x04000AA6 RID: 2726
		protected GameMobMotionControllerBase motionController;

		// Token: 0x04000AA7 RID: 2727
		protected GameMobGroupController mobGroup;

		// Token: 0x04000AA8 RID: 2728
		protected Vector2 lastPosition;

		// Token: 0x04000AA9 RID: 2729
		private bool isMoving;

		// Token: 0x04000AAA RID: 2730
		private bool lastIsMovingState;

		// Token: 0x04000AAB RID: 2731
		private bool isRunningKinematicState;

		// Token: 0x04000AAC RID: 2732
		private int? kinematicMovementTriggerID;

		// Token: 0x04000AAD RID: 2733
		private float lookDirection;

		// Token: 0x04000AAE RID: 2734
		private Vector2 smoothedVelocity;

		// Token: 0x04000AAF RID: 2735
		private float animationSpeedMultiplier;

		// Token: 0x04000AB0 RID: 2736
		private float? currentRootMotionSpeed;

		// Token: 0x04000AB1 RID: 2737
		private bool lastFearState;

		// Token: 0x04000AB2 RID: 2738
		private int? sacrificeAnimationIndexOverride;

		// Token: 0x04000AB3 RID: 2739
		private AnimatorDisabler deathStateAnimatorDisabler;

		// Token: 0x020004BD RID: 1213
		[Serializable]
		public struct AnimationEventSpawnableObject
		{
			// Token: 0x0600251C RID: 9500 RVA: 0x0007367C File Offset: 0x0007187C
			public bool TrySpawnObject(string firedEventArgument, Vector2 position, Transform parent, float currentLookDirection)
			{
				if (this.prefab != null && firedEventArgument.Equals(this.eventArgument, StringComparison.OrdinalIgnoreCase))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab);
					gameObject.transform.position = position;
					if (this.syncLookDirection && currentLookDirection != 0f)
					{
						GameMobAnimationController.SetLookDirection(gameObject.transform, currentLookDirection);
					}
					gameObject.transform.parent = parent;
					gameObject.transform.localScale = new Vector3
					{
						x = Mathf.Sign(gameObject.transform.localScale.x),
						y = gameObject.transform.localScale.y,
						z = gameObject.transform.localScale.z
					};
					return true;
				}
				return false;
			}

			// Token: 0x04001997 RID: 6551
			public string eventArgument;

			// Token: 0x04001998 RID: 6552
			public GameObject prefab;

			// Token: 0x04001999 RID: 6553
			public bool syncLookDirection;
		}

		// Token: 0x020004BE RID: 1214
		[Serializable]
		public struct RootMotionInfo
		{
			// Token: 0x17000788 RID: 1928
			// (get) Token: 0x0600251D RID: 9501 RVA: 0x00073754 File Offset: 0x00071954
			// (set) Token: 0x0600251E RID: 9502 RVA: 0x0007375C File Offset: 0x0007195C
			public string AnimationStateName
			{
				get
				{
					return this.animationStateName;
				}
				set
				{
					this.animationStateName = value;
					this.animationStateID = null;
				}
			}

			// Token: 0x17000789 RID: 1929
			// (get) Token: 0x0600251F RID: 9503 RVA: 0x00073771 File Offset: 0x00071971
			public int AnimationStateID
			{
				get
				{
					if (this.animationStateID == null)
					{
						this.animationStateID = new int?(Animator.StringToHash(this.animationStateName));
					}
					return this.animationStateID.Value;
				}
			}

			// Token: 0x0400199A RID: 6554
			public RootMotionData data;

			// Token: 0x0400199B RID: 6555
			[SerializeField]
			private string animationStateName;

			// Token: 0x0400199C RID: 6556
			public float animationSpeedOverride;

			// Token: 0x0400199D RID: 6557
			private int? animationStateID;
		}
	}
}
