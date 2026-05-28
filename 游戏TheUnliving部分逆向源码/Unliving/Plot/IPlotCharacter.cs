using System;
using System.Collections.Generic;

namespace Unliving.Plot
{
	// Token: 0x020002E1 RID: 737
	public interface IPlotCharacter
	{
		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x0600197B RID: 6523
		string CharacterID { get; }

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x0600197C RID: 6524
		IReadOnlyList<ConversationBranch> AvailableConversationBranches { get; }

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x0600197D RID: 6525
		ICharacterPlot CharacterPlot { get; }

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x0600197E RID: 6526
		// (set) Token: 0x0600197F RID: 6527
		ICharactersConversation PreparedConversation { get; set; }

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x06001980 RID: 6528
		ICharacterPlotProgress PlotProgressOverride { get; }
	}
}
