using System;
using Common.Factories;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.LeveledItems;

namespace Unliving.Abilities
{
	// Token: 0x020003A3 RID: 931
	public interface IGameAbilitiesFactory : IFactory<AbilityFactoryArgs, UnityEngine.Object>, IFactory, IAbilityPropertiesOverridesHandler
	{
		// Token: 0x06001E97 RID: 7831
		AbilityFactoryPrototype GetAbilityPrototypeData(int abilityID);

		// Token: 0x06001E98 RID: 7832
		AbilitySpecialBehaviourDescription GetAbilitySpecialBehaviourDescription(BaseAbility ability);

		// Token: 0x06001E99 RID: 7833
		AbilitySpecialBehaviourDescription GetRandomAbilitySpecialBehaviourDescription(int abilityID);

		// Token: 0x06001E9A RID: 7834
		void SetAbilitySpecialBehaviourFilterPredicate(Predicate<AbilitySpecialBehaviourGenerationOption> optionsFilter);
	}
}
