using System;
using Common;
using UnityEngine;

namespace Unliving.DropSystem
{
	// Token: 0x02000297 RID: 663
	[Serializable]
	public class RandomDropItem : IRandomDropItem, IWeighted, ICloneable<RandomDropItem>
	{
		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x060016D5 RID: 5845 RVA: 0x000490CF File Offset: 0x000472CF
		// (set) Token: 0x060016D6 RID: 5846 RVA: 0x000490D7 File Offset: 0x000472D7
		public float Weight
		{
			get
			{
				return this.weight;
			}
			set
			{
				this.weight = value;
			}
		}

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x060016D7 RID: 5847 RVA: 0x000490E0 File Offset: 0x000472E0
		// (set) Token: 0x060016D8 RID: 5848 RVA: 0x000490E8 File Offset: 0x000472E8
		public int CurrentCount
		{
			get
			{
				return this.currentCount;
			}
			set
			{
				this.currentCount = value;
			}
		}

		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x060016D9 RID: 5849 RVA: 0x000490F1 File Offset: 0x000472F1
		public int MinCount
		{
			get
			{
				return this.minCount;
			}
		}

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x060016DA RID: 5850 RVA: 0x000490F9 File Offset: 0x000472F9
		public RandomDropItemType ItemType
		{
			get
			{
				return this.itemType;
			}
		}

		// Token: 0x060016DB RID: 5851 RVA: 0x00049101 File Offset: 0x00047301
		public bool IsNewAddedItem()
		{
			return this.oldWeight == null;
		}

		// Token: 0x060016DC RID: 5852 RVA: 0x00049114 File Offset: 0x00047314
		public bool IsDirty()
		{
			if (this.oldWeight != null)
			{
				float? num = this.weight - this.oldWeight;
				float num2 = 0f;
				return !(num.GetValueOrDefault() == num2 & num != null);
			}
			return true;
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x0004917D File Offset: 0x0004737D
		public void Clear()
		{
			if (this.weight < 1E-05f)
			{
				this.weight = 0f;
			}
			this.oldWeight = new float?(this.weight);
		}

		// Token: 0x060016DE RID: 5854 RVA: 0x000491A8 File Offset: 0x000473A8
		public void RestorePrevValue()
		{
			if (this.oldWeight != null)
			{
				this.weight = this.oldWeight.Value;
			}
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x000491C8 File Offset: 0x000473C8
		public IDropable CreateDropable()
		{
			return new AnotherItemDropable
			{
				prefab = this.prefab
			};
		}

		// Token: 0x060016E0 RID: 5856 RVA: 0x000491DB File Offset: 0x000473DB
		public bool IsMaxCountReached()
		{
			return this.currentCount >= this.maxCount;
		}

		// Token: 0x060016E1 RID: 5857 RVA: 0x000491F0 File Offset: 0x000473F0
		public override string ToString()
		{
			return string.Format("{0}, weight: {1}, min: {2}, max: {3}, current: {4}", new object[]
			{
				this.ItemType,
				this.weight,
				this.minCount,
				this.maxCount,
				this.currentCount
			});
		}

		// Token: 0x060016E2 RID: 5858 RVA: 0x00049253 File Offset: 0x00047453
		public RandomDropItem Clone()
		{
			return (RandomDropItem)base.MemberwiseClone();
		}

		// Token: 0x04000D2F RID: 3375
		public RandomDropItemType itemType;

		// Token: 0x04000D30 RID: 3376
		public int minCount;

		// Token: 0x04000D31 RID: 3377
		public int maxCount;

		// Token: 0x04000D32 RID: 3378
		[Range(0f, 1f)]
		public float weight;

		// Token: 0x04000D33 RID: 3379
		public bool freezeWeight;

		// Token: 0x04000D34 RID: 3380
		public GameObject prefab;

		// Token: 0x04000D35 RID: 3381
		private float? oldWeight;

		// Token: 0x04000D36 RID: 3382
		private int currentCount;
	}
}
