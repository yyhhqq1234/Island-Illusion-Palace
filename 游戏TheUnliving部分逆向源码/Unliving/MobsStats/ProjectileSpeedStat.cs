using System;
using Game.Abilities;
using Game.Damage.Projectiles;

namespace Unliving.MobsStats
{
	// Token: 0x0200006F RID: 111
	public sealed class ProjectileSpeedStat : ProjectileStatBase
	{
		// Token: 0x06000322 RID: 802 RVA: 0x0000B984 File Offset: 0x00009B84
		protected override float GetProjectileStatValue(BaseProjectile projectilePrototype)
		{
			return projectilePrototype.TargetSpeed;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000B98C File Offset: 0x00009B8C
		protected override void SetProjectileStatValue(BaseProjectile projectilePrototype, float newValue)
		{
			projectilePrototype.TargetSpeed = newValue;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x0000B995 File Offset: 0x00009B95
		public ProjectileSpeedStat(MobStatID statID, ProjectileAbilityBase ability) : base(statID, ability)
		{
			base.Initialize((int)statID, ability);
		}

		// Token: 0x06000325 RID: 805 RVA: 0x0000B9A7 File Offset: 0x00009BA7
		public ProjectileSpeedStat(ProjectileAbilityBase ability) : this(MobStatID.ProjectileSpeed, ability)
		{
		}
	}
}
