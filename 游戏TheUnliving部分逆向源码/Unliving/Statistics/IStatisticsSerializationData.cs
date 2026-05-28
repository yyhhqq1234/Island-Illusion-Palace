using System;
using Common;

namespace Unliving.Statistics
{
	// Token: 0x02000125 RID: 293
	public interface IStatisticsSerializationData : ICloneable<IStatisticsSerializationData>
	{
		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000760 RID: 1888
		Type StatisticsType { get; }

		// Token: 0x06000761 RID: 1889
		IStatistics CreateInstance();
	}
}
