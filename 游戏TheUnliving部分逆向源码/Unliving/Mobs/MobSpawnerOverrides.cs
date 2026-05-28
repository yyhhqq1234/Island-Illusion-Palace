using System;
using Game.Factories;

namespace Unliving.Mobs
{
	// Token: 0x020001C7 RID: 455
	[Serializable]
	public sealed class MobSpawnerOverrides
	{
		// Token: 0x06000E54 RID: 3668 RVA: 0x0002D830 File Offset: 0x0002BA30
		public void Use(MobBehaviourSpawner targetSpawner)
		{
			if (!this.overrideSpawnerParams)
			{
				return;
			}
			ObjectSpawnerBase<MobBehaviour.ID, MobBehaviour>.BaseSpawningInfoItem[] spawningInfo = this.mobsInfo;
			targetSpawner.SpawningInfo = spawningInfo;
			targetSpawner.TrySetSpawningCounts(this.minMobsCount, this.maxMobsCount);
		}

		// Token: 0x04000879 RID: 2169
		public bool overrideSpawnerParams;

		// Token: 0x0400087A RID: 2170
		public MobBehaviourSpawner.MobSpawningInfoItem[] mobsInfo;

		// Token: 0x0400087B RID: 2171
		public int minMobsCount;

		// Token: 0x0400087C RID: 2172
		public int maxMobsCount;
	}
}
