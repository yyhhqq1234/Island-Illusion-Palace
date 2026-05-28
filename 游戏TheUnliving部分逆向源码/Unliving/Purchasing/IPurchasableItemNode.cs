using System;

namespace Unliving.Purchasing
{
	// Token: 0x02000101 RID: 257
	public interface IPurchasableItemNode
	{
		// Token: 0x170000FA RID: 250
		// (get) Token: 0x06000634 RID: 1588
		bool HomespaceSpawnerRequired { get; }

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000635 RID: 1589
		string Name { get; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000636 RID: 1590
		IPurchasable Data { get; }

		// Token: 0x06000637 RID: 1591
		object GetObjectID();

		// Token: 0x06000638 RID: 1592
		object GetDefaultID();

		// Token: 0x06000639 RID: 1593
		void SetID(object id);

		// Token: 0x0600063A RID: 1594
		void SetDefaultID();

		// Token: 0x0600063B RID: 1595
		bool HasDefaultID();

		// Token: 0x0600063C RID: 1596
		void UpdateParents();
	}
}
