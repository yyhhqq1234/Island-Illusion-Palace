using System;
using Game.Core;
using Unliving.Currencies;
using Unliving.Player;
using Unliving.Statistics;

namespace Unliving
{
	// Token: 0x0200000D RID: 13
	public class DemoGameSessionRewardRules : IGameSessionRewardRules
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000060 RID: 96 RVA: 0x000032B0 File Offset: 0x000014B0
		// (set) Token: 0x06000061 RID: 97 RVA: 0x000032B8 File Offset: 0x000014B8
		public bool RewardReceived { get; private set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000062 RID: 98 RVA: 0x000032C1 File Offset: 0x000014C1
		public ICurrencyOperationArgs[] Reward
		{
			get
			{
				return new ICurrencyOperationArgs[]
				{
					this.rewardArgs
				};
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000032D8 File Offset: 0x000014D8
		public void ReceiveReward(IGame game)
		{
			this.RewardReceived = true;
			PlayerStatisticsManager playerStatisticsManager;
			if (game.Services.TryGet<PlayerStatisticsManager>(out playerStatisticsManager))
			{
				int num = playerStatisticsManager.GameSessionStatsData.GetAllRevivedMobsCount();
				if (num <= 250)
				{
					this.rewardArgs.amount = (float)(num / 25);
					this.rewardArgs.amount = this.rewardArgs.amount + (float)((num % 25 > 5) ? 1 : 0);
				}
				else
				{
					num -= 250;
					this.rewardArgs.amount = 10f;
					this.rewardArgs.amount = this.rewardArgs.amount + (float)(num / 50);
					this.rewardArgs.amount = this.rewardArgs.amount + (float)((num % 50 > 5) ? 1 : 0);
				}
				IPlayerProvider playerProvider;
				if (this.rewardArgs.Amount != 0f && game.Services.TryGet<IPlayerProvider>(out playerProvider))
				{
					PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
					if (currentPlayer != null)
					{
						currentPlayer.PerformCurrencyOperation(this.rewardArgs);
					}
				}
			}
		}

		// Token: 0x0400002D RID: 45
		private CurrencyOperationArgs rewardArgs = new CurrencyOperationArgs
		{
			currencyID = CurrencyID.Meta,
			spending = false
		};
	}
}
