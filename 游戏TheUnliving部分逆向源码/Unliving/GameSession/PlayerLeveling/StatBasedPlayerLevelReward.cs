using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Unliving.MobsStats;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002BE RID: 702
	[Serializable]
	public sealed class StatBasedPlayerLevelReward : PlayerLevelRewardBase, IStatBasedPlayerLevelReward, IPlayerLevelReward, ICloneable
	{
		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x0600185C RID: 6236 RVA: 0x0004C6C3 File Offset: 0x0004A8C3
		IReadOnlyList<TargetedMobStatModifier> IStatBasedPlayerLevelReward.StatModifiers
		{
			get
			{
				return this.statModifiers;
			}
		}

		// Token: 0x0600185D RID: 6237 RVA: 0x0004C6CB File Offset: 0x0004A8CB
		public StatBasedPlayerLevelReward(StatBasedPlayerLevelReward rewardPrototype) : base(rewardPrototype)
		{
			TargetedMobStatModifier[] array = rewardPrototype.statModifiers;
			this.statModifiers = (((array != null) ? array.CloneArray<TargetedMobStatModifier>() : null) ?? new TargetedMobStatModifier[0]);
		}

		// Token: 0x0600185E RID: 6238 RVA: 0x0004C6F6 File Offset: 0x0004A8F6
		public StatBasedPlayerLevelReward(int rank, float weight, TargetedMobStatModifier[] statModifiers) : base(weight, rank)
		{
			this.statModifiers = statModifiers;
		}

		// Token: 0x0600185F RID: 6239 RVA: 0x0004C707 File Offset: 0x0004A907
		public override object Clone()
		{
			return new StatBasedPlayerLevelReward(this);
		}

		// Token: 0x06001860 RID: 6240 RVA: 0x0004C710 File Offset: 0x0004A910
		public override void Take(PlayerLevelRewardCollectionContext context)
		{
			foreach (TargetedMobStatModifier targetedMobStatModifier in this.statModifiers)
			{
				if (targetedMobStatModifier.targetStat != MobStatID.Undefined)
				{
					context.player.StatsController.AddModifier(targetedMobStatModifier.TargetStatID, targetedMobStatModifier.ToStatModifier());
				}
			}
		}

		// Token: 0x04000DB7 RID: 3511
		public TargetedMobStatModifier[] statModifiers;
	}
}
