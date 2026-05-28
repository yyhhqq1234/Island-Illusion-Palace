using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x0200022A RID: 554
	[CreateAssetMenu(fileName = "CoupledUsingAbilityTrigger", menuName = "Abilities/Triggers/Coupled Using Trigger")]
	public sealed class CoupledUsingAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x060012F5 RID: 4853 RVA: 0x0003C112 File Offset: 0x0003A312
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x060012F6 RID: 4854 RVA: 0x0003C115 File Offset: 0x0003A315
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x060012F7 RID: 4855 RVA: 0x0003C11C File Offset: 0x0003A31C
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060012F8 RID: 4856 RVA: 0x0003C11F File Offset: 0x0003A31F
		private void CountReloadedAbility(IAbility ability, bool add, bool force = false)
		{
			if (force || !ability.IsReloading())
			{
				this.reloadedAbilitiesCount += (add ? 1 : -1);
			}
		}

		// Token: 0x060012F9 RID: 4857 RVA: 0x0003C140 File Offset: 0x0003A340
		private void CountAbility(IAbility ability, bool add)
		{
			this.abilitiesWithTriggerCount += (add ? 1 : -1);
			this.CountReloadedAbility(ability, add, false);
		}

		// Token: 0x060012FA RID: 4858 RVA: 0x0003C15F File Offset: 0x0003A35F
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return ability.IsPrepInProgress() || this.reloadedAbilitiesCount == this.abilitiesWithTriggerCount;
		}

		// Token: 0x060012FB RID: 4859 RVA: 0x0003C179 File Offset: 0x0003A379
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.CountAbility(ability, true);
			ability.Used += this.OnAbilityUsed;
			ability.Reloaded += this.OnAbilityReloaded;
		}

		// Token: 0x060012FC RID: 4860 RVA: 0x0003C1B0 File Offset: 0x0003A3B0
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			this.CountAbility(ability, false);
			ability.Used -= this.OnAbilityUsed;
			ability.Reloaded -= this.OnAbilityReloaded;
			BaseGameMob baseGameMob = ability.OwnerBehaviour as BaseGameMob;
			if (baseGameMob != null)
			{
				baseGameMob.Killed -= this.OnMobKilled;
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x060012FD RID: 4861 RVA: 0x0003C211 File Offset: 0x0003A411
		protected override void OnAbilityOwnerChanged(BaseAbility ability, object lastOwner, object newOwner)
		{
			if (newOwner != null)
			{
				((BaseGameMob)newOwner).Killed += this.OnMobKilled;
				return;
			}
			((BaseGameMob)lastOwner).Killed -= this.OnMobKilled;
		}

		// Token: 0x060012FE RID: 4862 RVA: 0x0003C245 File Offset: 0x0003A445
		private void OnMobKilled(IGameMob mob)
		{
			BaseGameMob baseGameMob = mob as BaseGameMob;
			if (baseGameMob != null)
			{
				baseGameMob.AbilitiesController.RemoveAllAbilitiesExtenders<CoupledUsingAbilityTrigger>();
			}
			mob.Killed -= this.OnMobKilled;
		}

		// Token: 0x060012FF RID: 4863 RVA: 0x0003C26F File Offset: 0x0003A46F
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			if (((BaseAbility)ability).WasUsed)
			{
				return;
			}
			this.CountReloadedAbility(ability, false, true);
		}

		// Token: 0x06001300 RID: 4864 RVA: 0x0003C288 File Offset: 0x0003A488
		private void OnAbilityReloaded(BaseAbility ability)
		{
			this.CountReloadedAbility(ability, true, true);
		}

		// Token: 0x04000B1A RID: 2842
		[NonSerialized]
		private int abilitiesWithTriggerCount;

		// Token: 0x04000B1B RID: 2843
		[NonSerialized]
		private int reloadedAbilitiesCount;
	}
}
