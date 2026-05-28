using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Damage;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x02000384 RID: 900
	[Serializable]
	public sealed class DamageAbilityEffect : AbilityEffectBase, IDamageSender
	{
		// Token: 0x1700061E RID: 1566
		// (get) Token: 0x06001DB5 RID: 7605 RVA: 0x0005E451 File Offset: 0x0005C651
		// (set) Token: 0x06001DB6 RID: 7606 RVA: 0x0005E463 File Offset: 0x0005C663
		public DamageGenerator DamageGenerator
		{
			get
			{
				return this.damageOverride ?? this._damage;
			}
			set
			{
				this.damageOverride = value;
			}
		}

		// Token: 0x06001DB7 RID: 7607 RVA: 0x0005E46C File Offset: 0x0005C66C
		public DamageAbilityEffect()
		{
		}

		// Token: 0x06001DB8 RID: 7608 RVA: 0x0005E474 File Offset: 0x0005C674
		public DamageAbilityEffect(DamageAbilityEffect effectPrototype)
		{
			DamageGenerator damageGenerator = effectPrototype.DamageGenerator;
			this._damage = ((damageGenerator != null) ? damageGenerator.Clone() : null);
			this.divideAmountByTargetsCount = effectPrototype.divideAmountByTargetsCount;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DB9 RID: 7609 RVA: 0x0005E4A8 File Offset: 0x0005C6A8
		private Vector2? GetImpulseDirection(BaseAbility ability, Vector2 impulseTargetPosition)
		{
			return new Vector2?((impulseTargetPosition - ability.OwnerPosition).normalized);
		}

		// Token: 0x06001DBA RID: 7610 RVA: 0x0005E4D4 File Offset: 0x0005C6D4
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.DamageGenerator.ApplyDamage(effectTarget, new Vector2?(this.targetPosition), base.GetAmountModifier(effectTarget), this.impulseDirection, this.currentAbility ?? this))
			{
				base.NotifyEffectUsed(effectTarget, this.DamageGenerator.GetLastDamageInfo().DamageAmount);
				return true;
			}
			return false;
		}

		// Token: 0x06001DBB RID: 7611 RVA: 0x0005E52C File Offset: 0x0005C72C
		protected override float GetEffectAmount()
		{
			return this.DamageGenerator.amount;
		}

		// Token: 0x06001DBC RID: 7612 RVA: 0x0005E539 File Offset: 0x0005C739
		protected override void SetEffectAmount(float newAmount)
		{
			this.DamageGenerator.SetDamageAmount(newAmount, -1f);
		}

		// Token: 0x06001DBD RID: 7613 RVA: 0x0005E54C File Offset: 0x0005C74C
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			DamageGenerator damageGenerator = this.DamageGenerator;
			bool flag = damageGenerator.pushImpulse != 0f;
			int targetsCount = abilityUsingArgs.TargetsCount;
			this.currentAbility.PrepareDamageGenerator(damageGenerator);
			if (targetsCount != 0)
			{
				float amount = damageGenerator.amount;
				if (this.divideAmountByTargetsCount)
				{
					damageGenerator.SetDamageAmount(amount / (float)targetsCount, -1f);
				}
				IList<Component> targetsList = abilityUsingArgs.targetsList;
				for (int i = 0; i < targetsCount; i++)
				{
					Component component = targetsList[i];
					if (component != null)
					{
						this.targetPosition = component.transform.position;
					}
					this.impulseDirection = (flag ? this.GetImpulseDirection(this.currentAbility, this.targetPosition) : null);
					base.UseOnTarget(component, abilityUsingArgs, dt);
				}
				if (this.divideAmountByTargetsCount)
				{
					damageGenerator.SetDamageAmount(amount, -1f);
					return;
				}
			}
			else if (abilityUsingArgs.HasTargetObject)
			{
				this.targetPosition = abilityUsingArgs.TryGetTargetPosition();
				this.impulseDirection = (flag ? this.GetImpulseDirection(this.currentAbility, this.targetPosition) : null);
				base.UseOnTarget(abilityUsingArgs.targetObject, abilityUsingArgs, dt);
			}
		}

		// Token: 0x06001DBE RID: 7614 RVA: 0x0005E680 File Offset: 0x0005C880
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new DamageAbilityEffect((DamageAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010CA RID: 4298
		[SerializeField]
		private DamageGenerator _damage;

		// Token: 0x040010CB RID: 4299
		[Tooltip("Если активно, то количество урона будет распределено по целям по формуле: (amount / targetsCount).")]
		public bool divideAmountByTargetsCount;

		// Token: 0x040010CC RID: 4300
		private Vector2 targetPosition;

		// Token: 0x040010CD RID: 4301
		private Vector2? impulseDirection;

		// Token: 0x040010CE RID: 4302
		[NonSerialized]
		private DamageGenerator damageOverride;
	}
}
