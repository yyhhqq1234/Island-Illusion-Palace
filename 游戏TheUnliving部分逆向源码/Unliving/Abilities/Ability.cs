using System;
using System.Collections.Generic;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.LeveledItems;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x02000367 RID: 871
	[CreateAssetMenu(fileName = "Ability", menuName = "Abilities/Generic Ability")]
	public sealed class Ability : EffectBasedAbility, ICustomUsingArgsAbility, IAbility, IDestroyable, ILeveledItem, IItemLevelProvider, IMobStatsListProvider, IStatsListProvider<MobStatModifier>, ITempMobStatsModifiersReceiver, ITempStatsModifiersReceiver<TargetedMobStatModifier>, IInterruptableAction, ITypedMobActivationAbility, IGameMobJumpMotion
	{
		// Token: 0x170005E4 RID: 1508
		// (get) Token: 0x06001C7D RID: 7293 RVA: 0x00059D73 File Offset: 0x00057F73
		// (set) Token: 0x06001C7E RID: 7294 RVA: 0x00059D7B File Offset: 0x00057F7B
		public override int ID { get; set; }

		// Token: 0x170005E5 RID: 1509
		// (get) Token: 0x06001C7F RID: 7295 RVA: 0x00059D84 File Offset: 0x00057F84
		// (set) Token: 0x06001C80 RID: 7296 RVA: 0x00059D8C File Offset: 0x00057F8C
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

		// Token: 0x170005E6 RID: 1510
		// (get) Token: 0x06001C81 RID: 7297 RVA: 0x00059D95 File Offset: 0x00057F95
		// (set) Token: 0x06001C82 RID: 7298 RVA: 0x00059D9D File Offset: 0x00057F9D
		public Ability.Target UsingTarget
		{
			get
			{
				return this._usingTarget;
			}
			set
			{
				this._usingTarget = value;
			}
		}

		// Token: 0x170005E7 RID: 1511
		// (get) Token: 0x06001C83 RID: 7299 RVA: 0x00059DA6 File Offset: 0x00057FA6
		// (set) Token: 0x06001C84 RID: 7300 RVA: 0x00059DAE File Offset: 0x00057FAE
		public int MaxTargetsInRange
		{
			get
			{
				return this.maxTargetsInRange;
			}
			set
			{
				this.maxTargetsInRange = value;
			}
		}

		// Token: 0x170005E8 RID: 1512
		// (get) Token: 0x06001C85 RID: 7301 RVA: 0x00059DB7 File Offset: 0x00057FB7
		// (set) Token: 0x06001C86 RID: 7302 RVA: 0x00059DCD File Offset: 0x00057FCD
		public float AbilityTargetsSelectionAngle
		{
			get
			{
				if (this._usingTarget != Ability.Target.AllTargetsInRange)
				{
					return 0f;
				}
				return this._abilityTargetsSelectionAngle;
			}
			set
			{
				this._abilityTargetsSelectionAngle = Mathf.Clamp(value, 0f, 180f);
			}
		}

		// Token: 0x170005E9 RID: 1513
		// (get) Token: 0x06001C87 RID: 7303 RVA: 0x00059DE8 File Offset: 0x00057FE8
		// (set) Token: 0x06001C88 RID: 7304 RVA: 0x00059DFD File Offset: 0x00057FFD
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

		// Token: 0x170005EA RID: 1514
		// (get) Token: 0x06001C89 RID: 7305 RVA: 0x00059E0B File Offset: 0x0005800B
		public override bool IsTargetedAbility
		{
			get
			{
				return this._usingTarget == Ability.Target.Object || this._usingTarget == Ability.Target.Point || this._usingTarget == Ability.Target.WorldCursorPosition;
			}
		}

		// Token: 0x170005EB RID: 1515
		// (get) Token: 0x06001C8A RID: 7306 RVA: 0x00059E2A File Offset: 0x0005802A
		public override bool IsObjectTargetRequired
		{
			get
			{
				return this._usingTarget == Ability.Target.Object;
			}
		}

		// Token: 0x170005EC RID: 1516
		// (get) Token: 0x06001C8B RID: 7307 RVA: 0x00059E35 File Offset: 0x00058035
		public override bool IsZoneEffectAbility
		{
			get
			{
				return this._isZoneEffectAbility;
			}
		}

		// Token: 0x170005ED RID: 1517
		// (get) Token: 0x06001C8C RID: 7308 RVA: 0x00059E3D File Offset: 0x0005803D
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return !this.excludeAbilityOwner || this._usingTarget == Ability.Target.AbilityOwner;
			}
		}

		// Token: 0x170005EE RID: 1518
		// (get) Token: 0x06001C8D RID: 7309 RVA: 0x00059E52 File Offset: 0x00058052
		public bool CollectTargetsInSector
		{
			get
			{
				return this._abilityTargetsSelectionAngle > 0f;
			}
		}

		// Token: 0x170005EF RID: 1519
		// (get) Token: 0x06001C8E RID: 7310 RVA: 0x00059E61 File Offset: 0x00058061
		public bool HasWaveEffect
		{
			get
			{
				return this.useWaveEffectPropagation && !this._isZoneEffectAbility && this.HasUsingDuration() && this._usingTarget == Ability.Target.AllTargetsInRange;
			}
		}

		// Token: 0x170005F0 RID: 1520
		// (get) Token: 0x06001C8F RID: 7311 RVA: 0x00059E86 File Offset: 0x00058086
		// (set) Token: 0x06001C90 RID: 7312 RVA: 0x00059E8E File Offset: 0x0005808E
		public int ItemLevel { get; set; }

		// Token: 0x170005F1 RID: 1521
		// (get) Token: 0x06001C91 RID: 7313 RVA: 0x00059E97 File Offset: 0x00058097
		BaseAbility.UsingArgs ICustomUsingArgsAbility.SourceUsingArgs
		{
			get
			{
				if (base.PrepProgress <= 0f)
				{
					return null;
				}
				return this.originalUsingArgs;
			}
		}

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x06001C92 RID: 7314 RVA: 0x00059EAE File Offset: 0x000580AE
		BaseAbility.UsingArgs ICustomUsingArgsAbility.CurrentUsingArgs
		{
			get
			{
				if (base.PrepProgress <= 0f)
				{
					return null;
				}
				return this.currentUsingArgs;
			}
		}

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x06001C93 RID: 7315 RVA: 0x00059EC5 File Offset: 0x000580C5
		IReadOnlyList<IModifiableStat<MobStatModifier>> IStatsListProvider<MobStatModifier>.Stats
		{
			get
			{
				return this.statsListProvider.Stats;
			}
		}

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x06001C94 RID: 7316 RVA: 0x00059ED2 File Offset: 0x000580D2
		MobActivationAbilityType ITypedMobActivationAbility.ActivationAbilityType
		{
			get
			{
				if (!this._isPostMortemAbility)
				{
					return MobActivationAbilityType.None;
				}
				return this.mobActivationType;
			}
		}

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x06001C95 RID: 7317 RVA: 0x00059EE4 File Offset: 0x000580E4
		float IGameMobJumpMotion.MaxHeight
		{
			get
			{
				return this.maxJumpHeight;
			}
		}

		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x06001C96 RID: 7318 RVA: 0x00059EEC File Offset: 0x000580EC
		float IGameMobJumpMotion.MaxDistance
		{
			get
			{
				return this.maxJumpDistance;
			}
		}

		// Token: 0x06001C97 RID: 7319 RVA: 0x00059EF4 File Offset: 0x000580F4
		private bool IsValidTarget(Component target)
		{
			return !target.IsNull() && target.InLayerMask(this._validObjectLayers);
		}

		// Token: 0x06001C98 RID: 7320 RVA: 0x00059F14 File Offset: 0x00058114
		private bool IsTargetInRange(Component target)
		{
			float range = base.Range;
			if (range <= 0f)
			{
				return true;
			}
			Collider2D collider2D = target.CastOrGetComponent<Collider2D>();
			Vector2 b = base.OwnerPosition;
			if (collider2D != null && collider2D.enabled)
			{
				Bounds bounds = collider2D.bounds;
				Vector2 vector = bounds.center;
				float num = (bounds.max - vector).SqrMagnitude();
				return (vector - b).SqrMagnitude() < range * range + num;
			}
			return (target.transform.position - base.OwnerPosition).sqrMagnitude < range * range;
		}

		// Token: 0x06001C99 RID: 7321 RVA: 0x00059FC7 File Offset: 0x000581C7
		private void PassCursorPosition(BaseAbility.UsingArgs usingArgs)
		{
			usingArgs.Reset();
			usingArgs.PassWorldCursorPosition(this);
		}

		// Token: 0x06001C9A RID: 7322 RVA: 0x00059FD8 File Offset: 0x000581D8
		private bool PrepareActualUsingArgs(BaseAbility.UsingArgs args)
		{
			switch (this.UsingTarget)
			{
			case Ability.Target.AllTargetsInRange:
			case Ability.Target.AbilityOwnerSquad:
				args.targetObject = null;
				args.targetPosition = base.OwnerPosition;
				return true;
			case Ability.Target.AbilityOwner:
				args.PrepareForUsingOnOwner(this);
				return true;
			case Ability.Target.WorldCursorPosition:
				this.PassCursorPosition(args);
				return true;
			}
			return false;
		}

		// Token: 0x06001C9B RID: 7323 RVA: 0x0005A034 File Offset: 0x00058234
		protected override BaseAbility.ActivationErrorType GetActivationError(BaseAbility.UsingArgs usingArgs, bool isAutoUsePhase, ref object errorData)
		{
			switch (this.UsingTarget)
			{
			case Ability.Target.Object:
				if (!this.skipAbilityTargetLayersValidation && (!this.IsValidTarget(usingArgs.targetObject) || (this.checkAbilityRangeForSingleTarget && !this.IsTargetInRange(usingArgs.targetObject))))
				{
					return BaseAbility.ActivationErrorType.UnallowedTarget;
				}
				break;
			case Ability.Target.AbilityOwner:
				if (base.Owner == null)
				{
					return BaseAbility.ActivationErrorType.UnallowedTarget;
				}
				break;
			case Ability.Target.AbilityOwnerSquad:
			{
				BaseGameMob baseGameMob = base.Owner as BaseGameMob;
				if (baseGameMob == null || baseGameMob.Group == null)
				{
					return BaseAbility.ActivationErrorType.Internal;
				}
				break;
			}
			}
			return base.GetActivationError(usingArgs, isAutoUsePhase, ref errorData);
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x0005A0B8 File Offset: 0x000582B8
		protected override BaseAbility.UsingArgs GetActualUsingArgs(BaseAbility.UsingArgs currentUsingArgs, bool isAutoUseArgs)
		{
			this.originalUsingArgs = currentUsingArgs;
			currentUsingArgs.CopyValuesTo(this.usingArgsOverride);
			if (!this.PrepareActualUsingArgs(this.usingArgsOverride))
			{
				return currentUsingArgs;
			}
			return this.usingArgsOverride;
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x0005A0E4 File Offset: 0x000582E4
		protected override void SetAbilityTargets(BaseAbility.UsingArgs usingArgs)
		{
			IGameMob gameMob = (IGameMob)base.Owner;
			this.targetsValidator.Update(this.originalUsingArgs, usingArgs);
			if (this.UsingTarget == Ability.Target.AbilityOwnerSquad)
			{
				GameMobsGroupControllerBase group = gameMob.Group;
				if (!group.IsNull() && group.Mobs != null)
				{
					BaseGameMob[] mobsQueriesBuffer = GameLocation.MobsQueriesBuffer;
					int num = group.CopyMobsTo(mobsQueriesBuffer, this.maxTargetsInRange, new Predicate<BaseGameMob>(this.targetsValidator.IsValidTarget));
					if (num > 0)
					{
						usingArgs.targetsList = mobsQueriesBuffer;
					}
					else
					{
						usingArgs.targetsList = Array.Empty<Component>();
					}
					usingArgs.targetsCountOverride = num;
					return;
				}
			}
			else if (base.Range > 0f && this.UsingTarget != Ability.Target.AbilityOwner && !this.IsObjectTargetRequired)
			{
				if (this.isWaveEffectInProgress)
				{
					base.OverrideAmountModifier(1f);
					usingArgs.usingRangeOverride = Mathf.Lerp(0.2f, base.Range, base.GetUsingProgress());
				}
				Ability.TargetsCollectionArgs.PrepareForTargetsCollection(this, usingArgs, this.sortAbilityTargets, this.maxTargetsInRange);
				Ability.TargetsCollectionArgs.filter = new Predicate<BaseGameMob>(this.targetsValidator.IsValidTarget);
				this.CollectTargets(Ability.TargetsCollectionArgs, usingArgs);
			}
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x0005A208 File Offset: 0x00058408
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			this.PrepareActualUsingArgs(usingArgs);
			if (this._isZoneEffectAbility && AbilitiesFactory.CanGenerateEffectZone(this))
			{
				MobStatModifier ownerTotalDamageModifier = this.GetOwnerTotalDamageModifier();
				AbilityEffectZone effectZoneObject;
				if ((effectZoneObject = AbilitiesFactory.CreateEffectZone(this, usingArgs, base.OwnerBehaviour, this.effectZoneLifetime, 0f, new MobStatModifier?(ownerTotalDamageModifier))) != null)
				{
					base.ApplyAbilityEffects(usingArgs, true, false);
					base.RaiseEffectZoneEvent(effectZoneObject);
					if (this.IsContinuous)
					{
						base.Complete();
					}
					return;
				}
			}
			base.ApplyAbilityEffects(usingArgs, true);
		}

		// Token: 0x06001C9F RID: 7327 RVA: 0x0005A283 File Offset: 0x00058483
		public void AddTempStatModifier(TargetedMobStatModifier statModifier)
		{
			if (this.tempStatsModifiersHandler == null)
			{
				this.tempStatsModifiersHandler = new TempMobStatsModifiersHandler<TargetedMobStatModifier>(this);
			}
			this.tempStatsModifiersHandler.AddStatModifier(statModifier);
		}

		// Token: 0x06001CA0 RID: 7328 RVA: 0x0005A2A5 File Offset: 0x000584A5
		bool IInterruptableAction.TryInterrupt(bool force)
		{
			if (base.IsActivated && (force || !this.isWaveEffectInProgress))
			{
				base.Complete();
				return true;
			}
			return false;
		}

		// Token: 0x06001CA1 RID: 7329 RVA: 0x0005A2C4 File Offset: 0x000584C4
		protected override void OnInitialize(object context)
		{
			base.OnInitialize(context);
			if (this._isZoneEffectAbility)
			{
				this.effectZoneLifetime = Mathf.Max(this.UsingDuration, 0.2f);
				this.UsingDuration = 0f;
			}
			this.targetsValidator = new Ability.TargetsValidator(this);
			this.statsListProvider = new AbilityStatsListGenerator(this);
			ValueTuple<float, float> maxJumpMotionEffectParams = this.GetMaxJumpMotionEffectParams();
			this.maxJumpHeight = maxJumpMotionEffectParams.Item1;
			this.maxJumpDistance = maxJumpMotionEffectParams.Item2;
		}

		// Token: 0x06001CA2 RID: 7330 RVA: 0x0005A338 File Offset: 0x00058538
		protected override void OnPrepared(BaseAbility.UsingArgs usingArgs)
		{
			base.OnPrepared(usingArgs);
			this.isWaveEffectInProgress = this.HasWaveEffect;
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler == null)
			{
				return;
			}
			tempMobStatsModifiersHandler.ApplyModifiers(null);
		}

		// Token: 0x06001CA3 RID: 7331 RVA: 0x0005A35E File Offset: 0x0005855E
		protected override void OnCompleted(BaseAbility.UsingArgs usingArgs)
		{
			this.isWaveEffectInProgress = false;
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler != null)
			{
				tempMobStatsModifiersHandler.RemoveAppliedModifiers();
			}
			base.OnCompleted(usingArgs);
		}

		// Token: 0x04001015 RID: 4117
		private static readonly GameLocation.MobsGatheringArgs TargetsCollectionArgs = new GameLocation.MobsGatheringArgs();

		// Token: 0x04001018 RID: 4120
		public AbilityTypes abilityType;

		// Token: 0x04001019 RID: 4121
		[SerializeField]
		private BuffsGeneratorBuilderAsset.Reference[] buffsGenerators;

		// Token: 0x0400101A RID: 4122
		[SerializeField]
		[FormerlySerializedAs("abilityTarget")]
		[FormerlySerializedAs("_abilityTarget")]
		[Tooltip("Вид цели для применения абилити.")]
		private Ability.Target _usingTarget;

		// Token: 0x0400101B RID: 4123
		public bool useWaveEffectPropagation;

		// Token: 0x0400101C RID: 4124
		public MobActivationAbilityType mobActivationType;

		// Token: 0x0400101D RID: 4125
		[Tooltip("Опция для проверки вхождения цели в радиус абилити в режиме AbilityTarget = Object.")]
		public bool checkAbilityRangeForSingleTarget;

		// Token: 0x0400101E RID: 4126
		[Tooltip("Если опция активна, то проверка на вхождение цели абилити в допустимые слои объектов происходить не будет.")]
		public bool skipAbilityTargetLayersValidation;

		// Token: 0x0400101F RID: 4127
		[SerializeField]
		[Range(0f, 180f)]
		private float _abilityTargetsSelectionAngle;

		// Token: 0x04001020 RID: 4128
		[SerializeField]
		[FormerlySerializedAs("isZoneEffectAbility")]
		[Tooltip("Является ли данная способность \"лужей\". Using Duration будет использоваться как время жизни зоны эффекта абилити.")]
		private bool _isZoneEffectAbility;

		// Token: 0x04001021 RID: 4129
		[SerializeField]
		[Tooltip("Максимальное количество целей в радиусе абилити. Если 0, то количество не будет ограничено. Значения больше 0 используются для оптимизации.")]
		private int maxTargetsInRange = 50;

		// Token: 0x04001022 RID: 4130
		[Tooltip("Если опция активна, то к носителю абилити не будет применен ее эффект. Применимо для абилити, работающих со списками целей.")]
		public bool excludeAbilityOwner;

		// Token: 0x04001023 RID: 4131
		private readonly BaseAbility.UsingArgs usingArgsOverride = new BaseAbility.UsingArgs();

		// Token: 0x04001024 RID: 4132
		private BaseAbility.UsingArgs originalUsingArgs;

		// Token: 0x04001025 RID: 4133
		private Ability.TargetsValidator targetsValidator;

		// Token: 0x04001026 RID: 4134
		private bool isWaveEffectInProgress;

		// Token: 0x04001027 RID: 4135
		private AbilityStatsListGenerator statsListProvider;

		// Token: 0x04001028 RID: 4136
		private float effectZoneLifetime;

		// Token: 0x04001029 RID: 4137
		private float maxJumpHeight;

		// Token: 0x0400102A RID: 4138
		private float maxJumpDistance;

		// Token: 0x0400102B RID: 4139
		private TempMobStatsModifiersHandler<TargetedMobStatModifier> tempStatsModifiersHandler;

		// Token: 0x02000562 RID: 1378
		public enum Target
		{
			// Token: 0x04001C06 RID: 7174
			AllTargetsInRange,
			// Token: 0x04001C07 RID: 7175
			Point,
			// Token: 0x04001C08 RID: 7176
			Object,
			// Token: 0x04001C09 RID: 7177
			AbilityOwner,
			// Token: 0x04001C0A RID: 7178
			AbilityOwnerSquad,
			// Token: 0x04001C0B RID: 7179
			WorldCursorPosition
		}

		// Token: 0x02000563 RID: 1379
		private sealed class TargetsValidator
		{
			// Token: 0x06002703 RID: 9987 RVA: 0x0007961F File Offset: 0x0007781F
			private bool IsValidForAdditionalFilter(BaseGameMob abilityTarget)
			{
				return this.additionalTargetsFilter == null || this.additionalTargetsFilter(abilityTarget);
			}

			// Token: 0x06002704 RID: 9988 RVA: 0x00079638 File Offset: 0x00077838
			private bool IsTargetInsideSearchingSector(BaseGameMob abilityTarget)
			{
				if (this.targetsSelectionAngleCosine < 0f)
				{
					return true;
				}
				Vector2 vector = abilityTarget.transform.position - this.abilityOwnerPosition;
				if (this.abilityUsingDirection.x * vector.x + this.abilityUsingDirection.y * vector.y < 0f)
				{
					return false;
				}
				vector.Normalize();
				return this.abilityUsingDirection.x * vector.x + this.abilityUsingDirection.y * vector.y >= this.targetsSelectionAngleCosine;
			}

			// Token: 0x06002705 RID: 9989 RVA: 0x000796D5 File Offset: 0x000778D5
			private bool IsAffectedByWaveEffect(BaseGameMob abilityTarget)
			{
				return !this.ability.isWaveEffectInProgress || this.waveEffectAffectedTargets.Add(abilityTarget.GetInstanceID());
			}

			// Token: 0x06002706 RID: 9990 RVA: 0x000796F7 File Offset: 0x000778F7
			public TargetsValidator(Ability ability)
			{
				this.ability = ability;
			}

			// Token: 0x06002707 RID: 9991 RVA: 0x00079708 File Offset: 0x00077908
			public void Update(BaseAbility.UsingArgs originalUsingArgs, BaseAbility.UsingArgs actualUsingArgs)
			{
				if (!this.ability.WasUsed)
				{
					this.ignorableAbilityOwner = (this.ability.excludeAbilityOwner ? this.ability.OwnerBehaviour : null);
					this.additionalTargetsFilter = actualUsingArgs.affectableTargetsFilter;
					if (this.ability.CollectTargetsInSector)
					{
						this.targetsSelectionAngleCosine = Mathf.Cos(this.ability.AbilityTargetsSelectionAngle * 0.5f * 0.017453292f);
					}
					else
					{
						this.targetsSelectionAngleCosine = -1f;
					}
					if (this.ability.HasWaveEffect)
					{
						if (this.waveEffectAffectedTargets == null)
						{
							this.waveEffectAffectedTargets = new HashSet<int>();
						}
						else
						{
							this.waveEffectAffectedTargets.Clear();
						}
					}
				}
				this.abilityOwnerPosition = this.ability.OwnerPosition;
				if (this.targetsSelectionAngleCosine > 0f)
				{
					Vector2 vector = originalUsingArgs.TryGetTargetPosition();
					if (vector == this.abilityOwnerPosition)
					{
						IGameMob gameMob = this.ability.FindAttackTarget();
						if (gameMob != null)
						{
							vector = gameMob.Position;
						}
					}
					this.abilityUsingDirection = vector - this.abilityOwnerPosition;
					this.abilityUsingDirection.Normalize();
					if (Debug.isDebugBuild)
					{
						Quaternion rotation = Quaternion.AngleAxis(this.ability.AbilityTargetsSelectionAngle * 0.5f, new Vector3
						{
							z = 1f
						});
						Debug.DrawRay(this.abilityOwnerPosition, rotation * this.abilityUsingDirection * this.ability.Range, Color.magenta, 5f);
						Debug.DrawRay(this.abilityOwnerPosition, Quaternion.Inverse(rotation) * this.abilityUsingDirection * this.ability.Range, Color.magenta, 5f);
					}
				}
			}

			// Token: 0x06002708 RID: 9992 RVA: 0x000798DF File Offset: 0x00077ADF
			public bool IsValidTarget(BaseGameMob abilityTarget)
			{
				return abilityTarget != this.ignorableAbilityOwner && this.IsValidForAdditionalFilter(abilityTarget) && this.IsTargetInsideSearchingSector(abilityTarget) && this.IsAffectedByWaveEffect(abilityTarget);
			}

			// Token: 0x04001C0C RID: 7180
			private readonly Ability ability;

			// Token: 0x04001C0D RID: 7181
			private Component ignorableAbilityOwner;

			// Token: 0x04001C0E RID: 7182
			private Vector2 abilityOwnerPosition;

			// Token: 0x04001C0F RID: 7183
			private Vector2 abilityUsingDirection;

			// Token: 0x04001C10 RID: 7184
			private float targetsSelectionAngleCosine;

			// Token: 0x04001C11 RID: 7185
			private Predicate<Component> additionalTargetsFilter;

			// Token: 0x04001C12 RID: 7186
			private HashSet<int> waveEffectAffectedTargets;
		}
	}
}
