using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.MobsStats
{
	// Token: 0x0200005F RID: 95
	public sealed class MobAbilityUsingSpeedStat : MobStatBase
	{
		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060002BF RID: 703 RVA: 0x0000AA8B File Offset: 0x00008C8B
		public override float CurrentValue
		{
			get
			{
				return this.targetAbility.ReloadingTime;
			}
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000AA98 File Offset: 0x00008C98
		protected override void SetStatValue(float newValue)
		{
			float num = this.GetInitialValue() / Mathf.Max(newValue, 1E-05f);
			this.targetAbility.ReloadingTime = newValue;
			if (this.initialShotsPerUsing > 0f)
			{
				((ProjectileAbilityBase)this.targetAbility).MaxShotsPerUsing = (int)(this.initialShotsPerUsing * num);
				return;
			}
			if (this.initialUsingLoopStep > 0f)
			{
				this.targetAbility.UsingLoopStep = this.initialUsingLoopStep * num;
			}
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000AB0C File Offset: 0x00008D0C
		public MobAbilityUsingSpeedStat(MobStatID statID, BaseAbility targetAbility) : base(statID, targetAbility)
		{
			this.targetAbility = targetAbility;
			base.Initialize((int)statID, targetAbility);
			ProjectileAbilityBase projectileAbilityBase = targetAbility as ProjectileAbilityBase;
			if (projectileAbilityBase != null && targetAbility.HasUsingDuration() && projectileAbilityBase.MaxShotsPerUsing > 0)
			{
				this.initialShotsPerUsing = (float)projectileAbilityBase.MaxShotsPerUsing;
				return;
			}
			this.initialUsingLoopStep = targetAbility.UsingLoopStep;
		}

		// Token: 0x04000199 RID: 409
		private readonly BaseAbility targetAbility;

		// Token: 0x0400019A RID: 410
		private readonly float initialUsingLoopStep;

		// Token: 0x0400019B RID: 411
		private readonly float initialShotsPerUsing;
	}
}
