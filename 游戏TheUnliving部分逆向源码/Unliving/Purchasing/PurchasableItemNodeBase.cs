using System;
using GraphProcessor;
using UnityEngine;

namespace Unliving.Purchasing
{
	// Token: 0x02000107 RID: 263
	public abstract class PurchasableItemNodeBase : BaseNode, IPurchasableItemNode
	{
		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000660 RID: 1632 RVA: 0x00015397 File Offset: 0x00013597
		public PurchaseManagerGraph Graph
		{
			get
			{
				return this.graph as PurchaseManagerGraph;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000661 RID: 1633 RVA: 0x000153A4 File Offset: 0x000135A4
		public override string layoutStyle
		{
			get
			{
				return "PurchaseManagerNodesStyle";
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000662 RID: 1634
		public abstract bool HomespaceSpawnerRequired { get; }

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06000663 RID: 1635
		public abstract string Name { get; }

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000664 RID: 1636
		public abstract IPurchasable Data { get; }

		// Token: 0x06000665 RID: 1637
		public abstract object GetObjectID();

		// Token: 0x06000666 RID: 1638
		public abstract object GetDefaultID();

		// Token: 0x06000667 RID: 1639
		public abstract void SetDefaultID();

		// Token: 0x06000668 RID: 1640
		public abstract bool HasDefaultID();

		// Token: 0x06000669 RID: 1641
		public abstract void SetID(object id);

		// Token: 0x0600066A RID: 1642
		public abstract void UpdateParents();

		// Token: 0x04000400 RID: 1024
		[Output(null, true, name = "Childs", allowMultiple = true)]
		public PurchasableItemNodeBase output;

		// Token: 0x04000401 RID: 1025
		[Input(null, false, name = "Parent", allowMultiple = true)]
		[SerializeField]
		public PurchasableItemNodeBase input;
	}
}
