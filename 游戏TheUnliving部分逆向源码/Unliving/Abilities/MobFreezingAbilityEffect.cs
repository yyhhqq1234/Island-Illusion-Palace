using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;

namespace Unliving.Abilities
{
	// Token: 0x0200038B RID: 907
	[Serializable]
	public sealed class MobFreezingAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001DEE RID: 7662 RVA: 0x0005EE0A File Offset: 0x0005D00A
		public MobFreezingAbilityEffect()
		{
		}

		// Token: 0x06001DEF RID: 7663 RVA: 0x0005EE12 File Offset: 0x0005D012
		public MobFreezingAbilityEffect(MobFreezingAbilityEffect effectPrototype)
		{
			this.movementFreezing = effectPrototype.movementFreezing;
			this.blockAbilitiesUsing = effectPrototype.blockAbilitiesUsing;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DF0 RID: 7664 RVA: 0x0005EE3C File Offset: 0x0005D03C
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.movementFreezing != MobFreezingAbilityEffect.MovementFreezingType.None)
			{
				BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
				if (baseGameMob != null)
				{
					GameMobMotionControllerBase motionController = baseGameMob.MotionController;
					if (motionController != null)
					{
						motionController.FreezeMovement(dt, this.movementFreezing == MobFreezingAbilityEffect.MovementFreezingType.MakeFullyStatic);
					}
					else
					{
						baseGameMob.BlockMovement(dt);
					}
					if (this.blockAbilitiesUsing)
					{
						GameAbilitiesController abilitiesController = baseGameMob.AbilitiesController;
						if (abilitiesController != null)
						{
							abilitiesController.Deactivate(dt);
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x0005EEA1 File Offset: 0x0005D0A1
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x0005EEA8 File Offset: 0x0005D0A8
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x0005EEAA File Offset: 0x0005D0AA
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new MobFreezingAbilityEffect((MobFreezingAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010DE RID: 4318
		public MobFreezingAbilityEffect.MovementFreezingType movementFreezing;

		// Token: 0x040010DF RID: 4319
		public bool blockAbilitiesUsing;

		// Token: 0x02000571 RID: 1393
		public enum MovementFreezingType
		{
			// Token: 0x04001C4C RID: 7244
			None,
			// Token: 0x04001C4D RID: 7245
			FreezeMovement,
			// Token: 0x04001C4E RID: 7246
			MakeFullyStatic
		}
	}
}
