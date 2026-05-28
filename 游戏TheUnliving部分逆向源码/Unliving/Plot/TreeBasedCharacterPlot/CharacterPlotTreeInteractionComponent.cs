using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x020002FB RID: 763
	public class CharacterPlotTreeInteractionComponent : GameBehaviourBase, IPlotCharacter
	{
		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x060019F3 RID: 6643 RVA: 0x00051212 File Offset: 0x0004F412
		public string CharacterID
		{
			get
			{
				CharacterPlotNodeGraph characterPlotNodeGraph = this.characterPlotGraph;
				if (characterPlotNodeGraph == null)
				{
					return null;
				}
				return characterPlotNodeGraph.characterID;
			}
		}

		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x060019F4 RID: 6644 RVA: 0x00051225 File Offset: 0x0004F425
		public ICharacterPlot CharacterPlot
		{
			get
			{
				return this.characterPlotGraph;
			}
		}

		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x060019F5 RID: 6645 RVA: 0x0005122D File Offset: 0x0004F42D
		public ICharacterPlotProgress PlotProgressOverride
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700056E RID: 1390
		// (get) Token: 0x060019F6 RID: 6646 RVA: 0x00051230 File Offset: 0x0004F430
		// (set) Token: 0x060019F7 RID: 6647 RVA: 0x00051233 File Offset: 0x0004F433
		ICharactersConversation IPlotCharacter.PreparedConversation
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x060019F8 RID: 6648 RVA: 0x00051235 File Offset: 0x0004F435
		public IReadOnlyList<ConversationBranch> AvailableConversationBranches
		{
			get
			{
				return this.availableConversationBranches;
			}
		}

		// Token: 0x060019F9 RID: 6649 RVA: 0x00051240 File Offset: 0x0004F440
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.availableConversationBranches = this.conversationBranches.ToList<ConversationBranch>();
			if (currentGame.Services.TryGet<ICharacterConversationManager>(out this.conversationManager))
			{
				this.conversationManager.ConversationCompleted += new Action<IPlotCharacter, ICharactersConversation>(this.OnConversationCompleted);
			}
		}

		// Token: 0x060019FA RID: 6650 RVA: 0x0005128F File Offset: 0x0004F48F
		public void Interact()
		{
			if (this.activeConversation != null)
			{
				return;
			}
			ICharacterConversationManager characterConversationManager = this.conversationManager;
			this.activeConversation = ((characterConversationManager != null) ? characterConversationManager.StartConversation(this) : null);
		}

		// Token: 0x060019FB RID: 6651 RVA: 0x000512B3 File Offset: 0x0004F4B3
		private void OnConversationCompleted(IPlotCharacter character, ICharacterPlotItem conversation)
		{
			if (this.activeConversation == conversation)
			{
				this.availableConversationBranches.Remove(conversation.ConversationBranch);
				this.activeConversation = null;
			}
		}

		// Token: 0x060019FC RID: 6652 RVA: 0x000512D7 File Offset: 0x0004F4D7
		protected override void OnDestroy()
		{
			if (this.conversationManager != null)
			{
				this.conversationManager.ConversationCompleted -= new Action<IPlotCharacter, ICharactersConversation>(this.OnConversationCompleted);
			}
			base.OnDestroy();
		}

		// Token: 0x04000E73 RID: 3699
		[SerializeField]
		private CharacterPlotNodeGraph characterPlotGraph;

		// Token: 0x04000E74 RID: 3700
		[SerializeField]
		private ConversationBranch[] conversationBranches = new ConversationBranch[1];

		// Token: 0x04000E75 RID: 3701
		private ICharacterConversationManager conversationManager;

		// Token: 0x04000E76 RID: 3702
		private ICharacterPlotItem activeConversation;

		// Token: 0x04000E77 RID: 3703
		private List<ConversationBranch> availableConversationBranches;
	}
}
