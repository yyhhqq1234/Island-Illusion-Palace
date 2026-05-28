using System;
using System.Linq;
using Common.UnityExtensions;
using GraphProcessor;

namespace Unliving.Purchasing
{
	// Token: 0x0200010D RID: 269
	public class PurchaseManagerGraph : BaseGraph
	{
		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000691 RID: 1681 RVA: 0x000157F1 File Offset: 0x000139F1
		public bool IsValidScene
		{
			get
			{
				return !this.purchaseManagerSceneObject.IsNull();
			}
		}

		// Token: 0x14000039 RID: 57
		// (add) Token: 0x06000692 RID: 1682 RVA: 0x00015804 File Offset: 0x00013A04
		// (remove) Token: 0x06000693 RID: 1683 RVA: 0x0001583C File Offset: 0x00013A3C
		public event Action<object, object, PurchasableItemNodeBase> NodeIDChanged;

		// Token: 0x1400003A RID: 58
		// (add) Token: 0x06000694 RID: 1684 RVA: 0x00015874 File Offset: 0x00013A74
		// (remove) Token: 0x06000695 RID: 1685 RVA: 0x000158AC File Offset: 0x00013AAC
		public event Action PurchaseStateChanged;

		// Token: 0x06000696 RID: 1686 RVA: 0x000158E4 File Offset: 0x00013AE4
		public void OnNodeIDChanged(PurchasableItemNodeBase node, object oldID)
		{
			Action<object, object, PurchasableItemNodeBase> nodeIDChanged = this.NodeIDChanged;
			if (nodeIDChanged != null)
			{
				nodeIDChanged(oldID, node.GetObjectID(), node);
			}
			foreach (BaseNode baseNode in node.GetOutputNodes())
			{
				PurchasableItemNodeBase purchasableItemNodeBase = baseNode as PurchasableItemNodeBase;
				if (purchasableItemNodeBase != null)
				{
					purchasableItemNodeBase.UpdateParents();
				}
			}
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00015954 File Offset: 0x00013B54
		public void OnPurchaseStateChanged()
		{
			Action purchaseStateChanged = this.PurchaseStateChanged;
			if (purchaseStateChanged == null)
			{
				return;
			}
			purchaseStateChanged();
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x00015968 File Offset: 0x00013B68
		public bool TryGetPurchasable(object itemID, out IPurchasable item)
		{
			IPurchasableItemNode purchasableItemNode;
			if (this.TryGetNode(itemID, out purchasableItemNode))
			{
				item = purchasableItemNode.Data;
				return true;
			}
			item = null;
			return false;
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x00015990 File Offset: 0x00013B90
		public bool TryGetNode(object itemID, out IPurchasableItemNode itemNode)
		{
			for (int i = 0; i < this.nodes.Count; i++)
			{
				IPurchasableItemNode purchasableItemNode = this.nodes[i] as IPurchasableItemNode;
				if (purchasableItemNode != null && purchasableItemNode.Data != null && purchasableItemNode.Data.ObjectID.Equals(itemID))
				{
					itemNode = purchasableItemNode;
					return true;
				}
			}
			itemNode = null;
			return false;
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x000159EC File Offset: 0x00013BEC
		public void ConvertNode<T>(PurchasableItemNodeBase originalNode) where T : PurchasableItemNodeBase
		{
			if (originalNode is T)
			{
				return;
			}
			T t = (T)((object)Activator.CreateInstance(typeof(T)));
			t.SetDefaultID();
			t.Data.BuyArgs = originalNode.Data.BuyArgs;
			t.Data.UnlockArgs = originalNode.Data.UnlockArgs;
			t.OnNodeCreated();
			string guid = originalNode.GUID;
			originalNode.GUID = Guid.NewGuid().ToString();
			this.nodesPerGUID.Add(originalNode.GUID, originalNode);
			t.GUID = guid;
			base.AddNode(t);
			foreach (NodePort outputPort in (from e in originalNode.inputPorts[0].GetEdges()
			select e.outputPort).ToArray<NodePort>())
			{
				base.Connect(t.inputPorts[0], outputPort, false);
			}
			foreach (NodePort inputPort in (from e in originalNode.outputPorts[0].GetEdges()
			select e.inputPort).ToArray<NodePort>())
			{
				base.Connect(inputPort, t.outputPorts[0], false);
			}
			t.position = originalNode.position;
			base.RemoveNode(originalNode);
		}

		// Token: 0x04000409 RID: 1033
		public PurchaseManagerSceneObject purchaseManagerSceneObject;
	}
}
