using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;
using Unliving.AbilityResources;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003CD RID: 973
	[CreateAssetMenu(fileName = "ResourcesBasedAbilityModifiersActivator", menuName = "Abilities/Modifiers Activators/Resources Based Activator")]
	public sealed class ResourcesBasedAbilityModifiersActivator : AbilityModifiersActivatorBase<AbilityResourcesCollector>, IAbilityResourcesConsumer
	{
		// Token: 0x170006AE RID: 1710
		// (get) Token: 0x06002102 RID: 8450 RVA: 0x00067AD7 File Offset: 0x00065CD7
		// (set) Token: 0x06002103 RID: 8451 RVA: 0x00067ADF File Offset: 0x00065CDF
		public AbilityResourcesConsumer ResourcesConsumer
		{
			get
			{
				return this.resourcesConsumer;
			}
			set
			{
				this.resourcesConsumer = value;
			}
		}

		// Token: 0x170006AF RID: 1711
		// (get) Token: 0x06002104 RID: 8452 RVA: 0x00067AE8 File Offset: 0x00065CE8
		AbilityResourcesCollector.RequiredResourceInfo[] IAbilityResourcesConsumer.RequiredResources
		{
			get
			{
				AbilityResourcesConsumer abilityResourcesConsumer = this.resourcesConsumer;
				if (abilityResourcesConsumer == null)
				{
					return null;
				}
				return abilityResourcesConsumer.requiredResources;
			}
		}

		// Token: 0x06002105 RID: 8453 RVA: 0x00067AFB File Offset: 0x00065CFB
		private void StartResourcesConsumptionMotion(in AbilityResourcesCollector.CollectedResourcesInfo collectedResources, float durationOverride = 0f)
		{
			if (!this.treatResourcesAsUsingPoints)
			{
				this.resourcesConsumer.StartResourcesConsumptionMotion(collectedResources, durationOverride);
			}
		}

		// Token: 0x06002106 RID: 8454 RVA: 0x00067B12 File Offset: 0x00065D12
		protected override AbilityResourcesCollector CreateTrigger(BaseAbility ability)
		{
			return this.resourcesConsumer.CreateResourcesCollector(ability);
		}

		// Token: 0x06002107 RID: 8455 RVA: 0x00067B20 File Offset: 0x00065D20
		protected override bool RemoveTrigger(BaseAbility ability, out AbilityResourcesCollector resourcesCollector)
		{
			if (base.RemoveTrigger(ability, out resourcesCollector))
			{
				resourcesCollector.resourcesFilter = null;
				return true;
			}
			return false;
		}

		// Token: 0x06002108 RID: 8456 RVA: 0x00067B38 File Offset: 0x00065D38
		protected override bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			int maxUsingCount = this.maxModifiersUsingCount;
			AbilityModifiersOverrides overrides = args.overrides;
			if (overrides != null)
			{
				overrides.ClampUsingCount(ref maxUsingCount);
			}
			if (overrides != null)
			{
				overrides.SetResourcesOverridesActive(true, this.resourcesConsumer);
			}
			AbilityResourcesCollector trigger = base.GetTrigger(args.ability);
			AbilityResourcesBlock lackOfResources;
			bool flag = this.resourcesConsumer.CollectResources(trigger, args.abilityUsingArgs, maxUsingCount, out lackOfResources);
			if (overrides != null)
			{
				overrides.SetResourcesOverridesActive(false, this.resourcesConsumer);
			}
			if (!flag)
			{
				ResourcesBasedAbilityModifiersActivator.AbilityActivationError.Reset();
				ResourcesBasedAbilityModifiersActivator.AbilityActivationError.ability = args.ability;
				ResourcesBasedAbilityModifiersActivator.AbilityActivationError.type = BaseAbility.ActivationErrorType.External;
				ResourcesBasedAbilityModifiersActivator.AbilityActivationError.usingArgs = args.abilityUsingArgs;
				ResourcesBasedAbilityModifiersActivator.AbilityActivationError.lackOfResources = lackOfResources;
				ResourcesBasedAbilityModifiersActivator.AbilityActivationError.interruptionSource = this;
				abilityActivationError = ResourcesBasedAbilityModifiersActivator.AbilityActivationError;
			}
			return flag;
		}

		// Token: 0x06002109 RID: 8457 RVA: 0x00067BF8 File Offset: 0x00065DF8
		public float GetResourcesCollectionRange(BaseAbility ability)
		{
			AbilityResourcesCollector trigger = base.GetTrigger(ability);
			if (trigger == null)
			{
				return 0f;
			}
			return trigger.CollectionRange;
		}

		// Token: 0x0600210A RID: 8458 RVA: 0x00067C1C File Offset: 0x00065E1C
		IReadOnlyList<CollectableAbilityResource> IAbilityResourcesConsumer.GetCollectedResources(object context)
		{
			IAbility ability = context as IAbility;
			if (ability != null)
			{
				return base.GetTrigger(ability).LastCollectedResourcesInfo.Resources;
			}
			return null;
		}

		// Token: 0x0600210B RID: 8459 RVA: 0x00067C48 File Offset: 0x00065E48
		public override bool TryUse(AbilityModifiersActivatorArgs args, out AbilityModifierUsingArgs modifiersUsingArgs)
		{
			BaseAbility ability = args.ability;
			modifiersUsingArgs = base.InitializeModifiersUsingArgs();
			AbilityResourcesCollector.CollectedResourcesInfo lastCollectedResourcesInfo = base.GetTrigger(ability).LastCollectedResourcesInfo;
			if (lastCollectedResourcesInfo.IsCollected())
			{
				args.CopyCommonValues(modifiersUsingArgs);
				modifiersUsingArgs.modifiersUsingCount = lastCollectedResourcesInfo.UsingCount;
				modifiersUsingArgs.additionalData = lastCollectedResourcesInfo.Resources;
				modifiersUsingArgs.tryUseAtMultiplePositions = this.treatResourcesAsUsingPoints;
				AbilityUsingStage abilityUsingStage = args.abilityUsingStage;
				if (abilityUsingStage <= AbilityUsingStage.Prepared)
				{
					this.StartResourcesConsumptionMotion(lastCollectedResourcesInfo, 0f);
				}
				else
				{
					this.resourcesConsumer.ConsumeResources(lastCollectedResourcesInfo);
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600210C RID: 8460 RVA: 0x00067CD3 File Offset: 0x00065ED3
		public override void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false)
		{
			base.GetTrigger(ability).ReleaseCollectedResources();
		}

		// Token: 0x0600210D RID: 8461 RVA: 0x00067CE1 File Offset: 0x00065EE1
		private void OnValidate()
		{
			this.resourcesConsumer.SyncCollectionLayers(this);
		}

		// Token: 0x040014A7 RID: 5287
		private static readonly LackOfResourcesAbilityActivationError AbilityActivationError = new LackOfResourcesAbilityActivationError();

		// Token: 0x040014A8 RID: 5288
		[SerializeField]
		private AbilityResourcesConsumer resourcesConsumer;

		// Token: 0x040014A9 RID: 5289
		public int maxModifiersUsingCount;

		// Token: 0x040014AA RID: 5290
		public bool treatResourcesAsUsingPoints;

		// Token: 0x0200058D RID: 1421
		public enum CollectionPositionSource
		{
			// Token: 0x04001CD0 RID: 7376
			AbilityOwnerPosition,
			// Token: 0x04001CD1 RID: 7377
			AbilityUsingPoint
		}
	}
}
