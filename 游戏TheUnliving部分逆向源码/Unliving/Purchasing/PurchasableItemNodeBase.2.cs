using System;
using System.Collections.Generic;
using GraphProcessor;

namespace Unliving.Purchasing
{
	// Token: 0x02000108 RID: 264
	public abstract class PurchasableItemNodeBase<TObject, TObjectID> : PurchasableItemNodeBase where TObject : class, IPurchasable where TObjectID : Enum
	{
		// Token: 0x17000107 RID: 263
		// (get) Token: 0x0600066C RID: 1644 RVA: 0x000153B3 File Offset: 0x000135B3
		public override string Name
		{
			get
			{
				return this.GetObjectID().ToString();
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x0600066D RID: 1645 RVA: 0x000153C0 File Offset: 0x000135C0
		public override string name
		{
			get
			{
				return this.Name;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600066E RID: 1646 RVA: 0x000153C8 File Offset: 0x000135C8
		public override IPurchasable Data
		{
			get
			{
				return this.data;
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x000153D5 File Offset: 0x000135D5
		protected override void Enable()
		{
			base.Enable();
			base.onAfterEdgeConnected += delegate(SerializableEdge edge)
			{
				this.UpdateParents();
			};
			base.onAfterEdgeDisconnected += delegate(SerializableEdge edge)
			{
				this.UpdateParents();
			};
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x00015404 File Offset: 0x00013604
		public override void UpdateParents()
		{
			if (this.HasDefaultID())
			{
				return;
			}
			List<IPurchasable> list = new List<IPurchasable>();
			foreach (BaseNode baseNode in base.GetInputNodes())
			{
				IPurchasableItemNode purchasableItemNode = baseNode as IPurchasableItemNode;
				if (purchasableItemNode != null && !purchasableItemNode.HasDefaultID())
				{
					list.Add(purchasableItemNode.Data);
				}
			}
			this.data.Parents = list;
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x00015488 File Offset: 0x00013688
		public override void OnNodeCreated()
		{
			base.OnNodeCreated();
			this.SetDefaultID();
		}

		// Token: 0x04000402 RID: 1026
		public TObject data;
	}
}
