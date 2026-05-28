using System;
using System.Text;
using Unliving.Abilities;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002E6 RID: 742
	[Serializable]
	public sealed class AbilityKillsCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x06001997 RID: 6551 RVA: 0x00050384 File Offset: 0x0004E584
		protected override bool ShouldBeIgnored()
		{
			return this.requiredKillCount <= 0 || this.abilityID == AbilityID.None;
		}

		// Token: 0x06001998 RID: 6552 RVA: 0x0005039A File Offset: 0x0004E59A
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1}, {2})", base.GetType().Name, this.abilityID, this.requiredKillCount);
		}

		// Token: 0x06001999 RID: 6553 RVA: 0x000503C8 File Offset: 0x0004E5C8
		protected override bool GetState(CharacterPlotContext context)
		{
			PlayerStatisticsManager playerStatisticsManager;
			return context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager) && playerStatisticsManager.PlayerStatsData.GetAbilityKillsCount(this.abilityID) >= this.requiredKillCount;
		}

		// Token: 0x0600199A RID: 6554 RVA: 0x0005040C File Offset: 0x0004E60C
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				currentValue = (float)playerStatisticsManager.PlayerStatsData.GetAbilityKillsCount(this.abilityID);
				targetValue = (float)this.requiredKillCount;
				return currentValue / (float)this.requiredKillCount;
			}
			currentValue = 0f;
			targetValue = (float)this.requiredKillCount;
			return 0f;
		}

		// Token: 0x04000E56 RID: 3670
		public AbilityID abilityID;

		// Token: 0x04000E57 RID: 3671
		public int requiredKillCount = 1;
	}
}
