using System;
using Common.Factories;
using UnityEngine;
using Unliving.LeveledItems;

namespace Unliving.Abilities
{
	// Token: 0x020003A2 RID: 930
	public interface IAbilityActivatedContainersFactory : IGameAbilitiesFactory, IFactory<AbilityFactoryArgs, UnityEngine.Object>, IFactory, IAbilityPropertiesOverridesHandler
	{
	}
}
