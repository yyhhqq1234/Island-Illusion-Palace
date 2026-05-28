using System;
using System.Collections.Generic;
using Game.Abilities;
using Unliving.Abilities;
using Unliving.Currencies;

namespace Unliving.Mobs
{
	// Token: 0x020001DF RID: 479
	public interface IAbilityActivatedContainer
	{
		// Token: 0x140000AA RID: 170
		// (add) Token: 0x06000FA1 RID: 4001
		// (remove) Token: 0x06000FA2 RID: 4002
		event Action<BaseAbility> AbilityAdded;

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06000FA3 RID: 4003
		AbilityFactoryPrototype AbilityPrototype { get; }

		// Token: 0x06000FA4 RID: 4004
		void AddAbility(AbilityInfo abilityInfo);

		// Token: 0x06000FA5 RID: 4005
		void SetDestructionRewardArgs(ICurrencyOperationArgs args);

		// Token: 0x06000FA6 RID: 4006
		IReadOnlyList<BaseAbility> GetAbilities();

		// Token: 0x06000FA7 RID: 4007
		void DestroyContainer(object damageSender);

		// Token: 0x06000FA8 RID: 4008
		IAbilityActivatedContainerData GetAbilitiesContainerData();

		// Token: 0x06000FA9 RID: 4009
		void SetAbilitiesData(IAbilityActivatedContainerData data);
	}
}
