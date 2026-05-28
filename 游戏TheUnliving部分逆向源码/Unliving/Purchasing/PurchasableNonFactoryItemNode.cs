using System;
using GraphProcessor;
using Unliving.Pickables;

namespace Unliving.Purchasing
{
	// Token: 0x0200010B RID: 267
	[NodeMenuItem("Purchase Manager/Purchasable Non-Factory Item Node", null)]
	[Serializable]
	public class PurchasableNonFactoryItemNode : PurchasableItemNodeBase<PurchasableNonFactoryItem, NonFactoryPickableType>
	{
		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000683 RID: 1667 RVA: 0x00015663 File Offset: 0x00013863
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x00015668 File Offset: 0x00013868
		public override void SetID(object id)
		{
			if (id is NonFactoryPickableType)
			{
				NonFactoryPickableType purchaseItem = (NonFactoryPickableType)id;
				NonFactoryPickableType nonFactoryPickableType = this.itemType;
				this.itemType = purchaseItem;
				if (this.data == null)
				{
					this.data = new PurchasableNonFactoryItem(purchaseItem);
				}
				else
				{
					this.data.PurchaseItem = purchaseItem;
				}
				BaseGraph graph = this.graph;
				if (graph != null)
				{
					graph.NotifyNodeChanged(this);
				}
				PurchaseManagerGraph graph2 = base.Graph;
				if (graph2 == null)
				{
					return;
				}
				graph2.OnNodeIDChanged(this, nonFactoryPickableType);
			}
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x000156DD File Offset: 0x000138DD
		public override object GetObjectID()
		{
			return this.itemType;
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x000156EA File Offset: 0x000138EA
		public override bool HasDefaultID()
		{
			return this.itemType == (NonFactoryPickableType)this.GetDefaultID();
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x000156FF File Offset: 0x000138FF
		public override object GetDefaultID()
		{
			return NonFactoryPickableType.None;
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00015708 File Offset: 0x00013908
		public override void SetDefaultID()
		{
			object defaultID = this.GetDefaultID();
			this.SetID(defaultID);
			this.itemType = (NonFactoryPickableType)defaultID;
		}

		// Token: 0x04000405 RID: 1029
		public NonFactoryPickableType itemType;
	}
}
