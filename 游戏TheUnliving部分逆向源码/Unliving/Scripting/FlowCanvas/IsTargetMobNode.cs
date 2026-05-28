using System;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A6 RID: 166
	[Name("Is Target Mob", 0)]
	[Category("Unliving/Mobs")]
	public sealed class IsTargetMobNode : CallableFunctionNode<bool, BaseGameMob>
	{
		// Token: 0x06000456 RID: 1110 RVA: 0x0000F3B7 File Offset: 0x0000D5B7
		public override bool Invoke(BaseGameMob mob)
		{
			return this.mobDescription.IsMatch(mob);
		}

		// Token: 0x040002C4 RID: 708
		public GameMobDescription mobDescription;
	}
}
