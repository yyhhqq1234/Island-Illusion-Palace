using System;
using Game.Abilities;
using Game.Gameplay;
using UnityEngine;

namespace Unliving.Abilities.Effects
{
	// Token: 0x020003D3 RID: 979
	[Serializable]
	public sealed class UnlimitedStaminaAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06002127 RID: 8487 RVA: 0x000680DF File Offset: 0x000662DF
		public UnlimitedStaminaAbilityEffect()
		{
		}

		// Token: 0x06002128 RID: 8488 RVA: 0x000680E7 File Offset: 0x000662E7
		public UnlimitedStaminaAbilityEffect(UnlimitedStaminaAbilityEffect effectPrototype)
		{
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06002129 RID: 8489 RVA: 0x000680F6 File Offset: 0x000662F6
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x0600212A RID: 8490 RVA: 0x000680FD File Offset: 0x000662FD
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x0600212B RID: 8491 RVA: 0x00068100 File Offset: 0x00066300
		protected override bool Use(Component effectTarget, float dt)
		{
			IStaminaStatOwner staminaStatOwner = effectTarget as IStaminaStatOwner;
			if (staminaStatOwner != null)
			{
				staminaStatOwner.CurrentStamina = staminaStatOwner.MaxStamina;
				return true;
			}
			return false;
		}

		// Token: 0x0600212C RID: 8492 RVA: 0x00068126 File Offset: 0x00066326
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new UnlimitedStaminaAbilityEffect((UnlimitedStaminaAbilityEffect)originalBaseEffect);
		}
	}
}
