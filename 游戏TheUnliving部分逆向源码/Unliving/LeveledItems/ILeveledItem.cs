using System;

namespace Unliving.LeveledItems
{
	// Token: 0x02000259 RID: 601
	public interface ILeveledItem : IItemLevelProvider
	{
		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06001400 RID: 5120
		// (set) Token: 0x06001401 RID: 5121
		int ItemLevel { get; set; }
	}
}
