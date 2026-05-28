using System;
using System.Collections.Generic;
using System.Linq;
using Common.CollectionsExtensions;
using UnityEngine;
using Unliving.LevelGeneration;

namespace Unliving.DropSystem
{
	// Token: 0x02000294 RID: 660
	[Serializable]
	public class LocationDropData
	{
		// Token: 0x060016C8 RID: 5832 RVA: 0x00048A98 File Offset: 0x00046C98
		internal void Initialize(bool debugLogEnabled = false)
		{
			this.generatedItemsPool.Clear();
			List<IRandomDropItem> list = new List<IRandomDropItem>();
			List<IRandomDropItem> list2 = (from i in this.dropItems
			select i.Clone()).ToList<IRandomDropItem>();
			for (int m = 0; m < list2.Count; m++)
			{
				IRandomDropItem randomDropItem = list2[m];
				for (int j = 0; j < randomDropItem.MinCount; j++)
				{
					list.Add(randomDropItem);
				}
			}
			List<IRandomDropItem> list3 = list.ToList<IRandomDropItem>();
			RandomDropItemType lastItemType = RandomDropItemType.None;
			Func<IRandomDropItem, bool> <>9__2;
			for (int k = 0; k < list.Count; k++)
			{
				List<IRandomDropItem> list4;
				if (lastItemType == RandomDropItemType.None)
				{
					list4 = list3;
				}
				else
				{
					IEnumerable<IRandomDropItem> source = list3;
					Func<IRandomDropItem, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((IRandomDropItem i) => i.ItemType != lastItemType));
					}
					list4 = source.Where(predicate).ToList<IRandomDropItem>();
				}
				if (list4.Count == 0)
				{
					list4 = list3;
				}
				IRandomDropItem randomDropItem2;
				if (list4.GetRandomWeightedItem(out randomDropItem2, 0, 2147483647, null))
				{
					lastItemType = randomDropItem2.ItemType;
					this.generatedItemsPool.Add(randomDropItem2);
					list3.Remove(randomDropItem2);
					IRandomDropItem randomDropItem3 = randomDropItem2;
					int currentCount = randomDropItem3.CurrentCount;
					randomDropItem3.CurrentCount = currentCount + 1;
					if (debugLogEnabled)
					{
						string str = "Min required: ";
						IRandomDropItem randomDropItem4 = randomDropItem2;
						Debug.Log(str + ((randomDropItem4 != null) ? randomDropItem4.ToString() : null));
					}
				}
			}
			list2 = (from i in list2
			where !i.IsMaxCountReached()
			select i).ToList<IRandomDropItem>();
			int num = this.totalDropItemsCount - this.generatedItemsPool.Count;
			for (int l = 0; l < num; l++)
			{
				IRandomDropItem randomDropItem5;
				if (list2.GetRandomWeightedItem(out randomDropItem5, 0, 2147483647, null))
				{
					this.generatedItemsPool.Add(randomDropItem5);
					IRandomDropItem randomDropItem6 = randomDropItem5;
					int currentCount = randomDropItem6.CurrentCount;
					randomDropItem6.CurrentCount = currentCount + 1;
					if (randomDropItem5.IsMaxCountReached())
					{
						list2.Remove(randomDropItem5);
					}
					if (debugLogEnabled)
					{
						string str2 = "Random generated: ";
						IRandomDropItem randomDropItem7 = randomDropItem5;
						Debug.Log(str2 + ((randomDropItem7 != null) ? randomDropItem7.ToString() : null));
					}
				}
				else
				{
					this.generatedItemsPool.Add(this.fallbackDropItem);
					if (debugLogEnabled)
					{
						string str3 = "Fallback generated: ";
						RandomDropItem randomDropItem8 = this.fallbackDropItem;
						Debug.Log(str3 + ((randomDropItem8 != null) ? randomDropItem8.ToString() : null));
					}
				}
			}
		}

		// Token: 0x060016C9 RID: 5833 RVA: 0x00048D08 File Offset: 0x00046F08
		internal bool TryGetNextDropable(RandomDropItemType allowedTypes, out IDropable dropable)
		{
			dropable = null;
			if (this.generatedItemsPool.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < this.generatedItemsPool.Count; i++)
			{
				IRandomDropItem randomDropItem = this.generatedItemsPool[i];
				if ((randomDropItem.ItemType & allowedTypes) != RandomDropItemType.None && this.TryCreateDropable(randomDropItem, out dropable))
				{
					this.generatedItemsPool.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060016CA RID: 5834 RVA: 0x00048D6D File Offset: 0x00046F6D
		private bool TryCreateDropable(IRandomDropItem dropItem, out IDropable dropable)
		{
			dropable = dropItem.CreateDropable();
			return dropable != null;
		}

		// Token: 0x060016CB RID: 5835 RVA: 0x00048D7C File Offset: 0x00046F7C
		public void NormalizeWeights()
		{
			if (this.dropItems.Length == 0)
			{
				return;
			}
			if (this.dropItems.Length == 1)
			{
				this.dropItems[0].weight = 1f;
				this.dropItems[0].Clear();
				return;
			}
			float num = 1f;
			bool flag = false;
			for (int m = 0; m < this.dropItems.Length; m++)
			{
				RandomDropItem randomDropItem = this.dropItems[m];
				if (randomDropItem.IsNewAddedItem())
				{
					randomDropItem.weight = Mathf.Min(randomDropItem.weight, num);
					randomDropItem.Clear();
					flag = true;
				}
				num -= randomDropItem.weight;
			}
			if (flag)
			{
				return;
			}
			float num2 = this.dropItems.GetTotalWeight(0, int.MaxValue) - 1f;
			if (Mathf.Abs(num2) < 1E-05f)
			{
				return;
			}
			int num3 = this.dropItems.TakeWhile((RandomDropItem i) => !i.IsDirty()).Count<RandomDropItem>();
			if (num3 < 0)
			{
				return;
			}
			bool flag2 = false;
			float num4 = 0f;
			for (int j = 0; j < this.dropItems.Length; j++)
			{
				RandomDropItem randomDropItem2 = this.dropItems[j];
				if (j != num3 && !randomDropItem2.freezeWeight)
				{
					flag2 = true;
					num4 += randomDropItem2.weight;
				}
			}
			float num5 = num4 - num2;
			if (num5 < 0f)
			{
				this.dropItems[num3].weight += num5;
				num2 = this.dropItems.GetTotalWeight(0, int.MaxValue) - 1f;
			}
			else if (num5 > 1f)
			{
				this.dropItems[num3].weight -= 1f - num5;
				num2 = this.dropItems.GetTotalWeight(0, int.MaxValue) - 1f;
			}
			if (Mathf.Abs(num2) < 1E-05f)
			{
				return;
			}
			if (!flag2)
			{
				this.dropItems[num3].RestorePrevValue();
				return;
			}
			int num6 = 0;
			for (int k = 0; k < this.dropItems.Length; k++)
			{
				RandomDropItem randomDropItem3 = this.dropItems[k];
				if (k != num3 && !randomDropItem3.freezeWeight)
				{
					num6++;
					if (num6 > 10000)
					{
						break;
					}
					float num7 = Mathf.Sign(num2) * Mathf.Min(0.001f, Mathf.Abs(num2));
					if (randomDropItem3.weight - num7 < 0f)
					{
						num7 = randomDropItem3.weight;
					}
					else if (randomDropItem3.weight - num7 > 1f)
					{
						num7 = randomDropItem3.weight - 1f;
					}
					randomDropItem3.weight -= num7;
					num2 -= num7;
				}
				if (k >= this.dropItems.Length - 1 && Mathf.Abs(num2) > 1E-05f)
				{
					k = -1;
				}
			}
			for (int l = 0; l < this.dropItems.Length; l++)
			{
				this.dropItems[l].Clear();
			}
		}

		// Token: 0x04000D28 RID: 3368
		public GameLocation.TypeID locationType;

		// Token: 0x04000D29 RID: 3369
		public RandomDropItem fallbackDropItem;

		// Token: 0x04000D2A RID: 3370
		public int totalDropItemsCount;

		// Token: 0x04000D2B RID: 3371
		public RandomDropItem[] dropItems;

		// Token: 0x04000D2C RID: 3372
		public readonly List<IRandomDropItem> generatedItemsPool = new List<IRandomDropItem>();
	}
}
