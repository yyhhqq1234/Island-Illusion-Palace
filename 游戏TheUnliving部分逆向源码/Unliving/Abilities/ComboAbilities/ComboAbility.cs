using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Editor;
using Game.Abilities;
using Game.Buffs;
using Game.Damage;
using Game.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities.ComboAbilities.Triggers;
using Unliving.LeveledItems;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;

namespace Unliving.Abilities.ComboAbilities
{
	// Token: 0x020003D4 RID: 980
	[CreateAssetMenu(fileName = "ComboAbility", menuName = "Abilities/Combo Ability")]
	public sealed class ComboAbility : BaseAbility, ICompositeAbility, IAbility, IDestroyable, ITypedMobActivationAbility, ILeveledItem, IItemLevelProvider, IDamageSender, IMobStatsListProvider, IStatsListProvider<MobStatModifier>, ITempMobStatsModifiersReceiver, ITempStatsModifiersReceiver<TargetedMobStatModifier>, IInterruptableAction, IGameMobJumpMotion
	{
		// Token: 0x170006B5 RID: 1717
		// (get) Token: 0x0600212D RID: 8493 RVA: 0x00068133 File Offset: 0x00066333
		// (set) Token: 0x0600212E RID: 8494 RVA: 0x0006813B File Offset: 0x0006633B
		public override int ID { get; set; }

		// Token: 0x170006B6 RID: 1718
		// (get) Token: 0x0600212F RID: 8495 RVA: 0x00068144 File Offset: 0x00066344
		// (set) Token: 0x06002130 RID: 8496 RVA: 0x0006814C File Offset: 0x0006634C
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

		// Token: 0x170006B7 RID: 1719
		// (get) Token: 0x06002131 RID: 8497 RVA: 0x00068155 File Offset: 0x00066355
		// (set) Token: 0x06002132 RID: 8498 RVA: 0x0006815C File Offset: 0x0006635C
		public override float UsingDuration
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		// Token: 0x170006B8 RID: 1720
		// (get) Token: 0x06002133 RID: 8499 RVA: 0x0006815E File Offset: 0x0006635E
		// (set) Token: 0x06002134 RID: 8500 RVA: 0x00068166 File Offset: 0x00066366
		public ComboAbility.ChildAbility[] AbilitiesSequence
		{
			get
			{
				return this._abilitiesSequence;
			}
			set
			{
				this._abilitiesSequence = value;
			}
		}

		// Token: 0x170006B9 RID: 1721
		// (get) Token: 0x06002135 RID: 8501 RVA: 0x0006816F File Offset: 0x0006636F
		// (set) Token: 0x06002136 RID: 8502 RVA: 0x00068177 File Offset: 0x00066377
		public bool PassLevelToChildAbility
		{
			get
			{
				return this._passLevelToChildAbility;
			}
			set
			{
				this._passLevelToChildAbility = value;
			}
		}

		// Token: 0x170006BA RID: 1722
		// (get) Token: 0x06002137 RID: 8503 RVA: 0x00068180 File Offset: 0x00066380
		// (set) Token: 0x06002138 RID: 8504 RVA: 0x00068188 File Offset: 0x00066388
		public bool UseChildAbilityRange
		{
			get
			{
				return this._useChildAbilityRange;
			}
			set
			{
				this._useChildAbilityRange = value;
			}
		}

		// Token: 0x170006BB RID: 1723
		// (get) Token: 0x06002139 RID: 8505 RVA: 0x00068191 File Offset: 0x00066391
		// (set) Token: 0x0600213A RID: 8506 RVA: 0x00068199 File Offset: 0x00066399
		public bool UseChildAbilityReloadingTime
		{
			get
			{
				return this._useChildAbilityReloadingTime;
			}
			set
			{
				this._useChildAbilityReloadingTime = value;
			}
		}

		// Token: 0x170006BC RID: 1724
		// (get) Token: 0x0600213B RID: 8507 RVA: 0x000681A2 File Offset: 0x000663A2
		// (set) Token: 0x0600213C RID: 8508 RVA: 0x000681AA File Offset: 0x000663AA
		public bool UseChildAbilityPrepTime
		{
			get
			{
				return this._useChildAbilityPrepTime;
			}
			set
			{
				this._useChildAbilityPrepTime = value;
			}
		}

		// Token: 0x170006BD RID: 1725
		// (get) Token: 0x0600213D RID: 8509 RVA: 0x000681B3 File Offset: 0x000663B3
		// (set) Token: 0x0600213E RID: 8510 RVA: 0x000681BB File Offset: 0x000663BB
		public bool KeepChildAbilityUsingLayers
		{
			get
			{
				return this._keepChildAbilityUsingLayers;
			}
			set
			{
				this._keepChildAbilityUsingLayers = value;
			}
		}

		// Token: 0x170006BE RID: 1726
		// (get) Token: 0x0600213F RID: 8511 RVA: 0x000681C4 File Offset: 0x000663C4
		// (set) Token: 0x06002140 RID: 8512 RVA: 0x000681CC File Offset: 0x000663CC
		public float MaxNextUsingTimeError
		{
			get
			{
				return this._maxNextUsingTimeError;
			}
			set
			{
				this._maxNextUsingTimeError = value;
			}
		}

		// Token: 0x170006BF RID: 1727
		// (get) Token: 0x06002141 RID: 8513 RVA: 0x000681D5 File Offset: 0x000663D5
		// (set) Token: 0x06002142 RID: 8514 RVA: 0x000681DD File Offset: 0x000663DD
		DamageGenerator IDamageSender.DamageGenerator
		{
			get
			{
				return this.damageGenerator;
			}
			set
			{
			}
		}

