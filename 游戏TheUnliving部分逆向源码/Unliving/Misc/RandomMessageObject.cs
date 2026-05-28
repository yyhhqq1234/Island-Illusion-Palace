using System;
using System.Collections.Generic;
using System.Linq;

namespace Unliving.Misc
{
	// Token: 0x02000247 RID: 583
	[Serializable]
	public class RandomMessageObject
	{
		// Token: 0x060013A4 RID: 5028 RVA: 0x0003D8F4 File Offset: 0x0003BAF4
		public RandomMessageObject.Message GetRandomMessage(RandomMessageObject.MessageType type)
		{
			for (int i = 0; i < this.messages.Length; i++)
			{
				if (this.messages[i].type == type)
				{
					return this.messages[i];
				}
			}
			return null;
		}

		// Token: 0x04000B72 RID: 2930
		public RandomMessageObject.ID id;

		// Token: 0x04000B73 RID: 2931
		public RandomMessageObject.Message[] messages;

		// Token: 0x020004CD RID: 1229
		public enum ID
		{
			// Token: 0x040019C9 RID: 6601
			werewolf_merchant_cemetery,
			// Token: 0x040019CA RID: 6602
			werewolf_merchant_swamp
		}

		// Token: 0x020004CE RID: 1230
		public enum MessageType
		{
			// Token: 0x040019CC RID: 6604
			common,
			// Token: 0x040019CD RID: 6605
			unwelcome_message,
			// Token: 0x040019CE RID: 6606
			big_army,
			// Token: 0x040019CF RID: 6607
			buy_nothing,
			// Token: 0x040019D0 RID: 6608
			buy_smth
		}

		// Token: 0x020004CF RID: 1231
		[Serializable]
		public class Message : IMessage
		{
			// Token: 0x1700079D RID: 1949
			// (get) Token: 0x0600254D RID: 9549 RVA: 0x00073A37 File Offset: 0x00071C37
			public int Priority
			{
				get
				{
					return this.priority;
				}
			}

			// Token: 0x0600254E RID: 9550 RVA: 0x00073A3F File Offset: 0x00071C3F
			public string GetMessageText()
			{
				return this.GetRandomMessageText();
			}

			// Token: 0x0600254F RID: 9551 RVA: 0x00073A48 File Offset: 0x00071C48
			private string GetRandomMessageText()
			{
				if (this.randomMessages == null || this.randomMessages.Count == 0)
				{
					this.randomMessages = new Queue<string>(from m in this.messageIDs
					orderby Guid.NewGuid()
					select m);
				}
				return this.randomMessages.Dequeue();
			}

			// Token: 0x040019D1 RID: 6609
			public RandomMessageObject.MessageType type;

			// Token: 0x040019D2 RID: 6610
			public int priority;

			// Token: 0x040019D3 RID: 6611
			public string[] messageIDs;

			// Token: 0x040019D4 RID: 6612
			private Queue<string> randomMessages;
		}
	}
}
