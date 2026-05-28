using System;
using System.Text;
using UnityEngine;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002EF RID: 751
	[Serializable]
	public sealed class HPContainersLostCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019C0 RID: 6592 RVA: 0x00050A0E File Offset: 0x0004EC0E
		protected override bool ShouldBeIgnored()
		{
			return this.requiredContainersCount <= 0;
		}

		// Token: 0x060019C1 RID: 6593 RVA: 0x00050A1C File Offset: 0x0004EC1C
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}(Required Containers Count: {1})", base.GetType().Name, this.requiredContainersCount);
		}

		// Token: 0x060019C2 RID: 6594 RVA: 0x00050A40 File Offset: 0x0004EC40
		protected override bool GetState(CharacterPlotContext context)
		{
			float num;
			float num2;
			return this.GetProgress(context, out num, out num2) >= 1f;
		}

		// Token: 0x060019C3 RID: 6595 RVA: 0x00050A64 File Offset: 0x0004EC64
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			targetValue = (float)this.requiredContainersCount;
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				currentValue = (float)playerStatisticsManager.PlayerStatsData.GetHPContainersLostCount();
				return Mathf.Clamp01(currentValue / targetValue);
			}
			currentValue = 0f;
			return 0f;
		}

		// Token: 0x04000E63 RID: 3683
		public int requiredContainersCount;
	}
}
