using System;
using UnityEngine;
using Unliving.LeveledItems;

namespace Unliving.Purchasing
{
	// Token: 0x020000E5 RID: 229
	public class PurchasableData<T> : IPurchasableData, ILeveledItem, IItemLevelProvider where T : Enum
	{
		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000599 RID: 1433 RVA: 0x00013CF4 File Offset: 0x00011EF4
		public object ItemID
		{
			get
			{
				return this.itemID;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x0600059A RID: 1434 RVA: 0x00013D01 File Offset: 0x00011F01
		// (set) Token: 0x0600059B RID: 1435 RVA: 0x00013D09 File Offset: 0x00011F09
		public int ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
			set
			{
				this.itemLevel = value;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x0600059C RID: 1436 RVA: 0x00013D12 File Offset: 0x00011F12
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x00013D1A File Offset: 0x00011F1A
		public PurchasableData(T itemID, int itemLevel)
		{
			this.itemID = itemID;
			this.itemLevel = itemLevel;
		}

		// Token: 0x040003C4 RID: 964
		[SerializeField]
		private T itemID;

		// Token: 0x040003C5 RID: 965
		[SerializeField]
		private int itemLevel;
	}
}
