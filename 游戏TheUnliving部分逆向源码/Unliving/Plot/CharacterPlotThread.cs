using System;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002D8 RID: 728
	[Serializable]
	public sealed class CharacterPlotThread : CharacterPlotItemsPool<CharactersConversation>
	{
		// Token: 0x06001953 RID: 6483 RVA: 0x0004FC78 File Offset: 0x0004DE78
		public CharacterPlotThread(CharactersConversation[] items, CharacterPlotItemTriggerBase trigger) : base(items, trigger)
		{
		}

		// Token: 0x04000E42 RID: 3650
		public bool isSingleActivationAttemptThread;
	}
}
