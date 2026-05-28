using System;
using System.Reflection;

namespace Unliving.MobsStats
{
	// Token: 0x02000071 RID: 113
	public sealed class PropertyBasedMobStat : MobStatBase
	{
		// Token: 0x17000098 RID: 152
		// (get) Token: 0x0600032B RID: 811 RVA: 0x0000B9EA File Offset: 0x00009BEA
		public override float CurrentValue
		{
			get
			{
				return (float)this.targetPropertyInfo.GetValue(this.owner);
			}
		}

		// Token: 0x0600032C RID: 812 RVA: 0x0000BA02 File Offset: 0x00009C02
		public PropertyBasedMobStat(MobStatID statID, object statOwner, PropertyInfo targetPropertyInfo) : base(statID, statOwner)
		{
			this.targetPropertyInfo = targetPropertyInfo;
			base.Initialize((int)statID, statOwner);
		}

		// Token: 0x0600032D RID: 813 RVA: 0x0000BA1B File Offset: 0x00009C1B
		protected override void SetStatValue(float newValue)
		{
			this.targetPropertyInfo.SetValue(this.owner, newValue);
		}

		// Token: 0x040001FB RID: 507
		private readonly PropertyInfo targetPropertyInfo;
	}
}
