using System;
using System.Collections;
using Unliving.LeveledItems;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000163 RID: 355
	public interface IPlayerUpgrade : ILeveledItem, IItemLevelProvider
	{
		// Token: 0x17000190 RID: 400
		// (get) Token: 0x060009E6 RID: 2534
		IPlayerUpgrade UpgradePrototype { get; }

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x060009E7 RID: 2535
		bool IsActivated { get; }

		// Token: 0x060009E8 RID: 2536
		IPlayerUpgrade Clone();

		// Token: 0x060009E9 RID: 2537
		IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext);
	}
}
