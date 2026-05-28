using System;
using Unliving.LeveledItems;

namespace Unliving.Purchasing
{
	// Token: 0x020000DE RID: 222
	public interface IPurchasableData : ILeveledItem, IItemLevelProvider
	{
		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000589 RID: 1417
		object ItemID { get; }
	}
}
