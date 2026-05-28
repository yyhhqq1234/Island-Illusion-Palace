using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Voiceover
{
	// Token: 0x0200001C RID: 28
	[CreateAssetMenu(fileName = "AbilityAddedTrigger", menuName = "Game/VoiceoverTriggers/AbilityAddedTrigger")]
	public class AbilityAddedTrigger : BasyFewTypeTrigger<AbilityID>
	{
		// Token: 0x06000135 RID: 309 RVA: 0x00005498 File Offset: 0x00003698
		public override void InitializeTriggerLogic()
		{
			BasyFewTypeTrigger<AbilityID>.MessagePool<AbilityID>[] abilitiesMessages = this.AbilitiesMessages;
			this.TypedMessagesPool = abilitiesMessages;
			base.InitializeTriggerLogic();
			this.currentPlayer.AbilitiesController.AbilityAdded += this.OnAbilityAdded;
		}

		// Token: 0x06000136 RID: 310 RVA: 0x000054D8 File Offset: 0x000036D8
		private void OnAbilityAdded(IAbility ability)
		{
			VoiceoverMessage[] messages = base.GetMessages((AbilityID)ability.ID);
			if (!messages.IsNull())
			{
				base.QueueTrigger(messages);
			}
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00005501 File Offset: 0x00003701
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.AbilitiesController.AbilityAdded -= this.OnAbilityAdded;
			}
		}

		// Token: 0x0400008B RID: 139
		[Header("Сообщения для абилок")]
		public AbilityAddedTrigger.AbilityMessagePool[] AbilitiesMessages;

		// Token: 0x02000400 RID: 1024
		[Serializable]
		public class AbilityMessagePool : BasyFewTypeTrigger<AbilityID>.MessagePool<AbilityID>
		{
		}
	}
}
