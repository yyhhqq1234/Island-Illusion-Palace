using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using Game.Stats;
using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Abilities;
using Unliving.LevelGeneration;
using Unliving.Mobs.AbilityTriggers;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001B4 RID: 436
	public sealed class GameMobAIController : BaseGameMob.ControllerBase<BaseGameMob>
	{
		// Token: 0x06000C54 RID: 3156 RVA: 0x00026911 File Offset: 0x00024B11
		private static bool IsAbilityInUse(BaseAbility ability)
		{
			return ability.IsPrepInProgress() || ability.IsBusy();
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x00026924 File Offset: 0x00024B24
		public static float GetMobRadius(IGameMob mob)
		{
			NavMeshAgent navMeshAgent = mob.NavMeshAgent;
			if (!(navMeshAgent != null))
			{
				return mob.Radius;
			}
			return navMeshAgent.radius;
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x06000C56 RID: 3158 RVA: 0x0002694E File Offset: 0x00024B4E
		// (set) Token: 0x06000C57 RID: 3159 RVA: 0x00026956 File Offset: 0x00024B56
		public GameMobAIControllerParams CurrentParams
		{
			get
			{
				return this.currentParams;
			}
			set
			{
				if (this.currentParams == value)
				{
					return;
				}
				this.currentParams = value;
				this.CancelSpecialStatuses();
				this.ResetCurrentAbility(true);
				this.ResetAggressionToInitialState();
			}
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x06000C58 RID: 3160 RVA: 0x0002697C File Offset: 0x00024B7C
		// (set) Token: 0x06000C59 RID: 3161 RVA: 0x00026984 File Offset: 0x00024B84
		public bool IsAggressive
		{
			get
			{
				return this.isAggressive;
			}
			set
			{
				if (this.isAggressive == value)
				{
					return;
				}
				if (!value)
				{
					this.ResetBattleAbility();
				}
				this.isAggressive = value;
			}
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06000C5A RID: 3162 RVA: 0x000269A0 File Offset: 0x00024BA0
		// (set) Token: 0x06000C5B RID: 3163 RVA: 0x000269A8 File Offset: 0x00024BA8
		public bool IsIdling { get; private set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06000C5C RID: 3164 RVA: 0x000269B1 File Offset: 0x00024BB1
		// (set) Token: 0x06000C5D RID: 3165 RVA: 0x000269B9 File Offset: 0x00024BB9
		public bool IsScared { get; private set; }

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000C5E RID: 3166 RVA: 0x000269C2 File Offset: 0x00024BC2
		// (set) Token: 0x06000C5F RID: 3167 RVA: 0x000269CC File Offset: 0x00024BCC
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
			set
			{
				if (this.isActive == value || (value && this.ControllerOwner.IsKilled))
				{
					return;
				}
				this.CancelSpecialStatuses();
				this.CompleteFearState();
				this.ResetAggressionToInitialState();
				if (!value)
				{
					this.ResetCurrentAbility(false);
				}
				this.IsIdling = false;
				this.ResetSpeedModifier();
				this.isActive = value;
			}
		}

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000C60 RID: 3168 RVA: 0x00026A23 File Offset: 0x00024C23
		public BaseAbility CurrentAbility
		{
			get
			{
				return this.currentAbility;
			}
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000C61 RID: 3169 RVA: 0x00026A2B File Offset: 0x00024C2B
		public AbilityID CurrentAbilityID
		{
			get
			{
				if (!(this.currentAbility != null))
				{
					return AbilityID.None;
				}
				return (AbilityID)this.currentAbility.ID;
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000C62 RID: 3170 RVA: 0x00026A48 File Offset: 0x00024C48
		public IGameMob CurrentAbilityTarget
		{
			get
			{
				return this.currentAbilityTarget;
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000C63 RID: 3171 RVA: 0x00026A50 File Offset: 0x00024C50
		public Vector2 AbilityTargetPosition
		{
			get
			{
				return this.abilityTargetPosition;
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000C64 RID: 3172 RVA: 0x00026A58 File Offset: 0x00024C58
		public IGameMob CurrentAttackTarget
		{
			get
			{
				if (!this.isReadyForBattle)
				{
					return null;
				}
				return this.currentAbilityTarget;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06000C65 RID: 3173 RVA: 0x00026A6A File Offset: 0x00024C6A
		public bool CanUseSelectedAbility
		{
			get
			{
				return this.canUseSelectedAbility;
			}
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000C66 RID: 3174 RVA: 0x00026A72 File Offset: 0x00024C72
		public bool IsReadyForBattle
		{
			get
			{
				return this.isReadyForBattle;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000C67 RID: 3175 RVA: 0x00026A7A File Offset: 0x00024C7A
		public bool IsAbilityUsingDistanceReached
		{
			get
			{
				return this.isAbilityUsingDistanceReached;
			}
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000C68 RID: 3176 RVA: 0x00026A82 File Offset: 0x00024C82
		public bool IsAttackDistanceReached
		{
			get
			{
				return this.isReadyForBattle && this.isAbilityUsingDistanceReached;
			}
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000C69 RID: 3177 RVA: 0x00026A94 File Offset: 0x00024C94
		public bool IsAttacking
		{
			get
			{
				return this.isAttacking;
			}
		}

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000C6A RID: 3178 RVA: 0x00026A9C File Offset: 0x00024C9C
		public bool IsAimStoppingPrepProgressReached
		{
			get
			{
				return this.isAimStoppingPrepProgressReached;
			}
		}

		// Token: 0x14000085 RID: 133
		// (add) Token: 0x06000C6B RID: 3179 RVA: 0x00026AA4 File Offset: 0x00024CA4
		// (remove) Token: 0x06000C6C RID: 3180 RVA: 0x00026ADC File Offset: 0x00024CDC
		public event Action<GameMobAIController, IGameMob, IGameMob> AbilityTargetChanged;

		// Token: 0x14000086 RID: 134
		// (add) Token: 0x06000C6D RID: 3181 RVA: 0x00026B14 File Offset: 0x00024D14
		// (remove) Token: 0x06000C6E RID: 3182 RVA: 0x00026B4C File Offset: 0x00024D4C
		public event Action<GameMobAIController, IGameMob, bool> AggressionStateChanged;

		// Token: 0x14000087 RID: 135
		// (add) Token: 0x06000C6F RID: 3183 RVA: 0x00026B84 File Offset: 0x00024D84
		// (remove) Token: 0x06000C70 RID: 3184 RVA: 0x00026BBC File Offset: 0x00024DBC
		public event Action<GameMobAIController, bool> AttackStateChanged;

		// Token: 0x14000088 RID: 136
		// (add) Token: 0x06000C71 RID: 3185 RVA: 0x00026BF4 File Offset: 0x00024DF4
		// (remove) Token: 0x06000C72 RID: 3186 RVA: 0x00026C2C File Offset: 0x00024E2C
		public event Action<GameMobAIController, bool> FearStateChanged;

		// Token: 0x06000C73 RID: 3187 RVA: 0x00026C61 File Offset: 0x00024E61
		private bool IsOnHangingPlatform()
		{
			return this.motionController != null && this.motionController.CurrentHangingPlatform != null;
		}

		// Token: 0x06000C74 RID: 3188 RVA: 0x00026C7B File Offset: 0x00024E7B
		private void SetDestinationPoint(Vector2? point)
		{
			if (this.motionController != null)
			{
				this.motionController.IndividualDestination = point;
			}
		}

		// Token: 0x06000C75 RID: 3189 RVA: 0x00026C94 File Offset: 0x00024E94
		private void SetSpeedModifier(float newSpeedModifier)
		{
			if (newSpeedModifier <= 0f)
			{
				newSpeedModifier = 1f;
			}
			if (this.currentSpeedModifier == newSpeedModifier)
			{
				return;
			}
			StatsControllerBase<MobStatModifier> statsController = this.ControllerOwner.StatsController;
			if (statsController == null)
			{
				return;
			}
			if (newSpeedModifier != 1f)
			{
				statsController.AddModifier(1, new MobStatModifier(0f, 0f, newSpeedModifier));
			}
			else
			{
				statsController.RemoveModifier(1, new MobStatModifier(0f, 0f, this.currentSpeedModifier));
			}
			this.currentSpeedModifier = newSpeedModifier;
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x00026D0E File Offset: 0x00024F0E
		private void ResetSpeedModifier()
		{
			this.SetSpeedModifier(1f);
		}

		// Token: 0x06000C77 RID: 3191 RVA: 0x00026D1C File Offset: 0x00024F1C
		private IGameMob FindAbilityTarget(BaseAbility ability, Vector2 position, ref GameMobAIController.TargetsCollector targetsCollector, GameMobAIController.AbilityTargetsValidator validator, bool forceSearchExternalTarget)
		{
			this.hasBlockingTarget = false;
			if (!forceSearchExternalTarget)
			{
				Ability ability2 = ability as Ability;
				Ability.Target? target = (ability2 != null) ? new Ability.Target?(ability2.UsingTarget) : null;
				Ability.Target? target2 = target;
				Ability.Target target3 = Ability.Target.AbilityOwner;
				if (!(target2.GetValueOrDefault() == target3 & target2 != null))
				{
					target2 = target;
					target3 = Ability.Target.AbilityOwnerSquad;
					if (!(target2.GetValueOrDefault() == target3 & target2 != null))
					{
						goto IL_6C;
					}
				}
				return this.ControllerOwner;
			}
			IL_6C:
			this.abilityTargetSelector.CurrentTargets = null;
			this.abilityTargetSelector.TargetsCountOverride = -1;
			this.abilityTargetSelector.targetSelectionPoint = new Vector2?(position);
			this.abilityTargetSelector.SelectFirstAvailableTarget = false;
			this.abilityTargetSelector.MaxTargetsCount = -1;
			this.abilityTargetSelector.targetSelectionRadius = targetsCollector.Range;
			this.abilityTargetSelector.skipTargetInRangeCheck = true;
			this.abilityTargetSelector.ResetMaxAllowedTargetEstimation();
			MobAbilityParameters mobAbilityParameters = (ability != null) ? ability.GetMobAbilityParams() : null;
			validator.Prepare(ability, this.abilityTargetSelector, mobAbilityParameters);
			if (validator.IsValidTarget(this.currentExplicitAbilityTarget))
			{
				return this.currentExplicitAbilityTarget;
			}
			GameMobsGroupControllerBase group = this.ControllerOwner.Group;
			IGameMob gameMob = null;
			if (this.ControllerOwner.IsSacrificed || group.IsGroupDestinationReached || !group.HasForcedGroupDestination)
			{
				gameMob = this.currentAbilityTarget;
				if (targetsCollector.HasCollectedTargets)
				{
					targetsCollector.PassTargetsToTargetSelector(this.abilityTargetSelector);
				}
				else if (targetsCollector.CollectTargets(position, this.abilityTargetSelector, ability) && targetsCollector.IsCollectableLayers(validator.allowedTargetsLayers))
				{
					validator.skipTargetsLayerValidation = true;
				}
			}
			if (this.abilityTargetSelector.CurrentTargets != null)
			{
				GameMobTargetSelector.SelectionMethod selectionMethod;
				GameMobTargetSelector.PrioritySelector prioritySelector;
				this.GetTargetSelectionParams(mobAbilityParameters, out selectionMethod, out prioritySelector);
				this.abilityTargetSelector.SetTargetsEstimationParams(selectionMethod, prioritySelector);
				if (validator.isAttackTargetsValidator)
				{
					if (this.hasUnreachableAttackTarget)
					{
						this.abilityTargetSelector.SetTargetsEstimationParams(GameMobTargetSelector.SelectionMethod.Closest, GameMobTargetSelector.PrioritySelector.MaxAttackSpace);
					}
					else if (this.enemyTargetsPriorityEstimatorOverride != null)
					{
						this.abilityTargetSelector.SetCustomTargetsPriorityEstimation(this.enemyTargetsPriorityEstimatorOverride);
					}
				}
				else if (this.abilityTargetSelector.TargetSelectionMethod == GameMobTargetSelector.SelectionMethod.MinCurrentHealth)
				{
					this.abilityTargetSelector.MaxAllowedTargetEstimation = 0.999998f;
				}
				if (this.abilityTargetSelector.IsDefaultTargetSelector)
				{
					this.abilityTargetSelector.SelectFirstAvailableTarget = true;
					this.abilityTargetSelector.MaxTargetsCount = 32;
				}
			}
			IGameMob gameMob2 = this.abilityTargetSelector.FindNewTarget();
			validator.skipTargetsLayerValidation = false;
			IGameMob gameMob3;
			if (validator.HasValidBlockingTarget(gameMob2, out gameMob3))
			{
				gameMob2 = gameMob3;
				this.hasBlockingTarget = true;
			}
			if (gameMob2 == null && !this.IsAbilityTargetResetTimeReached() && validator.IsValidTarget(gameMob))
			{
				gameMob2 = gameMob;
			}
			PlayerBehaviour playerBehaviour;
			if (gameMob2 == null && validator.isAttackTargetsValidator && this.TryGetPlayerAsDefaultAttackTarget(validator, out playerBehaviour))
			{
				gameMob2 = playerBehaviour;
			}
			return gameMob2;
		}

		// Token: 0x06000C78 RID: 3192 RVA: 0x00026FA8 File Offset: 0x000251A8
		private IGameMob FindAbilityTarget(BaseAbility ability, out GameMobAIController.TargetsCollector usedCollector, GameMobAIController.TargetsCollector lastCollector = default(GameMobAIController.TargetsCollector), bool forceSearchAttackTarget = false, bool forceSearchExternalTarget = false)
		{
			if (!forceSearchExternalTarget)
			{
				forceSearchExternalTarget = ability.HasTargetedTriggers();
			}
			if (forceSearchAttackTarget || ability.IsBattleAbility())
			{
				int num = ability.ValidObjectLayers;
				float range = Mathf.Max(ability.Range, this.currentParams.TargetSearchRadius);
				usedCollector = GameMobAIController.TargetsCollector.GetMostReusableCollector(lastCollector, new GameMobAIController.TargetsCollector(this.ControllerOwner, num, range));
				this.abilityTargetsValidator.isAttackTargetsValidator = true;
				this.abilityTargetsValidator.SetLayers(num, this.currentParams);
				return this.FindAbilityTarget(ability, this.GetAttackTargetsSearchPosition(), ref usedCollector, this.abilityTargetsValidator, forceSearchExternalTarget);
			}
			int num2 = ability.ValidObjectLayers;
			float num3 = ability.Range;
			if (num3 <= 0f)
			{
				num3 = ability.GetMaxTriggersActivationRange(true);
			}
			usedCollector = GameMobAIController.TargetsCollector.GetMostReusableCollector(lastCollector, new GameMobAIController.TargetsCollector(this.ControllerOwner, num2, num3));
			this.abilityTargetsValidator.isAttackTargetsValidator = false;
			this.abilityTargetsValidator.SetLayers(num2, this.currentParams);
			return this.FindAbilityTarget(ability, this.ControllerOwner.Position, ref usedCollector, this.abilityTargetsValidator, forceSearchExternalTarget) ?? this.ControllerOwner;
		}

		// Token: 0x06000C79 RID: 3193 RVA: 0x000270C0 File Offset: 0x000252C0
		private bool TryGetPlayerAsDefaultAttackTarget(GameMobAIController.AbilityTargetsValidator targetsValidator, out PlayerBehaviour player)
		{
			IPlayerProvider playerProvider = this.playerProvider;
			player = ((playerProvider != null) ? playerProvider.CurrentPlayer : null);
			return this.currentParams.usePlayerAsDefaultAttackTarget && player != null && this.ControllerOwner.IsRendererVisible && player.InLayerMask(this.currentParams.attackLayers) && (targetsValidator == null || targetsValidator.IsValidTarget(player, false, true, false));
		}

		// Token: 0x06000C7A RID: 3194 RVA: 0x00027130 File Offset: 0x00025330
		private void SetSelectedAbility(BaseAbility newAbility)
		{
			if (this.currentAbility == newAbility)
			{
				return;
			}
			if (this.currentAbility != null)
			{
				this.currentAbility.Complete();
			}
			if (newAbility != null)
			{
				this.isReadyForBattle = newAbility.IsBattleAbility();
				this.currentAbilityParams = newAbility.GetMobAbilityParams();
				if (this.lastUsedAbility != null && this.lastUsedAbility != newAbility)
				{
					this.UpdateNextAbilityActivationTime(this.lastUsedAbility);
				}
			}
			else
			{
				this.SetAbilityTarget(null, this.isReadyForBattle);
				this.SetDestinationPoint(null);
				this.currentAbilityParams = null;
				this.abilityTriggersArgs.Reset();
				this.canUseSelectedAbility = false;
				this.isReadyForBattle = false;
			}
			this.currentAbility = newAbility;
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x000271F4 File Offset: 0x000253F4
		private void SetAbilityTarget(IGameMob newAbilityTarget, bool isAttackTarget = false)
		{
			this.currentExplicitAbilityTarget = null;
			if (newAbilityTarget == this.currentAbilityTarget)
			{
				return;
			}
			this.hasBlockingTarget = false;
			if (this.currentAbilityTarget != null)
			{
				BaseGameMob baseGameMob = this.currentAbilityTarget as BaseGameMob;
				if (baseGameMob != null)
				{
					baseGameMob.RemoveThreatener(this.ControllerOwner);
					baseGameMob.RemoveAttacker(this.ControllerOwner);
				}
				this.currentAbilityTarget.Killed -= this.OnAbilityTargetKilled;
			}
			if (newAbilityTarget != null)
			{
				if (isAttackTarget)
				{
					BaseGameMob baseGameMob2 = newAbilityTarget as BaseGameMob;
					if (baseGameMob2 != null)
					{
						baseGameMob2.AddThreatener(this.ControllerOwner);
					}
				}
				this.FocusOnAbilityTarget();
				this.KeepAbilityTarget();
				newAbilityTarget.Killed += this.OnAbilityTargetKilled;
			}
			IGameMob gameMob = this.currentAbilityTarget;
			this.currentAbilityTarget = newAbilityTarget;
			if (this.currentAbilityTarget == null || !this.currentAbilityTarget.IsLayerInMask(this.currentParams.attackLayers))
			{
				this.ResetAggressionToInitialState();
			}
			Action<GameMobAIController, IGameMob, IGameMob> abilityTargetChanged = this.AbilityTargetChanged;
			if (abilityTargetChanged != null)
			{
				abilityTargetChanged(this, gameMob, newAbilityTarget);
			}
			if (isAttackTarget && gameMob == null && newAbilityTarget != null)
			{
				Action<GameMobAIController, IGameMob, bool> aggressionStateChanged = this.AggressionStateChanged;
				if (aggressionStateChanged == null)
				{
					return;
				}
				aggressionStateChanged(this, newAbilityTarget, true);
				return;
			}
			else
			{
				Action<GameMobAIController, IGameMob, bool> aggressionStateChanged2 = this.AggressionStateChanged;
				if (aggressionStateChanged2 == null)
				{
					return;
				}
				aggressionStateChanged2(this, gameMob, false);
				return;
			}
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x0002731C File Offset: 0x0002551C
		private bool KeepCurrentAbility(BaseAbility.UsingArgs usingArgs)
		{
			if (this.currentAbility != null)
			{
				if (this.currentAbility.IsActivated || GameMobAIController.IsAbilityInUse(this.currentAbility))
				{
					return true;
				}
				if (this.IsWaitingForAbilityTargetFocusCompletion())
				{
					this.abilityTargetsValidator.isAttackTargetsValidator = this.currentAbility.IsBattleAbility();
					this.abilityTargetsValidator.SetLayers(0, this.currentParams);
					this.abilityTargetsValidator.Prepare(this.currentAbility, null, this.currentAbility.GetMobAbilityParams());
					return this.abilityTargetsValidator.IsValidTarget(this.currentAbilityTarget, true, true, true) && this.currentAbility.CanBeActivated(usingArgs);
				}
			}
			return false;
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x000273CC File Offset: 0x000255CC
		private void UpdateNextAbilityActivationTime(BaseAbility lastUsedAbility)
		{
			MobAbilityParameters mobAbilityParameters = (lastUsedAbility != null) ? lastUsedAbility.GetMobAbilityParams() : null;
			if (mobAbilityParameters != null)
			{
				this.nextAbilityActivationTime = Time.time + mobAbilityParameters.nextAbilityUsingDelay;
				return;
			}
			this.nextAbilityActivationTime = -1f;
		}

		// Token: 0x06000C7E RID: 3198 RVA: 0x0002740D File Offset: 0x0002560D
		private bool IsAbilityActivationTimeReached()
		{
			return Time.time > this.nextAbilityActivationTime;
		}

		// Token: 0x06000C7F RID: 3199 RVA: 0x0002741C File Offset: 0x0002561C
		private bool IsWaitingForAbilityTargetFocusCompletion()
		{
			return Time.time < this.abilityTargetFocusEndTime;
		}

		// Token: 0x06000C80 RID: 3200 RVA: 0x0002742B File Offset: 0x0002562B
		private void FocusOnAbilityTarget()
		{
			this.abilityTargetFocusEndTime = Time.time + this.currentParams.minTargetFocusDuration;
		}

		// Token: 0x06000C81 RID: 3201 RVA: 0x00027444 File Offset: 0x00025644
		private void ResetAbilityTargetFocusTime()
		{
			this.abilityTargetFocusEndTime = 0f;
		}

		// Token: 0x06000C82 RID: 3202 RVA: 0x00027451 File Offset: 0x00025651
		private void KeepAbilityTarget()
		{
			this.abilityTargetResetTime = Time.time + this.currentParams.maxTargetFollowingDuration;
		}

		// Token: 0x06000C83 RID: 3203 RVA: 0x0002746A File Offset: 0x0002566A
		private bool IsAbilityTargetResetTimeReached()
		{
			return Time.time > this.abilityTargetResetTime;
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x0002747C File Offset: 0x0002567C
		private bool IsBlockingAbilityTarget(IGameMob abilityTarget)
		{
			BaseGameMob baseGameMob = abilityTarget as BaseGameMob;
			if (baseGameMob != null)
			{
				GameMobMotionController gameMobMotionController = this.motionController;
				return ((gameMobMotionController != null) ? gameMobMotionController.CurrentBlockingMob : null) == baseGameMob;
			}
			return false;
		}

		// Token: 0x06000C85 RID: 3205 RVA: 0x000274B0 File Offset: 0x000256B0
		private void UpdateAbilityTargetInfo(IGameMob abilityTarget, BaseAbility.UsingArgs targetUsingArgs, out float minAbilityUsingDistance)
		{
			minAbilityUsingDistance = this.controllerOwnerRadius;
			if (abilityTarget != null)
			{
				minAbilityUsingDistance += GameMobAIController.GetMobRadius(abilityTarget);
				this.abilityTargetPosition = abilityTarget.Position;
			}
			this.abilityTargetDistance = (this.abilityTargetPosition - this.ControllerOwner.Position).magnitude;
			this.abilityTargetContactDistance = this.abilityTargetDistance - minAbilityUsingDistance;
			if (this.abilityTargetContactDistance < 0f || this.IsBlockingAbilityTarget(abilityTarget))
			{
				this.abilityTargetContactDistance = 0f;
			}
			this.abilityTriggersArgs.aiController = this;
			this.abilityTriggersArgs.target = abilityTarget;
			this.abilityTriggersArgs.targetDistance = this.abilityTargetContactDistance;
			if (targetUsingArgs != null)
			{
				targetUsingArgs.Reset();
				targetUsingArgs.targetObject = (abilityTarget as Component);
				targetUsingArgs.targetPosition = ((abilityTarget != null) ? abilityTarget.HitColliderCenter : this.abilityTargetPosition);
				targetUsingArgs.additionalContext = this.abilityTriggersArgs;
			}
		}

		// Token: 0x06000C86 RID: 3206 RVA: 0x0002759C File Offset: 0x0002579C
		private void UpdateAttackState(bool newAttackState, bool force = false)
		{
			bool flag = this.isAttacking;
			if (newAttackState)
			{
				if (!force && !this.isReadyForBattle)
				{
					return;
				}
				this.isAttacking = true;
				this.attackingStateResetTime = Time.time + 1f;
				if (!flag)
				{
					BaseGameMob baseGameMob = this.currentAbilityTarget as BaseGameMob;
					if (baseGameMob != null)
					{
						baseGameMob.AddAttacker(this.ControllerOwner);
					}
					Action<GameMobAIController, bool> attackStateChanged = this.AttackStateChanged;
					if (attackStateChanged == null)
					{
						return;
					}
					attackStateChanged(this, true);
					return;
				}
			}
			else if (force || (this.isAttacking && Time.time > this.attackingStateResetTime))
			{
				this.isAttacking = false;
				if (flag)
				{
					BaseGameMob baseGameMob2 = this.currentAbilityTarget as BaseGameMob;
					if (baseGameMob2 != null)
					{
						baseGameMob2.RemoveAttacker(this.ControllerOwner);
					}
					Action<GameMobAIController, bool> attackStateChanged2 = this.AttackStateChanged;
					if (attackStateChanged2 == null)
					{
						return;
					}
					attackStateChanged2(this, false);
				}
			}
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x00027659 File Offset: 0x00025859
		private bool TryGetAbilitiesList(out IReadOnlyList<BaseAbility> abilities, out int abilitiesCount)
		{
			if (this.abilitiesController != null)
			{
				abilities = this.abilitiesController.Abilities;
				abilitiesCount = this.abilitiesController.UsableAbilitiesCount;
				return abilitiesCount != 0;
			}
			abilities = null;
			abilitiesCount = 0;
			return false;
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x0002768C File Offset: 0x0002588C
		private bool IsAllowedAbility(BaseAbility ability, bool isBattleAbility)
		{
			return !(isBattleAbility ? (!this.isAggressive) : (!this.currentParams.canUseSupportAbilities)) && this.abilitiesController != null && !this.abilitiesController.IsUnallowedAbility(ability) && (!this.IsOnHangingPlatform() || (!ability.IsJumpMotionAbility() && ability.IsProjectileAbility(true)));
		}

		// Token: 0x06000C89 RID: 3209 RVA: 0x000276F0 File Offset: 0x000258F0
		private bool CanBeSelected(BaseAbility ability, out bool isBattleAbility, bool skipAbilityReloadingCheck = false)
		{
			isBattleAbility = ability.IsBattleAbility();
			if (ability.IsPostMortemAbility)
			{
				return false;
			}
			if (!this.exclusivelyUsingAbilitiesDescription.IsBlank())
			{
				return this.exclusivelyUsingAbilitiesDescription.IsMatch(ability);
			}
			if ((skipAbilityReloadingCheck || !ability.IsReloading()) && this.IsAllowedAbility(ability, isBattleAbility))
			{
				IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
				for (int i = 0; i < extensions.Count; i++)
				{
					IMobAbilityTrigger mobAbilityTrigger = extensions[i] as IMobAbilityTrigger;
					if (mobAbilityTrigger != null && !mobAbilityTrigger.RequiresTarget && !mobAbilityTrigger.IsConditionReached(ability))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000C8A RID: 3210 RVA: 0x00027780 File Offset: 0x00025980
		private bool IsAbilitiesTargetsSearchRequired()
		{
			PlayerBehaviour playerBehaviour;
			if (this.hasUnreachableAttackTarget || this.TryGetPlayerAsDefaultAttackTarget(null, out playerBehaviour))
			{
				return true;
			}
			GameMobMotionController gameMobMotionController = this.motionController;
			BaseGameMob baseGameMob = (gameMobMotionController != null) ? gameMobMotionController.CurrentBlockingMob : null;
			if (baseGameMob != null && baseGameMob.Faction != this.ControllerOwner.Faction)
			{
				return true;
			}
			IReadOnlyList<BaseAbility> readOnlyList;
			int num;
			if (this.TryGetAbilitiesList(out readOnlyList, out num))
			{
				GameMobsGroupControllerBase group = this.ControllerOwner.Group;
				if (group != null)
				{
					if (GameMobAIController.<IsAbilitiesTargetsSearchRequired>g__IsGroupInBattle|127_0(group))
					{
						return true;
					}
					IReadOnlyList<GameMobsGroupControllerBase> coupledGroups = group.CoupledGroups;
					for (int i = 0; i < coupledGroups.Count; i++)
					{
						if (GameMobAIController.<IsAbilitiesTargetsSearchRequired>g__IsGroupInBattle|127_0(coupledGroups[i]))
						{
							return true;
						}
					}
				}
				int num2 = 0;
				float num3 = this.currentParams.TargetSearchRadius;
				for (int j = 0; j < num; j++)
				{
					BaseAbility baseAbility = readOnlyList[j];
					bool flag;
					if (this.CanBeSelected(baseAbility, out flag, false))
					{
						num2 |= baseAbility.ValidObjectLayers;
						if (baseAbility.Range > num3 && baseAbility.IsSupportAbility())
						{
							num3 = baseAbility.Range;
						}
					}
				}
				return num2 != 0 && Physics2D.OverlapCircle(this.ControllerOwner.Position, num3, num2) != null;
			}
			return false;
		}

		// Token: 0x06000C8B RID: 3211 RVA: 0x000278B8 File Offset: 0x00025AB8
		private void SelectAbility(BaseAbility.UsingArgs targetUsingArgs)
		{
			this.canUseSelectedAbility = false;
			IGameMob gameMob = this.currentAbilityTarget;
			IReadOnlyList<BaseAbility> readOnlyList;
			int num;
			if (!this.TryGetAbilitiesList(out readOnlyList, out num) || (this.canUseSelectedAbility = this.KeepCurrentAbility(targetUsingArgs)))
			{
				return;
			}
			if (this.currentExplicitAbilityTarget == null && gameMob == null && !this.IsAbilitiesTargetsSearchRequired())
			{
				return;
			}
			GameMobAIController.TargetsCollector lastCollector = default(GameMobAIController.TargetsCollector);
			BaseAbility baseAbility = null;
			IGameMob gameMob2 = null;
			bool flag = false;
			if (num > 1)
			{
				int num2 = num;
				int num3 = this.abilitySelectionFrame;
				this.abilitySelectionFrame = num3 + 1;
				int index = num2 - (num3 % (num - 1) + 1);
				BaseAbility baseAbility2 = readOnlyList[index];
				bool flag2;
				if (this.CanBeSelected(baseAbility2, out flag2, false))
				{
					flag = flag2;
					GameMobAIController.TargetsCollector targetsCollector;
					gameMob2 = this.FindAbilityTarget(baseAbility2, out targetsCollector, default(GameMobAIController.TargetsCollector), flag2, false);
					float num4;
					this.UpdateAbilityTargetInfo(gameMob2, targetUsingArgs, out num4);
					if (gameMob2 != null && baseAbility2.CanBeActivated(targetUsingArgs))
					{
						baseAbility = baseAbility2;
					}
				}
			}
			if (baseAbility == null)
			{
				flag = false;
				BaseAbility baseAbility3 = readOnlyList[0];
				bool flag3;
				if (this.CanBeSelected(baseAbility3, out flag3, true))
				{
					flag = flag3;
					GameMobAIController.TargetsCollector targetsCollector;
					gameMob2 = this.FindAbilityTarget(baseAbility3, out targetsCollector, lastCollector, flag, false);
					if (gameMob2 != null)
					{
						baseAbility = baseAbility3;
					}
				}
			}
			this.SetSelectedAbility(baseAbility);
			this.SetAbilityTarget(gameMob2, flag);
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x000279DD File Offset: 0x00025BDD
		private bool HasSelfUseAbility()
		{
			return this.currentAbilityTarget == this.ControllerOwner;
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x000279F0 File Offset: 0x00025BF0
		private bool IsAbilityTargetInfoUpdateRequired(BaseAbility ability, MobAbilityParameters abilityParams, out bool isMaxAimingProgressReached)
		{
			isMaxAimingProgressReached = false;
			if (abilityParams != null)
			{
				float aimStoppingPrepThreshold = abilityParams.AimStoppingPrepThreshold;
				if (aimStoppingPrepThreshold > 0f && ability.HasPrepTime())
				{
					isMaxAimingProgressReached = (ability.PrepProgress >= aimStoppingPrepThreshold);
					return !isMaxAimingProgressReached;
				}
				if (abilityParams.useFakeShootPrediction)
				{
					return true;
				}
			}
			return !ability.IsActivated;
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x00027A48 File Offset: 0x00025C48
		private bool TryActivateAbility(BaseAbility ability, MobAbilityParameters @params, IGameMob target, BaseAbility.UsingArgs targetUsingArgs, out bool canBeUsed, bool force = false)
		{
			bool flag;
			if (this.IsAbilityTargetInfoUpdateRequired(ability, @params, out flag))
			{
				float num;
				this.UpdateAbilityTargetInfo(target, targetUsingArgs, out num);
				this.minAbilityTargetDistance = num;
			}
			this.isAimStoppingPrepProgressReached = flag;
			float num2 = ability.Range;
			if ((this.hasBlockingTarget && this.isReadyForBattle) || num2 <= 0f || this.HasSelfUseAbility())
			{
				canBeUsed = true;
			}
			else
			{
				if (@params != null)
				{
					num2 *= @params.AbilityUsingDistanceMultiplier;
				}
				canBeUsed = (this.abilityTargetContactDistance < Mathf.Max(num2 - this.minAbilityTargetDistance, 0.1f));
				if (canBeUsed && targetUsingArgs != null && !ability.IsProjectileAbility(true))
				{
					float range = ability.Range;
					float sqrMagnitude = (targetUsingArgs.targetPosition - ability.OwnerPosition).sqrMagnitude;
					if (sqrMagnitude > range * range)
					{
						targetUsingArgs.usingRangeOverride = Mathf.Sqrt(sqrMagnitude);
					}
				}
			}
			if ((force || this.IsAbilityActivationTimeReached()) && (canBeUsed || ability.IsPrepInProgress()))
			{
				if (targetUsingArgs.affectableTargetsFilter == null && @params != null)
				{
					@params.TryPassAllowedTargetsFilter(ref targetUsingArgs.affectableTargetsFilter);
				}
				ability.Activate(targetUsingArgs);
				return true;
			}
			return false;
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x00027B68 File Offset: 0x00025D68
		private void UseCurrentAbility(BaseAbility.UsingArgs targetUsingArgs)
		{
			if (this.currentAbility == null)
			{
				return;
			}
			if (this.TryActivateAbility(this.currentAbility, this.currentAbilityParams, this.currentAbilityTarget, targetUsingArgs, out this.isAbilityUsingDistanceReached, false))
			{
				this.UpdateAttackState(true, false);
				if (this.currentAbility.WasUsed)
				{
					this.lastUsedAbility = this.currentAbility;
				}
			}
			if (this.isAbilityUsingDistanceReached || this.abilityTargetDistance < this.currentParams.TargetSearchRadius || GameMobAIController.IsAbilityInUse(this.currentAbility))
			{
				this.KeepAbilityTarget();
			}
			if (!this.HasSelfUseAbility())
			{
				this.SetDestinationPoint(new Vector2?(this.isAbilityUsingDistanceReached ? this.ControllerOwner.Position : this.abilityTargetPosition));
			}
			if (this.IsAbilityTargetResetTimeReached())
			{
				this.SetAbilityTarget(null, false);
			}
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x00027C33 File Offset: 0x00025E33
		private void ResetCurrentAbility(bool force)
		{
			if (!force)
			{
				if (this.currentAbility != null)
				{
					this.currentAbility.Complete();
				}
				this.isWaitingForAbilityReset = true;
				return;
			}
			this.SetSelectedAbility(null);
			this.UpdateAttackState(false, true);
			this.isWaitingForAbilityReset = false;
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x00027C6F File Offset: 0x00025E6F
		private void ResetBattleAbility()
		{
			if (this.isReadyForBattle)
			{
				this.ResetCurrentAbility();
			}
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x00027C80 File Offset: 0x00025E80
		private void UpdateSpecialStatuses()
		{
			if (this.specialStatuses == null)
			{
				return;
			}
			bool canUseSpecialStatuses = this.currentParams.canUseSpecialStatuses;
			for (int i = 0; i < this.specialStatuses.Length; i++)
			{
				if (canUseSpecialStatuses)
				{
					this.specialStatuses[i].Update(this.abilityTriggersArgs);
				}
				else
				{
					this.specialStatuses[i].ForceDeactivate();
				}
			}
		}

		// Token: 0x06000C93 RID: 3219 RVA: 0x00027CDC File Offset: 0x00025EDC
		private void CancelSpecialStatuses()
		{
			if (this.specialStatuses == null)
			{
				return;
			}
			for (int i = 0; i < this.specialStatuses.Length; i++)
			{
				this.specialStatuses[i].ForceDeactivate();
			}
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x00027D14 File Offset: 0x00025F14
		private void UpdateFearStateAvoidanceDirection()
		{
			if (!this.IsScared)
			{
				return;
			}
			this.fearStateRunAwayDirection = default(Vector2);
			if (this.currentRunAwayDistance > this.minRunAwayDistance && Mathf.PerlinNoise(Time.time, (float)this.controllerRandomSeed) < 0.5f)
			{
				return;
			}
			Vector2 position = this.ControllerOwner.Position;
			int num = Physics2D.OverlapCircleNonAlloc(position, Mathf.Max(this.currentParams.TargetSearchRadius * 0.5f, 3f), GameMobAIController.TargetsSearchBuffer, this.currentParams.attackLayers);
			for (int i = 0; i < num; i++)
			{
				this.fearStateRunAwayDirection += position - GameMobAIController.TargetsSearchBuffer[i].transform.position;
			}
			if (this.explicitThreatPoint != null)
			{
				this.fearStateRunAwayDirection += position - this.explicitThreatPoint.Value;
				num++;
			}
			if (num != 0)
			{
				this.fearStateRunAwayDirection /= (float)num;
				this.fearStateRunAwayDirection.Normalize();
			}
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x00027E30 File Offset: 0x00026030
		private Vector2 GetLocalAvoidanceDirection(int obstacleLayers, Vector2 lastLocalAvoidanceDirection, Vector2 targetMovementDirection, float damping = 3f)
		{
			Vector2 currentVelocity = this.motionController.CurrentVelocity;
			Vector2 vector = default(Vector2);
			if (currentVelocity.SqrMagnitude() > 1E-05f)
			{
				Vector2 position = this.ControllerOwner.Position;
				float num = this.controllerOwnerRadius * 3f;
				Vector2 a = position + targetMovementDirection * num;
				Vector2 b = new Vector2
				{
					x = targetMovementDirection.y,
					y = -targetMovementDirection.x
				} * this.controllerOwnerRadius * 1.25f;
				GameMobAIController.<GetLocalAvoidanceDirection>g__AccumulateAvoidanceDirection|138_0(Physics2D.Raycast(position, targetMovementDirection, num, obstacleLayers).normal, ref vector);
				GameMobAIController.<GetLocalAvoidanceDirection>g__AccumulateAvoidanceDirection|138_0(Physics2D.Linecast(position, a - b, obstacleLayers).normal, ref vector);
				GameMobAIController.<GetLocalAvoidanceDirection>g__AccumulateAvoidanceDirection|138_0(Physics2D.Linecast(position, a + b, obstacleLayers).normal, ref vector);
				if (damping > 0f)
				{
					float num2 = damping * Time.deltaTime;
					vector.x = lastLocalAvoidanceDirection.x + (vector.x - lastLocalAvoidanceDirection.x) * num2;
					vector.y = lastLocalAvoidanceDirection.y + (vector.y - lastLocalAvoidanceDirection.y) * num2;
				}
			}
			return vector;
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x00027F6A File Offset: 0x0002616A
		private void ResetLocalObstacleAvoidanceDirection()
		{
			this.localObstacleAvoidanceDirection = default(Vector2);
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x00027F78 File Offset: 0x00026178
		private void UpdateFearState()
		{
			if (!this.IsScared)
			{
				return;
			}
			this.IsAggressive = false;
			float time = Time.time;
			if (this.ControllerOwner.IsActiveNavMeshAgent())
			{
				this.ControllerOwner.NavMeshAgent.ResetPath();
			}
			if (this.motionController != null)
			{
				this.motionController.BlockDestination();
				if (this.fearStateRunAwayDirection != default(Vector2))
				{
					Vector2 vector = this.fearStateRunAwayDirection;
					if (this.maxRunAwayTrajectoryVariation > 0f)
					{
						float x = time * this.ControllerOwner.Speed;
						vector = QuaternionExtensions.RotateVector2D(this.maxRunAwayTrajectoryVariation * (Mathf.PerlinNoise(x, (float)this.controllerRandomSeed) * 2f - 1f), vector, false);
					}
					this.localObstacleAvoidanceDirection = this.GetLocalAvoidanceDirection(this.currentParams.fearStateObstaclesLayers, this.localObstacleAvoidanceDirection, vector, 3f);
					vector += this.localObstacleAvoidanceDirection;
					vector.Normalize();
					this.motionController.MoveInDirection(vector);
					this.currentRunAwayDistance += this.motionController.CurrentVelocity.magnitude * Time.deltaTime;
				}
			}
			if (time > this.finalFearStateTime)
			{
				this.CompleteFearState();
			}
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x000280B0 File Offset: 0x000262B0
		private void UpdateSpeedModifier()
		{
			float speedModifier = this.IsScared ? this.currentParams.fearStateSpeedMultiplier : ((this.CurrentAttackTarget != null) ? this.currentParams.attackTargetChasingSpeedMultiplier : 1f);
			this.SetSpeedModifier(speedModifier);
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x000280F4 File Offset: 0x000262F4
		public Vector2 GetAttackTargetsSearchPosition()
		{
			GameMobMotionController gameMobMotionController = this.motionController;
			IGameMobsHangingPlatform gameMobsHangingPlatform = (gameMobMotionController != null) ? gameMobMotionController.CurrentHangingPlatform : null;
			if (gameMobsHangingPlatform != null)
			{
				Vector2 position = this.ControllerOwner.Position;
				float height = gameMobsHangingPlatform.Height;
				position.y -= height;
				return position;
			}
			return this.ControllerOwner.Position;
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x00028144 File Offset: 0x00026344
		public void GetTargetSelectionParams(MobAbilityParameters abilityParamsBlock, out GameMobTargetSelector.SelectionMethod selectionMethod, out GameMobTargetSelector.PrioritySelector prioritySelector)
		{
			selectionMethod = this.currentParams.targetSelectionMethod;
			prioritySelector = this.currentParams.priorityTargetSelector;
			if (abilityParamsBlock != null)
			{
				bool flag = abilityParamsBlock.targetSelectionMethodOverride != GameMobTargetSelector.SelectionMethod.None;
				if (flag || abilityParamsBlock.targetSelectionPriorityOverride != GameMobTargetSelector.PrioritySelector.Default)
				{
					if (flag)
					{
						selectionMethod = abilityParamsBlock.targetSelectionMethodOverride;
					}
					prioritySelector = abilityParamsBlock.targetSelectionPriorityOverride;
				}
			}
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x000281A0 File Offset: 0x000263A0
		public void ResetAggressionToInitialState()
		{
			this.IsAggressive = this.currentParams.isAggressiveByDefault;
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x000281B3 File Offset: 0x000263B3
		public void SetExplicitAbilityTarget(IGameMob explicitTarget, bool force = false)
		{
			this.currentExplicitAbilityTarget = explicitTarget;
			if (force)
			{
				this.ResetCurrentAbility();
			}
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x000281C5 File Offset: 0x000263C5
		public bool TrySetExplicitAbilityTarget(IGameMob explicitTarget)
		{
			if (explicitTarget == null || this.currentAbilityTarget != null || this.ControllerOwner.IsKilled)
			{
				return false;
			}
			this.SetExplicitAbilityTarget(explicitTarget, false);
			return true;
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x000281EA File Offset: 0x000263EA
		public void ForceResetExplicitAbilityTarget()
		{
			this.currentExplicitAbilityTarget = null;
		}

		// Token: 0x06000C9F RID: 3231 RVA: 0x000281F3 File Offset: 0x000263F3
		public void SetEnemyTargetsAdditionalValidator(Func<BaseGameMob, IGameMob, bool> additionalValidator)
		{
			this.enemyTargetsAdditionalValidator = additionalValidator;
		}

		// Token: 0x06000CA0 RID: 3232 RVA: 0x000281FC File Offset: 0x000263FC
		public void SetEnemyTargetsPriorityEstimatorOverride(Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation> priorityEstimationOverride)
		{
			this.enemyTargetsPriorityEstimatorOverride = priorityEstimationOverride;
		}

		// Token: 0x06000CA1 RID: 3233 RVA: 0x00028205 File Offset: 0x00026405
		public void SetExclusivelyUsingAbilitiesDescription(AbilityDescription abilityDescription)
		{
			this.exclusivelyUsingAbilitiesDescription = abilityDescription;
			if (!abilityDescription.IsBlank() && !abilityDescription.IsMatch(this.currentAbility))
			{
				this.ResetCurrentAbility();
			}
		}

		// Token: 0x06000CA2 RID: 3234 RVA: 0x0002822C File Offset: 0x0002642C
		public void ResetExclusivelyUsingAbilitiesDescription()
		{
			if (!this.exclusivelyUsingAbilitiesDescription.IsBlank() && this.exclusivelyUsingAbilitiesDescription.IsMatch(this.currentAbility))
			{
				this.ResetCurrentAbility(true);
			}
			this.exclusivelyUsingAbilitiesDescription = AbilityDescription.BlankDescription;
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x00028260 File Offset: 0x00026460
		public bool IsBusy()
		{
			return this.isAttacking || this.currentAbilityTarget != null || (this.currentAbility != null && GameMobAIController.IsAbilityInUse(this.currentAbility));
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x0002828F File Offset: 0x0002648F
		public bool IsEnemyTarget(IGameMob mob)
		{
			return this.currentParams.IsAttackLayers(mob.LayerMask);
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x000282A2 File Offset: 0x000264A2
		public bool HasBlockingAbilityTarget()
		{
			return this.IsBlockingAbilityTarget(this.currentAbilityTarget);
		}

		// Token: 0x06000CA6 RID: 3238 RVA: 0x000282B0 File Offset: 0x000264B0
		public bool HasBlockingAttackTarget()
		{
			return this.isReadyForBattle && this.HasBlockingAbilityTarget();
		}

		// Token: 0x06000CA7 RID: 3239 RVA: 0x000282C4 File Offset: 0x000264C4
		public IGameMob ForceGetAttackTarget(BaseAbility battleAbility)
		{
			GameMobAIController.TargetsCollector targetsCollector;
			return this.FindAbilityTarget(battleAbility, out targetsCollector, default(GameMobAIController.TargetsCollector), true, false);
		}

		// Token: 0x06000CA8 RID: 3240 RVA: 0x000282E5 File Offset: 0x000264E5
		public bool IsAllowedAbility(BaseAbility ability)
		{
			return this.IsAllowedAbility(ability, ability.IsBattleAbility());
		}

		// Token: 0x06000CA9 RID: 3241 RVA: 0x000282F4 File Offset: 0x000264F4
		public void ResetCurrentAbility()
		{
			this.ResetCurrentAbility(false);
		}

		// Token: 0x06000CAA RID: 3242 RVA: 0x00028300 File Offset: 0x00026500
		public bool TrySetScared(float fearStateDuration, float minRunAwayDistance, Vector2? explicitThreatPoint, bool force = false)
		{
			if ((!force && this.IsScared) || !this.currentParams.canBeScared || this.isOwnerKinematicModeActive || this.IsOnHangingPlatform())
			{
				return false;
			}
			bool isScared = this.IsScared;
			this.ResetBattleAbility();
			this.finalFearStateTime = Time.time + Math.Max(fearStateDuration, 0f) + 0.5f;
			this.explicitThreatPoint = explicitThreatPoint;
			if (this.minRunAwayDistance < minRunAwayDistance)
			{
				this.minRunAwayDistance = minRunAwayDistance;
			}
			this.IsScared = true;
			if (!isScared)
			{
				this.ResetLocalObstacleAvoidanceDirection();
				Action<GameMobAIController, bool> fearStateChanged = this.FearStateChanged;
				if (fearStateChanged != null)
				{
					fearStateChanged(this, true);
				}
			}
			return true;
		}

		// Token: 0x06000CAB RID: 3243 RVA: 0x0002839B File Offset: 0x0002659B
		public void CompleteFearState()
		{
			if (!this.IsScared)
			{
				return;
			}
			this.IsScared = false;
			this.ResetAggressionToInitialState();
			this.currentRunAwayDistance = 0f;
			Action<GameMobAIController, bool> fearStateChanged = this.FearStateChanged;
			if (fearStateChanged == null)
			{
				return;
			}
			fearStateChanged(this, false);
		}

		// Token: 0x06000CAC RID: 3244 RVA: 0x000283D0 File Offset: 0x000265D0
		public GameMobAIController(BaseGameMob targetMob, GameMobAIControllerParams parameters, BuffsBasedStatus[] specialStatuses) : base(targetMob)
		{
			this.currentParams = parameters;
			this.hitPointsController = targetMob.HitPointsController;
			this.motionController = (targetMob.MotionController as GameMobMotionController);
			this.abilitiesController = targetMob.AbilitiesController;
			this.controllerOwnerRadius = GameMobAIController.GetMobRadius(targetMob);
			this.currentSpeedModifier = 1f;
			this.controllerRandomSeed = targetMob.GetInstanceID();
			this.abilityTargetSelector = new GameMobTargetSelector(parameters.targetSelectionMethod, parameters.priorityTargetSelector)
			{
				targetUpdateDelay = 0f
			};
			this.abilityTargetsValidator = new GameMobAIController.AbilityTargetsValidator(this);
			this.specialStatuses = specialStatuses;
			if (this.hitPointsController != null)
			{
				this.hitPointsController.HitPointsChanged += this.OnOwnerHitPointsChanged;
			}
			if (this.motionController != null)
			{
				this.motionController.KinematicMotionStarted += this.OnOwnerKinematicMotionStarted;
			}
			this.ResetExclusivelyUsingAbilitiesDescription();
			targetMob.CurrentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider);
			this.IsActive = true;
		}

		// Token: 0x06000CAD RID: 3245 RVA: 0x000284F0 File Offset: 0x000266F0
		public void OnDeferredUpdate()
		{
			this.hasUnreachableAttackTarget = false;
			this.UpdateAttackState(this.currentAbility != null && this.currentAbility.InUse, false);
			if (!this.isActive)
			{
				return;
			}
			this.UpdateSpecialStatuses();
			if (this.isOwnerKinematicModeActive)
			{
				this.IsIdling = false;
				return;
			}
			GameMobGroupController gameMobGroupController = this.ControllerOwner.Group as GameMobGroupController;
			if (gameMobGroupController != null && gameMobGroupController.IsAttacking)
			{
				this.hasUnreachableAttackTarget = (this.isReadyForBattle && !this.isAbilityUsingDistanceReached && (this.motionController == null || this.motionController.IsMovementFreezed));
				if (this.hasUnreachableAttackTarget)
				{
					this.ResetAbilityTargetFocusTime();
				}
			}
			this.SelectAbility(this.abilityUsingArgs);
			this.UpdateSpeedModifier();
			this.UpdateFearStateAvoidanceDirection();
			this.IsIdling = (!this.IsBusy() && (this.motionController == null || (!this.motionController.HasDestination() && this.motionController.DesiredVelocity.SqrMagnitude() < 0.0001f)));
		}

		// Token: 0x06000CAE RID: 3246 RVA: 0x00028600 File Offset: 0x00026800
		private void OnOwnerKinematicMotionStarted(GameMobKinematicMotionBase motion)
		{
			BaseAbility baseAbility = motion.MotionContext as BaseAbility;
			if (baseAbility == null || (baseAbility != this.currentAbility && baseAbility.ParentAbility != this.currentAbility))
			{
				this.ResetCurrentAbility();
			}
		}

		// Token: 0x06000CAF RID: 3247 RVA: 0x00028644 File Offset: 0x00026844
		private void OnOwnerHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (this.currentParams.hasResponseAggression && args.IsDamage && args.Amount > 0f)
			{
				BaseGameMob baseGameMob = sender as BaseGameMob;
				this.IsAggressive = true;
				this.TrySetExplicitAbilityTarget(baseGameMob);
				if (baseGameMob != null && !this.hitPointsController.IsAlive)
				{
					GameMobGroupController gameMobGroupController = this.ControllerOwner.Group as GameMobGroupController;
					if (gameMobGroupController != null && !gameMobGroupController.HasAttackTargets)
					{
						IReadOnlyList<BaseGameMob> mobs = gameMobGroupController.Mobs;
						for (int i = 0; i < mobs.Count; i++)
						{
							GameMobAIController aicontroller = mobs[i].AIController;
							if (aicontroller != null && aicontroller != this && aicontroller.TrySetExplicitAbilityTarget(baseGameMob))
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000CB0 RID: 3248 RVA: 0x000286FD File Offset: 0x000268FD
		private void OnAbilityTargetKilled(IGameMob killedAbilityTarget)
		{
			this.SetAbilityTarget(null, false);
		}

		// Token: 0x06000CB1 RID: 3249 RVA: 0x00028708 File Offset: 0x00026908
		protected override void OnOwnerKilled(IGameMob owner)
		{
			this.IsActive = false;
			if (this.hitPointsController != null)
			{
				this.hitPointsController.HitPointsChanged -= this.OnOwnerHitPointsChanged;
			}
			if (this.motionController != null)
			{
				this.motionController.KinematicMotionStarted -= this.OnOwnerKinematicMotionStarted;
			}
			base.OnOwnerKilled(owner);
		}

		// Token: 0x06000CB2 RID: 3250 RVA: 0x00028764 File Offset: 0x00026964
		public void OnUpdate()
		{
			this.isAbilityUsingDistanceReached = false;
			this.isAimStoppingPrepProgressReached = false;
			if (this.isWaitingForAbilityReset || (this.hasBlockingTarget && this.motionController.CurrentBlockingMob == null))
			{
				this.ResetCurrentAbility(true);
			}
			if (!this.isActive)
			{
				return;
			}
			if (this.isOwnerKinematicModeActive = this.ControllerOwner.IsKinematic)
			{
				this.CompleteFearState();
				return;
			}
			this.UseCurrentAbility(this.abilityUsingArgs);
			this.UpdateFearState();
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x000287F4 File Offset: 0x000269F4
		[CompilerGenerated]
		internal static bool <IsAbilitiesTargetsSearchRequired>g__IsGroupInBattle|127_0(GameMobsGroupControllerBase group)
		{
			GameMobGroupController gameMobGroupController = group as GameMobGroupController;
			return gameMobGroupController != null && gameMobGroupController.InBattle;
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x00028813 File Offset: 0x00026A13
		[CompilerGenerated]
		internal static void <GetLocalAvoidanceDirection>g__AccumulateAvoidanceDirection|138_0(Vector2 hitNormal, ref Vector2 movementDirection)
		{
			movementDirection.x += hitNormal.x;
			movementDirection.y += hitNormal.y;
		}

		// Token: 0x0400070F RID: 1807
		private static readonly Collider2D[] TargetsSearchBuffer = new Collider2D[128];

		// Token: 0x04000716 RID: 1814
		private readonly IDamageable hitPointsController;

		// Token: 0x04000717 RID: 1815
		private readonly GameAbilitiesController abilitiesController;

		// Token: 0x04000718 RID: 1816
		private readonly GameMobMotionController motionController;

		// Token: 0x04000719 RID: 1817
		private readonly GameMobTargetSelector abilityTargetSelector;

		// Token: 0x0400071A RID: 1818
		private readonly float controllerOwnerRadius;

		// Token: 0x0400071B RID: 1819
		private readonly GameMobAIController.AbilityTargetsValidator abilityTargetsValidator;

		// Token: 0x0400071C RID: 1820
		private readonly BaseAbility.UsingArgs abilityUsingArgs = new BaseAbility.UsingArgs();

		// Token: 0x0400071D RID: 1821
		private readonly MobAbilityTriggerArgs abilityTriggersArgs = new MobAbilityTriggerArgs();

		// Token: 0x0400071E RID: 1822
		private readonly int controllerRandomSeed;

		// Token: 0x0400071F RID: 1823
		private readonly BuffsBasedStatus[] specialStatuses;

		// Token: 0x04000720 RID: 1824
		private GameMobAIControllerParams currentParams;

		// Token: 0x04000721 RID: 1825
		private bool isOwnerKinematicModeActive;

		// Token: 0x04000722 RID: 1826
		private int abilitySelectionFrame;

		// Token: 0x04000723 RID: 1827
		private BaseAbility currentAbility;

		// Token: 0x04000724 RID: 1828
		private MobAbilityParameters currentAbilityParams;

		// Token: 0x04000725 RID: 1829
		private BaseAbility lastUsedAbility;

		// Token: 0x04000726 RID: 1830
		private bool isAimStoppingPrepProgressReached;

		// Token: 0x04000727 RID: 1831
		private bool isWaitingForAbilityReset;

		// Token: 0x04000728 RID: 1832
		private IGameMob currentAbilityTarget;

		// Token: 0x04000729 RID: 1833
		private bool hasBlockingTarget;

		// Token: 0x0400072A RID: 1834
		private IGameMob currentExplicitAbilityTarget;

		// Token: 0x0400072B RID: 1835
		private Vector2 abilityTargetPosition;

		// Token: 0x0400072C RID: 1836
		private Vector2 fearStateRunAwayDirection;

		// Token: 0x0400072D RID: 1837
		private Vector2? explicitThreatPoint;

		// Token: 0x0400072E RID: 1838
		private Vector2 localObstacleAvoidanceDirection;

		// Token: 0x0400072F RID: 1839
		private float minRunAwayDistance;

		// Token: 0x04000730 RID: 1840
		private float currentRunAwayDistance;

		// Token: 0x04000731 RID: 1841
		private float maxRunAwayTrajectoryVariation = 70f;

		// Token: 0x04000732 RID: 1842
		private float finalFearStateTime;

		// Token: 0x04000733 RID: 1843
		private float abilityTargetDistance;

		// Token: 0x04000734 RID: 1844
		private bool isAbilityUsingDistanceReached;

		// Token: 0x04000735 RID: 1845
		private float currentSpeedModifier;

		// Token: 0x04000736 RID: 1846
		private float abilityTargetContactDistance;

		// Token: 0x04000737 RID: 1847
		private float minAbilityTargetDistance;

		// Token: 0x04000738 RID: 1848
		private float abilityTargetFocusEndTime;

		// Token: 0x04000739 RID: 1849
		private float abilityTargetResetTime;

		// Token: 0x0400073A RID: 1850
		private float nextAbilityActivationTime;

		// Token: 0x0400073B RID: 1851
		private bool isAggressive;

		// Token: 0x0400073C RID: 1852
		private bool canUseSelectedAbility;

		// Token: 0x0400073D RID: 1853
		private bool isReadyForBattle;

		// Token: 0x0400073E RID: 1854
		private bool isAttacking;

		// Token: 0x0400073F RID: 1855
		private float attackingStateResetTime;

		// Token: 0x04000740 RID: 1856
		private bool hasUnreachableAttackTarget;

		// Token: 0x04000741 RID: 1857
		private Func<BaseGameMob, IGameMob, bool> enemyTargetsAdditionalValidator;

		// Token: 0x04000742 RID: 1858
		private Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation> enemyTargetsPriorityEstimatorOverride;

		// Token: 0x04000743 RID: 1859
		private AbilityDescription exclusivelyUsingAbilitiesDescription;

		// Token: 0x04000744 RID: 1860
		private IPlayerProvider playerProvider;

		// Token: 0x04000745 RID: 1861
		private bool isActive;

		// Token: 0x0200047F RID: 1151
		private struct TargetsCollector
		{
			// Token: 0x06002404 RID: 9220 RVA: 0x0006F6BC File Offset: 0x0006D8BC
			public static GameMobAIController.TargetsCollector GetMostReusableCollector(GameMobAIController.TargetsCollector lastCollector, GameMobAIController.TargetsCollector newCollector)
			{
				if (!lastCollector.HasCollectedTargets || !lastCollector.CanCollectTargetsFor(newCollector))
				{
					return newCollector;
				}
				return lastCollector;
			}

			// Token: 0x17000751 RID: 1873
			// (get) Token: 0x06002405 RID: 9221 RVA: 0x0006F6D4 File Offset: 0x0006D8D4
			// (set) Token: 0x06002406 RID: 9222 RVA: 0x0006F6DC File Offset: 0x0006D8DC
			public int TargetsInRangeCount { readonly get; private set; }

			// Token: 0x17000752 RID: 1874
			// (get) Token: 0x06002407 RID: 9223 RVA: 0x0006F6E5 File Offset: 0x0006D8E5
			public bool HasCollectedTargets
			{
				get
				{
					return this.TargetsInRangeCount != 0;
				}
			}

			// Token: 0x06002408 RID: 9224 RVA: 0x0006F6F0 File Offset: 0x0006D8F0
			public TargetsCollector(BaseGameMob targetsSearcher, int layers, float range)
			{
				this.Layers = layers;
				this.Range = range;
				this.TargetsInRangeCount = 0;
				this.targetsSearcher = targetsSearcher;
				this.targetsBuffer = null;
			}

			// Token: 0x06002409 RID: 9225 RVA: 0x0006F715 File Offset: 0x0006D915
			public bool IsCollectableLayers(int layers)
			{
				return (this.Layers & layers) == layers;
			}

			// Token: 0x0600240A RID: 9226 RVA: 0x0006F722 File Offset: 0x0006D922
			public bool CanCollectTargetsFor(GameMobAIController.TargetsCollector otherCollector)
			{
				return this.IsCollectableLayers(otherCollector.Layers) && this.Range >= otherCollector.Range;
			}

			// Token: 0x0600240B RID: 9227 RVA: 0x0006F745 File Offset: 0x0006D945
			public void PassTargetsToTargetSelector(GameMobTargetSelector targetSelector)
			{
				if (targetSelector == null)
				{
					return;
				}
				targetSelector.CurrentTargets = this.targetsBuffer;
				targetSelector.TargetsCountOverride = this.TargetsInRangeCount;
			}

			// Token: 0x0600240C RID: 9228 RVA: 0x0006F764 File Offset: 0x0006D964
			public bool CollectTargets(Vector2 position, GameMobTargetSelector targetSelector, BaseAbility targetAbility)
			{
				GameLocation currentLocation = this.targetsSearcher.CurrentLocation;
				if (currentLocation != null)
				{
					GameMobAIController.TargetsCollector.CollectionArgs.position = position;
					GameMobAIController.TargetsCollector.CollectionArgs.range = this.Range;
					GameMobAIController.TargetsCollector.CollectionArgs.layers = this.Layers;
					BaseGameMob[] array;
					this.TargetsInRangeCount = currentLocation.GetMobsInRange(GameMobAIController.TargetsCollector.CollectionArgs, out array);
					this.targetsBuffer = array;
					if (this.TargetsInRangeCount == 1 && !targetAbility.CanBeUsedOnOwner && targetAbility.OwnerBehaviour.gameObject == this.targetsBuffer[0].gameObject)
					{
						this.TargetsInRangeCount = 0;
					}
					else if (this.TargetsInRangeCount != 0)
					{
						this.PassTargetsToTargetSelector(targetSelector);
						return true;
					}
				}
				this.TargetsInRangeCount = 0;
				return false;
			}

			// Token: 0x0400178C RID: 6028
			private static readonly GameLocation.MobsGatheringArgs CollectionArgs = new GameLocation.MobsGatheringArgs();

			// Token: 0x0400178E RID: 6030
			public readonly int Layers;

			// Token: 0x0400178F RID: 6031
			public readonly float Range;

			// Token: 0x04001790 RID: 6032
			private readonly BaseGameMob targetsSearcher;

			// Token: 0x04001791 RID: 6033
			private BaseGameMob[] targetsBuffer;
		}

		// Token: 0x02000480 RID: 1152
		private sealed class AbilityTargetsValidator
		{
			// Token: 0x0600240E RID: 9230 RVA: 0x0006F828 File Offset: 0x0006DA28
			private bool IsVisibleTarget(IGameMob targetMob)
			{
				if (this.forceIgnoreVisibilityObstacles || this.visibilityBlockingLayers == 0)
				{
					return true;
				}
				Collider2D collider = Physics2D.Linecast(this.abilityOwnerPosition, targetMob.Position, this.visibilityBlockingLayers).collider;
				return collider == null || collider.gameObject == targetMob.GameObject;
			}

			// Token: 0x0600240F RID: 9231 RVA: 0x0006F883 File Offset: 0x0006DA83
			public AbilityTargetsValidator(GameMobAIController aiController)
			{
				this.aiController = aiController;
				this.owner = aiController.ControllerOwner;
				this.targetSelectorValidatorProvider = new GameMobAIController.AbilityTargetsValidator.TargetSelectorValidatorAdapter(this);
			}

			// Token: 0x06002410 RID: 9232 RVA: 0x0006F8AA File Offset: 0x0006DAAA
			public void SetLayers(int allowedTargetsLayers, int visibilityBlockingLayers)
			{
				this.allowedTargetsLayers = allowedTargetsLayers;
				this.visibilityBlockingLayers = visibilityBlockingLayers;
				this.skipTargetsLayerValidation = (allowedTargetsLayers == 0);
			}

			// Token: 0x06002411 RID: 9233 RVA: 0x0006F8C4 File Offset: 0x0006DAC4
			public void SetLayers(int allowedTargetsLayers, GameMobAIControllerParams controllerParams)
			{
				this.SetLayers(allowedTargetsLayers, controllerParams.aggressionObstacleLayers);
			}

			// Token: 0x06002412 RID: 9234 RVA: 0x0006F8D8 File Offset: 0x0006DAD8
			public void Prepare(BaseAbility targetAbility, GameMobTargetSelector targetSelector, MobAbilityParameters abilityParameters)
			{
				this.includeAbilityOwner = targetAbility.CanBeUsedOnOwner;
				this.abilityOwnerPosition = this.owner.Position;
				this.isMobActivationAbilityCase = (targetAbility.IsPostMortemAbility && this.owner.IsSacrificed);
				this.visibilityBlockingLayers |= targetAbility.GetProjectileLayers(false).Item1;
				this.useTargetsFilter = false;
				this.forceIgnoreVisibilityObstacles = false;
				GameMobMotionControllerBase motionController = this.owner.MotionController;
				if (motionController != null)
				{
					BaseGameMob currentBlockingMob = motionController.CurrentBlockingMob;
				}
				GameMobsGroupControllerBase group = this.owner.Group;
				if (targetSelector != null)
				{
					targetSelector.AdditionalTargetValidator = this.targetSelectorValidatorProvider.CachedTargetValidator;
				}
				if (abilityParameters != null)
				{
					this.useTargetsFilter = abilityParameters.TryPassAllowedTargetsFilter(ref this.abilitySpecificTargetsFilter);
					this.forceIgnoreVisibilityObstacles = abilityParameters.forceIgnoreAggressionObstacles;
				}
				if (!this.useTargetsFilter)
				{
					this.useTargetsFilter = this.aiController.currentParams.additionalAttackTargetsFilter.TryPassTo(ref this.abilitySpecificTargetsFilter);
				}
			}

			// Token: 0x06002413 RID: 9235 RVA: 0x0006F9CC File Offset: 0x0006DBCC
			public bool IsValidTarget(IGameMob target, bool checkNull, bool checkVisibility, bool respectMinorTargets = true)
			{
				if ((!checkNull || target != null) && (this.skipTargetsLayerValidation || target.IsLayerInMask(this.allowedTargetsLayers)))
				{
					bool flag = target != this.owner;
					bool flag2;
					if (this.isAttackTargetsValidator)
					{
						flag2 = (flag && (respectMinorTargets || !target.IsMinorAttackTarget) && target.CanBeAttackedBy(this.owner));
						if (flag2 && this.aiController.enemyTargetsAdditionalValidator != null)
						{
							flag2 &= this.aiController.enemyTargetsAdditionalValidator(this.owner, target);
						}
					}
					else
					{
						flag2 = (!target.IsKilled && (this.includeAbilityOwner || flag) && target.IsCharacterByDefault());
					}
					if (flag2)
					{
						flag2 &= ((!this.useTargetsFilter || this.abilitySpecificTargetsFilter.IsMatch(target)) && (!checkVisibility || this.IsVisibleTarget(target)));
					}
					return flag2;
				}
				return false;
			}

			// Token: 0x06002414 RID: 9236 RVA: 0x0006FAA6 File Offset: 0x0006DCA6
			public bool IsValidTarget(IGameMob mob)
			{
				return this.IsValidTarget(mob, true, false, true);
			}

			// Token: 0x06002415 RID: 9237 RVA: 0x0006FAB4 File Offset: 0x0006DCB4
			public bool HasValidBlockingTarget(IGameMob selectedTarget, out IGameMob blockingTarget)
			{
				GameMobMotionControllerBase motionController = this.owner.MotionController;
				blockingTarget = ((motionController != null) ? motionController.CurrentBlockingMob : null);
				if (!this.isAttackTargetsValidator || blockingTarget == selectedTarget || !this.IsValidTarget(blockingTarget) || (!blockingTarget.IsCharacter && blockingTarget.Faction == this.owner.Faction))
				{
					return false;
				}
				if (selectedTarget != null)
				{
					Vector2 position = this.owner.Position;
					Vector2 lhs = blockingTarget.Position - position;
					lhs.Normalize();
					Vector2 rhs = selectedTarget.Position - position;
					rhs.Normalize();
					return Vector2.Dot(lhs, rhs) > 0.1f;
				}
				return true;
			}

			// Token: 0x04001792 RID: 6034
			public int allowedTargetsLayers;

			// Token: 0x04001793 RID: 6035
			public int visibilityBlockingLayers;

			// Token: 0x04001794 RID: 6036
			public bool skipTargetsLayerValidation;

			// Token: 0x04001795 RID: 6037
			public bool isAttackTargetsValidator;

			// Token: 0x04001796 RID: 6038
			private readonly GameMobAIController aiController;

			// Token: 0x04001797 RID: 6039
			private readonly BaseGameMob owner;

			// Token: 0x04001798 RID: 6040
			private readonly GameMobAIController.AbilityTargetsValidator.TargetSelectorValidatorAdapter targetSelectorValidatorProvider;

			// Token: 0x04001799 RID: 6041
			private bool includeAbilityOwner;

			// Token: 0x0400179A RID: 6042
			private Vector2 abilityOwnerPosition;

			// Token: 0x0400179B RID: 6043
			private bool isMobActivationAbilityCase;

			// Token: 0x0400179C RID: 6044
			private GameMobDescription abilitySpecificTargetsFilter;

			// Token: 0x0400179D RID: 6045
			private bool useTargetsFilter;

			// Token: 0x0400179E RID: 6046
			private bool forceIgnoreVisibilityObstacles;

			// Token: 0x020005B1 RID: 1457
			private sealed class TargetSelectorValidatorAdapter
			{
				// Token: 0x060027D7 RID: 10199 RVA: 0x0007C73A File Offset: 0x0007A93A
				public TargetSelectorValidatorAdapter(GameMobAIController.AbilityTargetsValidator parentValidator)
				{
					this.parentValidator = parentValidator;
					this.CachedTargetValidator = new Predicate<BaseGameMob>(this.IsValidTarget);
				}

				// Token: 0x060027D8 RID: 10200 RVA: 0x0007C75B File Offset: 0x0007A95B
				public bool IsValidTarget(BaseGameMob mob)
				{
					return this.parentValidator.IsValidTarget(mob, false, true, this.parentValidator.isMobActivationAbilityCase);
				}

				// Token: 0x04001D29 RID: 7465
				public readonly Predicate<BaseGameMob> CachedTargetValidator;

				// Token: 0x04001D2A RID: 7466
				private readonly GameMobAIController.AbilityTargetsValidator parentValidator;
			}
		}
	}
}
