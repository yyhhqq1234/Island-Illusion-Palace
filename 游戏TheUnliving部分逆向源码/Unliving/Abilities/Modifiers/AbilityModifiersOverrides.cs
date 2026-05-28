using System;
using Game.Abilities;
using Unliving.AbilityResources;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C1 RID: 961
	public sealed class AbilityModifiersOverrides : IAbilityExtension
	{
		// Token: 0x170006A5 RID: 1701
		// (get) Token: 0x060020A3 RID: 8355 RVA: 0x00066EA3 File Offset: 0x000650A3
		BaseAbility IAbilityExtension.CurrentAbility
		{
			get
			{
				return this.currentAbility;
			}
		}

		// Token: 0x170006A6 RID: 1702
		// (get) Token: 0x060020A4 RID: 8356 RVA: 0x00066EAB File Offset: 0x000650AB
		bool IAbilityExtension.IsSharedExtension
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170006A7 RID: 1703
		// (get) Token: 0x060020A5 RID: 8357 RVA: 0x00066EAE File Offset: 0x000650AE
		bool IAbilityExtension.CanBeUsedAsMainBehaviour
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1400011F RID: 287
		// (add) Token: 0x060020A6 RID: 8358 RVA: 0x00066EB4 File Offset: 0x000650B4
		// (remove) Token: 0x060020A7 RID: 8359 RVA: 0x00066EEC File Offset: 0x000650EC
		public event Action<IAbility, IAbilityExtension> AddedToAbility;

		// Token: 0x14000120 RID: 288
		// (add) Token: 0x060020A8 RID: 8360 RVA: 0x00066F24 File Offset: 0x00065124
		// (remove) Token: 0x060020A9 RID: 8361 RVA: 0x00066F5C File Offset: 0x0006515C
		public event Action<IAbility, IAbilityExtension> RemovedFromAbility;

		// Token: 0x060020AA RID: 8362 RVA: 0x00066F91 File Offset: 0x00065191
		public AbilityModifiersOverrides()
		{
		}

		// Token: 0x060020AB RID: 8363 RVA: 0x00066F99 File Offset: 0x00065199
		public AbilityModifiersOverrides(AbilityModifiersOverrides overrides)
		{
			this.usingCountOverride = overrides.usingCountOverride;
			this.activationResourcesAddition = overrides.activationResourcesAddition;
		}

		// Token: 0x060020AC RID: 8364 RVA: 0x00066FB9 File Offset: 0x000651B9
		public void SetUsingCount(ref int usingCount)
		{
			if ((float)this.usingCountOverride > 0f)
			{
				usingCount = this.usingCountOverride;
			}
		}

		// Token: 0x060020AD RID: 8365 RVA: 0x00066FD1 File Offset: 0x000651D1
		public void ClampUsingCount(ref int usingCount)
		{
			if ((float)this.usingCountOverride > 0f && usingCount > this.usingCountOverride)
			{
				usingCount = this.usingCountOverride;
			}
		}

		// Token: 0x060020AE RID: 8366 RVA: 0x00066FF4 File Offset: 0x000651F4
		public void SetResourcesOverridesActive(bool isActive, AbilityResourcesCollector.RequiredResourceInfo[] resourcesAmounts)
		{
			if (this.activationResourcesAddition == 0)
			{
				return;
			}
			for (int i = 0; i < resourcesAmounts.Length; i++)
			{
				ref AbilityResourcesCollector.RequiredResourceInfo ptr = ref resourcesAmounts[i];
				if (isActive)
				{
					ptr.requiredAmount += this.activationResourcesAddition;
				}
				else
				{
					ptr.requiredAmount -= this.activationResourcesAddition;
				}
			}
		}

		// Token: 0x060020AF RID: 8367 RVA: 0x00067045 File Offset: 0x00065245
		public void SetResourcesOverridesActive(bool isActive, AbilityResourcesConsumer resourcesConsumer)
		{
			this.SetResourcesOverridesActive(isActive, resourcesConsumer.requiredResources);
		}

		// Token: 0x060020B0 RID: 8368 RVA: 0x00067054 File Offset: 0x00065254
		public void ApplyResourcesOverrides(ref AbilityResourcesBlock resourcesBlock)
		{
			resourcesBlock.Add(this.activationResourcesAddition, true);
		}

		// Token: 0x060020B1 RID: 8369 RVA: 0x00067063 File Offset: 0x00065263
		public void SetSummonedMobsLifetime(ref float mobsLifetime)
		{
			if (this.summonedMobsLifetimeOverride > 0f)
			{
				mobsLifetime = this.summonedMobsLifetimeOverride;
			}
		}

		// Token: 0x060020B2 RID: 8370 RVA: 0x0006707A File Offset: 0x0006527A
		bool IAbilityExtension.TryInstantiate(out IAbilityExtension extensionInstance)
		{
			extensionInstance = new AbilityModifiersOverrides(this);
			return true;
		}

		// Token: 0x060020B3 RID: 8371 RVA: 0x00067085 File Offset: 0x00065285
		void IAbilityExtension.Destroy()
		{
		}

		// Token: 0x060020B4 RID: 8372 RVA: 0x00067087 File Offset: 0x00065287
		void IAbilityExtension.OnAddedToAbility(BaseAbility ability)
		{
			this.currentAbility = ability;
			Action<IAbility, IAbilityExtension> addedToAbility = this.AddedToAbility;
			if (addedToAbility == null)
			{
				return;
			}
			addedToAbility(ability, this);
		}

		// Token: 0x060020B5 RID: 8373 RVA: 0x000670A2 File Offset: 0x000652A2
		void IAbilityExtension.OnRemovedFromAbility(BaseAbility ability)
		{
			Action<IAbility, IAbilityExtension> removedFromAbility = this.RemovedFromAbility;
			if (removedFromAbility != null)
			{
				removedFromAbility(ability, this);
			}
			this.currentAbility = null;
		}

		// Token: 0x04001480 RID: 5248
		public int usingCountOverride;

		// Token: 0x04001481 RID: 5249
		public int activationResourcesAddition;

		// Token: 0x04001482 RID: 5250
		public int levelOverride;

		// Token: 0x04001483 RID: 5251
		public float summonedMobsLifetimeOverride;

		// Token: 0x04001484 RID: 5252
		private BaseAbility currentAbility;
	}
}
