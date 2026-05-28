using System;
using Common;

namespace Unliving.Statistics
{
	// Token: 0x0200012F RID: 303
	public abstract class StatisticsSerializationDataBase : IStatisticsSerializationData, ICloneable<IStatisticsSerializationData>
	{
		// Token: 0x17000130 RID: 304
		// (get) Token: 0x060007B0 RID: 1968
		public abstract Type StatisticsType { get; }

		// Token: 0x060007B1 RID: 1969
		public abstract IStatistics CreateInstance();

		// Token: 0x060007B2 RID: 1970
		public abstract void Initialize();

		// Token: 0x060007B3 RID: 1971 RVA: 0x0001998C File Offset: 0x00017B8C
		public IStatisticsSerializationData Clone()
		{
			return this.Copy<StatisticsSerializationDataBase>();
		}
	}
}
