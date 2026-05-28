using System;
using Common;
using Game.Core;

namespace Unliving.Statistics
{
	// Token: 0x02000124 RID: 292
	public interface IStatistics : ICloneable<IStatistics>
	{
		// Token: 0x0600075B RID: 1883
		void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider);

		// Token: 0x0600075C RID: 1884
		void SetGameData(IGame game, PlayerStatisticsManager.Data data);

		// Token: 0x0600075D RID: 1885
		void Destroy();

		// Token: 0x0600075E RID: 1886
		IStatisticsSerializationData GetSerializationData();

		// Token: 0x0600075F RID: 1887
		void SetSerializationData(IStatisticsSerializationData data);
	}
}
