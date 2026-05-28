using System;
using System.Collections;
using Common.Editor;
using Game.Core;
using UnityEngine;
using Unliving.LeveledItems;
using Unliving.Purchasing;

namespace Unliving.Player.Upgrades
{
	// Token: 0x0200016C RID: 364
	[CreateAssetMenu(fileName = "PurchasableCurrencyArgsModificationPlayerUpgrade", menuName = "Game/Player/Player Upgrade/Purchasable Currency Args Modification Upgrade")]
	public class PurchasableCurrencyArgsModificationPlayerUpgrade : ScriptableObject, IPlayerUpgrade, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000A16 RID: 2582 RVA: 0x00021F4F File Offset: 0x0002014F
		// (set) Token: 0x06000A17 RID: 2583 RVA: 0x00021F57 File Offset: 0x00020157
		public IPlayerUpgrade UpgradePrototype { get; private set; }

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000A18 RID: 2584 RVA: 0x00021F60 File Offset: 0x00020160
		// (set) Token: 0x06000A19 RID: 2585 RVA: 0x00021F63 File Offset: 0x00020163
		public int ItemLevel
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000A1A RID: 2586 RVA: 0x00021F65 File Offset: 0x00020165
		// (set) Token: 0x06000A1B RID: 2587 RVA: 0x00021F6D File Offset: 0x0002016D
		public bool IsActivated { get; private set; }

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000A1C RID: 2588 RVA: 0x00021F76 File Offset: 0x00020176
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x00021F79 File Offset: 0x00020179
		public IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext)
		{
			if (this.IsActivated)
			{
				yield break;
			}
			this.IsActivated = true;
			this.currentGame = (activationContext as IGame);
			PurchaseManager purchaseManager;
			if (this.currentGame.Services.TryGet<PurchaseManager>(out purchaseManager))
			{
				for (int i = 0; i < this.priceModifiers.Length; i++)
				{
					purchaseManager.AddCurrencyArgsModifier(this.priceModifiers[i]);
				}
			}
			yield break;
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x00021F8F File Offset: 0x0002018F
		public IPlayerUpgrade Clone()
		{
			PurchasableCurrencyArgsModificationPlayerUpgrade purchasableCurrencyArgsModificationPlayerUpgrade = (PurchasableCurrencyArgsModificationPlayerUpgrade)base.MemberwiseClone();
			purchasableCurrencyArgsModificationPlayerUpgrade.UpgradePrototype = (this.UpgradePrototype ?? this);
			return purchasableCurrencyArgsModificationPlayerUpgrade;
		}

		// Token: 0x040005F7 RID: 1527
		[SerializeReference]
		[ManagedObjectField(typeof(PurchasableCurrencyArgsModifierBase))]
		public PurchasableCurrencyArgsModifierBase[] priceModifiers;

		// Token: 0x040005F8 RID: 1528
		private IGame currentGame;
	}
}
