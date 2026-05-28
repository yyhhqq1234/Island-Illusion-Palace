using System;
using System.Collections.Generic;

namespace Unliving.Plot.Milestones
{
	// Token: 0x0200030D RID: 781
	public interface IPlotMilestoneManager
	{
		// Token: 0x14000101 RID: 257
		// (add) Token: 0x06001A66 RID: 6758
		// (remove) Token: 0x06001A67 RID: 6759
		event Action<PlotMilestoneNode> MilestoneReached;

		// Token: 0x1700058B RID: 1419
		// (get) Token: 0x06001A68 RID: 6760
		IReadOnlyList<PlotMilestoneNode> Milestones { get; }

		// Token: 0x1700058C RID: 1420
		// (get) Token: 0x06001A69 RID: 6761
		IReadOnlyList<string> ReachedMilestones { get; }

		// Token: 0x1700058D RID: 1421
		// (get) Token: 0x06001A6A RID: 6762
		IReadOnlyList<string> CurrentGameSessionReachedMilestones { get; }

		// Token: 0x06001A6B RID: 6763
		bool IsMilestoneReached(string milestoneID);

		// Token: 0x06001A6C RID: 6764
		void CheckMilestoneState(string milestoneID, out PlotMilestoneNode milestoneNode);

		// Token: 0x06001A6D RID: 6765
		IReadOnlyList<string> GetAchievementMilestones(bool onlyNotReached);
	}
}
