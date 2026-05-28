using System;
using System.Text;
using Unliving.LevelGeneration;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002EE RID: 750
	[Serializable]
	public sealed class GamerunAttemptCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019BB RID: 6587 RVA: 0x0005092C File Offset: 0x0004EB2C
		public GamerunAttemptCountTrigger()
		{
			this.difficultyLevel = -1;
		}

		// Token: 0x060019BC RID: 6588 RVA: 0x0005093B File Offset: 0x0004EB3B
		protected override bool ShouldBeIgnored()
		{
			return this.gamerunAttemptCount <= 0;
		}

		// Token: 0x060019BD RID: 6589 RVA: 0x00050949 File Offset: 0x0004EB49
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0} gamerun with DifficultyLevel: {1} attempt count({2})", base.GetType().Name, this.difficultyLevel, this.gamerunAttemptCount);
		}

		// Token: 0x060019BE RID: 6590 RVA: 0x00050978 File Offset: 0x0004EB78
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			currentValue = 0f;
			targetValue = (float)this.gamerunAttemptCount;
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				if (this.locationType == GameLocation.TypeID.Undefined)
				{
					currentValue = (float)playerStatisticsManager.PlayerStatsData.GetGamerunsCount();
				}
				else
				{
					currentValue = (float)playerStatisticsManager.PlayerStatsData.GetLocationRunsCount(this.locationType, this.difficultyLevel);
				}
			}
			return currentValue / targetValue;
		}

		// Token: 0x060019BF RID: 6591 RVA: 0x000509EC File Offset: 0x0004EBEC
		protected override bool GetState(CharacterPlotContext context)
		{
			float num;
			float num2;
			return this.GetProgress(context, out num, out num2) >= 1f;
		}

		// Token: 0x04000E60 RID: 3680
		public GameLocation.TypeID locationType;

		// Token: 0x04000E61 RID: 3681
		public int difficultyLevel;

		// Token: 0x04000E62 RID: 3682
		public int gamerunAttemptCount;
	}
}
