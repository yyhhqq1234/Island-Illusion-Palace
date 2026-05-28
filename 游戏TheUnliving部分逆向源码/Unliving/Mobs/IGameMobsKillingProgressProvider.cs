using System;

namespace Unliving.Mobs
{
	// Token: 0x020001E8 RID: 488
	public interface IGameMobsKillingProgressProvider
	{
		// Token: 0x1700034F RID: 847
		// (get) Token: 0x0600103D RID: 4157
		float MobsKillingProgress { get; }

		// Token: 0x140000B4 RID: 180
		// (add) Token: 0x0600103E RID: 4158
		// (remove) Token: 0x0600103F RID: 4159
		event Action<IGameMobsKillingProgressProvider, float, float> MobsKillingProgressChanged;
	}
}
