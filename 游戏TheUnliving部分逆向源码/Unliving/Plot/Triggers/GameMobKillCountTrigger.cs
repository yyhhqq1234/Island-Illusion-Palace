using System;
using System.Text;
using Game.Factories;
using Unliving.Mobs;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002EC RID: 748
	[Serializable]
	public sealed class GameMobKillCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019B1 RID: 6577 RVA: 0x00050741 File Offset: 0x0004E941
		protected override bool ShouldBeIgnored()
		{
			return this.requiredKillCount <= 0 || this.targetMobID == MobBehaviour.ID.None;
		}

		// Token: 0x060019B2 RID: 6578 RVA: 0x00050757 File Offset: 0x0004E957
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1}, {2})", base.GetType().Name, this.targetMobID, this.requiredKillCount);
		}

		// Token: 0x060019B3 RID: 6579 RVA: 0x00050784 File Offset: 0x0004E984
		protected override bool GetState(CharacterPlotContext context)
		{
			PlayerStatisticsManager playerStatisticsManager;
			return context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager) && playerStatisticsManager.PlayerStatsData.GetMobsKilledCount(this.targetMobID) >= this.requiredKillCount;
		}

		// Token: 0x060019B4 RID: 6580 RVA: 0x000507C8 File Offset: 0x0004E9C8
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				currentValue = (float)playerStatisticsManager.PlayerStatsData.GetMobsKilledCount(this.targetMobID);
				targetValue = (float)this.requiredKillCount;
				return currentValue / (float)this.requiredKillCount;
			}
			currentValue = 0f;
			targetValue = (float)this.requiredKillCount;
			return 0f;
		}

		// Token: 0x04000E5C RID: 3676
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID targetMobID;

		// Token: 0x04000E5D RID: 3677
		public int requiredKillCount = 1;
	}
}
