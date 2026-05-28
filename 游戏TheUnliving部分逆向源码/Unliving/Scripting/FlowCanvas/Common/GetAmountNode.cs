using System;
using Common;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas.Common
{
	// Token: 0x020000D1 RID: 209
	[Name("Get Amount", 0)]
	[Category("Unliving/Common")]
	public sealed class GetAmountNode : PureFunctionNode<float, IAmountBased>
	{
		// Token: 0x0600051E RID: 1310 RVA: 0x00012915 File Offset: 0x00010B15
		public override float Invoke(IAmountBased amountSource)
		{
			return amountSource.Amount;
		}
	}
}
