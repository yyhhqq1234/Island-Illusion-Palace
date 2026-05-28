using System;
using FlowCanvas;
using Game.Abilities;
using Unliving.Abilities;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000BA RID: 186
	public abstract class ModifyAbilityExtensionNodeBase<TExtension> : ModifyAbilityNodeBase where TExtension : class, IAbilityExtension
	{
		// Token: 0x060004B9 RID: 1209 RVA: 0x00010FB8 File Offset: 0x0000F1B8
		protected sealed override bool TryModifyAbility(GameAbilitiesController abilitiesController, BaseAbility ability, Flow flow)
		{
			TExtension extension;
			return ability.TryGetExtension(out extension) && this.ModifyAbilityExtension(ability, extension);
		}

		// Token: 0x060004BA RID: 1210
		protected abstract bool ModifyAbilityExtension(BaseAbility ability, TExtension extension);
	}
}
