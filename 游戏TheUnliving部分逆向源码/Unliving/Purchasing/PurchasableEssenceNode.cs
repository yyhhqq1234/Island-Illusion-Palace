using System;
using GraphProcessor;
using Unliving.Essence;

namespace Unliving.Purchasing
{
	// Token: 0x02000106 RID: 262
	[NodeMenuItem("Purchase Manager/Purchasable Essence Node", null)]
	[Serializable]
	public class PurchasableEssenceNode : PurchasableItemNodeBase<PurchasableEssence, EssenceType>
	{
		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x000152C5 File Offset: 0x000134C5
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x000152C8 File Offset: 0x000134C8
		public override void SetID(object id)
		{
			if (id is EssenceType)
			{
				EssenceType purchaseItem = (EssenceType)id;
				EssenceType essenceType = this.essenceType;
				this.essenceType = purchaseItem;
				if (this.data == null)
				{
					this.data = new PurchasableEssence(purchaseItem);
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
				graph2.OnNodeIDChanged(this, essenceType);
			}
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0001533D File Offset: 0x0001353D
		public override object GetObjectID()
		{
			return this.essenceType;
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0001534A File Offset: 0x0001354A
		public override bool HasDefaultID()
		{
			return this.essenceType == (EssenceType)this.GetDefaultID();
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0001535F File Offset: 0x0001355F
		public override object GetDefaultID()
		{
			return EssenceType.None;
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x00015368 File Offset: 0x00013568
		public override void SetDefaultID()
		{
			object defaultID = this.GetDefaultID();
			this.SetID(defaultID);
			this.essenceType = (EssenceType)defaultID;
		}

		// Token: 0x040003FF RID: 1023
		public EssenceType essenceType;
	}
}
