using System;
using Unliving.PlayerProfileManagement;
using Unliving.Plot;
using Unliving.Plot.TreeBasedCharacterPlot;

namespace Unliving.Purchasing
{
	// Token: 0x0200010F RID: 271
	public class PlotItemStateTrigger : PurchasableUnlockTriggerBase
	{
		// Token: 0x0600069D RID: 1693 RVA: 0x00015BAC File Offset: 0x00013DAC
		public override bool IsFired(Context context)
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
			if (characterPlotNodeGraph2 == null)
			{
				return false;
			}
			PlayerProfileManager playerProfileManager = context.playerProfileManager;
			PlayerProfilePlotProgress playerProfilePlotProgress;
			if (playerProfileManager == null)
			{
				playerProfilePlotProgress = null;
			}
			else
			{
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				playerProfilePlotProgress = ((currentPlayerProfile != null) ? currentPlayerProfile.gamePlotProgress : null);
			}
			PlayerProfilePlotProgress playerProfilePlotProgress2 = playerProfilePlotProgress;
			if (playerProfilePlotProgress2 == null)
			{
				return false;
			}
			ICharacterPlotProgress characterPlotProgress = playerProfilePlotProgress2.GetCharacterPlotProgress(characterPlotNodeGraph2.characterID);
			CharacterPlotTreeNodeArgs characterPlotTreeNodeArgs = new CharacterPlotTreeNodeArgs();
			CharacterPlotThreadNodeAsset characterPlotThreadNodeAsset2 = this.nodeAsset;
			characterPlotTreeNodeArgs.plotTreeNode = ((characterPlotThreadNodeAsset2 != null) ? characterPlotThreadNodeAsset2.Node : null);
			CharacterPlotTreeNodeArgs args = characterPlotTreeNodeArgs;
			return characterPlotProgress.IsCompletedPlotItem(args);
		}

		// Token: 0x0400040A RID: 1034
		public CharacterPlotThreadNodeAsset nodeAsset;

		// Token: 0x0400040B RID: 1035
		public int conversationIndex = -1;
	}
}
