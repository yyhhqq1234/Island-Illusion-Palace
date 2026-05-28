using System;

namespace Unliving.Player.TemporaryItemsStorage
{
	// Token: 0x02000175 RID: 373
	public abstract class TemporaryItemBase
	{
		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000A58 RID: 2648
		public abstract string ID { get; }

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000A59 RID: 2649 RVA: 0x00022569 File Offset: 0x00020769
		public object Content { get; }

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000A5A RID: 2650
		public abstract float ExpirationProgress { get; }

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000A5B RID: 2651
		public abstract object CurrentProgressValue { get; }

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000A5C RID: 2652
		public abstract object TargetProgressValue { get; }

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000A5D RID: 2653
		public abstract bool IsExpired { get; }

		// Token: 0x06000A5E RID: 2654 RVA: 0x00022571 File Offset: 0x00020771
		public TemporaryItemBase(object content)
		{
			this.Content = content;
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x00022580 File Offset: 0x00020780
		public virtual bool CanBeStored(TemporaryItemsStorageController storage)
		{
			return true;
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x00022583 File Offset: 0x00020783
		public virtual void OnStored(TemporaryItemsStorageController storage)
		{
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x00022585 File Offset: 0x00020785
		public virtual void OnDiscarded(TemporaryItemsStorageController storage)
		{
		}
	}
}
