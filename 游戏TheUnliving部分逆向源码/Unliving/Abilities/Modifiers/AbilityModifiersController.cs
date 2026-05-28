using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Editor;
using Common.PivotGroup;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Abilities.TargetsCollection;
using Game.Damage.Projectiles;
using Game.ObjectPool;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.AbilityResources;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C0 RID: 960
	[CreateAssetMenu(fileName = "AbilityModifiersController", menuName = "Abilities/Ability Modifiers Controller")]
	public class AbilityModifiersController : AbilityExtensionAssetBase
	{
		// Token: 0x17000699 RID: 1689
		// (get) Token: 0x0600206F RID: 8303 RVA: 0x000661D3 File Offset: 0x000643D3
		public override bool CanBeUsedAsMainBehaviour
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700069A RID: 1690
		// (get) Token: 0x06002070 RID: 8304 RVA: 0x000661D6 File Offset: 0x000643D6
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700069B RID: 1691
		// (get) Token: 0x06002071 RID: 8305 RVA: 0x000661D9 File Offset: 0x000643D9
		// (set) Token: 0x06002072 RID: 8306 RVA: 0x000661E1 File Offset: 0x000643E1
		public AbilityModifiersController Prototype { get; set; }

		// Token: 0x1700069C RID: 1692
		// (get) Token: 0x06002073 RID: 8307 RVA: 0x000661EA File Offset: 0x000643EA
		// (set) Token: 0x06002074 RID: 8308 RVA: 0x000661F2 File Offset: 0x000643F2
		public AbilityModifiersActivatorBase Activator
		{
			get
			{
				return this.activator;
			}
			set
			{
				this.activator = value;
			}
		}

		// Token: 0x1700069D RID: 1693
		// (get) Token: 0x06002075 RID: 8309 RVA: 0x000661FB File Offset: 0x000643FB
		// (set) Token: 0x06002076 RID: 8310 RVA: 0x00066203 File Offset: 0x00064403
		public StatsController Stats { get; set; }

		// Token: 0x1700069E RID: 1694
		// (get) Token: 0x06002077 RID: 8311 RVA: 0x0006620C File Offset: 0x0006440C
		public AbilityStatModifier[] StatsModifiersPrototypes
		{
			get
			{
				return this.statsModifiersPrototypes;
			}
		}

		// Token: 0x1700069F RID: 1695
		// (get) Token: 0x06002078 RID: 8312 RVA: 0x00066214 File Offset: 0x00064414
		public AbilityEffectsBasedModifier EffectsBasedModifierPrototype
		{
			get
			{
				return this.effectsBasedModifierPrototype;
			}
		}

		// Token: 0x170006A0 RID: 1696
		// (get) Token: 0x06002079 RID: 8313 RVA: 0x0006621C File Offset: 0x0006441C
		public AbilityBuffsBasedModifier BuffsBasedModifierPrototype
		{
			get
			{
				return this.buffsBasedModifierPrototype;
			}
		}

		// Token: 0x170006A1 RID: 1697
		// (get) Token: 0x0600207A RID: 8314 RVA: 0x00066224 File Offset: 0x00064424
		public AbilityEffectZoneSpawningModifier EffectZoneSpawningModifierPrototype
		{
			get
			{
				return this.effectZoneSpawningModifierPrototype;
			}
		}

		// Token: 0x170006A2 RID: 1698
		// (get) Token: 0x0600207B RID: 8315 RVA: 0x0006622C File Offset: 0x0006442C
		public AbilitySummoningModifier SummoningBasedModifierPrototype
		{
			get
			{
				return this.summoningBasedModifierPrototype;
			}
		}

		// Token: 0x170006A3 RID: 1699
		// (get) Token: 0x0600207C RID: 8316 RVA: 0x00066234 File Offset: 0x00064434
		public ExternalAbilitiesUsingModifier ExternalAbilitiesModifierPrototype
		{
			get
			{
				return this.externalAbilitiesModifierPrototype;
			}
		}

		// Token: 0x170006A4 RID: 1700
		// (get) Token: 0x0600207D RID: 8317 RVA: 0x0006623C File Offset: 0x0006443C
		public AbilityModifiersController.VFXController ModifiersVFX
		{
			get
			{
				return this.modifiersVFX;
			}
		}

		// Token: 0x0600207E RID: 8318 RVA: 0x00066244 File Offset: 0x00064444
		private void AddModifiers(BaseAbility ability)
		{
			AbilityModifiersController.<>c__DisplayClass61_0 CS$<>8__locals1;
			CS$<>8__locals1.ability = ability;
			List<AbilityModifierBase> list = new List<AbilityModifierBase>(8);
			AbilityModifierBase abilityModifierBase;
			for (int i = 0; i < this.statsModifiersPrototypes.Length; i++)
			{
				AbilityModifiersController.<AddModifiers>g__AddModifier|61_0(list, this.statsModifiersPrototypes[i], out abilityModifierBase, ref CS$<>8__locals1);
			}
			AbilityModifierBase abilityModifierBase2;
			AbilityModifiersController.<AddModifiers>g__AddModifier|61_0(list, this.effectsBasedModifierPrototype, out abilityModifierBase2, ref CS$<>8__locals1);
			AbilityModifierBase abilityModifierBase3;
			AbilityModifiersController.<AddModifiers>g__AddModifier|61_0(list, this.buffsBasedModifierPrototype, out abilityModifierBase3, ref CS$<>8__locals1);
			AbilityModifiersController.<AddModifiers>g__AddModifier|61_0(list, this.effectZoneSpawningModifierPrototype, out abilityModifierBase, ref CS$<>8__locals1);
			AbilityModifierBase abilityModifierBase4;
			AbilityModifiersController.<AddModifiers>g__AddModifier|61_0(list, this.summoningBasedModifierPrototype, out abilityModifierBase4, ref CS$<>8__locals1);
			AbilityModifiersController.<AddModifiers>g__AddModifier|61_0(list, this.externalAbilitiesModifierPrototype, out abilityModifierBase, ref CS$<>8__locals1);
			if (abilityModifierBase2 != null)
			{
				abilityModifierBase2.Used += this.OnTargetedModifierUsed;
			}
			else if (abilityModifierBase3 != null)
			{
				abilityModifierBase3.Used += this.OnTargetedModifierUsed;
			}
			if (abilityModifierBase4 != null)
			{
				((AbilitySummoningModifier)abilityModifierBase4).MobSummoned += this.OnSummoningModifierUsed;
			}
			this.abilitiesModifiers.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(CS$<>8__locals1.ability), list);
		}

		// Token: 0x0600207F RID: 8319 RVA: 0x00066340 File Offset: 0x00064540
		private void RemoveModifiers(BaseAbility ability)
		{
			int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
			List<AbilityModifierBase> list = this.abilitiesModifiers[abilityInstanceID];
			for (int i = 0; i < list.Count; i++)
			{
				AbilityModifierBase abilityModifierBase = list[i];
				abilityModifierBase.OnRemovedFromAbility(ability);
				abilityModifierBase.Used -= this.OnTargetedModifierUsed;
				AbilitySummoningModifier abilitySummoningModifier = abilityModifierBase as AbilitySummoningModifier;
				if (abilitySummoningModifier != null)
				{
					abilitySummoningModifier.MobSummoned -= this.OnSummoningModifierUsed;
				}
			}
			list.Clear();
			this.abilitiesModifiers.Remove(abilityInstanceID);
		}

		// Token: 0x06002080 RID: 8320 RVA: 0x000663C0 File Offset: 0x000645C0
		private bool TryHandleAsCompositeAbility(BaseAbility ability, bool register)
		{
			ICompositeAbility compositeAbility = ability as ICompositeAbility;
			if (compositeAbility != null && compositeAbility.WillUseChildAbilitiesSequentially)
			{
				IList<IAbility> childAbilities = compositeAbility.ChildAbilities;
				int count = childAbilities.Count;
				for (int i = 0; i < count; i++)
				{
					BaseAbility ability2 = (BaseAbility)childAbilities[i];
					if (register)
					{
						this.RegisterAbility(ability2);
					}
					else
					{
						this.UnregisterAbility(ability2);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002081 RID: 8321 RVA: 0x00066420 File Offset: 0x00064620
		private void RegisterAbility(BaseAbility ability)
		{
			if (this.TryHandleAsCompositeAbility(ability, true))
			{
				return;
			}
			if (this.CanBeAdded(ability))
			{
				AbilityModifiersActivatorBase abilityModifiersActivatorBase = this.activator;
				if (abilityModifiersActivatorBase != null)
				{
					abilityModifiersActivatorBase.RegisterAbility(ability);
				}
				this.AddModifiers(ability);
				ability.Activating += this.OnAbilityPreparing;
				ability.Activated += this.OnAbilityPrepared;
				ability.Used += this.OnAbilityUsed;
				ability.Completed += this.OnAbilityCompleted;
				ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
				if (projectileAbilityBase != null)
				{
					projectileAbilityBase.ProjectileLaunched += this.OnAbilityProjectileLaunched;
					projectileAbilityBase.ProjectileUsingPrepared += this.OnBeforeAbilityProjectileHit;
					projectileAbilityBase.ProjectileUsed += this.OnAbilityProjectileHit;
				}
			}
		}

		// Token: 0x06002082 RID: 8322 RVA: 0x000664E8 File Offset: 0x000646E8
		private void UnregisterAbility(BaseAbility ability)
		{
			if (this.TryHandleAsCompositeAbility(ability, false))
			{
				return;
			}
			ability.Activating -= this.OnAbilityPreparing;
			ability.Activated -= this.OnAbilityPrepared;
			ability.Used -= this.OnAbilityUsed;
			ability.Completed -= this.OnAbilityCompleted;
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched -= this.OnAbilityProjectileLaunched;
				projectileAbilityBase.ProjectileUsingPrepared -= this.OnBeforeAbilityProjectileHit;
			}
			AbilityModifiersActivatorBase abilityModifiersActivatorBase = this.activator;
			if (abilityModifiersActivatorBase != null)
			{
				abilityModifiersActivatorBase.UnregisterAbility(ability);
			}
			this.RemoveModifiers(ability);
		}

		// Token: 0x06002083 RID: 8323 RVA: 0x00066590 File Offset: 0x00064790
		private void UseModifiers<TModifier>(AbilityModifierUsingArgs usingArgs) where TModifier : AbilityModifierBase
		{
			List<AbilityModifierBase> list;
			if (this.abilitiesModifiers.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(usingArgs.targetAbility), out list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					TModifier tmodifier = list[i] as TModifier;
					if (tmodifier != null)
					{
						tmodifier.Use(usingArgs);
					}
				}
			}
		}

		// Token: 0x06002084 RID: 8324 RVA: 0x000665EE File Offset: 0x000647EE
		private void SetModifiersOverrides(BaseAbility ability, AbilityModifiersActivatorArgs activatorArgs)
		{
			activatorArgs.overrides = ability.GetModifiersOverrides();
		}

		// Token: 0x06002085 RID: 8325 RVA: 0x000665FC File Offset: 0x000647FC
		private BaseAbility.UsingArgs GetActualAbilityUsingArgs(BaseAbility ability, BaseAbility.UsingArgs currentUsingArgs)
		{
			if (this.modifiersUsingTarget == AbilityModifiersController.UsingTarget.AbilityOwner)
			{
				this.OwnerOnlyAbilityUsingArgs.PrepareForUsingOnOwner(ability);
				return this.OwnerOnlyAbilityUsingArgs;
			}
			return currentUsingArgs;
		}

		// Token: 0x06002086 RID: 8326 RVA: 0x0006661C File Offset: 0x0006481C
		private AbilityModifierUsingArgs GetPreparedProjectileModifiersArgs(ProjectileAbilityBase.UsingEventArgs projectileUsingArgs, int usingCount)
		{
			ProjectileAbilityBase ability = projectileUsingArgs.ability;
			BaseAbility.UsingArgs actualAbilityUsingArgs = this.GetActualAbilityUsingArgs(ability, projectileUsingArgs.abilityUsingArgs);
			this.projectilesEffectModifiersUsingArgs.Reset();
			this.projectilesEffectModifiersUsingArgs.modifiersStats = this.Stats;
			this.projectilesEffectModifiersUsingArgs.targetAbility = ability;
			this.projectilesEffectModifiersUsingArgs.CopyTargetsInfo(actualAbilityUsingArgs);
			this.projectilesEffectModifiersUsingArgs.modifiersUsingCount = usingCount;
			return this.projectilesEffectModifiersUsingArgs;
		}

		// Token: 0x06002087 RID: 8327 RVA: 0x00066684 File Offset: 0x00064884
		private AbilityModifiersActivatorArgs UpdateActivatorArgs(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs, AbilityUsingStage abilityUsingStage)
		{
			this.activatorArgs.modifiersController = this;
			this.activatorArgs.ability = ability;
			this.activatorArgs.abilityUsingArgs = abilityUsingArgs;
			this.activatorArgs.abilityUsingStage = abilityUsingStage;
			this.SetModifiersOverrides(ability, this.activatorArgs);
			return this.activatorArgs;
		}

		// Token: 0x06002088 RID: 8328 RVA: 0x000666D4 File Offset: 0x000648D4
		private void TriggerActivator(BaseAbility ability, BaseAbility.UsingArgs usingArgs, AbilityUsingStage usingStage)
		{
			if (this.activator == null)
			{
				return;
			}
			BaseAbility.UsingArgs actualAbilityUsingArgs = this.GetActualAbilityUsingArgs(ability, usingArgs);
			AbilityModifiersActivatorArgs abilityModifiersActivatorArgs = this.UpdateActivatorArgs(ability, actualAbilityUsingArgs, usingStage);
			BaseAbility.ActivationError activationError;
			if (!this.activator.TryActivate(abilityModifiersActivatorArgs, out activationError) && this.canBlockAbilityUsing)
			{
				if (ability.IsBusy())
				{
					ability.Complete();
					return;
				}
				if (usingStage == AbilityUsingStage.Preparing)
				{
					if (activationError != null)
					{
						abilityModifiersActivatorArgs.ability.TryInterruptActivation(activationError);
						return;
					}
					this.abilityInterruptionError.Reset();
					this.abilityInterruptionError.type = BaseAbility.ActivationErrorType.External;
					this.abilityInterruptionError.usingArgs = abilityModifiersActivatorArgs.abilityUsingArgs;
					this.abilityInterruptionError.interruptionSource = this.activator;
					abilityModifiersActivatorArgs.ability.TryInterruptActivation(this.abilityInterruptionError);
				}
			}
		}

		// Token: 0x06002089 RID: 8329 RVA: 0x00066787 File Offset: 0x00064987
		private void ResetActivator(BaseAbility ability, bool force)
		{
			if (this.activator != null)
			{
				this.activator.Reset(this, ability, force);
			}
		}

		// Token: 0x0600208A RID: 8330 RVA: 0x000667A8 File Offset: 0x000649A8
		private bool TryStartUsing(IAbility ability, object abilityUsingArgs, AbilityUsingStage usingStage, out AbilityModifierUsingArgs modifiersUsingArgs)
		{
			BaseAbility baseAbility = (BaseAbility)ability;
			BaseAbility.UsingArgs actualAbilityUsingArgs = this.GetActualAbilityUsingArgs(baseAbility, (BaseAbility.UsingArgs)abilityUsingArgs);
			if (this.activator != null)
			{
				if (this.activator.TryUse(this.UpdateActivatorArgs(baseAbility, actualAbilityUsingArgs, usingStage), out modifiersUsingArgs))
				{
					modifiersUsingArgs.modifiersStats = this.Stats;
					if (this.maxModifiersUsingCount > 0 && modifiersUsingArgs.modifiersUsingCount > this.maxModifiersUsingCount)
					{
						modifiersUsingArgs.modifiersUsingCount = this.maxModifiersUsingCount;
					}
					return true;
				}
				return false;
			}
			else
			{
				if ((usingStage == AbilityUsingStage.Prepared || usingStage == AbilityUsingStage.Used || usingStage == AbilityUsingStage.ProjectileLaunched) && this.defaultModifiersUsingCount > 0)
				{
					int modifiersUsingCount = this.defaultModifiersUsingCount;
					this.defaultModifiersUsingArgs.Reset();
					modifiersUsingArgs = this.defaultModifiersUsingArgs;
					modifiersUsingArgs.modifiersStats = this.Stats;
					AbilityModifiersOverrides modifiersOverrides = baseAbility.GetModifiersOverrides();
					if (modifiersOverrides != null)
					{
						modifiersOverrides.SetUsingCount(ref modifiersUsingCount);
					}
					this.defaultModifiersUsingArgs.CopyTargetsInfo(actualAbilityUsingArgs);
					this.defaultModifiersUsingArgs.targetAbility = baseAbility;
					this.defaultModifiersUsingArgs.modifiersUsingCount = modifiersUsingCount;
					return true;
				}
				modifiersUsingArgs = null;
				return false;
			}
		}

		// Token: 0x0600208B RID: 8331 RVA: 0x000668A8 File Offset: 0x00064AA8
		private void UseTargetedModifiers(AbilityModifierUsingArgs modifiersUsingArgs)
		{
			if (modifiersUsingArgs.modifiersUsingCount <= 0 || (!this.effectsBasedModifierPrototype.IsActive && !this.buffsBasedModifierPrototype.IsActive))
			{
				return;
			}
			BaseAbility targetAbility = modifiersUsingArgs.targetAbility;
			float num = this.getTargetsInAbilityRange ? targetAbility.GetAbilityEffectRange() : this.targetsCollectionRange;
			int num2 = this.getTargetsWithAbilityLayers ? targetAbility.ValidObjectLayers : this.targetsCollectionLayers;
			if (this.modifiersUsingTarget != AbilityModifiersController.UsingTarget.Default || num <= 0f || num2 == 0)
			{
				this.<UseTargetedModifiers>g__UseModifiers|74_1(modifiersUsingArgs, modifiersUsingArgs.targetsInfo);
				return;
			}
			this.modifiersTargetsCollectionArgs.PrepareForTargetsCollection(null, this.sortTargets, -1);
			this.modifiersTargetsCollectionArgs.range = num;
			this.modifiersTargetsCollectionArgs.collectableTargetsLayers = num2;
			this.modifiersTargetsCollectionArgs.maxTargetsCount = ((this.maxTargetsCountOverride > 0) ? this.maxTargetsCountOverride : targetAbility.GetMaxTargetsInRangeCount());
			IList<CollectableAbilityResource> collectedResources = modifiersUsingArgs.GetCollectedResources();
			if (modifiersUsingArgs.tryUseAtMultiplePositions && collectedResources != null && collectedResources.Count != 0)
			{
				int modifiersUsingCount = modifiersUsingArgs.modifiersUsingCount;
				modifiersUsingArgs.modifiersUsingCount = 1;
				for (int i = 0; i < collectedResources.Count; i++)
				{
					this.<UseTargetedModifiers>g__UseModifiers|74_1(modifiersUsingArgs, this.<UseTargetedModifiers>g__CollectTargets|74_0(collectedResources[i].GetPosition(false), this.modifiersTargetsCollectionArgs));
				}
				modifiersUsingArgs.modifiersUsingCount = modifiersUsingCount;
				return;
			}
			Vector2 position = this.forceGetTargetsAroundAbilityOwner ? targetAbility.OwnerPosition : modifiersUsingArgs.targetsInfo.targetPosition;
			this.<UseTargetedModifiers>g__UseModifiers|74_1(modifiersUsingArgs, this.<UseTargetedModifiers>g__CollectTargets|74_0(position, this.modifiersTargetsCollectionArgs));
		}

		// Token: 0x0600208C RID: 8332 RVA: 0x00066A2E File Offset: 0x00064C2E
		private void UseNonStatModifiers(AbilityModifierUsingArgs modifiersUsingArgs)
		{
			this.UseTargetedModifiers(modifiersUsingArgs);
			this.UseModifiers<AbilityEffectZoneSpawningModifier>(modifiersUsingArgs);
			this.UseModifiers<AbilitySummoningModifier>(modifiersUsingArgs);
			this.UseModifiers<ExternalAbilitiesUsingModifier>(modifiersUsingArgs);
		}

		// Token: 0x0600208D RID: 8333 RVA: 0x00066A4C File Offset: 0x00064C4C
		private void UseModifiersOnProjectileHit(ProjectileAbilityBase.UsingEventArgs projectileUsingArgs, bool isProjectileEffectApplied)
		{
			ProjectileAbilityBase ability = projectileUsingArgs.ability;
			BaseAbility.UsingArgs abilityUsingArgs = projectileUsingArgs.abilityUsingArgs;
			AbilityUsingStage usingStage = isProjectileEffectApplied ? AbilityUsingStage.PostUsed : AbilityUsingStage.Used;
			this.TriggerActivator(ability, abilityUsingArgs, usingStage);
			AbilityModifierUsingArgs abilityModifierUsingArgs;
			if (this.TryStartUsing(ability, abilityUsingArgs, usingStage, out abilityModifierUsingArgs))
			{
				if (!isProjectileEffectApplied)
				{
					this.UseModifiers<AbilityStatModifier>(abilityModifierUsingArgs);
				}
				this.UseNonStatModifiers(abilityModifierUsingArgs);
				this.ResetActivator(ability, true);
			}
		}

		// Token: 0x0600208E RID: 8334 RVA: 0x00066A9E File Offset: 0x00064C9E
		protected virtual bool CanBeAdded(BaseAbility targetAbility)
		{
			return true;
		}

		// Token: 0x0600208F RID: 8335 RVA: 0x00066AA4 File Offset: 0x00064CA4
		public int GetAllModifiers<TModifier>(BaseAbility ability, out AbilityModifierBase[] modifiers) where TModifier : AbilityModifierBase
		{
			int result = 0;
			List<AbilityModifierBase> list;
			if (this.abilitiesModifiers.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					TModifier tmodifier = list[i] as TModifier;
					if (tmodifier != null)
					{
						AbilityModifiersController.ModifiersQueriesBuffer[result++] = tmodifier;
					}
				}
			}
			modifiers = AbilityModifiersController.ModifiersQueriesBuffer;
			return result;
		}

		// Token: 0x06002090 RID: 8336 RVA: 0x00066B0C File Offset: 0x00064D0C
		public bool TryGetModifier<TModifier>(BaseAbility ability, out TModifier modifier) where TModifier : AbilityModifierBase
		{
			AbilityModifierBase[] array;
			if (this.GetAllModifiers<TModifier>(ability, out array) != 0)
			{
				modifier = (TModifier)((object)array[0]);
				return true;
			}
			modifier = default(TModifier);
			return false;
		}

		// Token: 0x06002091 RID: 8337 RVA: 0x00066B3C File Offset: 0x00064D3C
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.RegisterAbility(ability);
		}

		// Token: 0x06002092 RID: 8338 RVA: 0x00066B4C File Offset: 0x00064D4C
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			this.UnregisterAbility(ability);
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06002093 RID: 8339 RVA: 0x00066B5C File Offset: 0x00064D5C
		private void OnAbilityPreparing(IAbility ability, object usingArgs)
		{
			if (ability.PrepProgress != 0f || !ability.CanBeActivated(usingArgs))
			{
				return;
			}
			this.TriggerActivator((BaseAbility)ability, (BaseAbility.UsingArgs)usingArgs, AbilityUsingStage.Preparing);
		}

		// Token: 0x06002094 RID: 8340 RVA: 0x00066B88 File Offset: 0x00064D88
		private void OnAbilityPrepared(IAbility ability, object usingArgs)
		{
			AbilityModifierUsingArgs usingArgs2;
			if (this.TryStartUsing(ability, usingArgs, AbilityUsingStage.Prepared, out usingArgs2))
			{
				this.UseModifiers<AbilityStatModifier>(usingArgs2);
			}
		}

		// Token: 0x06002095 RID: 8341 RVA: 0x00066BAC File Offset: 0x00064DAC
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			BaseAbility baseAbility = ability as BaseAbility;
			if (baseAbility == null || baseAbility.WasUsed)
			{
				return;
			}
			AbilityModifierUsingArgs modifiersUsingArgs;
			if (!baseAbility.IsProjectileAbility(false) && this.TryStartUsing(ability, usingArgs, AbilityUsingStage.Used, out modifiersUsingArgs))
			{
				this.UseNonStatModifiers(modifiersUsingArgs);
			}
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x00066BEC File Offset: 0x00064DEC
		private void OnAbilityProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs launchArgs)
		{
			ProjectileAbilityBase ability = launchArgs.ability;
			BaseAbility.UsingArgs abilityUsingArgs = launchArgs.abilityUsingArgs;
			if (launchArgs.shotIndex != 0)
			{
				this.TriggerActivator(ability, abilityUsingArgs, AbilityUsingStage.ProjectileLaunched);
			}
			AbilityModifierUsingArgs abilityModifierUsingArgs;
			if (this.TryStartUsing(ability, abilityUsingArgs, AbilityUsingStage.ProjectileLaunched, out abilityModifierUsingArgs))
			{
				this.preparedProjectilesInfo.Add(launchArgs.launchedProjectile.InstanceID, abilityModifierUsingArgs.modifiersUsingCount);
				AbilityModifiersController.VFXController vfxcontroller = this.modifiersVFX;
				if (vfxcontroller != null)
				{
					vfxcontroller.ModifyProjectileVisuals(launchArgs.launchedProjectile as BaseProjectile);
				}
				launchArgs.launchedProjectile.Destroyed += this.OnAbilityProjectileDestroyed;
				this.ResetActivator(ability, true);
			}
		}

		// Token: 0x06002097 RID: 8343 RVA: 0x00066C7C File Offset: 0x00064E7C
		private void OnBeforeAbilityProjectileHit(ProjectileAbilityBase.UsingEventArgs projectileUsingArgs)
		{
			int instanceID = projectileUsingArgs.projectileHitArgs.projectile.InstanceID;
			int usingCount;
			if (this.preparedProjectilesInfo.TryGetValue(instanceID, out usingCount))
			{
				AbilityModifierUsingArgs preparedProjectileModifiersArgs = this.GetPreparedProjectileModifiersArgs(projectileUsingArgs, usingCount);
				this.UseModifiers<AbilityStatModifier>(preparedProjectileModifiersArgs);
				this.UseNonStatModifiers(preparedProjectileModifiersArgs);
				return;
			}
			this.UseModifiersOnProjectileHit(projectileUsingArgs, false);
		}

		// Token: 0x06002098 RID: 8344 RVA: 0x00066CCA File Offset: 0x00064ECA
		private void OnAbilityProjectileHit(ProjectileAbilityBase.UsingEventArgs projectileUsingArgs)
		{
			this.UseModifiersOnProjectileHit(projectileUsingArgs, true);
		}

		// Token: 0x06002099 RID: 8345 RVA: 0x00066CD4 File Offset: 0x00064ED4
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.ResetActivator((BaseAbility)ability, false);
		}

		// Token: 0x0600209A RID: 8346 RVA: 0x00066CE3 File Offset: 0x00064EE3
		private void OnAbilityProjectileDestroyed(IProjectile projectile)
		{
			this.preparedProjectilesInfo.Remove(projectile.InstanceID);
			projectile.Destroyed -= this.OnAbilityProjectileDestroyed;
		}

		// Token: 0x0600209B RID: 8347 RVA: 0x00066D0C File Offset: 0x00064F0C
		private void OnTargetedModifierUsed(AbilityModifierUsingArgs modifierUsingArgs)
		{
			if (this.modifiersVFX == null)
			{
				return;
			}
			BaseAbility.UsingArgs targetsInfo = modifierUsingArgs.targetsInfo;
			targetsInfo.ProcessTargets(new Action<Component>(this.<OnTargetedModifierUsed>g__CreateAffectedTargetEffect|90_0));
			this.modifiersVFX.CreateUsingPointEffect(targetsInfo.targetPosition);
		}

		// Token: 0x0600209C RID: 8348 RVA: 0x00066D4D File Offset: 0x00064F4D
		private void OnSummoningModifierUsed(object summoner, IGameMob summonedMob, Vector2 summoningPoint)
		{
			AbilityModifiersController.VFXController vfxcontroller = this.modifiersVFX;
			if (vfxcontroller == null)
			{
				return;
			}
			vfxcontroller.CreateSummoningEffect(summoningPoint);
		}

		// Token: 0x0600209F RID: 8351 RVA: 0x00066E1B File Offset: 0x0006501B
		[CompilerGenerated]
		internal static bool <AddModifiers>g__AddModifier|61_0(List<AbilityModifierBase> list, AbilityModifierBase modifier, out AbilityModifierBase modifierInstance, ref AbilityModifiersController.<>c__DisplayClass61_0 A_3)
		{
			if (modifier.IsActive)
			{
				modifierInstance = modifier.Clone();
				list.Add(modifierInstance);
				modifierInstance.OnAddedToAbility(A_3.ability);
				return true;
			}
			modifierInstance = null;
			return false;
		}

		// Token: 0x060020A0 RID: 8352 RVA: 0x00066E48 File Offset: 0x00065048
		[CompilerGenerated]
		private BaseAbility.UsingArgs <UseTargetedModifiers>g__CollectTargets|74_0(Vector2 position, AbilityTargetsCollectionArgs targetsCollectionArgs)
		{
			this.targetedModifiersTargetsInfo.Reset();
			this.targetedModifiersTargetsInfo.targetPosition = position;
			AbilityTargetsCollector<Collider2D>.CollectTargets(targetsCollectionArgs, this.targetedModifiersTargetsInfo, ref AbilityModifiersController.targetsInRangeBuffer);
			return this.targetedModifiersTargetsInfo;
		}

		// Token: 0x060020A1 RID: 8353 RVA: 0x00066E7D File Offset: 0x0006507D
		[CompilerGenerated]
		private void <UseTargetedModifiers>g__UseModifiers|74_1(AbilityModifierUsingArgs usingArgs, BaseAbility.UsingArgs collectedTargetsInfo)
		{
			usingArgs.targetsInfo = collectedTargetsInfo;
			this.UseModifiers<AbilityEffectsBasedModifier>(usingArgs);
			this.UseModifiers<AbilityBuffsBasedModifier>(usingArgs);
		}

		// Token: 0x060020A2 RID: 8354 RVA: 0x00066E94 File Offset: 0x00065094
		[CompilerGenerated]
		private void <OnTargetedModifierUsed>g__CreateAffectedTargetEffect|90_0(Component target)
		{
			this.modifiersVFX.CreateAffectedTargetEffect(target);
		}

		// Token: 0x0400145E RID: 5214
		private static readonly AbilityModifierBase[] ModifiersQueriesBuffer = new AbilityModifierBase[8];

		// Token: 0x0400145F RID: 5215
		private static Component[] targetsInRangeBuffer = new Component[64];

		// Token: 0x04001462 RID: 5218
		[SerializeField]
		[FormerlySerializedAs("_activator")]
		private AbilityModifiersActivatorBase activator;

		// Token: 0x04001463 RID: 5219
		public int defaultModifiersUsingCount = 1;

		// Token: 0x04001464 RID: 5220
		public int maxModifiersUsingCount = -1;

		// Token: 0x04001465 RID: 5221
		public AbilityModifiersController.UsingTarget modifiersUsingTarget;

		// Token: 0x04001466 RID: 5222
		[Space]
		public float targetsCollectionRange = 5f;

		// Token: 0x04001467 RID: 5223
		public LayerMask targetsCollectionLayers;

		// Token: 0x04001468 RID: 5224
		[Tooltip("При значениях <= 0 будет использовано ограничение количества целей от абилити.")]
		public int maxTargetsCountOverride;

		// Token: 0x04001469 RID: 5225
		public bool getTargetsInAbilityRange;

		// Token: 0x0400146A RID: 5226
		public bool getTargetsWithAbilityLayers;

		// Token: 0x0400146B RID: 5227
		public bool forceGetTargetsAroundAbilityOwner;

		// Token: 0x0400146C RID: 5228
		public bool sortTargets = true;

		// Token: 0x0400146D RID: 5229
		[Space]
		public bool canBlockAbilityUsing;

		// Token: 0x0400146E RID: 5230
		[Space]
		[SerializeField]
		[FormerlySerializedAs("_statsModifiers")]
		private AbilityStatModifier[] statsModifiersPrototypes;

		// Token: 0x0400146F RID: 5231
		[SerializeField]
		[FormerlySerializedAs("_effectsBasedModifier")]
		private AbilityEffectsBasedModifier effectsBasedModifierPrototype;

		// Token: 0x04001470 RID: 5232
		[SerializeField]
		[FormerlySerializedAs("_buffsBasedModifier")]
		private AbilityBuffsBasedModifier buffsBasedModifierPrototype;

		// Token: 0x04001471 RID: 5233
		[SerializeField]
		[FormerlySerializedAs("_effectZoneSpawningModifier")]
		private AbilityEffectZoneSpawningModifier effectZoneSpawningModifierPrototype;

		// Token: 0x04001472 RID: 5234
		[SerializeField]
		[FormerlySerializedAs("_summoningBasedModifier")]
		private AbilitySummoningModifier summoningBasedModifierPrototype;

		// Token: 0x04001473 RID: 5235
		[SerializeField]
		private ExternalAbilitiesUsingModifier externalAbilitiesModifierPrototype;

		// Token: 0x04001474 RID: 5236
		[Space]
		[SerializeField]
		[FormerlySerializedAs("_modifiersVFX")]
		private AbilityModifiersController.VFXController modifiersVFX;

		// Token: 0x04001475 RID: 5237
		private readonly Dictionary<int, List<AbilityModifierBase>> abilitiesModifiers = new Dictionary<int, List<AbilityModifierBase>>(64);

		// Token: 0x04001476 RID: 5238
		private readonly Dictionary<int, int> preparedProjectilesInfo = new Dictionary<int, int>(64);

		// Token: 0x04001477 RID: 5239
		private readonly AbilityTargetsCollectionArgs modifiersTargetsCollectionArgs = new AbilityTargetsCollectionArgs();

		// Token: 0x04001478 RID: 5240
		private readonly BaseAbility.UsingArgs OwnerOnlyAbilityUsingArgs = new BaseAbility.UsingArgs();

		// Token: 0x04001479 RID: 5241
		private readonly BaseAbility.UsingArgs targetedModifiersTargetsInfo = new BaseAbility.UsingArgs();

		// Token: 0x0400147A RID: 5242
		private readonly BaseAbility.ActivationError abilityInterruptionError = new BaseAbility.ActivationError();

		// Token: 0x0400147B RID: 5243
		private readonly AbilityModifiersActivatorArgs activatorArgs = new AbilityModifiersActivatorArgs();

		// Token: 0x0400147C RID: 5244
		private readonly AbilityModifierUsingArgs defaultModifiersUsingArgs = new AbilityModifierUsingArgs();

		// Token: 0x0400147D RID: 5245
		private readonly AbilityModifierUsingArgs projectilesEffectModifiersUsingArgs = new AbilityModifierUsingArgs();

		// Token: 0x02000587 RID: 1415
		public enum UsingTarget
		{
			// Token: 0x04001CB7 RID: 7351
			Default,
			// Token: 0x04001CB8 RID: 7352
			AbilityOwner
		}

		// Token: 0x02000588 RID: 1416
		[Serializable]
		public sealed class VFXController
		{
			// Token: 0x17000808 RID: 2056
			// (get) Token: 0x06002779 RID: 10105 RVA: 0x0007B815 File Offset: 0x00079A15
			// (set) Token: 0x0600277A RID: 10106 RVA: 0x0007B81D File Offset: 0x00079A1D
			public IUnityObjectPool<ParticleSystem> EffectsPool
			{
				get
				{
					return this.effectsPool;
				}
				set
				{
					this.effectsPool = value;
				}
			}

			// Token: 0x0600277B RID: 10107 RVA: 0x0007B828 File Offset: 0x00079A28
			private ParticleSystem InstantiateEffect(GameObject prefab, Vector3 position)
			{
				if (prefab == null)
				{
					return null;
				}
				if (this.effectsPool != null)
				{
					AbilityModifiersController.VFXController.EffectsPoolArgs.unityObjectPrototype = prefab;
					AbilityModifiersController.VFXController.EffectsPoolArgs.position = new Vector3?(position);
					return this.effectsPool.TakeObject(AbilityModifiersController.VFXController.EffectsPoolArgs);
				}
				ParticleSystem particleSystem = prefab.InstantiateParticleSystem();
				particleSystem.transform.position = position;
				particleSystem.DestroyAfterEmission(false, true);
				return particleSystem;
			}

			// Token: 0x0600277C RID: 10108 RVA: 0x0007B88E File Offset: 0x00079A8E
			public ParticleSystem CreateUsingPointEffect(Vector3 usingPoint)
			{
				return this.InstantiateEffect(this.usingPointEffectPrefab, usingPoint);
			}

			// Token: 0x0600277D RID: 10109 RVA: 0x0007B89D File Offset: 0x00079A9D
			public ParticleSystem CreateAffectedTargetEffect(Vector3 targetPosition)
			{
				return this.InstantiateEffect(this.affectedTargetEffectPrefab, targetPosition);
			}

			// Token: 0x0600277E RID: 10110 RVA: 0x0007B8AC File Offset: 0x00079AAC
			public ParticleSystem CreateAffectedTargetEffect(Component affectedTarget)
			{
				if (affectedTarget != null)
				{
					if (!string.IsNullOrEmpty(this.targetEffectAttachmentPoint) && this.targetEffectAttachmentPoint != TaggedPivotGroup.UntaggedTagValue)
					{
						int? num = this.effectPivotTagHash;
						int num3;
						if (num == null)
						{
							int? num2 = this.effectPivotTagHash = new int?(TaggedPivotGroup.TagToHash(this.targetEffectAttachmentPoint));
							num3 = num2.Value;
						}
						else
						{
							num3 = num.GetValueOrDefault();
						}
						int pivotTagHash = num3;
						BaseGameMob baseGameMob = affectedTarget.CastOrGetComponent<BaseGameMob>();
						TaggedPivot taggedPivot;
						if (baseGameMob == null)
						{
							taggedPivot = null;
						}
						else
						{
							TaggedPivotGroup taggedPivotsGroup = baseGameMob.TaggedPivotsGroup;
							taggedPivot = ((taggedPivotsGroup != null) ? taggedPivotsGroup.GetPivot(pivotTagHash) : null);
						}
						TaggedPivot taggedPivot2 = taggedPivot;
						if (taggedPivot2 != null)
						{
							return this.CreateAffectedTargetEffect(taggedPivot2.GetWorldPosition(taggedPivot2.CurrentGroup.GroupTransform));
						}
					}
					return this.CreateAffectedTargetEffect(affectedTarget.transform.position);
				}
				return null;
			}

			// Token: 0x0600277F RID: 10111 RVA: 0x0007B970 File Offset: 0x00079B70
			public ParticleSystem CreateSummoningEffect(Vector3 summoningPoint)
			{
				return this.InstantiateEffect(this.summoningEffectPrefab, summoningPoint);
			}

			// Token: 0x06002780 RID: 10112 RVA: 0x0007B980 File Offset: 0x00079B80
			public void ModifyProjectileVisuals(BaseProjectile projectile)
			{
				Component component = ((projectile != null) ? projectile.AttachedObject : null) as Component;
				GameObject gameObject = (component != null) ? component.gameObject : null;
				if (gameObject == null)
				{
					return;
				}
				SpriteRenderer spriteRenderer;
				if ((this.modifyProjectileColor || this.modifiedProjectileSprite != null) && gameObject.TryGetComponent<SpriteRenderer>(out spriteRenderer))
				{
					if (this.modifyProjectileColor)
					{
						spriteRenderer.color = this.modifiedProjectileColor;
					}
					if (this.modifiedProjectileSprite != null)
					{
						spriteRenderer.sprite = this.modifiedProjectileSprite;
					}
				}
				ProjectileRendererComponent projectileRendererComponent = projectile.AttachedObject as ProjectileRendererComponent;
				if (projectileRendererComponent != null)
				{
					ParticleSystem particleSystem = this.additionalProjectileParticlesPrefab.InstantiateParticleSystem();
					if (particleSystem == null)
					{
						return;
					}
					Transform transform = particleSystem.transform;
					if (projectileRendererComponent.ParticleSystem == null)
					{
						projectileRendererComponent.ParticleSystem = particleSystem;
						transform.parent = projectileRendererComponent.transform;
					}
					else
					{
						transform.parent = projectileRendererComponent.ParticleSystem.transform;
					}
					transform.localPosition = default(Vector3);
					particleSystem.Play();
				}
			}

			// Token: 0x04001CB9 RID: 7353
			private static readonly UnityObjectPoolArgs EffectsPoolArgs = new UnityObjectPoolArgs();

			// Token: 0x04001CBA RID: 7354
			public GameObject usingPointEffectPrefab;

			// Token: 0x04001CBB RID: 7355
			public GameObject affectedTargetEffectPrefab;

			// Token: 0x04001CBC RID: 7356
			[Tag]
			public string targetEffectAttachmentPoint;

			// Token: 0x04001CBD RID: 7357
			public GameObject summoningEffectPrefab;

			// Token: 0x04001CBE RID: 7358
			[Space]
			public Sprite modifiedProjectileSprite;

			// Token: 0x04001CBF RID: 7359
			public GameObject additionalProjectileParticlesPrefab;

			// Token: 0x04001CC0 RID: 7360
			public Color modifiedProjectileColor;

			// Token: 0x04001CC1 RID: 7361
			public bool modifyProjectileColor;

			// Token: 0x04001CC2 RID: 7362
			private IUnityObjectPool<ParticleSystem> effectsPool;

			// Token: 0x04001CC3 RID: 7363
			private int? effectPivotTagHash;
		}
	}
}
