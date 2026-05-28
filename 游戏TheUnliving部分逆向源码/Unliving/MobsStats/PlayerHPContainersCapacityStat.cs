using System;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.MobsStats
{
	// Token: 0x0200006B RID: 107
	public sealed class PlayerHPContainersCapacityStat : MobStatBase
	{
		// Token: 0x17000094 RID: 148
		// (get) Token: 0x0600030C RID: 780 RVA: 0x0000B721 File Offset: 0x00009921
		public override float CurrentValue
		{
			get
			{
				VitalEnergyHitPointsController vitalEnergyHitPointsController = this.hitPointsController;
				if (vitalEnergyHitPointsController == null)
				{
					return 0f;
				}
				return vitalEnergyHitPointsController.InitialHitPoints;
			}
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0000B738 File Offset: 0x00009938
		protected override void SetStatValue(float newValue)
		{
			if (this.hitPointsController == null)
			{
				return;
			}
			this.hitPointsController.InitialHitPoints = newValue;
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0000B755 File Offset: 0x00009955
		public PlayerHPContainersCapacityStat(PlayerBehaviour player) : base(MobStatID.PlayerHPContainerCapacity, player)
		{
			this.hitPointsController = (player.HitPointsController as VitalEnergyHitPointsController);
			base.Initialize(69, player);
		}

		// Token: 0x040001F6 RID: 502
		private readonly VitalEnergyHitPointsController hitPointsController;
	}
}
