using System;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x020002FD RID: 765
	[Serializable]
	public sealed class CharacterPlotTreeNodeArgs : ICharacterPlotItemArgs
	{
		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x060019FF RID: 6655 RVA: 0x0005131A File Offset: 0x0004F51A
		public ICharacterPlotItem ActualPlotItem
		{
			get
			{
				CharacterPlotItemBase result;
				if ((result = this.plotItem) == null)
				{
					CharacterPlotNodeBase characterPlotNodeBase = this.plotTreeNode;
					if (characterPlotNodeBase == null)
					{
						return null;
					}
					result = characterPlotNodeBase.GetPlotItem();
				}
				return result;
			}
		}

		// Token: 0x04000E7A RID: 3706
		public CharacterPlotItemBase plotItem;

		// Token: 0x04000E7B RID: 3707
		public CharacterPlotNodeBase plotTreeNode;

		// Token: 0x04000E7C RID: 3708
		public int conversationIndex = -1;
	}
}
