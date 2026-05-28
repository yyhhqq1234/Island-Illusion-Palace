using System;
using UnityEngine;
using Unliving.MobsStats;

namespace Unliving.Mobs
{
	// Token: 0x020001CC RID: 460
	public sealed class FakeMobInstantiationArgs
	{
		// Token: 0x04000889 RID: 2185
		public GameObject mobPrefab;

		// Token: 0x0400088A RID: 2186
		public UnityEngine.Object owner;

		// Token: 0x0400088B RID: 2187
		public FakeMobBehaviour.AbilityDescription[] abilitiesOverride;

		// Token: 0x0400088C RID: 2188
		public ValueTuple<MobStatID, MobStatModifier>[] statsModifiers;

		// Token: 0x0400088D RID: 2189
		public int abilitiesLevelOverride = -1;

		// Token: 0x0400088E RID: 2190
		public GameMobsGroupControllerBase group;

		// Token: 0x0400088F RID: 2191
		public Vector3 position;

		// Token: 0x04000890 RID: 2192
		public bool activateOnStart = true;
	}
}
