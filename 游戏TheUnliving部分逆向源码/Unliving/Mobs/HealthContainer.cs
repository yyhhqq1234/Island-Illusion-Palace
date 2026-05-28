using System;
using Game.Damage;

namespace Unliving.Mobs
{
	// Token: 0x020001DE RID: 478
	[Serializable]
	public class HealthContainer : EnergyContainer
	{
		// Token: 0x06000F9F RID: 3999 RVA: 0x000316D3 File Offset: 0x0002F8D3
		public HealthContainer()
		{
		}

		// Token: 0x06000FA0 RID: 4000 RVA: 0x000316DB File Offset: 0x0002F8DB
		public HealthContainer(float initialAmount)
		{
			this.initialAmount = initialAmount;
			this.ResetEnergy();
		}
	}
}
