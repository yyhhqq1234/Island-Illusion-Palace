using System;
using Game.Damage;

namespace Unliving.MobsStats
{
	// Token: 0x02000061 RID: 97
	public sealed class MobDamageStat : MobStatBase
	{
		// Token: 0x1700008B RID: 139
		// (get) Token: 0x060002C6 RID: 710 RVA: 0x0000ABF6 File Offset: 0x00008DF6
		public override float CurrentValue
		{
			get
			{
				DamageGenerator damageGenerator = this.damageSender.DamageGenerator;
				if (damageGenerator == null)
				{
					return 0f;
				}
				return damageGenerator.amount;
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000AC12 File Offset: 0x00008E12
		protected override void SetStatValue(float newValue)
		{
			DamageGenerator damageGenerator = this.damageSender.DamageGenerator;
			if (damageGenerator == null)
			{
				return;
			}
			damageGenerator.SetDamageAmount(newValue, -1f);
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000AC2F File Offset: 0x00008E2F
		public MobDamageStat(MobStatID statID, object statOwner, IDamageSender damageSender) : base(statID, statOwner)
		{
			this.damageSender = damageSender;
			base.Initialize((int)statID, statOwner);
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000AC48 File Offset: 0x00008E48
		public MobDamageStat(object statOwner, IDamageSender damageSender) : this(MobStatID.MobDamage, statOwner, damageSender)
		{
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000AC53 File Offset: 0x00008E53
		public MobDamageStat(IDamageSender statOwner) : this(MobStatID.MobDamage, statOwner, statOwner)
		{
		}

		// Token: 0x0400019E RID: 414
		private readonly IDamageSender damageSender;
	}
}
