using System;
using System.Collections.Generic;
using Common;
using Common.Editor;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.PlayerProfileManagement;

namespace Unliving.Purchasing
{
	// Token: 0x020000EC RID: 236
	[Serializable]
	public abstract class PurchasableItemBase : IPurchasable, ICloneable<IPurchasable>, ILeveledItem, IItemLevelProvider, IEquatable<IPurchasable>
	{
		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060005AD RID: 1453
		public abstract object ObjectID { get; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x060005AE RID: 1454 RVA: 0x00013EE0 File Offset: 0x000120E0
		public bool Locked
		{
			get
			{
				Context context = this.context;
				if (((context != null) ? context.purchaseManager : null) != null && this.context.purchaseManager.DebugUnlockAll)
				{
					return false;
				}
				if (this.trigger != null && !this.trigger.IsFired(this.context))
				{
					return true;
				}
				if (this.parents == null || this.parents.Count == 0)
				{
					return false;
				}
				using (List<IPurchasable>.Enumerator enumerator = this.parents.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator.Current.Purchased)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x00013F9C File Offset: 0x0001219C
		public virtual bool Purchased
		{
			get
			{
				Context context = this.context;
				return (((context != null) ? context.purchaseManager : null) != null && this.context.purchaseManager.DebugUnlockAll) || this.purchased;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x060005B0 RID: 1456 RVA: 0x00013FD2 File Offset: 0x000121D2
		// (set) Token: 0x060005B1 RID: 1457 RVA: 0x00013FDA File Offset: 0x000121DA
		public List<IPurchasable> Parents
		{
			get
			{
				return this.parents;
			}
			set
			{
				this.parents = value;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x060005B2 RID: 1458 RVA: 0x00013FE3 File Offset: 0x000121E3
		public IPurchasableUnlockTrigger Trigger
		{
			get
			{
				return this.trigger;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x00013FEC File Offset: 0x000121EC
		// (set) Token: 0x060005B4 RID: 1460 RVA: 0x0001404E File Offset: 0x0001224E
		public virtual CurrencyOperationArgs UnlockArgs
		{
			get
			{
				List<PurchasableCurrencyArgsModifierBase> list;
				if (this.context != null && this.context.purchaseManager.TryGetPriceModifiers(this, out list))
				{
					ICurrencyOperationArgs currencyOperationArgs = this.unlockArgs.Clone();
					for (int i = 0; i < list.Count; i++)
					{
						list[i].ApplyUnlockArgsModification(ref currencyOperationArgs);
					}
					return (CurrencyOperationArgs)currencyOperationArgs;
				}
				return this.unlockArgs;
			}
			set
			{
				this.unlockArgs = value;
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x060005B5 RID: 1461 RVA: 0x00014058 File Offset: 0x00012258
		// (set) Token: 0x060005B6 RID: 1462 RVA: 0x000140BA File Offset: 0x000122BA
		public CurrencyOperationArgs BuyArgs
		{
			get
			{
				List<PurchasableCurrencyArgsModifierBase> list;
				if (this.context != null && this.context.purchaseManager.TryGetPriceModifiers(this, out list))
				{
					ICurrencyOperationArgs currencyOperationArgs = this.buyArgs.Clone();
					for (int i = 0; i < list.Count; i++)
					{
						list[i].ApplyBuyArgsModification(ref currencyOperationArgs);
					}
					return (CurrencyOperationArgs)currencyOperationArgs;
				}
				return this.buyArgs;
			}
			set
			{
				this.buyArgs = value;
			}
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x060005B7 RID: 1463 RVA: 0x000140C3 File Offset: 0x000122C3
		// (set) Token: 0x060005B8 RID: 1464 RVA: 0x000140CB File Offset: 0x000122CB
		public int ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
			set
			{
				this.itemLevel = value;
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x060005B9 RID: 1465 RVA: 0x000140D4 File Offset: 0x000122D4
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x000140DC File Offset: 0x000122DC
		public virtual void SetGameData(IGame currentGame)
		{
			PlayerProfileManager playerProfileManager;
			currentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager);
			PurchaseManager purchaseManager;
			currentGame.Services.TryGet<PurchaseManager>(out purchaseManager);
			this.context = new Context
			{
				game = currentGame,
				purchaseManager = purchaseManager,
				playerProfileManager = playerProfileManager,
				requester = this.ObjectID.ToString()
			};
			this.unlockArgs.sender = this.ObjectID;
			this.buyArgs.sender = this.ObjectID;
			this.UpdateParentData();
		}

		// Token: 0x060005BB RID: 1467
		public abstract IPurchasableData GetPurchasableData();

		// Token: 0x060005BC RID: 1468
		public abstract void SetPurchasableData(IPurchasableData data);

		// Token: 0x060005BD RID: 1469 RVA: 0x00014160 File Offset: 0x00012360
		public virtual bool TryPurchase(MultiRepresentationObjectInstantiator.ObjectType pickingContext)
		{
			if (this.Locked)
			{
				return false;
			}
			PlayerProfile currentPlayerProfile = this.context.playerProfileManager.CurrentPlayerProfile;
			if (pickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				if (this.Purchased)
				{
					return false;
				}
				if (currentPlayerProfile.TryExecuteCurrencyOperation(this.UnlockArgs))
				{
					this.purchased = true;
					this.context.purchaseManager.PurchaseItem(this);
					this.context.playerProfileManager.UpdatePlayerPurchasements(this.context.purchaseManager);
					return true;
				}
			}
			else if (pickingContext == MultiRepresentationObjectInstantiator.ObjectType.StoreObject)
			{
				return this.Purchased && currentPlayerProfile.TryExecuteCurrencyOperation(this.BuyArgs);
			}
			return false;
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x000141FF File Offset: 0x000123FF
		public void ChangePurchaseState(bool state)
		{
			this.purchased = state;
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x00014208 File Offset: 0x00012408
		public bool CanBePurchased(MultiRepresentationObjectInstantiator.ObjectType pickingContext)
		{
			PlayerProfile currentPlayerProfile = this.context.playerProfileManager.CurrentPlayerProfile;
			if (pickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return this.Purchased || (float)currentPlayerProfile.GetCurrencyAmount(this.UnlockArgs.currencyID) >= this.UnlockArgs.amount;
			}
			return pickingContext != MultiRepresentationObjectInstantiator.ObjectType.StoreObject || (float)currentPlayerProfile.GetCurrencyAmount(this.BuyArgs.currencyID) >= this.BuyArgs.amount;
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x00014280 File Offset: 0x00012480
		private void UpdateParentData()
		{
			if (this.parents == null)
			{
				return;
			}
			for (int i = 0; i < this.parents.Count; i++)
			{
				IPurchasable purchasable;
				if (this.context.purchaseManager.TryGetPurchasable(this.parents[i].ObjectID, out purchasable))
				{
					this.parents[i] = (purchasable as PurchasableItemBase);
				}
			}
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x000142E3 File Offset: 0x000124E3
		public IPurchasable Clone()
		{
			return (IPurchasable)base.MemberwiseClone();
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x000142F0 File Offset: 0x000124F0
		public bool Equals(IPurchasable other)
		{
			return object.Equals(base.GetType(), other.GetType()) && object.Equals(this.ObjectID, other.ObjectID);
		}

		// Token: 0x040003CD RID: 973
		[SerializeReference]
		[HideInInspector]
		private List<IPurchasable> parents;

		// Token: 0x040003CE RID: 974
		[SerializeReference]
		[ManagedObjectField(typeof(PurchasableUnlockTriggerBase))]
		private PurchasableUnlockTriggerBase trigger;

		// Token: 0x040003CF RID: 975
		[SerializeField]
		private CurrencyOperationArgs unlockArgs;

		// Token: 0x040003D0 RID: 976
		[SerializeField]
		private CurrencyOperationArgs buyArgs;

		// Token: 0x040003D1 RID: 977
		[SerializeField]
		private bool purchased;

		// Token: 0x040003D2 RID: 978
		[SerializeField]
		private int itemLevel;

		// Token: 0x040003D3 RID: 979
		protected Context context;
	}
}
