using System;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas.Common
{
	// Token: 0x020000D0 RID: 208
	[Name("Bool Value", 0)]
	[Category("Unliving/Common")]
	public sealed class BoolValueNode : PureFunctionNode<bool, bool>
	{
		// Token: 0x0600051C RID: 1308 RVA: 0x0001290A File Offset: 0x00010B0A
		public override bool Invoke(bool value)
		{
			return value;
		}
	}
}
