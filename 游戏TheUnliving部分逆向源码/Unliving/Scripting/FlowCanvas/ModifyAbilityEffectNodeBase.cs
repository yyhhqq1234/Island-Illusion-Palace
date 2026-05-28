using System;
using FlowCanvas;
using Game.Abilities;
using Unliving.Abilities;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B9 RID: 185
	public abstract class ModifyAbilityEffectNodeBase<TEffect> : ModifyAbilityNodeBase where TEffect : class
	{
		// Token: 0x060004B6 RID: 1206 RVA: 0x00010F60 File Offset: 0x0000F160
		protected sealed override bool TryModifyAbility(GameAbilitiesController abilitiesController, BaseAbility ability, Flow flow)
		{
			IEffectsBasedAbility effectsBasedAbility = ability as IEffectsBasedAbility;
			if (effectsBasedAbility != null)
			{
				AbilityEffectBase[] abilityEffects = effectsBasedAbility.AbilityEffects;
				for (int i = 0; i < abilityEffects.Length; i++)
				{
					TEffect teffect = abilityEffects[i] as TEffect;
					if (teffect != null && this.TryModifyAbilityEffect(effectsBasedAbility, teffect))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060004B7 RID: 1207
		protected abstract bool TryModifyAbilityEffect(IEffectsBasedAbility ability, TEffect abilityEffect);
	}
}
