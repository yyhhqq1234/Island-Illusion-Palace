using System;
using System.Text;
using UnityEngine;
using Unliving.Purchasing;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F6 RID: 758
	public abstract class PurchasableItemStatusTriggerBase<ObjectID> : CharacterPlotItemTriggerBase where ObjectID : Enum
	{
		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x060019E2 RID: 6626
		protected abstract ObjectID[] PurchasableItems { get; }

		// Token: 0x060019E3 RID: 6627 RVA: 0x00050F9C File Offset: 0x0004F19C
		protected override bool ShouldBeIgnored()
		{
			return false;
		}

		// Token: 0x060019E4 RID: 6628 RVA: 0x00050F9F File Offset: 0x0004F19F
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return base.GetType().Name + "(" + string.Join<ObjectID>(" ", this.PurchasableItems) + ")";
		}

		// Token: 0x060019E5 RID: 6629 RVA: 0x00050FCC File Offset: 0x0004F1CC
		public override float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			targetValue = (float)this.PurchasableItems.Length;
			currentValue = 0f;
			PurchaseManager purchaseManager;
			if (context.currentGame.Services.TryGet<PurchaseManager>(out purchaseManager))
			{
				foreach (ObjectID objectID in this.PurchasableItems)
				{
					IPurchasable purchasable;
					if (purchaseManager.TryGetPurchasable(objectID, out purchasable))
					{
						if (this.targetItemStatus == PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus.Unlocked && !purchasable.Locked && !purchasable.Purchased)
						{
							currentValue += 1f;
						}
						if (this.targetItemStatus == PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus.Locked && purchasable.Locked)
						{
							currentValue += 1f;
						}
						if (this.targetItemStatus == PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus.Purchased && purchasable.Purchased)
						{
							currentValue += 1f;
						}
					}
				}
			}
			return Mathf.Clamp01(currentValue / targetValue);
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x00051098 File Offset: 0x0004F298
		protected override bool GetState(CharacterPlotContext context)
		{
			PurchaseManager purchaseManager;
			if (context.currentGame.Services.TryGet<PurchaseManager>(out purchaseManager))
			{
				foreach (ObjectID objectID in this.PurchasableItems)
				{
					IPurchasable purchasable;
					if (!purchaseManager.TryGetPurchasable(objectID, out purchasable))
					{
						return false;
					}
					if (this.targetItemStatus == PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus.Unlocked && (purchasable.Locked || purchasable.Purchased))
					{
						return false;
					}
					if (this.targetItemStatus == PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus.Locked && !purchasable.Locked)
					{
						return false;
					}
					if (this.targetItemStatus == PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus.Purchased && !purchasable.Purchased)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x04000E6E RID: 3694
		public PurchasableItemStatusTriggerBase<ObjectID>.PurchasableItemStatus targetItemStatus;

		// Token: 0x02000540 RID: 1344
		public enum PurchasableItemStatus
		{
			// Token: 0x04001B8E RID: 7054
			Locked,
			// Token: 0x04001B8F RID: 7055
			Unlocked,
			// Token: 0x04001B90 RID: 7056
			Purchased
		}
	}
}
