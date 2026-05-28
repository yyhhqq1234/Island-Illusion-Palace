using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot.Test
{
	// Token: 0x0200030A RID: 778
	[DisallowMultipleComponent]
	public sealed class CharacterPlotTreeTestingComponent : GameBehaviourBase, IPlotCharacter
	{
		// Token: 0x17000586 RID: 1414
		// (get) Token: 0x06001A53 RID: 6739 RVA: 0x0005240A File Offset: 0x0005060A
		string IPlotCharacter.CharacterID
		{
			get
			{
				CharacterPlotNodeGraph characterPlotNodeGraph = this.targetPlotGraph;
				if (characterPlotNodeGraph == null)
				{
					return null;
				}
				return characterPlotNodeGraph.characterID;
			}
		}

		// Token: 0x17000587 RID: 1415
		// (get) Token: 0x06001A54 RID: 6740 RVA: 0x0005241D File Offset: 0x0005061D
		ICharacterPlot IPlotCharacter.CharacterPlot
		{
			get
			{
				return this.targetPlotGraph;
			}
		}

		// Token: 0x17000588 RID: 1416
		// (get) Token: 0x06001A55 RID: 6741 RVA: 0x00052425 File Offset: 0x00050625
		// (set) Token: 0x06001A56 RID: 6742 RVA: 0x00052428 File Offset: 0x00050628
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

		// Token: 0x17000589 RID: 1417
		// (get) Token: 0x06001A57 RID: 6743 RVA: 0x0005242A File Offset: 0x0005062A
		public ICharacterPlotProgress PlotProgressOverride
		{
			get
			{
				return this.currentPlotProgress;
			}
		}

		// Token: 0x1700058A RID: 1418
		// (get) Token: 0x06001A58 RID: 6744 RVA: 0x00052432 File Offset: 0x00050632
		public IReadOnlyList<ConversationBranch> AvailableConversationBranches
		{
			get
			{
				return this.conversationBranches;
			}
		}

		// Token: 0x06001A59 RID: 6745 RVA: 0x0005243C File Offset: 0x0005063C
		private void StartConversation()
		{
			if (this.currentConversation != null)
			{
				return;
			}
			this.currentConversation = this.conversationManager.StartConversation(this);
			IEnumerable<CharacterPhrase> enumerable = this.currentConversation as IEnumerable<CharacterPhrase>;
			this.currentConversationData = ((enumerable != null) ? enumerable.GetEnumerator() : null);
			IEnumerator<CharacterPhrase> enumerator = this.currentConversationData;
			this.currentPhrase = ((enumerator != null && enumerator.MoveNext()) ? this.currentConversationData.Current : null);
		}

		// Token: 0x06001A5A RID: 6746 RVA: 0x000524AC File Offset: 0x000506AC
		private void CompleteConversation()
		{
			if (this.currentConversation == null)
			{
				return;
			}
			ICharactersConversation conversation = this.currentConversation;
			this.currentConversation = null;
			this.conversationManager.CompleteConversation(this, conversation);
		}

		// Token: 0x06001A5B RID: 6747 RVA: 0x000524DD File Offset: 0x000506DD
		private CharacterPhrase GetNextPhrase()
		{
			if (this.currentConversationData != null && this.currentConversationData.MoveNext())
			{
				this.currentPhrase = this.currentConversationData.Current;
				return this.currentPhrase;
			}
			return null;
		}

		// Token: 0x06001A5C RID: 6748 RVA: 0x0005250D File Offset: 0x0005070D
		private void OnConversationCompleted(IPlotCharacter plotProvider, ICharacterPlotItem completedConversation)
		{
			if (this.currentConversation == completedConversation)
			{
				this.CompleteConversation();
			}
		}

		// Token: 0x06001A5D RID: 6749 RVA: 0x0005251E File Offset: 0x0005071E
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<ICharacterConversationManager>(out this.conversationManager))
			{
				this.conversationManager.ConversationCompleted += new Action<IPlotCharacter, ICharactersConversation>(this.OnConversationCompleted);
			}
		}

		// Token: 0x06001A5E RID: 6750 RVA: 0x00052551 File Offset: 0x00050751
		private void Start()
		{
			this.currentPlotProgress = this.testPlotProgress.CreatePlotProgress();
		}

		// Token: 0x06001A5F RID: 6751 RVA: 0x00052564 File Offset: 0x00050764
		private void OnGUI()
		{
			if (this.targetPlotGraph == null)
			{
				return;
			}
			Vector2 vector = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			Vector2 vector2 = new Vector2((float)Screen.width * 0.8f, (float)Screen.height * 0.8f);
			GUILayout.BeginArea(new Rect(vector.x - vector2.x * 0.5f, vector.y - vector2.y * 0.5f, vector2.x, vector2.y));
			if (this.currentConversation == null)
			{
				if (GUILayout.Button("Start Conversation with " + this.targetPlotGraph.characterID, Array.Empty<GUILayoutOption>()))
				{
					this.StartConversation();
				}
			}
			else if (this.showTestUI)
			{
				GUILayout.Label(this.currentPhrase.ID + ": " + Environment.NewLine + this.currentPhrase.text, Array.Empty<GUILayoutOption>());
				if (GUILayout.Button("Next", Array.Empty<GUILayoutOption>()) && this.GetNextPhrase() == null)
				{
					this.CompleteConversation();
				}
			}
			GUILayout.EndArea();
		}

		// Token: 0x06001A60 RID: 6752 RVA: 0x00052686 File Offset: 0x00050886
		protected override void OnDestroy()
		{
			if (this.conversationManager != null)
			{
				this.conversationManager.ConversationCompleted -= new Action<IPlotCharacter, ICharactersConversation>(this.OnConversationCompleted);
			}
			base.OnDestroy();
		}

		// Token: 0x04000E9E RID: 3742
		public CharacterPlotNodeGraph targetPlotGraph;

		// Token: 0x04000E9F RID: 3743
		public CharacterPlotTreeProgressGenerator testPlotProgress;

		// Token: 0x04000EA0 RID: 3744
		public bool showTestUI = true;

		// Token: 0x04000EA1 RID: 3745
		public ConversationBranch[] conversationBranches = new ConversationBranch[1];

		// Token: 0x04000EA2 RID: 3746
		private ICharacterConversationManager conversationManager;

		// Token: 0x04000EA3 RID: 3747
		private ICharacterPlotProgress currentPlotProgress;

		// Token: 0x04000EA4 RID: 3748
		private ICharactersConversation currentConversation;

		// Token: 0x04000EA5 RID: 3749
		private IEnumerator<CharacterPhrase> currentConversationData;

		// Token: 0x04000EA6 RID: 3750
		private CharacterPhrase currentPhrase;
	}
}
