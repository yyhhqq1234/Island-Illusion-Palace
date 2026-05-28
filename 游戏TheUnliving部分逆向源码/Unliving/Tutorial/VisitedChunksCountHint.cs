using System;
using Game.Core;
using Unliving.LevelGeneration;
using Unliving.Statistics;

namespace Unliving.Tutorial
{
	// Token: 0x02000031 RID: 49
	[Serializable]
	public class VisitedChunksCountHint : TutorialHintBase
	{
		// Token: 0x060001A5 RID: 421 RVA: 0x00006D31 File Offset: 0x00004F31
		public override void OnSceneLoaded(IGame game)
		{
			base.OnSceneLoaded(game);
			if (this.isCompleted)
			{
				return;
			}
			if (this.statisticsManager == null && game.Services.TryGet<PlayerStatisticsManager>(out this.statisticsManager))
			{
				this.ResetVisitedChunksCount();
			}
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x00006D6A File Offset: 0x00004F6A
		protected override bool IsConditionReached()
		{
			return this.statisticsManager.GameSessionStatsData.GetChunksVisitedCount(this.chunkTypes) - this.lastVisitedChunksCount >= this.targetChunksCount;
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x00006D99 File Offset: 0x00004F99
		private void ResetVisitedChunksCount()
		{
			this.lastVisitedChunksCount = this.statisticsManager.GameSessionStatsData.GetChunksVisitedCount(this.chunkTypes);
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x00006DBC File Offset: 0x00004FBC
		protected override void OnHintConditionReached()
		{
			this.ResetVisitedChunksCount();
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x00006DC4 File Offset: 0x00004FC4
		protected override void OnHintCompleted()
		{
		}

		// Token: 0x040000DA RID: 218
		public LocationChunk.TypeID[] chunkTypes;

		// Token: 0x040000DB RID: 219
		public int targetChunksCount;

		// Token: 0x040000DC RID: 220
		private PlayerStatisticsManager statisticsManager;

		// Token: 0x040000DD RID: 221
		private int lastVisitedChunksCount;
	}
}
