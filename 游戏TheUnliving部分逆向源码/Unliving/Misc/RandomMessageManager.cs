using System;
using Common.ServiceRegistry;
using Game.Core;
using UnityEngine;

namespace Unliving.Misc
{
	// Token: 0x02000246 RID: 582
	[Service(typeof(RandomMessageManager), new Type[]
	{

	})]
	[CreateAssetMenu(fileName = "RandomMessageManager", menuName = "Game/Global/Random Message Manager")]
	public class RandomMessageManager : GlobalManagerBase, IMessageManager<RandomMessageObject.ID, RandomMessageObject.MessageType>
	{
		// Token: 0x060013A1 RID: 5025 RVA: 0x0003D82C File Offset: 0x0003BA2C
		public IMessage GetMessage(RandomMessageObject.ID objectID, RandomMessageObject.MessageType messageType)
		{
			for (int i = 0; i < this.randomMessages.Length; i++)
			{
				if (this.randomMessages[i].id == objectID)
				{
					return this.randomMessages[i].GetRandomMessage(messageType);
				}
			}
			return null;
		}

		// Token: 0x060013A2 RID: 5026 RVA: 0x0003D86C File Offset: 0x0003BA6C
		public IMessage GetMaxPriorityMessage(RandomMessageObject.ID objectID, RandomMessageObject.MessageType[] messageTypesBuffer, int bufferSize)
		{
			RandomMessageObject randomMessageObject = null;
			for (int i = 0; i < this.randomMessages.Length; i++)
			{
				if (this.randomMessages[i].id == objectID)
				{
					randomMessageObject = this.randomMessages[i];
					break;
				}
			}
			if (randomMessageObject == null)
			{
				return null;
			}
			int num = int.MinValue;
			IMessage result = null;
			for (int j = 0; j < bufferSize; j++)
			{
				RandomMessageObject.MessageType type = messageTypesBuffer[j];
				RandomMessageObject.Message randomMessage = randomMessageObject.GetRandomMessage(type);
				if (randomMessage.priority > num)
				{
					num = randomMessage.priority;
					result = randomMessage;
				}
			}
			return result;
		}

		// Token: 0x04000B71 RID: 2929
		public RandomMessageObject[] randomMessages;
	}
}
