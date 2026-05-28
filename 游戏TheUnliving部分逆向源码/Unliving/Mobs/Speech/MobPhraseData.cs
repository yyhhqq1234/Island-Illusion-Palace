using System;

namespace Unliving.Mobs.Speech
{
	// Token: 0x02000207 RID: 519
	public sealed class MobPhraseData
	{
		// Token: 0x06001190 RID: 4496 RVA: 0x000374B4 File Offset: 0x000356B4
		public MobPhraseData(MobBehaviour.ID mobID, string localizationKey)
		{
			this.MobID = mobID;
			this.LocalizationKey = localizationKey;
		}

		// Token: 0x04000A0B RID: 2571
		public readonly MobBehaviour.ID MobID;

		// Token: 0x04000A0C RID: 2572
		public readonly string LocalizationKey;
	}
}
