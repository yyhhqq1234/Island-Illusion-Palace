using System;
using GraphProcessor;
using Unliving.Essence;

namespace Unliving.Purchasing
{
	// Token: 0x02000102 RID: 258
	[NodeMenuItem("Purchase Manager/Ash Smithy Node", null)]
	[Serializable]
	public class AshSmithyNode : PurchasableItemNodeBase<PurchasableAshSmithy, EssenceType>
	{
		// Token: 0x170000FD RID: 253
		// (get) Token: 0x0600063D RID: 1597 RVA: 0x00015033 File Offset: 0x00013233
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x00015036 File Offset: 0x00013236
		public override object GetDefaultID()
		{
			return EssenceType.AshSmithy;
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0001503E File Offset: 0x0001323E
		public override object GetObjectID()
		{
			return EssenceType.AshSmithy;
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x00015046 File Offset: 0x00013246
		public override bool HasDefaultID()
		{
			return false;
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x00015049 File Offset: 0x00013249
		public override void SetDefaultID()
		{
			this.data = new PurchasableAshSmithy();
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x00015056 File Offset: 0x00013256
		public override void SetID(object id)
		{
			this.data = new PurchasableAshSmithy();
		}

		// Token: 0x040003FB RID: 1019
		private const EssenceType AshSmithyEssenceType = EssenceType.AshSmithy;
	}
}
