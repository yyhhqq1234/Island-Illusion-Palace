using System;
using System.Text;
using Unliving.LevelGeneration;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F0 RID: 752
	[Serializable]
	public sealed class LocationExplorationTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019C5 RID: 6597 RVA: 0x00050AC1 File Offset: 0x0004ECC1
		protected override bool ShouldBeIgnored()
		{
			return this.targetLocation == GameLocation.TypeID.Undefined;
		}

		// Token: 0x060019C6 RID: 6598 RVA: 0x00050ACC File Offset: 0x0004ECCC
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1})", base.GetType().Name, this.targetLocation);
		}

		// Token: 0x060019C7 RID: 6599 RVA: 0x00050AF0 File Offset: 0x0004ECF0
		protected override bool GetState(CharacterPlotContext context)
		{
			PlayerStatisticsManager playerStatisticsManager;
			return context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager) && playerStatisticsManager.PlayerStatsData.IsLocationExlored(this.targetLocation);
		}

		// Token: 0x04000E64 RID: 3684
		public GameLocation.TypeID targetLocation;
	}
}
