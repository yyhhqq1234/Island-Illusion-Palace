using System;
using Game.Abilities;
using Game.Damage.Projectiles;

namespace Unliving.MobsStats
{
	// Token: 0x0200006E RID: 110
	public sealed class ProjectileMaxHitCountStat : ProjectileStatBase
	{
		// Token: 0x0600031E RID: 798 RVA: 0x0000B954 File Offset: 0x00009B54
		protected override float GetProjectileStatValue(BaseProjectile projectilePrototype)
		{
			return (float)projectilePrototype.MaxHitCount;
		}

		// Token: 0x0600031F RID: 799 RVA: 0x0000B95D File Offset: 0x00009B5D
		protected override void SetProjectileStatValue(BaseProjectile projectilePrototype, float newValue)
		{
			projectilePrototype.MaxHitCount = (int)newValue;
		}

		// Token: 0x06000320 RID: 800 RVA: 0x0000B967 File Offset: 0x00009B67
		public ProjectileMaxHitCountStat(MobStatID statID, ProjectileAbilityBase ability) : base(statID, ability)
		{
			base.Initialize((int)statID, ability);
		}

		// Token: 0x06000321 RID: 801 RVA: 0x0000B979 File Offset: 0x00009B79
		public ProjectileMaxHitCountStat(ProjectileAbilityBase ability) : this(MobStatID.MaxProjectileHits, ability)
		{
		}
	}
}
