using System;
using System.Collections.Generic;
using Game.Stats;

namespace Unliving.MobsStats
{
	// Token: 0x02000072 RID: 114
	public sealed class StatsController : StatsControllerBase<MobStatModifier>
	{
		// Token: 0x0600032E RID: 814 RVA: 0x0000BA34 File Offset: 0x00009C34
		protected override void RegisterModifier(MobStatModifier modifier, List<MobStatModifier> modifiers)
		{
			if (modifiers.Count == 0)
			{
				modifiers.Add(modifier);
				return;
			}
			modifiers[0] = modifiers[0] + modifier;
		}

		// Token: 0x0600032F RID: 815 RVA: 0x0000BA5A File Offset: 0x00009C5A
		protected override bool UnregisterModifier(MobStatModifier modifier, List<MobStatModifier> modifiers)
		{
			if (modifiers.Count != 0)
			{
				modifiers[0] = modifiers[0] - modifier;
				return true;
			}
			return false;
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0000BA7B File Offset: 0x00009C7B
		public StatsController(object statsOwner, IEnumerable<IModifiableStat<MobStatModifier>> stats) : base(statsOwner, stats)
		{
		}

		// Token: 0x06000331 RID: 817 RVA: 0x0000BA88 File Offset: 0x00009C88
		public StatsController(object statsOwner, bool tryGetPropertyStats) : base(statsOwner, null)
		{
			if (tryGetPropertyStats)
			{
				IList<PropertyBasedStatsHelper.StatPropertyInfo> statPropertiesInfo = PropertyBasedStatsHelper.GetStatPropertiesInfo(statsOwner);
				if (statPropertiesInfo != null)
				{
					for (int i = 0; i < statPropertiesInfo.Count; i++)
					{
						PropertyBasedStatsHelper.StatPropertyInfo statPropertyInfo = statPropertiesInfo[i];
						if (statPropertyInfo.StatID != null)
						{
							base.AddStat(new PropertyBasedMobStat((MobStatID)statPropertyInfo.StatID.Value, statsOwner, statPropertyInfo.PropertyInfo));
						}
					}
				}
			}
		}
	}
}
