using System;
using System.Collections.Generic;
using Game.Stats;

namespace Unliving.MobsStats
{
	// Token: 0x02000074 RID: 116
	public sealed class TempMobStatsModifiersHandler<TTargetedModifier> where TTargetedModifier : ITargetedStatModifier<MobStatModifier>
	{
		// Token: 0x0600033A RID: 826 RVA: 0x0000BBF0 File Offset: 0x00009DF0
		public TempMobStatsModifiersHandler(IReadOnlyList<IModifiableStat<MobStatModifier>> stats, int maxModifiersCount)
		{
			this.stats = stats;
			this.modifiers = new TTargetedModifier[maxModifiersCount];
			this.modifiersCount = 0;
			this.appliedModifiersMask = 0;
		}

		// Token: 0x0600033B RID: 827 RVA: 0x0000BC19 File Offset: 0x00009E19
		public TempMobStatsModifiersHandler(IReadOnlyList<IModifiableStat<MobStatModifier>> stats) : this(stats, 8)
		{
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0000BC23 File Offset: 0x00009E23
		public TempMobStatsModifiersHandler(IMobStatsListProvider statsListProvider) : this(statsListProvider.Stats)
		{
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0000BC34 File Offset: 0x00009E34
		public void AddStatModifier(TTargetedModifier modifier)
		{
			if (this.modifiersCount == this.modifiers.Length)
			{
				return;
			}
			TTargetedModifier[] array = this.modifiers;
			int num = this.modifiersCount;
			this.modifiersCount = num + 1;
			array[num] = modifier;
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0000BC70 File Offset: 0x00009E70
		public void ApplyModifiers(Predicate<IStat> statsFilter = null)
		{
			if (this.modifiersCount == 0)
			{
				return;
			}
			if (this.appliedModifiersMask != 0)
			{
				return;
			}
			this.appliedModifiersMask = this.modifiers.ModifyStats(this.stats, true, this.modifiersCount, statsFilter);
			if (this.appliedModifiersMask != 0)
			{
				for (int i = 0; i < this.modifiersCount; i++)
				{
					if ((1 << i & this.appliedModifiersMask) == 0)
					{
						this.modifiers[i].Invalidate();
					}
				}
				return;
			}
			this.modifiersCount = 0;
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0000BCF7 File Offset: 0x00009EF7
		public void RemoveAppliedModifiers()
		{
			if (this.appliedModifiersMask == 0)
			{
				return;
			}
			this.modifiers.ModifyStats(this.stats, false, this.modifiersCount, null);
			this.modifiersCount = 0;
			this.appliedModifiersMask = 0;
		}

		// Token: 0x040001FF RID: 511
		private readonly IReadOnlyList<IModifiableStat<MobStatModifier>> stats;

		// Token: 0x04000200 RID: 512
		private readonly TTargetedModifier[] modifiers;

		// Token: 0x04000201 RID: 513
		private int modifiersCount;

		// Token: 0x04000202 RID: 514
		private int appliedModifiersMask;
	}
}
