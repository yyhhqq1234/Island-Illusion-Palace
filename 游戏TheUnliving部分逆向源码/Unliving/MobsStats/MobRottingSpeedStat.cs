using System;
using Unliving.Mobs;

namespace Unliving.MobsStats
{
	// Token: 0x02000063 RID: 99
	public sealed class MobRottingSpeedStat : MobStatBase
	{
		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060002D2 RID: 722 RVA: 0x0000ACF9 File Offset: 0x00008EF9
		// (set) Token: 0x060002D3 RID: 723 RVA: 0x0000AD01 File Offset: 0x00008F01
		public MobDecayController RottingController
		{
			get
			{
				return this.rottingController;
			}
			set
			{
				if (this.rottingController == value)
				{
					return;
				}
				this.rottingController = value;
				this.UpdateInitialValue();
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x060002D4 RID: 724 RVA: 0x0000AD1A File Offset: 0x00008F1A
		public override float CurrentValue
		{
			get
			{
				MobDecayController mobDecayController = this.rottingController;
				if (mobDecayController == null)
				{
					return 0f;
				}
				return mobDecayController.DecaySpeed;
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000AD31 File Offset: 0x00008F31
		protected override void SetStatValue(float newValue)
		{
			if (this.rottingController != null)
			{
				this.rottingController.DecaySpeed = newValue;
			}
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000AD47 File Offset: 0x00008F47
		public MobRottingSpeedStat(BaseGameMob mob) : base(MobStatID.MobRottingSpeed, mob)
		{
			MobHealthController mobHealthController = mob.HitPointsController as MobHealthController;
			this.rottingController = ((mobHealthController != null) ? mobHealthController.DecayController : null);
			base.Initialize(43, mob);
		}

		// Token: 0x040001A0 RID: 416
		private MobDecayController rottingController;
	}
}
