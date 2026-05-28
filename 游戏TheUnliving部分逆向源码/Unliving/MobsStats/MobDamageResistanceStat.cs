using System;
using Game.Damage;
using Unliving.Mobs;

namespace Unliving.MobsStats
{
	// Token: 0x02000060 RID: 96
	public sealed class MobDamageResistanceStat : MobStatBase
	{
		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060002C2 RID: 706 RVA: 0x0000AB65 File Offset: 0x00008D65
		public override float CurrentValue
		{
			get
			{
				return this.currentDamageResistance;
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000AB70 File Offset: 0x00008D70
		protected override float GetModifiedStatValue(float initialValue)
		{
			MobStatModifier currentModifiers = base.CurrentModifiers;
			return (currentModifiers.AbsoluteModifier * 0.01f + currentModifiers.BaseModifier) * currentModifiers.ExtraModifier;
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000ABA1 File Offset: 0x00008DA1
		protected override void SetStatValue(float newValue)
		{
			if (this.hitPointsController == null)
			{
				return;
			}
			this.hitPointsController.ModifyDamageResistance(-this.currentDamageResistance);
			this.hitPointsController.ModifyDamageResistance(newValue);
			this.currentDamageResistance = newValue;
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000ABD1 File Offset: 0x00008DD1
		public MobDamageResistanceStat(IGameMob mob) : base(MobStatID.MobDamageResistance, mob)
		{
			this.hitPointsController = (mob.HitPointsController as IResistableDamageable);
			base.Initialize(68, mob);
		}

		// Token: 0x0400019C RID: 412
		private readonly IResistableDamageable hitPointsController;

		// Token: 0x0400019D RID: 413
		private float currentDamageResistance;
	}
}
