using System;
using Common.UnityExtensions;
using Game.Core;
using UltEvents;
using UnityEngine;
using Unliving.Factories;
using Unliving.Purchasing;

namespace Unliving.Pickables
{
	// Token: 0x02000186 RID: 390
	public sealed class PickableNPCBonus : PickableObjectBase<MilestoneItemID>
	{
		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000AD3 RID: 2771 RVA: 0x00023968 File Offset: 0x00021B68
		public override MilestoneItemID ID
		{
			get
			{
				return this.milestoneItemID;
			}
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000AD4 RID: 2772 RVA: 0x00023970 File Offset: 0x00021B70
		// (set) Token: 0x06000AD5 RID: 2773 RVA: 0x00023973 File Offset: 0x00021B73
		public override MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext
		{
			get
			{
				return MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject;
			}
			set
			{
			}
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x00023978 File Offset: 0x00021B78
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			base.PurchasableData = this.GetPurchasableData(this.milestoneItemID);
			IPurchasable purchasableData = base.PurchasableData;
			if (purchasableData != null)
			{
				purchasableData.SetGameData(currentGame);
			}
			this.purchaseManager.ItemPurchased += this.OnItemPurchased;
			this.UpdateState();
		}

		// Token: 0x06000AD7 RID: 2775 RVA: 0x000239CD File Offset: 0x00021BCD
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x000239D0 File Offset: 0x00021BD0
		private void OnItemPurchased(IPurchasable purchasable)
		{
			this.UpdateState();
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x000239D8 File Offset: 0x00021BD8
		private void UpdateState()
		{
			if (base.PurchasableData.Purchased)
			{
				UltEvent ultEvent = this.itemPurchasedEvent;
				if (ultEvent == null)
				{
					return;
				}
				ultEvent.Invoke();
				return;
			}
			else if (base.PurchasableData.Locked)
			{
				UltEvent ultEvent2 = this.itemLockedEvent;
				if (ultEvent2 == null)
				{
					return;
				}
				ultEvent2.Invoke();
				return;
			}
			else
			{
				UltEvent ultEvent3 = this.itemUnlockedEvent;
				if (ultEvent3 == null)
				{
					return;
				}
				ultEvent3.Invoke();
				return;
			}
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x00023A31 File Offset: 0x00021C31
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.purchaseManager.IsNull())
			{
				this.purchaseManager.ItemPurchased -= this.OnItemPurchased;
			}
		}

		// Token: 0x04000638 RID: 1592
		[SerializeField]
		private MilestoneItemID milestoneItemID;

		// Token: 0x04000639 RID: 1593
		[SerializeField]
		private UltEvent itemLockedEvent;

		// Token: 0x0400063A RID: 1594
		[SerializeField]
		private UltEvent itemUnlockedEvent;

		// Token: 0x0400063B RID: 1595
		[SerializeField]
		private UltEvent itemPurchasedEvent;
	}
}
