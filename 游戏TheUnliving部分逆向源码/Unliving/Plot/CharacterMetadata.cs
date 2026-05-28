using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002D2 RID: 722
	[Serializable]
	public struct CharacterMetadata
	{
		// Token: 0x0600192C RID: 6444 RVA: 0x0004F940 File Offset: 0x0004DB40
		public bool IsActive(CharacterPlotContext context)
		{
			if (this.triggers.Length == 0)
			{
				return true;
			}
			for (int i = 0; i < this.triggers.Length; i++)
			{
				if (!this.triggers[i].IsFired(context))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600192D RID: 6445 RVA: 0x0004F97E File Offset: 0x0004DB7E
		public bool HasVoiceoverEvent()
		{
			return this.voiceoverEvents != null && this.voiceoverEvents.Length != 0;
		}

		// Token: 0x0600192E RID: 6446 RVA: 0x0004F994 File Offset: 0x0004DB94
		public bool TryGetVoiceoverEvent(out AK.Wwise.Event wwiseEvent)
		{
			wwiseEvent = null;
			if (!this.HasVoiceoverEvent())
			{
				return false;
			}
			wwiseEvent = this.voiceoverEvents[UnityEngine.Random.Range(0, this.voiceoverEvents.Length)];
			return true;
		}

		// Token: 0x04000E28 RID: 3624
		[FormerlySerializedAs("overrideID")]
		public string characterNameKey;

		// Token: 0x04000E29 RID: 3625
		public Sprite characterSprite;

		// Token: 0x04000E2A RID: 3626
		public AK.Wwise.Event[] voiceoverEvents;

		// Token: 0x04000E2B RID: 3627
		[SerializeReference]
		[CharacterPlotItemTrigger]
		public CharacterPlotItemTriggerBase[] triggers;
	}
}
