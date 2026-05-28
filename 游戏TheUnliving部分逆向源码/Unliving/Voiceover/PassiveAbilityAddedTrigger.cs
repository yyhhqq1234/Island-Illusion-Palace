using System;
using Common.UnityExtensions;
using Game.PassiveAbilities;
using UnityEngine;
using Unliving.PassiveAbilities;

namespace Unliving.Voiceover
{
	// Token: 0x02000027 RID: 39
	[CreateAssetMenu(fileName = "PassiveAbilityAddedTrigger", menuName = "Game/VoiceoverTriggers/PassiveAbilityAddedTrigger")]
	public class PassiveAbilityAddedTrigger : BasyFewTypeTrigger<PassiveAbilityID>
	{
		// Token: 0x06000165 RID: 357 RVA: 0x00005E20 File Offset: 0x00004020
		public override void InitializeTriggerLogic()
		{
			BasyFewTypeTrigger<PassiveAbilityID>.MessagePool<PassiveAbilityID>[] passiveAbilitiesMessages = this.PassiveAbilitiesMessages;
			this.TypedMessagesPool = passiveAbilitiesMessages;
			base.InitializeTriggerLogic();
			this.currentPlayer.PassiveAbilitiesController.AbilityAdded += this.OnPassiveAbilityAdded;
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00005E60 File Offset: 0x00004060
		private void OnPassiveAbilityAdded(IPassiveAbility ability)
		{
			VoiceoverMessage[] messages = base.GetMessages((PassiveAbilityID)ability.NumericID);
			if (!messages.IsNull())
			{
				base.QueueTrigger(messages);
			}
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00005E89 File Offset: 0x00004089
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.PassiveAbilitiesController.AbilityAdded -= this.OnPassiveAbilityAdded;
			}
		}

		// Token: 0x040000AD RID: 173
		[Header("Сообщения для пассивных абилок")]
		public PassiveAbilityAddedTrigger.PassiveAbilityMessagePool[] PassiveAbilitiesMessages;

		// Token: 0x02000407 RID: 1031
		[Serializable]
		public class PassiveAbilityMessagePool : BasyFewTypeTrigger<PassiveAbilityID>.MessagePool<PassiveAbilityID>
		{
		}
	}
}
