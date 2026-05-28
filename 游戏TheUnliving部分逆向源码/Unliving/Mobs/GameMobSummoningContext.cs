using System;
using Game.Abilities;
using Unliving.MobsStats;

namespace Unliving.Mobs
{
	// Token: 0x020001D5 RID: 469
	public sealed class GameMobSummoningContext
	{
		// Token: 0x06000EFF RID: 3839 RVA: 0x0002FAB5 File Offset: 0x0002DCB5
		public GameMobSummoningContext()
		{
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x0002FABD File Offset: 0x0002DCBD
		public GameMobSummoningContext(BaseAbility summoningAbility)
		{
			this.summoner = (IGameMob)summoningAbility.Owner;
			this.summoningSource = summoningAbility;
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x0002FADD File Offset: 0x0002DCDD
		public GameMobsGroupControllerBase GetSummonerGroup()
		{
			return this.summoner.Group ?? this.summoner.LastGroup;
		}

		// Token: 0x06000F02 RID: 3842 RVA: 0x0002FAF9 File Offset: 0x0002DCF9
		public override string ToString()
		{
			return string.Format("(summoner: {0}, summoningSource: {1})", this.summoner, this.summoningSource);
		}

		// Token: 0x040008DE RID: 2270
		public IGameMob summoner;

		// Token: 0x040008DF RID: 2271
		public object summoningSource;

		// Token: 0x040008E0 RID: 2272
		public int abilitiesLevelOverride;

		// Token: 0x040008E1 RID: 2273
		public ValueTuple<MobStatID, MobStatModifier>[] statsModifiers;
	}
}
