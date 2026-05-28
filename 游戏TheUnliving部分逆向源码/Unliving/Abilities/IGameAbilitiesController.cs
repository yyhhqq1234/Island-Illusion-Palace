using System;
using Game.Abilities;

namespace Unliving.Abilities
{
	// Token: 0x020003AB RID: 939
	public interface IGameAbilitiesController : IAbilitiesController
	{
		// Token: 0x06001F1B RID: 7963
		BaseAbility AddAbility(AbilityInfo abilityInfo);

		// Token: 0x06001F1C RID: 7964
		bool RemoveAbility(AbilityInfo abilityDescription);

		// Token: 0x06001F1D RID: 7965
		bool IsSpecialAbility(int abilityID);
	}
}
