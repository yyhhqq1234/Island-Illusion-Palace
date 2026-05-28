using System;
using FlowCanvas.Nodes;
using Game.Buffs;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200008C RID: 140
	[Name("Cancel Buff", 0)]
	[Category("Unliving/Buffs")]
	public sealed class CancelBuffNode : CallableActionNode<IBuff>
	{
		// Token: 0x060003D4 RID: 980 RVA: 0x0000D45C File Offset: 0x0000B65C
		public override void Invoke(IBuff buffInstance)
		{
			if (buffInstance != null)
			{
				buffInstance.Complete();
			}
		}
	}
}
