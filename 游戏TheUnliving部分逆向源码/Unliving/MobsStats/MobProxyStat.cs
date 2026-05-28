using System;
using Game.Stats;

namespace Unliving.MobsStats
{
	// Token: 0x02000062 RID: 98
	public class MobProxyStat : ProxyStat<MobStatModifier>
	{
		// Token: 0x1700008C RID: 140
		// (get) Token: 0x060002CB RID: 715 RVA: 0x0000AC5E File Offset: 0x00008E5E
		public MobStatModifier AppliedModifiers
		{
			get
			{
				return this.appliedModifiers;
			}
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000AC66 File Offset: 0x00008E66
		protected override void StoreModifier(MobStatModifier modifier)
		{
			this.appliedModifiers += modifier;
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000AC7A File Offset: 0x00008E7A
		protected override void RemoveStoredModifier(MobStatModifier modifier)
		{
			this.appliedModifiers -= modifier;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000AC8E File Offset: 0x00008E8E
		protected override void ApplyAllModifiers(IModifiableStat<MobStatModifier> stat)
		{
			base.HandleModifier(stat, this.appliedModifiers, true);
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0000AC9E File Offset: 0x00008E9E
		protected override void RemoveAllModifiers(IModifiableStat<MobStatModifier> stat)
		{
			base.HandleModifier(stat, this.appliedModifiers, false);
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000ACAE File Offset: 0x00008EAE
		public MobProxyStat(MobStatID statID, object statOwner) : base((int)statID, statOwner)
		{
			base.Name = statID.ToString();
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000ACD6 File Offset: 0x00008ED6
		public override string ToString()
		{
			return string.Format("{0}: (modifiers: {1} owner: {2})", base.Name, this.appliedModifiers, this.owner);
		}

		// Token: 0x0400019F RID: 415
		private MobStatModifier appliedModifiers = MobStatModifier.Neutral;
	}
}
