using System;
using System.Text;
using Common.Editor;
using UnityEngine;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002EA RID: 746
	[Serializable]
	public sealed class ConditionalTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019A9 RID: 6569 RVA: 0x0005056D File Offset: 0x0004E76D
		protected override bool ShouldBeIgnored()
		{
			return this.triggers == null || this.triggers.Length == 0;
		}

		// Token: 0x060019AA RID: 6570 RVA: 0x00050584 File Offset: 0x0004E784
		protected override bool GetState(CharacterPlotContext context)
		{
			bool? flag = null;
			for (int i = 0; i < this.triggers.Length; i++)
			{
				ref ConditionalTrigger.TriggerReference ptr = ref this.triggers[i];
				if (ptr.trigger != null)
				{
					bool flag2 = ptr.trigger.IsFired(context);
					if (flag == null)
					{
						flag = new bool?(ptr.@operator == ConditionalTrigger.Operator.And);
					}
					if (!flag2 && ptr.@operator == ConditionalTrigger.Operator.And)
					{
						return false;
					}
					if (ptr.@operator == ConditionalTrigger.Operator.Or && flag2)
					{
						return true;
					}
				}
			}
			return flag.Value;
		}

		// Token: 0x060019AB RID: 6571 RVA: 0x00050610 File Offset: 0x0004E810
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			stringBuilder.Clear();
			if (this.triggers != null && this.triggers.Length != 0)
			{
				for (int i = 0; i < this.triggers.Length; i++)
				{
					ref ConditionalTrigger.TriggerReference ptr = ref this.triggers[i];
					if (ptr.trigger != null)
					{
						string value = string.Format(" {0} {1}", (ptr.@operator == ConditionalTrigger.Operator.And) ? '&' : '|', ((IFormattable)ptr.trigger).ToString(null, null));
						if (i == 0)
						{
							stringBuilder.Append(value);
						}
						else
						{
							stringBuilder.AppendLine(value);
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x04000E5A RID: 3674
		public ConditionalTrigger.TriggerReference[] triggers;

		// Token: 0x0200053D RID: 1341
		public enum Operator
		{
			// Token: 0x04001B86 RID: 7046
			And,
			// Token: 0x04001B87 RID: 7047
			Or
		}

		// Token: 0x0200053E RID: 1342
		[Serializable]
		public struct TriggerReference
		{
			// Token: 0x04001B88 RID: 7048
			public ConditionalTrigger.Operator @operator;

			// Token: 0x04001B89 RID: 7049
			[SerializeReference]
			[ManagedObjectField(typeof(CharacterPlotItemTriggerBase))]
			public CharacterPlotItemTriggerBase trigger;
		}
	}
}
