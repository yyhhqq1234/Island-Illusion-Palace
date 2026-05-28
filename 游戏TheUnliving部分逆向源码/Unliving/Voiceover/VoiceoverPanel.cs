using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unliving.Voiceover
{
	// Token: 0x0200001B RID: 27
	public class VoiceoverPanel : GameBehaviourBase
	{
		// Token: 0x0600012E RID: 302 RVA: 0x000051FB File Offset: 0x000033FB
		private void Start()
		{
			BaseVoiceoverTrigger.TriggerQueued += this.OnTriggerQueued;
			this.avatarPlaceholder = this.defaultAvatar.transform.parent;
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00005224 File Offset: 0x00003424
		private void LateUpdate()
		{
			if (this.messagesQueue.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage> keyValuePair in this.messagesQueue)
			{
				Debug.Log(keyValuePair.Value.MessageText);
			}
			if (this.messagesQueue.Any((KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage> m) => m.Value.IsTutorialMessage && m.Key.TriggerFiredCount == 0))
			{
				this.ShowMessage((from m in this.messagesQueue
				where m.Value.IsTutorialMessage && m.Key.TriggerFiredCount == 0
				select m).First<KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage>>());
			}
			else
			{
				this.ShowMessage((from m in this.messagesQueue
				orderby m.Key.TriggerFiredCount
				select m).First<KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage>>());
			}
			this.messagesQueue.Clear();
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00005334 File Offset: 0x00003534
		private void OnTriggerQueued(BaseVoiceoverTrigger trigger, VoiceoverMessage message)
		{
			if (this.isShown && !message.IsTutorialMessage)
			{
				return;
			}
			this.messagesQueue.Add(new KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage>(trigger, message));
		}

		// Token: 0x06000131 RID: 305 RVA: 0x0000535C File Offset: 0x0000355C
		private void ShowMessage(KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage> message)
		{
			if (this.isShown && !message.Value.IsTutorialMessage)
			{
				return;
			}
			Debug.Log("<b>" + message.Value.MessageText + "</b>");
			this.isShown = true;
			message.Value.MessageFired();
			message.Key.TriggerFired();
			base.StopAllCoroutines();
			if (!this.avatarInstance.IsNull())
			{
				UnityEngine.Object.Destroy(this.avatarInstance);
			}
			if (!message.Value.AvatarPrefab.IsNull())
			{
				this.avatarInstance = UnityEngine.Object.Instantiate<GameObject>(message.Value.AvatarPrefab, this.avatarPlaceholder);
			}
			this.defaultAvatar.SetActive(!this.avatarInstance.IsNull());
			this.messageText.text = message.Value.MessageText;
			base.StartCoroutine(this.ShowDefaultAvatar(message.Value.MessageShowTime));
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00005456 File Offset: 0x00003656
		private IEnumerator ShowDefaultAvatar(float delay)
		{
			yield return new WaitForSeconds(Mathf.Max(2f, delay));
			if (!this.avatarInstance.IsNull())
			{
				UnityEngine.Object.Destroy(this.avatarInstance);
			}
			this.messageText.text = string.Empty;
			this.defaultAvatar.SetActive(true);
			this.isShown = false;
			yield break;
		}

		// Token: 0x06000133 RID: 307 RVA: 0x0000546C File Offset: 0x0000366C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			BaseVoiceoverTrigger.TriggerQueued -= this.OnTriggerQueued;
		}

		// Token: 0x04000085 RID: 133
		[SerializeField]
		private GameObject defaultAvatar;

		// Token: 0x04000086 RID: 134
		private Transform avatarPlaceholder;

		// Token: 0x04000087 RID: 135
		private GameObject avatarInstance;

		// Token: 0x04000088 RID: 136
		[SerializeField]
		private Text messageText;

		// Token: 0x04000089 RID: 137
		private List<KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage>> messagesQueue = new List<KeyValuePair<BaseVoiceoverTrigger, VoiceoverMessage>>();

		// Token: 0x0400008A RID: 138
		private bool isShown;
	}
}
