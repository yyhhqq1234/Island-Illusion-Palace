using System;

namespace Unliving.MobsStats
{
	// Token: 0x0200005A RID: 90
	public interface IMobStatsModifiersGenerator
	{
		// Token: 0x060002B2 RID: 690
		int GetCurrentStatsModifiers(object modifiersTarget, out TargetedMobStatModifier[] statsModifiers);
	}
}
