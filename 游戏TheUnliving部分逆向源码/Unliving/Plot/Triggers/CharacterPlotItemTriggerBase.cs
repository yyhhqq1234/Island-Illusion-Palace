using System;
using System.Text;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002E9 RID: 745
	[Serializable]
	public abstract class CharacterPlotItemTriggerBase : IFormattable
	{
		// Token: 0x060019A0 RID: 6560 RVA: 0x000504A0 File Offset: 0x0004E6A0
		protected static string ObjArgToString(object arg, string argString = null)
		{
			if (arg.IsNull())
			{
				return "none";
			}
			if (argString != null)
			{
				return argString;
			}
			UnityEngine.Object @object = arg as UnityEngine.Object;
			if (@object != null)
			{
				return @object.name;
			}
			return arg.ToString();
		}

		// Token: 0x060019A1 RID: 6561 RVA: 0x000504D7 File Offset: 0x0004E6D7
		public virtual float GetProgress(CharacterPlotContext context, out float currentValue, out float targetValue)
		{
			targetValue = 1f;
			currentValue = (this.GetState(context) ? 1f : 0f);
			return currentValue;
		}

		// Token: 0x060019A2 RID: 6562
		protected abstract bool ShouldBeIgnored();

		// Token: 0x060019A3 RID: 6563
		protected abstract bool GetState(CharacterPlotContext context);

		// Token: 0x060019A4 RID: 6564 RVA: 0x000504F9 File Offset: 0x0004E6F9
		protected virtual string ToFormattedString(StringBuilder stringBuilder)
		{
			return this.ToString();
		}

		// Token: 0x060019A5 RID: 6565 RVA: 0x00050501 File Offset: 0x0004E701
		public bool IsFired(CharacterPlotContext context)
		{
			if (this.ShouldBeIgnored())
			{
				return true;
			}
			if (!this.isInverted)
			{
				return this.GetState(context);
			}
			return !this.GetState(context);
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x00050527 File Offset: 0x0004E727
		string IFormattable.ToString(string format, IFormatProvider formatProvider)
		{
			if (!this.isInverted)
			{
				return this.ToFormattedString(CharacterPlotItemTriggerBase.StringBuilder);
			}
			return "!(" + this.ToFormattedString(CharacterPlotItemTriggerBase.StringBuilder) + ")";
		}

		// Token: 0x04000E58 RID: 3672
		private static readonly StringBuilder StringBuilder = new StringBuilder(64);

		// Token: 0x04000E59 RID: 3673
		public bool isInverted;
	}
}
