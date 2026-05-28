using System;
using Common;
using UnityEngine;

namespace Unliving.Currencies
{
	// Token: 0x02000347 RID: 839
	[Serializable]
	public struct CurrencyOperationArgs : ICurrencyOperationArgs, ICloneable<ICurrencyOperationArgs>
	{
		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x06001B53 RID: 6995 RVA: 0x00056545 File Offset: 0x00054745
		public CurrencyID CurrencyID
		{
			get
			{
				return this.currencyID;
			}
		}

		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x06001B54 RID: 6996 RVA: 0x0005654D File Offset: 0x0005474D
		// (set) Token: 0x06001B55 RID: 6997 RVA: 0x00056555 File Offset: 0x00054755
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

		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x06001B56 RID: 6998 RVA: 0x0005655E File Offset: 0x0005475E
		public bool Spending
		{
			get
			{
				return this.spending;
			}
		}

		// Token: 0x170005A4 RID: 1444
		// (get) Token: 0x06001B57 RID: 6999 RVA: 0x00056566 File Offset: 0x00054766
		public object Sender
		{
			get
			{
				return this.sender;
			}
		}

		// Token: 0x06001B58 RID: 7000 RVA: 0x0005656E File Offset: 0x0005476E
		public ICurrencyOperationArgs Clone()
		{
			return (ICurrencyOperationArgs)base.MemberwiseClone();
		}

		// Token: 0x04000F66 RID: 3942
		public CurrencyID currencyID;

		// Token: 0x04000F67 RID: 3943
		public float amount;

		// Token: 0x04000F68 RID: 3944
		public bool spending;

		// Token: 0x04000F69 RID: 3945
		[HideInInspector]
		[NonSerialized]
		public object sender;
	}
}
