using System;
using Game.Buffs;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003DE RID: 990
	public sealed class BuffsReceivingTrigger : IDisposable
	{
		// Token: 0x14000127 RID: 295
		// (add) Token: 0x0600219A RID: 8602 RVA: 0x00068F94 File Offset: 0x00067194
		// (remove) Token: 0x0600219B RID: 8603 RVA: 0x00068FCC File Offset: 0x000671CC
		public event Action<int, bool> BuffsStateChanged;

		// Token: 0x0600219C RID: 8604 RVA: 0x00069004 File Offset: 0x00067204
		private int GetCounterIndex(IBuff buff)
		{
			int id = buff.ID;
			for (int i = 0; i < this.buffsCount; i++)
			{
				if (this.buffsCounters[i].buffID == id)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600219D RID: 8605 RVA: 0x00069040 File Offset: 0x00067240
		public BuffsReceivingTrigger(IBuffableObject buffsReceiver, BuffsGeneratorBuilderAsset[] allowedBuffsGenerators = null)
		{
			this.buffsController = buffsReceiver.BuffsController;
			this.allowedBuffsGenerators = allowedBuffsGenerators;
			this.buffsController.BuffActivated += this.OnBuffActivated;
			this.buffsController.BuffCompleted += this.OnBuffCompleted;
		}

		// Token: 0x0600219E RID: 8606 RVA: 0x000690A4 File Offset: 0x000672A4
		private void OnBuffActivated(IBuff buff)
		{
			if (this.allowedBuffsGenerators != null)
			{
				bool flag = false;
				int num = 0;
				while (num < this.allowedBuffsGenerators.Length && !(flag = this.allowedBuffsGenerators[num].IsRelatedBuff(buff)))
				{
					num++;
				}
				if (!flag)
				{
					return;
				}
			}
			int counterIndex = this.GetCounterIndex(buff);
			if (counterIndex >= 0)
			{
				BuffsReceivingTrigger.BuffsCounter[] array = this.buffsCounters;
				int num2 = counterIndex;
				array[num2].count = array[num2].count + 1;
				return;
			}
			BuffsReceivingTrigger.BuffsCounter[] array2 = this.buffsCounters;
			int num3 = this.buffsCount;
			this.buffsCount = num3 + 1;
			int num4 = num3;
			array2[num4].count = 1;
			array2[num4].buffID = buff.ID;
			Action<int, bool> buffsStateChanged = this.BuffsStateChanged;
			if (buffsStateChanged == null)
			{
				return;
			}
			buffsStateChanged(buff.ID, true);
		}

		// Token: 0x0600219F RID: 8607 RVA: 0x0006914C File Offset: 0x0006734C
		private void OnBuffCompleted(IBuff buff)
		{
			int counterIndex = this.GetCounterIndex(buff);
			if (counterIndex < 0)
			{
				return;
			}
			ref BuffsReceivingTrigger.BuffsCounter ptr = ref this.buffsCounters[counterIndex];
			if (ptr.count > 1)
			{
				ptr.count--;
				return;
			}
			this.buffsCount--;
			if (this.buffsCount > 0 && counterIndex != this.buffsCount)
			{
				this.buffsCounters[counterIndex] = this.buffsCounters[this.buffsCount];
			}
			Action<int, bool> buffsStateChanged = this.BuffsStateChanged;
			if (buffsStateChanged == null)
			{
				return;
			}
			buffsStateChanged(buff.ID, false);
		}

		// Token: 0x060021A0 RID: 8608 RVA: 0x000691DC File Offset: 0x000673DC
		public void Dispose()
		{
			this.buffsController.BuffActivated -= this.OnBuffActivated;
			this.buffsController.BuffCompleted -= this.OnBuffCompleted;
		}

		// Token: 0x040014F5 RID: 5365
		private readonly IBuffsController buffsController;

		// Token: 0x040014F6 RID: 5366
		private readonly BuffsReceivingTrigger.BuffsCounter[] buffsCounters = new BuffsReceivingTrigger.BuffsCounter[32];

		// Token: 0x040014F7 RID: 5367
		private readonly BuffsGeneratorBuilderAsset[] allowedBuffsGenerators;

		// Token: 0x040014F8 RID: 5368
		private int buffsCount;

		// Token: 0x02000598 RID: 1432
		private struct BuffsCounter
		{
			// Token: 0x04001D05 RID: 7429
			public int buffID;

			// Token: 0x04001D06 RID: 7430
			public int count;
		}
	}
}
