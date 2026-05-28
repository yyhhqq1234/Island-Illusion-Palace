using System;
using System.Collections.Generic;
using Common.Factories;

namespace Unliving.Mobs
{
	// Token: 0x020001C1 RID: 449
	public interface IGameMobsFactory : IObjectFactory<IGameMob>, IFactory<IBaseObjectDescription, IGameMob>, IFactory
	{
		// Token: 0x06000DB7 RID: 3511
		GameMobFactionInfo GetFactionInfo(GameMobFactions faction);

		// Token: 0x06000DB8 RID: 3512
		IEnumerable<GameMobFactionInfo> GetEnemyFactionsInfo(int mobLayer);

		// Token: 0x06000DB9 RID: 3513
		IGameMob Create(GameMobsFactoryArgsBase args);

		// Token: 0x06000DBA RID: 3514
		IGameMob SummonMob(GameMobSummoningContext context, GameMobsGroupControllerBase group, GameMobsFactoryArgsBase factoryArgs, float lifetime = -1f, bool canBeHealed = false);
	}
}
