using System;
using UnityEngine;
using Unliving.Plot.Milestones;

namespace Unliving.Purchasing
{
	// Token: 0x02000110 RID: 272
	public class PlotMilestoneReachedTrigger : PurchasableUnlockTriggerBase
	{
		// Token: 0x0600069F RID: 1695 RVA: 0x00015C40 File Offset: 0x00013E40
		public override bool IsFired(Context context)
		{
			if (context == null)
			{
				return false;
			}
			PlotMilestoneManager plotMilestoneManager;
			if (!context.game.Services.TryGet<PlotMilestoneManager>(out plotMilestoneManager))
			{
				return false;
			}
			if (!plotMilestoneManager.HasMilestoneWithID(this.milestoneID))
			{
				Debug.LogError("Milestone with ID: " + this.milestoneID + " is missing. Have you spelled it correctly? Requested by " + context.requester);
				return false;
			}
			return plotMilestoneManager.IsMilestoneReached(this.milestoneID);
		}

		// Token: 0x0400040C RID: 1036
		public string milestoneID;
	}
}
