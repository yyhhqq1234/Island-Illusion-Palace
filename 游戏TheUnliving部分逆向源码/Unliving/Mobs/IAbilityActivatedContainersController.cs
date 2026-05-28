using System;
using System.Collections.Generic;
using Unliving.Abilities;
using Unliving.Currencies;

namespace Unliving.Mobs
{
	// Token: 0x020001E1 RID: 481
	public interface IAbilityActivatedContainersController
	{
		// Token: 0x140000AB RID: 171
		// (add) Token: 0x06000FAB RID: 4011
		// (remove) Token: 0x06000FAC RID: 4012
		event Action<int, IAbilityActivatedContainer> AbilityActivatedContainerAdded;

		// Token: 0x140000AC RID: 172
		// (add) Token: 0x06000FAD RID: 4013
		// (remove) Token: 0x06000FAE RID: 4014
		event Action<IAbilityActivatedContainer> AbilityActivatedContainerDestroyed;

		// Token: 0x06000FAF RID: 4015
		void AddContainer(AbilityInfo abilityInfo, ICurrencyOperationArgs destructionRewardArgs);

		// Token: 0x06000FB0 RID: 4016
		void RemoveLastContainer(object damageSender);

		// Token: 0x06000FB1 RID: 4017
		IReadOnlyList<IAbilityActivatedContainer> GetContainers();

		// Token: 0x06000FB2 RID: 4018
		List<IAbilityActivatedContainerData> GetContainersData();

		// Token: 0x06000FB3 RID: 4019
		void SetContainersData(List<IAbilityActivatedContainerData> data);

		// Token: 0x06000FB4 RID: 4020
		bool TryGetAbilityActivatedContainer(int containerIndex, out IAbilityActivatedContainer container);

		// Token: 0x06000FB5 RID: 4021
		bool HasAbilityContainer(AbilityID abilityID);
	}
}
