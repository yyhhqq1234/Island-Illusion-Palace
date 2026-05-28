using System;
using Common;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas.Common
{
	// Token: 0x020000D2 RID: 210
	[Name("Set Amount", 0)]
	[Category("Unliving/Common")]
	public sealed class SetAmountNode : CallableActionNode<IAmountBased, float>
	{
		// Token: 0x06000520 RID: 1312 RVA: 0x00012925 File Offset: 0x00010B25
		public override void Invoke(IAmountBased amountSource, float newAmount)
		{
			amountSource.Amount = newAmount;
		}
	}
}
