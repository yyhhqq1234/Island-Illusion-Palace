using System;
using Common.Factories;
using Common.ServiceRegistry;
using UnityEngine;
using Unliving.LeveledItems;

namespace Unliving.Abilities
{
	// Token: 0x0200039B RID: 923
	[Service(typeof(AbilityActivatedContainersFactory), new Type[]
	{
		typeof(IAbilityActivatedContainersFactory)
	})]
	public sealed class AbilityActivatedContainersFactory : AbilitiesFactory, IAbilityActivatedContainersFactory, IGameAbilitiesFactory, IFactory<AbilityFactoryArgs, UnityEngine.Object>, IFactory, IAbilityPropertiesOverridesHandler
	{
		// Token: 0x0400112D RID: 4397
		public const int InitialAbilityID = 13000;
	}
}
