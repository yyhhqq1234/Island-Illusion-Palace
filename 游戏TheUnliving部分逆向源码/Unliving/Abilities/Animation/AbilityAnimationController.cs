using System;
using Game.Abilities;
using Game.Abilities.Animation;
using UnityEngine;
using Unliving.Mobs.Animation;
using Unliving.Mobs.Motion;

namespace Unliving.Abilities.Animation
{
	// Token: 0x020003E3 RID: 995
	[CreateAssetMenu(fileName = "MobAbilityAnimationController", menuName = "Abilities/Controllers/Mob Ability Animation Controller")]
	public sealed class AbilityAnimationController : BaseAbilityAnimationController
	{
		// Token: 0x060021A8 RID: 8616 RVA: 0x00069281 File Offset: 0x00067481
		private void OnHarpoonAbilityPullingAttemptTaken(PullingProjectileAbility ability, GameMobKinematicMotionBase pullingMotion)
		{
			base.ActivateAnimatorTrigger(AbilityAnimationController.HarpoonLandedTriggerID);
		}

		// Token: 0x060021A9 RID: 8617 RVA: 0x00069290 File Offset: 0x00067490
		protected override void OnPreparingAbility(IAbility ability, object usingArgs)
		{
			if (this.sacrificeAnimationIndexOverride > 0 && ability.PrepProgress == 0f && ((BaseAbility)ability).IsMobActivationAbility())
			{
				((GameMobAnimationController)this.currentAnimationController).SetSacrificeAnimationIndex(this.sacrificeAnimationIndexOverride);
			}
			base.OnPreparingAbility(ability, usingArgs);
		}

		// Token: 0x060021AA RID: 8618 RVA: 0x000692E0 File Offset: 0x000674E0
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			PullingProjectileAbility pullingProjectileAbility = ability as PullingProjectileAbility;
			if (pullingProjectileAbility != null)
			{
				pullingProjectileAbility.PullingAttemptTaken += this.OnHarpoonAbilityPullingAttemptTaken;
			}
		}

		// Token: 0x040014FB RID: 5371
		private static readonly int HarpoonLandedTriggerID = Animator.StringToHash("HarpoonLanded");

		// Token: 0x040014FC RID: 5372
		public int sacrificeAnimationIndexOverride = -1;
	}
}
