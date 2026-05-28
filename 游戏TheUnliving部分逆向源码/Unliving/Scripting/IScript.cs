using System;
using System.Collections.Generic;

namespace Unliving.Scripting
{
	// Token: 0x02000078 RID: 120
	public interface IScript
	{
		// Token: 0x0600037A RID: 890
		bool TryGetVariableValue(string variableName, Type variableType, out object value);

		// Token: 0x0600037B RID: 891
		bool TrySetVariableValue(string variableName, Type variableType, object value);

		// Token: 0x0600037C RID: 892
		bool TryGetVariableValue<T>(string variableName, out T value);

		// Token: 0x0600037D RID: 893
		bool TrySetVariableValue<T>(string variableName, T value);

		// Token: 0x0600037E RID: 894
		IEnumerable<ValueTuple<string, object>> GetVariables();
	}
}
