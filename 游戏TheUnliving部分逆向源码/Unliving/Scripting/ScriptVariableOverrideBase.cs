using System;
using UnityEngine;

namespace Unliving.Scripting
{
	// Token: 0x0200007A RID: 122
	public abstract class ScriptVariableOverrideBase<T> : IScriptVariableOverride
	{
		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000383 RID: 899 RVA: 0x0000C006 File Offset: 0x0000A206
		public Type VariableType
		{
			get
			{
				return typeof(T);
			}
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000384 RID: 900 RVA: 0x0000C012 File Offset: 0x0000A212
		public string VariableName
		{
			get
			{
				return this.variableName;
			}
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000385 RID: 901 RVA: 0x0000C01A File Offset: 0x0000A21A
		// (set) Token: 0x06000386 RID: 902 RVA: 0x0000C027 File Offset: 0x0000A227
		object IScriptVariableOverride.Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = (T)((object)value);
			}
		}

		// Token: 0x04000217 RID: 535
		[SerializeField]
		private string variableName;

		// Token: 0x04000218 RID: 536
		public T value;
	}
}
