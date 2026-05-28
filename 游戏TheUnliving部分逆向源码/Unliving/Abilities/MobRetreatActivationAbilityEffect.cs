using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200038D RID: 909
	[Serializable]
	public sealed class MobRetreatActivationAbilityEffect : AbilityEffectBase, IDamageSender
	{
		// Token: 0x17000620 RID: 1568
		// (get) Token: 0x06001DFC RID: 7676 RVA: 0x0005F027 File Offset: 0x0005D227
		// (set) Token: 0x06001DFD RID: 7677 RVA: 0x0005F02E File Offset: 0x0005D22E
		DamageGenerator IDamageSender.DamageGenerator
		{
			get
			{
				return DamageGenerator.Empty;
			}
			set
			{
			}
		}

		// Token: 0x06001DFE RID: 7678 RVA: 0x0005F030 File Offset: 0x0005D230
		public MobRetreatActivationAbilityEffect()
		{
		}

		// Token: 0x06001DFF RID: 7679 RVA: 0x0005F043 File Offset: 0x0005D243
		public MobRetreatActivationAbilityEffect(MobRetreatActivationAbilityEffect effectPrototype)
		{
			this.maxRetreatDistance = effectPrototype.maxRetreatDistance;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E00 RID: 7680 RVA: 0x0005F06C File Offset: 0x0005D26C
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			Component component = base.GetEffectOwner() as Component;
			if (!component.IsNull())
			{
				this.ownerPosition = new Vector2?(component.transform.position);
			}
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x06001E01 RID: 7681 RVA: 0x0005F0B0 File Offset: 0x0005D2B0
		protected override bool Use(Component effectTarget, float dt)
		{
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			GameMobAIController gameMobAIController = (baseGameMob != null) ? baseGameMob.AIController : null;
			if (gameMobAIController != null)
			{
				gameMobAIController.TrySetScared(dt, this.maxRetreatDistance, this.ownerPosition, true);
				if (gameMobAIController.IsScared)
				{
					base.NotifyEffectUsed(effectTarget, 0f);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x0005F0FF File Offset: 0x0005D2FF
		protected override float GetEffectAmount()
		{
			return this.maxRetreatDistance;
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x0005F107 File Offset: 0x0005D307
		protected override void SetEffectAmount(float newAmount)
		{
			this.maxRetreatDistance = newAmount;
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x0005F110 File Offset: 0x0005D310
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new MobRetreatActivationAbilityEffect((MobRetreatActivationAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010E2 RID: 4322
		public float maxRetreatDistance = 5f;

		// Token: 0x040010E3 RID: 4323
		private Vector2? ownerPosition;
	}
}
