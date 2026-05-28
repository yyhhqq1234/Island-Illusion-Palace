using System;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002E7 RID: 743
	[Serializable]
	public sealed class AlwaysFalseTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x0600199C RID: 6556 RVA: 0x0005047F File Offset: 0x0004E67F
		protected override bool GetState(CharacterPlotContext context)
		{
			return false;
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x00050482 File Offset: 0x0004E682
		protected override bool ShouldBeIgnored()
		{
			return false;
		}
	}
}
