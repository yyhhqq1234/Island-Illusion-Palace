using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.Voiceover
{
	// Token: 0x0200001F RID: 31
	[Serializable]
	public abstract class BasyFewTypeTrigger<T> : BaseVoiceoverTrigger
	{
		// Token: 0x06000146 RID: 326 RVA: 0x00005774 File Offset: 0x00003974
		public override void InitializeTriggerLogic()
		{
			this.messagesDict.Clear();
			foreach (BasyFewTypeTrigger<T>.MessagePool<T> messagePool in this.TypedMessagesPool)
			{
				if (this.messagesDict.ContainsKey(messagePool.TypeID))
				{
					Debug.LogError(string.Format("Duplicate item with {0}: {1}", messagePool.TypeID.GetType(), messagePool.TypeID));
				}
				else
				{
					this.messagesDict.Add(messagePool.TypeID, messagePool.Messages);
				}
			}
		}

		// Token: 0x06000147 RID: 327 RVA: 0x000057FC File Offset: 0x000039FC
		public VoiceoverMessage[] GetMessages(T typeID)
		{
			if (this.messagesDict.ContainsKey(typeID))
			{
				return this.messagesDict[typeID];
			}
			return this.CommonMessagePool;
		}

		// Token: 0x04000097 RID: 151
		[Header("Общие сообщения")]
		public VoiceoverMessage[] CommonMessagePool;

		// Token: 0x04000098 RID: 152
		public BasyFewTypeTrigger<T>.MessagePool<T>[] TypedMessagesPool;

		// Token: 0x04000099 RID: 153
		protected Dictionary<T, VoiceoverMessage[]> messagesDict = new Dictionary<T, VoiceoverMessage[]>();

		// Token: 0x02000402 RID: 1026
		[Serializable]
		public class MessagePool<V>
		{
			// Token: 0x0400159E RID: 5534
			[Header("Тип")]
			public T TypeID;

			// Token: 0x0400159F RID: 5535
			[Header("Пул сообщений")]
			public VoiceoverMessage[] Messages;
		}
	}
}
