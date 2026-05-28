using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.AbilityResources
{
	// Token: 0x02000358 RID: 856
	public sealed class AbilityResourcesCounter
	{
		// Token: 0x06001BD6 RID: 7126 RVA: 0x00057E3C File Offset: 0x0005603C
		public static AbilityResourcesCounter CreateFromAbilityActivator(BaseAbility ability)
		{
			AbilityResourcesBlock abilityResourcesBlock;
			IAbilityResourcesConsumer abilityResourcesConsumer;
			if (ability.TryGetActivationResources(out abilityResourcesBlock, out abilityResourcesConsumer, true))
			{
				float resourcesCollectionRange = abilityResourcesConsumer.GetResourcesCollectionRange(ability);
				return new AbilityResourcesCounter(ability, abilityResourcesBlock, resourcesCollectionRange);
			}
			return null;
		}

		// Token: 0x170005C6 RID: 1478
		// (get) Token: 0x06001BD7 RID: 7127 RVA: 0x00057E68 File Offset: 0x00056068
		public AbilityResourcesBlock RequiredResources
		{
			get
			{
				return this.requiredResources;
			}
		}

		// Token: 0x170005C7 RID: 1479
		// (get) Token: 0x06001BD8 RID: 7128 RVA: 0x00057E70 File Offset: 0x00056070
		public AbilityResourcesBlock CollectedResources
		{
			get
			{
				return this.collectedResources;
			}
		}

		// Token: 0x1400010C RID: 268
		// (add) Token: 0x06001BD9 RID: 7129 RVA: 0x00057E78 File Offset: 0x00056078
		// (remove) Token: 0x06001BDA RID: 7130 RVA: 0x00057EB0 File Offset: 0x000560B0
		public event Action<AbilityResourcesCounter> Updated;

		// Token: 0x06001BDB RID: 7131 RVA: 0x00057EE5 File Offset: 0x000560E5
		private void SetDirty(bool force)
		{
			if (force)
			{
				Action<AbilityResourcesCounter> updated = this.Updated;
				if (updated != null)
				{
					updated(this);
				}
				this.isDirty = false;
				return;
			}
			this.isDirty = true;
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x00057F0B File Offset: 0x0005610B
		public AbilityResourcesCounter(BaseAbility ability, AbilityResourcesBlock requiredResources, float resourcesCollectionRange)
		{
			this.Ability = ability;
			this.ResourcesCollectionRange = resourcesCollectionRange;
			this.requiredResources = requiredResources;
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x00057F28 File Offset: 0x00056128
		public void Reset(bool setUpdated = true)
		{
			this.collectedResources.ResetAmounts();
			this.SetDirty(setUpdated);
		}

		// Token: 0x06001BDE RID: 7134 RVA: 0x00057F3C File Offset: 0x0005613C
		public void Update(AbilityResourceType resourceType, int amount, bool setUpdated = true)
		{
			if (amount == 0)
			{
				return;
			}
			this.collectedResources.Add(resourceType, amount, false);
			this.SetDirty(setUpdated);
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x00057F58 File Offset: 0x00056158
		public void Update(CollectableAbilityResource abilityResource, int amount = 1, Vector2? consumerPosition = null, bool setUpdated = true)
		{
			if (!abilityResource.gameObject.activeInHierarchy)
			{
				return;
			}
			if (amount > 0 && consumerPosition != null && (consumerPosition.Value - abilityResource.Collider.ClosestPoint(consumerPosition.Value)).SqrMagnitude() > this.ResourcesCollectionRange * this.ResourcesCollectionRange)
			{
				return;
			}
			this.collectedResources.Add(abilityResource.type, amount, false);
			this.SetDirty(setUpdated);
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x00057FD1 File Offset: 0x000561D1
		public void Update(ref AbilityResourcesBlock newCollectedResources, bool setUpdated = true)
		{
			if (this.collectedResources.Equals(ref newCollectedResources))
			{
				return;
			}
			this.collectedResources = newCollectedResources;
			this.SetDirty(setUpdated);
		}

		// Token: 0x06001BE1 RID: 7137 RVA: 0x00057FF5 File Offset: 0x000561F5
		public void ApplyChanges()
		{
			if (!this.isDirty)
			{
				return;
			}
			Action<AbilityResourcesCounter> updated = this.Updated;
			if (updated != null)
			{
				updated(this);
			}
			this.isDirty = false;
		}

		// Token: 0x06001BE2 RID: 7138 RVA: 0x00058019 File Offset: 0x00056219
		public float GetRequiredResourcesEstimation()
		{
			return this.collectedResources.GetRequiredAmountsEstimation(ref this.requiredResources);
		}

		// Token: 0x04000FC4 RID: 4036
		public readonly BaseAbility Ability;

		// Token: 0x04000FC5 RID: 4037
		public readonly float ResourcesCollectionRange;

		// Token: 0x04000FC6 RID: 4038
		private AbilityResourcesBlock requiredResources;

		// Token: 0x04000FC7 RID: 4039
		private AbilityResourcesBlock collectedResources;

		// Token: 0x04000FC8 RID: 4040
		private bool isDirty;
	}
}
