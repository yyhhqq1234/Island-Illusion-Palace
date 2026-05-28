using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000235 RID: 565
	[CreateAssetMenu(fileName = "MobVisibilityAbilityTrigger", menuName = "Abilities/Triggers/Mob Visibility Trigger")]
	public sealed class MobVisibilityAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06001357 RID: 4951 RVA: 0x0003CB2E File Offset: 0x0003AD2E
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x0003CB31 File Offset: 0x0003AD31
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06001359 RID: 4953 RVA: 0x0003CB38 File Offset: 0x0003AD38
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600135A RID: 4954 RVA: 0x0003CB3B File Offset: 0x0003AD3B
		private static bool IsAbilityOwnerVisible(BaseAbility ability)
		{
			return ((BaseGameMob)ability.Owner).IsRendererVisible;
		}

		// Token: 0x0600135B RID: 4955 RVA: 0x0003CB4D File Offset: 0x0003AD4D
		protected override MobAbilityTriggerBase.TriggerState CreateTriggerState(BaseAbility ability)
		{
			if (!this.checkMobVisibilityOnce)
			{
				return null;
			}
			return new MobAbilityTriggerBase.TriggerState();
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x0003CB60 File Offset: 0x0003AD60
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (this.checkMobVisibilityOnce)
			{
				MobAbilityTriggerBase.TriggerState storedTriggerState = base.GetStoredTriggerState(ability);
				if (!storedTriggerState.isConditionReached)
				{
					storedTriggerState.isConditionReached = MobVisibilityAbilityTrigger.IsAbilityOwnerVisible(ability);
				}
				return storedTriggerState.isConditionReached;
			}
			return MobVisibilityAbilityTrigger.IsAbilityOwnerVisible(ability);
		}

		// Token: 0x04000B3A RID: 2874
		public bool checkMobVisibilityOnce = true;
	}
}
