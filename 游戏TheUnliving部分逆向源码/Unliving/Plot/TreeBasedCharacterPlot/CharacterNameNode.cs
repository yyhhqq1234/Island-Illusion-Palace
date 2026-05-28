using System;
using GraphProcessor;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000303 RID: 771
	[NodeMenuItem("Plot/Character Name Node", null)]
	[Serializable]
	public class CharacterNameNode : BaseNode
	{
		// Token: 0x17000573 RID: 1395
		// (get) Token: 0x06001A24 RID: 6692 RVA: 0x00051E92 File Offset: 0x00050092
		public override string layoutStyle
		{
			get
			{
				return "CharacterNameNodeStyle";
			}
		}

		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06001A25 RID: 6693 RVA: 0x00051E99 File Offset: 0x00050099
		public override string name
		{
			get
			{
				return this.defaultMetadata.characterNameKey;
			}
		}

		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x06001A26 RID: 6694 RVA: 0x00051EA6 File Offset: 0x000500A6
		public string CharacterID
		{
			get
			{
				return this.defaultMetadata.characterNameKey;
			}
		}

		// Token: 0x06001A27 RID: 6695 RVA: 0x00051EB4 File Offset: 0x000500B4
		public CharacterMetadata GetCurrentCharacterMetadata(CharacterPlotContext context)
		{
			CharacterMetadata characterMetadata = this.defaultMetadata;
			for (int i = 0; i < this.items.Length; i++)
			{
				CharacterMetadata characterMetadata2 = this.items[i];
				if (!characterMetadata2.IsActive(context))
				{
					break;
				}
				characterMetadata.characterNameKey = characterMetadata2.characterNameKey;
				characterMetadata.characterSprite = (characterMetadata2.characterSprite ?? characterMetadata.characterSprite);
				if (characterMetadata2.HasVoiceoverEvent())
				{
					characterMetadata.voiceoverEvents = characterMetadata2.voiceoverEvents;
				}
			}
			return characterMetadata;
		}

		// Token: 0x04000E84 RID: 3716
		public CharacterMetadata defaultMetadata;

		// Token: 0x04000E85 RID: 3717
		public CharacterMetadata[] items;
	}
}
