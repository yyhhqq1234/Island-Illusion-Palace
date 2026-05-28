using System;
using System.Collections.Generic;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000165 RID: 357
	public interface IPlayerUpgradesRegistry
	{
		// Token: 0x060009EE RID: 2542
		void AddUpgrade(IPlayerUpgrade playerUpgrade);

		// Token: 0x060009EF RID: 2543
		void RemoveUpgrade(IPlayerUpgrade playerUpgrade);

		// Token: 0x060009F0 RID: 2544
		IEnumerable<IPlayerUpgrade> GetUpgrades();
	}
}
