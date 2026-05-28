using System;
using Common;

namespace Unliving.MobsStats
{
	// Token: 0x02000054 RID: 84
	public sealed class AmountStat : MobStatBase
	{
		// Token: 0x17000083 RID: 131
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x0000A89E File Offset: 0x00008A9E
		public override float CurrentValue
		{
			get
			{
				if (this.AmountBasedEffect == null)
				{
					return 0f;
				}
				return this.AmountBasedEffect.Amount;
			}
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000A8B9 File Offset: 0x00008AB9
		public AmountStat(MobStatID statID, object statOwner, IAmountBased amountBasedEffect) : base(statID, statOwner)
		{
			this.AmountBasedEffect = amountBasedEffect;
			base.Initialize((int)statID, statOwner);
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0000A8D2 File Offset: 0x00008AD2
		protected override void SetStatValue(float newValue)
		{
			if (this.AmountBasedEffect != null)
			{
				this.AmountBasedEffect.Amount = newValue;
			}
		}

		// Token: 0x0400018F RID: 399
		public readonly IAmountBased AmountBasedEffect;
	}
}
