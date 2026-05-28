using System;
using System.Collections.Generic;
using Unliving.MobsStats;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002B4 RID: 692
	public interface IStatBasedPlayerLevelReward : IPlayerLevelReward, ICloneable
	{
		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x06001829 RID: 6185
		IReadOnlyList<TargetedMobStatModifier> StatModifiers { get; }
	}
}
