using System;
using Game.Localization;

namespace Unliving.Abilities
{
	// Token: 0x020003B0 RID: 944
	[Flags]
	[LocalizationObject(LocalizationPrefix.mobActivationTypeID_)]
	public enum MobActivationAbilityType
	{
		// Token: 0x040013BB RID: 5051
		None = 0,
		// Token: 0x040013BC RID: 5052
		Fighters = 1,
		// Token: 0x040013BD RID: 5053
		Giants = 2,
		// Token: 0x040013BE RID: 5054
		Ranged = 4,
		// Token: 0x040013BF RID: 5055
		Unholy = 8,
		// Token: 0x040013C0 RID: 5056
		Test1 = 16,
		// Token: 0x040013C1 RID: 5057
		Test2 = 32,
		// Token: 0x040013C2 RID: 5058
		Test3 = 64,
		// Token: 0x040013C3 RID: 5059
		Test4 = 128,
		// Token: 0x040013C4 RID: 5060
		Test5 = 256,
		// Token: 0x040013C5 RID: 5061
		Test6 = 512,
		// Token: 0x040013C6 RID: 5062
		Test7 = 1024,
		// Token: 0x040013C7 RID: 5063
		Test8 = 2048
	}
}
