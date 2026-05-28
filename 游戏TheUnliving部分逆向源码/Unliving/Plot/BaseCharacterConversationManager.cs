using System;
using System.Linq;
using Common.ServiceRegistry;
using Game.Core;
using Game.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.PlayerProfileManagement;
using Unliving.Plot.TreeBasedCharacterPlot;

namespace Unliving.Plot
{
	// Token: 0x020002D1 RID: 721
	[CreateAssetMenu(fileName = "CharacterConversationManager", menuName = "Game/Plot/Character Conversation Manager")]
	[Service(typeof(BaseCharacterConversationManager), new Type[]
	{
		typeof(ICharacterConversationManager)
	})]
	public sealed class BaseCharacterConversationManager : GlobalManagerBase, ICharacterConversationManager
	{
		// Token: 0x140000F6 RID: 246
		// (add) Token: 0x0600191D RID: 6429 RVA: 0x0004F4BC File Offset: 0x0004D6BC
		// (remove) Token: 0x0600191E RID: 6430 RVA: 0x0004F4F4 File Offset: 0x0004D6F4
		public event Action<IPlotCharacter, ICharactersConversation> ConversationStarted;

		// Token: 0x140000F7 RID: 247
		// (add) Token: 0x0600191F RID: 6431 RVA: 0x0004F52C File Offset: 0x0004D72C
		// (remove) Token: 0x06001920 RID: 6432 RVA: 0x0004F564 File Offset: 0x0004D764
		public event Action<IPlotCharacter, ICharactersConversation, Action> ConversationPreActivated;

		// Token: 0x140000F8 RID: 248
		// (add) Token: 0x06001921 RID: 6433 RVA: 0x0004F59C File Offset: 0x0004D79C
		// (remove) Token: 0x06001922 RID: 6434 RVA: 0x0004F5D4 File Offset: 0x0004D7D4
		public event Action<IPlotCharacter, ICharactersConversation> ConversationCompleted;

		// Token: 0x06001923 RID: 6435 RVA: 0x0004F609 File Offset: 0x0004D809
		public ICharacterPlotProgress GetCharacterPlotProgress(IPlotCharacter character)
		{
			return this.playerPlotProgress.GetCharacterPlotProgress(character.CharacterID);
		}

		// Token: 0x06001924 RID: 6436 RVA: 0x0004F61C File Offset: 0x0004D81C
		private ICharactersConversation GetConversation(IPlotCharacter character, ConversationBranch conversationBranch)
		{
			ICharactersConversation preparedConversation = character.PreparedConversation;
			if (preparedConversation != null)
			{
				character.PreparedConversation = null;
				return preparedConversation;
			}
			if (this.conversationContext == null)
			{
				this.conversationContext = new CharacterPlotContext
				{
					currentGame = base.CurrentGame
				};
			}
			this.conversationContext.totalPlotProgress = this.playerPlotProgress;
			this.conversationContext.characterID = character.CharacterID;
			this.conversationContext.characterPlot = character.CharacterPlot;
			this.conversationContext.characterPlotProgress = (character.PlotProgressOverride ?? this.GetCharacterPlotProgress(character));
			return character.CharacterPlot.GetConversation(this.conversationContext, conversationBranch);
		}

		// Token: 0x06001925 RID: 6437 RVA: 0x0004F6BC File Offset: 0x0004D8BC
		public void PrepareConversation(IPlotCharacter character)
		{
			for (int i = 0; i < Enum.GetValues(typeof(ConversationBranch)).Length; i++)
			{
				ConversationBranch branch = (ConversationBranch)i;
				if (character.AvailableConversationBranches.Any((ConversationBranch b) => b == branch))
				{
					ICharactersConversation conversation = this.GetConversation(character, branch);
					character.PreparedConversation = conversation;
					if (conversation != null)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001926 RID: 6438 RVA: 0x0004F728 File Offset: 0x0004D928
		public ICharactersConversation StartConversation(IPlotCharacter character)
		{
			if (character.CharacterPlot != null)
			{
				ICharactersConversation conversation = this.GetConversation(character, ConversationBranch.Primary);
				if (conversation != null)
				{
					LocalizationManager localizationManager;
					if (base.CurrentGame.Services.TryGet<LocalizationManager>(out localizationManager))
					{
						ILocalizableDataHolder localizableDataHolder = conversation as ILocalizableDataHolder;
						if (localizableDataHolder != null && !localizationManager.TrySetLocalizedData(localizableDataHolder))
						{
							Debug.LogError("Missing localization data for conversation: " + conversation.ID);
							return null;
						}
					}
					Action<IPlotCharacter, ICharactersConversation, Action> conversationPreActivated = this.ConversationPreActivated;
					if (conversationPreActivated != null)
					{
						conversationPreActivated(character, conversation, delegate
						{
							this.StartConversationInternal(conversation);
						});
					}
					return conversation;
				}
			}
			return null;
		}

		// Token: 0x06001927 RID: 6439 RVA: 0x0004F7D8 File Offset: 0x0004D9D8
		private void StartConversationInternal(ICharactersConversation conversation)
		{
			IGameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager) && gameSessionManager.CurrentCutsceneContext == null)
			{
				gameSessionManager.SetCutsceneActive(true, conversation);
				this.completeCutscene = true;
			}
		}

		// Token: 0x06001928 RID: 6440 RVA: 0x0004F810 File Offset: 0x0004DA10
		public void CompleteConversation(IPlotCharacter character, ICharactersConversation conversation)
		{
			IGameSessionManager gameSessionManager;
			if (this.completeCutscene && base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager))
			{
				gameSessionManager.SetCutsceneActive(false, conversation);
				this.completeCutscene = false;
			}
			character.CharacterPlot.CompleteConversation(conversation, character.PlotProgressOverride ?? this.GetCharacterPlotProgress(character));
			Action<IPlotCharacter, ICharactersConversation> conversationCompleted = this.ConversationCompleted;
			if (conversationCompleted == null)
			{
				return;
			}
			conversationCompleted(character, conversation);
		}

		// Token: 0x06001929 RID: 6441 RVA: 0x0004F878 File Offset: 0x0004DA78
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				this.playerPlotProgress = ((currentPlayerProfile != null) ? currentPlayerProfile.gamePlotProgress : null);
			}
		}

		// Token: 0x0600192A RID: 6442 RVA: 0x0004F8B4 File Offset: 0x0004DAB4
		public CharacterMetadata GetActualCharacterMetadata(IPlotCharacter character, string defaultCharacterID)
		{
			if (this.conversationContext == null)
			{
				this.conversationContext = new CharacterPlotContext
				{
					currentGame = base.CurrentGame
				};
			}
			this.conversationContext.totalPlotProgress = this.playerPlotProgress;
			this.conversationContext.characterID = character.CharacterID;
			this.conversationContext.characterPlot = character.CharacterPlot;
			this.conversationContext.characterPlotProgress = this.GetCharacterPlotProgress(character);
			return this.charactersNamesNodeGraph.GetActualCharacterMetadata(this.conversationContext, defaultCharacterID);
		}

		// Token: 0x04000E24 RID: 3620
		[SerializeField]
		private CharactersNamesNodeGraph charactersNamesNodeGraph;

		// Token: 0x04000E25 RID: 3621
		private CharacterPlotContext conversationContext;

		// Token: 0x04000E26 RID: 3622
		private TotalGamePlotProgressBase playerPlotProgress;

		// Token: 0x04000E27 RID: 3623
		private bool completeCutscene;
	}
}
