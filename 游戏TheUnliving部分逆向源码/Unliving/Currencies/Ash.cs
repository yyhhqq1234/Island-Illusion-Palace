using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x02000344 RID: 836
	[Serializable]
	public struct Ash : ICurrency, ICloneable<ICurrency>
	{
		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x06001B49 RID: 6985 RVA: 0x000564E9 File Offset: 0x000546E9
		public CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Ash;
			}
		}

		// Token: 0x1700059C RID: 1436
		// (get) Token: 0x06001B4A RID: 6986 RVA: 0x000564EC File Offset: 0x000546EC
		public bool LocalProgressCurrency
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700059D RID: 1437
		// (get) Token: 0x06001B4B RID: 6987 RVA: 0x000564EF File Offset: 0x000546EF
		// (set) Token: 0x06001B4C RID: 6988 RVA: 0x000564F7 File Offset: 0x000546F7
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

		// Token: 0x06001B4D RID: 6989 RVA: 0x00056500 File Offset: 0x00054700
		public ICurrency Clone()
		{
			return (ICurrency)base.MemberwiseClone();
		}

		// Token: 0x04000F5C RID: 3932
		[SerializeField]
		private float amount;
	}
}
