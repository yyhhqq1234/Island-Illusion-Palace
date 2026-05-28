using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x02000345 RID: 837
	[Serializable]
	public struct Cinder : ICurrency, ICloneable<ICurrency>
	{
		// Token: 0x1700059E RID: 1438
		// (get) Token: 0x06001B4E RID: 6990 RVA: 0x00056517 File Offset: 0x00054717
		public CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Cinder;
			}
		}

		// Token: 0x1700059F RID: 1439
		// (get) Token: 0x06001B4F RID: 6991 RVA: 0x0005651A File Offset: 0x0005471A
		public bool LocalProgressCurrency
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x06001B50 RID: 6992 RVA: 0x0005651D File Offset: 0x0005471D
		// (set) Token: 0x06001B51 RID: 6993 RVA: 0x00056525 File Offset: 0x00054725
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

		// Token: 0x06001B52 RID: 6994 RVA: 0x0005652E File Offset: 0x0005472E
		public ICurrency Clone()
		{
			return (ICurrency)base.MemberwiseClone();
		}

		// Token: 0x04000F5D RID: 3933
		[SerializeField]
		private float amount;
	}
}
