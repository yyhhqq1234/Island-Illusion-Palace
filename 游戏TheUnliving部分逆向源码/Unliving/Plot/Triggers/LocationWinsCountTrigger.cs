using System;
using System.Text;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F1 RID: 753
	[Serializable]
	public sealed class LocationWinsCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019C9 RID: 6601 RVA: 0x00050B31 File Offset: 0x0004ED31
		public LocationWinsCountTrigger()
		{
			this.difficultyLevel = -1;
		}

		// Token: 0x060019CA RID: 6602 RVA: 0x00050B40 File Offset: 0x0004ED40
		protected override bool ShouldBeIgnored()
		{
			return this.targetLocation == GameLocation.TypeID.Undefined;
		}

		// Token: 0x060019CB RID: 6603 RVA: 0x00050B4C File Offset: 0x0004ED4C
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1} with DifficultyLevel {2} {3} wins)", new object[]
			{
				base.GetType().Name,
				this.targetLocation,
				this.difficultyLevel,
				this.winsCount
			});
		}

		// Token: 0x060019CC RID: 6604 RVA: 0x00050BA4 File Offset: 0x0004EDA4
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			currentValue = 0f;
			targetValue = (float)this.winsCount;
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				currentValue = (float)playerStatisticsManager.PlayerStatsData.GetLocationWinsCount(this.targetLocation, this.difficultyLevel);
			}
			return Mathf.Clamp01(currentValue / targetValue);
		}

		// Token: 0x060019CD RID: 6605 RVA: 0x00050C00 File Offset: 0x0004EE00
		protected override bool GetState(CharacterPlotContext context)
		{
			float num;
			float num2;
			return this.GetProgress(context, out num, out num2) >= 1f;
		}

		// Token: 0x04000E65 RID: 3685
		public GameLocation.TypeID targetLocation;

		// Token: 0x04000E66 RID: 3686
		public int difficultyLevel;

		// Token: 0x04000E67 RID: 3687
		public int winsCount;
	}
}
