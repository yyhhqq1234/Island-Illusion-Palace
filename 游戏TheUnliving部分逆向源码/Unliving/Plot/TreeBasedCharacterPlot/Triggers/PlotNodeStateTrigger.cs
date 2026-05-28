using System;
using Unliving.Plot.Triggers;

namespace Unliving.Plot.TreeBasedCharacterPlot.Triggers
{
	// Token: 0x02000308 RID: 776
	[Serializable]
	public sealed class PlotNodeStateTrigger : PlotItemStateTriggerBase
	{
		// Token: 0x06001A4C RID: 6732 RVA: 0x000522D4 File Offset: 0x000504D4
		protected override ICharacterPlotItemArgs GetTargetPlotItemArgs()
		{
			CharacterPlotThreadNodeAsset characterPlotThreadNodeAsset = this.nodeAsset;
			if (((characterPlotThreadNodeAsset != null) ? characterPlotThreadNodeAsset.Node : null) == null)
			{
				return null;
			}
			PlotNodeStateTrigger.TargetNodeArgs.plotTreeNode = this.nodeAsset.Node;
			PlotNodeStateTrigger.TargetNodeArgs.conversationIndex = this.conversationIndex;
			return PlotNodeStateTrigger.TargetNodeArgs;
		}

		// Token: 0x06001A4D RID: 6733 RVA: 0x00052324 File Offset: 0x00050524
		protected override ICharacterPlotProgress GetTargetCharacterPlotProgress(CharacterPlotContext context)
		{
			CharacterPlotThreadNodeAsset characterPlotThreadNodeAsset = this.nodeAsset;
			CharacterPlotNodeGraph characterPlotNodeGraph;
			if (characterPlotThreadNodeAsset == null)
			{
				characterPlotNodeGraph = null;
			}
			else
			{
				CharacterPlotNodeBase node = characterPlotThreadNodeAsset.Node;
				characterPlotNodeGraph = ((node != null) ? node.Graph : null);
			}
			CharacterPlotNodeGraph characterPlotNodeGraph2 = characterPlotNodeGraph;
			return context.totalPlotProgress.GetCharacterPlotProgress(characterPlotNodeGraph2.characterID);
		}

		// Token: 0x04000E98 RID: 3736
		private static readonly CharacterPlotTreeNodeArgs TargetNodeArgs = new CharacterPlotTreeNodeArgs();

		// Token: 0x04000E99 RID: 3737
		public CharacterPlotThreadNodeAsset nodeAsset;

		// Token: 0x04000E9A RID: 3738
		public int conversationIndex = -1;
	}
}
