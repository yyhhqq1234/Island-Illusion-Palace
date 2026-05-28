using System;
using Game.Core;
using Unliving.Statistics;

namespace Unliving.Tutorial
{
	// Token: 0x0200002E RID: 46
	[Serializable]
	public class ModifierPickedUpHint : TutorialHintBase
	{
		// Token: 0x06000193 RID: 403 RVA: 0x00006B55 File Offset: 0x00004D55
		public override void OnSceneLoaded(IGame game)
		{
			base.OnSceneLoaded(game);
			if (this.isCompleted)
			{
				return;
			}
			if (this.statisticsManager == null && game.Services.TryGet<PlayerStatisticsManager>(out this.statisticsManager))
			{
				this.ResetCurrentCount();
			}
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00006B8E File Offset: 0x00004D8E
		protected override bool IsConditionReached()
		{
			return this.statisticsManager.GameSessionStatsData.GetAllModifiersPickedUpCount() - this.lastModifiersCount >= this.targetModifiersCount;
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00006BB7 File Offset: 0x00004DB7
		private void ResetCurrentCount()
		{
			this.lastModifiersCount = this.statisticsManager.GameSessionStatsData.GetAllModifiersPickedUpCount();
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00006BD4 File Offset: 0x00004DD4
		protected override void OnHintConditionReached()
		{
			this.ResetCurrentCount();
		}

		// Token: 0x06000197 RID: 407 RVA: 0x00006BDC File Offset: 0x00004DDC
		protected override void OnHintCompleted()
		{
		}

		// Token: 0x040000CE RID: 206
		public int targetModifiersCount;

		// Token: 0x040000CF RID: 207
		private PlayerStatisticsManager statisticsManager;

		// Token: 0x040000D0 RID: 208
		private int lastModifiersCount;
	}
}
