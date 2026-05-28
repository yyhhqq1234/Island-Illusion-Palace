using System;

namespace Unliving.Plot
{
	// Token: 0x020002DC RID: 732
	public interface ICharacterPlot
	{
		// Token: 0x06001969 RID: 6505
		ICharactersConversation GetConversation(CharacterPlotContext context, ConversationBranch conversationBranch);

		// Token: 0x0600196A RID: 6506
		void CompleteConversation(ICharactersConversation conversation, ICharacterPlotProgress characterPlotProgress);
	}
}
