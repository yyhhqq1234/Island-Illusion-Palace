using System;

namespace Unliving.Statistics
{
	// Token: 0x02000126 RID: 294
	public interface IStoreStatisticsProvider
	{
		// Token: 0x06000762 RID: 1890
		bool SetStatValue(string statID, int value);

		// Token: 0x06000763 RID: 1891
		void ClearAchievementsData();

		// Token: 0x06000764 RID: 1892
		void ClearStatisticsData();
	}
}
