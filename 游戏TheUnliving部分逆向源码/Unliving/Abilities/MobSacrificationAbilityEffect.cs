using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200038E RID: 910
	[Serializable]
	public sealed class MobSacrificationAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E05 RID: 7685 RVA: 0x0005F11D File Offset: 0x0005D31D
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001E06 RID: 7686 RVA: 0x0005F124 File Offset: 0x0005D324
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001E07 RID: 7687 RVA: 0x0005F126 File Offset: 0x0005D326
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			this.sacrificer = (base.CurrentAbility.Owner as BaseGameMob);
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x06001E08 RID: 7688 RVA: 0x0005F148 File Offset: 0x0005D348
		protected override bool Use(Component effectTarget, float dt)
		{
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			return baseGameMob != null && baseGameMob.Sacrifice(this.sacrificer, true, this.checkGroup);
		}

		// Token: 0x06001E09 RID: 7689 RVA: 0x0005F17A File Offset: 0x0005D37A
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new MobSacrificationAbilityEffect((MobSacrificationAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E0A RID: 7690 RVA: 0x0005F187 File Offset: 0x0005D387
		public MobSacrificationAbilityEffect()
		{
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x0005F18F File Offset: 0x0005D38F
		public MobSacrificationAbilityEffect(MobSacrificationAbilityEffect effectPrototype)
		{
			this.checkGroup = effectPrototype.checkGroup;
		}

		// Token: 0x040010E4 RID: 4324
		public bool checkGroup;

		// Token: 0x040010E5 RID: 4325
		private BaseGameMob sacrificer;
	}
}
