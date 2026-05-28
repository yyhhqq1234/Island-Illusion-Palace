using System;
using System.Collections.Generic;
using UnityEngine;
using Unliving.Player.Upgrades;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x0200011A RID: 282
	[Serializable]
	public sealed class PlayerUpgradesStoreData
	{
		// Token: 0x17000112 RID: 274
		// (get) Token: 0x060006C2 RID: 1730 RVA: 0x00015E90 File Offset: 0x00014090
		public IReadOnlyList<PurchasablePlayerUpgradesSequence> Upgrades
		{
			get
			{
				return this.upgrades;
			}
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x00015E98 File Offset: 0x00014098
		public void Initialize(IReadOnlyList<PlayerUpgradeInfo> purchasedUpgrades, IReadOnlyList<PlayerUpgradeInfo> selectedUpgrades, Predicate<PurchasablePlayerUpgradeData> upgradeAvailabilityPredicate)
		{
			PlayerUpgradesStoreData.<>c__DisplayClass3_0 CS$<>8__locals1 = new PlayerUpgradesStoreData.<>c__DisplayClass3_0();
			CS$<>8__locals1.upgradeAvailabilityPredicate = upgradeAvailabilityPredicate;
			CS$<>8__locals1.purchasedUpgrades = purchasedUpgrades;
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				PurchasablePlayerUpgradesSequence purchasablePlayerUpgradesSequence = this.upgrades[i];
				purchasablePlayerUpgradesSequence.Initialize(new Func<PurchasablePlayerUpgradesSequence, PurchasablePlayerUpgradeData, ValueTuple<bool, int>>(CS$<>8__locals1.<Initialize>g__InitializeUpgrade|0), 0);
				if (purchasablePlayerUpgradesSequence.HasAvailableUpgrades && selectedUpgrades != null)
				{
					for (int j = 0; j < selectedUpgrades.Count; j++)
					{
						int upgradeIndex = purchasablePlayerUpgradesSequence.GetUpgradeIndex(selectedUpgrades[j].upgradeID);
						if (upgradeIndex >= 0)
						{
							purchasablePlayerUpgradesSequence.SelectedUpgradeIndex = upgradeIndex;
							break;
						}
					}
				}
			}
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x00015F28 File Offset: 0x00014128
		public void CheckMilestones(IReadOnlyList<string> reachedMilestones)
		{
			if (reachedMilestones == null)
			{
				return;
			}
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				this.upgrades[i].UpdateLockState(reachedMilestones);
			}
		}

		// Token: 0x04000417 RID: 1047
		[SerializeField]
		private PurchasablePlayerUpgradesSequence[] upgrades;
	}
}
