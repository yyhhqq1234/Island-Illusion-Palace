using System;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000319 RID: 793
	public class PlotMilestoneInEditorValidator
	{
		// Token: 0x06001AA5 RID: 6821 RVA: 0x00053541 File Offset: 0x00051741
		public bool IsMilestoneIDValid(string milestoneID)
		{
			return true;
		}

		// Token: 0x04000ED4 RID: 3796
		private const string pathToMilestoneGraph = "Assets/Dialogues/_PlotMilestoneManagerGraph.asset";

		// Token: 0x04000ED5 RID: 3797
		private PlotMilestoneManagerGraph mGraph;
	}
}
