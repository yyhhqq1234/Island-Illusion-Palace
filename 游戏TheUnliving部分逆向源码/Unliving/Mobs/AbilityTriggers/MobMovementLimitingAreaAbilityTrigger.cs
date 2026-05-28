using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs.Motion;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000233 RID: 563
	[CreateAssetMenu(fileName = "MobMovementLimitingAreaAbilityTrigger", menuName = "Abilities/Triggers/Mob Movement Limiting Area Trigger")]
	public sealed class MobMovementLimitingAreaAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x0600134D RID: 4941 RVA: 0x0003CA9D File Offset: 0x0003AC9D
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x0600134E RID: 4942 RVA: 0x0003CAA0 File Offset: 0x0003ACA0
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x0600134F RID: 4943 RVA: 0x0003CAA7 File Offset: 0x0003ACA7
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001350 RID: 4944 RVA: 0x0003CAAC File Offset: 0x0003ACAC
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			BaseGameMob abilityOwningMob = base.GetAbilityOwningMob(ability);
			GameMobMotionControllerBase gameMobMotionControllerBase = (abilityOwningMob != null) ? abilityOwningMob.MotionController : null;
			if (gameMobMotionControllerBase == null)
			{
				return true;
			}
			if (this.triggerType != MobMovementLimitingAreaAbilityTrigger.TriggerType.HasActiveLimitingArea)
			{
				return !gameMobMotionControllerBase.HasActiveMovementLimitingArea;
			}
			return gameMobMotionControllerBase.HasActiveMovementLimitingArea;
		}

		// Token: 0x04000B39 RID: 2873
		public MobMovementLimitingAreaAbilityTrigger.TriggerType triggerType;

		// Token: 0x020004CB RID: 1227
		public enum TriggerType
		{
			// Token: 0x040019C3 RID: 6595
			HasActiveLimitingArea,
			// Token: 0x040019C4 RID: 6596
			HasNoLimitingArea
		}
	}
}
