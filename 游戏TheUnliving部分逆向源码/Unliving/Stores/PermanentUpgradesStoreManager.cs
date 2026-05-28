using System;
using System.Collections.Generic;
using System.Linq;
using Common.ServiceRegistry;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.MobsStats;
using Unliving.PermanentUpgrades;
using Unliving.PlayerProfileManagement;
using Unliving.Purchasing;

namespace Unliving.Stores
{
	// Token: 0x0200004D RID: 77
	[Service(typeof(PermanentUpgradesStoreManager), new Type[]
	{

	})]
	[CreateAssetMenu(fileName = "PermanentUpgradesStoreManager", menuName = "Game/Global/Permanent Upgrades Store Manager")]
	public class PermanentUpgradesStoreManager : StoreManagerBase
	{
		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600025F RID: 607 RVA: 0x00009999 File Offset: 0x00007B99
		public override CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Cinder;
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000260 RID: 608 RVA: 0x0000999C File Offset: 0x00007B9C
		public List<PurchasablePermanentUpgradeCollection> UpgradesCollection
		{
			get
			{
				return this.GetUpgradesCollection();
			}
		}

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x06000261 RID: 609 RVA: 0x000099A4 File Offset: 0x00007BA4
		// (remove) Token: 0x06000262 RID: 610 RVA: 0x000099DC File Offset: 0x00007BDC
		public event Action<PurchasablePermanentUpgradeCollection, int> UpgradeSelected;

		// Token: 0x06000263 RID: 611 RVA: 0x00009A14 File Offset: 0x00007C14
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			base.CurrentGame.Services.TryGet<PurchaseManager>(out this.purchaseManager);
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnProfileLoaded;
			}
			this.UpdateStatModificationsCache();
		}

		// Token: 0x06000264 RID: 612 RVA: 0x00009A74 File Offset: 0x00007C74
		private void OnProfileLoaded(PlayerProfile profile)
		{
			this.UpdateStatModificationsCache();
		}

		// Token: 0x06000265 RID: 613 RVA: 0x00009A7C File Offset: 0x00007C7C
		public override bool TryPurchaseItem(IPurchasable item)
		{
			if (base.TryPurchaseItem(item))
			{
				this.UpdateStatModificationsCache();
				return true;
			}
			return false;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x00009A90 File Offset: 0x00007C90
		public IList<MobStatID> GetAllStatsIDs()
		{
			if (this.allStatsList.Count == 0)
			{
				List<PurchasablePermanentUpgradeCollection> upgradesCollection = this.GetUpgradesCollection();
				for (int i = 0; i < upgradesCollection.Count; i++)
				{
					PermanentUpgradesDataAsset.UpgradeData[] upgrades = upgradesCollection[i].upgrades;
					for (int j = 0; j < upgrades.Length; j++)
					{
						MobStatID statID = upgrades[j].statID;
						if (!this.allStatsList.Contains(statID))
						{
							this.allStatsList.Add(statID);
						}
					}
				}
			}
			return this.allStatsList;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x00009B10 File Offset: 0x00007D10
		public float GetStatModificationValue(MobStatID statID)
		{
			float result;
			if (this.statModificationsCache.TryGetValue(statID, out result))
			{
				return result;
			}
			return 0f;
		}

		// Token: 0x06000268 RID: 616 RVA: 0x00009B34 File Offset: 0x00007D34
		public bool TrySelectUpgrade(PurchasablePermanentUpgradeCollection collection, int upgradeIndex)
		{
			List<PurchasablePermanentUpgradeCollection> upgradesCollection = this.GetUpgradesCollection();
			for (int i = 0; i < upgradesCollection.Count; i++)
			{
				if (upgradesCollection[i] == collection && collection.TrySelectCurrentUpgrade(upgradeIndex))
				{
					this.UpdateStatModificationsCache();
					Action<PurchasablePermanentUpgradeCollection, int> upgradeSelected = this.UpgradeSelected;
					if (upgradeSelected != null)
					{
						upgradeSelected(collection, upgradeIndex);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000269 RID: 617 RVA: 0x00009B88 File Offset: 0x00007D88
		private void UpdateStatModificationsCache()
		{
			this.statModificationsCache.Clear();
			foreach (PurchasablePermanentUpgradeCollection purchasablePermanentUpgradeCollection in this.GetUpgradesCollection())
			{
				if (purchasablePermanentUpgradeCollection.Purchased)
				{
					PermanentUpgradesDataAsset.UpgradeData selectedUpgrade = purchasablePermanentUpgradeCollection.SelectedUpgrade;
					MobStatID statID = selectedUpgrade.statID;
					float baseModifier = selectedUpgrade.statModifier.BaseModifier;
					if (this.statModificationsCache.ContainsKey(statID))
					{
						Dictionary<MobStatID, float> dictionary = this.statModificationsCache;
						MobStatID key = statID;
						dictionary[key] += baseModifier;
					}
					else
					{
						this.statModificationsCache.Add(statID, baseModifier);
					}
				}
			}
		}

		// Token: 0x0600026A RID: 618 RVA: 0x00009C44 File Offset: 0x00007E44
		private List<PurchasablePermanentUpgradeCollection> GetUpgradesCollection()
		{
			return (from p in this.purchaseManager.GetPurchasablesOfType<PurchasablePermanentUpgradeCollection>(false)
			orderby p.upgradeCollectionIndex
			select p).ToList<PurchasablePermanentUpgradeCollection>();
		}

		// Token: 0x0600026B RID: 619 RVA: 0x00009C7B File Offset: 0x00007E7B
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnProfileLoaded;
			}
		}

		// Token: 0x04000172 RID: 370
		private PurchaseManager purchaseManager;

		// Token: 0x04000173 RID: 371
		private PlayerProfileManager profileManager;

		// Token: 0x04000174 RID: 372
		private readonly Dictionary<MobStatID, float> statModificationsCache = new Dictionary<MobStatID, float>();

		// Token: 0x04000175 RID: 373
		private readonly List<MobStatID> allStatsList = new List<MobStatID>();
	}
}
