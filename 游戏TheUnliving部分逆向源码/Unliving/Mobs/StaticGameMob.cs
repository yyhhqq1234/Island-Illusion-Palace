using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using Game.Factories;
using Game.PassiveAbilities;
using Game.Stats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x02000201 RID: 513
	[DisallowMultipleComponent]
	public sealed class StaticGameMob : BaseGameMob
	{
		// Token: 0x1700038E RID: 910
		// (get) Token: 0x06001118 RID: 4376 RVA: 0x0003573F File Offset: 0x0003393F
		// (set) Token: 0x06001119 RID: 4377 RVA: 0x00035747 File Offset: 0x00033947
		public StaticGameMob.AbilityDecorator[] InitialAbilities
		{
			get
			{
				return this._initialAbilities;
			}
			set
			{
				this._initialAbilities = value;
			}
		}

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x0600111A RID: 4378 RVA: 0x00035750 File Offset: 0x00033950
		// (set) Token: 0x0600111B RID: 4379 RVA: 0x00035758 File Offset: 0x00033958
		public bool TryToUseAbilitiesConstantly
		{
			get
			{
				return this._tryToUseAbilitiesConstantly;
			}
			set
			{
				this._tryToUseAbilitiesConstantly = value;
			}
		}

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x0600111C RID: 4380 RVA: 0x00035761 File Offset: 0x00033961
		// (set) Token: 0x0600111D RID: 4381 RVA: 0x00035769 File Offset: 0x00033969
		public GameMobAreaObserver AttackTargetsProvider
		{
			get
			{
				return this._attackTargetsProvider;
			}
			set
			{
				this._attackTargetsProvider = value;
			}
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x0600111E RID: 4382 RVA: 0x00035772 File Offset: 0x00033972
		// (set) Token: 0x0600111F RID: 4383 RVA: 0x0003577A File Offset: 0x0003397A
		public Transform MuzzleTransform
		{
			get
			{
				return this._muzzleTransform;
			}
			set
			{
				this._muzzleTransform = value;
			}
		}

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x06001120 RID: 4384 RVA: 0x00035783 File Offset: 0x00033983
		// (set) Token: 0x06001121 RID: 4385 RVA: 0x0003578B File Offset: 0x0003398B
		public float AimingSpeed
		{
			get
			{
				return this._aimingSpeed;
			}
			set
			{
				this._aimingSpeed = value;
			}
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06001122 RID: 4386 RVA: 0x00035794 File Offset: 0x00033994
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06001123 RID: 4387 RVA: 0x0003579C File Offset: 0x0003399C
		// (set) Token: 0x06001124 RID: 4388 RVA: 0x000357B0 File Offset: 0x000339B0
		public bool IsAccessible
		{
			get
			{
				return this.isActive && this.isAccessible;
			}
			set
			{
				if (value != this.isAccessible)
				{
					this.isAccessible = value;
					this.SetActive(this.isActive, true);
					if (value)
					{
						UnityEvent becameAccessible = this._becameAccessible;
						if (becameAccessible == null)
						{
							return;
						}
						becameAccessible.Invoke();
						return;
					}
					else
					{
						UnityEvent becameUnaccessible = this._becameUnaccessible;
						if (becameUnaccessible == null)
						{
							return;
						}
						becameUnaccessible.Invoke();
					}
				}
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06001125 RID: 4389 RVA: 0x000357FE File Offset: 0x000339FE
		public IReadOnlyList<BaseGameMob> TargetsInRange
		{
			get
			{
				if (this._attackTargetsProvider.IsNull())
				{
					return null;
				}
				return this._attackTargetsProvider.ObjectsInRange;
			}
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06001126 RID: 4390 RVA: 0x0003581A File Offset: 0x00033A1A
		public BaseGameMob SelectedTarget
		{
			get
			{
				if (this._attackTargetsProvider.IsNull())
				{
					return null;
				}
				return this._attackTargetsProvider.SelectedObject;
			}
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06001127 RID: 4391 RVA: 0x00035836 File Offset: 0x00033A36
		public bool CanAim
		{
			get
			{
				return this._muzzleTransform != null && this._aimingSpeed > 0f;
			}
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x06001128 RID: 4392 RVA: 0x00035855 File Offset: 0x00033A55
		public bool IsAimed
		{
			get
			{
				return this.isAimed;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x06001129 RID: 4393 RVA: 0x0003585D File Offset: 0x00033A5D
		public PlayerBehaviour PlayerInRange
		{
			get
			{
				return this.playerInRange;
			}
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x0600112A RID: 4394 RVA: 0x00035865 File Offset: 0x00033A65
		public UnityEvent Activated
		{
			get
			{
				return this._activated;
			}
		}

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x0600112B RID: 4395 RVA: 0x0003586D File Offset: 0x00033A6D
		public UnityEvent Deactivated
		{
			get
			{
				return this._deactivated;
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x0600112C RID: 4396 RVA: 0x00035875 File Offset: 0x00033A75
		public UnityEvent BecameAccessible
		{
			get
			{
				return this._becameAccessible;
			}
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x0600112D RID: 4397 RVA: 0x0003587D File Offset: 0x00033A7D
		public UnityEvent BecameUnaccessible
		{
			get
			{
				return this._becameUnaccessible;
			}
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x0600112E RID: 4398 RVA: 0x00035885 File Offset: 0x00033A85
		public override bool IsCharacter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x0600112F RID: 4399 RVA: 0x00035888 File Offset: 0x00033A88
		public override GameMobMotionControllerBase MotionController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x06001130 RID: 4400 RVA: 0x0003588B File Offset: 0x00033A8B
		public override GameMobAIController AIController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x06001131 RID: 4401 RVA: 0x0003588E File Offset: 0x00033A8E
		public override GameAbilitiesController AbilitiesController
		{
			get
			{
				return this.abilitiesController;
			}
		}

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x06001132 RID: 4402 RVA: 0x00035896 File Offset: 0x00033A96
		public override BasePassiveAbilitiesController PassiveAbilitiesController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06001133 RID: 4403 RVA: 0x00035899 File Offset: 0x00033A99
		public override StatsControllerBase<MobStatModifier> StatsController
		{
			get
			{
				return this.statsController;
			}
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x06001134 RID: 4404 RVA: 0x000358A1 File Offset: 0x00033AA1
		public override Vector2 CurrentLookDirection
		{
			get
			{
				if (!(this._muzzleTransform != null))
				{
					return base.CurrentLookDirection;
				}
				return this._muzzleTransform.TransformDirection2D(Vector2.up);
			}
		}

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x06001135 RID: 4405 RVA: 0x000358C8 File Offset: 0x00033AC8
		protected override bool CanGenerateResources
		{
			get
			{
				return this.canGenerateResources;
			}
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x000358D0 File Offset: 0x00033AD0
		private void SetActive(bool newActivityState, bool force = false)
		{
			if (force || this.isActive != newActivityState)
			{
				this.isActive = newActivityState;
				if (this.isAccessible && this.isActive)
				{
					this._activated.Invoke();
					return;
				}
				this.abilitiesController.ForAll(new Action<IAbility>(StaticGameMob.<SetActive>g__CompleteAbility|78_0));
				BaseAbility.UsingArgs usingArgs = this.abilitiesUsingArgs;
				if (usingArgs != null)
				{
					usingArgs.Reset();
				}
				this._deactivated.Invoke();
			}
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x0003593F File Offset: 0x00033B3F
		private void UpdateActivityState()
		{
			if (!this._tryToUseAbilitiesConstantly)
			{
				this.SetActive(this.validTargetsCount > 0, false);
			}
		}

		// Token: 0x06001138 RID: 4408 RVA: 0x00035959 File Offset: 0x00033B59
		protected override Transform GetDefaultShootingPointsTransform()
		{
			if (!this._muzzleTransform.IsNull())
			{
				return this._muzzleTransform;
			}
			return base.transform;
		}

		// Token: 0x06001139 RID: 4409 RVA: 0x00035978 File Offset: 0x00033B78
		private bool IsValidTarget(BaseGameMob target)
		{
			for (int i = 0; i < this._initialAbilities.Length; i++)
			{
				if (this._initialAbilities[i].IsValidAbilityTarget(target))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600113A RID: 4410 RVA: 0x000359AB File Offset: 0x00033BAB
		private bool HasTotallyExhaustedAbilities()
		{
			return this._destroyIfTotallyExhausted && this.exhaustedAbilitiesCount >= this._initialAbilities.Length;
		}

		// Token: 0x0600113B RID: 4411 RVA: 0x000359CA File Offset: 0x00033BCA
		private void OnMobEnteredRange(BaseGameMob obj)
		{
			if (this.playerInRange.IsNull())
			{
				this.playerInRange = (obj as PlayerBehaviour);
			}
			if (this.IsValidTarget(obj))
			{
				this.validTargetsCount++;
			}
			this.UpdateActivityState();
		}

		// Token: 0x0600113C RID: 4412 RVA: 0x00035A02 File Offset: 0x00033C02
		private void OnMobExitedRange(BaseGameMob obj)
		{
			if (this.playerInRange == obj)
			{
				this.playerInRange = null;
			}
			if (this.IsValidTarget(obj))
			{
				this.validTargetsCount--;
			}
			this.UpdateActivityState();
		}

		// Token: 0x0600113D RID: 4413 RVA: 0x00035A38 File Offset: 0x00033C38
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.isEnvironmentMob = !this.affectLocationChunkLogic;
			this.abilitiesController = new StaticGameMob.MobAbilitiesController(this, base.CurrentGame.Services.Get<IGameAbilitiesFactory>());
			this.statsController = new StatsController(this, false);
			this.abilitiesUsingArgs = new BaseAbility.UsingArgs();
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x00035A90 File Offset: 0x00033C90
		protected override void OnMobInitialization()
		{
			base.OnMobInitialization();
			this.playerInRange = null;
			if (this._attackTargetsProvider == null)
			{
				base.TryGetComponent<GameMobAreaObserver>(out this._attackTargetsProvider);
			}
			for (int i = 0; i < this._initialAbilities.Length; i++)
			{
				StaticGameMob.AbilityDecorator abilityDecorator = this._initialAbilities[i];
				abilityDecorator.Initialize(this);
				if (abilityDecorator.CurrentAbility != null && this.abilitiesController.AddAbility(abilityDecorator.CurrentAbility))
				{
					GameAbilitiesController.HandleAbilityToMobStatsAttachment(this.statsController, abilityDecorator.CurrentAbility, true);
				}
			}
			IDamageSender statOwner;
			if (this.statsController.GetStat(2) == null && base.TryGetComponent<IDamageSender>(out statOwner))
			{
				this.statsController.AddStat(new MobDamageStat(statOwner));
			}
			if (!this._attackTargetsProvider.IsNull())
			{
				this._attackTargetsProvider.ObjectEnteredArea += this.OnMobEnteredRange;
				this._attackTargetsProvider.ObjectExitedArea += this.OnMobExitedRange;
			}
			this.SetActive(this._tryToUseAbilitiesConstantly || this._attackTargetsProvider == null, true);
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x00035BA0 File Offset: 0x00033DA0
		protected override void Update()
		{
			this.isAimed = false;
			if (this.isActive && !base.IsKilled)
			{
				BaseGameMob selectedTarget = this.SelectedTarget;
				if (!selectedTarget.IsNull())
				{
					this.abilitiesUsingArgs.targetObject = selectedTarget;
					this.abilitiesUsingArgs.targetPosition = selectedTarget.HitColliderCenter;
					if (this.CanAim)
					{
						this.isAimed = (Mathf.Abs(this._muzzleTransform.LookAt2D(this.abilitiesUsingArgs.targetPosition, this._aimingSpeed, true)) < this._maxAimingError);
					}
					else
					{
						this.isAimed = true;
					}
				}
				else if (!this.CanAim)
				{
					this.abilitiesUsingArgs.targetPosition = base.HitColliderCenter;
				}
			}
			base.Update();
			if (this.HasTotallyExhaustedAbilities())
			{
				base.KillMob(this);
			}
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x00035C78 File Offset: 0x00033E78
		protected override void OnKilled(IDamageable hitPointsController)
		{
			if (!this._attackTargetsProvider.IsNull())
			{
				this._attackTargetsProvider.ObjectEnteredArea -= this.OnMobEnteredRange;
				this._attackTargetsProvider.ObjectExitedArea -= this.OnMobExitedRange;
			}
			this.SetActive(false, true);
			base.OnKilled(hitPointsController);
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x00035CFB File Offset: 0x00033EFB
		[CompilerGenerated]
		internal static void <SetActive>g__CompleteAbility|78_0(IAbility ability)
		{
			if (!((BaseAbility)ability).IsPostMortemAbility)
			{
				ability.Complete();
			}
		}

		// Token: 0x040009AC RID: 2476
		[SerializeField]
		private bool canGenerateResources;

		// Token: 0x040009AD RID: 2477
		[SerializeField]
		private StaticGameMob.AbilityDecorator[] _initialAbilities;

		// Token: 0x040009AE RID: 2478
		[SerializeField]
		[Tooltip("Если включено, то абилити будут применяться независимо от наличия целей в радиусе.")]
		private bool _tryToUseAbilitiesConstantly;

		// Token: 0x040009AF RID: 2479
		[SerializeField]
		[Tooltip("Моб будет уничтожен если ВСЕ его абилити были использованы максимально возможное количество раз.")]
		private bool _destroyIfTotallyExhausted;

		// Token: 0x040009B0 RID: 2480
		[SerializeField]
		[Tooltip("Ссылка на компонент AreaObserver, который предоставляет список целей, находящихся в его радиусе.")]
		private GameMobAreaObserver _attackTargetsProvider;

		// Token: 0x040009B1 RID: 2481
		[SerializeField]
		[Tooltip("Ссылка на вращающееся тело или ствол для прицеливания. Если не задана, то моб не будет наводиться на цель.")]
		private Transform _muzzleTransform;

		// Token: 0x040009B2 RID: 2482
		[SerializeField]
		[Tooltip("Скорость наведения на цель.")]
		private float _aimingSpeed = 50f;

		// Token: 0x040009B3 RID: 2483
		[SerializeField]
		[Tooltip("Минимальный угол наведения, необходимый для стрельбы.")]
		private float _maxAimingError = 1f;

		// Token: 0x040009B4 RID: 2484
		[Obsolete]
		public bool affectLocationChunkLogic = true;

		// Token: 0x040009B5 RID: 2485
		[Space(5f)]
		[SerializeField]
		private UnityEvent _activated;

		// Token: 0x040009B6 RID: 2486
		[SerializeField]
		private UnityEvent _deactivated;

		// Token: 0x040009B7 RID: 2487
		[SerializeField]
		[FormerlySerializedAs("AccessibleStateEvents")]
		private UnityEvent _becameAccessible;

		// Token: 0x040009B8 RID: 2488
		[SerializeField]
		[FormerlySerializedAs("UnaccessibleStateEvents")]
		private UnityEvent _becameUnaccessible;

		// Token: 0x040009B9 RID: 2489
		private GameAbilitiesController abilitiesController;

		// Token: 0x040009BA RID: 2490
		private StatsController statsController;

		// Token: 0x040009BB RID: 2491
		private BaseAbility.UsingArgs abilitiesUsingArgs;

		// Token: 0x040009BC RID: 2492
		private bool isAimed;

		// Token: 0x040009BD RID: 2493
		private PlayerBehaviour playerInRange;

		// Token: 0x040009BE RID: 2494
		private int validTargetsCount;

		// Token: 0x040009BF RID: 2495
		private int exhaustedAbilitiesCount;

		// Token: 0x040009C0 RID: 2496
		[NonSerialized]
		private bool isActive;

		// Token: 0x040009C1 RID: 2497
		[NonSerialized]
		private bool isAccessible = true;

		// Token: 0x020004AA RID: 1194
		public sealed class MobAbilitiesController : GameAbilitiesController
		{
			// Token: 0x06002492 RID: 9362 RVA: 0x00071311 File Offset: 0x0006F511
			public MobAbilitiesController(StaticGameMob abilitiesOwner, IGameAbilitiesFactory abilitiesFactory) : base(abilitiesOwner, abilitiesFactory)
			{
				this.currentMob = abilitiesOwner;
			}

			// Token: 0x06002493 RID: 9363 RVA: 0x00071324 File Offset: 0x0006F524
			protected override void UpdateAbilities(float deltaTime)
			{
				for (int i = 0; i < this.abilities.Count; i++)
				{
					BaseAbility baseAbility = this.abilities[i];
					if (this.currentMob.isActive && !baseAbility.IsPostMortemAbility && !baseAbility.IsAutoUseAbility)
					{
						baseAbility.Activate(this.currentMob.abilitiesUsingArgs);
					}
					baseAbility.UpdateAbility(deltaTime);
				}
			}

			// Token: 0x06002494 RID: 9364 RVA: 0x00071389 File Offset: 0x0006F589
			public override bool IsSpecialAbility(int abilityID)
			{
				return true;
			}

			// Token: 0x0400191C RID: 6428
			private readonly StaticGameMob currentMob;
		}

		// Token: 0x020004AB RID: 1195
		[Serializable]
		public sealed class AbilityDecorator
		{
			// Token: 0x06002495 RID: 9365 RVA: 0x0007138C File Offset: 0x0006F58C
			private static int GetAbilityEffectResourcesCost(float effectAmount)
			{
				if (effectAmount <= 0f)
				{
					return 0;
				}
				return Mathf.Max(Mathf.CeilToInt(effectAmount), 1);
			}

			// Token: 0x1700076C RID: 1900
			// (get) Token: 0x06002496 RID: 9366 RVA: 0x000713A4 File Offset: 0x0006F5A4
			public BaseAbility CurrentAbility
			{
				get
				{
					return this._currentAbility;
				}
			}

			// Token: 0x1700076D RID: 1901
			// (get) Token: 0x06002497 RID: 9367 RVA: 0x000713AC File Offset: 0x0006F5AC
			public int CurrentResourceAmount
			{
				get
				{
					return this.currentResourceAmount;
				}
			}

			// Token: 0x1700076E RID: 1902
			// (get) Token: 0x06002498 RID: 9368 RVA: 0x000713B4 File Offset: 0x0006F5B4
			public bool HasLimitedResource
			{
				get
				{
					return this.maxResourceCapacity > 0;
				}
			}

			// Token: 0x1700076F RID: 1903
			// (get) Token: 0x06002499 RID: 9369 RVA: 0x000713BF File Offset: 0x0006F5BF
			public bool IsExhausted
			{
				get
				{
					return this.HasLimitedResource && this.currentResourceAmount <= 0;
				}
			}

			// Token: 0x0600249A RID: 9370 RVA: 0x000713D7 File Offset: 0x0006F5D7
			public AbilityDecorator(StaticGameMob owner)
			{
				if (!Application.isPlaying)
				{
					return;
				}
				this.Initialize(owner);
			}

			// Token: 0x0600249B RID: 9371 RVA: 0x000713F8 File Offset: 0x0006F5F8
			private void InitializeAbility()
			{
				if (this._currentAbility == null)
				{
					return;
				}
				if (!this._currentAbility.IsPostMortemAbility)
				{
					this._currentAbility.AddPreActivationCondition(new BaseAbility.ActivationCondition(this.AbilityPreActivationCondition));
					this._currentAbility.AddActivationCondition(new BaseAbility.ActivationCondition(this.AbilityActivationCondition));
				}
				if (this.reloadingTimeOverride > 0f)
				{
					this._currentAbility.ReloadingTime = this.reloadingTimeOverride;
				}
				if (this.isPostMortemAbility)
				{
					this._currentAbility.IsPostMortemAbility = true;
				}
				Ability ability = this._currentAbility as Ability;
				if (ability != null)
				{
					if (this.abilityOwner.AttackTargetsProvider != null)
					{
						ability.CollectAbilityTargets = false;
						ability.Range = this.abilityOwner.AttackTargetsProvider.Range;
					}
					ability.UsingTarget = Ability.Target.AllTargetsInRange;
				}
				EffectBasedAbility effectBasedAbility = this._currentAbility as EffectBasedAbility;
				if (effectBasedAbility != null)
				{
					for (int i = 0; i < effectBasedAbility.AbilityEffects.Length; i++)
					{
						this.amountBasedAbilityEffect = effectBasedAbility.AbilityEffects[i];
						if (this.amountBasedAbilityEffect != null && this.amountBasedAbilityEffect.Amount > 0f)
						{
							if (this.amountOverride > 0f)
							{
								this.amountBasedAbilityEffect.Amount = this.amountOverride;
							}
							this.initialAbilityEffectAmount = this.amountBasedAbilityEffect.Amount;
							break;
						}
					}
					effectBasedAbility.EffectApplied += this.OnAbilityEffectApplied;
				}
				MobAbilityParameters mobAbilityParameters;
				if (this._currentAbility.TryGetExtension(out mobAbilityParameters))
				{
					mobAbilityParameters.TryPassAllowedTargetsFilter(ref this.additionalTargetsFilter);
				}
				if (!this.abilityOwner.AttackTargetsProvider.IsNull())
				{
					GameMobAreaObserver attackTargetsProvider = this.abilityOwner.AttackTargetsProvider;
					attackTargetsProvider.observableLayers |= this._currentAbility.ValidObjectLayers;
				}
				this._currentAbility.BeforeUsed += this.OnBeforeAbilityUsed;
				this._currentAbility.Used += this.OnAbilityUsed;
				this._currentAbility.Destroyed += this.OnAbilityDestroyed;
			}

			// Token: 0x0600249C RID: 9372 RVA: 0x000715F8 File Offset: 0x0006F7F8
			private void ResetAmountBasedEffect()
			{
				if (!this.amountBasedAbilityEffect.IsNull())
				{
					if (this.abilityOwner.StatsController != null && this.amountBasedAbilityEffect is IDamageSender)
					{
						int num = 2;
						MobProxyStat mobProxyStat = this.abilityOwner.StatsController.GetStat(num) as MobProxyStat;
						if (mobProxyStat != null)
						{
							IReadOnlyList<IModifiableStat<MobStatModifier>> targetStats = mobProxyStat.TargetStats;
							for (int i = 0; i < targetStats.Count; i++)
							{
								IModifiableStat<MobStatModifier> modifiableStat = targetStats[i];
								if (modifiableStat.ID == num && modifiableStat.Owner == this._currentAbility)
								{
									float initialValue = modifiableStat.InitialValue;
									this.amountBasedAbilityEffect.Amount = mobProxyStat.AppliedModifiers.GetModifiedStatValue(initialValue);
									return;
								}
							}
						}
					}
					this.amountBasedAbilityEffect.Amount = this.initialAbilityEffectAmount;
				}
			}

			// Token: 0x0600249D RID: 9373 RVA: 0x000716C0 File Offset: 0x0006F8C0
			private bool CanBeActivated()
			{
				if (!this.abilityOwner.TryToUseAbilitiesConstantly)
				{
					if (this.affectedTargets == StaticGameMob.AbilityDecorator.AffectedTargets.SelectedTarget)
					{
						return this.IsValidAbilityTarget(this.abilityOwner.SelectedTarget);
					}
					if (this.affectedTargets == StaticGameMob.AbilityDecorator.AffectedTargets.PlayerOnly)
					{
						return !this.abilityOwner.PlayerInRange.IsNull();
					}
				}
				return true;
			}

			// Token: 0x0600249E RID: 9374 RVA: 0x00071713 File Offset: 0x0006F913
			private bool AbilityPreActivationCondition(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
			{
				return this.abilityOwner.IsAccessible && !this.IsExhausted && this.CanBeActivated();
			}

			// Token: 0x0600249F RID: 9375 RVA: 0x00071732 File Offset: 0x0006F932
			private bool AbilityActivationCondition(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
			{
				return !this.isAimingRequired || this.abilityOwner.IsAimed;
			}

			// Token: 0x060024A0 RID: 9376 RVA: 0x0007174C File Offset: 0x0006F94C
			private void OnBeforeAbilityUsed(IAbility ability, object usingArgs)
			{
				BaseAbility.UsingArgs usingArgs2 = usingArgs as BaseAbility.UsingArgs;
				this.abilityEffectAmountSum = 0;
				if (this.countResourcePerObject && this.amountBasedAbilityEffect != null && this.HasLimitedResource)
				{
					int abilityEffectResourcesCost = StaticGameMob.AbilityDecorator.GetAbilityEffectResourcesCost(this.amountBasedAbilityEffect.Amount);
					if (this.currentResourceAmount < abilityEffectResourcesCost)
					{
						this.amountBasedAbilityEffect.Amount = (float)this.currentResourceAmount;
					}
				}
				switch (this.affectedTargets)
				{
				case StaticGameMob.AbilityDecorator.AffectedTargets.AllInRange:
				{
					IReadOnlyList<BaseGameMob> targetsInRange = this.abilityOwner.TargetsInRange;
					if (targetsInRange != null)
					{
						int count = targetsInRange.Count;
						int num = 0;
						PlayerBehaviour playerInRange = this.abilityOwner.PlayerInRange;
						for (int i = 0; i < count; i++)
						{
							BaseGameMob baseGameMob = targetsInRange[i];
							if (this.IsValidAbilityTarget(baseGameMob) && (this.forceIncludePlayerAsTarget || playerInRange != baseGameMob))
							{
								StaticGameMob.AbilityDecorator.FilteredTargetsBuffer[num] = baseGameMob;
								num++;
							}
						}
						usingArgs2.Reset();
						if (num != 0)
						{
							usingArgs2.targetsList = StaticGameMob.AbilityDecorator.FilteredTargetsBuffer;
							usingArgs2.targetsCountOverride = num;
						}
						usingArgs2.targetPosition = this.abilityOwner.Position;
					}
					break;
				}
				case StaticGameMob.AbilityDecorator.AffectedTargets.SelectedTarget:
					usingArgs2.Reset();
					usingArgs2.targetObject = this.abilityOwner.SelectedTarget;
					if (this.abilityOwner.SelectedTarget != null)
					{
						usingArgs2.targetPosition = this.abilityOwner.SelectedTarget.HitColliderCenter;
						return;
					}
					break;
				case StaticGameMob.AbilityDecorator.AffectedTargets.PlayerOnly:
					usingArgs2.Reset();
					usingArgs2.targetObject = this.abilityOwner.PlayerInRange;
					if (this.abilityOwner.PlayerInRange != null)
					{
						usingArgs2.targetPosition = this.abilityOwner.PlayerInRange.HitColliderCenter;
						return;
					}
					break;
				default:
					return;
				}
			}

			// Token: 0x060024A1 RID: 9377 RVA: 0x000718FC File Offset: 0x0006FAFC
			private void OnAbilityEffectApplied(AbilityEffectBase effect, Component obj, float usedEffectAmount)
			{
				if (!this.HasLimitedResource || usedEffectAmount <= 0f)
				{
					return;
				}
				if (this.countResourcePerObject && this.amountBasedAbilityEffect != null && effect == this.amountBasedAbilityEffect)
				{
					this.abilityEffectAmountSum += StaticGameMob.AbilityDecorator.GetAbilityEffectResourcesCost(usedEffectAmount);
					int num = this.currentResourceAmount - this.abilityEffectAmountSum;
					int abilityEffectResourcesCost = StaticGameMob.AbilityDecorator.GetAbilityEffectResourcesCost(this.amountBasedAbilityEffect.Amount);
					if (num < abilityEffectResourcesCost)
					{
						this.amountBasedAbilityEffect.Amount = (float)Mathf.Max(num, 0);
						return;
					}
				}
				else
				{
					this.abilityEffectAmountSum = 1;
				}
			}

			// Token: 0x060024A2 RID: 9378 RVA: 0x00071984 File Offset: 0x0006FB84
			private void OnAbilityUsed(IAbility ability, object usingArgs)
			{
				if (this.HasLimitedResource)
				{
					this.currentResourceAmount -= this.abilityEffectAmountSum;
					if (this.currentResourceAmount < 0)
					{
						this.currentResourceAmount = 0;
					}
				}
				this.ResetAmountBasedEffect();
				if (this.IsExhausted)
				{
					this.abilityOwner.exhaustedAbilitiesCount++;
				}
			}

			// Token: 0x060024A3 RID: 9379 RVA: 0x000719E0 File Offset: 0x0006FBE0
			private void OnAbilityDestroyed(object ability)
			{
				this._currentAbility.BeforeUsed -= this.OnBeforeAbilityUsed;
				this._currentAbility.Used -= this.OnAbilityUsed;
				this._currentAbility.Destroyed -= this.OnAbilityDestroyed;
				EffectBasedAbility effectBasedAbility = this._currentAbility as EffectBasedAbility;
				if (effectBasedAbility != null)
				{
					effectBasedAbility.EffectApplied -= this.OnAbilityEffectApplied;
				}
				this.ResetAmountBasedEffect();
			}

			// Token: 0x060024A4 RID: 9380 RVA: 0x00071A5C File Offset: 0x0006FC5C
			public bool IsValidAbilityTarget(BaseGameMob target)
			{
				if (target.IsNull() || (this.additionalTargetsFilter != null && !this.additionalTargetsFilter(target)))
				{
					return false;
				}
				if (this.targetMobID == MobBehaviour.ID.None)
				{
					return this.targetMobsFaction == GameMobFactions.None || target.Faction == this.targetMobsFaction;
				}
				MobBehaviour mobBehaviour = target as MobBehaviour;
				MobBehaviour.ID? id = (mobBehaviour != null) ? new MobBehaviour.ID?(mobBehaviour.ObjectID) : null;
				MobBehaviour.ID id2 = this.targetMobID;
				return id.GetValueOrDefault() == id2 & id != null;
			}

			// Token: 0x060024A5 RID: 9381 RVA: 0x00071AE4 File Offset: 0x0006FCE4
			public void Initialize(StaticGameMob abilityOwner)
			{
				if (abilityOwner.IsNull())
				{
					return;
				}
				AbilityFactoryArgs query = new AbilityFactoryArgs
				{
					abilityPrototype = this.targetAbility,
					abilityID = this.targetAbilityID
				};
				IGameAbilitiesFactory gameAbilitiesFactory = abilityOwner.CurrentGame.Services.Get<IGameAbilitiesFactory>();
				this._currentAbility = (BaseAbility)((gameAbilitiesFactory != null) ? gameAbilitiesFactory.Create(query) : null);
				this.currentResourceAmount = Mathf.Max(0, this.maxResourceCapacity);
				this.abilityOwner = abilityOwner;
				this.InitializeAbility();
			}

			// Token: 0x0400191D RID: 6429
			private static readonly BaseGameMob[] FilteredTargetsBuffer = new BaseGameMob[100];

			// Token: 0x0400191E RID: 6430
			[Tooltip("Прямая ссылка на объект способности. Если не задана, то абилити будет создана по указанному айди.")]
			public BaseAbility targetAbility;

			// Token: 0x0400191F RID: 6431
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			public AbilityID targetAbilityID;

			// Token: 0x04001920 RID: 6432
			[Space(5f)]
			[Tooltip("На какие цели распространяется действие способности.")]
			public StaticGameMob.AbilityDecorator.AffectedTargets affectedTargets;

			// Token: 0x04001921 RID: 6433
			[Tooltip("Если активно, то игрок не будет исключаться из общего списка целей абилити.")]
			public bool forceIncludePlayerAsTarget;

			// Token: 0x04001922 RID: 6434
			[Tooltip("Если задано, то способность будет применяться только к мобам данной фракции.")]
			public GameMobFactions targetMobsFaction;

			// Token: 0x04001923 RID: 6435
			[Tooltip("Если задано, то способность будет применяться только к мобам данного типа.")]
			public MobBehaviour.ID targetMobID;

			// Token: 0x04001924 RID: 6436
			[Space(5f)]
			[FormerlySerializedAs("usingDelayOverride")]
			[Tooltip("При значениях > 0 данный параметр будет использоваться как время перезарядки способности.")]
			public float reloadingTimeOverride;

			// Token: 0x04001925 RID: 6437
			[Tooltip("При значениях > 0 данный параметр будет использоваться как количество \"эффекта\" способности. Например, значение урона.")]
			public float amountOverride;

			// Token: 0x04001926 RID: 6438
			[Tooltip("Ресурс использования способности. Если <= 0, то способность будет использоваться бесконечно.")]
			public int maxResourceCapacity;

			// Token: 0x04001927 RID: 6439
			[Tooltip("Если активировано, то ресурс будет потребляться исходя из количества, нанесенного способностью эффекта.")]
			public bool countResourcePerObject;

			// Token: 0x04001928 RID: 6440
			[Space(5f)]
			[Tooltip("Необходимо ли наведение на цель для использования способности.")]
			public bool isAimingRequired = true;

			// Token: 0x04001929 RID: 6441
			[FormerlySerializedAs("activateOnlyWhenOwnerDied")]
			[Tooltip("Способность будет активирована только когда ее носитель будет уничтожен.")]
			public bool isPostMortemAbility;

			// Token: 0x0400192A RID: 6442
			[NonSerialized]
			private StaticGameMob abilityOwner;

			// Token: 0x0400192B RID: 6443
			[NonSerialized]
			private BaseAbility _currentAbility;

			// Token: 0x0400192C RID: 6444
			private IAmountBased amountBasedAbilityEffect;

			// Token: 0x0400192D RID: 6445
			private float initialAbilityEffectAmount;

			// Token: 0x0400192E RID: 6446
			private int currentResourceAmount;

			// Token: 0x0400192F RID: 6447
			private int abilityEffectAmountSum;

			// Token: 0x04001930 RID: 6448
			private Predicate<Component> additionalTargetsFilter;

			// Token: 0x020005B4 RID: 1460
			public enum AffectedTargets
			{
				// Token: 0x04001D34 RID: 7476
				AllInRange,
				// Token: 0x04001D35 RID: 7477
				SelectedTarget,
				// Token: 0x04001D36 RID: 7478
				PlayerOnly
			}
		}
	}
}
