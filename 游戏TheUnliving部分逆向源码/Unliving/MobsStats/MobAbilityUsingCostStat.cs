using System;
using Game.Abilities;

namespace Unliving.MobsStats
{
	// Token: 0x0200005E RID: 94
	public sealed class MobAbilityUsingCostStat : MobStatBase
	{
		// Token: 0x17000088 RID: 136
		// (get) Token: 0x060002BB RID: 699 RVA: 0x0000AA4C File Offset: 0x00008C4C
		public override float CurrentValue
		{
			get
			{
				return this.ability.Cost;
			}
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0000AA59 File Offset: 0x00008C59
		protected override void SetStatValue(float newValue)
		{
			this.ability.Cost = newValue;
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000AA67 File Offset: 0x00008C67
		public MobAbilityUsingCostStat(MobStatID statID, IAbility ability) : base(statID, ability)
		{
			this.ability = ability;
			base.Initialize((int)statID, ability);
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000AA80 File Offset: 0x00008C80
		public MobAbilityUsingCostStat(IAbility ability) : this(MobStatID.AbilityUsingCost, ability)
		{
		}

		// Token: 0x04000198 RID: 408
		private readonly IAbility ability;
	}
}
