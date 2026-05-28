using System;
using Game.Core;
using Unliving.Currencies;
using Unliving.MobsStats;
using Unliving.PermanentUpgrades;
using Unliving.Player;
using Unliving.Stores;

namespace Unliving.Purchasing
{
	// Token: 0x020000F2 RID: 242
	[Serializable]
	public class PurchasablePermanentUpgradeCollection : PurchasableItemBase
	{
		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x060005CF RID: 1487 RVA: 0x00014382 File Offset: 0x00012582
		public override object ObjectID
		{
			get
			{
				return string.Format("{0}{1}", "PermanentUpgrade_", this.upgradeCollectionIndex);
			}
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060005D0 RID: 1488 RVA: 0x0001439E File Offset: 0x0001259E
		// (set) Token: 0x060005D1 RID: 1489 RVA: 0x000143A6 File Offset: 0x000125A6
		public override CurrencyOperationArgs UnlockArgs
		{
			get
			{
				return this.unlockArgs;
			}
			set
			{
				this.unlockArgs = value;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060005D2 RID: 1490 RVA: 0x000143B0 File Offset: 0x000125B0
		public PermanentUpgradesDataAsset.UpgradeData SelectedUpgrade
		{
			get
			{
				PermanentUpgradesDataAsset.UpgradeData result;
				if (this.TryGetUpgradeData(this.selectedItemIndex, out result))
				{
					return result;
				}
				return default(PermanentUpgradesDataAsset.UpgradeData);
			}
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x000143D8 File Offset: 0x000125D8
		public PurchasablePermanentUpgradeCollection(PermanentUpgradesDataAsset.UpgradesCollectionData collectionData)
		{
			this.upgradeCollectionIndex = collectionData.id;
			this.upgrades = collectionData.upgrades;
			this.unlockArgs.amount = (float)collectionData.cost;
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x00014434 File Offset: 0x00012634
		public override void SetGameData(IGame currentGame)
		{
			base.SetGameData(currentGame);
			PermanentUpgradesStoreManager permanentUpgradesStoreManager;
			if (currentGame.Services.TryGet<PermanentUpgradesStoreManager>(out permanentUpgradesStoreManager))
			{
				this.unlockArgs.currencyID = permanentUpgradesStoreManager.CurrencyID;
			}
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00014468 File Offset: 0x00012668
		public override IPurchasableData GetPurchasableData()
		{
			return new PermanentUpgradeCollectionPurchasableData(this.ObjectID.ToString(), this.selectedItemIndex);
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00014480 File Offset: 0x00012680
		public override void SetPurchasableData(IPurchasableData data)
		{
			PermanentUpgradeCollectionPurchasableData permanentUpgradeCollectionPurchasableData = data as PermanentUpgradeCollectionPurchasableData;
			if (permanentUpgradeCollectionPurchasableData != null)
			{
				this.selectedItemIndex = permanentUpgradeCollectionPurchasableData.selectedItemIndex;
			}
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x000144A3 File Offset: 0x000126A3
		public int GetNextUpgradeIndex()
		{
			if (this.selectedItemIndex == this.upgrades.Length - 1)
			{
				return 0;
			}
			return this.selectedItemIndex + 1;
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x000144C1 File Offset: 0x000126C1
		public int GetPreviousUpgradeIndex()
		{
			if (this.selectedItemIndex == 0)
			{
				return this.upgrades.Length - 1;
			}
			return this.selectedItemIndex - 1;
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x000144DE File Offset: 0x000126DE
		public bool TryGetUpgradeData(int itemIndex, out PermanentUpgradesDataAsset.UpgradeData upgradeData)
		{
			upgradeData = default(PermanentUpgradesDataAsset.UpgradeData);
			if (itemIndex >= this.upgrades.Length)
			{
				return false;
			}
			upgradeData = this.upgrades[itemIndex];
			return true;
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00014507 File Offset: 0x00012707
		public bool TrySelectCurrentUpgrade(int index)
		{
			if (!this.Purchased || index >= this.upgrades.Length)
			{
				return false;
			}
			this.selectedItemIndex = index;
			return true;
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x00014528 File Offset: 0x00012728
		public void Apply(PlayerBehaviour player)
		{
			if (!this.Purchased)
			{
				return;
			}
			PermanentUpgradesDataAsset.UpgradeData selectedUpgrade = this.SelectedUpgrade;
			if (selectedUpgrade.statID == MobStatID.Undefined)
			{
				return;
			}
			player.StatsController.AddModifier((int)selectedUpgrade.statID, selectedUpgrade.statModifier);
		}

		// Token: 0x040003DF RID: 991
		public const string ItemPrefix = "PermanentUpgrade_";

		// Token: 0x040003E0 RID: 992
		public int upgradeCollectionIndex;

		// Token: 0x040003E1 RID: 993
		public int selectedItemIndex;

		// Token: 0x040003E2 RID: 994
		public PermanentUpgradesDataAsset.UpgradeData[] upgrades;

		// Token: 0x040003E3 RID: 995
		[NonSerialized]
		private CurrencyOperationArgs unlockArgs = new CurrencyOperationArgs
		{
			currencyID = CurrencyID.Prima,
			spending = true
		};
	}
}
