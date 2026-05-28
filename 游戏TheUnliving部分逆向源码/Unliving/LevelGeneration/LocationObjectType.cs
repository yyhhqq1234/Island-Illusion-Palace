using System;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000273 RID: 627
	[Flags]
	public enum LocationObjectType
	{
		// Token: 0x04000C65 RID: 3173
		Undefined = 0,
		// Token: 0x04000C66 RID: 3174
		HorizontalLocationGateway = 1,
		// Token: 0x04000C67 RID: 3175
		VerticalLocationGateway = 2,
		// Token: 0x04000C68 RID: 3176
		Player = 4,
		// Token: 0x04000C69 RID: 3177
		PlayerMob = 8,
		// Token: 0x04000C6A RID: 3178
		Mob = 16,
		// Token: 0x04000C6B RID: 3179
		MinorMob = 32,
		// Token: 0x04000C6C RID: 3180
		Corpse = 64,
		// Token: 0x04000C6D RID: 3181
		Merchant = 128,
		// Token: 0x04000C6E RID: 3182
		BossMob = 256,
		// Token: 0x04000C6F RID: 3183
		Resource = 512
	}
}
