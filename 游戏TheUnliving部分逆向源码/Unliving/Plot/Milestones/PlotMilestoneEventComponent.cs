using System;
using UltEvents;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000314 RID: 788
	public class PlotMilestoneEventComponent : PlotMilestoneComponentBase<MilestoneEventsData>
	{
		// Token: 0x06001A7E RID: 6782 RVA: 0x00052BE8 File Offset: 0x00050DE8
		protected override void InitializeMilestonesData()
		{
			for (int i = 0; i < this.milestones.Length; i++)
			{
				MilestoneEventsData milestone = this.milestones[i];
				milestone.activationEvent = delegate()
				{
					UltEvent milestoneEvents = milestone.milestoneEvents;
					if (milestoneEvents == null)
					{
						return;
					}
					milestoneEvents.Invoke();
				};
			}
		}
	}
}
