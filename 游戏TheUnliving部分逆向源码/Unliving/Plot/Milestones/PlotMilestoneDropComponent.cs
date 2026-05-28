using System;
using Unliving.DropSystem;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000312 RID: 786
	public class PlotMilestoneDropComponent : PlotMilestoneComponentBase<MilestoneDropData>
	{
		// Token: 0x06001A7B RID: 6779 RVA: 0x00052B44 File Offset: 0x00050D44
		protected override void InitializeMilestonesData()
		{
			if (base.TryGetComponent<DropSpawner>(out this.dropSpawner))
			{
				for (int i = 0; i < this.milestones.Length; i++)
				{
					MilestoneDropData milestone = this.milestones[i];
					if (milestone.settings == MilestoneDropData.DropItemsSettings.AddItems)
					{
						milestone.activationEvent = delegate()
						{
							this.dropSpawner.dropItemsPool.AddPoolItems(milestone.dropables);
						};
					}
					else if (milestone.settings == MilestoneDropData.DropItemsSettings.OverrideItems)
					{
						milestone.activationEvent = delegate()
						{
							this.dropSpawner.dropItemsPool.ReplacePoolItems(milestone.dropables);
						};
					}
				}
			}
		}

		// Token: 0x04000EBD RID: 3773
		private DropSpawner dropSpawner;
	}
}
