using System;
using System.Linq;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Voiceover
{
	// Token: 0x0200001E RID: 30
	public abstract class BaseVoiceoverTrigger : ScriptableObject
	{
		// Token: 0x14000019 RID: 25
		// (add) Token: 0x0600013D RID: 317 RVA: 0x0000557C File Offset: 0x0000377C
		// (remove) Token: 0x0600013E RID: 318 RVA: 0x000055B0 File Offset: 0x000037B0
		public static event Action<BaseVoiceoverTrigger, VoiceoverMessage> TriggerQueued;

		// Token: 0x0600013F RID: 319 RVA: 0x000055E3 File Offset: 0x000037E3
		public void TriggerFired()
		{
			this.lastFiredTime = Time.time;
			this.TriggerFiredCount++;
		}

		// Token: 0x06000140 RID: 320
		public abstract void InitializeTriggerLogic();

		// Token: 0x06000141 RID: 321 RVA: 0x000055FE File Offset: 0x000037FE
		public virtual void Initialize(IGame currentGame)
		{
			this.gameSessionManager = currentGame.Services.Get<GameSessionManager>();
			this.gameSessionManager.PlayerRegistered += this.OnPlayerChanged;
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00005628 File Offset: 0x00003828
		private void OnPlayerChanged(PlayerBehaviour player)
		{
			this.currentPlayer = player;
			if (!this.currentPlayer.IsNull())
			{
				this.InitializeTriggerLogic();
			}
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00005644 File Offset: 0x00003844
		protected void QueueTrigger(VoiceoverMessage[] messages)
		{
			if (messages.Any((VoiceoverMessage m) => m.IsTutorialMessage && m.MessageFiredCount == 0))
			{
				Action<BaseVoiceoverTrigger, VoiceoverMessage> triggerQueued = BaseVoiceoverTrigger.TriggerQueued;
				if (triggerQueued == null)
				{
					return;
				}
				triggerQueued(this, (from m in messages
				where m.IsTutorialMessage && m.MessageFiredCount == 0
				select m).First<VoiceoverMessage>());
				return;
			}
			else
			{
				if (Time.time - this.lastFiredTime < this.TriggerTimeout)
				{
					return;
				}
				if (messages.Length == 0)
				{
					return;
				}
				VoiceoverMessage arg = (from m in messages
				where !m.IsTutorialMessage
				orderby m.MessageFiredCount
				select m).First<VoiceoverMessage>();
				Action<BaseVoiceoverTrigger, VoiceoverMessage> triggerQueued2 = BaseVoiceoverTrigger.TriggerQueued;
				if (triggerQueued2 == null)
				{
					return;
				}
				triggerQueued2(this, arg);
				return;
			}
		}

		// Token: 0x06000144 RID: 324 RVA: 0x0000572D File Offset: 0x0000392D
		protected virtual void OnDestroy()
		{
			if (!this.gameSessionManager.IsNull())
			{
				this.gameSessionManager.PlayerRegistered -= this.OnPlayerChanged;
			}
		}

		// Token: 0x04000092 RID: 146
		[Header("Таймаут срабатывания триггера(сек.)")]
		public float TriggerTimeout = 60f;

		// Token: 0x04000093 RID: 147
		[NonSerialized]
		private float lastFiredTime = float.MinValue;

		// Token: 0x04000094 RID: 148
		[NonSerialized]
		public int TriggerFiredCount;

		// Token: 0x04000095 RID: 149
		protected GameSessionManager gameSessionManager;

		// Token: 0x04000096 RID: 150
		protected PlayerBehaviour currentPlayer;
	}
}
