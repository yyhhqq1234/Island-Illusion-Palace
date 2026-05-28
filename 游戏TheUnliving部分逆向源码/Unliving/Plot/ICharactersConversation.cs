using System;
using System.Collections.Generic;
using Common;

namespace Unliving.Plot
{
	// Token: 0x020002E0 RID: 736
	public interface ICharactersConversation : ICharacterPlotItem, IWeighted
	{
		// Token: 0x140000FE RID: 254
		// (add) Token: 0x06001976 RID: 6518
		// (remove) Token: 0x06001977 RID: 6519
		event Action<ICharactersConversation, ICharacterPlotItem> PhraseStarted;

		// Token: 0x140000FF RID: 255
		// (add) Token: 0x06001978 RID: 6520
		// (remove) Token: 0x06001979 RID: 6521
		event Action<ICharactersConversation, ICharacterPlotItem> PhraseCompleted;

		// Token: 0x0600197A RID: 6522
		IEnumerator<ICharacterPlotItem> GetConversationIterator();
	}
}
