using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Gameplay;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000394 RID: 916
	[Serializable]
	public sealed class StaminaRestoringAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E3A RID: 7738 RVA: 0x0005FD8E File Offset: 0x0005DF8E
		public StaminaRestoringAbilityEffect()
		{
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x0005FDA1 File Offset: 0x0005DFA1
		public StaminaRestoringAbilityEffect(StaminaRestoringAbilityEffect effectPrototype)
		{
			this.restoringAmount = effectPrototype.restoringAmount;
			this.effectTargetOverride = effectPrototype.effectTargetOverride;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x0005FDD4 File Offset: 0x0005DFD4
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.restoringAmount > 0f)
			{
				IStaminaStatOwner staminaStatOwner = effectTarget.CastOrGetComponent<IStaminaStatOwner>();
				if (staminaStatOwner != null && staminaStatOwner.CurrentStamina < staminaStatOwner.MaxStamina)
				{
					float currentStamina = staminaStatOwner.CurrentStamina;
					staminaStatOwner.CurrentStamina += this.restoringAmount * base.GetAmountModifier((Component)staminaStatOwner);
					base.NotifyEffectUsed((Component)staminaStatOwner, staminaStatOwner.CurrentStamina - currentStamina);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001E3D RID: 7741 RVA: 0x0005FE44 File Offset: 0x0005E044
		protected override float GetEffectAmount()
		{
			return this.restoringAmount;
		}

		// Token: 0x06001E3E RID: 7742 RVA: 0x0005FE4C File Offset: 0x0005E04C
		protected override void SetEffectAmount(float newAmount)
		{
			this.restoringAmount = newAmount;
		}

		// Token: 0x06001E3F RID: 7743 RVA: 0x0005FE55 File Offset: 0x0005E055
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new StaminaRestoringAbilityEffect((StaminaRestoringAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E40 RID: 7744 RVA: 0x0005FE64 File Offset: 0x0005E064
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			if (this.effectTargetOverride == StaminaRestoringAbilityEffect.TargetOverride.GroupLeader)
			{
				IGameMob gameMob = base.GetEffectOwner() as IGameMob;
				object obj;
				if (gameMob == null)
				{
					obj = null;
				}
				else
				{
					GameMobsGroupControllerBase group = gameMob.Group;
					obj = ((group != null) ? group.Leader : null);
				}
				Component effectTarget = obj as Component;
				base.UseOnTarget(effectTarget, abilityUsingArgs, dt);
				return;
			}
			if (this.effectTargetOverride == StaminaRestoringAbilityEffect.TargetOverride.AbilityOwner)
			{
				this.forceUseOnOwner = true;
			}
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x0400110F RID: 4367
		public float restoringAmount = 10f;

		// Token: 0x04001110 RID: 4368
		public StaminaRestoringAbilityEffect.TargetOverride effectTargetOverride;

		// Token: 0x02000577 RID: 1399
		public enum TargetOverride
		{
			// Token: 0x04001C61 RID: 7265
			None,
			// Token: 0x04001C62 RID: 7266
			AbilityOwner,
			// Token: 0x04001C63 RID: 7267
			GroupLeader
		}
	}
}
