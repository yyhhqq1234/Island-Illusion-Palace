using System;
using System.Collections.Generic;
using Common.ServiceRegistry;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Player.Upgrades;
using Unliving.PlayerProfileManagement;
using Unliving.Plot.Milestones;
using Unliving.Stores;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x0200011C RID: 284
	[Service(typeof(PlayerUpgradesStoreManager), new Type[]
	{
		typeof(IPlayerUpgradesStoreManager)
	})]
	[CreateAssetMenu(fileName = "PlayerUpgradesStoreManager", menuName = "Game/Global/Player Upgrades Store Manager")]
	public sealed class PlayerUpgradesStoreManager : GlobalManagerBase, IPlayerUpgradesStoreManager, IStoreManager
	{
		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060006D5 RID: 1749 RVA: 0x00016089 File Offset: 0x00014289
		public IReadOnlyList<PurchasablePlayerUpgradesSequence> UpgradesData
		{
			get
			{
				return this.upgradesData.Upgrades;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x00016096 File Offset: 0x00014296
		public bool IsStoreActive
		{
			get
			{
				return this.isStoreActive;
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x060006D7 RID: 1751 RVA: 0x0001609E File Offset: 0x0001429E
		public bool IsStoreOpened
		{
			get
			{
				return this.isStoreOpened;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x060006D8 RID: 1752 RVA: 0x000160A6 File Offset: 0x000142A6
		// (set) Token: 0x060006D9 RID: 1753 RVA: 0x000160AE File Offset: 0x000142AE
		public IStoreInteractionSpot CurrentInteractionSpot { get; private set; }

		// Token: 0x1400003D RID: 61
		// (add) Token: 0x060006DA RID: 1754 RVA: 0x000160B8 File Offset: 0x000142B8
		// (remove) Token: 0x060006DB RID: 1755 RVA: 0x000160F0 File Offset: 0x000142F0
		public event Action<IStoreManager, bool> StoreStateChanged;

		// Token: 0x1400003E RID: 62
		// (add) Token: 0x060006DC RID: 1756 RVA: 0x00016128 File Offset: 0x00014328
		// (remove) Token: 0x060006DD RID: 1757 RVA: 0x00016160 File Offset: 0x00014360
		public event Action<IPlayerUpgradesStoreManager, int> UpgradeSelected;

		// Token: 0x1400003F RID: 63
		// (add) Token: 0x060006DE RID: 1758 RVA: 0x00016198 File Offset: 0x00014398
		// (remove) Token: 0x060006DF RID: 1759 RVA: 0x000161D0 File Offset: 0x000143D0
		public event Action<IPlayerUpgradesStoreManager, int> UpgradeLevelChanged;

		// Token: 0x060006E0 RID: 1760 RVA: 0x00016205 File Offset: 0x00014405
		private PurchasablePlayerUpgradeData GetSelectedUpgrade(int upgradesSequenceIndex)
		{
			return this.upgradesData.Upgrades[upgradesSequenceIndex].SelectedUpgrade;
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x0001621D File Offset: 0x0001441D
		private IEnumerable<PlayerUpgradeInfo> GetSelectedUpgrades()
		{
			IReadOnlyList<PurchasablePlayerUpgradesSequence> upgrades = this.upgradesData.Upgrades;
			int num;
			for (int i = 0; i < upgrades.Count; i = num + 1)
			{
				PurchasablePlayerUpgradesSequence purchasablePlayerUpgradesSequence = upgrades[i];
				if (purchasablePlayerUpgradesSequence.IsAvailable)
				{
					PurchasablePlayerUpgradeData selectedUpgrade = purchasablePlayerUpgradesSequence.SelectedUpgrade;
					PlayerUpgradeInfo playerUpgradeInfo;
					if (selectedUpgrade != null && selectedUpgrade.TryGetPlayerUpgradeInfo(out playerUpgradeInfo))
					{
						yield return playerUpgradeInfo;
					}
				}
				num = i;
			}
			yield break;
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x00016230 File Offset: 0x00014430
		private void SetStoreState(bool isOpened, IStoreInteractionSpot interactionSpot)
		{
			if (!this.isStoreActive || this.isStoreOpened == isOpened)
			{
				return;
			}
			this.CurrentInteractionSpot = interactionSpot;
			IGameSessionManager gameSessionManager;
			base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager);
			if (isOpened)
			{
				this.upgradesData.CheckMilestones(this.milestoneManager.ReachedMilestones);
				if (gameSessionManager != null)
				{
					gameSessionManager.SetSessionState(SessionState.Freezed);
				}
				GameManager.SetGameFreezed(false);
			}
			else
			{
				this.profileManager.UpdatePurchasedPlayerUpgrades(this);
				this.profileManager.UpdateSelectedPlayerUpgrades(this.GetSelectedUpgrades());
				this.gameManager.SavePlayerProfile((gameSessionManager != null) ? gameSessionManager.CurrentPlayer : null);
				if (gameSessionManager != null)
				{
					gameSessionManager.SetSessionState(SessionState.InProgress);
				}
			}
			this.isStoreOpened = isOpened;
			Action<IStoreManager, bool> storeStateChanged = this.StoreStateChanged;
			if (storeStateChanged == null)
			{
				return;
			}
			storeStateChanged(this, isOpened);
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x000162F2 File Offset: 0x000144F2
		public void OpenStore(IStoreInteractionSpot interactionSpot)
		{
			this.SetStoreState(true, interactionSpot);
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x000162FC File Offset: 0x000144FC
		public void CloseStore()
		{
			this.SetStoreState(false, null);
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x00016306 File Offset: 0x00014506
		public bool SwitchSelectedUpgrade(int upgradesSequenceIndex)
		{
			if (this.upgradesData.Upgrades[upgradesSequenceIndex].TrySelectNextUpgrade())
			{
				Action<IPlayerUpgradesStoreManager, int> upgradeSelected = this.UpgradeSelected;
				if (upgradeSelected != null)
				{
					upgradeSelected(this, upgradesSequenceIndex);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x00016338 File Offset: 0x00014538
		public bool CanBeAdvancedToNextLevel(int upgradesSequenceIndex, out int? upgradeCost)
		{
			PurchasablePlayerUpgradeData selectedUpgrade = this.GetSelectedUpgrade(upgradesSequenceIndex);
			upgradeCost = ((selectedUpgrade != null) ? selectedUpgrade.GetNextLevelCost() : null);
			int currencyAmount = this.profileManager.CurrentPlayerProfile.GetCurrencyAmount(this.storeCurrency);
			int? num = upgradeCost;
			return currencyAmount >= num.GetValueOrDefault() & num != null;
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x00016398 File Offset: 0x00014598
		public bool TryBuyNextUpgrade(int upgradesSequenceIndex)
		{
			PlayerProfile currentPlayerProfile = this.profileManager.CurrentPlayerProfile;
			PurchasablePlayerUpgradeData selectedUpgrade = this.GetSelectedUpgrade(upgradesSequenceIndex);
			int? num;
			if (selectedUpgrade != null && selectedUpgrade.IsAvailableAndUnlocked && this.CanBeAdvancedToNextLevel(upgradesSequenceIndex, out num))
			{
				this.currencyOperationArgs.amount = (float)(-(float)num.Value);
				currentPlayerProfile.TryExecuteCurrencyOperation(this.currencyOperationArgs);
				selectedUpgrade.AdvanceLevel();
				Action<IPlayerUpgradesStoreManager, int> upgradeLevelChanged = this.UpgradeLevelChanged;
				if (upgradeLevelChanged != null)
				{
					upgradeLevelChanged(this, upgradesSequenceIndex);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x00016414 File Offset: 0x00014614
		public void ResetUpgrades()
		{
			IReadOnlyList<PurchasablePlayerUpgradesSequence> upgrades = this.upgradesData.Upgrades;
			int num = 0;
			for (int i = 0; i < upgrades.Count; i++)
			{
				PurchasablePlayerUpgradesSequence purchasablePlayerUpgradesSequence = upgrades[i];
				int totalUpgradesCost = purchasablePlayerUpgradesSequence.GetTotalUpgradesCost();
				if (purchasablePlayerUpgradesSequence.ResetUpgradesLevel())
				{
					num += totalUpgradesCost;
					Action<IPlayerUpgradesStoreManager, int> upgradeLevelChanged = this.UpgradeLevelChanged;
					if (upgradeLevelChanged != null)
					{
						upgradeLevelChanged(this, i);
					}
				}
			}
			this.currencyOperationArgs.amount = (float)num;
			this.profileManager.CurrentPlayerProfile.TryExecuteCurrencyOperation(this.currencyOperationArgs);
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x00016498 File Offset: 0x00014698
		public void LoadData(IReadOnlyList<PlayerUpgradeInfo> purchasedUpgrades, IReadOnlyList<PlayerUpgradeInfo> selectedUpgrades)
		{
			PlayerUpgradesStoreManager.<>c__DisplayClass36_0 CS$<>8__locals1 = new PlayerUpgradesStoreManager.<>c__DisplayClass36_0();
			base.CurrentGame.Services.TryGet<IPlayerUpgradesFactory>(out CS$<>8__locals1.upgradesFactory);
			this.upgradesData.Initialize(purchasedUpgrades, selectedUpgrades, (CS$<>8__locals1.upgradesFactory != null) ? new Predicate<PurchasablePlayerUpgradeData>(CS$<>8__locals1.<LoadData>g__IsUpgradeAvailable|0) : null);
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x000164E8 File Offset: 0x000146E8
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.currencyOperationArgs = new CurrencyOperationArgs
			{
				currencyID = this.storeCurrency
			};
			currentGame.Services.TryGet<GameManager>(out this.gameManager);
			currentGame.Services.TryGet<IPlotMilestoneManager>(out this.milestoneManager);
			if (currentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x00016577 File Offset: 0x00014777
		private void OnPlayerProfileLoaded(PlayerProfile profile)
		{
			this.profileManager.LoadPurchasedPlayerUpgrades(this);
			if (this.isStoreActive)
			{
				this.profileManager.UpdateSelectedPlayerUpgrades(this.GetSelectedUpgrades());
			}
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x0001659E File Offset: 0x0001479E
		public CurrencyID GetStoreCurrencyID()
		{
			return this.storeCurrency;
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x000165A6 File Offset: 0x000147A6
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
		}

		// Token: 0x0400041F RID: 1055
		[SerializeField]
		private CurrencyID storeCurrency;

		// Token: 0x04000420 RID: 1056
		[SerializeField]
		private bool isStoreActive = true;

		// Token: 0x04000421 RID: 1057
		[SerializeField]
		private PlayerUpgradesStoreData upgradesData;

		// Token: 0x04000422 RID: 1058
		private GameManager gameManager;

		// Token: 0x04000423 RID: 1059
		private PlayerProfileManager profileManager;

		// Token: 0x04000424 RID: 1060
		private IPlotMilestoneManager milestoneManager;

		// Token: 0x04000425 RID: 1061
		private bool isStoreOpened;

		// Token: 0x04000426 RID: 1062
		private CurrencyOperationArgs currencyOperationArgs;
	}
}
