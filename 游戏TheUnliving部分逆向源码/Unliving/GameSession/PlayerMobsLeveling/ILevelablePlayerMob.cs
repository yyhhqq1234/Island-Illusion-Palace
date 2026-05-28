using System;
using Game.Buffs;
using Unliving.Mobs;

namespace Unliving.GameSession.PlayerMobsLeveling
{
	// Token: 0x020002AD RID: 685
	public interface ILevelablePlayerMob : IGameMob, IBuffableObject
	{
		// Token: 0x140000E7 RID: 231
		// (add) Token: 0x060017FD RID: 6141
		// (remove) Token: 0x060017FE RID: 6142
		event Action<int> MobLevelChanged;

		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x060017FF RID: 6143
		int MobType { get; }

		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x06001800 RID: 6144
		// (set) Token: 0x06001801 RID: 6145
		int MobLevel { get; set; }
	}
}
