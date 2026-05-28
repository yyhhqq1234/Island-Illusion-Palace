using System;
using Common.CollectionsExtensions;
using Unliving.Abilities;

namespace Unliving.Player
{
	// Token: 0x0200013D RID: 317
	[Serializable]
	public sealed class InitialPlayerAbilitiesInfo
	{
		// Token: 0x060007FE RID: 2046 RVA: 0x0001A237 File Offset: 0x00018437
		public bool IsValid()
		{
			return this.nativeAbilities.IsNotNullOrEmpty<AbilityInfo>() || this.abilities.IsNotNullOrEmpty<AbilityInfo>() || this.abilityTypes.IsNotNullOrEmpty<AbilityTypes>() || this.nativeAbilityTypes.IsNotNullOrEmpty<AbilityTypes>();
		}

		// Token: 0x0400048F RID: 1167
		public AbilityInfo[] nativeAbilities;

		// Token: 0x04000490 RID: 1168
		public AbilityTypes[] nativeAbilityTypes;

		// Token: 0x04000491 RID: 1169
		public AbilityInfo[] abilities;

		// Token: 0x04000492 RID: 1170
		public AbilityTypes[] abilityTypes;
	}
}
