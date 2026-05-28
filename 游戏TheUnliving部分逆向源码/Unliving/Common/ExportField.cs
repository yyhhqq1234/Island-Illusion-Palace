using System;

namespace Common
{
	// Token: 0x02000007 RID: 7
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class ExportField : Attribute
	{
		// Token: 0x06000020 RID: 32 RVA: 0x00002876 File Offset: 0x00000A76
		public ExportField(string key)
		{
			this.ExportKey = key;
		}

		// Token: 0x04000013 RID: 19
		public readonly string ExportKey;
	}
}
