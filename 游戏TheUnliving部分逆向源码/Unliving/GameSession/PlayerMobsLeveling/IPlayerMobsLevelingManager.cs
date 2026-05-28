using System;
using System.Collections.Generic;

namespace Unliving.GameSession.PlayerMobsLeveling
{
	// Token: 0x020002AE RID: 686
	public interface IPlayerMobsLevelingManager
	{
		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x06001802 RID: 6146
		int CurrentMaxMobLevel { get; }

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x06001803 RID: 6147
		int TotalPlayerArmyPower { get; }

		// Token: 0x140000E8 RID: 232
		// (add) Token: 0x06001804 RID: 6148
		// (remove) Token: 0x06001805 RID: 6149
		event Action<IPlayerMobsLevelingManager> RegisteredMobsCollectionChanged;

		// Token: 0x140000E9 RID: 233
		// (add) Token: 0x06001806 RID: 6150
		// (remove) Token: 0x06001807 RID: 6151
		event Action<IPlayerMobsLevelingManager, ILevelablePlayerMob> PlayerMobLevelAdvanced;

		// Token: 0x140000EA RID: 234
		// (add) Token: 0x06001808 RID: 6152
		// (remove) Token: 0x06001809 RID: 6153
		event Action<IPlayerMobsLevelingManager, int> TotalPlayerArmyPowerChanged;

		// Token: 0x0600180A RID: 6154
		IReadOnlyList<ILevelablePlayerMob> GetMobsWithLevel(int mobsLevel);
	}
}
