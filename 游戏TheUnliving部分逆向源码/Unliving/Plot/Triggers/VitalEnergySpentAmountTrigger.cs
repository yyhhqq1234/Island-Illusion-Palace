using System;
using System.Text;
using UnityEngine;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002FA RID: 762
	[Serializable]
	public sealed class VitalEnergySpentAmountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019EE RID: 6638 RVA: 0x00051167 File Offset: 0x0004F367
		protected override bool ShouldBeIgnored()
		{
			return this.requiredVitalEnergyAmount <= 0;
		}

		// Token: 0x060019EF RID: 6639 RVA: 0x00051175 File Offset: 0x0004F375
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}(Required Vital Energy Amount: {1})", base.GetType().Name, this.requiredVitalEnergyAmount);
		}

		// Token: 0x060019F0 RID: 6640 RVA: 0x00051198 File Offset: 0x0004F398
		protected override bool GetState(CharacterPlotContext context)
		{
			float num;
			float num2;
			return this.GetProgress(context, out num, out num2) >= 1f;
		}

		// Token: 0x060019F1 RID: 6641 RVA: 0x000511BC File Offset: 0x0004F3BC
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			currentValue = 0f;
			targetValue = (float)this.requiredVitalEnergyAmount;
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				currentValue = (float)playerStatisticsManager.PlayerStatsData.GetVitalEnergySpentAmount();
			}
			return Mathf.Clamp01(currentValue / targetValue);
		}

		// Token: 0x04000E72 RID: 3698
		public int requiredVitalEnergyAmount;
	}
}
