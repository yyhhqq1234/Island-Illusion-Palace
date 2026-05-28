using System;
using Game.ObjectPool;

namespace Unliving.ObjectPools
{
	// Token: 0x020001AB RID: 427
	[Serializable]
	public sealed class ObjectPoolInitializationParams
	{
		// Token: 0x040006F6 RID: 1782
		public bool isPoolActive = true;

		// Token: 0x040006F7 RID: 1783
		public PoolableUnityObjectData[] localObjectPoolsData;
	}
}
