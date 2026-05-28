using System;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A5 RID: 165
	[Name("Is Target Ability", 0)]
	[Category("Unliving/Abilities")]
	public sealed class IsTargetAbilityNode : CallableFunctionNode<bool, BaseAbility>
	{
		// Token: 0x06000454 RID: 1108 RVA: 0x0000F3A1 File Offset: 0x0000D5A1
		public override bool Invoke(BaseAbility ability)
		{
			return this.abilityDescription.IsMatch(ability);
		}

		// Token: 0x040002C3 RID: 707
		public AbilityDescription abilityDescription;
	}
}
