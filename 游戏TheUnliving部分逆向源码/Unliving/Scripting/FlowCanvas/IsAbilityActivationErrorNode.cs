using System;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A3 RID: 163
	[Name("Is Ability Activation Error", 0)]
	[Category("Unliving/Abilities")]
	public sealed class IsAbilityActivationErrorNode : CallableFunctionNode<bool, BaseAbility.ActivationErrorType, BaseAbility.ActivationErrorType>
	{
		// Token: 0x0600044E RID: 1102 RVA: 0x0000F276 File Offset: 0x0000D476
		public override bool Invoke(BaseAbility.ActivationErrorType currentError, BaseAbility.ActivationErrorType targetError)
		{
			return currentError == targetError;
		}
	}
}
