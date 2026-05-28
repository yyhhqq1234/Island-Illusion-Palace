using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000387 RID: 903
	[Serializable]
	public sealed class IncomingDamageModificationAbilityEffect : StateBasedAbilityEffect
	{
		// Token: 0x06001DCF RID: 7631 RVA: 0x0005E949 File Offset: 0x0005CB49
		public IncomingDamageModificationAbilityEffect()
		{
		}

		// Token: 0x06001DD0 RID: 7632 RVA: 0x0005E95C File Offset: 0x0005CB5C
		public IncomingDamageModificationAbilityEffect(IncomingDamageModificationAbilityEffect effectPrototype)
		{
			this.damageMultiplier = effectPrototype.damageMultiplier;
			this.setInvisibleToEnemies = effectPrototype.setInvisibleToEnemies;
			this.useOnce = effectPrototype.useOnce;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DD1 RID: 7633 RVA: 0x0005E99A File Offset: 0x0005CB9A
		private bool ModifyDamage(IHitPointsChangingArgs damageArgs)
		{
			if (damageArgs.IsDamage && !damageArgs.IsForcedChanging)
			{
				damageArgs.Amount *= Mathf.Max(this.damageMultiplier, 0f);
				return true;
			}
			return false;
		}

		// Token: 0x06001DD2 RID: 7634 RVA: 0x0005E9CC File Offset: 0x0005CBCC
		protected override void SetEffectActive(Component effectTarget, bool isActive)
		{
			IDamageable damageable = effectTarget.CastOrGetComponent<IDamageable>();
			if (damageable != null)
			{
				if (this.setInvisibleToEnemies)
				{
					BaseGameMob baseGameMob = damageable.Behaviour as BaseGameMob;
					if (baseGameMob != null)
					{
						baseGameMob.isVisibleToEnemies = !isActive;
					}
				}
				if (isActive)
				{
					damageable.BeforeHitPointsChanged += this.OnBeforeEffectTargetHealthChanged;
					return;
				}
				damageable.BeforeHitPointsChanged -= this.OnBeforeEffectTargetHealthChanged;
			}
		}

		// Token: 0x06001DD3 RID: 7635 RVA: 0x0005EA2C File Offset: 0x0005CC2C
		protected override float GetEffectAmount()
		{
			return this.damageMultiplier;
		}

		// Token: 0x06001DD4 RID: 7636 RVA: 0x0005EA34 File Offset: 0x0005CC34
		protected override void SetEffectAmount(float newAmount)
		{
			this.damageMultiplier = newAmount;
		}

		// Token: 0x06001DD5 RID: 7637 RVA: 0x0005EA40 File Offset: 0x0005CC40
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			if (this.useOnce && base.WasUsed())
			{
				return;
			}
			DamageGenerator.DamageSendingArgs damageArgs;
			if (abilityUsingArgs.TryGetAdditionalContext(out damageArgs))
			{
				this.ModifyDamage(damageArgs);
				base.MarkAsUsed();
				return;
			}
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x06001DD6 RID: 7638 RVA: 0x0005EA7F File Offset: 0x0005CC7F
		private void OnBeforeEffectTargetHealthChanged(IHitPointsSource damageReceiver, object sender, IHitPointsChangingArgs args)
		{
			if (this.ModifyDamage(args) && this.useOnce)
			{
				base.UnregisterAffectedTarget(((IDamageable)damageReceiver).Behaviour);
			}
		}

		// Token: 0x06001DD7 RID: 7639 RVA: 0x0005EAA4 File Offset: 0x0005CCA4
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new IncomingDamageModificationAbilityEffect((IncomingDamageModificationAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010D5 RID: 4309
		public float damageMultiplier = 0.5f;

		// Token: 0x040010D6 RID: 4310
		public bool setInvisibleToEnemies;

		// Token: 0x040010D7 RID: 4311
		public bool useOnce;
	}
}
