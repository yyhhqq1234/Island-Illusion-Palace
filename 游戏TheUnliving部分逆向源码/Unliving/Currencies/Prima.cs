using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x0200034D RID: 845
	[Serializable]
	public struct Prima : ICurrency, ICloneable<ICurrency>
	{
		// Token: 0x170005B2 RID: 1458
		// (get) Token: 0x06001B6D RID: 7021 RVA: 0x000565E1 File Offset: 0x000547E1
		public CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Prima;
			}
		}

		// Token: 0x170005B3 RID: 1459
		// (get) Token: 0x06001B6E RID: 7022 RVA: 0x000565E4 File Offset: 0x000547E4
		public bool LocalProgressCurrency
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005B4 RID: 1460
		// (get) Token: 0x06001B6F RID: 7023 RVA: 0x000565E7 File Offset: 0x000547E7
		// (set) Token: 0x06001B70 RID: 7024 RVA: 0x000565EF File Offset: 0x000547EF
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

		// Token: 0x06001B71 RID: 7025 RVA: 0x000565F8 File Offset: 0x000547F8
		public ICurrency Clone()
		{
			return (ICurrency)base.MemberwiseClone();
		}

		// Token: 0x04000F6C RID: 3948
		[SerializeField]
		private float amount;
	}
}
