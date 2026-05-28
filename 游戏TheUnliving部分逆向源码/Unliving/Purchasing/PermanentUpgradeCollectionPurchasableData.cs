using System;
using Unliving.LeveledItems;

namespace Unliving.Purchasing
{
	// Token: 0x020000F1 RID: 241
	[Serializable]
	public class PermanentUpgradeCollectionPurchasableData : IPurchasableData, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060005CA RID: 1482 RVA: 0x0001435C File Offset: 0x0001255C
		public object ItemID
		{
			get
			{
				return this.itemID;
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x060005CB RID: 1483 RVA: 0x00014364 File Offset: 0x00012564
		// (set) Token: 0x060005CC RID: 1484 RVA: 0x00014367 File Offset: 0x00012567
		public int ItemLevel
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060005CD RID: 1485 RVA: 0x00014369 File Offset: 0x00012569
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x0001436C File Offset: 0x0001256C
		public PermanentUpgradeCollectionPurchasableData(string itemID, int selectedItemIndex)
		{
			this.itemID = itemID;
			this.selectedItemIndex = selectedItemIndex;
		}

		// Token: 0x040003DD RID: 989
		public string itemID;

		// Token: 0x040003DE RID: 990
		public int selectedItemIndex;
	}
}
