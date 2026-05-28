using System;

namespace Unliving.MobsStats
{
	// Token: 0x02000058 RID: 88
	public class DelegateBasedMobStat : MobStatBase
	{
		// Token: 0x17000085 RID: 133
		// (get) Token: 0x060002AF RID: 687 RVA: 0x0000A992 File Offset: 0x00008B92
		public override float CurrentValue
		{
			get
			{
				return this.getValueDelegate();
			}
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000A99F File Offset: 0x00008B9F
		public DelegateBasedMobStat(MobStatID statID, object statOwner, Func<float> getValueDelegate, Action<float> setValueDelegate) : base(statID, statOwner)
		{
			this.getValueDelegate = getValueDelegate;
			this.setValueDelegate = setValueDelegate;
			base.Initialize((int)statID, statOwner);
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000A9C0 File Offset: 0x00008BC0
		protected override void SetStatValue(float newValue)
		{
			this.setValueDelegate(newValue);
		}

		// Token: 0x04000194 RID: 404
		private readonly Func<float> getValueDelegate;

		// Token: 0x04000195 RID: 405
		private readonly Action<float> setValueDelegate;
	}
}
