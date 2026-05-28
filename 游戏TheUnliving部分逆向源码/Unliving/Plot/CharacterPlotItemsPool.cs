using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using UnityEngine;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002D7 RID: 727
	public class CharacterPlotItemsPool<TItem> : CharacterPlotItemBase where TItem : ICharacterPlotItem
	{
		// Token: 0x06001946 RID: 6470 RVA: 0x0004FA33 File Offset: 0x0004DC33
		private static bool IsItemAvailable(TItem item, int itemIndex, CharacterPlotItemsPool<TItem>.AvailabilityPredicate itemAvailabilityPredicate)
		{
			return itemAvailabilityPredicate == null || itemAvailabilityPredicate(item, itemIndex);
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x06001947 RID: 6471 RVA: 0x0004FA42 File Offset: 0x0004DC42
		// (set) Token: 0x06001948 RID: 6472 RVA: 0x0004FA4A File Offset: 0x0004DC4A
		public override string ID
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x06001949 RID: 6473 RVA: 0x0004FA53 File Offset: 0x0004DC53
		// (set) Token: 0x0600194A RID: 6474 RVA: 0x0004FA5B File Offset: 0x0004DC5B
		public override CharacterPlotItemTriggerBase Trigger
		{
			get
			{
				return this.trigger;
			}
			set
			{
				this.trigger = value;
			}
		}

		// Token: 0x17000558 RID: 1368
		// (get) Token: 0x0600194B RID: 6475 RVA: 0x0004FA64 File Offset: 0x0004DC64
		// (set) Token: 0x0600194C RID: 6476 RVA: 0x0004FA6C File Offset: 0x0004DC6C
		public override CharacterPlotItemTriggerBase DeactivationTrigger
		{
			get
			{
				return this.deactivationTrigger;
			}
			set
			{
				this.deactivationTrigger = value;
			}
		}

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x0600194D RID: 6477 RVA: 0x0004FA75 File Offset: 0x0004DC75
		public IReadOnlyList<TItem> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x0600194E RID: 6478 RVA: 0x0004FA7D File Offset: 0x0004DC7D
		protected void SetItems(TItem[] newItems)
		{
			this.items = newItems;
		}

		// Token: 0x0600194F RID: 6479 RVA: 0x0004FA86 File Offset: 0x0004DC86
		public CharacterPlotItemsPool(TItem[] items, CharacterPlotItemTriggerBase trigger)
		{
			this.items = items;
			this.trigger = trigger;
		}

		// Token: 0x06001950 RID: 6480 RVA: 0x0004FA9C File Offset: 0x0004DC9C
		public int GetItemIndex(TItem item)
		{
			return Array.IndexOf<TItem>(this.items, item);
		}

		// Token: 0x06001951 RID: 6481 RVA: 0x0004FAAC File Offset: 0x0004DCAC
		public bool TryGetItem(CharacterPlotItemsPool<TItem>.AvailabilityPredicate itemAvailabilityPredicate, out TItem selectedItem, int startIndex = 0, bool getRandomItem = false)
		{
			if (getRandomItem)
			{
				int num = 0;
				float num2 = 0f;
				if (this.availableItemsBuffer == null || this.availableItemsBuffer.Length != this.items.Length)
				{
					this.availableItemsBuffer = new TItem[this.items.Length];
				}
				for (int i = startIndex; i < this.items.Length; i++)
				{
					TItem titem = this.items[i];
					if (CharacterPlotItemsPool<TItem>.IsItemAvailable(titem, i, itemAvailabilityPredicate))
					{
						this.availableItemsBuffer[num++] = titem;
						num2 += titem.Weight;
					}
				}
				if (num != 0)
				{
					if (num2 <= 0f)
					{
						selectedItem = this.availableItemsBuffer[UnityEngine.Random.Range(0, num)];
						return true;
					}
					return this.availableItemsBuffer.GetRandomWeightedItem(out selectedItem, 0, num, new float?(num2));
				}
			}
			else
			{
				int num3 = -1;
				int? num4 = null;
				for (int j = startIndex; j < this.items.Length; j++)
				{
					TItem item = this.items[j];
					if (CharacterPlotItemsPool<TItem>.IsItemAvailable(item, j, itemAvailabilityPredicate))
					{
						if (num4 != null)
						{
							int priority = item.Priority;
							int? num5 = num4;
							if (!(priority > num5.GetValueOrDefault() & num5 != null))
							{
								goto IL_134;
							}
						}
						num3 = j;
						num4 = new int?(item.Priority);
					}
					IL_134:;
				}
				if (num3 >= 0)
				{
					selectedItem = this.items[num3];
					return true;
				}
			}
			selectedItem = default(TItem);
			return false;
		}

		// Token: 0x06001952 RID: 6482 RVA: 0x0004FC24 File Offset: 0x0004DE24
		public bool TryGetItem(string itemID, out TItem item)
		{
			for (int i = 0; i < this.items.Length; i++)
			{
				TItem titem = this.items[i];
				if (string.Equals(titem.ID, itemID, StringComparison.OrdinalIgnoreCase))
				{
					item = titem;
					return true;
				}
			}
			item = default(TItem);
			return false;
		}

		// Token: 0x04000E3D RID: 3645
		[SerializeField]
		private string id;

		// Token: 0x04000E3E RID: 3646
		[SerializeField]
		private TItem[] items;

		// Token: 0x04000E3F RID: 3647
		[SerializeReference]
		[CharacterPlotItemTrigger]
		private CharacterPlotItemTriggerBase trigger;

		// Token: 0x04000E40 RID: 3648
		[SerializeReference]
		[CharacterPlotItemTrigger]
		private CharacterPlotItemTriggerBase deactivationTrigger;

		// Token: 0x04000E41 RID: 3649
		private TItem[] availableItemsBuffer;

		// Token: 0x0200053A RID: 1338
		// (Invoke) Token: 0x06002692 RID: 9874
		public delegate bool AvailabilityPredicate(TItem item, int itemIndex);
	}
}
