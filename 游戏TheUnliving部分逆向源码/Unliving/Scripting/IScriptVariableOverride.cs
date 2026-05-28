using System;

namespace Unliving.Scripting
{
	// Token: 0x02000079 RID: 121
	public interface IScriptVariableOverride
	{
		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600037F RID: 895
		Type VariableType { get; }

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000380 RID: 896
		string VariableName { get; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000381 RID: 897
		// (set) Token: 0x06000382 RID: 898
		object Value { get; set; }
	}
}
