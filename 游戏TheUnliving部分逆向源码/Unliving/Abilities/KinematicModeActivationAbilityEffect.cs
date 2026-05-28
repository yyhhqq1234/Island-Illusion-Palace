using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Mobs;
using Unliving.Mobs.Motion;

namespace Unliving.Abilities
{
	// Token: 0x02000389 RID: 905
	public sealed class KinematicModeActivationAbilityEffect : AbilityEffectBase, IRevertibleAbilityEffect
	{
		// Token: 0x06001DDF RID: 7647 RVA: 0x0005EBBE File Offset: 0x0005CDBE
		private void TrySetDamageReceivingBlocked(BaseGameMob targetMob, bool isBlocked)
		{
			if (!this.blockDamageReceiving)
			{
				return;
			}
			if (isBlocked)
			{
				targetMob.SetLayerOverride(2);
				return;
			}
			targetMob.ResetLayerOverride();
		}

		// Token: 0x06001DE0 RID: 7648 RVA: 0x0005EBDA File Offset: 0x0005CDDA
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DE1 RID: 7649 RVA: 0x0005EBE1 File Offset: 0x0005CDE1
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DE2 RID: 7650 RVA: 0x0005EBE4 File Offset: 0x0005CDE4
		protected override bool Use(Component effectTarget, float dt)
		{
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob != null)
			{
				baseGameMob.IsKinematic = true;
				this.TrySetDamageReceivingBlocked(baseGameMob, true);
				return true;
			}
			return false;
		}

		// Token: 0x06001DE3 RID: 7651 RVA: 0x0005EC13 File Offset: 0x0005CE13
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new KinematicModeActivationAbilityEffect((KinematicModeActivationAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001DE4 RID: 7652 RVA: 0x0005EC20 File Offset: 0x0005CE20
		public KinematicModeActivationAbilityEffect()
		{
		}

		// Token: 0x06001DE5 RID: 7653 RVA: 0x0005EC28 File Offset: 0x0005CE28
		public KinematicModeActivationAbilityEffect(KinematicModeActivationAbilityEffect effectPrototype)
		{
			this.blockDamageReceiving = effectPrototype.blockDamageReceiving;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DE6 RID: 7654 RVA: 0x0005EC44 File Offset: 0x0005CE44
		void IRevertibleAbilityEffect.RevertEffect(IAbility ability, object effectTarget)
		{
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob != null)
			{
				GameMobMotionControllerBase motionController = baseGameMob.MotionController;
				if (motionController == null || motionController.CurrentKinematicMotion == null)
				{
					baseGameMob.IsKinematic = false;
				}
				this.TrySetDamageReceivingBlocked(baseGameMob, false);
			}
		}

		// Token: 0x040010D9 RID: 4313
		private const int IgnoreRaycastLayer = 2;

		// Token: 0x040010DA RID: 4314
		[FormerlySerializedAs("affectMobCollider")]
		public bool blockDamageReceiving;
	}
}
