using System;
using Game.Buffs;

namespace Unliving.MobsStats
{
	// Token: 0x02000056 RID: 86
	public sealed class BuffDurationStat : MobStatBase
	{
		// Token: 0x17000084 RID: 132
		// (get) Token: 0x060002A8 RID: 680 RVA: 0x0000A8F1 File Offset: 0x00008AF1
		public override float CurrentValue
		{
			get
			{
				return this.buffGenerator.BuffDuration;
			}
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000A8FE File Offset: 0x00008AFE
		protected override void SetStatValue(float newValue)
		{
			this.buffGenerator.BuffDuration = newValue;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000A90C File Offset: 0x00008B0C
		public BuffDurationStat(MobStatID statID, object statOwner, IBuffsGenerator buffGenerator) : base(statID, statOwner)
		{
			this.buffGenerator = buffGenerator;
			base.Initialize((int)statID, statOwner);
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000A925 File Offset: 0x00008B25
		public BuffDurationStat(object statOwner, IBuffsGenerator buffGenerator) : this(MobStatID.BuffsDuration, statOwner, buffGenerator)
		{
		}

		// Token: 0x04000190 RID: 400
		private readonly IBuffsGenerator buffGenerator;
	}
}
