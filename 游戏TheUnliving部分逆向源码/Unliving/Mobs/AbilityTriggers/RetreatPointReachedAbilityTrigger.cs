using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000238 RID: 568
	[CreateAssetMenu(fileName = "RetreatPointReachedAbilityTrigger", menuName = "Abilities/Triggers/Retreat Point Reached Trigger")]
	public sealed class RetreatPointReachedAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06001368 RID: 4968 RVA: 0x0003CC39 File Offset: 0x0003AE39
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06001369 RID: 4969 RVA: 0x0003CC3C File Offset: 0x0003AE3C
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x0600136A RID: 4970 RVA: 0x0003CC43 File Offset: 0x0003AE43
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600136B RID: 4971 RVA: 0x0003CC48 File Offset: 0x0003AE48
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			BaseGameMob abilityOwningMob = base.GetAbilityOwningMob(ability);
			GameMobAIController gameMobAIController = (abilityOwningMob != null) ? abilityOwningMob.AIController : null;
			return gameMobAIController != null && gameMobAIController.IsScared && gameMobAIController.IsIdling;
		}
	}
}
