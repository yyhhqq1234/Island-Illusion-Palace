using System;
using System.Text;
using Game.Factories;
using Unliving.Mobs;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002EB RID: 747
	[Serializable]
	public sealed class GameMobExplorationTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019AD RID: 6573 RVA: 0x000506AB File Offset: 0x0004E8AB
		protected override bool ShouldBeIgnored()
		{
			return this.targetMobID == MobBehaviour.ID.None;
		}

		// Token: 0x060019AE RID: 6574 RVA: 0x000506B6 File Offset: 0x0004E8B6
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1})", base.GetType().Name, this.targetMobID);
		}

		// Token: 0x060019AF RID: 6575 RVA: 0x000506D8 File Offset: 0x0004E8D8
		protected override bool GetState(CharacterPlotContext context)
		{
			PlayerStatisticsManager playerStatisticsManager;
			return context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager) && ((float)playerStatisticsManager.PlayerStatsData.GetMobDamageAmountToPlayer(this.targetMobID) > 0f || (float)playerStatisticsManager.PlayerStatsData.GetPlayerDamageAmountToMob(this.targetMobID) > 0f);
		}

		// Token: 0x04000E5B RID: 3675
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID targetMobID;
	}
}
