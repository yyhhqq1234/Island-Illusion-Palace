using System;
using GraphProcessor;
using UnityEngine;

namespace Unliving.Purchasing
{
	// Token: 0x02000109 RID: 265
	[NodeMenuItem("Purchase Manager/Purchasable Milestone Item Node", null)]
	[Serializable]
	public class PurchasableMilestoneItemNode : PurchasableItemNodeBase<PurchasableMilestoneItem, MilestoneItemID>
	{
		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06000675 RID: 1653 RVA: 0x000154AE File Offset: 0x000136AE
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x000154B4 File Offset: 0x000136B4
		public override void SetID(object id)
		{
			if (id is MilestoneItemID)
			{
				MilestoneItemID milestoneItemID = (MilestoneItemID)id;
				MilestoneItemID milestoneItemID2 = this.milestoneID;
				this.milestoneID = milestoneItemID;
				if (this.data == null)
				{
					this.data = new PurchasableMilestoneItem();
				}
				this.data.milestoneID = this.milestoneID;
				Debug.Log((this.data == null).ToString() + " " + this.milestoneID.ToString());
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
				graph2.OnNodeIDChanged(this, milestoneItemID2);
			}
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0001555F File Offset: 0x0001375F
		public override object GetObjectID()
		{
			return this.milestoneID;
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x0001556C File Offset: 0x0001376C
		public override bool HasDefaultID()
		{
			return this.milestoneID == MilestoneItemID.None;
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x00015577 File Offset: 0x00013777
		public override object GetDefaultID()
		{
			return MilestoneItemID.None;
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x00015580 File Offset: 0x00013780
		public override void SetDefaultID()
		{
			object defaultID = this.GetDefaultID();
			this.SetID(defaultID);
		}

		// Token: 0x04000403 RID: 1027
		public MilestoneItemID milestoneID;
	}
}
