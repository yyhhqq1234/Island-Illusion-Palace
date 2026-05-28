using System;
using Unliving.Currencies;

namespace Unliving.Purchasing
{
	// Token: 0x020000E4 RID: 228
	public abstract class PurchasableCurrencyArgsModifier<TObjectType, TObjectID> : PurchasableCurrencyArgsModifierBase where TObjectType : IPurchasable where TObjectID : Enum
	{
		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000594 RID: 1428 RVA: 0x00013C64 File Offset: 0x00011E64
		public Type PurchasableType
		{
			get
			{
				return typeof(TObjectType);
			}
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x00013C70 File Offset: 0x00011E70
		public override bool IsMatch(IPurchasable purchasable)
		{
			if (this.isAny)
			{
				return object.Equals(this.PurchasableType, purchasable.GetType());
			}
			return this.purchasableIDs.Length != 0 && this.purchasableIDs.Contains((TObjectID)((object)purchasable.ObjectID));
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x00013CAD File Offset: 0x00011EAD
		public override void ApplyBuyArgsModification(ref ICurrencyOperationArgs buyArgs)
		{
			if (this.changingPrice == PurchasableCurrencyArgsModifier<TObjectType, TObjectID>.ChangingPriceType.InGameStorePrice)
			{
				buyArgs.Amount += (float)this.amount;
			}
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x00013CCC File Offset: 0x00011ECC
		public override void ApplyUnlockArgsModification(ref ICurrencyOperationArgs unlockArgs)
		{
			if (this.changingPrice == PurchasableCurrencyArgsModifier<TObjectType, TObjectID>.ChangingPriceType.UnlockPrice)
			{
				unlockArgs.Amount += (float)this.amount;
			}
		}

		// Token: 0x040003C0 RID: 960
		public PurchasableCurrencyArgsModifier<TObjectType, TObjectID>.ChangingPriceType changingPrice;

		// Token: 0x040003C1 RID: 961
		public bool isAny;

		// Token: 0x040003C2 RID: 962
		public TObjectID[] purchasableIDs;

		// Token: 0x040003C3 RID: 963
		public int amount;

		// Token: 0x0200042E RID: 1070
		public enum ChangingPriceType
		{
			// Token: 0x0400162F RID: 5679
			InGameStorePrice,
			// Token: 0x04001630 RID: 5680
			UnlockPrice
		}
	}
}
