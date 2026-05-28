using System;
using System.Text;
using Game.Factories;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Statistics;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002ED RID: 749
	[Serializable]
	public sealed class GameMobRevivedCountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019B6 RID: 6582 RVA: 0x0005083B File Offset: 0x0004EA3B
		protected override bool ShouldBeIgnored()
		{
			return this.requiredCount <= 0;
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x00050849 File Offset: 0x0004EA49
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1}, {2})", base.GetType().Name, this.targetMobID, this.requiredCount);
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x00050878 File Offset: 0x0004EA78
		protected override bool GetState(CharacterPlotContext context)
		{
			float num;
			float num2;
			return this.GetProgress(context, out num, out num2) >= 1f;
		}

		// Token: 0x060019B9 RID: 6585 RVA: 0x0005089C File Offset: 0x0004EA9C
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			currentValue = 0f;
			targetValue = (float)this.requiredCount;
			PlayerStatisticsManager playerStatisticsManager;
			if (context.currentGame.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				if (this.targetMobID != MobBehaviour.ID.None)
				{
					int num;
					if (playerStatisticsManager.PlayerStatsData.GetAllRevivedMobs().TryGetValue(this.targetMobID, out num))
					{
						currentValue = (float)num;
					}
				}
				else
				{
					currentValue = (float)playerStatisticsManager.PlayerStatsData.GetAllRevivedMobsCount();
				}
				return Mathf.Clamp01(currentValue / targetValue);
			}
			return 0f;
		}

		// Token: 0x04000E5E RID: 3678
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID targetMobID;

		// Token: 0x04000E5F RID: 3679
		public int requiredCount = 1;
	}
}
