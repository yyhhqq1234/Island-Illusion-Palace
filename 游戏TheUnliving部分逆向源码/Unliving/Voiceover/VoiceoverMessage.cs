using System;
using UnityEngine;

namespace Unliving.Voiceover
{
	// Token: 0x0200001D RID: 29
	[Serializable]
	public class VoiceoverMessage
	{
		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000139 RID: 313 RVA: 0x0000553A File Offset: 0x0000373A
		// (set) Token: 0x0600013A RID: 314 RVA: 0x00005542 File Offset: 0x00003742
		public int MessageFiredCount { get; private set; }

		// Token: 0x0600013B RID: 315 RVA: 0x0000554C File Offset: 0x0000374C
		public void MessageFired()
		{
			int messageFiredCount = this.MessageFiredCount;
			this.MessageFiredCount = messageFiredCount + 1;
		}

		// Token: 0x0400008C RID: 140
		[Header("Префаб аватара филактерии")]
		public GameObject AvatarPrefab;

		// Token: 0x0400008D RID: 141
		[Header("Время показа сообщения")]
		public float MessageShowTime = 5f;

		// Token: 0x0400008E RID: 142
		[TextArea(2, 6)]
		[Header("Текст сообщения")]
		public string MessageText;

		// Token: 0x0400008F RID: 143
		[Header("Это сообщение-туториал?")]
		public bool IsTutorialMessage;
	}
}
