using System;
using System.Collections.Generic;
using Unliving.Currencies;
using Unliving.Player.Upgrades;
using Unliving.Stores;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x02000118 RID: 280
	public interface IPlayerUpgradesStoreManager : IStoreManager
	{
		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060006AF RID: 1711
		IReadOnlyList<PurchasablePlayerUpgradesSequence> UpgradesData { get; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060006B0 RID: 1712
		bool IsStoreActive { get; }

		// Token: 0x1400003B RID: 59
		// (add) Token: 0x060006B1 RID: 1713
		// (remove) Token: 0x060006B2 RID: 1714
		event Action<IPlayerUpgradesStoreManager, int> UpgradeSelected;

		// Token: 0x1400003C RID: 60
		// (add) Token: 0x060006B3 RID: 1715
		// (remove) Token: 0x060006B4 RID: 1716
		event Action<IPlayerUpgradesStoreManager, int> UpgradeLevelChanged;

		// Token: 0x060006B5 RID: 1717
		bool SwitchSelectedUpgrade(int upgradesSequenceIndex);

		// Token: 0x060006B6 RID: 1718
		bool CanBeAdvancedToNextLevel(int upgradesSequenceIndex, out int? upgradeCost);

		// Token: 0x060006B7 RID: 1719
		bool TryBuyNextUpgrade(int upgradesSequenceIndex);

		// Token: 0x060006B8 RID: 1720
		void ResetUpgrades();

		// Token: 0x060006B9 RID: 1721
		CurrencyID GetStoreCurrencyID();

		// Token: 0x060006BA RID: 1722
		void LoadData(IReadOnlyList<PlayerUpgradeInfo> upgradesData, IReadOnlyList<PlayerUpgradeInfo> selectedUpgrades);
	}
}
