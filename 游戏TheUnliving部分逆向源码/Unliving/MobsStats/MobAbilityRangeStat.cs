using System;
using Game.Abilities;

namespace Unliving.MobsStats
{
	// Token: 0x0200005D RID: 93
	public sealed class MobAbilityRangeStat : MobStatBase
	{
		// Token: 0x17000087 RID: 135
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x0000AA0D File Offset: 0x00008C0D
		public override float CurrentValue
		{
			get
			{
				return this.ability.Range;
			}
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000AA1A File Offset: 0x00008C1A
		protected override void SetStatValue(float newValue)
		{
			this.ability.Range = newValue;
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0000AA28 File Offset: 0x00008C28
		public MobAbilityRangeStat(MobStatID statID, IAbility ability) : base(statID, ability)
		{
			this.ability = ability;
			base.Initialize((int)statID, ability);
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000AA41 File Offset: 0x00008C41
		public MobAbilityRangeStat(IAbility ability) : this(MobStatID.AbilityRange, ability)
		{
		}

		// Token: 0x04000197 RID: 407
		private readonly IAbility ability;
	}
}
