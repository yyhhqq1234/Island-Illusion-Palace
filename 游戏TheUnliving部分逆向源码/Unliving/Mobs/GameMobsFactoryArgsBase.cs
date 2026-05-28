using System;
using Common.Factories;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001BE RID: 446
	public abstract class GameMobsFactoryArgsBase : IBaseObjectDescription
	{
		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000DAC RID: 3500
		// (set) Token: 0x06000DAD RID: 3501
		public abstract int ObjectID { get; set; }

		// Token: 0x040007F4 RID: 2036
		public GameObject arbitraryMobPrefab;

		// Token: 0x040007F5 RID: 2037
		public Vector3 spawnPosition;

		// Token: 0x040007F6 RID: 2038
		public GameMobFactions mobFaction;

		// Token: 0x040007F7 RID: 2039
		public GameMobSpawningInfo spawnerInfo;

		// Token: 0x040007F8 RID: 2040
		public float hitPointsAmountOverride;

		// Token: 0x040007F9 RID: 2041
		public bool isEnvironmentMob;
	}
}
