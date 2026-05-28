using System;
using GraphProcessor;
using Unliving.Player.Upgrades;

namespace Unliving.Purchasing
{
	// Token: 0x0200010C RID: 268
	[NodeMenuItem("Purchase Manager/Purchasable Player Upgrade Node", null)]
	[Serializable]
	public class PurchasablePlayerUpgradeNode : PurchasableItemNodeBase<PurchasablePlayerUpgrade, PlayerUpgradeID>
	{
		// Token: 0x1700010D RID: 269
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x00015737 File Offset: 0x00013937
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0001573C File Offset: 0x0001393C
		public override void SetID(object id)
		{
			if (id is PlayerUpgradeID)
			{
				PlayerUpgradeID purchaseItem = (PlayerUpgradeID)id;
				PlayerUpgradeID playerUpgradeID = this.playerUpgradeID;
				this.playerUpgradeID = purchaseItem;
				if (this.data == null)
				{
					this.data = new PurchasablePlayerUpgrade(purchaseItem);
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
				graph2.OnNodeIDChanged(this, playerUpgradeID);
			}
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x000157B1 File Offset: 0x000139B1
		public override object GetObjectID()
		{
			return this.playerUpgradeID;
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x000157BE File Offset: 0x000139BE
		public override bool HasDefaultID()
		{
			return this.playerUpgradeID == (PlayerUpgradeID)this.GetDefaultID();
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x000157D3 File Offset: 0x000139D3
		public override object GetDefaultID()
		{
			return PlayerUpgradeID.None;
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x000157DB File Offset: 0x000139DB
		public override void SetDefaultID()
		{
			this.SetID(this.GetDefaultID());
		}

		// Token: 0x04000406 RID: 1030
		public PlayerUpgradeID playerUpgradeID;
	}
}
