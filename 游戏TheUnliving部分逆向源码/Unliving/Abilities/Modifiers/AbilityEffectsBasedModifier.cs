using System;
using Game.Abilities;
using UnityEngine;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003BD RID: 957
	[Serializable]
	public sealed class AbilityEffectsBasedModifier : AbilityModifierBase
	{
		// Token: 0x17000697 RID: 1687
		// (get) Token: 0x06002055 RID: 8277 RVA: 0x00065E53 File Offset: 0x00064053
		public override bool IsActive
		{
			get
			{
				return this.additionalEffects != null && this.additionalEffects.Length != 0;
			}
		}

		// Token: 0x06002056 RID: 8278 RVA: 0x00065E6C File Offset: 0x0006406C
		protected override void OnUse(AbilityModifierUsingArgs usingArgs)
		{
			BaseAbility targetAbility = usingArgs.targetAbility;
			BaseAbility.UsingArgs targetsInfo = usingArgs.targetsInfo;
			float amountModifierOverride = (this.baseEffectsAmountModifier > 0f) ? (this.baseEffectsAmountModifier * (float)usingArgs.modifiersUsingCount) : 1f;
			AbilityModifiersOverrides extension = usingArgs.targetAbility.GetExtension<AbilityModifiersOverrides>();
			int num = (extension != null) ? extension.levelOverride : 0;
			MobStatModifier mobStatModifier;
			bool flag = usingArgs.TryGetStatModifier(MobStatID.GroupMobsActivationModifiersDamage, out mobStatModifier);
			for (int i = 0; i < this.additionalEffects.Length; i++)
			{
				AbilityEffectBase abilityEffectBase = this.additionalEffects[i];
				if (num > 0)
				{
					AbilityAdditionAbilityEffect abilityAdditionAbilityEffect = abilityEffectBase as AbilityAdditionAbilityEffect;
					if (abilityAdditionAbilityEffect != null && abilityAdditionAbilityEffect.abilityLevelOverride <= 0)
					{
						abilityAdditionAbilityEffect.abilityLevelOverride = num;
					}
				}
				float amount = abilityEffectBase.Amount;
				abilityEffectBase.CurrentAbility = targetAbility;
				abilityEffectBase.AmountModifierOverride = amountModifierOverride;
				if (flag && abilityEffectBase.IsDamagingAbilityEffect())
				{
					abilityEffectBase.Amount = mobStatModifier.GetModifiedStatValue(abilityEffectBase.Amount);
				}
				abilityEffectBase.Use(targetsInfo, 1f);
				abilityEffectBase.CurrentAbility = null;
				abilityEffectBase.Amount = amount;
			}
		}

		// Token: 0x06002057 RID: 8279 RVA: 0x00065F72 File Offset: 0x00064172
		protected override void OnReset(AbilityModifierUsingArgs usingArgs)
		{
		}

		// Token: 0x06002058 RID: 8280 RVA: 0x00065F74 File Offset: 0x00064174
		public AbilityEffectsBasedModifier(AbilityEffectsBasedModifier modifierPrototype) : base(modifierPrototype)
		{
			this.additionalEffects = modifierPrototype.additionalEffects;
			this.baseEffectsAmountModifier = modifierPrototype.baseEffectsAmountModifier;
		}

		// Token: 0x06002059 RID: 8281 RVA: 0x00065FA0 File Offset: 0x000641A0
		public override AbilityModifierBase Clone()
		{
			return new AbilityEffectsBasedModifier(this);
		}

		// Token: 0x04001454 RID: 5204
		[SerializeReference]
		[AbilityEffectsList]
		public AbilityEffectBase[] additionalEffects;

		// Token: 0x04001455 RID: 5205
		public float baseEffectsAmountModifier = 1f;
	}
}