		// Token: 0x170006C0 RID: 1728
		// (get) Token: 0x06002143 RID: 8515 RVA: 0x000681DF File Offset: 0x000663DF
		public override bool IsTargetedAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170006C1 RID: 1729
		// (get) Token: 0x06002144 RID: 8516 RVA: 0x000681E2 File Offset: 0x000663E2
		public override bool IsObjectTargetRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170006C2 RID: 1730
		// (get) Token: 0x06002145 RID: 8517 RVA: 0x000681E5 File Offset: 0x000663E5
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return this.canBeUsedOnOwner;
			}
		}

		// Token: 0x170006C3 RID: 1731
		// (get) Token: 0x06002146 RID: 8518 RVA: 0x000681ED File Offset: 0x000663ED
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170006C4 RID: 1732
		// (get) Token: 0x06002147 RID: 8519 RVA: 0x000681F0 File Offset: 0x000663F0
		public override bool CanBeUsed
		{
			get
			{
				return this.HasAbilitiesSequence();
			}
		}

		// Token: 0x170006C5 RID: 1733
		// (get) Token: 0x06002148 RID: 8520 RVA: 0x000681F8 File Offset: 0x000663F8
		public override bool IsContinuous
		{
			get
			{
				return this.isCoupledCombo && !this.useAllAbilitiesAtTheSameTime;
			}
		}

		// Token: 0x170006C6 RID: 1734
		// (get) Token: 0x06002149 RID: 8521 RVA: 0x0006820D File Offset: 0x0006640D
		public override bool InUse
		{
			get
			{
				return this.coupledUsingCoroutine != null;
			}
		}

		// Token: 0x170006C7 RID: 1735
		// (get) Token: 0x0600214A RID: 8522 RVA: 0x00068218 File Offset: 0x00066418
		public int SelectedChildAbilityIndex
		{
			get
			{
				return this.selectedChildAbilityIndex;
			}
		}

		// Token: 0x170006C8 RID: 1736
		// (get) Token: 0x0600214B RID: 8523 RVA: 0x00068220 File Offset: 0x00066420
		// (set) Token: 0x0600214C RID: 8524 RVA: 0x00068228 File Offset: 0x00066428
		public int ItemLevel { get; set; }

		// Token: 0x170006C9 RID: 1737
		// (get) Token: 0x0600214D RID: 8525 RVA: 0x00068231 File Offset: 0x00066431
		IList<IAbility> ICompositeAbility.ChildAbilities
		{
			get
			{
				return this.childAbilitiesImpl ?? this._abilitiesSequence.InitializeChildAbilitiesAsPrototypes(ref this.childAbilitiesImpl);
			}
		}

		// Token: 0x170006CA RID: 1738
		// (get) Token: 0x0600214E RID: 8526 RVA: 0x0006824E File Offset: 0x0006644E
		bool ICompositeAbility.WillUseChildAbilitiesSequentially
		{
			get
			{
				return !this.useAllAbilitiesAtTheSameTime;
			}
		}

		// Token: 0x170006CB RID: 1739
		// (get) Token: 0x0600214F RID: 8527 RVA: 0x00068259 File Offset: 0x00066459
		IReadOnlyList<IModifiableStat<MobStatModifier>> IStatsListProvider<MobStatModifier>.Stats
		{
			get
			{
				return this.totalAbilityStatsList;
			}
		}

		// Token: 0x170006CC RID: 1740
		// (get) Token: 0x06002150 RID: 8528 RVA: 0x00068261 File Offset: 0x00066461
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

		// Token: 0x170006CD RID: 1741
		// (get) Token: 0x06002151 RID: 8529 RVA: 0x00068273 File Offset: 0x00066473
		float IGameMobJumpMotion.MaxHeight
		{
			get
			{
				return this.maxChildAbilityJumpHeight;
			}
		}

		// Token: 0x170006CE RID: 1742
		// (get) Token: 0x06002152 RID: 8530 RVA: 0x0006827B File Offset: 0x0006647B
		float IGameMobJumpMotion.MaxDistance
		{
			get
			{
				return this.maxChildAbilityJumpDistance;
			}
		}

		// Token: 0x14000123 RID: 291
		// (add) Token: 0x06002153 RID: 8531 RVA: 0x00068284 File Offset: 0x00066484
		// (remove) Token: 0x06002154 RID: 8532 RVA: 0x000682BC File Offset: 0x000664BC
		public event Action<ComboAbility> ComboResetted;

		// Token: 0x14000124 RID: 292
		// (add) Token: 0x06002155 RID: 8533 RVA: 0x000682F4 File Offset: 0x000664F4
		// (remove) Token: 0x06002156 RID: 8534 RVA: 0x0006832C File Offset: 0x0006652C
		public event Action<ComboAbility.ChildAbility> ChildAbilitySelected;

		// Token: 0x14000125 RID: 293
		// (add) Token: 0x06002157 RID: 8535 RVA: 0x00068364 File Offset: 0x00066564
		// (remove) Token: 0x06002158 RID: 8536 RVA: 0x0006839C File Offset: 0x0006659C
		public event Action<ComboAbility.ChildAbility> ChildAbilityActivated;

		// Token: 0x14000126 RID: 294
		// (add) Token: 0x06002159 RID: 8537 RVA: 0x000683D4 File Offset: 0x000665D4
		// (remove) Token: 0x0600215A RID: 8538 RVA: 0x0006840C File Offset: 0x0006660C
		public event Action<ComboAbility.ChildAbility> ChildAbilityCompleted;

		// Token: 0x0600215B RID: 8539 RVA: 0x00068441 File Offset: 0x00066641
		private bool HasAbilitiesSequence()
		{
			return this._abilitiesSequence != null && this._abilitiesSequence.Length != 0;
		}

		// Token: 0x0600215C RID: 8540 RVA: 0x00068458 File Offset: 0x00066658
		private void TrySetDamageGenerator(BaseAbility childAbility)
		{
			if (this.damageGenerator == null || this.damageGenerator.amount <= 0f)
			{
				IDamageSender damageSender = childAbility as IDamageSender;
				if (damageSender != null)
				{
					this.damageGenerator = damageSender.DamageGenerator;
				}
			}
		}

		// Token: 0x0600215D RID: 8541 RVA: 0x00068498 File Offset: 0x00066698
		private void RegisterChildAbilityStats(BaseAbility ability)
		{
			IMobStatsListProvider mobStatsListProvider = ability as IMobStatsListProvider;
			if (mobStatsListProvider != null)
			{
				this.totalAbilityStatsList.AddRange(mobStatsListProvider.Stats);
			}
		}

		// Token: 0x0600215E RID: 8542 RVA: 0x000684C0 File Offset: 0x000666C0
		private int GetNextAbilityIndex(int abilityIndex)
		{
			if (!this.isCycledCombo)
			{
				return Mathf.Min(abilityIndex + 1, this._abilitiesSequence.Length - 1);
			}
			return (abilityIndex + 1) % this._abilitiesSequence.Length;
		}

		// Token: 0x0600215F RID: 8543 RVA: 0x000684E9 File Offset: 0x000666E9
		private void UpdateExpectedActivationTime()
		{
			this.expectedNextActivationTime = Time.realtimeSinceStartup + this._maxNextUsingTimeError;
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x000684FD File Offset: 0x000666FD
		private void ResetCoupledSequence()
		{
			if (this.coupledUsingCoroutine != null)
			{
				if (!this.useAllAbilitiesAtTheSameTime)
				{
					this.storedOwnerBehaviour.StopCoroutine(this.coupledUsingCoroutine);
				}
				this.coupledUsingCoroutine = null;
			}
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x00068528 File Offset: 0x00066728
		private void ResetCombo()
		{
			if (!this.isCoupledCombo && this.expectedNextActivationTime < 0f)
			{
				return;
			}
			this.ResetCoupledSequence();
			this.nextAbilityIndex = 0;
			this.selectedChildAbilityIndex = 0;
			this.expectedNextActivationTime = -1f;
			this.UpdateNextAbilityInfo(this.nextAbilityIndex);
			Action<ComboAbility> comboResetted = this.ComboResetted;
			if (comboResetted == null)
			{
				return;
			}
			comboResetted(this);
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x00068587 File Offset: 0x00066787
		private IEnumerator UsingSequenceRoutine(BaseAbility.UsingArgs usingArgs)
		{
			ComboAbility.UsingContext.Reset();
			BaseAbility.UsingArgs storedUsingArgs = usingArgs.Clone();
			bool anyTriggerWasFired = false;
			int num;
			for (int i = 0; i < this._abilitiesSequence.Length; i = num + 1)
			{
				ComboAbility.ChildAbility childAbility = this._abilitiesSequence[i];
				if (!(childAbility.AbilityInstance == null))
				{
					BaseAbility.UsingArgs usingArgs2 = storedUsingArgs.Clone();
					if (childAbility.tryUpdateUsingPoint)
					{
						usingArgs2.TryUpdateTargetPosition();
					}
					if (childAbility.Trigger != null)
					{
						ComboAbility.UsingContext.childAbility = childAbility;
						ComboAbility.UsingContext.anyChildAbilityTriggerWasFired = anyTriggerWasFired;
						ComboAbility.UsingContext.usingArgs = usingArgs2;
						if (!childAbility.Trigger.IsFired(ComboAbility.UsingContext))
						{
							goto IL_152;
						}
						anyTriggerWasFired = true;
					}
					childAbility.Use(usingArgs2);
					while (childAbility.InProgress())
					{
						yield return null;
					}
					if (childAbility.TryInterruptProgressBasedAbility())
					{
						yield return null;
					}
					childAbility = null;
				}
				IL_152:
				num = i;
			}
			this.coupledUsingCoroutine = null;
			yield break;
		}

		// Token: 0x06002163 RID: 8547 RVA: 0x0006859D File Offset: 0x0006679D
		protected override BaseAbility.ActivationErrorType GetActivationError(BaseAbility.UsingArgs usingArgs, bool isAutoUsePhase, ref object errorData)
		{
			if (!this._abilitiesSequence[this.nextAbilityIndex].CanBeActivated(usingArgs))
			{
				return BaseAbility.ActivationErrorType.NotReady;
			}
			return base.GetActivationError(usingArgs, isAutoUsePhase, ref errorData);
		}

		// Token: 0x06002164 RID: 8548 RVA: 0x000685BF File Offset: 0x000667BF
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			this.childAbilityWasUsed = false;
			if (this.isCoupledCombo)
			{
				this.coupledUsingCoroutine = this.storedOwnerBehaviour.StartCoroutine(this.UsingSequenceRoutine(usingArgs));
			}
			else
			{
				this.selectedChildAbility.Use(usingArgs);
			}
			base.SetFullyUsed();
		}

		// Token: 0x06002165 RID: 8549 RVA: 0x000685FC File Offset: 0x000667FC
		private void UpdateNextAbilityInfo(int abilityIndexOverride = -1)
		{
			if (this.isCoupledCombo)
			{
				return;
			}
			this.nextAbilityIndex = ((abilityIndexOverride >= 0) ? abilityIndexOverride : this.GetNextAbilityIndex(this.selectedChildAbilityIndex));
			ComboAbility.ChildAbility childAbility = this._abilitiesSequence[this.nextAbilityIndex];
			if (childAbility != this.selectedChildAbility)
			{
				childAbility.PassParamsToParent();
			}
		}

		// Token: 0x06002166 RID: 8550 RVA: 0x00068648 File Offset: 0x00066848
		private void UpdateSelectedAbility()
		{
			if (this.isCoupledCombo)
			{
				return;
			}
			this.selectedChildAbility = this._abilitiesSequence[this.selectedChildAbilityIndex];
		}

		// Token: 0x06002167 RID: 8551 RVA: 0x00068666 File Offset: 0x00066866
		public void AddTempStatModifier(TargetedMobStatModifier statModifier)
		{
			if (this.tempStatsModifiersHandler == null)
			{
				this.tempStatsModifiersHandler = new TempMobStatsModifiersHandler<TargetedMobStatModifier>(this);
			}
			this.tempStatsModifiersHandler.AddStatModifier(statModifier);
		}

		// Token: 0x06002168 RID: 8552 RVA: 0x00068688 File Offset: 0x00066888
		bool IInterruptableAction.TryInterrupt(bool force)
		{
			return false;
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x0006868C File Offset: 0x0006688C
		protected override void OnInitialize(object context)
		{
			if (ComboAbility.abilitiesFactory == null)
			{
				ComboAbility.abilitiesFactory = (context as IGameAbilitiesFactory);
			}
			this.canBeUsedOnOwner = false;
			this.isCoupledCombo = this.useAllAbilitiesAtTheSameTime;
			if (this.HasAbilitiesSequence())
			{
				if (this.childAbilitiesImpl == null)
				{
					this.childAbilitiesImpl = new IAbility[this._abilitiesSequence.Length];
				}
				AbilityFactoryArgs abilityFactoryArgs = new AbilityFactoryArgs
				{
					abilityID = (AbilityID)this.ID,
					targetsLayersOverride = (this._keepChildAbilityUsingLayers ? null : new int?(base.ValidObjectLayers)),
					rangeOverride = ((!this._useChildAbilityRange) ? new float?(base.Range) : null),
					reloadingTimeOverride = ((!this._useChildAbilityReloadingTime) ? new float?(base.ReloadingTime) : null),
					canGenerateBuffs = true,
					abilityOwner = base.Owner,
					parentAbility = this
				};
				if (this._passLevelToChildAbility)
				{
					abilityFactoryArgs.SetLevelFromAbility(this);
				}
				for (int i = 0; i < this._abilitiesSequence.Length; i++)
				{
					ComboAbility.ChildAbility childAbility = this._abilitiesSequence[i];
					childAbility.Initialize(this, i, abilityFactoryArgs);
					this.childAbilitiesImpl[i] = childAbility.AbilityInstance;
					if (!this.canBeUsedOnOwner)
					{
						this.canBeUsedOnOwner |= childAbility.AbilityInstance.CanBeUsedOnOwner;
					}
					if (this.useAllAbilitiesAtTheSameTime)
					{
						childAbility.waitingMode = ComboAbility.AbilityWaitingMode.None;
					}
					if (!this.isCoupledCombo && childAbility.IsAwaitable())
					{
						this.isCoupledCombo = true;
					}
					if (this.maxChildAbilityJumpHeight < childAbility.JumpMotionHeight)
					{
						this.maxChildAbilityJumpHeight = childAbility.JumpMotionHeight;
					}
					if (this.maxChildAbilityJumpDistance < childAbility.JumpMotionDistance)
					{
						this.maxChildAbilityJumpDistance = childAbility.JumpMotionDistance;
					}
				}
				this.ResetCombo();
				this.UpdateSelectedAbility();
			}
			else
			{
				this.childAbilitiesImpl = Array.Empty<IAbility>();
			}
			base.OnInitialize(context);
		}

		// Token: 0x0600216A RID: 8554 RVA: 0x00068870 File Offset: 0x00066A70
		protected override void OnOwnerChanged(object lastOwner, object newOwner)
		{
			if (base.IsPostMortemActivationInProgress)
			{
				return;
			}
			if (this.HasAbilitiesSequence())
			{
				this.ResetCombo();
				for (int i = 0; i < this._abilitiesSequence.Length; i++)
				{
					this._abilitiesSequence[i].SetOwner(newOwner);
				}
			}
			this.storedOwnerBehaviour = base.OwnerBehaviour;
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x000688C4 File Offset: 0x00066AC4
		protected override void OnPreparing(BaseAbility.UsingArgs usingArgs)
		{
			if (!this.abilityWasSelected && !this.isCoupledCombo)
			{
				if (Time.realtimeSinceStartup < this.expectedNextActivationTime)
				{
					this.selectedChildAbilityIndex = this.nextAbilityIndex;
				}
				this.UpdateSelectedAbility();
				Action<ComboAbility.ChildAbility> childAbilitySelected = this.ChildAbilitySelected;
				if (childAbilitySelected != null)
				{
					childAbilitySelected(this.selectedChildAbility);
				}
				this.selectedChildAbility.PassUsingDelayToParent();
				this.abilityWasSelected = true;
			}
			base.OnPreparing(usingArgs);
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x00068930 File Offset: 0x00066B30
		protected override void OnPrepared(BaseAbility.UsingArgs usingArgs)
		{
			base.OnPrepared(usingArgs);
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler == null)
			{
				return;
			}
			tempMobStatsModifiersHandler.ApplyModifiers(null);
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x0006894A File Offset: 0x00066B4A
		private void OnChildAbilityActivated(ComboAbility.ChildAbility ability)
		{
			Action<ComboAbility.ChildAbility> childAbilityActivated = this.ChildAbilityActivated;
			if (childAbilityActivated == null)
			{
				return;
			}
			childAbilityActivated(ability);
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x0006895D File Offset: 0x00066B5D
		private void OnChildAbilityCompleted(ComboAbility.ChildAbility ability, bool wasUsed)
		{
			this.childAbilityWasUsed = wasUsed;
			Action<ComboAbility.ChildAbility> childAbilityCompleted = this.ChildAbilityCompleted;
			if (childAbilityCompleted != null)
			{
				childAbilityCompleted(ability);
			}
			if (!this.resetComboIfChildAbilityFailed || this.childAbilityWasUsed)
			{
				this.UpdateNextAbilityInfo(-1);
				return;
			}
			this.ResetCombo();
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x00068996 File Offset: 0x00066B96
		protected override void OnCompleted(BaseAbility.UsingArgs usingArgs)
		{
			this.ResetCoupledSequence();
			this.abilityWasSelected = false;
			this.UpdateExpectedActivationTime();
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler != null)
			{
				tempMobStatsModifiersHandler.RemoveAppliedModifiers();
			}
			base.OnCompleted(usingArgs);
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x000689C4 File Offset: 0x00066BC4
		public override void UpdateAbility(float deltaTime)
		{
			bool flag = this.reloadingProgress < 1f;
			if (flag && this.selectedChildAbility != null)
			{
				this.selectedChildAbility.PassReloadingTimeToParent();
			}
			if (!this.isCoupledCombo)
			{
				if (flag)
				{
					this.UpdateExpectedActivationTime();
				}
				else if (!this.abilityWasSelected && this.expectedNextActivationTime > 0f && Time.realtimeSinceStartup > this.expectedNextActivationTime)
				{
					this.ResetCombo();
				}
			}
			base.UpdateAbility(deltaTime);
			Vector3 ownerPosition = base.OwnerPosition;
			if (this.coupledUsingCoroutine != null)
			{
				this.reloadingProgress = 0f;
			}
			for (int i = 0; i < this._abilitiesSequence.Length; i++)
			{
				this._abilitiesSequence[i].Update(ownerPosition, deltaTime);
			}
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x00068A74 File Offset: 0x00066C74
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.HasAbilitiesSequence())
			{
				for (int i = 0; i < this._abilitiesSequence.Length; i++)
				{
					this._abilitiesSequence[i].Destroy();
				}
			}
		}

		// Token: 0x040014BA RID: 5306
		private static readonly ComboAbilityUsingContext UsingContext = new ComboAbilityUsingContext();

		// Token: 0x040014BB RID: 5307
		private static IGameAbilitiesFactory abilitiesFactory;

		// Token: 0x040014C2 RID: 5314
		public AbilityTypes abilityType;

		// Token: 0x040014C3 RID: 5315
		public MobActivationAbilityType mobActivationType;

		// Token: 0x040014C4 RID: 5316
		[SerializeField]
		private ComboAbility.ChildAbility[] _abilitiesSequence;

		// Token: 0x040014C5 RID: 5317
		[FormerlySerializedAs("useAllAtOnce")]
		public bool useAllAbilitiesAtTheSameTime;

		// Token: 0x040014C6 RID: 5318
		[Space(5f)]
		[SerializeField]
		private bool _passLevelToChildAbility = true;

		// Token: 0x040014C7 RID: 5319
		[SerializeField]
		private bool _useChildAbilityRange;

		// Token: 0x040014C8 RID: 5320
		[SerializeField]
		[FormerlySerializedAs("_useComboAbilityReloadingTime")]
		private bool _useChildAbilityReloadingTime;

		// Token: 0x040014C9 RID: 5321
		[SerializeField]
		[FormerlySerializedAs("_useComboAbilityPrepTime")]
		private bool _useChildAbilityPrepTime;

		// Token: 0x040014CA RID: 5322
		[SerializeField]
		private bool _keepChildAbilityUsingLayers;

		// Token: 0x040014CB RID: 5323
		[SerializeField]
		private float _maxNextUsingTimeError = 0.5f;

		// Token: 0x040014CC RID: 5324
		public bool isCycledCombo = true;

		// Token: 0x040014CD RID: 5325
		public bool resetComboIfChildAbilityFailed = true;

		// Token: 0x040014CE RID: 5326
		private readonly List<IModifiableStat<MobStatModifier>> totalAbilityStatsList = new List<IModifiableStat<MobStatModifier>>(10);

		// Token: 0x040014CF RID: 5327
		private TempMobStatsModifiersHandler<TargetedMobStatModifier> tempStatsModifiersHandler;

		// Token: 0x040014D0 RID: 5328
		private bool canBeUsedOnOwner;

		// Token: 0x040014D1 RID: 5329
		private bool isCoupledCombo;

		// Token: 0x040014D2 RID: 5330
		private MonoBehaviour storedOwnerBehaviour;

		// Token: 0x040014D3 RID: 5331
		private int selectedChildAbilityIndex;

		// Token: 0x040014D4 RID: 5332
		private int nextAbilityIndex;

		// Token: 0x040014D5 RID: 5333
		private float expectedNextActivationTime;

		// Token: 0x040014D6 RID: 5334
		private bool abilityWasSelected;

		// Token: 0x040014D7 RID: 5335
		private bool childAbilityWasUsed;

		// Token: 0x040014D8 RID: 5336
		private ComboAbility.ChildAbility selectedChildAbility;

		// Token: 0x040014D9 RID: 5337
		private Coroutine coupledUsingCoroutine;

		// Token: 0x040014DA RID: 5338
		private DamageGenerator damageGenerator;

		// Token: 0x040014DB RID: 5339
		private float maxChildAbilityJumpHeight;

		// Token: 0x040014DC RID: 5340
		private float maxChildAbilityJumpDistance;

		// Token: 0x040014DD RID: 5341
		private IAbility[] childAbilitiesImpl;

		// Token: 0x02000592 RID: 1426
		public enum AbilityWaitingMode
		{
			// Token: 0x04001CDE RID: 7390
			None,
			// Token: 0x04001CDF RID: 7391
			Default,
			// Token: 0x04001CE0 RID: 7392
			WaitForCooldown,
			// Token: 0x04001CE1 RID: 7393
			WaitForBuffCompletion
		}

		// Token: 0x02000593 RID: 1427
		[Serializable]
		public sealed class ChildAbility : IAbilityDecorator
		{
			// Token: 0x0600278E RID: 10126 RVA: 0x0007BD14 File Offset: 0x00079F14
			public static BaseAbility ToGameAbility(ComboAbility.ChildAbility childAbility)
			{
				if (childAbility == null)
				{
					return null;
				}
				return childAbility.AbilityInstance;
			}

			// Token: 0x1700080A RID: 2058
			// (get) Token: 0x0600278F RID: 10127 RVA: 0x0007BD21 File Offset: 0x00079F21
			// (set) Token: 0x06002790 RID: 10128 RVA: 0x0007BD29 File Offset: 0x00079F29
			public float MaxAbilityProgress
			{
				get
				{
					return this._maxAbilityProgress;
				}
				set
				{
					this._maxAbilityProgress = Mathf.Clamp01(value);
				}
			}

			// Token: 0x1700080B RID: 2059
			// (get) Token: 0x06002791 RID: 10129 RVA: 0x0007BD37 File Offset: 0x00079F37
			// (set) Token: 0x06002792 RID: 10130 RVA: 0x0007BD3F File Offset: 0x00079F3F
			public ComboAbilityTriggerBase Trigger
			{
				get
				{
					return this._trigger;
				}
				set
				{
					this._trigger = value;
				}
			}

			// Token: 0x1700080C RID: 2060
			// (get) Token: 0x06002793 RID: 10131 RVA: 0x0007BD48 File Offset: 0x00079F48
			public ComboAbility ParentAbility
			{
				get
				{
					return this.parentAbility;
				}
			}

			// Token: 0x1700080D RID: 2061
			// (get) Token: 0x06002794 RID: 10132 RVA: 0x0007BD50 File Offset: 0x00079F50
			public BaseAbility SourceAbility
			{
				get
				{
					return this._sourceAbility;
				}
			}

			// Token: 0x1700080E RID: 2062
			// (get) Token: 0x06002795 RID: 10133 RVA: 0x0007BD58 File Offset: 0x00079F58
			public int Index
			{
				get
				{
					return this.index;
				}
			}

			// Token: 0x1700080F RID: 2063
			// (get) Token: 0x06002796 RID: 10134 RVA: 0x0007BD60 File Offset: 0x00079F60
			public BaseAbility AbilityInstance
			{
				get
				{
					return this.abilityInstance;
				}
			}

			// Token: 0x17000810 RID: 2064
			// (get) Token: 0x06002797 RID: 10135 RVA: 0x0007BD68 File Offset: 0x00079F68
			public bool IsLastChild
			{
				get
				{
					return this.index == this.parentAbility.AbilitiesSequence.Length - 1;
				}
			}

			// Token: 0x17000811 RID: 2065
			// (get) Token: 0x06002798 RID: 10136 RVA: 0x0007BD81 File Offset: 0x00079F81
			public float JumpMotionHeight
			{
				get
				{
					return this.jumpMotionHeight;
				}
			}

			// Token: 0x17000812 RID: 2066
			// (get) Token: 0x06002799 RID: 10137 RVA: 0x0007BD89 File Offset: 0x00079F89
			public float JumpMotionDistance
			{
				get
				{
					return this.jumpMotionDistance;
				}
			}

			// Token: 0x17000813 RID: 2067
			// (get) Token: 0x0600279A RID: 10138 RVA: 0x0007BD91 File Offset: 0x00079F91
			BaseAbility IAbilityDecorator.AbilityPrototype
			{
				get
				{
					return this._sourceAbility;
				}
			}

			// Token: 0x17000814 RID: 2068
			// (get) Token: 0x0600279B RID: 10139 RVA: 0x0007BD99 File Offset: 0x00079F99
			BaseAbility IAbilityDecorator.AbilityInstance
			{
				get
				{
					return this.abilityInstance;
				}
			}

			// Token: 0x0600279C RID: 10140 RVA: 0x0007BDA1 File Offset: 0x00079FA1
			private bool IsProgressBasedAbility()
			{
				return this.progressBasedActionAbility != null;
			}

			// Token: 0x0600279D RID: 10141 RVA: 0x0007BDAC File Offset: 0x00079FAC
			private void OnAbilityBuffSent(BaseAbility ability, IBuffsController targetBuffsController, IBuff buff)
			{
				this.generatedBuffsCount++;
				buff.Completed += this.OnBuffCompleted;
			}

			// Token: 0x0600279E RID: 10142 RVA: 0x0007BDCE File Offset: 0x00079FCE
			private void OnBuffCompleted(IBuff completedBuff)
			{
				this.generatedBuffsCount--;
				completedBuff.Completed -= this.OnBuffCompleted;
			}

			// Token: 0x0600279F RID: 10143 RVA: 0x0007BDF0 File Offset: 0x00079FF0
			private void OnAbilityActivated(IAbility ability, object usingArgs)
			{
				this.parentAbility.OnChildAbilityActivated(this);
			}

			// Token: 0x060027A0 RID: 10144 RVA: 0x0007BE00 File Offset: 0x0007A000
			private void OnAbilityCompleted(IAbility ability, object usingArgs)
			{
				bool wasUsed = this.abilityInstance.WasUsed;
				this.abilityInstance.PrepTime = this.storedPrepTime;
				this.abilityInstance.UsingDelay = this.storedUsingDelay;
				this.nextAbilityStartTime = ((wasUsed && this.nextAbilityPause > 0f) ? (Time.time + this.nextAbilityPause) : 0f);
				if (this.waitingMode == ComboAbility.AbilityWaitingMode.WaitForCooldown && this.nextAbilityStartTime != 0f)
				{
					this.nextAbilityStartTime += this.abilityInstance.ReloadingTime;
				}
				this.parentAbility.OnChildAbilityCompleted(this, wasUsed);
			}

			// Token: 0x060027A1 RID: 10145 RVA: 0x0007BEA0 File Offset: 0x0007A0A0
			public void Initialize(ComboAbility parentAbility, int abilityIndex, AbilityFactoryArgs abilitiesFactoryArgs)
			{
				if (this._sourceAbility == null || this.abilityInstance != null)
				{
					return;
				}
				this.parentAbility = parentAbility;
				this.index = abilityIndex;
				abilitiesFactoryArgs.abilityPrototype = this._sourceAbility;
				this.abilityInstance = (BaseAbility)ComboAbility.abilitiesFactory.Create(abilitiesFactoryArgs);
				this.abilityInstance.IsAutoUseAbility = false;
				ComboAbility comboAbility = this.abilityInstance as ComboAbility;
				if (comboAbility != null && this._sourceAbility == parentAbility)
				{
					comboAbility.AbilitiesSequence = new ComboAbility.ChildAbility[0];
				}
				this.storedPrepTime = this.abilityInstance.PrepTime;
				this.awaitableAbility = (this.abilityInstance as IContinuousAction);
				this.progressBasedActionAbility = (this.abilityInstance as IProgressBasedAction);
				parentAbility.TrySetDamageGenerator(this.abilityInstance);
				parentAbility.RegisterChildAbilityStats(this.abilityInstance);
				ValueTuple<float, float> jumpAbilityParams = this.abilityInstance.GetJumpAbilityParams();
				this.jumpMotionHeight = jumpAbilityParams.Item1;
				this.jumpMotionDistance = jumpAbilityParams.Item2;
				this.abilityInstance.Activated += this.OnAbilityActivated;
				this.abilityInstance.Completed += this.OnAbilityCompleted;
				if (this.waitingMode == ComboAbility.AbilityWaitingMode.WaitForBuffCompletion && this.abilityInstance.CanGenerateBuffs)
				{
					this.abilityInstance.BuffSent += this.OnAbilityBuffSent;
				}
			}

			// Token: 0x060027A2 RID: 10146 RVA: 0x0007BFFC File Offset: 0x0007A1FC
			public bool IsAwaitable()
			{
				if (this.nextAbilityPause > 0f)
				{
					return true;
				}
				switch (this.waitingMode)
				{
				case ComboAbility.AbilityWaitingMode.Default:
					return this.abilityInstance.IsContinuous;
				case ComboAbility.AbilityWaitingMode.WaitForCooldown:
					return this.abilityInstance.ReloadingTime > 0f;
				case ComboAbility.AbilityWaitingMode.WaitForBuffCompletion:
					return this.abilityInstance.CanGenerateBuffs;
				default:
					return false;
				}
			}

			// Token: 0x060027A3 RID: 10147 RVA: 0x0007C061 File Offset: 0x0007A261
			public void SetOwner(object newOwner)
			{
				this.abilityInstance.Owner = newOwner;
			}

			// Token: 0x060027A4 RID: 10148 RVA: 0x0007C070 File Offset: 0x0007A270
			public void PassParamsToParent()
			{
				if (this.parentAbility._useChildAbilityRange)
				{
					this.parentAbility.Range = this.abilityInstance.Range;
				}
				if (this.parentAbility._useChildAbilityPrepTime)
				{
					this.parentAbility.PrepTime = this.abilityInstance.PrepTime;
				}
			}

			// Token: 0x060027A5 RID: 10149 RVA: 0x0007C0C3 File Offset: 0x0007A2C3
			public void PassUsingDelayToParent()
			{
				if (this.parentAbility.UsingDelay <= 0f)
				{
					this.parentAbility.UsingDelay = this.abilityInstance.UsingDelay;
				}
			}

			// Token: 0x060027A6 RID: 10150 RVA: 0x0007C0ED File Offset: 0x0007A2ED
			public void PassReloadingTimeToParent()
			{
				this.parentAbility.ReloadingTime = this.abilityInstance.ReloadingTime;
			}

			// Token: 0x060027A7 RID: 10151 RVA: 0x0007C105 File Offset: 0x0007A305
			public bool CanBeActivated(BaseAbility.UsingArgs usingArgs)
			{
				return this.abilityInstance.CanBeActivated(usingArgs);
			}

			// Token: 0x060027A8 RID: 10152 RVA: 0x0007C114 File Offset: 0x0007A314
			public void Use(BaseAbility.UsingArgs usingArgs)
			{
				this.storedUsingDelay = this.abilityInstance.UsingDelay;
				this.abilityInstance.PrepTime = 0f;
				this.abilityInstance.MaxUsingCount = 0;
				this.abilityInstance.HasInfiniteUsingDuration = false;
				this.abilityInstance.UsingDelay = 0f;
				this.abilityInstance.Activate(usingArgs);
			}

			// Token: 0x060027A9 RID: 10153 RVA: 0x0007C178 File Offset: 0x0007A378
			public bool InProgress()
			{
				if (this.abilityInstance.IsWaitingForUse || (this.nextAbilityStartTime > 0f && Time.time < this.nextAbilityStartTime))
				{
					return true;
				}
				switch (this.waitingMode)
				{
				case ComboAbility.AbilityWaitingMode.Default:
					if (this._maxAbilityProgress > 0f && this._maxAbilityProgress < 1f && this.IsProgressBasedAbility())
					{
						return this.progressBasedActionAbility.CurrentProgress < this._maxAbilityProgress;
					}
					if (!this.abilityInstance.InUse)
					{
						IContinuousAction continuousAction = this.awaitableAbility;
						return continuousAction != null && !continuousAction.IsPerformed;
					}
					return true;
				case ComboAbility.AbilityWaitingMode.WaitForCooldown:
					return this.abilityInstance.IsReloading();
				case ComboAbility.AbilityWaitingMode.WaitForBuffCompletion:
					return this.generatedBuffsCount > 0;
				default:
					return false;
				}
			}

			// Token: 0x060027AA RID: 10154 RVA: 0x0007C240 File Offset: 0x0007A440
			public void Update(Vector3 ownerPosition, float deltaTime)
			{
				if (this.parentAbility.IsPostMortemActivationInProgress)
				{
					this.abilityInstance.Owner = this.parentAbility.storedOwnerBehaviour;
				}
				this.abilityInstance.OwnerPosition = ownerPosition;
				this.abilityInstance.UpdateAbility(deltaTime);
				this.abilityInstance.ForceReload();
			}

			// Token: 0x060027AB RID: 10155 RVA: 0x0007C294 File Offset: 0x0007A494
			public bool TryInterruptProgressBasedAbility()
			{
				if (this.waitingMode == ComboAbility.AbilityWaitingMode.Default && this.IsProgressBasedAbility())
				{
					IInterruptableAction interruptableAction = this.abilityInstance as IInterruptableAction;
					if (interruptableAction != null)
					{
						return interruptableAction.TryInterrupt(this.forceInterrupt);
					}
				}
				return false;
			}

			// Token: 0x060027AC RID: 10156 RVA: 0x0007C2D0 File Offset: 0x0007A4D0
			public void Destroy()
			{
				if (this.abilityInstance != null)
				{
					this.abilityInstance.BuffSent -= this.OnAbilityBuffSent;
					this.abilityInstance.Completed -= this.OnAbilityCompleted;
					this.abilityInstance.Destroy();
					this.abilityInstance = null;
				}
			}

			// Token: 0x04001CE2 RID: 7394
			[SerializeField]
			private BaseAbility _sourceAbility;

			// Token: 0x04001CE3 RID: 7395
			[Obsolete]
			[HideInInspector]
			[Tooltip("Усиленная / критическая атака и т.п.")]
			public bool isPowerfulAbility;

			// Token: 0x04001CE4 RID: 7396
			public ComboAbility.AbilityWaitingMode waitingMode = ComboAbility.AbilityWaitingMode.Default;

			// Token: 0x04001CE5 RID: 7397
			public float nextAbilityPause;

			// Token: 0x04001CE6 RID: 7398
			public bool tryUpdateUsingPoint;

			// Token: 0x04001CE7 RID: 7399
			[SerializeField]
			[Range(0f, 1f)]
			[FormerlySerializedAs("_interruptionThreshold")]
			private float _maxAbilityProgress;

			// Token: 0x04001CE8 RID: 7400
			public bool forceInterrupt = true;

			// Token: 0x04001CE9 RID: 7401
			[SerializeReference]
			[ManagedObjectField(typeof(ComboAbilityTriggerBase))]
			private ComboAbilityTriggerBase _trigger;

			// Token: 0x04001CEA RID: 7402
			private ComboAbility parentAbility;

			// Token: 0x04001CEB RID: 7403
			private int index = -1;

			// Token: 0x04001CEC RID: 7404
			private BaseAbility abilityInstance;

			// Token: 0x04001CED RID: 7405
			private float storedPrepTime;

			// Token: 0x04001CEE RID: 7406
			private float storedUsingDelay;

			// Token: 0x04001CEF RID: 7407
			private IContinuousAction awaitableAbility;

			// Token: 0x04001CF0 RID: 7408
			private IProgressBasedAction progressBasedActionAbility;

			// Token: 0x04001CF1 RID: 7409
			private int generatedBuffsCount;

			// Token: 0x04001CF2 RID: 7410
			private float nextAbilityStartTime;

			// Token: 0x04001CF3 RID: 7411
			private float jumpMotionHeight;

			// Token: 0x04001CF4 RID: 7412
			private float jumpMotionDistance;
		}
	}
}
