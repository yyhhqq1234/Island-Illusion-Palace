using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200036E RID: 878
	[CreateAssetMenu(fileName = "AbilityGlobalCooldownController", menuName = "Abilities/Controllers/Global Cooldown Controller")]
	public sealed class AbilityGlobalCooldownController : AbilityExtensionAssetBase, IAbilityUpdateNotifiable
	{
		// Token: 0x06001CE6 RID: 7398 RVA: 0x0005B4F4 File Offset: 0x000596F4
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.Completed += this.OnAbilityCompleted;
			ability.AddUpdateListener(this);
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x0005B516 File Offset: 0x00059716
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Completed -= this.OnAbilityCompleted;
			ability.RemoveUpdateListener(this);
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x0005B538 File Offset: 0x00059738
		protected override void OnAbilityOwnerChanged(BaseAbility currentAbility, object lastOwner, object newOwner)
		{
			this.abilityOwner = (newOwner as BaseGameMob);
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x0005B546 File Offset: 0x00059746
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.isCooldownActive = this.currentAbility.WasUsed;
			this.endTime = (this.isCooldownActive ? (Time.time + this.ownerFreezeCooldown) : -1f);
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x0005B57A File Offset: 0x0005977A
		void IAbilityUpdateNotifiable.OnAbilityUpdated(BaseAbility ability, BaseAbility.UsingArgs currentUsingArgs)
		{
			if (this.isCooldownActive)
			{
				this.isCooldownActive = (Time.time < this.endTime);
				BaseGameMob baseGameMob = this.abilityOwner;
				if (baseGameMob == null)
				{
					return;
				}
				baseGameMob.BlockMovement(0f);
			}
		}

		// Token: 0x0400105B RID: 4187
		public float ownerFreezeCooldown = 5f;

		// Token: 0x0400105C RID: 4188
		private BaseGameMob abilityOwner;

		// Token: 0x0400105D RID: 4189
		private float endTime;

		// Token: 0x0400105E RID: 4190
		private bool isCooldownActive;
	}
}
