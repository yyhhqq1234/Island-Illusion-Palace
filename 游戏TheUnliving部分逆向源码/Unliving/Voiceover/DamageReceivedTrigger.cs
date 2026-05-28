using System;
using Common.UnityExtensions;
using Game.Damage;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Voiceover
{
	// Token: 0x02000022 RID: 34
	[CreateAssetMenu(fileName = "DamageReceivedTrigger", menuName = "Game/VoiceoverTriggers/DamageReceivedTrigger")]
	public class DamageReceivedTrigger : BaseVoiceoverTrigger
	{
		// Token: 0x06000151 RID: 337 RVA: 0x00005982 File Offset: 0x00003B82
		public override void InitializeTriggerLogic()
		{
			this.currentPlayer.HitPointsController.HitPointsChanged += this.OnPlayerHealthChanged;
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000059A0 File Offset: 0x00003BA0
		private void OnPlayerHealthChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			DamageGenerator.DamageSendingArgs damageSendingArgs = args as DamageGenerator.DamageSendingArgs;
			if (damageSendingArgs == null || damageSendingArgs.amount <= 0f || sender as PlayerBehaviour == this.currentPlayer)
			{
				return;
			}
			if ((this.TriggerType == DamageReceivedTrigger.TriggerTypes.LowHealthDamageTrigger && this.currentPlayer.HitPointsController.CurrentHitPoints < this.currentPlayer.HitPointsController.MaxHitPoints * 0.2f) || this.TriggerType == DamageReceivedTrigger.TriggerTypes.AnyDamageTrigger)
			{
				base.QueueTrigger(this.Messages);
			}
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00005A1D File Offset: 0x00003C1D
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.HitPointsController.HitPointsChanged -= this.OnPlayerHealthChanged;
			}
		}

		// Token: 0x0400009C RID: 156
		[Header("Тип триггера")]
		public DamageReceivedTrigger.TriggerTypes TriggerType;

		// Token: 0x0400009D RID: 157
		[Header("Пул сообщений")]
		public VoiceoverMessage[] Messages;

		// Token: 0x02000405 RID: 1029
		public enum TriggerTypes
		{
			// Token: 0x040015A1 RID: 5537
			AnyDamageTrigger,
			// Token: 0x040015A2 RID: 5538
			LowHealthDamageTrigger
		}
	}
}
