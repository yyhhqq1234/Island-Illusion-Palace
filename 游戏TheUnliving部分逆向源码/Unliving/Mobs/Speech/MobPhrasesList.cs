using System;
using UnityEngine;

namespace Unliving.Mobs.Speech
{
	// Token: 0x02000209 RID: 521
	[Serializable]
	public sealed class MobPhrasesList
	{
		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x06001191 RID: 4497 RVA: 0x000374CA File Offset: 0x000356CA
		public bool IsEmpty
		{
			get
			{
				return this.phrases == null || this.phrases.Length == 0;
			}
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x000374E0 File Offset: 0x000356E0
		public bool TryGetRandomPhrase(out MobPhrasesList.MobPhrase phrase)
		{
			if (!this.IsEmpty)
			{
				phrase = this.phrases[UnityEngine.Random.Range(0, this.phrases.Length)];
				return true;
			}
			phrase = default(MobPhrasesList.MobPhrase);
			return false;
		}

		// Token: 0x04000A10 RID: 2576
		public MobPhraseTrigger targetTrigger;

		// Token: 0x04000A11 RID: 2577
		public MobPhrasesList.MobPhrase[] phrases;

		// Token: 0x020004B4 RID: 1204
		[Serializable]
		public struct MobPhrase
		{
			// Token: 0x04001975 RID: 6517
			public string localizationKey;
		}
	}
}
