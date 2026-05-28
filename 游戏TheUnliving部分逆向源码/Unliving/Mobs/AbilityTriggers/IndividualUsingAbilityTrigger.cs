using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x0200022F RID: 559
	[CreateAssetMenu(fileName = "IndividualUsingAbilityTrigger", menuName = "Abilities/Triggers/Individual Using Trigger")]
	public sealed class IndividualUsingAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06001324 RID: 4900 RVA: 0x0003C6A9 File Offset: 0x0003A8A9
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06001325 RID: 4901 RVA: 0x0003C6AC File Offset: 0x0003A8AC
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06001326 RID: 4902 RVA: 0x0003C6B3 File Offset: 0x0003A8B3
		// (set) Token: 0x06001327 RID: 4903 RVA: 0x0003C6BB File Offset: 0x0003A8BB
		public float MaxActiveAbilitiesPercent
		{
			get
			{
				return this.maxActiveAbilitiesPercent;
			}
			set
			{
				this.maxActiveAbilitiesPercent = Mathf.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06001328 RID: 4904 RVA: 0x0003C6D3 File Offset: 0x0003A8D3
		public int CurrentAbilitiesCount
		{
			get
			{
				return this.currentAbilitiesCount;
			}
		}

		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06001329 RID: 4905 RVA: 0x0003C6DB File Offset: 0x0003A8DB
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600132A RID: 4906 RVA: 0x0003C6E0 File Offset: 0x0003A8E0
		private int GetMaxActiveOwnersCount()
		{
			if (this.maxActiveAbilitiesPercent > 0f)
			{
				int b = (int)((float)this.currentAbilitiesCount * this.maxActiveAbilitiesPercent * 0.01f);
				return Mathf.Max(Mathf.Max(this.maxActiveAbilitiesCount, b), 1);
			}
			return Mathf.Max(this.maxActiveAbilitiesCount, 1);
		}

		// Token: 0x0600132B RID: 4907 RVA: 0x0003C72F File Offset: 0x0003A92F
		private bool HasAvailableUseCount()
		{
			return this.isTriggerReloaded && this.activeAbilities.Count < this.GetMaxActiveOwnersCount();
		}

		// Token: 0x0600132C RID: 4908 RVA: 0x0003C74E File Offset: 0x0003A94E
		private bool InUse(IAbility ability)
		{
			return this.activeAbilities.Contains(AbilityExtensionAssetBase.GetAbilityInstanceID(ability));
		}

		// Token: 0x0600132D RID: 4909 RVA: 0x0003C764 File Offset: 0x0003A964
		private bool RemoveActiveOwner(IAbility ability)
		{
			if (this.activeAbilities.Remove(AbilityExtensionAssetBase.GetAbilityInstanceID(ability)))
			{
				if (!this.isTriggerReloaded && this.activeAbilities.Count == 0)
				{
					this.nextUsingTime = Time.time + Mathf.Max(this.usingCooldown, 0f);
					this.isTriggerReloaded = true;
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600132E RID: 4910 RVA: 0x0003C7C0 File Offset: 0x0003A9C0
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.currentAbilitiesCount++;
			if (!this.isInitialized)
			{
				this.activeAbilities = new List<int>(this.GetMaxActiveOwnersCount());
				this.isTriggerReloaded = true;
				this.isInitialized = true;
			}
			ability.Activating += this.OnAbilityPreparing;
		}

		// Token: 0x0600132F RID: 4911 RVA: 0x0003C81B File Offset: 0x0003AA1B
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			this.currentAbilitiesCount--;
			this.RemoveActiveOwner(ability);
			ability.Activating -= this.OnAbilityPreparing;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001330 RID: 4912 RVA: 0x0003C84C File Offset: 0x0003AA4C
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return (Time.time > this.nextUsingTime && this.HasAvailableUseCount()) || this.InUse(ability);
		}

		// Token: 0x06001331 RID: 4913 RVA: 0x0003C86C File Offset: 0x0003AA6C
		private void OnAbilityPreparing(IAbility ability, object usingArgs)
		{
			if (ability.PrepProgress == 0f && this.HasAvailableUseCount())
			{
				int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
				if (!this.activeAbilities.Contains(abilityInstanceID))
				{
					this.activeAbilities.Add(abilityInstanceID);
					if (this.activeAbilities.Count == this.GetMaxActiveOwnersCount())
					{
						this.isTriggerReloaded = false;
					}
				}
			}
		}

		// Token: 0x06001332 RID: 4914 RVA: 0x0003C8C9 File Offset: 0x0003AAC9
		protected override void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.RemoveActiveOwner(ability);
			base.OnAbilityCompleted(ability, usingArgs);
		}

		// Token: 0x06001333 RID: 4915 RVA: 0x0003C8DB File Offset: 0x0003AADB
		private void OnEnable()
		{
			this.nextUsingTime = 0f;
			this.isInitialized = false;
		}

		// Token: 0x04000B29 RID: 2857
		public float usingCooldown;

		// Token: 0x04000B2A RID: 2858
		public int maxActiveAbilitiesCount = 1;

		// Token: 0x04000B2B RID: 2859
		[SerializeField]
		[Range(0f, 100f)]
		private float maxActiveAbilitiesPercent;

		// Token: 0x04000B2C RID: 2860
		private int currentAbilitiesCount;

		// Token: 0x04000B2D RID: 2861
		private float nextUsingTime;

		// Token: 0x04000B2E RID: 2862
		private List<int> activeAbilities;

		// Token: 0x04000B2F RID: 2863
		private bool isTriggerReloaded;

		// Token: 0x04000B30 RID: 2864
		private bool isInitialized;
	}
}
