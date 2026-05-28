using System;
using Common.Factories;
using Game.Abilities;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003F3 RID: 1011
	public interface IAbilitySpecialBehavioursFactory : IFactory<AbilitySpecialBehaviourID, AbilityModifiersController>, IFactory
	{
		// Token: 0x06002200 RID: 8704
		AbilitySpecialBehaviourID GetSpecialBehaviourID(BaseAbility ability);
	}
}
