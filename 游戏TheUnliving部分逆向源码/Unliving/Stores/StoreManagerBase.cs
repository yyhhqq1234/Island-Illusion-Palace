using System;
using Game.Core;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.Purchasing;

namespace Unliving.Stores
{
	// Token: 0x0200004F RID: 79
	public abstract class StoreManagerBase : GlobalManagerBase, IPurchasableBasedStoreManager, IStoreManager
	{
		// Token: 0x14000029 RID: 41
		// (add) Token: 0x06000280 RID: 640 RVA: 0x00009EF4 File Offset: 0x000080F4
		// (remove) Token: 0x06000281 RID: 641 RVA: 0x00009F2C File Offset: 0x0000812C
		public event Action<IStoreManager, bool> StoreStateChanged;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x06000282 RID: 642 RVA: 0x00009F64 File Offset: 0x00008164
		// (remove) Token: 0x06000283 RID: 643 RVA: 0x00009F9C File Offset: 0x0000819C
		public event Action<IPurchasable> StoreItemPurchased;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x06000284 RID: 644 RVA: 0x00009FD4 File Offset: 0x000081D4
		// (remove) Token: 0x06000285 RID: 645 RVA: 0x0000A00C File Offset: 0x0000820C
		public event Action<IPurchasable> StoreItemUpgraded;

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000286 RID: 646 RVA: 0x0000A041 File Offset: 0x00008241
		// (set) Token: 0x06000287 RID: 647 RVA: 0x0000A049 File Offset: 0x00008249
		public IStoreInteractionSpot CurrentInteractionSpot { get; private set; }

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000288 RID: 648 RVA: 0x0000A052 File Offset: 0x00008252
		public bool IsStoreOpened
		{
			get
			{
				return this.isStoreOpened;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000289 RID: 649
		public abstract CurrencyID CurrencyID { get; }

		// Token: 0x0600028A RID: 650 RVA: 0x0000A05A File Offset: 0x0000825A
		public void OpenStore(IStoreInteractionSpot interactionSpot)
		{
			this.SetStoreState(true, interactionSpot);
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000A064 File Offset: 0x00008264
		public void CloseStore()
		{
			this.SetStoreState(false, null);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0000A06E File Offset: 0x0000826E
		public virtual bool TryPurchaseItem(IPurchasable item)
		{
			if (!item.Locked && item.CanBePurchased(MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject) && item.TryPurchase(MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject))
			{
				Action<IPurchasable> storeItemPurchased = this.StoreItemPurchased;
				if (storeItemPurchased != null)
				{
					storeItemPurchased(item);
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0000A0A0 File Offset: 0x000082A0
		public bool TryUpgradeItem(IPurchasable item)
		{
			IUpgradable upgradable = item as IUpgradable;
			if (upgradable != null && !upgradable.IsMaxLevelReached && upgradable.TryUpgrade())
			{
				Action<IPurchasable> storeItemUpgraded = this.StoreItemUpgraded;
				if (storeItemUpgraded != null)
				{
					storeItemUpgraded(item);
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0000A0DC File Offset: 0x000082DC
		private void SetStoreState(bool isOpened, IStoreInteractionSpot interactionSpot)
		{
			if (this.isStoreOpened == isOpened)
			{
				return;
			}
			this.CurrentInteractionSpot = interactionSpot;
			IGameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager))
			{
				SessionState sessionState = isOpened ? SessionState.Freezed : SessionState.InProgress;
				gameSessionManager.SetSessionState(sessionState);
				GameManager.SetGameFreezed(false);
			}
			this.isStoreOpened = isOpened;
			Action<IStoreManager, bool> storeStateChanged = this.StoreStateChanged;
			if (storeStateChanged == null)
			{
				return;
			}
			storeStateChanged(this, isOpened);
		}

		// Token: 0x0400017B RID: 379
		private const MultiRepresentationObjectInstantiator.ObjectType PickingContext = MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject;

		// Token: 0x04000180 RID: 384
		private bool isStoreOpened;
	}
}
