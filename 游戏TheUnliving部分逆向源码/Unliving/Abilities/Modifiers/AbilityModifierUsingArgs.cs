using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Stats;
using Unliving.AbilityResources;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003BF RID: 959
	public sealed class AbilityModifierUsingArgs
	{
		// Token: 0x06002068 RID: 8296 RVA: 0x000660F8 File Offset: 0x000642F8
		public AbilityModifiersOverrides GetModifiersOverrides()
		{
			BaseAbility baseAbility = this.targetAbility;
			if (baseAbility == null)
			{
				return null;
			}
			return baseAbility.GetExtension<AbilityModifiersOverrides>();
		}

		// Token: 0x06002069 RID: 8297 RVA: 0x0006610B File Offset: 0x0006430B
		public void CopyTargetsInfo(BaseAbility.UsingArgs abilityTargetsInfo)
		{
			abilityTargetsInfo.CopyValuesTo(AbilityModifierUsingArgs.SafeTargetsInfo);
			this.targetsInfo = AbilityModifierUsingArgs.SafeTargetsInfo;
		}

		// Token: 0x0600206A RID: 8298 RVA: 0x00066123 File Offset: 0x00064323
		public IList<CollectableAbilityResource> GetCollectedResources()
		{
			return this.additionalData as IList<CollectableAbilityResource>;
		}

		// Token: 0x0600206B RID: 8299 RVA: 0x00066130 File Offset: 0x00064330
		public bool TryGetStatModifier(MobStatID statID, out MobStatModifier statModifier)
		{
			StatsController statsController = this.modifiersStats;
			IModifiableStat<MobStatModifier> modifiableStat = (statsController != null) ? statsController.GetStat((int)statID) : null;
			statModifier = MobStatModifier.Neutral;
			MobStatBase mobStatBase = modifiableStat as MobStatBase;
			if (mobStatBase != null)
			{
				statModifier = mobStatBase.CurrentModifiers;
			}
			else
			{
				MobProxyStat mobProxyStat = modifiableStat as MobProxyStat;
				if (mobProxyStat != null)
				{
					statModifier = mobProxyStat.AppliedModifiers;
				}
			}
			return statModifier != MobStatModifier.Neutral;
		}

		// Token: 0x0600206C RID: 8300 RVA: 0x0006619A File Offset: 0x0006439A
		public void Reset()
		{
			this.targetAbility = null;
			this.targetsInfo = null;
			this.modifiersUsingCount = 0;
			this.additionalData = null;
			this.tryUseAtMultiplePositions = false;
		}

		// Token: 0x04001457 RID: 5207
		private static readonly BaseAbility.UsingArgs SafeTargetsInfo = new BaseAbility.UsingArgs();

		// Token: 0x04001458 RID: 5208
		public BaseAbility targetAbility;

		// Token: 0x04001459 RID: 5209
		public BaseAbility.UsingArgs targetsInfo;

		// Token: 0x0400145A RID: 5210
		public int modifiersUsingCount;

		// Token: 0x0400145B RID: 5211
		public object additionalData;

		// Token: 0x0400145C RID: 5212
		public bool tryUseAtMultiplePositions;

		// Token: 0x0400145D RID: 5213
		public StatsController modifiersStats;
	}
}
