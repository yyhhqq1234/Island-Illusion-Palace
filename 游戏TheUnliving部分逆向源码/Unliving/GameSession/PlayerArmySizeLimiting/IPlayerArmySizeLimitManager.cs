using System;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.GameSession.PlayerArmySizeLimiting
{
	// Token: 0x020002C0 RID: 704
	public interface IPlayerArmySizeLimitManager
	{
		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x06001867 RID: 6247
		// (set) Token: 0x06001868 RID: 6248
		bool IsActive { get; set; }

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x06001869 RID: 6249
		// (set) Token: 0x0600186A RID: 6250
		int MaxPlayerArmySize { get; set; }

		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x0600186B RID: 6251
		int CurrentPlayerArmySize { get; }

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x0600186C RID: 6252
		// (set) Token: 0x0600186D RID: 6253
		float MobStatsGainCoeff { get; set; }

		// Token: 0x140000EF RID: 239
		// (add) Token: 0x0600186E RID: 6254
		// (remove) Token: 0x0600186F RID: 6255
		event Action<IPlayerArmySizeLimitManager, BaseGameMob, BaseGameMob> PlayerMobConsumed;

		// Token: 0x06001870 RID: 6256
		void ModifyMaxArmySize(MobStatModifier modifier);

		// Token: 0x06001871 RID: 6257
		void ModifyMobsGainCoeff(MobStatModifier modifier);
	}
}
