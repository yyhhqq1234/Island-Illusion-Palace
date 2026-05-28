using System;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000096 RID: 150
	[Name("Get Ability Cost", 0)]
	[Category("Unliving/Abilities")]
	public sealed class GetAbilityCostNode : PureFunctionNode<float, IAbility>
	{
		// Token: 0x060003FF RID: 1023 RVA: 0x0000DFD3 File Offset: 0x0000C1D3
		public override float Invoke(IAbility ability)
		{
			if (ability == null)
			{
				return 0f;
			}
			return ability.Cost;
		}
	}
}
