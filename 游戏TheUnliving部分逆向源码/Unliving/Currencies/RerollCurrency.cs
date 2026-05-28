using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x0200034E RID: 846
	[Serializable]
	public struct RerollCurrency : ICurrency, ICloneable<ICurrency>
	{
		// Token: 0x170005B5 RID: 1461
		// (get) Token: 0x06001B72 RID: 7026 RVA: 0x0005660F File Offset: 0x0005480F
		public CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.RerollCurrency;
			}
		}

		// Token: 0x170005B6 RID: 1462
		// (get) Token: 0x06001B73 RID: 7027 RVA: 0x00056612 File Offset: 0x00054812
		public bool LocalProgressCurrency
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170005B7 RID: 1463
		// (get) Token: 0x06001B74 RID: 7028 RVA: 0x00056615 File Offset: 0x00054815
		// (set) Token: 0x06001B75 RID: 7029 RVA: 0x0005661D File Offset: 0x0005481D
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

		// Token: 0x06001B76 RID: 7030 RVA: 0x00056626 File Offset: 0x00054826
		public ICurrency Clone()
		{
			return (ICurrency)base.MemberwiseClone();
		}

		// Token: 0x04000F6D RID: 3949
		[SerializeField]
		private float amount;
	}
}
