using System;
using Common;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002DD RID: 733
	public interface ICharacterPlotItem : IWeighted
	{
		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x0600196B RID: 6507
		string ID { get; }

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x0600196C RID: 6508
		int Priority { get; }

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x0600196D RID: 6509
		ConversationBranch ConversationBranch { get; }

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x0600196E RID: 6510
		CharacterPlotItemTriggerBase Trigger { get; }

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x0600196F RID: 6511
		CharacterPlotItemTriggerBase DeactivationTrigger { get; }

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x06001970 RID: 6512
		// (set) Token: 0x06001971 RID: 6513
		CharacterPlotItemRuntimeData RuntimeData { get; set; }
	}
}
