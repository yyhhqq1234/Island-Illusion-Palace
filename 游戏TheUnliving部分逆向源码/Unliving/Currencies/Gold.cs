using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x02000348 RID: 840
	[Serializable]
	public struct Gold : ICurrency, ICloneable<ICurrency>
	{
		// Token: 0x170005A5 RID: 1445
		// (get) Token: 0x06001B59 RID: 7001 RVA: 0x00056585 File Offset: 0x00054785
		public CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Gold;
			}
		}

		// Token: 0x170005A6 RID: 1446
		// (get) Token: 0x06001B5A RID: 7002 RVA: 0x00056588 File Offset: 0x00054788
		public bool LocalProgressCurrency
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170005A7 RID: 1447
		// (get) Token: 0x06001B5B RID: 7003 RVA: 0x0005658B File Offset: 0x0005478B
		// (set) Token: 0x06001B5C RID: 7004 RVA: 0x00056593 File Offset: 0x00054793
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

		// Token: 0x06001B5D RID: 7005 RVA: 0x0005659C File Offset: 0x0005479C
		public ICurrency Clone()
		{
			return (ICurrency)base.MemberwiseClone();
		}

		// Token: 0x04000F6A RID: 3946
		[SerializeField]
		private float amount;
	}
}
