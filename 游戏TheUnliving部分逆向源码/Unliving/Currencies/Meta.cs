using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x0200034C RID: 844
	[Serializable]
	public struct Meta : ICurrency, ICloneable<ICurrency>
	{
		// Token: 0x170005AF RID: 1455
		// (get) Token: 0x06001B68 RID: 7016 RVA: 0x000565B3 File Offset: 0x000547B3
		public CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Meta;
			}
		}

		// Token: 0x170005B0 RID: 1456
		// (get) Token: 0x06001B69 RID: 7017 RVA: 0x000565B6 File Offset: 0x000547B6
		public bool LocalProgressCurrency
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005B1 RID: 1457
		// (get) Token: 0x06001B6A RID: 7018 RVA: 0x000565B9 File Offset: 0x000547B9
		// (set) Token: 0x06001B6B RID: 7019 RVA: 0x000565C1 File Offset: 0x000547C1
		public float Amount
		{
			get
			{
				return this.amount;
			}
			set
			{
				this.amount = value;
			}
		}

		// Token: 0x06001B6C RID: 7020 RVA: 0x000565CA File Offset: 0x000547CA
		public ICurrency Clone()
		{
			return (ICurrency)base.MemberwiseClone();
		}

		// Token: 0x04000F6B RID: 3947
		[SerializeField]
		private float amount;
	}
}
