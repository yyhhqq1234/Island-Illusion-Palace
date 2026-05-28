using System;
using Game.Abilities;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C5 RID: 965
	public sealed class AbilityModifiersActivatorArgs
	{
		// Token: 0x060020CA RID: 8394 RVA: 0x000674D8 File Offset: 0x000656D8
		public void CopyCommonValues(AbilityModifierUsingArgs modifiersUsingArgs)
		{
			modifiersUsingArgs.targetAbility = this.ability;
			modifiersUsingArgs.CopyTargetsInfo(this.abilityUsingArgs);
		}

		// Token: 0x04001492 RID: 5266
		public AbilityModifiersController modifiersController;

		// Token: 0x04001493 RID: 5267
		public BaseAbility ability;

		// Token: 0x04001494 RID: 5268
		public AbilityUsingStage abilityUsingStage;

		// Token: 0x04001495 RID: 5269
		public BaseAbility.UsingArgs abilityUsingArgs;

		// Token: 0x04001496 RID: 5270
		public AbilityModifiersOverrides overrides;
	}
}
