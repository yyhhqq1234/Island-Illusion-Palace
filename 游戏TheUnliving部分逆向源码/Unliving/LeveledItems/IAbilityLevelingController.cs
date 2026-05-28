using System;
using Game.Abilities;

namespace Unliving.LeveledItems
{
	// Token: 0x02000254 RID: 596
	public interface IAbilityLevelingController
	{
		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x060013F7 RID: 5111
		// (set) Token: 0x060013F8 RID: 5112
		AbilityPropertyValuesOverrides[] AbilityPropertiesOverrides { get; set; }

		// Token: 0x060013F9 RID: 5113
		bool GetAbilityLevel(BaseAbility ability, out int level);
	}
}
