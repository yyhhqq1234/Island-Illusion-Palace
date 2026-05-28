using System;
using System.Collections.Generic;
using Game.Damage;

namespace Unliving.Mobs
{
	// Token: 0x020001E2 RID: 482
	public interface IContainerBasedHPControllerWithVitalEnergy : IContainerBasedHPController, IDamageable, IHitPointsSource
	{
		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000FB6 RID: 4022
		IReadOnlyCollection<VitalContainer> VitalContainers { get; }

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06000FB7 RID: 4023
		float MaxVitalAmount { get; }

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06000FB8 RID: 4024
		float CurrentVitalAmount { get; }

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x06000FB9 RID: 4025
		// (set) Token: 0x06000FBA RID: 4026
		bool RestoreVitalEnergyOnHealing { get; set; }
	}
}
