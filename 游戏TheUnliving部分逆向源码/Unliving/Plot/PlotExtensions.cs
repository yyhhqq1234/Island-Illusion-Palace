using System;

namespace Unliving.Plot
{
	// Token: 0x020002E3 RID: 739
	public static class PlotExtensions
	{
		// Token: 0x0600198E RID: 6542 RVA: 0x0005026D File Offset: 0x0004E46D
		private static bool IsSpeaker(CharacterPhrase phrase, string speakerID)
		{
			return string.Equals(phrase.ID, speakerID, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600198F RID: 6543 RVA: 0x0005027C File Offset: 0x0004E47C
		public static bool IsPlayerPhrase(this CharacterPhrase phrase)
		{
			return PlotExtensions.IsSpeaker(phrase, "player");
		}

		// Token: 0x06001990 RID: 6544 RVA: 0x00050289 File Offset: 0x0004E489
		public static bool IsNPCPhrase(this CharacterPhrase phrase)
		{
			return PlotExtensions.IsSpeaker(phrase, "npc");
		}

		// Token: 0x06001991 RID: 6545 RVA: 0x00050296 File Offset: 0x0004E496
		public static bool IsNarratorPhrase(this CharacterPhrase phrase)
		{
			return PlotExtensions.IsSpeaker(phrase, "narrator");
		}

		// Token: 0x04000E52 RID: 3666
		public const string PlayerSpeakerID = "player";

		// Token: 0x04000E53 RID: 3667
		public const string NPCSpeakerID = "npc";

		// Token: 0x04000E54 RID: 3668
		public const string NarratorSpeakerID = "narrator";
	}
}
