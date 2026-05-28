using System;
using Game.Core;
using UnityEngine;

namespace Unliving.Plot.Milestones
{
	// Token: 0x0200030E RID: 782
	public class PlotMilestoneActivatorComponent : GameBehaviourBase
	{
		// Token: 0x06001A6E RID: 6766 RVA: 0x00052770 File Offset: 0x00050970
		public void Start()
		{
			if (this.activateOnStart)
			{
				this.ReachMilestone();
			}
		}

		// Token: 0x06001A6F RID: 6767 RVA: 0x00052780 File Offset: 0x00050980
		public void ReachMilestone()
		{
			if (base.CurrentGame.Services.TryGet<PlotMilestoneManager>(out this.plotMilestoneManager))
			{
				this.plotMilestoneManager.SetMilestoneReached(this.milestoneID);
			}
		}

		// Token: 0x04000EAC RID: 3756
		[SerializeField]
		private string milestoneID;

		// Token: 0x04000EAD RID: 3757
		[SerializeField]
		private bool activateOnStart = true;

		// Token: 0x04000EAE RID: 3758
		private PlotMilestoneManager plotMilestoneManager;
	}
}
