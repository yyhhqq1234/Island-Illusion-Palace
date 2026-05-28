using System;
using Common.Factories;
using Game.Abilities;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003F2 RID: 1010
	public interface IAbilitySpecialBehavioursActivatorsFactory : IFactory<AbilitySpecialBehaviourActivatorID, AbilityModifiersActivatorBase>, IFactory
	{
		// Token: 0x060021FF RID: 8703
		AbilitySpecialBehaviourActivatorID GetSpecialBehaviourActivatorID(BaseAbility ability);
	}
}
