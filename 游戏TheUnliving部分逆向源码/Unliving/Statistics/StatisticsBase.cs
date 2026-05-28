using System;
using Common;
using Game.Core;

namespace Unliving.Statistics
{
	// Token: 0x0200012D RID: 301
	public abstract class StatisticsBase : IStatistics, ICloneable<IStatistics>
	{
		// Token: 0x060007A6 RID: 1958 RVA: 0x00019921 File Offset: 0x00017B21
		public virtual void SetGameData(IGame game, PlayerStatisticsManager.Data playerStatisticsData)
		{
			this.currentGame = game;
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x0001992A File Offset: 0x00017B2A
		public virtual void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider)
		{
			this.storeStatisticsProvider = storeStatisticsProvider;
		}

		// Token: 0x060007A8 RID: 1960
		public abstract void Destroy();

		// Token: 0x060007A9 RID: 1961
		public abstract IStatisticsSerializationData GetSerializationData();

		// Token: 0x060007AA RID: 1962
		public abstract void SetSerializationData(IStatisticsSerializationData serializationData);

		// Token: 0x060007AB RID: 1963 RVA: 0x00019933 File Offset: 0x00017B33
		public IStatistics Clone()
		{
			return this.Copy<StatisticsBase>();
		}

		// Token: 0x04000471 RID: 1137
		protected IGame currentGame;

		// Token: 0x04000472 RID: 1138
		protected IStoreStatisticsProvider storeStatisticsProvider;
	}
}
