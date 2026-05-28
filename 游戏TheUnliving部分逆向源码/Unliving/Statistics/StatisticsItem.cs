using System;

namespace Unliving.Statistics
{
	// Token: 0x02000130 RID: 304
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class StatisticsItem : Attribute
	{
		// Token: 0x060007B5 RID: 1973 RVA: 0x0001999C File Offset: 0x00017B9C
		public StatisticsItem(params string[] statisticsIDs)
		{
			this.statisticsIDs = statisticsIDs;
		}

		// Token: 0x04000474 RID: 1140
		public readonly string[] statisticsIDs;
	}
}
