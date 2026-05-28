using System;
using Common;

namespace Unliving.DropSystem
{
	// Token: 0x02000296 RID: 662
	public interface IRandomDropItem : IWeighted
	{
		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x060016CF RID: 5839
		RandomDropItemType ItemType { get; }

		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x060016D0 RID: 5840
		int MinCount { get; }

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x060016D1 RID: 5841
		// (set) Token: 0x060016D2 RID: 5842
		int CurrentCount { get; set; }

		// Token: 0x060016D3 RID: 5843
		bool IsMaxCountReached();

		// Token: 0x060016D4 RID: 5844
		IDropable CreateDropable();
	}
}
