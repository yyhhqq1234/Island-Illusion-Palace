using System;
using Game.Abilities;
using Game.Damage.Projectiles;

namespace Unliving.MobsStats
{
	// Token: 0x02000070 RID: 112
	public abstract class ProjectileStatBase : MobStatBase
	{
		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000326 RID: 806 RVA: 0x0000B9B2 File Offset: 0x00009BB2
		public sealed override float CurrentValue
		{
			get
			{
				return this.GetProjectileStatValue(this.projectilePrototype);
			}
		}

		// Token: 0x06000327 RID: 807
		protected abstract void SetProjectileStatValue(BaseProjectile projectilePrototype, float newValue);

		// Token: 0x06000328 RID: 808
		protected abstract float GetProjectileStatValue(BaseProjectile projectilePrototype);

		// Token: 0x06000329 RID: 809 RVA: 0x0000B9C0 File Offset: 0x00009BC0
		protected override void SetStatValue(float newValue)
		{
			this.SetProjectileStatValue(this.projectilePrototype, newValue);
		}

		// Token: 0x0600032A RID: 810 RVA: 0x0000B9CF File Offset: 0x00009BCF
		public ProjectileStatBase(MobStatID statID, ProjectileAbilityBase ability) : base(statID, ability)
		{
			this.projectilePrototype = (BaseProjectile)ability.ProjectilePrototype;
		}

		// Token: 0x040001FA RID: 506
		private readonly BaseProjectile projectilePrototype;
	}
}
