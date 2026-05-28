using System;
using Game.Abilities;

namespace Unliving.MobsStats
{
	// Token: 0x0200005C RID: 92
	public sealed class MobAbilityCooldownStat : MobStatBase
	{
		// Token: 0x17000086 RID: 134
		// (get) Token: 0x060002B3 RID: 691 RVA: 0x0000A9CE File Offset: 0x00008BCE
		public override float CurrentValue
		{
			get
			{
				return this.ability.ReloadingTime;
			}
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000A9DB File Offset: 0x00008BDB
		protected override void SetStatValue(float newValue)
		{
			this.ability.ReloadingTime = newValue;
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000A9E9 File Offset: 0x00008BE9
		public MobAbilityCooldownStat(MobStatID statID, IAbility ability) : base(statID, ability)
		{
			this.ability = ability;
			base.Initialize((int)statID, ability);
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0000AA02 File Offset: 0x00008C02
		public MobAbilityCooldownStat(IAbility ability) : this(MobStatID.AbilityCooldown, ability)
		{
		}

		// Token: 0x04000196 RID: 406
		private readonly IAbility ability;
	}
}
