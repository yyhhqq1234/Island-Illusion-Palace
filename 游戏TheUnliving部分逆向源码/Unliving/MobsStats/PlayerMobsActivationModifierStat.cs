using System;
using Game.Stats;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Player;

namespace Unliving.MobsStats
{
	// Token: 0x0200006C RID: 108
	public sealed class PlayerMobsActivationModifierStat : MobStatBase
	{
		// Token: 0x17000095 RID: 149
		// (get) Token: 0x0600030F RID: 783 RVA: 0x0000B77A File Offset: 0x0000997A
		public override float CurrentValue
		{
			get
			{
				return float.NaN;
			}
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000B781 File Offset: 0x00009981
		protected override void SetStatValue(float newValue)
		{
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0000B783 File Offset: 0x00009983
		public PlayerMobsActivationModifierStat(MobStatID statID, PlayerBehaviour player) : base(statID, player)
		{
			this.activationModifiersController = player.ActivationModifiersController;
			base.Initialize((int)statID, player);
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000B7A1 File Offset: 0x000099A1
		public override float GetInitialValue()
		{
			return float.NaN;
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0000B7A8 File Offset: 0x000099A8
		public override void UpdateInitialValue()
		{
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0000B7AA File Offset: 0x000099AA
		public override void AddModifier(MobStatModifier modifier)
		{
			StatsController modifiersStats = this.activationModifiersController.ModifiersStats;
			if (modifiersStats == null)
			{
				return;
			}
			IModifiableStat<MobStatModifier> stat = modifiersStats.GetStat(base.ID);
			if (stat == null)
			{
				return;
			}
			stat.AddModifier(modifier);
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0000B7D2 File Offset: 0x000099D2
		public override void RemoveModifier(MobStatModifier modifier)
		{
			StatsController modifiersStats = this.activationModifiersController.ModifiersStats;
			if (modifiersStats == null)
			{
				return;
			}
			IModifiableStat<MobStatModifier> stat = modifiersStats.GetStat(base.ID);
			if (stat == null)
			{
				return;
			}
			stat.RemoveModifier(modifier);
		}

		// Token: 0x06000316 RID: 790 RVA: 0x0000B7FA File Offset: 0x000099FA
		protected override void OnParentStatValueModified(float newValue)
		{
		}

		// Token: 0x040001F7 RID: 503
		private readonly MobsActivationModifiersController activationModifiersController;
	}
}
