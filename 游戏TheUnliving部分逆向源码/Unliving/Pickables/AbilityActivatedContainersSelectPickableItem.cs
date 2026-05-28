using System;
using Unliving.Currencies;

namespace Unliving.Pickables
{
	// Token: 0x0200017D RID: 381
	public class AbilityActivatedContainersSelectPickableItem : SelectPickableItem
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000A96 RID: 2710 RVA: 0x00022E60 File Offset: 0x00021060
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.HealthContainer;
			}
		}

		// Token: 0x06000A97 RID: 2711 RVA: 0x00022E63 File Offset: 0x00021063
		public override CurrencyOperationArgs GetCloseRewardArgs()
		{
			return this.closeReward;
		}

		// Token: 0x04000629 RID: 1577
		public bool hideCloseButton;

		// Token: 0x0400062A RID: 1578
		public CurrencyOperationArgs closeReward;
	}
}
