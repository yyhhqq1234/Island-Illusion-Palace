using System;
using Common.Editor;
using UnityEngine;

namespace Unliving.Scripting
{
	// Token: 0x0200007F RID: 127
	[Serializable]
	public sealed class ScriptVariablesOverrides
	{
		// Token: 0x170000BC RID: 188
		// (get) Token: 0x0600038C RID: 908 RVA: 0x0000C05D File Offset: 0x0000A25D
		// (set) Token: 0x0600038D RID: 909 RVA: 0x0000C065 File Offset: 0x0000A265
		public IScriptVariableOverride[] VariableOverrides
		{
			get
			{
				return this.variableOverrides;
			}
			set
			{
				this.variableOverrides = value;
			}
		}

		// Token: 0x0600038E RID: 910 RVA: 0x0000C070 File Offset: 0x0000A270
		public void ApplyOverrides(IScript script)
		{
			for (int i = 0; i < this.variableOverrides.Length; i++)
			{
				IScriptVariableOverride scriptVariableOverride = this.variableOverrides[i];
				script.TrySetVariableValue(scriptVariableOverride.VariableName, scriptVariableOverride.VariableType, scriptVariableOverride.Value);
			}
		}

		// Token: 0x04000219 RID: 537
		[SerializeReference]
		[ManagedObjectField(typeof(IScriptVariableOverride))]
		private IScriptVariableOverride[] variableOverrides;
	}
}
