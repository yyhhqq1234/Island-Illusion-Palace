using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000234 RID: 564
	[CreateAssetMenu(fileName = "MobSacrificeAbilityTrigger", menuName = "Abilities/Triggers/Mob Sacrifice Trigger")]
	public sealed class MobSacrificeAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06001352 RID: 4946 RVA: 0x0003CAF2 File Offset: 0x0003ACF2
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06001353 RID: 4947 RVA: 0x0003CAF5 File Offset: 0x0003ACF5
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06001354 RID: 4948 RVA: 0x0003CAFC File Offset: 0x0003ACFC
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001355 RID: 4949 RVA: 0x0003CB00 File Offset: 0x0003AD00
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			BaseGameMob abilityOwningMob = base.GetAbilityOwningMob(ability);
			return abilityOwningMob != null && abilityOwningMob.IsSacrificed;
		}
	}
}
