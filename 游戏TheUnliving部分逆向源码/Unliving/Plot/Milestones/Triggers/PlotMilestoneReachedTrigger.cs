using System;
using UnityEngine;
using Unliving.Plot.Triggers;

namespace Unliving.Plot.Milestones.Triggers
{
	// Token: 0x0200031A RID: 794
	[Serializable]
	public sealed class PlotMilestoneReachedTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x06001AA6 RID: 6822 RVA: 0x00053544 File Offset: 0x00051744
		protected override bool ShouldBeIgnored()
		{
			return string.IsNullOrEmpty(this.milestoneID);
		}

		// Token: 0x06001AA7 RID: 6823 RVA: 0x00053554 File Offset: 0x00051754
		protected override bool GetState(CharacterPlotContext context)
		{
			PlotMilestoneManager plotMilestoneManager;
			if (!context.currentGame.Services.TryGet<PlotMilestoneManager>(out plotMilestoneManager))
			{
				return false;
			}
			if (!plotMilestoneManager.HasMilestoneWithID(this.milestoneID))
			{
				Debug.LogError("Milestone with ID: " + this.milestoneID + " is missing. Have you spelled it correctly? Requested by " + context.characterID);
				return false;
			}
			return plotMilestoneManager.IsMilestoneReached(this.milestoneID);
		}

		// Token: 0x04000ED6 RID: 3798
		public string milestoneID;
	}
}
