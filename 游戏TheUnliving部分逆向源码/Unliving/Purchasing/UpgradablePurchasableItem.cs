using System;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Factories;

namespace Unliving.Purchasing
{
	// Token: 0x020000F5 RID: 245
	public abstract class UpgradablePurchasableItem<T> : PurchasableItem<T>, IUpgradable where T : Enum
	{
		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060005DF RID: 1503 RVA: 0x00014592 File Offset: 0x00012792
		public bool IsMaxLevelReached
		{
			get
			{
				return base.ItemLevel >= this.upgradesPrices.Length;
			}
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x000145A7 File Offset: 0x000127A7
		public override void SetGameData(IGame currentGame)
		{
			base.SetGameData(currentGame);
			this.upgradesPrices = this.context.purchaseManager.GetUpgradesPrices(base.GetType());
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x000145CC File Offset: 0x000127CC
		public bool TryUpgrade()
		{
			if (!this.Purchased)
			{
				return false;
			}
			if (this.context.playerProfileManager.CurrentPlayerProfile.TryExecuteCurrencyOperation(this.GetCurrentUpgradeCurrencyArgs()))
			{
				base.ItemLevel = Mathf.Clamp(base.ItemLevel + 1, 1, this.upgradesPrices.Length);
				this.context.purchaseManager.PurchaseItem(this);
				this.context.playerProfileManager.UpdatePlayerPurchasements(this.context.purchaseManager);
				return true;
			}
			return false;
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x00014650 File Offset: 0x00012850
		public override bool TryPurchase(MultiRepresentationObjectInstantiator.ObjectType pickingContext)
		{
			if (base.TryPurchase(pickingContext))
			{
				base.ItemLevel = 1;
				return true;
			}
			return false;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x00014668 File Offset: 0x00012868
		public CurrencyOperationArgs GetCurrentUpgradeCurrencyArgs()
		{
			int num = Mathf.Clamp(base.ItemLevel - 1, 0, this.upgradesPrices.Length - 1);
			return this.upgradesPrices[num];
		}

		// Token: 0x040003E5 RID: 997
		private CurrencyOperationArgs[] upgradesPrices;
	}
}
