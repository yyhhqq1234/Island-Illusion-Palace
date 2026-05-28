using System;
using Game.LevelGeneration;

namespace Unliving.Mobs
{
	// Token: 0x020001C2 RID: 450
	public interface IGameMobsSpawner
	{
		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06000DBB RID: 3515
		IGameMobsFactory MobsFactory { get; }

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06000DBC RID: 3516
		bool SpawnEnvironmentMobs { get; }

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06000DBD RID: 3517
		GameMobFactions GroupOwner { get; }

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06000DBE RID: 3518
		int RemainingSpawningCount { get; }

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000DBF RID: 3519
		ILocationChunk SpawningChunk { get; }

		// Token: 0x06000DC0 RID: 3520
		BaseGameMob SpawnMob(int mobID);
	}
}
