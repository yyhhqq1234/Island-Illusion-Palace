using System;
using System.Text;
using UnityEngine;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F4 RID: 756
	[Serializable]
	public sealed class PlayerKilledCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019D7 RID: 6615 RVA: 0x00050E55 File Offset: 0x0004F055
		protected override bool ShouldBeIgnored()
		{
			return this.requiredKillsCount <= 0;
		}

		// Token: 0x060019D8 RID: 6616 RVA: 0x00050E63 File Offset: 0x0004F063
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}(Required player kills Count: {1})", base.GetType().Name, this.requiredKillsCount);
		}

		// Token: 0x060019D9 RID: 6617 RVA: 0x00050E88 File Offset: 0x0004F088
		protected override bool GetState(CharacterPlotContext context)
		{
			float num;
			float num2;
			return this.GetProgress(context, out num, out num2) >= 1f;
		}

		// Token: 0x060019DA RID: 6618 RVA: 0x00050EAC File Offset: 0x0004F0AC
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			targetValue = (float)this.requiredKillsCount;
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				currentValue = (float)playerStatisticsManager.PlayerStatsData.GetPlayerKilledCount();
				return Mathf.Clamp01(currentValue / targetValue);
			}
			currentValue = 0f;
			return 0f;
		}

		// Token: 0x04000E6C RID: 3692
		public int requiredKillsCount;
	}
}
