using System;
using System.Collections.Generic;
using Game.Abilities;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C7 RID: 967
	public abstract class AbilityModifiersActivatorBase<TTrigger> : AbilityModifiersActivatorBase
	{
		// Token: 0x060020DE RID: 8414 RVA: 0x000676E0 File Offset: 0x000658E0
		protected TTrigger GetTrigger(IAbility ability)
		{
			TTrigger result;
			this.triggers.TryGetValue(AbilityModifiersActivatorBase.GetAbilityInstanceID(ability), out result);
			return result;
		}

		// Token: 0x060020DF RID: 8415 RVA: 0x00067704 File Offset: 0x00065904
		protected TTrigger SetTrigger(BaseAbility ability, TTrigger value)
		{
			this.triggers[AbilityModifiersActivatorBase.GetAbilityInstanceID(ability)] = value;
			return value;
		}

		// Token: 0x060020E0 RID: 8416
		protected abstract TTrigger CreateTrigger(BaseAbility ability);

		// Token: 0x060020E1 RID: 8417 RVA: 0x00067728 File Offset: 0x00065928
		protected virtual bool RemoveTrigger(BaseAbility ability, out TTrigger removedTrigger)
		{
			int abilityInstanceID = AbilityModifiersActivatorBase.GetAbilityInstanceID(ability);
			if (this.triggers.TryGetValue(abilityInstanceID, out removedTrigger))
			{
				this.triggers.Remove(abilityInstanceID);
				return true;
			}
			return false;
		}

		// Token: 0x060020E2 RID: 8418 RVA: 0x0006775B File Offset: 0x0006595B
		protected sealed override void OnAbilityRegistered(BaseAbility ability)
		{
			this.SetTrigger(ability, this.CreateTrigger(ability));
		}

		// Token: 0x060020E3 RID: 8419 RVA: 0x0006776C File Offset: 0x0006596C
		protected sealed override void OnAbilityUnregistered(BaseAbility ability)
		{
			TTrigger ttrigger;
			this.RemoveTrigger(ability, out ttrigger);
		}

		// Token: 0x0400149D RID: 5277
		private readonly Dictionary<int, TTrigger> triggers = new Dictionary<int, TTrigger>(32);
	}
}
