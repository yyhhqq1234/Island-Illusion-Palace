using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x020002FE RID: 766
	[Serializable]
	public sealed class CharacterPlotTreeProgress : ICharacterPlotProgress
	{
		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06001A01 RID: 6657 RVA: 0x00051346 File Offset: 0x0004F546
		// (set) Token: 0x06001A02 RID: 6658 RVA: 0x0005134E File Offset: 0x0004F54E
		public bool UseCurrentPlotThreadForce { get; set; }

		// Token: 0x06001A03 RID: 6659 RVA: 0x00051357 File Offset: 0x0004F557
		private bool HasThreadInProgress()
		{
			return !string.IsNullOrEmpty(this.currentPlotThread);
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x00051367 File Offset: 0x0004F567
		public CharacterPlotTreeProgress()
		{
			this.currentPlotThread = null;
			this.currentPlotConversationIndex = -1;
			this.completedPlotThreads = new HashSet<string>();
		}

		// Token: 0x06001A05 RID: 6661 RVA: 0x00051388 File Offset: 0x0004F588
		public CharacterPlotTreeProgress(string currentPlotThread, int currentConversationIndex, IEnumerable<string> completedPlotThreads)
		{
			this.currentPlotThread = currentPlotThread;
			this.currentPlotConversationIndex = currentConversationIndex;
			this.completedPlotThreads = ((completedPlotThreads != null) ? new HashSet<string>(completedPlotThreads) : new HashSet<string>());
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x000513B4 File Offset: 0x0004F5B4
		public bool IsCompletedPlotThread(ICharacterPlotItem plotThread, int conversationIndex = -1)
		{
			return this.completedPlotThreads.Contains(plotThread.ID) || (conversationIndex >= 0 && this.IsPlotThreadInProgress(plotThread) && this.currentPlotConversationIndex > conversationIndex);
		}

		// Token: 0x06001A07 RID: 6663 RVA: 0x000513E3 File Offset: 0x0004F5E3
		public bool IsPlotThreadInProgress(ICharacterPlotItem plotThread)
		{
			return this.HasThreadInProgress() && this.currentPlotThread == plotThread.ID;
		}

		// Token: 0x06001A08 RID: 6664 RVA: 0x00051400 File Offset: 0x0004F600
		public bool TryGetCurrentPlotThread(out string plotThreadID, out int conversationIndex)
		{
			if (this.HasThreadInProgress())
			{
				plotThreadID = this.currentPlotThread;
				conversationIndex = this.currentPlotConversationIndex;
				return true;
			}
			plotThreadID = null;
			conversationIndex = -1;
			return false;
		}

		// Token: 0x06001A09 RID: 6665 RVA: 0x00051423 File Offset: 0x0004F623
		public void Update(ICharacterPlotItem currentPlotThread, int newConversationIndex)
		{
			if (newConversationIndex >= 0)
			{
				this.currentPlotThread = currentPlotThread.ID;
				this.currentPlotConversationIndex = newConversationIndex;
				return;
			}
			this.completedPlotThreads.Add(currentPlotThread.ID);
			this.currentPlotThread = null;
			this.currentPlotConversationIndex = -1;
		}

		// Token: 0x06001A0A RID: 6666 RVA: 0x00051460 File Offset: 0x0004F660
		public void AddCompletedExpositionThread(ICharacterPlotItem currentPlotThread)
		{
			string id = currentPlotThread.ID;
			if (this.completedPlotThreads.Contains(id))
			{
				return;
			}
			this.completedPlotThreads.Add(id);
		}

		// Token: 0x06001A0B RID: 6667 RVA: 0x00051490 File Offset: 0x0004F690
		bool ICharacterPlotProgress.IsCompletedPlotItem(ICharacterPlotItemArgs args)
		{
			CharacterPlotTreeNodeArgs characterPlotTreeNodeArgs = args as CharacterPlotTreeNodeArgs;
			if (characterPlotTreeNodeArgs != null)
			{
				return this.IsCompletedPlotThread(characterPlotTreeNodeArgs.ActualPlotItem, characterPlotTreeNodeArgs.conversationIndex);
			}
			return this.IsCompletedPlotThread(args.ActualPlotItem, -1);
		}

		// Token: 0x06001A0C RID: 6668 RVA: 0x000514C7 File Offset: 0x0004F6C7
		bool ICharacterPlotProgress.IsPlotItemInProgress(ICharacterPlotItemArgs args)
		{
			return this.IsPlotThreadInProgress(args.ActualPlotItem);
		}

		// Token: 0x06001A0D RID: 6669 RVA: 0x000514D8 File Offset: 0x0004F6D8
		void ICharacterPlotProgress.Update(ICharacterPlotItemArgs args)
		{
			CharacterPlotTreeNodeArgs characterPlotTreeNodeArgs = (CharacterPlotTreeNodeArgs)args;
			this.Update(characterPlotTreeNodeArgs.ActualPlotItem, characterPlotTreeNodeArgs.conversationIndex);
		}

		// Token: 0x04000E7E RID: 3710
		[SerializeField]
		[HideInInspector]
		private string currentPlotThread;

		// Token: 0x04000E7F RID: 3711
		[SerializeField]
		[HideInInspector]
		private int currentPlotConversationIndex;

		// Token: 0x04000E80 RID: 3712
		[SerializeField]
		private HashSet<string> completedPlotThreads;
	}
}
