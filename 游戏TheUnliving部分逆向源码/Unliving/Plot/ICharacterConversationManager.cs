using System;

namespace Unliving.Plot
{
	// Token: 0x020002DB RID: 731
	public interface ICharacterConversationManager
	{
		// Token: 0x140000FB RID: 251
		// (add) Token: 0x0600195F RID: 6495
		// (remove) Token: 0x06001960 RID: 6496
		event Action<IPlotCharacter, ICharactersConversation, Action> ConversationPreActivated;

		// Token: 0x140000FC RID: 252
		// (add) Token: 0x06001961 RID: 6497
		// (remove) Token: 0x06001962 RID: 6498
		event Action<IPlotCharacter, ICharactersConversation> ConversationStarted;

		// Token: 0x140000FD RID: 253
		// (add) Token: 0x06001963 RID: 6499
		// (remove) Token: 0x06001964 RID: 6500
		event Action<IPlotCharacter, ICharactersConversation> ConversationCompleted;

		// Token: 0x06001965 RID: 6501
		CharacterMetadata GetActualCharacterMetadata(IPlotCharacter character, string defaultCharacterID);

		// Token: 0x06001966 RID: 6502
		void PrepareConversation(IPlotCharacter character);

		// Token: 0x06001967 RID: 6503
		ICharactersConversation StartConversation(IPlotCharacter character);

		// Token: 0x06001968 RID: 6504
		void CompleteConversation(IPlotCharacter character, ICharactersConversation conversation);
	}
}
