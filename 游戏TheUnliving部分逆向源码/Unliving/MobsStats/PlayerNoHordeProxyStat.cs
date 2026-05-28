using System;
using System.Collections.Generic;
using Game.Stats;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.MobsStats
{
	// Token: 0x0200006D RID: 109
	public sealed class PlayerNoHordeProxyStat : MobProxyStat
	{
		// Token: 0x17000096 RID: 150
		// (get) Token: 0x06000317 RID: 791 RVA: 0x0000B7FC File Offset: 0x000099FC
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x0000B804 File Offset: 0x00009A04
		private void SetStatActive(bool isActive)
		{
			if (this.isActive == isActive)
			{
				return;
			}
			this.isActive = isActive;
			IReadOnlyList<IModifiableStat<MobStatModifier>> targetStats = base.TargetStats;
			for (int i = 0; i < targetStats.Count; i++)
			{
				if (isActive)
				{
					this.ApplyAllModifiers(targetStats[i]);
				}
				else
				{
					this.RemoveAllModifiers(targetStats[i]);
				}
			}
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0000B859 File Offset: 0x00009A59
		protected override void ApplyAllModifiers(IModifiableStat<MobStatModifier> stat)
		{
			if (!this.isActive)
			{
				return;
			}
			base.ApplyAllModifiers(stat);
		}

		// Token: 0x0600031A RID: 794 RVA: 0x0000B86C File Offset: 0x00009A6C
		public PlayerNoHordeProxyStat(MobStatID statID, PlayerBehaviour player) : base(statID, player)
		{
			this.player = player;
			player.Group.MobAdded += this.OnPlayerGroupMobAdded;
			player.Group.MobRemoved += this.OnPlayerGroupMobRemoved;
			player.Destroyed += this.OnPlayerDestroyed;
			this.SetStatActive(!player.Group.HasMobs);
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0000B8DC File Offset: 0x00009ADC
		private void OnPlayerGroupMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.SetStatActive(!group.HasMobs);
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0000B8ED File Offset: 0x00009AED
		private void OnPlayerGroupMobAdded(GameMobsGroupControllerBase arg1, BaseGameMob arg2)
		{
			this.SetStatActive(false);
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0000B8F8 File Offset: 0x00009AF8
		private void OnPlayerDestroyed(object obj)
		{
			this.player.Destroyed -= this.OnPlayerDestroyed;
			this.player.Group.MobAdded -= this.OnPlayerGroupMobAdded;
			this.player.Group.MobRemoved -= this.OnPlayerGroupMobRemoved;
		}

		// Token: 0x040001F8 RID: 504
		private readonly PlayerBehaviour player;

		// Token: 0x040001F9 RID: 505
		private bool isActive;
	}
}
