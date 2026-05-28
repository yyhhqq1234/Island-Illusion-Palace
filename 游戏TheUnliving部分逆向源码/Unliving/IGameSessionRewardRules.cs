using System;
using Game.Core;
using Unliving.Currencies;

namespace Unliving
{
	// Token: 0x02000011 RID: 17
	public interface IGameSessionRewardRules
	{
		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000D9 RID: 217
		bool RewardReceived { get; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000DA RID: 218
		ICurrencyOperationArgs[] Reward { get; }

		// Token: 0x060000DB RID: 219
		void ReceiveReward(IGame game);
	}
}
