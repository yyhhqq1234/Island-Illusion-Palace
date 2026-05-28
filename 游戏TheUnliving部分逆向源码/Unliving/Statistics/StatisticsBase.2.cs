using System;

namespace Unliving.Statistics
{
	// Token: 0x0200012E RID: 302
	public abstract class StatisticsBase<TData> : StatisticsBase where TData : IStatisticsSerializationData
	{
		// Token: 0x060007AD RID: 1965 RVA: 0x00019943 File Offset: 0x00017B43
		public override IStatisticsSerializationData GetSerializationData()
		{
			return this.data;
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x00019950 File Offset: 0x00017B50
		public override void SetSerializationData(IStatisticsSerializationData serializationData)
		{
			if (serializationData is TData)
			{
				TData tdata = (TData)((object)serializationData);
				this.data = (TData)((object)tdata.Clone());
			}
		}

		// Token: 0x04000473 RID: 1139
		public TData data;
	}
}
