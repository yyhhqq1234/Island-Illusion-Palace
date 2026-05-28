using System;
using Common.Factories;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000164 RID: 356
	public interface IPlayerUpgradesFactory : IObjectFactory<IPlayerUpgrade>, IFactory<IBaseObjectDescription, IPlayerUpgrade>, IFactory
	{
		// Token: 0x060009EA RID: 2538
		PlayerUpgradeID GetPlayerUpgradeID(IPlayerUpgrade playerUpgrade);

		// Token: 0x060009EB RID: 2539
		PlayerUpgradeData GetPlayerUpgradeData(PlayerUpgradeID upgradeID);

		// Token: 0x060009EC RID: 2540
		IPlayerUpgrade Create(PlayerUpgradesFactoryArgs args);

		// Token: 0x060009ED RID: 2541
		IPlayerUpgrade CreatePlayerFeaturesBlocker();
	}
}
