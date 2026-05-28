using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x02000386 RID: 902
	[Serializable]
	public sealed class HealingAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001DC8 RID: 7624 RVA: 0x0005E839 File Offset: 0x0005CA39
		public HealingAbilityEffect()
		{
		}

		// Token: 0x06001DC9 RID: 7625 RVA: 0x0005E84C File Offset: 0x0005CA4C
		public HealingAbilityEffect(HealingAbilityEffect effectPrototype)
		{
			this.healingAmount = effectPrototype.healingAmount;
			this.isSilentEffect = effectPrototype.isSilentEffect;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DCA RID: 7626 RVA: 0x0005E880 File Offset: 0x0005CA80
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.healingAmount > 0f)
			{
				IDamageable damageable = (!effectTarget.IsNull()) ? effectTarget.GetComponent<IDamageable>() : null;
				if (damageable != null && damageable.HitPointsLack > 0f)
				{
					float currentHitPoints = damageable.CurrentHitPoints;
					HealingAbilityEffect.HealthRestoringArgs.amount = this.healingAmount * base.GetAmountModifier(effectTarget);
					HealingAbilityEffect.HealthRestoringArgs.isSilentChanging = this.isSilentEffect;
					IHitPointsSource hitPointsSource = damageable;
					BaseAbility currentAbility = this.currentAbility;
					hitPointsSource.ModifyHitPoints((currentAbility != null) ? currentAbility.AbilityEffectSender : null, HealingAbilityEffect.HealthRestoringArgs);
					base.NotifyEffectUsed(effectTarget, damageable.CurrentHitPoints - currentHitPoints);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001DCB RID: 7627 RVA: 0x0005E91E File Offset: 0x0005CB1E
		protected override float GetEffectAmount()
		{
			return this.healingAmount;
		}

		// Token: 0x06001DCC RID: 7628 RVA: 0x0005E926 File Offset: 0x0005CB26
		protected override void SetEffectAmount(float newAmount)
		{
			this.healingAmount = newAmount;
		}

		// Token: 0x06001DCD RID: 7629 RVA: 0x0005E92F File Offset: 0x0005CB2F
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new HealingAbilityEffect((HealingAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010D2 RID: 4306
		private static readonly HitPointsController.HPChangingArgs HealthRestoringArgs = new HitPointsController.HPChangingArgs(false);

		// Token: 0x040010D3 RID: 4307
		public float healingAmount = 25f;

		// Token: 0x040010D4 RID: 4308
		public bool isSilentEffect;
	}
}
