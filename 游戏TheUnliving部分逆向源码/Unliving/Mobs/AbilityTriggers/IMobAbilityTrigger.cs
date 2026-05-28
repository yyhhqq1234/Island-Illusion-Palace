using System;
using Game.Abilities;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000230 RID: 560
	public interface IMobAbilityTrigger
	{
		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06001335 RID: 4917
		bool RequiresTarget { get; }

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06001336 RID: 4918
		float ActivationRange { get; }

		// Token: 0x06001337 RID: 4919
		bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs);

		// Token: 0x06001338 RID: 4920
		bool IsConditionReached(BaseAbility ability);
	}
}
