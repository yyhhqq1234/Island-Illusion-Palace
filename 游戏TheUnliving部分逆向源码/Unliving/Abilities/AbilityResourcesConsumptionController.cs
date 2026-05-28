using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities.Modifiers;
using Unliving.AbilityResources;
using Unliving.Mobs.AbilityTriggers;

namespace Unliving.Abilities
{
	// Token: 0x02000377 RID: 887
	[CreateAssetMenu(fileName = "AbilityResourcesConsumptionController", menuName = "Abilities/Controllers/Resources Consumption Controller")]
	public sealed class AbilityResourcesConsumptionController : AbilityExtensionAssetBase, IAbilityResourcesConsumer, IMobAbilityTrigger
	{
		// Token: 0x17000607 RID: 1543
		// (get) Token: 0x06001D27 RID: 7463 RVA: 0x0005C38B File Offset: 0x0005A58B
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000608 RID: 1544
		// (get) Token: 0x06001D28 RID: 7464 RVA: 0x0005C38E File Offset: 0x0005A58E
		// (set) Token: 0x06001D29 RID: 7465 RVA: 0x0005C396 File Offset: 0x0005A596
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

		// Token: 0x17000609 RID: 1545
		// (get) Token: 0x06001D2A RID: 7466 RVA: 0x0005C39F File Offset: 0x0005A59F
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

		// Token: 0x1700060A RID: 1546
		// (get) Token: 0x06001D2B RID: 7467 RVA: 0x0005C3B2 File Offset: 0x0005A5B2
		bool IMobAbilityTrigger.RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700060B RID: 1547
		// (get) Token: 0x06001D2C RID: 7468 RVA: 0x0005C3B5 File Offset: 0x0005A5B5
		float IMobAbilityTrigger.ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x06001D2D RID: 7469 RVA: 0x0005C3BC File Offset: 0x0005A5BC
		private AbilityResourcesCollector GetResourcesCollector(IAbility ability)
		{
			AbilityResourcesCollector result;
			this.resourcesCollectors.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out result);
			return result;
		}

		// Token: 0x06001D2E RID: 7470 RVA: 0x0005C3E0 File Offset: 0x0005A5E0
		public float GetResourcesCollectionRange(BaseAbility consumingAbility)
		{
			AbilityResourcesCollector resourcesCollector = this.GetResourcesCollector(consumingAbility);
			if (resourcesCollector == null)
			{
				return 0f;
			}
			return resourcesCollector.CollectionRange;
		}

		// Token: 0x06001D2F RID: 7471 RVA: 0x0005C404 File Offset: 0x0005A604
		public IReadOnlyList<CollectableAbilityResource> GetCollectedResources(object context)
		{
			IAbility ability = context as IAbility;
			if (ability != null)
			{
				return this.GetResourcesCollector(ability).LastCollectedResourcesInfo.Resources;
			}
			return null;
		}

		// Token: 0x06001D30 RID: 7472 RVA: 0x0005C42E File Offset: 0x0005A62E
		bool IMobAbilityTrigger.IsConditionReached(BaseAbility ability)
		{
			return this.resourcesConsumer.HasResourcesInRange(this.GetResourcesCollector(ability), null);
		}

		// Token: 0x06001D31 RID: 7473 RVA: 0x0005C443 File Offset: 0x0005A643
		bool IMobAbilityTrigger.IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return ((IMobAbilityTrigger)this).IsConditionReached(ability);
		}

		// Token: 0x06001D32 RID: 7474 RVA: 0x0005C44C File Offset: 0x0005A64C
		private void OnActivatingAbility(IAbility ability, object usingArgs)
		{
			if (ability.PrepProgress != 0f)
			{
				return;
			}
			BaseAbility baseAbility = (BaseAbility)ability;
			BaseAbility.UsingArgs usingArgs2 = (BaseAbility.UsingArgs)usingArgs;
			AbilityModifiersOverrides modifiersOverrides = baseAbility.GetModifiersOverrides();
			if (modifiersOverrides != null)
			{
				modifiersOverrides.SetResourcesOverridesActive(true, this.resourcesConsumer);
			}
			AbilityResourcesBlock lackOfResources;
			if (!this.resourcesConsumer.CollectResources(this.GetResourcesCollector(baseAbility), usingArgs2, 1, out lackOfResources))
			{
				this.abilityInterruptionError.Reset();
				this.abilityInterruptionError.type = BaseAbility.ActivationErrorType.External;
				this.abilityInterruptionError.usingArgs = usingArgs2;
				this.abilityInterruptionError.lackOfResources = lackOfResources;
				this.abilityInterruptionError.interruptionSource = this;
				baseAbility.TryInterruptActivation(this.abilityInterruptionError);
			}
			if (modifiersOverrides == null)
			{
				return;
			}
			modifiersOverrides.SetResourcesOverridesActive(false, this.resourcesConsumer);
		}

		// Token: 0x06001D33 RID: 7475 RVA: 0x0005C4FE File Offset: 0x0005A6FE
		private void OnAbilityActivated(IAbility ability, object usingArgs)
		{
			this.resourcesConsumer.StartResourcesConsumptionMotion(this.GetResourcesCollector(ability), 0f);
		}

		// Token: 0x06001D34 RID: 7476 RVA: 0x0005C518 File Offset: 0x0005A718
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			AbilityResourcesCollector resourcesCollector = this.GetResourcesCollector(ability);
			this.resourcesConsumer.ConsumeResources(resourcesCollector);
			resourcesCollector.ReleaseCollectedResources();
		}

		// Token: 0x06001D35 RID: 7477 RVA: 0x0005C540 File Offset: 0x0005A740
		public override void OnAddedToAbility(BaseAbility ability)
		{
			this.resourcesCollectors.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), this.resourcesConsumer.CreateResourcesCollector(ability));
			ability.Activating += this.OnActivatingAbility;
			ability.Activated += this.OnAbilityActivated;
			ability.Used += this.OnAbilityUsed;
			base.OnAddedToAbility(ability);
		}

		// Token: 0x06001D36 RID: 7478 RVA: 0x0005C5A8 File Offset: 0x0005A7A8
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			base.OnRemovedFromAbility(ability);
			this.resourcesCollectors.Remove(AbilityExtensionAssetBase.GetAbilityInstanceID(ability));
			ability.Activating -= this.OnActivatingAbility;
			ability.Activated -= this.OnAbilityActivated;
			ability.Used -= this.OnAbilityUsed;
		}

		// Token: 0x06001D37 RID: 7479 RVA: 0x0005C604 File Offset: 0x0005A804
		private void OnValidate()
		{
			this.resourcesConsumer.SyncCollectionLayers(this);
		}

		// Token: 0x0400107F RID: 4223
		[SerializeField]
		private AbilityResourcesConsumer resourcesConsumer;

		// Token: 0x04001080 RID: 4224
		private readonly Dictionary<int, AbilityResourcesCollector> resourcesCollectors = new Dictionary<int, AbilityResourcesCollector>(32);

		// Token: 0x04001081 RID: 4225
		private readonly LackOfResourcesAbilityActivationError abilityInterruptionError = new LackOfResourcesAbilityActivationError();
	}
}
