using System;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Currencies;
using Unliving.Essence;

namespace Unliving.Purchasing
{
	// Token: 0x020000E8 RID: 232
	[Serializable]
	public class PurchasableAshSmithy : PurchasableEssence
	{
		// Token: 0x060005A1 RID: 1441 RVA: 0x00013D59 File Offset: 0x00011F59
		public PurchasableAshSmithy() : this(EssenceType.AshSmithy)
		{
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x00013D62 File Offset: 0x00011F62
		public PurchasableAshSmithy(EssenceType essenceType) : base(essenceType)
		{
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x00013D6C File Offset: 0x00011F6C
		public void UpdateEnchantPrice()
		{
			base.BuyArgs = new CurrencyOperationArgs
			{
				currencyID = this.currencyID,
				amount = (float)this.enchantPrice,
				spending = true
			};
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x00013DAC File Offset: 0x00011FAC
		public void UpdateUpgradePrice(int level)
		{
			level = Mathf.Clamp(level - 1, 0, this.upgradePrices.Length - 1);
			base.BuyArgs = new CurrencyOperationArgs
			{
				currencyID = this.currencyID,
				amount = (float)this.upgradePrices[level],
				spending = true
			};
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x00013E04 File Offset: 0x00012004
		public void UpdateReforgePrice(int level)
		{
			level = Mathf.Clamp(level - 1, 0, this.reforgePrices.Length - 1);
			base.BuyArgs = new CurrencyOperationArgs
			{
				currencyID = this.currencyID,
				amount = (float)this.reforgePrices[level],
				spending = true
			};
		}

		// Token: 0x040003C6 RID: 966
		public CurrencyID currencyID;

		// Token: 0x040003C7 RID: 967
		public int[] upgradePrices;

		// Token: 0x040003C8 RID: 968
		[FormerlySerializedAs("upgradeAndReplacePrices")]
		public int[] reforgePrices;

		// Token: 0x040003C9 RID: 969
		[FormerlySerializedAs("replaceSpecialBehaviourPrice")]
		public int enchantPrice;
	}
}
