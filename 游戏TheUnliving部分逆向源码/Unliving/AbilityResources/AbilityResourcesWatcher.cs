using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Utility;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.AbilityResources
{
	// Token: 0x0200035B RID: 859
	public sealed class AbilityResourcesWatcher : AreaWatcher<CollectableAbilityResource>
	{
		// Token: 0x170005CA RID: 1482
		// (get) Token: 0x06001BFB RID: 7163 RVA: 0x000585D0 File Offset: 0x000567D0
		// (set) Token: 0x06001BFC RID: 7164 RVA: 0x000585D8 File Offset: 0x000567D8
		public IAbilitiesController AbilitiesSource
		{
			get
			{
				return this.abilitiesSource;
			}
			set
			{
				if (this.abilitiesSource == value)
				{
					return;
				}
				if (this.abilitiesSource != null)
				{
					for (int i = 0; i < this.resourcesCounters.Count; i++)
					{
						Action<AbilityResourcesWatcher, AbilityResourcesCounter> resourcesCounterRemoved = this.ResourcesCounterRemoved;
						if (resourcesCounterRemoved != null)
						{
							resourcesCounterRemoved(this, this.resourcesCounters[i]);
						}
					}
					this.resourcesCounters.Clear();
					this.abilitiesSource.AbilityAdded -= this.OnAbilityAdded;
					this.abilitiesSource.AbilityRemoved -= this.OnAbilityRemoved;
				}
				if (value != null)
				{
					IReadOnlyList<IAbility> abilities = value.Abilities;
					for (int j = 0; j < abilities.Count; j++)
					{
						this.AddResourcesCounter((BaseAbility)abilities[j]);
					}
					this.UpdateActivity();
					this.SetRangeDirty();
					value.AbilityAdded += this.OnAbilityAdded;
					value.AbilityRemoved += this.OnAbilityRemoved;
				}
				this.abilitiesSource = value;
			}
		}

		// Token: 0x170005CB RID: 1483
		// (get) Token: 0x06001BFD RID: 7165 RVA: 0x000586C8 File Offset: 0x000568C8
		public IReadOnlyList<AbilityResourcesCounter> ResourcesCounters
		{
			get
			{
				return this.resourcesCounters;
			}
		}

		// Token: 0x1400010E RID: 270
		// (add) Token: 0x06001BFE RID: 7166 RVA: 0x000586D0 File Offset: 0x000568D0
		// (remove) Token: 0x06001BFF RID: 7167 RVA: 0x00058708 File Offset: 0x00056908
		public event Action<AbilityResourcesWatcher, AbilityResourcesCounter> ResourcesCounterAdded;

		// Token: 0x1400010F RID: 271
		// (add) Token: 0x06001C00 RID: 7168 RVA: 0x00058740 File Offset: 0x00056940
		// (remove) Token: 0x06001C01 RID: 7169 RVA: 0x00058778 File Offset: 0x00056978
		public event Action<AbilityResourcesWatcher, AbilityResourcesCounter> ResourcesCounterRemoved;

		// Token: 0x06001C02 RID: 7170 RVA: 0x000587B0 File Offset: 0x000569B0
		private void AddResourcesCounter(BaseAbility ability)
		{
			AbilityResourcesCounter abilityResourcesCounter = AbilityResourcesCounter.CreateFromAbilityActivator(ability);
			if (abilityResourcesCounter != null)
			{
				this.resourcesCounters.Add(abilityResourcesCounter);
				Action<AbilityResourcesWatcher, AbilityResourcesCounter> resourcesCounterAdded = this.ResourcesCounterAdded;
				if (resourcesCounterAdded != null)
				{
					resourcesCounterAdded(this, abilityResourcesCounter);
				}
				this.SetRangeDirty();
			}
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x000587EC File Offset: 0x000569EC
		private void RemoveResourcesCounter(BaseAbility ability)
		{
			for (int i = this.resourcesCounters.Count - 1; i >= 0; i--)
			{
				AbilityResourcesCounter abilityResourcesCounter = this.resourcesCounters[i];
				if (abilityResourcesCounter.Ability == ability)
				{
					Action<AbilityResourcesWatcher, AbilityResourcesCounter> resourcesCounterRemoved = this.ResourcesCounterRemoved;
					if (resourcesCounterRemoved != null)
					{
						resourcesCounterRemoved(this, abilityResourcesCounter);
					}
					this.resourcesCounters.RemoveAt(i);
					this.SetRangeDirty();
					return;
				}
			}
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x00058854 File Offset: 0x00056A54
		public AbilityResourcesCounter GetAbilityResourcesCounter(int abilityID)
		{
			for (int i = this.resourcesCounters.Count - 1; i >= 0; i--)
			{
				AbilityResourcesCounter abilityResourcesCounter = this.resourcesCounters[i];
				if (abilityResourcesCounter.Ability.ID == abilityID)
				{
					return abilityResourcesCounter;
				}
			}
			return null;
		}

		// Token: 0x06001C05 RID: 7173 RVA: 0x00058897 File Offset: 0x00056A97
		private void SetRangeDirty()
		{
			this.isRangeDirty = true;
		}

		// Token: 0x06001C06 RID: 7174 RVA: 0x000588A0 File Offset: 0x00056AA0
		private void SetResourcesDirty()
		{
			this.isResourcesDirty = true;
		}

		// Token: 0x06001C07 RID: 7175 RVA: 0x000588AC File Offset: 0x00056AAC
		private void UpdateRange()
		{
			if (!this.isRangeDirty)
			{
				return;
			}
			IAbilitiesController abilitiesController = this.abilitiesSource;
			IReadOnlyList<IAbility> readOnlyList = (abilitiesController != null) ? abilitiesController.Abilities : null;
			if (readOnlyList != null)
			{
				float num = 0f;
				for (int i = 0; i < readOnlyList.Count; i++)
				{
					BaseAbility baseAbility = (BaseAbility)readOnlyList[i];
					AbilityResourcesBlock abilityResourcesBlock;
					IAbilityResourcesConsumer abilityResourcesConsumer;
					if (baseAbility.TryGetActivationResources(out abilityResourcesBlock, out abilityResourcesConsumer, true))
					{
						float resourcesCollectionRange = abilityResourcesConsumer.GetResourcesCollectionRange(baseAbility);
						if (resourcesCollectionRange > num)
						{
							num = resourcesCollectionRange;
						}
					}
				}
				if (num != 0f && base.Range != num)
				{
					base.SetRange(num);
				}
			}
			this.isRangeDirty = false;
		}

		// Token: 0x06001C08 RID: 7176 RVA: 0x0005893C File Offset: 0x00056B3C
		private void ResetResourcesCounters(bool setCountersUpdated)
		{
			for (int i = 0; i < this.resourcesCounters.Count; i++)
			{
				this.resourcesCounters[i].Reset(setCountersUpdated);
			}
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x00058974 File Offset: 0x00056B74
		private void UpdateResourcesCounters()
		{
			if (!this.isResourcesDirty)
			{
				return;
			}
			IReadOnlyList<CollectableAbilityResource> objectsInRange = base.ObjectsInRange;
			int count = objectsInRange.Count;
			Vector2 value = base.transform.position;
			this.ResetResourcesCounters(false);
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < this.resourcesCounters.Count; j++)
				{
					this.resourcesCounters[j].Update(objectsInRange[i], 1, new Vector2?(value), false);
				}
			}
			for (int k = 0; k < this.resourcesCounters.Count; k++)
			{
				this.resourcesCounters[k].ApplyChanges();
			}
			this.isResourcesDirty = false;
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x00058A2A File Offset: 0x00056C2A
		private void UpdateActivity()
		{
			base.enabled = (this.resourcesCounters.Count != 0);
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x00058A40 File Offset: 0x00056C40
		private void OnAbilityAdded(IAbility ability)
		{
			this.AddResourcesCounter((BaseAbility)ability);
			this.UpdateActivity();
		}

		// Token: 0x06001C0C RID: 7180 RVA: 0x00058A54 File Offset: 0x00056C54
		private void OnAbilityRemoved(IAbility ability)
		{
			this.RemoveResourcesCounter((BaseAbility)ability);
			this.UpdateActivity();
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x00058A68 File Offset: 0x00056C68
		private void Reset()
		{
			this.isContinuousObjectValidationEnabled = false;
			base.UpdateRate = 2f;
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x00058A7C File Offset: 0x00056C7C
		protected override void OnObjectEnteredArea(CollectableAbilityResource collectableAbilityResource)
		{
			this.SetResourcesDirty();
			base.OnObjectEnteredArea(collectableAbilityResource);
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x00058A8B File Offset: 0x00056C8B
		protected override void OnObjectExitedArea(CollectableAbilityResource collectableAbilityResource)
		{
			this.SetResourcesDirty();
			base.OnObjectExitedArea(collectableAbilityResource);
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x00058A9A File Offset: 0x00056C9A
		protected override void OnDeferredUpdatePerformed()
		{
			this.UpdateRange();
			this.UpdateResourcesCounters();
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x00058AA8 File Offset: 0x00056CA8
		private void OnEnable()
		{
			if (this._areaCollider != null)
			{
				this._areaCollider.enabled = true;
			}
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x00058AC4 File Offset: 0x00056CC4
		protected override void OnDisable()
		{
			this.ResetResourcesCounters(true);
			this._areaCollider.enabled = false;
			base.OnDisable();
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x00058ADF File Offset: 0x00056CDF
		private void OnDestroy()
		{
			this.AbilitiesSource = null;
			this.resourcesCounters.TrimExcess();
		}

		// Token: 0x04000FD8 RID: 4056
		private readonly List<AbilityResourcesCounter> resourcesCounters = new List<AbilityResourcesCounter>(8);

		// Token: 0x04000FD9 RID: 4057
		private IAbilitiesController abilitiesSource;

		// Token: 0x04000FDA RID: 4058
		private bool isRangeDirty;

		// Token: 0x04000FDB RID: 4059
		private bool isResourcesDirty;
	}
}
