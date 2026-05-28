using System;
using System.Collections.Generic;

namespace Unliving.LeveledItems
{
	// Token: 0x02000257 RID: 599
	public interface IAbilityPropertiesOverridesSource
	{
		// Token: 0x060013FD RID: 5117
		AbilityPropertyValuesOverrides[] GetAbilityPropertiesOverrides(IAbilityLevelingController levelingController);

		// Token: 0x060013FE RID: 5118
		IReadOnlyList<AbilityPropertyValuesOverrides> GetAbilityPropertiesOverrides(string abilityName);
	}
}
