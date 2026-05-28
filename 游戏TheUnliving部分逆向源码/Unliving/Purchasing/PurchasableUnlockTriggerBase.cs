using System;
using System.Text;

namespace Unliving.Purchasing
{
	// Token: 0x02000113 RID: 275
	[Serializable]
	public abstract class PurchasableUnlockTriggerBase : IPurchasableUnlockTrigger, IFormattable
	{
		// Token: 0x060006A3 RID: 1699
		public abstract bool IsFired(Context context);

		// Token: 0x060006A4 RID: 1700 RVA: 0x00015CC6 File Offset: 0x00013EC6
		protected virtual string ToFormattedString(StringBuilder stringBuilder)
		{
			return this.ToString();
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x00015CCE File Offset: 0x00013ECE
		string IFormattable.ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToFormattedString(PurchasableUnlockTriggerBase.StringBuilder);
		}

		// Token: 0x04000411 RID: 1041
		private static readonly StringBuilder StringBuilder = new StringBuilder(64);
	}
}
